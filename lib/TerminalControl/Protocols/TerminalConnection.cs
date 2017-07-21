/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalConnection.cs,v 1.5 2012/03/14 16:33:38 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;

using Poderosa.Util;

using Granados;
using Granados.SSH2;
using Granados.KeyboardInteractive;

namespace Poderosa.Protocols {
    internal class PlainPoderosaSocket : IPoderosaSocket {
        private IByteAsyncInputStream _callback;
        private Socket _socket;
        private byte[] _buf;
        private ByteDataFragment _dataFragment;
        private AsyncCallback _callbackRoot;
        private TerminalConnection _ownerConnection;

        public PlainPoderosaSocket(Socket s) {
            _socket = s;
            _buf = new byte[ProtocolsPlugin.Instance.ProtocolOptions.SocketBufferSize];
            _dataFragment = new ByteDataFragment(_buf, 0, 0);
            _callbackRoot = new AsyncCallback(RepeatCallback);
        }

        public void SetOwnerConnection(TerminalConnection con) {
            _ownerConnection = con;
        }

        public void Transmit(ByteDataFragment data) {
            _socket.Send(data.Buffer, data.Offset, data.Length, SocketFlags.None);
        }
        public void Transmit(byte[] data, int offset, int length) {
            _socket.Send(data, offset, length, SocketFlags.None);
        }
        public void Close() {
            try {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Disconnect(false);
                Debug.WriteLineIf(DebugOpt.Socket, "PlainSocket close");
            }
            catch (Exception ex) {
                RuntimeUtil.SilentReportException(ex);
            }
        }
        public void ForceDisposed() {
            _socket.Close();
        }

        public void RepeatAsyncRead(IByteAsyncInputStream receiver) {
            _callback = receiver;
            BeginReceive();
        }

        private void RepeatCallback(IAsyncResult result) {
            try {
                int n = _socket.EndReceive(result);
                _dataFragment.Set(_buf, 0, n);
                Debug.Assert(_ownerConnection != null); //これを呼び出すようになるまでにはセットされていること！

                if (n > 0) {
                    if (OnReceptionCore(_dataFragment) == GenericResult.Succeeded)
                        BeginReceive();
                }
                else if (n < 0) {
                    //WindowsMEにおいては、ときどきここで-1が返ってきていることが発覚した。下のErrorCode 995の場合も同様
                    BeginReceive();
                }
                else {
                    OnNormalTerminationCore();
                }
            }
            catch (ObjectDisposedException) {
                // _socket has been closed
                OnNormalTerminationCore();
            }
            catch (Exception ex) {
                if (!_ownerConnection.IsClosed) {
                    RuntimeUtil.SilentReportException(ex);
                    if ((ex is SocketException) && ((SocketException)ex).ErrorCode == 995) {
                        BeginReceive();
                    }
                    else
                        OnAbnormalTerminationCore(ex.Message);
                }
            }
        }

        //IByteAsuncInputStreamのハンドラで例外が来るとけっこう惨事なのでこの中でしっかりガード

        private GenericResult OnReceptionCore(ByteDataFragment data) {
            try {
                _callback.OnReception(_dataFragment);
                return GenericResult.Succeeded;
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
                Close();
                return GenericResult.Failed;
            }
        }

        private GenericResult OnNormalTerminationCore() {
            try {
                _ownerConnection.CloseBySocket();
                _callback.OnNormalTermination();
                return GenericResult.Succeeded;
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
                _socket.Disconnect(false);
                return GenericResult.Failed;
            }
        }

        private GenericResult OnAbnormalTerminationCore(string msg) {
            try {
                _ownerConnection.CloseBySocket();
                _callback.OnAbnormalTermination(msg);
                return GenericResult.Succeeded;
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
                _socket.Disconnect(false);
                return GenericResult.Failed;
            }
        }

        public bool Available {
            get {
                return _socket.Available > 0;
            }
        }

        private void BeginReceive() {
            _socket.BeginReceive(_buf, 0, _buf.Length, SocketFlags.None, _callbackRoot, null);
        }
    }

    //送信したものをそのまま戻す
    internal class LoopbackSocket : IPoderosaSocket {
        private IByteAsyncInputStream _receiver;

        public void RepeatAsyncRead(IByteAsyncInputStream receiver) {
            _receiver = receiver;
        }

        public bool Available {
            get {
                return false;
            }
        }

        public void ForceDisposed() {
        }

        public void Transmit(ByteDataFragment data) {
            _receiver.OnReception(data);
        }

        public void Transmit(byte[] data, int offset, int length) {
            Transmit(new ByteDataFragment(data, offset, length));
        }

        public void Close() {
        }
    }


    public class ConnectionStats {
        private int _sentDataAmount;
        private int _receivedDataAmount;

        public int SentDataAmount {
            get {
                return _sentDataAmount;
            }
        }
        public int ReceivedDataAmount {
            get {
                return _receivedDataAmount;
            }
        }
        public void AddSentDataStats(int bytecount) {
            //_sentPacketCount++;
            _sentDataAmount += bytecount;
        }
        public void AddReceivedDataStats(int bytecount) {
            //_receivedPacketCount++;
            _receivedDataAmount += bytecount;
        }
    }

    public abstract class TerminalConnection : ITerminalConnection {
        protected ITerminalParameter _destination;
        protected ConnectionStats _stats;
        protected ITerminalOutput _terminalOutput; //派生クラスではこれをセットする
        protected IPoderosaSocket _socket;

        //すでにクローズされたかどうかのフラグ
        protected bool _closed;

        protected TerminalConnection(ITerminalParameter p) {
            _destination = p;
            _stats = new ConnectionStats();
        }

        public ITerminalParameter Destination {
            get {
                return _destination;
            }
            set {
                _destination = value;
            }
        }
        public ITerminalOutput TerminalOutput {
            get {
                return _terminalOutput;
            }
        }
        public IPoderosaSocket Socket {
            get {
                return _socket;
            }
        }
        public bool IsClosed {
            get {
                return _closed;
            }
        }

        //ソケット側でエラーが起きたときの処置
        public void CloseBySocket() {
            if (!_closed)
                CloseCore();
        }

        //終了処理
        public virtual void Close() {
            if (!_closed)
                CloseCore();
        }

        private void CloseCore() {
            _closed = true;
        }

        public virtual IAdaptable GetAdapter(Type adapter) {
            return ProtocolsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

    public abstract class TCPTerminalConnection : TerminalConnection {

        protected TCPTerminalConnection(ITCPParameter p)
            : base((ITerminalParameter)p.GetAdapter(typeof(ITerminalParameter))) {
        }
    }


    public class SSHTerminalConnection : TCPTerminalConnection {

        private readonly SSHSocket _sshSocket;
        private readonly ISSHLoginParameter _sshLoginParameter;

        public SSHTerminalConnection(ISSHLoginParameter ssh)
            : base((ITCPParameter)ssh.GetAdapter(typeof(ITCPParameter))) {
            _sshLoginParameter = ssh;
            SSHSocket s = new SSHSocket(this);
            _sshSocket = s;
            _socket = s;
            _terminalOutput = s;
        }
        public ISSHConnectionEventHandler ConnectionEventReceiver {
            get {
                return _sshSocket;
            }
        }
        public ISSHLoginParameter SSHLoginParameter {
            get {
                return _sshLoginParameter;
            }
        }
        public IKeyboardInteractiveAuthenticationHandler GetKeyboardInteractiveAuthenticationHandler() {
            return _sshSocket;
        }

        public void AttachTransmissionSide(ISSHConnection con, AuthenticationStatus authStatus) {
            _sshSocket.SetSSHConnection(con);
            if (authStatus == AuthenticationStatus.Success) {
                SSHSocket ss = (SSHSocket)_sshSocket;
                ss.OpenShell();
            }
            else if (authStatus == AuthenticationStatus.NeedKeyboardInput) {
                SSHSocket ss = (SSHSocket)_sshSocket;
                ss.OpenKeyboardInteractiveShell();
            }
        }

        public override void Close() {
            if (_closed)
                return; //２度以上クローズしても副作用なし 
            base.Close();
            _sshSocket.Close();
        }

#if false
        //BACK-BURNER: keyboard-interactive
        public override void Write(byte[] buf) {
            if (_connection.AuthenticationResult == AuthenticationResult.Prompt)
                InputAuthenticationResponse(buf, 0, buf.Length);
            else {
                AddSentDataStats(buf.Length);
                _channel.Transmit(buf);
            }
        }
        public override void Write(byte[] buf, int offset, int length) {
            if (_connection.AuthenticationResult == AuthenticationResult.Prompt)
                InputAuthenticationResponse(buf, offset, length);
            else {
                AddSentDataStats(length);
                _channel.Transmit(buf, offset, length);
            }
        }

        //authentication process for keyboard-interactive
        private void InputAuthenticationResponse(byte[] buf, int offset, int length) {
            for (int i = offset; i < offset + length; i++) {
                byte b = buf[i];
                if (_passwordBuffer == null)
                    _passwordBuffer = new MemoryStream();
                if (b == 13 || b == 10) { //CR/LF
                    byte[] pwd = _passwordBuffer.ToArray();
                    if (pwd.Length > 0) {
                        _passwordBuffer.Close();
                        string[] response = new string[1];
                        response[0] = Encoding.ASCII.GetString(pwd);
                        OnData(Encoding.ASCII.GetBytes("\r\n"), 0, 2); //表示上改行しないと格好悪い
                        if (((Granados.SSHCV2.SSH2Connection)_connection).DoKeyboardInteractiveAuth(response) == AuthenticationResult.Success)
                            _channel = _connection.OpenShell(this);
                        _passwordBuffer = null;
                    }
                }
                else if (b == 3 || b == 27) { //Ctrl+C, ESC
                    GEnv.GetConnectionCommandTarget(this).Disconnect();
                    return;
                }
                else
                    _passwordBuffer.WriteByte(b);
            }
        }
#endif

    }

    internal class TelnetReceiver : IByteAsyncInputStream {
        private IByteAsyncInputStream _callback;
        private TelnetNegotiator _negotiator;
        private TelnetTerminalConnection _parent;
        private ByteDataFragment _localdata;
        private bool _gotCR;

        public TelnetReceiver(TelnetTerminalConnection parent, TelnetNegotiator negotiator) {
            _parent = parent;
            _negotiator = negotiator;
            _localdata = new ByteDataFragment();
        }

        public void SetReceiver(IByteAsyncInputStream receiver) {
            _callback = receiver;
        }

        public void OnReception(ByteDataFragment data) {
            ProcessBuffer(data);
            if (!_parent.IsClosed)
                _negotiator.Flush(_parent.RawSocket);
        }

        public void OnNormalTermination() {
            _callback.OnNormalTermination();
        }

        public void OnAbnormalTermination(string msg) {
            _callback.OnAbnormalTermination(msg);
        }

        //CR NUL -> CR 変換および IACからはじまるシーケンスの処理
        private void ProcessBuffer(ByteDataFragment data) {
            int limit = data.Offset + data.Length;
            int offset = data.Offset;
            byte[] buf = data.Buffer;
            //Debug.WriteLine(String.Format("Telnet len={0}, proc={1}", data.Length, _negotiator.InProcessing));

            while (offset < limit) {
                while (offset < limit && _negotiator.InProcessing) {
                    if (_negotiator.Process(buf[offset++]) == TelnetNegotiator.ProcessResult.REAL_0xFF)
                        _callback.OnReception(_localdata.Set(buf, offset - 1, 1));
                }

                int delim = offset;
                while (delim < limit) {
                    byte b = buf[delim];
                    if (b == 0xFF) {
                        _gotCR = false;
                        _negotiator.StartNegotiate();
                        break;
                    }
                    if (b == 0 && _gotCR) {
                        _gotCR = false;
                        break; //CR NUL
                    }
                    _gotCR = (b == 0xd);
                    delim++;
                }

                if (delim > offset)
                    _callback.OnReception(_localdata.Set(buf, offset, delim - offset)); //delimの手前まで処理
                offset = delim + 1;
            }

        }
    }

    internal class TelnetSocket : IPoderosaSocket, ITerminalOutput {
        private IPoderosaSocket _socket;
        private TelnetReceiver _callback;
        private TelnetTerminalConnection _parent;
        private bool _telnetNewLine;

        public TelnetSocket(TelnetTerminalConnection parent, IPoderosaSocket socket, TelnetReceiver receiver, bool telnetNewLine) {
            _parent = parent;
            _callback = receiver;
            _socket = socket;
            _telnetNewLine = telnetNewLine;
        }

        public void RepeatAsyncRead(IByteAsyncInputStream callback) {
            _callback.SetReceiver(callback);
            _socket.RepeatAsyncRead(_callback);
        }

        public void Close() {
            _socket.Close();
        }
        public void ForceDisposed() {
            _socket.Close();
        }

        public void Resize(int width, int height) {
            if (!_parent.IsClosed) {
                TelnetOptionWriter wr = new TelnetOptionWriter();
                wr.WriteTerminalSize(width, height);
                wr.WriteTo(_socket);
            }
        }

        public void Transmit(ByteDataFragment data) {
            Transmit(data.Buffer, data.Offset, data.Length);
        }

        public void Transmit(byte[] buf, int offset, int length) {
            for (int i = 0; i < length; i++) {
                byte t = buf[offset + i];
                if (t == 0xFF || t == 0x0D) { //0xFFまたはCRLF以外のCRを見つけたら
                    WriteEscaping(buf, offset, length);
                    return;
                }
            }
            _socket.Transmit(buf, offset, length); //大抵の場合はこういうデータは入っていないので、高速化のためそのまま送り出す
        }
        private void WriteEscaping(byte[] buf, int offset, int length) {
            byte[] newbuf = new byte[length * 2];
            int newoffset = 0;
            for (int i = 0; i < length; i++) {
                byte t = buf[offset + i];
                if (t == 0xFF) {
                    newbuf[newoffset++] = 0xFF;
                    newbuf[newoffset++] = 0xFF; //２個
                }
                else if (t == 0x0D && !(_telnetNewLine && i + 1 < length && buf[offset + i + 1] == 0x0A)) {
                    // CR    --> CR NUL (Telnet CR)
                    // CR LF --> CR NUL LF
                    //        or CR LF (Telnet New Line)
                    newbuf[newoffset++] = 0x0D;
                    newbuf[newoffset++] = 0x00;
                }
                else
                    newbuf[newoffset++] = t;
            }
            _socket.Transmit(newbuf, 0, newoffset);
        }

        public bool Available {
            get {
                return _socket.Available;
            }
        }

        public void AreYouThere() {
            byte[] data = new byte[2];
            data[0] = (byte)TelnetCode.IAC;
            data[1] = (byte)TelnetCode.AreYouThere;
            _socket.Transmit(data, 0, data.Length);
        }
        public void SendBreak() {
            byte[] data = new byte[2];
            data[0] = (byte)TelnetCode.IAC;
            data[1] = (byte)TelnetCode.Break;
            _socket.Transmit(data, 0, data.Length);
        }
        public void SendKeepAliveData() {
            byte[] data = new byte[2];
            data[0] = (byte)TelnetCode.IAC;
            data[1] = (byte)TelnetCode.NOP;
            // Note:
            //  Disconnecting or Closing socket may happen before Send() is called.
            //  In such case, SocketException or ObjectDisposedException will be thrown in Send().
            //  We just ignore the exceptions.
            try {
                _socket.Transmit(data, 0, data.Length);
            }
            catch (SocketException) {
            }
            catch (ObjectDisposedException) {
            }
        }
    }

    internal class TelnetTerminalConnection : TCPTerminalConnection {
        private TelnetReceiver _telnetReceiver;
        private TelnetSocket _telnetSocket;
        private IPoderosaSocket _rawSocket;

        public TelnetTerminalConnection(ITCPParameter p, TelnetNegotiator neg, PlainPoderosaSocket s)
            : base(p) {
            s.SetOwnerConnection(this);
            _telnetReceiver = new TelnetReceiver(this, neg);
            ITelnetParameter telnetParams = (ITelnetParameter)p.GetAdapter(typeof(ITelnetParameter));
            bool telnetNewLine = (telnetParams != null) ? telnetParams.TelnetNewLine : true/*default*/;
            _telnetSocket = new TelnetSocket(this, s, _telnetReceiver, telnetNewLine);
            _rawSocket = s;
            _socket = _telnetSocket;
            _terminalOutput = _telnetSocket;
        }
        //Telnetのエスケープ機能つき
        public TelnetSocket TelnetSocket {
            get {
                return _telnetSocket;
            }
        }
        //TelnetSocketが内包する生ソケット
        public IPoderosaSocket RawSocket {
            get {
                return _rawSocket;
            }
        }

        public override void Close() {
            if (_closed)
                return; //２度以上クローズしても副作用なし 
            _telnetSocket.Close();
            base.Close();
        }



    }

    internal class RawTerminalConnection : ITerminalConnection, ITerminalOutput {
        private IPoderosaSocket _socket;
        private ITerminalParameter _terminalParameter;

        public RawTerminalConnection(IPoderosaSocket socket, ITerminalParameter tp) {
            _socket = socket;
            _terminalParameter = tp;
        }


        public ITerminalParameter Destination {
            get {
                return _terminalParameter;
            }
        }

        public ITerminalOutput TerminalOutput {
            get {
                return this;
            }
        }

        public IPoderosaSocket Socket {
            get {
                return _socket;
            }
        }

        public bool IsClosed {
            get {
                return false;
            }
        }

        public void Close() {
            _socket.Close();
        }

        public IAdaptable GetAdapter(Type adapter) {
            return ProtocolsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        //ITerminalOutputはシカト
        public void SendBreak() {
        }

        public void SendKeepAliveData() {
        }

        public void AreYouThere() {
        }

        public void Resize(int width, int height) {
        }
    }


}
