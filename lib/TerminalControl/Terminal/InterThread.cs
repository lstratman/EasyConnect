/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: InterThread.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Threading;
using System.Windows.Forms;

using Poderosa.Connection;
using Poderosa.Communication;
using Poderosa.ConnectionParam;
using Poderosa.Text;
using Poderosa.Config;
using Poderosa.Forms;

namespace Poderosa
{
	//受信スレッドが直接メッセージボックスなどを出しにいくのは何かとトラブルの温床なので、
	//メインのスレッドと受け渡しをしながら処理する。
	//たとえば、オプションダイアログを出している間にサーバが切断を切ってきたときなど、GFrameを親にMessageBoxを呼んでしまい、その後の動作がちょっと不審になった。

	public class InterThreadUIService {
		private string _msg;
		private Exception _exception;
		private DialogResult _dialogResult;
		private TerminalConnection _connection;
		protected IntPtr _mainFrameHandle;

		public enum Service {
			Warning,
			Information,
			AskUserYesNo,
			DisconnectedFromServer,
			ToggleSelectionMode,
			IndicateBell,
			CriticalError,
			InvalidCharDetected,
			UnsupportedCharSetDetected,
			UnsupportedEscapeSequence,
			InvalidDocumentOperation,
			RefreshConnection,
			End
		}

		public IntPtr MainFrameHandle {
			get {
				return _mainFrameHandle;
			}
			set {
				_mainFrameHandle = value;
			}
		}

		//これらの各メソッドはそれぞれロックしながら実行される
		internal void Warning(TerminalDocument doc, string msg) {
			Monitor.Exit(doc);
			lock(this) {
				_msg = msg;
				SendMessageCore(Service.Warning);
			}
			Monitor.Enter(doc);
		}
		public void Warning(string msg) {
			lock(this) {
				_msg = msg;
				SendMessageCore(Service.Warning);
			}
		}
		public void Information(string msg) {
			lock(this) {
				_msg = msg;
				SendMessageCore(Service.Information);
			}
		}
		public DialogResult AskUserYesNo(string msg) {
			lock(this) {
				_msg = msg;
				SendMessageCore(Service.AskUserYesNo);
			}
			return _dialogResult;
		}
		internal void ReportCriticalError(TerminalDocument doc, Exception ex) {
			Monitor.Exit(doc);
			lock(this) {
				_exception = ex;
				SendMessageCore(Service.CriticalError);
			}
			Monitor.Enter(doc);
		}
		public void ReportCriticalError(Exception ex) {
			lock(this) {
				_exception = ex;
				SendMessageCore(Service.CriticalError);
			}
		}
		public void DisconnectedFromServer(TerminalConnection con) {
			lock(this) {
				_connection = con;
				SendMessageCore(Service.DisconnectedFromServer);
			}
		}
		internal void IndicateBell(TerminalDocument doc) {
			Monitor.Exit(doc);
			lock(this) {
				SendMessageCore(Service.IndicateBell);
			}
			Monitor.Enter(doc);
		}
		internal void InvalidCharDetected(TerminalDocument doc, string msg) {
			Monitor.Exit(doc);
			lock(this) {
				_msg = msg;
				SendMessageCore(Service.InvalidCharDetected);
			}
			Monitor.Enter(doc);
		}
		internal void UnsupportedCharSetDetected(TerminalDocument doc, string msg) {
			Monitor.Exit(doc);
			lock(this) {
				_msg = msg;
				SendMessageCore(Service.UnsupportedCharSetDetected);
			}
			Monitor.Enter(doc);
		}
		internal void UnsupportedEscapeSequence(TerminalDocument doc, string msg) {
			Monitor.Exit(doc);
			lock(this) {
				_msg = msg;
				SendMessageCore(Service.UnsupportedEscapeSequence);
			}
			Monitor.Enter(doc);
		}
		internal void InvalidDocumentOperation(TerminalDocument doc, string msg) {
			Monitor.Exit(doc);
			lock(this) {
				_msg = msg;
				SendMessageCore(Service.InvalidDocumentOperation);
			}
			Monitor.Enter(doc);
		}
		public void RefreshConnection(ConnectionTag tag) {
			TerminalDocument doc = tag.Document;
			Monitor.Exit(doc);
			lock(this) {
				_connection = tag.Connection;
				SendMessageCore(Service.RefreshConnection);
			}
			Monitor.Enter(doc);
		}


		protected void SendMessageCore(Service svc) {
			Win32.SendMessage(_mainFrameHandle, GConst.WMG_UIMESSAGE, new IntPtr((int)svc), IntPtr.Zero);
		}

		internal void AdjustIMEComposition(IntPtr hWnd, TerminalDocument doc) {
			Monitor.Exit(doc);
			lock(this) {
				Win32.SendMessage(hWnd, GConst.WMG_MAINTHREADTASK, new IntPtr(GConst.WPG_ADJUSTIMECOMPOSITION), IntPtr.Zero);
			}
			Monitor.Enter(doc);
		}
		internal void ToggleTextSelectionMode(IntPtr hWnd, TerminalDocument doc) {
			Monitor.Exit(doc);
			lock(this) {
				Win32.SendMessage(hWnd, GConst.WMG_MAINTHREADTASK, new IntPtr(GConst.WPG_TOGGLESELECTIONMODE), IntPtr.Zero);
			}
			Monitor.Enter(doc);
		}


		//メインスレッドから実行
		public virtual void ProcessMessage(IntPtr wParam, IntPtr lParam) {
			switch((Service)(int)wParam) {
				case Service.Warning:
					GUtil.Warning(GEnv.Frame, _msg);
					break;
				case Service.Information:
					GUtil.Warning(GEnv.Frame, _msg, MessageBoxIcon.Information);
					break;
				case Service.AskUserYesNo:
					_dialogResult = GUtil.AskUserYesNo(GEnv.Frame, _msg);
					break;
				case Service.DisconnectedFromServer:
					DisconnectedFromServer();
					break;
				case Service.IndicateBell:
					GEnv.Frame.IndicateBell();
					break;
				case Service.CriticalError:
					GUtil.ReportCriticalError(_exception);
					break;
				case Service.InvalidCharDetected: //いまはこの３つはメッセージテキストでしか区別しない
				case Service.UnsupportedCharSetDetected:
				case Service.UnsupportedEscapeSequence:
				case Service.InvalidDocumentOperation:
					BadCharDetected();
					break;
				case Service.RefreshConnection:
					GEnv.Frame.RefreshConnection(GEnv.Connections.FindTag(_connection));
					break;
			}
		}

		private void DisconnectedFromServer() {
			/*
            if(!_connection.IsClosed && (_connection.Param is TCPTerminalParam)) {
				string host = ((TCPTerminalParam)_connection.Param).Host;
				string msg = String.Format(GEnv.Strings.GetString("Message.InterThread.Disconnected"), host);
				if(GEnv.Options.DisconnectNotification==DisconnectNotification.MessageBox)
					GUtil.Warning(GEnv.Frame, msg);
				else if(GEnv.Options.DisconnectNotification==DisconnectNotification.StatusBar)
					GEnv.Frame.SetStatusBarText(msg);
			}
             * */
			_connection.Close();
            _connection.WriteChars("disconnected".ToCharArray());

            /*
			 * GEnv.Connections.FindTag(_connection).IsTerminated = true;

			if(GEnv.Options.CloseOnDisconnect)
				GEnv.GetConnectionCommandTarget(_connection).Close();
			else	
				GEnv.Frame.RefreshConnection(GEnv.Connections.FindTag(_connection));

			if(GEnv.Options.QuitAppWithLastPane && GEnv.Connections.Count==0)
				GEnv.Frame.AsForm().Close();
             */
            
		}

		private void BadCharDetected() {
			switch(GEnv.Options.WarningOption) {
				case WarningOption.StatusBar:
					GEnv.Frame.SetStatusBarText(_msg);
					break;
				case WarningOption.MessageBox: {
					WarningWithDisableOption dlg = new WarningWithDisableOption(_msg);
					if(GUtil.ShowModalDialog(GEnv.Frame, dlg)==DialogResult.OK && dlg.CheckedDisableOption)
						GEnv.Options.WarningOption = WarningOption.Ignore;
					break;
				}
			}
		}
	}
}
