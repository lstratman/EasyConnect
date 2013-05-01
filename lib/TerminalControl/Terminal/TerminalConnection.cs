/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TerminalConnection.cs,v 1.2 2005/04/20 08:45:47 okajima Exp $
*/
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;

using Poderosa.ConnectionParam;
using Poderosa.Connection;
using Poderosa.Log;
using Poderosa.SSH;
using Poderosa.Toolkit;
using Poderosa.Terminal;
using Poderosa.Text;

using Granados.SSHC;

namespace Poderosa.Communication
{
	public interface IDataReceiver {
		void DataArrived(byte[] buf, int offset, int count);
		void DisconnectedFromServer();
		void ErrorOccurred(string msg);
	}
	internal abstract class AbstractGuevaraSocket {
		
		protected IDataReceiver _callback;

		internal abstract void Transmit(byte[] data, int offset, int length);
		internal abstract void Flush();
		internal abstract void Close();
		internal abstract bool DataAvailable { get; }
		internal abstract void RepeatAsyncRead(IDataReceiver receiver);
	}
	internal class PlainGuevaraSocket : AbstractGuevaraSocket {
		private Socket _socket;
		private byte[] _buf;
		internal PlainGuevaraSocket(Socket s) {
			_socket = s;
			_buf = new byte[0x1000];
		}
		internal override bool DataAvailable {
			get {
				return _socket.Available>0;
			}
		}
		
		internal override void Transmit(byte[] data, int offset, int length) {
			_socket.Send(data, offset, length, SocketFlags.None);
		}
		internal override void Flush() {
		}
		internal override void Close() {
			_socket.Close();
		}
		internal override void RepeatAsyncRead(IDataReceiver receiver) {
			_callback = receiver;
			_socket.BeginReceive(_buf, 0, _buf.Length, SocketFlags.None, new AsyncCallback(RepeatCallback), null);
		}

#if true
		private void RepeatCallback(IAsyncResult result) {
			try {
				int n = _socket.EndReceive(result);
				//GUtil.WriteDebugLog(String.Format("t={0}, n={1} av={2}", DateTime.Now.ToString(), n, _socket.Available));
				//Debug.WriteLine(String.Format("r={0}, n={1} ", result.IsCompleted, n));
				
				if(n > 0) {
					_callback.DataArrived(_buf, 0, n);
					_socket.BeginReceive(_buf, 0, _buf.Length, SocketFlags.None, new AsyncCallback(RepeatCallback), null);
				}
				else if(n < 0) {
					//WindowsMEにおいては、ときどきここで-1が返ってきていることが発覚した。
					_socket.BeginReceive(_buf, 0, _buf.Length, SocketFlags.None, new AsyncCallback(RepeatCallback), null);
				}
				else
					_callback.DisconnectedFromServer();
			}
			catch(Exception ex) {
				if((ex is SocketException) && ((SocketException)ex).ErrorCode==995) {
					//GUtil.WriteDebugLog(String.Format("t={0} error995", DateTime.Now.ToString()));
					_socket.BeginReceive(_buf, 0, _buf.Length, SocketFlags.None, new AsyncCallback(RepeatCallback), null);
				}
				else
					_callback.ErrorOccurred(ex.Message);
			}
		}
#else //受信直後からメインスレッドに処理を移すバージョン
		private int _readLen;
		private bool _nextReadRequired;
		private void RepeatCallback(IAsyncResult result) {
			_readLen = _socket.EndReceive(result);
			_nextReadRequired = false;
			GEnv.Frame.AsForm().Invoke(new MethodInvoker(ReadBody), null);
			if(_nextReadRequired)
				_socket.BeginReceive(_buf, 0, _buf.Length, SocketFlags.None, new AsyncCallback(RepeatCallback), null);
		}
		private void ReadBody() {
			try {
				Debug.Assert(!GEnv.Frame.AsForm().InvokeRequired);
				if(_readLen > 0) {
					_callback.DataArrived(_buf, 0, _readLen);
					_nextReadRequired = true;
				}
				else if(_readLen < 0) {
					//WindowsMEにおいては、ときどきここで-1が返ってきていることが発覚した。
					//_socket.BeginReceive(_buf, 0, _buf.Length, SocketFlags.None, new AsyncCallback(RepeatCallback), null);
					_nextReadRequired = true;
				}
				else
					_callback.DisconnectedFromServer();
			}
			catch(Exception ex) {
				if((ex is SocketException) && ((SocketException)ex).ErrorCode==995) {
					//GUtil.WriteDebugLog(String.Format("t={0} error995", DateTime.Now.ToString()));
					_nextReadRequired = true;
				}
				else
					_callback.ErrorOccurred(ex.Message);
			}
		}
#endif
	}
	internal class ChannelGuevaraSocket : AbstractGuevaraSocket, ISSHConnectionEventReceiver, ISSHChannelEventReceiver {
		private SSHChannel _channel;
		private bool _ready;
		private AutoResetEvent _event;

		internal ChannelGuevaraSocket() {
			_ready = false;
		}
		internal SSHChannel SSHChennal {
			get {
				return _channel;
			}
			set {
				_channel = value;
			}
		}

		internal override void RepeatAsyncRead(IDataReceiver cb) {
			Debug.Assert(_callback==null); //１回しか呼んではだめ
			_callback = cb;
			if(_event!=null) {
				_event.Set();
				_event.Close();
				_event = null;
			}
		}

		internal override bool DataAvailable {
			get {
				return _channel.Connection.Available;
			}
		}
		internal override void Transmit(byte[] data, int offset, int length) {
			if(!_ready || _channel==null) throw new IOException("channel not ready");
			_channel.Transmit(data, offset, length);
		}
		internal override void Flush() {
		}
		internal override void Close() {
			_channel.Close();
			if(_channel.Connection.ChannelCount<=1)
				_channel.Connection.Close();
		}

		public void OnData(byte[] data, int offset, int length) {
			EnsureHandler();
			_callback.DataArrived(data, offset, length);
		}

		public void OnChannelEOF() {
			EnsureHandler();
			_callback.DisconnectedFromServer();
		}

		public void OnChannelError(Exception ex, string msg) {
			EnsureHandler();
			if(!_ready)
				msg = GEnv.Strings.GetString("Message.ChannelPoderosaSocket.FailedToPortforward") + msg;
			_callback.ErrorOccurred(msg);
		}

		public void OnChannelClosed() {
			EnsureHandler();
			_callback.DisconnectedFromServer();

			if (LoggedOff != null)
				LoggedOff(this, null);
		}

		public void OnChannelReady() {
			_ready = true;
		}

		public void OnAuthenticationPrompt(string[] msg) {
			for(int i=0; i<msg.Length; i++) {
				if(i==0) msg[i] += "\r\n";
				byte[] buf = Encoding.ASCII.GetBytes(msg[i]);
				OnData(buf, 0, buf.Length);
			}
		}

		public void OnExtendedData(int type, byte[] data) {
		}

		public void OnConnectionClosed() {
			EnsureHandler();
			_callback.DisconnectedFromServer();

            if (Disconnected != null)
                Disconnected(this, null);
		}
		public void OnDebugMessage(bool display, byte[] data) {
			Debug.WriteLine(String.Format("SSH debug {0}[{1}]", data.Length, data[0] ));
		}
		public void OnIgnoreMessage(byte[] data) {
			Debug.WriteLine(String.Format("SSH ignore {0}[{1}]", data.Length, data[0] ));
		}
		public void OnUnknownMessage(byte type, byte[] data) {
			Debug.WriteLine(String.Format("Unexpected SSH packet type {0}", type));
		}
		public void OnError(Exception ex, string msg) {
			EnsureHandler();
			_callback.ErrorOccurred(msg);
		}
		public void OnMiscPacket(byte type, byte[] data, int offset, int length) {
		}

		public Granados.SSHC.PortForwardingCheckResult CheckPortForwardingRequest(string host, int port, string originator, int originator_port) {
			return new Granados.SSHC.PortForwardingCheckResult();
		}
		public void EstablishPortforwarding(ISSHChannelEventReceiver receiver, SSHChannel channel) {
		}

	    public event EventHandler Connected;
	    public event EventHandler Disconnected;
		public event EventHandler LoggedOff;

	    private void EnsureHandler() {
			if(_callback!=null) return;
			_event = new AutoResetEvent(false);
			_event.WaitOne();
		}
	}		
	public abstract class TerminalConnection {

		internal TerminalParam _param;
		internal ITerminalTextLogger _loggerT;
		internal ITerminalBinaryLogger _loggerB;

		internal int _width;
		internal int _height;

		internal LogType _logType;
		internal string _logPath;
		//ログの一時的なオフのためのスイッチ
		internal bool _logsuspended;

		//送受信したデータサイズの情報
		internal int _sentPacketCount;
		internal int _sentDataSize;
		internal int _receivedPacketCount;
		internal int _receivedDataSize;

		internal IPAddress _serverAddress;
		internal string _serverName;
		//すでにクローズされたかどうかのフラグ
		internal bool _closed;

		protected TerminalConnection(TerminalParam p, int width, int height) {
			_param = p;
			_width = width;
			_height = height;
			ResetLog(p.LogType, p.LogPath, p.LogAppend);
			_logsuspended = false;
		}

		public IPAddress ServerAddress {
			get {
				return _serverAddress;
			}
		}
		public string ServerName {
			get {
				return _serverName;
			}
		}
		public abstract string ProtocolDescription {
			get;
		}

		public int SentPacketCount {
			get {
				return _sentPacketCount;
			}
		}
		public int SentDataSize {
			get {
				return _sentDataSize;
			}
		}
		public int ReceivedPacketCount {
			get {
				return _receivedPacketCount;
			}
		}
		public int ReceivedDataSize {
			get {
				return _receivedDataSize;
			}
		}

		public TerminalParam Param {
			get {
				return _param;
			}
		}
		public int TerminalHeight {
			get {
				return _height;
			}
		}
		public int TerminalWidth {
			get {
				return _width;
			}
		}

		public bool LogSuspended {
			get {
				return _logsuspended;
			}
			set {
				_logsuspended = value;
			}
		}

		public LogType LogType {
			get {
				return _logType;
			}
		}
		public string LogPath {
			get {
				return _logPath;
			}
		}
		public void SetServerInfo(string host, IPAddress address) {
			_serverName = host;
			_serverAddress = address;
		}
		public void CommentLog(string comment) {
			_loggerT.Comment(comment);
			_loggerB.Comment(comment);
		}
		public ITerminalTextLogger TextLogger {
			get {
				return _loggerT;
			}
		}
		public ITerminalBinaryLogger BinaryLogger {
			get {
				return _loggerB;
			}
		}

		public bool IsClosed {
			get {
				return _closed;
			}
		}



		public abstract string[] ConnectionParameter { get; }

		public abstract bool Available { get; }

		public virtual void Resize(int width, int height) {
			Debug.Assert(width>0 && height>0);
			_width = width;
			_height = height;
		}

		public abstract ConnectionTag Reproduce(); //同じところへの接続をもう１本

		//終了処理
		public virtual void Close() {
			if(_loggerT!=null) {
				_loggerT.Flush();
				_loggerT.Close();
				_loggerT = null;
			}
			if(_loggerB!=null) {
				_loggerB.Flush();
				_loggerB.Close();
				_loggerB = null;
			}
			_closed = true;
		}
		//接続を閉じる前の最後のアクション。こちらから切るときにはこれをよぶべきだが、サーバから切ってきたときは呼ぶ必要なし
		public virtual void Disconnect() {
		}

		public abstract void RepeatAsyncRead(IDataReceiver cb);


		public void WriteChars(char[] data) {
			byte[] b = _param.EncodingProfile.GetBytes(data);
			Write(b);
		}
		public void WriteChar(char data) {
			byte[] b = _param.EncodingProfile.GetBytes(data);
			Write(b);
		}
		public abstract void Write(byte[] data);
		public abstract void Write(byte[] data, int offset, int length);

		//デフォルト実装は空
		public virtual void AreYouThere() {
			throw new NotSupportedException("[AYT] unsupported");
		}
		public virtual void SendBreak() {
			throw new NotSupportedException("[Break] unsupported");
		}
		public virtual void SendKeepAliveData() {
		}

		internal void AddSentDataStats(int bytecount) {
			_sentPacketCount++;
			_sentDataSize += bytecount;
		}
		internal void AddReceivedDataStats(int bytecount) {
			_receivedPacketCount++;
			_receivedDataSize += bytecount;
		}

		public void ResetLog(LogType t, string path, bool append) {
			_logType = t;
			_logPath = path;

			if(_loggerT!=null) _loggerT.Close();
			if(_loggerB!=null) _loggerB.Close();

			switch(t) {
				case LogType.None:
					_loggerT = new NullTextLogger();
					_loggerB = new NullBinaryLogger();
					break;
				case LogType.Default:
					_loggerT = new DefaultLogger(new StreamWriter(path, append, Encoding.Default));
					_loggerB = new NullBinaryLogger();
					break;
				case LogType.Binary:
					_loggerT = new NullTextLogger();
					_loggerB = new BinaryLogger(new FileStream(path, append? FileMode.Append : FileMode.Create));
					break;
				case LogType.Xml:
					_loggerT = new XmlLogger(new StreamWriter(path, append, Encoding.UTF8), _param); //DebugLogはUTF8
					_loggerB = new NullBinaryLogger();
					break;
			}
			_loggerT = new InternalLoggerT(_loggerT, this);
			_loggerB = new InternalLoggerB(_loggerB, this);
			_loggerT.TerminalResized(_width, _height);
		}


		private class InternalLoggerT : ITerminalTextLogger {
			private TerminalConnection _parent;
			private ITerminalTextLogger _logger;

			public InternalLoggerT(ITerminalTextLogger l, TerminalConnection p) {
				_parent = p;
				_logger = l;
			}

			public void Append(char ch) {
				if(!_parent.LogSuspended) _logger.Append(ch);
			}
			public void Append(char[] ch) {
				if(!_parent.LogSuspended) _logger.Append(ch);
			}
			public void Append(char[] ch, int offset, int length) {
				if(!_parent.LogSuspended) _logger.Append(ch,offset,length);
			}
			public void BeginEscapeSequence() {
				if(!_parent.LogSuspended) _logger.BeginEscapeSequence();
			}
			public void AbortEscapeSequence() {
				if(!_parent.LogSuspended) _logger.AbortEscapeSequence();
			}
			public void CommitEscapeSequence() {
				if(!_parent.LogSuspended) _logger.CommitEscapeSequence();
			}
			public void Comment(string comment) {
				if(!_parent.LogSuspended) _logger.Comment(comment);
			}
			public void Flush() {
				_logger.Flush();
			}
			public void Close() {
				_logger.Close();
			}
			public bool IsActive {
				get {
					return _logger.IsActive;
				}
			}
			public void PacketDelimiter() {
				if(!_parent.LogSuspended) _logger.PacketDelimiter();
			}
			public void TerminalResized(int width, int height) {
				if(!_parent.LogSuspended) _logger.TerminalResized(width, height);
			}
			public void WriteLine(GLine line) {
				if(!_parent.LogSuspended) _logger.WriteLine(line);
			}

		}
		private class InternalLoggerB : ITerminalBinaryLogger {
			private TerminalConnection _parent;
			private ITerminalBinaryLogger _logger;

			public InternalLoggerB(ITerminalBinaryLogger l, TerminalConnection p) {
				_parent = p;
				_logger = l;
			}

			public void Append(byte[] data, int offset, int length) {
				if(!_parent.LogSuspended) _logger.Append(data,offset,length);
			}
			public void Comment(string comment) {
				if(!_parent.LogSuspended) _logger.Comment(comment);
			}
			public void Flush() {
				_logger.Flush();
			}
			public void Close() {
				_logger.Close();
			}
			public bool IsActive {
				get {
					return _logger.IsActive;
				}
			}
		}
	}
	internal abstract class TCPTerminalConnection : TerminalConnection {

		protected bool _usingSocks;

		protected TCPTerminalConnection(TerminalParam p, int w, int h) : base(p, w, h) {
			_usingSocks = false;
		}
		public TCPTerminalParam TCPParam {
			get {
				return _param as TCPTerminalParam;
			}
		}
		public override string ProtocolDescription {
			get {
				string s = _param.MethodName;
				if(_usingSocks) s += GEnv.Strings.GetString("Caption.TCPTerminalConnection.UsingSOCKS");
				return s;
			}
		}

		//設定は最初だけ行う
		public bool UsingSocks {
			get {
				return _usingSocks;
			}
			set {
				_usingSocks = value;
			}
		}
	}
	internal class SSHTerminalConnection : TCPTerminalConnection, ISSHConnectionEventReceiver, ISSHChannelEventReceiver {
		private SSHConnection _connection;
		private SSHChannel _channel;
		private MemoryStream _passwordBuffer;
		private bool _waitingSendBreakReply;
		public SSHTerminalConnection(TCPTerminalParam p, int width, int height) : base(p, width, height) {
		}

		public override ConnectionTag Reproduce() {
			SSHTerminalParam sshp = (SSHTerminalParam)_param.Clone();
			if(GEnv.Options.DefaultLogType!=LogType.None) {
				sshp.LogType = GEnv.Options.DefaultLogType;
				sshp.LogPath = GUtil.CreateLogFileName(sshp.ShortDescription);
			}
			else
				sshp.LogType = LogType.None;

			if(sshp.Method==ConnectionMethod.SSH2 && !this.IsClosed) { //SSH2のときはコネクションを共有してマルチチャネルを使用
				System.Drawing.Size sz = GEnv.Frame.TerminalSizeForNextConnection;
				SSHTerminalConnection newcon = new SSHTerminalConnection(sshp, sz.Width, sz.Height);
				newcon.FixConnection(_connection);
				newcon.OpenShell();
				newcon.SetServerInfo(_serverName, _serverAddress);
				return new ConnectionTag(newcon);
			}
			else { 
				bool pp_found = (sshp.Passphrase!=null && sshp.Passphrase.Length>0);
				if(sshp.Method==ConnectionMethod.SSH1 && !pp_found)
					throw new ApplicationException(GEnv.Strings.GetString("Message.SSHTerminalConnection.ReproduceErrorOnSSH1"));
				if(sshp.Method==ConnectionMethod.SSH2 && !pp_found && this.IsClosed)
					throw new ApplicationException(GEnv.Strings.GetString("Message.SSHTerminalConnection.ReproduceErrorOnSSH2"));
				HostKeyChecker checker = new HostKeyChecker(GEnv.Frame, sshp);
				return CommunicationUtil.CreateNewConnection(sshp, new HostKeyCheckCallback(checker.CheckHostKeyCallback));
			}
		}


		public void FixConnection(SSHConnection con) {
			_connection = con;
		}

		public SSHConnectionInfo ConnectionInfo {
			get {
				return _connection.ConnectionInfo;
			}
		}

		public void OpenShell() {
			_channel = _connection.OpenShell(this);
		}

		public override void Disconnect() {
			Close();
		}

		public override void Close() {
			if(_closed) return; //２度以上クローズしても副作用なし 
			base.Close();
			try {
				if(_channel!=null) _channel.Close();
				if(_connection.ChannelCount==0) {
					_connection.Close();
				}
			}
			catch(Exception ex) {
				GUtil.Warning(GEnv.Frame, GEnv.Strings.GetString("Message.SSHTerminalConnection.CloseError")+ex.Message);
			}
		}

		public override string[] ConnectionParameter {
			get {
				string[] d = new string[2];
				d[0] = String.Format("ServerVersionString: {0}", _connection.ConnectionInfo.ServerVersionString);
				d[1] = String.Format("EncryptionAlgorithm: {0}",_connection.ConnectionInfo.AlgorithmForReception.ToString());
				return d;
			}
		}
		public override bool Available {
			get {
				return !_closed && _connection.Available;
			}
		}
		public override void Write(byte[] buf) {
			if(_connection.AuthenticationResult==AuthenticationResult.Prompt)
				InputAuthenticationResponse(buf, 0, buf.Length);
			else {
				AddSentDataStats(buf.Length);
				_channel.Transmit(buf);
			}
		}
		public override void Write(byte[] buf, int offset, int length) {
			if(_connection.AuthenticationResult==AuthenticationResult.Prompt)
				InputAuthenticationResponse(buf, offset, length);
			else {
				AddSentDataStats(length);
				_channel.Transmit(buf, offset, length);
			}
		}

		//authentication process for keyboard-interactive
		private void InputAuthenticationResponse(byte[] buf, int offset, int length) {
			for(int i=offset; i<offset+length; i++) {
				byte b = buf[i];
				if(_passwordBuffer==null) _passwordBuffer = new MemoryStream();
				if(b==13 || b==10) { //CR/LF
					byte[] pwd = _passwordBuffer.ToArray();
					if(pwd.Length>0) {
						_passwordBuffer.Close();
						string[] response = new string[1];
						response[0] = Encoding.ASCII.GetString(pwd);
						OnData(Encoding.ASCII.GetBytes("\r\n"), 0, 2); //表示上改行しないと格好悪い
						if(((Granados.SSHCV2.SSH2Connection)_connection).DoKeyboardInteractiveAuth(response)==AuthenticationResult.Success)
							_channel = _connection.OpenShell(this);
						_passwordBuffer = null;
					}
				}
				else if(b==3 || b==27) { //Ctrl+C, ESC
					GEnv.GetConnectionCommandTarget(this).Disconnect();
					return;
				}
				else
					_passwordBuffer.WriteByte(b);
			}
		}

		//非同期に受信する。
		private IDataReceiver _callback;
		private MemoryStream _buffer;
		public override void RepeatAsyncRead(IDataReceiver cb) {
			_callback = cb;
			if(_buffer!=null) {
				lock(this) {
					_buffer.Close();
					byte[] t = _buffer.ToArray();
					if(t.Length>0) _callback.DataArrived(t, 0, t.Length);
					_buffer = null;
				}
			}
		}


		public override void Resize(int width, int height) {
			if(_connection.AuthenticationResult!=AuthenticationResult.Success) return;
			base.Resize(width, height);
			if(!_closed)
				_channel.ResizeTerminal(width, height, 0, 0);
		}
		public override void SendBreak() {
			if(_connection.AuthenticationResult!=AuthenticationResult.Success) return;
			if(((TCPTerminalParam)_param).Method==ConnectionMethod.SSH1)
				base.SendBreak(); //これで拒否になる
			else {
				_waitingSendBreakReply = true;
				((Granados.SSHCV2.SSH2Channel)_channel).SendBreak(500);
			}
		}
		public override void SendKeepAliveData() {
			_connection.SendIgnorableData("keep alive");
		}

		public void OnChannelClosed() {
			if(!_closed)
				_callback.DisconnectedFromServer();

			if (LoggedOff != null)
				LoggedOff(this, null);
		}
		public void OnChannelEOF() {
			if(!_closed)
				_callback.DisconnectedFromServer();
		}
		public void OnData(byte[] data, int offset, int length) {
			if(_callback==null) { //RepeatAsyncReadが呼ばれる前のデータを集めておく
				lock(this) {
					if(_buffer==null) _buffer = new MemoryStream(0x100);
					_buffer.Write(data, offset, length);
				}
			}
			else
				_callback.DataArrived(data, offset, length);
		}
		public void OnAuthenticationPrompt(string[] msg) {
			for(int i=0; i<msg.Length; i++) {
				if(i!=0) msg[i] += "\r\n";
				byte[] buf = Encoding.ASCII.GetBytes(msg[i]);
				OnData(buf, 0, buf.Length);
			}
		}
		public void OnExtendedData(int type, byte[] data) {
		}
		public void OnMiscPacket(byte type, byte[] data, int offset, int length) {
			if(_waitingSendBreakReply) {
				_waitingSendBreakReply = false;
				if(type==(byte)Granados.SSHCV2.PacketType.SSH_MSG_CHANNEL_FAILURE)
					GEnv.InterThreadUIService.Warning(GEnv.Strings.GetString("Message.SSHTerminalconnection.BreakError"));
			}
		}
		public void OnConnectionClosed() {
			if(!_closed) {
				EnsureCallbackHandler();
				_callback.DisconnectedFromServer();
			}

            if (Disconnected != null)
                Disconnected(this, null);
		}
		public void OnDebugMessage(bool display, byte[] data) {
			Debug.WriteLine(String.Format("SSH debug {0}[{1}]", data.Length, data[0] ));
		}
		public void OnIgnoreMessage(byte[] data) {
			Debug.WriteLine(String.Format("SSH ignore {0}[{1}]", data.Length, data[0] ));
		}
		public void OnUnknownMessage(byte type, byte[] data) {
			Debug.WriteLine(String.Format("Unexpected SSH packet type {0}", type));
		}

		public void OnChannelReady() { //!!Transmitを許可する通知が必要？
            if (Connected != null)
                Connected(this, null);
		}

		public void OnChannelError(Exception ex, string msg) {
			if(!_closed) {
				EnsureCallbackHandler();
				_callback.ErrorOccurred(msg);
			}

			if (_closed && Disconnected != null) {
				Disconnected(this, new ErrorEventArgs(new Exception(msg)));
			}
		}
		public void OnError(Exception ex, string msg) {
			if(!_closed) {
				EnsureCallbackHandler();
				_callback.ErrorOccurred(msg);
			}

			if (_closed && Disconnected != null) {
				Disconnected(this, new ErrorEventArgs(new Exception(msg)));
			}
		}

		public Granados.SSHC.PortForwardingCheckResult CheckPortForwardingRequest(string host, int port, string originator, int originator_port) {
			return new Granados.SSHC.PortForwardingCheckResult();
		}
		public void EstablishPortforwarding(ISSHChannelEventReceiver receiver, SSHChannel channel) {
		}

	    public event EventHandler Connected;
	    public event EventHandler Disconnected;
		public event EventHandler LoggedOff;

	    private void EnsureCallbackHandler() {
			int n = 0;
			while(_callback==null && n++<10) //わずかな時間差でハンドラがセットされないこともある
				Thread.Sleep(100);
		}
	}
	internal class TelnetTerminalConnection : TCPTerminalConnection, IDataReceiver {
		private AbstractGuevaraSocket _socket;
		private TelnetNegotiator _negotiator;

		public TelnetTerminalConnection(TerminalParam p, TelnetNegotiator neg, AbstractGuevaraSocket s, int width, int height) : base(p, width, height) {
			_socket = s;
			_negotiator = neg;
		}

		public override void Close() {
			if(_closed) return; //２度以上クローズしても副作用なし 
			base.Close();
			try {
				_socket.Close();
			}
			catch(Exception ex) {
				GUtil.Warning(GEnv.Frame, GEnv.Strings.GetString("Message.SSHTerminalConnection.CloseError")+ex.Message);
			}
		}
		public override void Disconnect() {
		}
		public override ConnectionTag Reproduce() {
			TerminalParam np = (TerminalParam)_param.Clone();
			if(GEnv.Options.DefaultLogType!=LogType.None) {
				np.LogType = GEnv.Options.DefaultLogType;
				np.LogPath = GUtil.CreateLogFileName(np.ShortDescription);
			}
			else
				np.LogType = LogType.None;

			return CommunicationUtil.CreateNewConnection(np);
		}


		public override string[] ConnectionParameter {
			get {
				string[] d = new string[1];
				d[0] = String.Format("");
				return d;
			}
		}

		public override bool Available {
			get {
				return !_closed && _socket.DataAvailable;
			}
		}

		private IDataReceiver _callback;
		public override void RepeatAsyncRead(IDataReceiver cb) {
			if(_callback!=null) throw new InvalidOperationException("duplicated AsyncRead() is attempted");
			
			_callback = cb;
			_socket.RepeatAsyncRead(this);
		}

		public void DataArrived(byte[] buf, int offset, int count) {
			ProcessBuffer(buf, offset, offset+count);
			_negotiator.Flush(_socket);
		}

		public void DisconnectedFromServer() {
			if(!_closed)
				_callback.DisconnectedFromServer();
		}

		public void ErrorOccurred(string msg) {
			if(!_closed)
				_callback.ErrorOccurred(msg);
		}

		//CR NUL -> CR
		private void ProcessBuffer(byte[] buf, int offset, int limit) {
			while(offset < limit) {
				while(offset < limit && _negotiator.InProcessing) {
					if(_negotiator.Process(buf[offset++])==TelnetNegotiator.ProcessResult.REAL_0xFF)
						_callback.DataArrived(buf, offset-1, 1);
				}

				int delim = offset;
				while(delim < limit) {
					byte b = buf[delim];
					if(b==0xFF) {
						_negotiator.StartNegotiate();
						break;
					}
					if(b==0 && delim-1>=0 && buf[delim-1]==0x0D) break; //CR NULサポート
					delim++;
				}

				if(delim>offset) _callback.DataArrived(buf, offset, delim-offset); //delimの手前まで処理
				offset = delim+1;
			}

		}

		public override void Resize(int width, int height) {
			base.Resize(width, height);
			if(!_closed) {
				TelnetOptionWriter wr = new TelnetOptionWriter();
				wr.WriteTerminalSize(width, height);
				wr.WriteTo(_socket);
			}
		}

		public override void Write(byte[] buf) {
			Write(buf, 0, buf.Length);
		}
		public override void Write(byte[] buf, int offset, int length) {
			AddSentDataStats(length);
			for(int i=0; i<length; i++) {
				byte t = buf[offset+i];
				if(t==0xFF || t==0x0D) { //0xFFまたはCRLF以外のCRを見つけたら
					WriteEscaping(buf, offset, length);
					return;
				}
			}
			_socket.Transmit(buf, offset, length); //大抵の場合はこういうデータは入っていないので、高速化のためそのまま送り出す
		}
		private void WriteEscaping(byte[] buf, int offset, int length) {
			byte[] newbuf = new byte[length*2];
			int newoffset = 0;
			for(int i=0; i<length; i++) {
				byte t = buf[offset+i];
				if(t==0xFF) {
					newbuf[newoffset++] = 0xFF;
					newbuf[newoffset++] = 0xFF; //２個
				}
				else if(t==0x0D/* && (i==length-1 || buf[offset+i+1]!=0x0A)*/) {
					newbuf[newoffset++] = 0x0D;
					newbuf[newoffset++] = 0x00;
				}
				else
					newbuf[newoffset++] = t;
			}
			_socket.Transmit(newbuf, 0, newoffset);
		}

		public override void AreYouThere() {
			byte[] data = new byte[2];
			data[0] = (byte)TelnetCode.IAC;
			data[1] = (byte)TelnetCode.AreYouThere;
			_socket.Transmit(data, 0, data.Length);
		}
		public override void SendBreak() {
			byte[] data = new byte[2];
			data[0] = (byte)TelnetCode.IAC;
			data[1] = (byte)TelnetCode.Break;
			_socket.Transmit(data, 0, data.Length);
		}
		public override void SendKeepAliveData() {
			byte[] data = new byte[2];
			data[0] = (byte)TelnetCode.IAC;
			data[1] = (byte)TelnetCode.NOP;
			_socket.Transmit(data, 0, data.Length);
		}


	}
	public class SerialTerminalConnection : TerminalConnection {
		private byte[] _buf;

		//シリアルの非同期通信をちゃんとやろうとすると.NETライブラリでは不十分なのでほぼAPI直読み
		private IntPtr _fileHandle;

		private Win32.OVERLAPPED _readOL;
		private Win32.OVERLAPPED _writeOL;

		public SerialTerminalConnection(SerialTerminalParam p, IntPtr fh, int width, int height) : base(p, width, height) {
			_fileHandle = fh;
			_buf = new byte[0x1000];
			_readOL.hEvent  = Win32.CreateEvent(IntPtr.Zero, 0, 0, null);
			_writeOL.hEvent = Win32.CreateEvent(IntPtr.Zero, 0, 0, null);
		}
		public override void Close() {
			if(_closed) return; //２度以上クローズしても副作用なし 
			base.Close();
			
			Win32.CloseHandle(_readOL.hEvent);
			Win32.CloseHandle(_writeOL.hEvent);
			Win32.CloseHandle(_fileHandle);
			_readOL.hEvent = _writeOL.hEvent = _fileHandle = IntPtr.Zero;
			//Debug.WriteLine("COM connection termingating...");
		}
		public override ConnectionTag Reproduce() {
			throw new NotSupportedException(GEnv.Strings.GetString("Message.SerialTerminalConnection.ReproduceError"));
		}

		public override void Disconnect() {
		}
		public override string ProtocolDescription {
			get {
				return "-";
			}
		}

		public override string[] ConnectionParameter {
			get {
				string[] d = new string[6];
				SerialTerminalParam p = (SerialTerminalParam)_param;
				d[0] = String.Format("Port         : COM{0}", p.Port);
				d[1] = String.Format("Baud rate    : {0}", p.BaudRate);
				d[2] = String.Format("Data bits    : {0}", p.ByteSize);
				d[3] = String.Format("Stop bits    : {0}", EnumDescAttribute.For(typeof(StopBits)).GetDescription(p.StopBits));
				d[4] = String.Format("Parity       : {0}", EnumDescAttribute.For(typeof(Parity)).GetDescription(p.Parity));
				d[5] = String.Format("Flow Control : {0}", EnumDescAttribute.For(typeof(FlowControl)).GetDescription(p.FlowControl));
				return d;
			}
		}
		public override bool Available {
			get {
				return false; //シリアルだとよくわからないのでfalseを返しておく
			}
		}
		private IDataReceiver _callback;
		public override void RepeatAsyncRead(IDataReceiver cb) {
			if(_callback!=null) throw new InvalidOperationException("duplicated AsyncRead() is attempted");
			
			_callback = cb;
			GUtil.CreateThread(new ThreadStart(AsyncEntry)).Start();
			//_stream.BeginRead(_buf, 0, _buf.Length, new AsyncCallback(RepeatCallback), null);
		}

		private void AsyncEntry() {
			Win32.OVERLAPPED ol = new Win32.OVERLAPPED();
			try {
				//初期化
				bool success = false;
				int len = 0, flags = 0;
				ol.hEvent = Win32.CreateEvent(IntPtr.Zero, 0, 0, null);
				success = Win32.ClearCommError(_fileHandle, out flags, IntPtr.Zero);
				//このSetCommMaskを実行しないとWaitCommEventが失敗してしまう
				success = Win32.SetCommMask(_fileHandle, 0);
				success = Win32.SetCommMask(_fileHandle, 1); //EV_RXCHAR

				byte[] buf = new byte[128];
				while(true) {
					success = Win32.WaitCommEvent(_fileHandle, out flags, ref ol); //ここは普通falseが返る
					if(!success && Win32.GetLastError()!=Win32.ERROR_IO_PENDING)
						throw new Exception("WaitCommEvent failed " + Win32.GetLastError());

					//ここでデータが来るまでブロックする。ただ、ドキュメントによればWaitCommEventに対応するGetOverlappedResultはlenの値は無意味とのこと
					success = Win32.GetOverlappedResult(_fileHandle, ref ol, ref len, true);
					if(!success) break; //たとえば接続を切るなどでfalseが返ってくる

					do {
						len = 0;
						success = Win32.ReadFile(_fileHandle, buf, buf.Length, ref len, ref _readOL);
						//このWaitForSingleObjectが必要かどうかがよくわからない
						//if(Win32.WaitForSingleObject(_readOL.hEvent, 5000)!=Win32.WAIT_OBJECT_0)
						//	throw new Exception("WaitForSingleObject timed out");
						success = Win32.GetOverlappedResult(_fileHandle, ref _readOL, ref len, true);
						if(len>0) _callback.DataArrived(buf, 0, len);
					} while(len > 0);
				}
			}
			catch(Exception ex) {
				if(!_closed) {
					_callback.ErrorOccurred(ex.Message);
				}
			}
			finally {
				Win32.CloseHandle(ol.hEvent);
				//Debug.WriteLine("COM thread terminating...");
			}
		}

		public override void Write(byte[] buf) {
			Write(buf, 0, buf.Length);
		}
		public override void Write(byte[] data, int offset, int length) {
			SerialTerminalParam sp = (SerialTerminalParam)_param;
			if(sp.TransmitDelayPerChar==0) {
				if(sp.TransmitDelayPerLine==0)
					WriteMain(data, offset, length); //最も単純
				else { //改行のみウェイト挿入
					byte nl = (byte)(sp.TransmitNL==NewLine.CR? 13 : 10); 
					int limit = offset+length;
					int c = offset;
					while(offset<limit) {
						if(data[offset]==nl) {
							WriteMain(data, c, offset-c+1);
							Thread.Sleep(sp.TransmitDelayPerLine);
							System.Windows.Forms.Application.DoEvents();
							c = offset+1;
						}
						offset++;
					}
					if(c < limit) WriteMain(data, c, limit-c);
				}
			}
			else {
				byte nl = (byte)(sp.TransmitNL==NewLine.CR? 13 : 10); 
				for(int i=0; i<length; i++) {
					WriteMain(data, offset+i, 1);
					Thread.Sleep(data[offset+i]==nl? sp.TransmitDelayPerLine : sp.TransmitDelayPerChar);
					System.Windows.Forms.Application.DoEvents();
				}
			}

		}

		private void WriteMain(byte[] buf, int offset, int length) {
			if(length==0) return; //長さ０だと fixed(byte* p = buf) が失敗してしまう
			
			AddSentDataStats(length);
			//_stream.Write(buf, 0, buf.Length);
			int result = 0;
			if(offset!=0) {
				byte[] nb = new byte[length];
				Array.Copy(buf, offset, nb, 0, length);
				buf = nb; //次のWriteFileでoffsetが０でないときはすぐにはサポートできない
			}
			bool success;
			success = Win32.WriteFile(_fileHandle, buf, length, ref result, ref _writeOL);
			if(Win32.GetLastError()!=Win32.ERROR_IO_PENDING)
				throw new IOException("WriteFile failed for " + Win32.GetLastError());
			//このWaitForSingleObjectが必要かどうかがよくわからない
			//err = Win32.WaitForSingleObject(_writeOL.hEvent, 10000);
			success = Win32.GetOverlappedResult(_fileHandle, ref _writeOL, ref result, true);
			Debug.Assert(success);
		}
		public override void SendBreak() {
			Win32.SetCommBreak(_fileHandle);
			System.Threading.Thread.Sleep(500); //500ms待機
			Win32.ClearCommBreak(_fileHandle);
		}

		public void ApplySerialParam(SerialTerminalParam param) {
			//paramの内容でDCBを更新してセットしなおす
			Win32.DCB dcb = new Win32.DCB();
			CommunicationUtil.FillDCB(_fileHandle, ref dcb);
			CommunicationUtil.UpdateDCB(ref dcb, param);

			if(!Win32.SetCommState(_fileHandle, ref dcb))
				throw new ArgumentException(GEnv.Strings.GetString("Message.SerialTerminalConnection.ConfigError"));

			_param = param; //SetCommStateが成功したら更新
		}

	}

	/*
	public class ProcessConnection : TerminalConnection {
		private Process _process;
		private byte[] _bufOut;
		private byte[] _bufErr;

		public ProcessConnection(ProcessTerminalParam param, Process proc, int width, int height) : base(param, width, height) {
			_process = proc;
			_bufOut = new byte[0x1000];
			_bufErr = new byte[0x1000];
		}
		internal override void Close() {
			if(_closed) return; //２度以上クローズしても副作用なし 
			base.Close();
			
		}
		public override ConnectionTag Reproduce() {
			throw new NotSupportedException("プロセスへの接続の複製はできません。");
		}

		internal override void Disconnect() {
			_process.Close();
		}
		public override string ProtocolDescription {
			get {
				return "-";
			}
		}

		public override string[] ConnectionParameter {
			get {
				string[] d = new string[1];
				d[0] = ((ProcessTerminalParam)_param).Process;
				return d;
			}
		}
		public override bool Available {
			get {
				return false; 
			}
		}
		private IDataReceiver _callback;
		internal override void RepeatAsyncRead(IDataReceiver cb) {
			if(_callback!=null) throw new InvalidOperationException("duplicated AsyncRead() is attempted");
			
			_callback = cb;
			_process.StandardOutput.BaseStream.BeginRead(_bufOut, 0, _bufOut.Length, new AsyncCallback(RepeatCallback), _bufOut);
			_process.StandardError. BaseStream.BeginRead(_bufErr, 0, _bufErr.Length, new AsyncCallback(RepeatCallback), _bufErr);
		}
		private void RepeatCallback(IAsyncResult res) {
			lock(this) {
				try {
					bool stderr = (res.AsyncState==_bufErr);
					Stream strm = stderr? _process.StandardError.BaseStream : _process.StandardOutput.BaseStream;
					int len = strm.EndRead(res);
					//Debug.WriteLine(String.Format("RC {0} {1}", stderr, len));
					byte[] buf = stderr? _bufErr : _bufOut;
					if(len > 0) {
						_callback.DataArrived(buf, 0, len);
						strm.BeginRead(buf, 0, buf.Length, new AsyncCallback(RepeatCallback), buf);
					}
					else if(len < 0) {
						strm.BeginRead(buf, 0, buf.Length, new AsyncCallback(RepeatCallback), buf);
					}
					else
						_callback.DisconnectedFromServer();
				}
				catch(Exception ex) {
					_callback.ErrorOccurred(ex.Message);
				}
			}
		}

		public override void Write(byte[] buf) {
			Write(buf, 0, buf.Length);
		}
		public override void Write(byte[] data, int offset, int length) {
			_process.StandardInput.BaseStream.Write(data, offset, length);
			_process.StandardInput.BaseStream.Flush();
		}

	}
	*/
	public class FakeConnection : TerminalConnection {
		public FakeConnection(TerminalParam param) : base(param, 80, 25) {}
		public override void Write(byte[] data) {
		}
		public override void Write(byte[] data, int offset, int length) {
		}
		public override void RepeatAsyncRead(IDataReceiver r) {
		}
		public override string[] ConnectionParameter {
			get {
				return new string[1] { "fake connection" };
			}
		}
		public override bool Available {
			get {
				return false;
			}
		}
		public override string ProtocolDescription {
			get {
				return "";
			}
		}
		public override ConnectionTag Reproduce() {
			throw new NotSupportedException();
		}

	}
}
