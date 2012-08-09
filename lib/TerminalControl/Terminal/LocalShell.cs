/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: LocalShell.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;

using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Config;
using Poderosa.Communication;
using Poderosa.Toolkit;

namespace Poderosa.LocalShell
{
	public abstract class LocalShellUtil {

		public static EncodingType DefaultEncoding {
			get {
				return GEnv.Options.Language==Language.Japanese? EncodingType.SHIFT_JIS : EncodingType.ISO8859_1;
			}
		}

		//接続用ソケットのサポート
		protected static Socket _listener;
		protected static int _localPort;
		//同期
		protected static object _lockObject = new object();

		//接続先のSocketを準備して返す。失敗すればparentを親にしてエラーを表示し、nullを返す。
		public static ConnectionTag PrepareSocket(IWin32Window parent, LocalShellTerminalParam param) {
			try {
				return new Connector(param).Connect();
			}
			catch(Exception ex) {
				string key = IsCygwin(param)? "Message.CygwinUtil.FailedToConnect" : "Message.SFUUtil.FailedToConnect";
				GUtil.Warning(parent, GEnv.Strings.GetString(key)+ex.Message);
				return null;
			}
		}
		public static Connector AsyncPrepareSocket(ISocketWithTimeoutClient client, LocalShellTerminalParam param) {
			Connector c = new Connector(param, client);
			GUtil.CreateThread(new ThreadStart(c.AsyncConnect)).Start();
			return c;
		}
		public class Connector {
			private LocalShellTerminalParam _param;
			private Process _process;
			private ISocketWithTimeoutClient _client;
			private Thread _asyncThread;
			private bool _interrupted;

			public Connector(LocalShellTerminalParam param) {
				_param = param;
			}
			public Connector(LocalShellTerminalParam param, ISocketWithTimeoutClient client) {
				_param = param;
				_client = client;
			}

			public void AsyncConnect() {
				bool success = false;
				_asyncThread = Thread.CurrentThread;
				try {
					ConnectionTag result = Connect();
					result.ChildProcess = _process;
					success = true;
					if(!_interrupted) _client.SuccessfullyExit(result);
				}
				catch(Exception ex) {
					if(!_interrupted) _client.ConnectionFailed(ex.Message);
				}
				finally {
					if(!success && _process!=null)
						_process.Kill();
				}
			}
			public void Interrupt() {
				_interrupted = true;
			}

			public ConnectionTag Connect() {
				lock(_lockObject) {
					if(_localPort==0)
						PrepareListener();

					PrepareEnv(_param);
				}

				string cygtermPath = "cygterm\\"+(IsCygwin(_param)? "cygterm.exe" : "sfuterm.exe");
				string connectionName = IsCygwin(_param)? "Cygwin" : "SFU";

				string args = String.Format("-p {0} -v HOME=\"{1}\" -s \"{2}\"", _localPort, _param.Home, _param.Shell);
				ProcessStartInfo psi = new ProcessStartInfo(cygtermPath, args);
				psi.CreateNoWindow = true;
				psi.ErrorDialog = true;
				psi.UseShellExecute = false;
				psi.WindowStyle = ProcessWindowStyle.Hidden;

				_process = Process.Start(psi);
				if(_interrupted) return null;
				Socket sock = _listener.Accept();
				if(_interrupted) return null;

				Size sz = GEnv.Frame.TerminalSizeForNextConnection;
				TelnetNegotiator neg = new TelnetNegotiator(_param, sz.Width, sz.Height);
				TelnetTerminalConnection r = new TelnetTerminalConnection(_param, neg, new PlainGuevaraSocket(sock), sz.Width, sz.Height);
				r.UsingSocks = false;
				r.SetServerInfo(connectionName, null);
				return new ConnectionTag(r);
			}

		}

		protected static void PrepareListener() {
			_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_localPort = 20345;
			do {
				try {
					_listener.Bind(new IPEndPoint(IPAddress.Loopback, _localPort));
					_listener.Listen(1);
					break;
				}
				catch(Exception) {
					if(_localPort++==20360) throw new Exception("port overflow!!"); //さすがにこれはめったにないはず
				}
			} while(true);

		}

		private static bool _cygwinDLL_loaded;
		protected static void PrepareEnv(LocalShellTerminalParam p) {
			if(!_cygwinDLL_loaded && IsCygwin(p)) {
				//初回のみ、cygwin.dllがロードできるように環境変数を追加 SFUのときは余計だが
				char[] buf = new char[1024];
				int n = Win32.GetEnvironmentVariable("PATH", buf, buf.Length);
				string newval = new string(buf, 0, n) + ";" + CygwinUtil.GuessRootDirectory() + "\\bin";
				Win32.SetEnvironmentVariable("PATH", newval);
				_cygwinDLL_loaded = true;
			}
		}

		public static void Terminate() {
			if(_listener!=null) _listener.Close();
		}

		private static bool IsCygwin(LocalShellTerminalParam tp) {
			return tp is CygwinTerminalParam;
		}

	}

	public class SFUUtil : LocalShellUtil
	{
		public static string DefaultHome {
			get {
				string a = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); 
				//最後の\の後にApplication Dataがあるので
				int t = a.LastIndexOf('\\');
				char drive = a[0];
				return "/dev/fs/"+Char.ToUpper(drive)+a.Substring(2, t-2).Replace('\\','/');
			}
		}
		public static string DefaultShell {
			get {
				return "/bin/csh -l";
			}
		}
		public static string GuessRootDirectory() {
			RegistryKey reg = null;
			string keyname = "SOFTWARE\\Microsoft\\Services for UNIX";
			reg = Registry.LocalMachine.OpenSubKey(keyname);
			if(reg==null) {
				GUtil.Warning(GEnv.Frame, String.Format(GEnv.Strings.GetString("Message.SFUUtil.KeyNotFound"), keyname));
				return "";
			}
			string t = (string)reg.GetValue("InstallPath");
			reg.Close();
			return t;
		}

	}

	public class CygwinUtil : LocalShellUtil {
		public static string DefaultHome {
			get {
				return "/home/"+Environment.UserName;
			}
		}
		public static string DefaultShell {
			get {
				return "/bin/bash -i -l";
			}
		}

		public static string GuessRootDirectory() {
			RegistryKey reg = null;
			string keyname = "SOFTWARE\\Cygnus Solutions\\Cygwin\\mounts v2\\/";
			//HKCU -> HKLMの順でサーチ
			reg = Registry.CurrentUser.OpenSubKey(keyname);
			if(reg==null) {
				reg = Registry.LocalMachine.OpenSubKey(keyname);
				if(reg==null) {
					GUtil.Warning(GEnv.Frame, String.Format(GEnv.Strings.GetString("Message.CygwinUtil.KeyNotFound"), keyname));
					return "";
				}
			}
			string t = (string)reg.GetValue("native");
			reg.Close();
			return t;
		}
	}
}
