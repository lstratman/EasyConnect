/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ContainerCommandTarget.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.LocalShell;
using Poderosa.Debugging;
using Poderosa.Terminal;
using Poderosa.Communication;
using Poderosa.Text;
using Poderosa.Config;
using Poderosa.Forms;
using Poderosa.Toolkit;


namespace Poderosa
{
	/// <summary>
	/// アプリケーション全体に関わる各種コマンドの実行を行う。ツールバー、メニュー、キーボードショートカットは
	/// 最終的にここを呼ぶことになる。
	/// </summary>
	internal class ContainerGlobalCommandTarget : GlobalCommandTarget
    {
        #region poderosa stuff
        private GFrame _frame;
		private bool _activationBlockFlag;

		public void Init(GFrame f) {
			_frame = f;
		}

		public CommandResult NewConnectionWithDialog(TCPTerminalParam param) {
			if(!CheckPaneCount()) return CommandResult.Denied;

			ConnectionHistory hst = GApp.ConnectionHistory;
			LoginDialog dlg = new LoginDialog();
			if(param!=null)
				dlg.ApplyParam(param);
			else
				dlg.ApplyParam(hst.TopTCPParam);

			if(GCUtil.ShowModalDialog(_frame, dlg)==DialogResult.OK) { //ダイアログを出さないなら無条件で接続を開く
				AddNewTerminal(dlg.Result);
				return CommandResult.Success;
			}
			else
				return CommandResult.Cancelled;
		}

		public CommandResult NewConnection(TerminalParam p) {
			if(!CheckPaneCount()) return CommandResult.Denied;

			ConnectionTag con = null;
			if(p is TCPTerminalParam) {
				TCPTerminalParam param = (TCPTerminalParam)p;
				if(param.IsSSH) {
					SSHShortcutLoginDialog dlg = new SSHShortcutLoginDialog((SSHTerminalParam)param);
					if(GCUtil.ShowModalDialog(_frame, dlg)==DialogResult.OK)
						con = dlg.Result;
				}
				else
					con = CommunicationUtil.CreateNewConnection(param);
			}
			else if(p is SerialTerminalParam) {
				SerialTerminalParam param = (SerialTerminalParam)p;
				con = CommunicationUtil.CreateNewSerialConnection(_frame, param);
			}
			else if(p is LocalShellTerminalParam) {
				LocalShellTerminalParam param = (LocalShellTerminalParam)p;
				con = CommunicationUtil.CreateNewLocalShellConnection(_frame, param);
			}

			if(con!=null) {
				AddNewTerminal(con);
				return CommandResult.Success;
			}
			else
				return CommandResult.Cancelled;
		}
		public CommandResult NewSerialConnectionWithDialog(SerialTerminalParam param) {
			if(!CheckPaneCount()) return CommandResult.Denied;

			SerialLoginDialog dlg = new SerialLoginDialog();
			if(param!=null)
				dlg.ApplyParam(param);
			else
				dlg.ApplyParam(GApp.ConnectionHistory.TopSerialParam);

			if(GCUtil.ShowModalDialog(_frame, dlg)==DialogResult.OK) {
				ConnectionTag con = dlg.Result;
				if(con!=null) {
					AddNewTerminal(con);
					return CommandResult.Success;
				}
			}

			return CommandResult.Cancelled;

			
			//XModemReceiver xmodem = new XModemReceiver(GEnv.Connections.ActiveTag, "C:\\IOPort\\xmodemresult.txt");
			//XModemSender xmodem = new XModemSender(GEnv.Connections.ActiveTag, "C:\\IOPort\\xslt.cs");
			//xmodem.Start();

			//return CommandResult.Success;
		}
		public CommandResult NewCygwinConnectionWithDialog(CygwinTerminalParam param) {
			if(!CheckPaneCount()) return CommandResult.Denied;

			if(CygwinUtil.GuessRootDirectory().Length==0) return CommandResult.Failed;

			LocalShellLoginDialog dlg = new LocalShellLoginDialog();
			if(param!=null)
				dlg.ApplyParam(param);
			else
				dlg.ApplyParam(GApp.ConnectionHistory.TopCygwinParam);

			CommandResult res = CommandResult.Cancelled;
			if(GCUtil.ShowModalDialog(_frame, dlg)==DialogResult.OK) {
				ConnectionTag con = dlg.Result;
				if(con!=null) {
					AddNewTerminal(con);
					res = CommandResult.Success;
				}
			}
			dlg.Dispose();

			return res;
		}
		public CommandResult NewSFUConnectionWithDialog(SFUTerminalParam param) {
			if(!CheckPaneCount()) return CommandResult.Denied;

			if(SFUUtil.GuessRootDirectory().Length==0) return CommandResult.Failed;

			LocalShellLoginDialog dlg = new LocalShellLoginDialog();
			if(param!=null)
				dlg.ApplyParam(param);
			else
				dlg.ApplyParam(GApp.ConnectionHistory.TopSFUParam);

			if(GCUtil.ShowModalDialog(_frame, dlg)==DialogResult.OK) {
				ConnectionTag con = dlg.Result;
				if(con!=null) {
					AddNewTerminal(con);
					return CommandResult.Success;
				}
			}

			return CommandResult.Cancelled;
		}
		public ConnectionTag SilentNewConnection(TerminalParam p) {
			if(!CheckPaneCount()) return null;

			ConnectionTag con = null;
			if(p is SSHTerminalParam) {
				SSHTerminalParam tp = (SSHTerminalParam)p;
				con = CommunicationUtil.CreateNewConnection(tp, null);
			}
			else if(p is TelnetTerminalParam) {
				TelnetTerminalParam tp = (TelnetTerminalParam)p;
				con = CommunicationUtil.CreateNewConnection(tp);
			}
			else if(p is SerialTerminalParam) {
				SerialTerminalParam tp = (SerialTerminalParam)p;
				con = CommunicationUtil.CreateNewSerialConnection(_frame, tp);
			}
			else if(p is LocalShellTerminalParam) {
				LocalShellTerminalParam tp = (LocalShellTerminalParam)p;
				con = CommunicationUtil.CreateNewLocalShellConnection(_frame, tp);
			}

			if(con!=null)
            {
				AddNewTerminal(con);

            }
			return con;
		}

		//TerminalConnection
		protected override void AddNewTerminalInternal(ConnectionTag con) {
			GApp.ConnectionHistory.Update(con.Connection.Param);
			_frame.AddConnection(con);
			_frame.AdjustMRUMenu();

			ActivateConnection(con.Connection);
			_frame.RefreshConnection(con);
		}

		internal bool CheckPaneCount() {
			if(GEnv.Connections.Count>=32) {
				GUtil.Warning(_frame, GApp.Strings.GetString("Messages.ConnectionLimitExceeds"));
				return false;
			}
			else
				return true;
		}
		public CommandResult EmulateUsingLog() {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Emulation Using Log";
			dlg.Multiselect = false;
			if(GCUtil.ShowModalDialog(_frame, dlg)==DialogResult.OK) {
				//エミュレートによるテスト
				FakeConnection fc = new FakeConnection(TCPTerminalParam.Fake);
				//!!new ConnectionTag()はpublicにしたくない 
				AddNewTerminal(new ConnectionTag(fc));
				TestUtil.EmulateWithLog(dlg.FileName, GEnv.Connections.FindTag(fc));
				return CommandResult.Success;
			}

			return CommandResult.Cancelled;
		}

		public CommandResult OpenShortCutWithDialog() {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = GApp.Strings.GetString("Util.OpenShortcutFile");
			dlg.Multiselect = false;
			dlg.InitialDirectory = GApp.Options.DefaultFileDir;
			dlg.DefaultExt = "gts";
			dlg.AddExtension = true;
			dlg.Filter = "Terminal Shortcut(*.gts)|*.gts|All Files(*.*)|*.*";
			if(GCUtil.ShowModalDialog(_frame, dlg)==DialogResult.OK) {
				GApp.Options.DefaultFileDir = GUtil.FileToDir(dlg.FileName);
				return OpenShortCut(dlg.FileName);
			}

			return CommandResult.Cancelled;
		}

		public CommandResult OpenShortCut(string filename) {
			try {
				if(GApp.Frame.WindowState==FormWindowState.Minimized) GApp.Frame.WindowState = FormWindowState.Normal;
				ConfigNode cn = DOMNodeConverter.Read(XMLUtil.FileToDOM(filename).DocumentElement);
				TerminalParam param = TerminalParam.CreateFromConfigNode(cn);
				param.FeedLogOption();
				NewConnection(param);
				return CommandResult.Success;
			}
			catch(Exception ex) {
				GUtil.Warning(_frame, ex.Message);
				return CommandResult.Failed;
			}
		}
		public ConnectionTag SilentOpenShortCut(string filename) {
			XmlReader r = null;
			try {
				if(GApp.Frame.WindowState==FormWindowState.Minimized) GApp.Frame.WindowState = FormWindowState.Normal;
				ConfigNode cn = DOMNodeConverter.Read(XMLUtil.FileToDOM(filename).DocumentElement);
				TerminalParam param = TerminalParam.CreateFromConfigNode(cn);
				param.FeedLogOption();
				return SilentNewConnection(param);
			}
			catch(Exception ex) {
				GUtil.Warning(_frame, ex.Message);
			}
			finally {
				if(r!=null) r.Close();
			}
			return null;
		}

		public CommandResult MoveToPrevPane() {
			Connections cc = GEnv.Connections;
			TerminalConnection a = cc.ActiveConnection;
			if(a==null) return CommandResult.Ignored;
			ActivateConnection(cc.PrevConnection(cc.FindTag(a)).Connection);
			return CommandResult.Success;
		}
		public CommandResult MoveToNextPane() {
			Connections cc = GEnv.Connections;
			TerminalConnection a = cc.ActiveConnection;
			if(a==null) return CommandResult.Ignored;
			ActivateConnection(cc.NextConnection(cc.FindTag(a)).Connection);
			return CommandResult.Success;
		}
		public CommandResult MoveTabToNext() {
			return MoveTab(1);
		}
		public CommandResult MoveTabToPrev() {
			return MoveTab(-1);
		}
		private CommandResult MoveTab(int d) {
			ConnectionTag ct = GEnv.Connections.ActiveTag;
			int index = GEnv.Connections.IndexOf(ct);
			int newindex = index + d;
			if(newindex>=0 && newindex < GEnv.Connections.Count) {
				GEnv.Connections.Reorder(index, newindex);
				GApp.Frame.ReorderWindowMenu(index, newindex, GEnv.Connections.ActiveTag);
				return CommandResult.Success;
			}
			else
				return CommandResult.Ignored;
		}


		public CommandResult ToggleFreeSelectionMode() {
			ConnectionTag ct = GEnv.Connections.ActiveTag;
			if(ct==null) return CommandResult.Ignored;
			IPoderosaTerminalPane p = ct.AttachedPane;
			if(p==null) return CommandResult.Failed;
			p.ToggleFreeSelectionMode();
			return CommandResult.Success;
		}
		public CommandResult ToggleAutoSelectionMode() {
			ConnectionTag ct = GEnv.Connections.ActiveTag;
			if(ct==null) return CommandResult.Ignored;
			IPoderosaTerminalPane p = ct.AttachedPane;
			if(p==null) return CommandResult.Failed;
			p.ToggleAutoSelectionMode();
			return CommandResult.Success;
		}

		public CommandResult CloseAll() {
			if(GApp.Options.AskCloseOnExit && GEnv.Connections.LiveConnectionsExist)
				if(GUtil.AskUserYesNo(_frame, GApp.Strings.GetString("Message.AskExitEvenIfConnected"), MessageBoxIcon.Exclamation)==DialogResult.No) return CommandResult.Cancelled;

			GEnv.Connections.CloseAllConnections();
			_frame.RemoveAllConnections();
			_frame.AdjustTerminalUI(false, null);
			GEnv.Connections.Clear();
			return CommandResult.Success;
		}

		public CommandResult CloseAllDisconnected() {
			foreach(ConnectionTag ct in GEnv.Connections.GetSnapshot()) {
				if(ct.Connection.IsClosed) {
					GApp.GetConnectionCommandTarget(ct).Close();
				}
			}
			GApp.Frame.RefreshConnection(GEnv.Connections.ActiveTag);
			return CommandResult.Success;
		}

		public CommandResult MoveActivePane(Keys direction) {
			GEnv.TextSelection.Clear();
			_frame.PaneContainer.MoveActivePane(direction);
			return CommandResult.Success;
		}

		public CommandResult CopyToFile() {
			if(GEnv.TextSelection.IsEmpty) return CommandResult.Ignored;

			SaveFileDialog dlg = new SaveFileDialog();
			dlg.InitialDirectory = GApp.Options.DefaultFileDir;
			dlg.Title = GApp.Strings.GetString("Util.DestinationToSave");
			dlg.Filter = "All Files(*.*)|*.*";
			string selectedtext = GEnv.TextSelection.GetSelectedText(); //このファイル選択ダイアログを閉じた直後にOnGotFocusが発生し、テキスト選択がクリアされることがあるので事前に取得しておく
			if(GCUtil.ShowModalDialog(_frame, dlg)==DialogResult.OK) {
				GApp.Options.DefaultFileDir = GUtil.FileToDir(dlg.FileName);
				try {
					StreamWriter wr = new StreamWriter(new FileStream(dlg.FileName, FileMode.Create), Encoding.Default);
					wr.Write(selectedtext);
					wr.Close();
					return CommandResult.Success;
				}
				catch(Exception ex) {
					GUtil.Warning(GApp.Frame, ex.Message);
					return CommandResult.Failed;
				}
			}
			else
				return CommandResult.Cancelled;
		}

		public CommandResult QuitApp() {
			CloseAll();
			_frame.Close();
			return CommandResult.Success;
		}
		
		//Activateの処理中は、GEnc.Coonectionsから取れるActiveConnectionは古いやつである。
		public CommandResult ActivateConnection(TerminalConnection con) {
			if(con==GEnv.Connections.ActiveConnection) {
				IPoderosaTerminalPane p = GEnv.Connections.FindTag(con).AttachedPane;
				if(p!=null && !p.AsControl().Focused) p.AsControl().Focus(); //!!アクティブなコネクションだからPaneがnullってことはないはずだが、そうなってしまうケースがあるようだ。真の原因は別にあるはずだが未解明
				return CommandResult.Success;
			}

			return ActivateConnection2(con==null? null : GEnv.Connections.FindTag(con));
		}

		//この中で多重にActivateされてくることがある
		internal CommandResult ActivateConnection2(ConnectionTag ct) {
			//Debug.WriteLine("Activating " + GEnv.Connections.IndexOf(ct));
			if(_activationBlockFlag || ct==null) return CommandResult.Ignored;

			_activationBlockFlag = true;
			GEnv.TextSelection.Clear();
			GEnv.Connections.BringToActivationOrderTop(ct);
			_frame.ActivateConnection(ct);
			_activationBlockFlag = false;
			return CommandResult.Success;
		}
		public CommandResult SetFocusToActiveConnection() {
			ConnectionTag tag = GEnv.Connections.ActiveTag;
			if(tag==null || tag.AttachedPane==null) return CommandResult.Ignored;
			//アクティブにしたときにクラッシュするのはこのFocus呼び出しが濃厚。ここはGFrame.OnActivatedから呼ばれている
			//tag.AttachedPane.AsControl().Focus(); //タイトルバーをクリックするなどするとペインにフォーカスがない
			return CommandResult.Success;
		}

		public CommandResult SetFrameStyle(GFrameStyle fs) {
			if(fs==GApp.Options.FrameStyle) return CommandResult.Ignored;
			GEnv.TextSelection.Clear();

			ContainerOptions opt = (ContainerOptions)GApp.Options.Clone();
			opt.FrameStyle = fs;
			GApp.UpdateOptions(opt);
			return CommandResult.Success;
		}
		public CommandResult ResetAllRenderProfiles(RenderProfile prof) {
			GEnv.DefaultRenderProfile = prof;
			foreach(ConnectionTag ct in GEnv.Connections) {
				ct.RenderProfile = null;
				if(ct.AttachedPane!=null) ct.AttachedPane.ApplyRenderProfile(prof);
			}
			return CommandResult.Success;
		}
		public CommandResult ExpandPane(bool expand) {
			GApp.Frame.PaneContainer.ResizeSplitterByFocusedPane(expand);
			return CommandResult.Success;
		}

		public CommandResult ShowOptionDialog() {
			OptionDialog dlg = new OptionDialog();
			GCUtil.ShowModalDialog(_frame, dlg);
			return CommandResult.Success;
		}
		public CommandResult ShowKeyGenWizard() {
			KeyGenWizard dlg = new KeyGenWizard();
			GCUtil.ShowModalDialog(_frame, dlg);
			return CommandResult.Success;
		}
		public CommandResult ShowChangePassphraseDialog() {
			ChangePassphrase dlg = new ChangePassphrase();
			GCUtil.ShowModalDialog(_frame, dlg);
			return CommandResult.Success;
		}

		public CommandResult ShowReceiveFileDialog() {
			if(_frame.XModemDialog!=null) {
				if(_frame.XModemDialog.Receiving)
					return CommandResult.Ignored;
				else {
					GUtil.Warning(GEnv.Frame, GApp.Strings.GetString("Message.NowFileSending"));
					return CommandResult.Failed;
				}
			}
			if(GEnv.Connections.ActiveTag==null) return CommandResult.Failed;

			XModemDialog dlg = new XModemDialog();
			dlg.Owner = _frame;
			dlg.Receiving = true;
			dlg.ConnectionTag = GEnv.Connections.ActiveTag;
			_frame.CenteringDialog(dlg);
			dlg.Show();
			_frame.XModemDialog = dlg;
			return CommandResult.Success;
		}
		public CommandResult ShowSendFileDialog() {
			if(_frame.XModemDialog!=null) {
				if(!_frame.XModemDialog.Receiving)
					return CommandResult.Ignored;
				else {
					GUtil.Warning(GEnv.Frame, GApp.Strings.GetString("Message.NowFileReceiving"));
					return CommandResult.Failed;
				}
			}
			if(GEnv.Connections.ActiveTag==null) return CommandResult.Failed;

			XModemDialog dlg = new XModemDialog();
			dlg.Owner = _frame;
			dlg.Receiving = false;
			dlg.ConnectionTag = GEnv.Connections.ActiveTag;
			_frame.CenteringDialog(dlg);
			dlg.Show();
			_frame.XModemDialog = dlg;
			return CommandResult.Success;
		}


		public CommandResult ShowMacroConfigDialog() {
			MacroList dlg = new MacroList();
			GCUtil.ShowModalDialog(_frame, dlg);
			return CommandResult.Success;
		}
		public CommandResult StopMacro() {
			if(GApp.MacroManager.MacroIsRunning) {
				GApp.MacroManager.StopMacro();
				return CommandResult.Success;
			}
			else
				return CommandResult.Ignored;
		}

		public CommandResult ShowAboutBox() {
			AboutBox dlg = new AboutBox();
			GCUtil.ShowModalDialog(_frame, dlg);
			if(dlg.CreditButtonClicked)
				GCUtil.ShowModalDialog(_frame, new Credits());
			return CommandResult.Success;
		}
		public CommandResult ShowWelcomeDialog() {
			WelcomeDialog dlg = new WelcomeDialog();
			DialogResult r = GCUtil.ShowModalDialog(_frame, dlg);
			if(r==DialogResult.OK) {
				if(dlg.CID!=CID.NOP) GApp.GlobalCommandTarget.Exec(dlg.CID);
			}
			return CommandResult.Success;
		}
		public CommandResult ShowProductWeb() {
			try {
				Process p = Process.Start(GCConst.PRODUCT_WEB);
				return p==null? CommandResult.Failed : CommandResult.Success;
			}
			catch(Exception) {
				return CommandResult.Failed;
			}
		}
		public CommandResult LaunchPortforwarding() {
			try {
				Process p = Process.Start(GApp.BaseDirectory + "\\portforwarding.exe");
				if(p==null) {
					GUtil.Warning(GApp.Frame, GApp.Strings.GetString("Message.PFLaunchFailed"));
					return CommandResult.Failed;
				}
				else
					return CommandResult.Success;
			}
			catch(Exception ex) {
				GUtil.Warning(GApp.Frame, GApp.Strings.GetString("Message.PFLaunchFailed")+"\n" + ex.Message);
				return CommandResult.Failed;
			}
		}


		public override CommandResult SetConnectionLocation(ConnectionTag ct, TerminalPane pane) {
			if(ct.AttachedPane==pane) return CommandResult.Ignored;
			_frame.PaneContainer.SetConnectionLocation(ct, pane);
			return CommandResult.Success;
		}

		public CommandResult Exec(Commands.Entry ent) {
			if(ent.Category==Commands.Category.Fixed) {
				int n = (int)(ent.CID - CID.ActivateConnection0);
				if(n < GEnv.Connections.Count)
					return ActivateConnection2(GEnv.Connections.TagAt(n));
				else
					return CommandResult.Ignored;
			}
			else if(ent.Category==Commands.Category.Macro)
				return ExecMacro(((Commands.MacroEntry)ent).Index);
			else
				return Exec(ent.CID);
		}
		public CommandResult ExecMacro(int index) {
			if(index < GApp.MacroManager.ModuleCount) {
				GApp.MacroManager.Execute(GApp.Frame, GApp.MacroManager.GetModule(index));
				return CommandResult.Success;
			}
			else
				return CommandResult.Ignored;
		}
        #endregion
        [DllImport("MSVCRT.DLL")]
        public static extern void time(ref long t);
		public CommandResult Exec(CID id) {
			switch(id) {
                #region Poderosa stuff
                case CID.PrevPane:
					return MoveToPrevPane();
				case CID.NextPane:
					return MoveToNextPane();
				case CID.MoveTabToPrev:
					return MoveTabToPrev();
				case CID.MoveTabToNext:
					return MoveTabToNext();
				case CID.MovePaneUp:
					return MoveActivePane(Keys.Up);
				case CID.MovePaneDown:
					return MoveActivePane(Keys.Down);
				case CID.MovePaneLeft:
					return MoveActivePane(Keys.Left);
				case CID.MovePaneRight:
					return MoveActivePane(Keys.Right);
				case CID.FrameStyleSingle:
					return SetFrameStyle(GFrameStyle.Single);
				case CID.FrameStyleDivHorizontal:
					return SetFrameStyle(GFrameStyle.DivHorizontal);
				case CID.FrameStyleDivVertical:
					return SetFrameStyle(GFrameStyle.DivVertical);
				case CID.FrameStyleDivHorizontal3:
					return SetFrameStyle(GFrameStyle.DivHorizontal3);
				case CID.FrameStyleDivVertical3:
					return SetFrameStyle(GFrameStyle.DivVertical3);
				case CID.NewConnection:
					return NewConnectionWithDialog(null);
				case CID.NewSerialConnection:
					return NewSerialConnectionWithDialog(null);
				case CID.NewCygwinConnection:
					return NewCygwinConnectionWithDialog(null);
				case CID.NewSFUConnection:
					return NewSFUConnectionWithDialog(null);
				case CID.OpenShortcut:
					return OpenShortCutWithDialog();
				case CID.Copy:
					return Copy();
				case CID.CopyAsLook:
					return CopyAsLook();
				case CID.CopyToFile:
					return CopyToFile();
				case CID.ToggleFreeSelectionMode:
					return ToggleFreeSelectionMode();
				case CID.ToggleAutoSelectionMode:
					return ToggleAutoSelectionMode();
				case CID.OptionDialog:
					return ShowOptionDialog();
				case CID.KeyGen:
					return ShowKeyGenWizard();
				case CID.ChangePassphrase:
					return ShowChangePassphraseDialog();
				case CID.Portforwarding:
					return LaunchPortforwarding();
				case CID.MacroConfig:
					return ShowMacroConfigDialog();
				case CID.StopMacro:
					return StopMacro();
				case CID.ReceiveFile:
					return ShowReceiveFileDialog();
				case CID.SendFile:
					return ShowSendFileDialog();
				case CID.AboutBox:
					return ShowAboutBox();
				case CID.ShowWelcomeDialog:
					return ShowWelcomeDialog();
				case CID.CloseAll:
					return CloseAll();
				case CID.CloseAllDisconnected:
					return CloseAllDisconnected();
				case CID.ExpandPane:
					return ExpandPane(true);
				case CID.ShrinkPane:
					return ExpandPane(false);
				case CID.Quit:
					return QuitApp();
#if DEBUG
				case CID.EmulateLog:
					return EmulateUsingLog();
#endif
				case CID.ProductWeb:
					return ShowProductWeb();
				case CID.NOP:
					return CommandResult.Ignored;
				default:
					int n = id - CID.ExecMacro;
					if(n>=0 && n<100)
						return ExecMacro(n);
					else {
						Debug.WriteLine("unknown connection command "+id);
						return CommandResult.Ignored;
                    }
                #endregion
            }
        }
        #region other stuff
        //Menu / ToolBarからのコマンドの回避策
		private CID _delayed_command_id;
		private string _delayed_string_data;
		public void DelayedExec(CID id) {
			_delayed_command_id = id;
			Win32.PostMessage(GApp.Frame.Handle, GConst.WMG_DELAYED_COMMAND, IntPtr.Zero, IntPtr.Zero);
		}
		public void DelayedOpenShortcut(string fn) {
			_delayed_command_id = CID.OpenShortcut;
			_delayed_string_data = fn;
			Win32.PostMessage(GApp.Frame.Handle, GConst.WMG_DELAYED_COMMAND, IntPtr.Zero, IntPtr.Zero);
		}
		public void DoDelayedExec() {
			if(_delayed_command_id==CID.OpenShortcut)
				OpenShortCut(_delayed_string_data);
			else
				Exec(_delayed_command_id);
        }
        #endregion
    }

	internal class ContainerConnectionCommandTarget : ConnectionCommandTarget {
        #region poderosa stuff
		public ContainerConnectionCommandTarget(TerminalConnection con) : base(con) {
		}

		public CommandResult Reproduce() {
			if(!GApp.GlobalCommandTarget.CheckPaneCount()) return CommandResult.Failed;
			try {
				ConnectionTag con = _connection.Reproduce();
				if(con==null) return CommandResult.Failed; //接続失敗時はこの中でメッセージが出ている
				if(_connection.IsClosed) { //再接続
					ConnectionTag old = GEnv.Connections.FindTag(_connection);
					con.Document.InsertBefore(old.Document, _connection.TerminalHeight);
					GEnv.Connections.Replace(old, con);
					con.ImportProperties(old);
					if(old.AttachedPane!=null) {
						old.AttachedPane.Attach(con);
					}
					con.Receiver.Listen();
					GApp.Frame.ReplaceConnection(old, con);
					GApp.Frame.RefreshConnection(con);
					GApp.GlobalCommandTarget.ActivateConnection(con.Connection);
				}
				else
					GApp.GlobalCommandTarget.AddNewTerminal(con);
				return CommandResult.Success;
			}
			catch(Exception ex) {
				GUtil.Warning(GApp.Frame, ex.Message);
				return CommandResult.Failed;
			}
		}
		public CommandResult SetTransmitNewLine(NewLine nl) {
			//Set Value
			_connection.Param.TransmitNL = nl;
			GFrame f = GApp.Frame;
			//ToolBar
			if(GApp.Options.ShowToolBar) {
				f.ToolBar.NewLineBox.SelectedIndex = (int)nl;
				f.ToolBar.Invalidate(true);
			}
			//Menu
			f.MenuNewLineCR.Checked   = nl==NewLine.CR;
			f.MenuNewLineLF.Checked   = nl==NewLine.LF;
			f.MenuNewLineCRLF.Checked = nl==NewLine.CRLF;
			return CommandResult.Success;
		}
		public CommandResult SetLocalEcho(bool enabled) {
			//Set Value
			_connection.Param.LocalEcho = enabled;
			GFrame f = GApp.Frame;
			//ToolBar
			if(GApp.Options.ShowToolBar) {
				f.ToolBar.LocalEchoButton.Checked = enabled;
				f.ToolBar.Invalidate(true);
			}
			//Menu
			f.MenuLocalEcho.Checked = enabled;
			return CommandResult.Success;
		}
		public CommandResult ShowLineFeedRuleDialog() {
			LineFeedStyleDialog dlg = new LineFeedStyleDialog(_connection);
			if(GCUtil.ShowModalDialog(GApp.Frame, dlg)==DialogResult.OK)
				_connection.Param.LineFeedRule = dlg.LineFeedRule;
			return CommandResult.Success;
		}
		public CommandResult SetLogSuspended(bool suspended) {
			//Set Value
			_connection.LogSuspended = suspended;
			GFrame f = GApp.Frame;
			//ToolBar
			if(GApp.Options.ShowToolBar) {
				f.ToolBar.SuspendLogButton.Checked = suspended;
				f.ToolBar.Invalidate(true);
			}
			//Menu
			f.MenuSuspendLog.Checked = suspended;
			return CommandResult.Success;
		}
		public CommandResult SetEncoding(EncodingProfile enc) {
			ConnectionTag ct = GEnv.Connections.FindTag(_connection);
			_connection.Param.EncodingProfile = enc;
			ct.Terminal.Reset();
			GApp.Frame.AdjustTerminalUI(true, ct);
			return CommandResult.Success;
		}

		public CommandResult EditRenderProfile() {
			ConnectionTag tag = GEnv.Connections.FindTag(_connection);
			EditRenderProfile dlg = new EditRenderProfile(tag.RenderProfile);
			if(GCUtil.ShowModalDialog(GApp.Frame, dlg)==DialogResult.OK) {
				tag.RenderProfile = dlg.Result;
				GApp.ConnectionHistory.ReplaceIdenticalParam(tag.Connection.Param);
				if(tag.AttachedPane!=null) tag.AttachedPane.ApplyRenderProfile(dlg.Result);
				GApp.Frame.AdjustMRUMenu();
				return CommandResult.Success;
			}
			else
				return CommandResult.Cancelled;
		}


		public CommandResult ChangeLogWithDialog() {
			ChangeLogDialog dlg = new ChangeLogDialog(_connection);
			if(GCUtil.ShowModalDialog(GApp.Frame, dlg)==DialogResult.OK) {
				GApp.Frame.AdjustTerminalUI(true, GEnv.Connections.FindTag(_connection));
				return CommandResult.Success;
			}
			else
				return CommandResult.Cancelled;
		}
		public CommandResult CommentLog() {
			LogNoteForm dlg = new LogNoteForm();
			if(GCUtil.ShowModalDialog(GApp.Frame, dlg)==DialogResult.OK) {
				_connection.CommentLog(dlg.ResultText);
				return CommandResult.Success;
			}
			else
				return CommandResult.Cancelled;
		}
		public CommandResult ShowServerInfo() {
			ServerInfo dlg = new ServerInfo(_connection);
			GCUtil.ShowModalDialog(GApp.Frame, dlg);
			return CommandResult.Success;
		}
		public CommandResult RenameTab() {
			try {
				InputBox box = new InputBox();
				box.AllowsZeroLenString = false;
				box.Text = GApp.Strings.GetString("Util.RenameTab");
				box.Content = _connection.Param.Caption;
				if(GCUtil.ShowModalDialog(GApp.Frame, box)==DialogResult.OK) {
					_connection.Param.Caption = box.Content;
					GApp.ConnectionHistory.ReplaceIdenticalParam(_connection.Param);
					GApp.Frame.AdjustMRUMenu();
					GApp.Frame.RefreshConnection(GEnv.Connections.FindTag(_connection));
					return CommandResult.Success;
				}
				else
					return CommandResult.Cancelled;
			}
			catch(Exception ex) {
				Debug.WriteLine(ex.StackTrace);
				GUtil.Warning(GApp.Frame, ex.Message);
				return CommandResult.Failed;
			}
		}
		public CommandResult SerialConfig() {
			SerialTerminalConnection stc = _connection as SerialTerminalConnection;
			if(stc==null) return CommandResult.Failed;

			try {
				SerialConfigForm f = new SerialConfigForm();
				f.ApplyParam(stc);
				if(GCUtil.ShowModalDialog(GApp.Frame, f)==DialogResult.OK)
					return CommandResult.Success;
				else
					return CommandResult.Cancelled;
			}
			catch(NotSupportedException) {
				GUtil.Warning(GApp.Frame, GApp.Strings.GetString("Message.SerialConfigIsSerialOnly"));
				return CommandResult.Failed;
			}
		}
		public CommandResult SaveShortCut() {
			//コネクションがあればファイルを尋ねる
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Title = GApp.Strings.GetString("Util.SaveShortcut");
			dlg.InitialDirectory = GApp.Options.DefaultFileDir;
			dlg.DefaultExt = "gts";
			dlg.AddExtension = true;
			dlg.Filter = "Terminal Shortcut(*.gts)|*.gts";
			if(GCUtil.ShowModalDialog(GApp.Frame, dlg)==DialogResult.OK) {
				try {
					ConfigNode cn = new ConfigNode("poderosa-shortcut");
					_connection.Param.Export(cn);
					XmlWriter wr = XMLUtil.CreateDefaultWriter(dlg.FileName);
					DOMNodeConverter.Write(wr, cn);
					wr.WriteEndDocument();
					wr.Close();
					GApp.Options.DefaultFileDir = GUtil.FileToDir(dlg.FileName);
					return CommandResult.Success;
				}
				catch(Exception ex) {
					MessageBox.Show(GEnv.Frame, ex.Message, GApp.Strings.GetString("Message.SaveError"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return CommandResult.Failed;
				}
			}
			else
				return CommandResult.Cancelled;
		}
		public override CommandResult Paste() {
			string value = (string)Clipboard.GetDataObject().GetData("Text");
			if(value==null || value.Length==0) return CommandResult.Ignored;
			
			ConnectionTag ct = GEnv.Connections.FindTag(_connection);
			if(ct.ModalTerminalTask!=null) return CommandResult.Denied;

			if(value.Length>0x1000) {
				SendingLargeText dlg = new SendingLargeText(new PasteProcessor(ct, value));
				if(GCUtil.ShowModalDialog(GApp.Frame, dlg)==DialogResult.OK)
					return CommandResult.Success;
				else
					return CommandResult.Cancelled;
			}
			else
				return PasteMain(value);
		}

		public CommandResult PasteFromFile() {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.InitialDirectory = GApp.Options.DefaultFileDir;
			dlg.Title = GApp.Strings.GetString("Util.SourceToLoad");
			dlg.Filter = "All Files(*.*)|*.*";
			ConnectionTag ct = GEnv.Connections.FindTag(_connection);
			if(ct.ModalTerminalTask!=null) return CommandResult.Denied;

			if(GCUtil.ShowModalDialog(GApp.Frame, dlg)==DialogResult.OK) {
				GApp.Options.DefaultFileDir = GUtil.FileToDir(dlg.FileName);
				try {
					StreamReader re = new StreamReader(new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read), _connection.Param.EncodingProfile.Encoding);
					if(new FileInfo(dlg.FileName).Length > 0x1000) { //4KB以上は非同期
						SendingLargeText dlg2 = new SendingLargeText(new PasteProcessor(ct, re));
						re.Close();
						if(GCUtil.ShowModalDialog(GApp.Frame, dlg2)==DialogResult.OK)
							return CommandResult.Success;
						else
							return CommandResult.Cancelled;
					}
					else {
						PasteProcessor p = new PasteProcessor(ct, re);
						p.Perform();
						re.Close();
					}
					//GEnv.Frame.StatusBar.IndicateSendData();
					return CommandResult.Success;
				}
				catch(Exception ex) {
					GUtil.Warning(GEnv.Frame, ex.Message);
					return CommandResult.Failed;
				}
			}
			else
				return CommandResult.Cancelled;
		}
		public CommandResult Exec(CID id) {
			switch(id) {
				case CID.Close:
					return Close();
				case CID.Reproduce:
					return Reproduce();
				case CID.SaveShortcut:
					return SaveShortCut();
				case CID.ShowServerInfo:
					return ShowServerInfo();
				case CID.RenameTab:
					return RenameTab();
				case CID.Paste:
					return Paste();
				case CID.PasteFromFile:
					return PasteFromFile();
				case CID.ClearScreen:
					return ClearScreen();
				case CID.ClearBuffer:
					return ClearBuffer();
				case CID.SelectAll:
					return SelectAll();
				case CID.ToggleNewLine:
					return SetTransmitNewLine(TerminalUtil.NextNewLineOption(_connection.Param.TransmitNL));
				case CID.ToggleLocalEcho:
					return SetLocalEcho(!_connection.Param.LocalEcho);
				case CID.LineFeedRule:
					return ShowLineFeedRuleDialog();
				case CID.ToggleLogSuspension:
					return SetLogSuspended(!_connection.LogSuspended);
				case CID.EditRenderProfile:
					return EditRenderProfile();
				case CID.ChangeLogFile:
					return ChangeLogWithDialog();
				case CID.CommentLog:
					return CommentLog();
				case CID.SendBreak:
					return SendBreak();
				case CID.AreYouThere:
					return AreYouThere();
				case CID.SerialConfig:
					return SerialConfig();
				case CID.ResetTerminal:
					return ResetTerminal();
#if DEBUG
				case CID.DumpText:
					return DumpText();
#endif
				case CID.NOP:
					return CommandResult.Ignored;
				default:
					Debug.WriteLine("unknown connection command "+id);
					return CommandResult.Ignored;
			}
        }
        #endregion
    }
}
