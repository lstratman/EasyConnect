/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: SocketWithTimeout.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Poderosa.Communication
{

	public interface ISocketWithTimeoutClient {
		void SuccessfullyExit(object result);
		void ConnectionFailed(string message);
		void CancelTimer();
		System.Windows.Forms.IWin32Window GetWindow();
	}


	/// <summary>
	/// ソケットを開き、接続を確立する手順をしつつタイムアウトや途中で中止することもできるための機能
	/// </summary>
	public abstract class SocketWithTimeout {
		public void AsyncConnect(ISocketWithTimeoutClient client, string host, int port) {
			_async = true;
			_client = client;
			_event = null;
			_host = host;
			_port = port;
			_socks = null;

			UI.UILibUtil.CreateThread(new ThreadStart(this.Run)).Start();
		}
		public void AsyncConnect(ISocketWithTimeoutClient client, Socks socks) {
			_async = true;
			_client = client;
			_event = null;
			_socks = socks;

			UI.UILibUtil.CreateThread(new ThreadStart(this.Run)).Start();
		}


		private void ExitProcess() {
			if(!_interrupted) {
				if(_succeeded)
					_client.SuccessfullyExit(this.Result);
				else
					_client.ConnectionFailed(_errorMessage);
			}
		}

		protected void SetIgnoreTimeout() {
			_ignoreTimeout = true;
			_client.CancelTimer();
		}

		public void Interrupt() {
			_interrupted = true;
			if(!_async)
				_event.Set();
			if(_tcpConnected)
				_socket.Close();
		}

		protected Socks _socks;

		protected ISocketWithTimeoutClient _client;
		protected IPAddressSet _addressSet;
		protected IPAddress _connectedAddress;
		protected AutoResetEvent _event;
		protected Socket _socket;
		protected string _host;
		protected int _port;

		protected bool _async;
		protected bool _succeeded;
		protected bool _interrupted;
		protected bool _ignoreTimeout;
		protected bool _tcpConnected;
		protected string _errorMessage;

		private void Run() {
			_tcpConnected = false;
			_ignoreTimeout = false;
			_succeeded = false;
			try {
				_addressSet = null;
				_errorMessage = null;
				MakeConnection();

				_errorMessage = null;
				Negotiate();
				
				_succeeded = true;
			}
			catch(Exception ex) {
				if(_errorMessage==null)
					_errorMessage = ex.Message;
				else
					_errorMessage += ex.Message;
			}
			finally {
				if(_async) {
					ExitProcess();
				}
				else {
					_event.Set();
					_event.Close();
				}

				if(_tcpConnected && !_interrupted && _errorMessage!=null) {
					try {
						_socket.Close();
					}
					catch(Exception) { //ここでちゃんと閉じることが出来ない場合があった
					}
				}
			}
		}

		protected virtual void MakeConnection() {
			//まずSOCKSを使うべきかどうかを判定する
			if(_socks!=null) {
				IPAddressSet a = null;
				try {
					a = new IPAddressSet(IPAddress.Parse(_socks.DestName));
				}
				catch(FormatException ) {
					try {
						a = new IPAddressSet(_socks.DestName);
					}
					catch(Exception) { //ここで名前解決できずとも、SOCKSサーバが解決できるかもしれないのでエラーにはしない
					}
				}
				
				if(a!=null && !SocksApplicapable(_socks.ExcludingNetworks, a)) { //SOCKSを使わないことが確定
					_addressSet = a;
					_host = _socks.DestName;
					_port = _socks.DestPort;
					_socks = null;
				}
			}

			string dest = _socks==null? _host : _socks.ServerName;
			int    port = _socks==null? _port : _socks.ServerPort;
			string msg  = _socks==null? GetHostDescription() : "SOCKS Server";
			if(_addressSet==null) {
				try {
					_addressSet = new IPAddressSet(IPAddress.Parse(dest)); //最初からIPアドレス形式のときは手で変換。そうでないとDNSの逆引きをしてタイムアウト、とかややこしいことが起こる
				}
				catch(FormatException ) {
					_errorMessage = String.Format("The {0} {1} was not found.", msg, dest);
					_addressSet = new IPAddressSet(dest);
				}
			}

			_errorMessage = String.Format("Failed to connect {0} {1}. Please check the address and the port.", msg, dest);
			_socket = NetUtil.ConnectTCPSocket(_addressSet, port);
			_connectedAddress = ((IPEndPoint)_socket.RemoteEndPoint).Address;
			
			if(_socks!=null) {
				_errorMessage = "An error occurred while SOCKS negotiation.";
				_socks.Connect(_socket);
				_host = _socks.DestName;
				_port = _socks.DestPort;
			}

			_tcpConnected = true;
		}


		//これをオーバーライドしてTCP接続後の動作をする
		protected abstract void Negotiate();

		protected virtual string GetHostDescription() {
			return "";
		}

		//成功したらこれを実装して結果を返す
		protected abstract object Result {
			get;
		}

		public Socks Socks {
			get {
				return _socks;
			}
			set {
				_socks = value;
			}
		}

		public bool Succeeded {
			get {
				return _succeeded;
			}
		}
		public bool Interrupted {
			get {
				return _interrupted;
			}
		}

		public string ErrorMessage {
			get {
				return _errorMessage;
			}
		}
		public IPAddress IPAddress {
			get {
				return _connectedAddress;
			}
		}

		private static bool SocksApplicapable(string nss, IPAddressSet address) {
			foreach(string netaddress in nss.Split(';')) {
				if(netaddress.Length==0) continue;

				if(!NetUtil.IsNetworkAddress(netaddress)) {
					throw new FormatException(String.Format("{0} is not suitable as a network address.", netaddress));
				}
				if(NetUtil.NetAddressIncludesIPAddress(netaddress, address.Primary))
					return false;
				else if(address.Secondary!=null && NetUtil.NetAddressIncludesIPAddress(netaddress, address.Secondary))
					return false;
			}
			return true;
		}
	}
}
