/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: CommunicationUtil.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

using Granados.SSHC;

using Poderosa.ConnectionParam;
using Poderosa.Connection;
using Poderosa.Config;
using Poderosa.Forms;
using Poderosa.Terminal;
using Poderosa.Toolkit;
using Poderosa.SSH;
using Poderosa.LocalShell;

namespace Poderosa.Communication
{
	public class SSHConnector : SocketWithTimeout {

		private SSHTerminalParam _param;
		private Size _size;
		private string _password;
		private HostKeyCheckCallback _keycheck;
		private ConnectionTag _result;

		public SSHConnector(SSHTerminalParam param, Size size, string password, HostKeyCheckCallback keycheck) {
			_param = param;
			_size = size;
			_password = password;
			_keycheck = keycheck;
		}
		protected override string GetHostDescription() {
			return "SSH Server";
		}

		protected override void Negotiate() {
			SSHConnectionParameter con = new SSHConnectionParameter();
			con.Protocol = _param.Method==ConnectionMethod.SSH1? SSHProtocol.SSH1 : SSHProtocol.SSH2;
			con.CheckMACError = GEnv.Options.SSHCheckMAC;
			con.UserName = _param.Account;
			con.Password = _password;
			con.AuthenticationType = _param.AuthType==AuthType.KeyboardInteractive? AuthenticationType.KeyboardInteractive : _param.AuthType==AuthType.Password? AuthenticationType.Password : AuthenticationType.PublicKey;
			con.IdentityFile = _param.IdentityFile;
			con.TerminalWidth = _size.Width;
			con.TerminalHeight = _size.Height;
			con.TerminalName = EnumDescAttribute.For(typeof(TerminalType)).GetDescription(_param.TerminalType);
            //con.TerminalName = "xterm";
			con.WindowSize = GEnv.Options.SSHWindowSize;
			con.PreferableCipherAlgorithms = LocalSSHUtil.ParseCipherAlgorithm(GEnv.Options.CipherAlgorithmOrder);
			con.PreferableHostKeyAlgorithms = LocalSSHUtil.ParsePublicKeyAlgorithm(GEnv.Options.HostKeyAlgorithmOrder);
			if(_keycheck!=null) con.KeyCheck += new HostKeyCheckCallback(this.CheckKey);
										
			SSHTerminalConnection r = new SSHTerminalConnection(_param, _size.Width, _size.Height);
			SSHConnection ssh = SSHConnection.Connect(con, r, _socket);
            
			if(ssh!=null) {
				if(GEnv.Options.RetainsPassphrase)
					_param.Passphrase = _password; //接続成功時のみセット
				r.FixConnection(ssh);
				if(ssh.AuthenticationResult==AuthenticationResult.Success) r.OpenShell();
				r.UsingSocks = _socks!=null;
				r.SetServerInfo(_param.Host, this.IPAddress);
				_result = new ConnectionTag(r);
			}
			else {
				throw new IOException(GEnv.Strings.GetString("Message.SSHConnector.Cancelled"));
			}
		}
		protected override object Result {
			get {
				return _result;
			}
		}

		private bool CheckKey(SSHConnectionInfo ci) {
			SetIgnoreTimeout(); //これが呼ばれるということは途中までSSHのネゴシエートができているのでタイムアウトはしないようにする
			return _keycheck(ci);
		}
	}

	internal class TelnetConnector : SocketWithTimeout {
		private TelnetTerminalParam _param;
		private Size _size;
		private ConnectionTag _result;

		public TelnetConnector(TelnetTerminalParam param, Size size) {
			_param = param;
			_size = size;
		}
		
		protected override void Negotiate() {
			TelnetNegotiator neg = new TelnetNegotiator(_param, _size.Width, _size.Height);
			TelnetTerminalConnection r = new TelnetTerminalConnection(_param, neg, new PlainGuevaraSocket(_socket), _size.Width, _size.Height);
			r.UsingSocks = _socks!=null;
			r.SetServerInfo(_param.Host, this.IPAddress);
			_result = new ConnectionTag(r);
		}
		
		protected override object Result {
			get {
				return _result;
			}
		}
	}

	public class CommunicationUtil {

		public class SilentClient : ISocketWithTimeoutClient {
			
			private AutoResetEvent _event;
			private SocketWithTimeout _connector;
			private ConnectionTag _result;
			private string _errorMessage;

			public SilentClient() {
				_event = new AutoResetEvent(false);
			}

			public void SuccessfullyExit(object result) {
				_result = (ConnectionTag)result;
				//_result.SetServerInfo(((TCPTerminalParam)_result.Param).Host, swt.IPAddress);
				_event.Set();
			}
			public void ConnectionFailed(string message) {
				_errorMessage = message;
				_event.Set();
			}
			public void CancelTimer() {
			}

			public ConnectionTag Wait(SocketWithTimeout swt) {
				_connector = swt;
				//Form form = GEnv.Frame.AsForm();
				//if(form!=null) form.Cursor = Cursors.WaitCursor;
				_event.WaitOne(); 
				_event.Close();
				//if(form!=null) form.Cursor = Cursors.Default;
				if(_result==null) {
					//GUtil.Warning(GEnv.Frame, _errorMessage);
                    //MessageBox.Show("error");
					return null;
				}
				else
					return _result;
			}
			public IWin32Window GetWindow() {
				return GEnv.Frame;
			}
		}

		public static ConnectionTag CreateNewConnection(TerminalParam param) {
			if(param is SerialTerminalParam)
				return CreateNewSerialConnection(GEnv.Frame, (SerialTerminalParam)param);
			else if(param is LocalShellTerminalParam)
				return CreateNewLocalShellConnection(GEnv.Frame, (LocalShellTerminalParam)param);
			else {
				SilentClient s = new SilentClient();
				SocketWithTimeout swt = StartNewConnection(s, (TCPTerminalParam)param, null, null);
				if(swt==null) return null;
				else return s.Wait(swt);
			}
		}
		public static ConnectionTag CreateNewConnection(SSHTerminalParam param, HostKeyCheckCallback keycheck) {
			SilentClient s = new SilentClient();
			SocketWithTimeout swt = StartNewConnection(s, param, param.Passphrase, keycheck);
			if(swt==null) return null;
			else return s.Wait(swt);
		}

		public static SocketWithTimeout StartNewConnection(ISocketWithTimeoutClient client, TCPTerminalParam param, string password, HostKeyCheckCallback keycheck) {
			Size sz = GEnv.Frame.TerminalSizeForNextConnection;
            //Size sz = new System.Drawing.Size(528, 316);
			
			SocketWithTimeout swt;
			if(param.Method==ConnectionMethod.Telnet) { //Telnet
				swt = new TelnetConnector((TelnetTerminalParam)param, sz);
			}
			else { //SSH 
				swt = new SSHConnector((SSHTerminalParam)param, sz, password, keycheck);
			}

			if(GEnv.Options.UseSocks)
				swt.AsyncConnect(client, CreateSocksParam(param.Host, param.Port));
			else
				swt.AsyncConnect(client, param.Host, param.Port);
			return swt;
		}
		private static Socks CreateSocksParam(string dest_host, int dest_port) {
			Socks s = new Socks();
			s.DestName = dest_host;
			s.DestPort = (short)dest_port;
			s.Account = GEnv.Options.SocksAccount;
			s.Password = GEnv.Options.SocksPassword;
			s.ServerName = GEnv.Options.SocksServer;
			s.ServerPort = (short)GEnv.Options.SocksPort;
			s.ExcludingNetworks = GEnv.Options.SocksNANetworks;
			return s;
		}

		public static ConnectionTag CreateNewSerialConnection(IWin32Window parent, SerialTerminalParam param) {
			bool successful = false;
			FileStream strm = null;
			try {
				string portstr = String.Format("\\\\.\\COM{0}", param.Port);
				IntPtr ptr = Win32.CreateFile(portstr, Win32.GENERIC_READ|Win32.GENERIC_WRITE, 0, IntPtr.Zero, Win32.OPEN_EXISTING, Win32.FILE_ATTRIBUTE_NORMAL|Win32.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
				if(ptr==Win32.INVALID_HANDLE_VALUE) {
					string msg = GEnv.Strings.GetString("Message.CommunicationUtil.FailedToOpenSerial");
					int err = Win32.GetLastError();
					if(err==2) msg += GEnv.Strings.GetString("Message.CommunicationUtil.NoSuchDevice");
					else if(err==5) msg += GEnv.Strings.GetString("Message.CommunicationUtil.DeviceIsBusy");
					else msg += "\nGetLastError="+ Win32.GetLastError();
					throw new Exception(msg);
				}						
				//strm = new FileStream(ptr, FileAccess.Write, true, 8, true);
				Win32.DCB dcb = new Win32.DCB();
				FillDCB(ptr, ref dcb);
				UpdateDCB(ref dcb, param);

				if(!Win32.SetCommState(ptr, ref dcb))
					throw new Exception(GEnv.Strings.GetString("Message.CommunicationUtil.FailedToConfigSerial"));
				Win32.COMMTIMEOUTS timeouts = new Win32.COMMTIMEOUTS();
				Win32.GetCommTimeouts(ptr, ref timeouts);
				timeouts.ReadIntervalTimeout = 0xFFFFFFFF;
				timeouts.ReadTotalTimeoutConstant = 0;
				timeouts.ReadTotalTimeoutMultiplier = 0;
				timeouts.WriteTotalTimeoutConstant = 100;
				timeouts.WriteTotalTimeoutMultiplier = 100;
				Win32.SetCommTimeouts(ptr, ref timeouts);
				successful = true;
				System.Drawing.Size sz = GEnv.Frame.TerminalSizeForNextConnection;
				SerialTerminalConnection r = new SerialTerminalConnection(param, ptr, sz.Width, sz.Height);
				r.SetServerInfo("COM"+param.Port, null);
				return new ConnectionTag(r);
			}
			catch(Exception ex) {
				GUtil.Warning(parent, ex.Message);
				return null;
			}
			finally {
				if(!successful && strm!=null) strm.Close();
			}
		}

		public static ConnectionTag CreateNewLocalShellConnection(IWin32Window parent, LocalShellTerminalParam param) {
			return LocalShellUtil.PrepareSocket(parent, param);
		}
		

		public static bool FillDCB(IntPtr handle, ref Win32.DCB dcb) {
			dcb.DCBlength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32.DCB)); //sizeofくらいunsafeでなくても使わせてくれよ
			return Win32.GetCommState(handle, ref dcb);
		}
		public static void UpdateDCB(ref Win32.DCB dcb, SerialTerminalParam param) {
			dcb.BaudRate = (uint)param.BaudRate;
			dcb.ByteSize = param.ByteSize;
			dcb.Parity = (byte)param.Parity;
			dcb.StopBits = (byte)param.StopBits;
			//フロー制御：TeraTermのソースからちょっぱってきた
			if(param.FlowControl==FlowControl.Xon_Xoff) {
				//dcb.fOutX = TRUE;
				//dcb.fInX = TRUE;
				//dcbを完全にコントロールするオプションが必要かもな
				dcb.Misc |= 0x300; //上記２行のかわり
				dcb.XonLim = 2048; //CommXonLim;
				dcb.XoffLim = 2048; //CommXoffLim;
				dcb.XonChar = 0x11;
				dcb.XoffChar = 0x13;
			}
			else if(param.FlowControl==FlowControl.Hardware) {
				//dcb.fOutxCtsFlow = TRUE;
				//dcb.fRtsControl = RTS_CONTROL_HANDSHAKE;
				dcb.Misc |= 0x4 | 0x2000;
			}
		}
	}
}

