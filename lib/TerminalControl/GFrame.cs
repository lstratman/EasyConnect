/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: GFrame.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Xml;

using Poderosa.UI;
using Poderosa.Terminal;
using Poderosa.Communication;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Config;
using Poderosa.Text;
using Poderosa.MacroEnv;
using Poderosa.Toolkit;

namespace Poderosa.Forms
{

	internal class GFrame : Form
    {
        #region Poderosa Fields
        private Hashtable _windowMenuItemMap;
		private Hashtable _MRUMenuToParameter;
		private InitialAction _initialAction;
		internal bool _firstflag;
		public MultiPaneControl _multiPaneControl;
		private GStatusBar _gStatusBar;
		private XModemDialog _xmodemDialog;

		private TerminalConnection _commandTargetConnection;
		private ContextMenu _contextMenu;

		private System.Windows.Forms.MainMenu _menu;
		private GDialogBar _toolBar;
		private Poderosa.Forms.TabBar _tabBar;
		private System.Windows.Forms.StatusBar _statusBar;
		private System.Windows.Forms.StatusBarPanel _textStatusBarPanel;
		private System.Windows.Forms.StatusBarPanel _bellIndicateStatusBarPanel;
		private System.Windows.Forms.StatusBarPanel _caretPosStatusBarPanel;
		//!!ならびかえ
		private GMenuItem _menuBarFile1;
		private GMenuItem _menuBarFile2;
		private GMenuItem _menuReceiveFile;
		private GMenuItem _menuSendFile;
		private GMenuItem _menuBarBeforeMRU;
		private GMenuItem _menuBarAfterMRU;
		private GMenuItem _menuQuit;
		private GMenuItem _menuBarConsole2;
		private GMainMenuItem _menuFile;
		private GMenuItem _menuOpenShortcut;
		private GMenuItem _menuNewConnection;
		private GMenuItem _menuCygwinNewConnection;
		private GMenuItem _menuSFUNewConnection;
		private GMainMenuItem _menuTool;
		private GMenuItem _menuSaveShortcut;
		private GMenuItem _menuKeyGen;
		private GMenuItem _menuChangePassphrase;
		private GMenuItem _menuBarTool1;
		private GMenuItem _menuMacro;
		private GMenuItem _menuBarTool2;
		private GMenuItem _menuOption;
		private GMenuItem _menuMacroConfig;
		private GMenuItem _menuStopMacro;
		private GMenuItem _menuBarMacro;
		private GMainMenuItem _menuConsole;
		private GMenuItem _menuNewLine;
		private GMenuItem _menuNewLine_CR;
		private GMenuItem _menuNewLine_LF;
		private GMenuItem _menuNewLine_CRLF;
		private GMenuItem _menuLineFeedRule;
		private GMenuItem _menuLocalEcho;
		private GMenuItem _menuSendSpecial;
		private GMenuItem _menuSendBreak;
		private GMenuItem _menuAreYouThere;
		private GMenuItem _menuSerialConfig;
		private GMenuItem _menuResetTerminal;
		private GMenuItem _menuEncoding;
		private GMenuItem _menuSuspendLog;
		private GMenuItem _menuCommentLog;
		private GMenuItem _menuServerInfo;
		private GMainMenuItem _menuWindow;
		private GMainMenuItem _menuHelp;
		private GMenuItem _menuAboutBox;
		private GMenuItem _menuProductWeb;
		private GMenuItem _menuChangeLog;
		private GMenuItem _menuBarConsole3;
		private GMenuItem _menuConsoleClose;
		private GMenuItem _menuConsoleReproduce;
		private GMenuItem _menuRenameTab;
		private GMenuItem _menuBarConsole1;
		private GMenuItem _menuPane;
		private GMenuItem _menuTab;
		private GMenuItem _menuMovePane;
		private GMenuItem _menuBarWindow3;
		private GMenuItem _menuMovePaneUp;
		private GMenuItem _menuMovePaneDown;
		private GMenuItem _menuMovePaneLeft;
		private GMenuItem _menuMovePaneRight;
		private GMenuItem _menuCloseAll;
		private GMenuItem _menuCloseAllDisconnected;
		private GMenuItem _menuBarWindow1;
		private GMenuItem _menuPrevTab;
		private GMenuItem _menuNextTab;
		private GMenuItem _menuMoveTabToPrev;
		private GMenuItem _menuMoveTabToNext;
		private GMenuItem _menuExpandPane;
		private GMenuItem _menuShrinkPane;
		private GMenuItem _menuSerialNewConnection;
		private GMainMenuItem _menuEdit;
		private GMenuItem _menuCopy;
		private GMenuItem _menuPaste;
		private GMenuItem _menuCopyToFile;
		private GMenuItem _menuPasteFromFile;
		private GMenuItem _menuSelectAll;
		private GMenuItem _menuBarEdit1;
		private GMenuItem _menuBarEdit2;
		private GMenuItem _menuBarEdit3;
		private GMenuItem _menuClearScreen;
		private GMenuItem _menuClearBuffer;
		private GMenuItem _menuFreeSelectionMode;
		private GMenuItem _menuAutoSelectionMode;
		private GMenuItem _menuBarWindow2;
		private GMenuItem _menuFrameStyle;
		private GMenuItem _menuFrameStyleSingle;
		private GMenuItem _menuFrameStyleDivHorizontal;
		private GMenuItem _menuFrameStyleDivVertical;
		private GMenuItem _menuFrameStyleDivHorizontal3;
		private GMenuItem _menuFrameStyleDivVertical3;
		private GMenuItem _menuEditRenderProfile;
		private GMenuItem _menuCopyAsLook;
		private GMenuItem _menuLaunchPortforwarding;
        #endregion
		public GFrame(InitialAction act)
        {
            #region Poderosa Constructors
            _initialAction = act;
			_windowMenuItemMap = new Hashtable();
			_MRUMenuToParameter = new Hashtable();
			_firstflag=true;
			//
			// Windows フォーム デザイナ サポートに必要です。
			//

			SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.UserPaint|ControlStyles.DoubleBuffer, true);
			InitializeComponent();
			this.Icon = GApp.Options.GuevaraMode? GIcons.GetOldGuevaraIcon() : GIcons.GetAppIcon();
			InitMenuText();

			//システムからエンコーディングを列挙してメニューをセット
			foreach(string e in EnumDescAttribute.For(typeof(EncodingType)).DescriptionCollection()) {
				GMenuItem m = new GMenuItem();
				m.Text = e;
				m.Click += new EventHandler(this.OnChangeEncoding);
				_menuEncoding.MenuItems.Add(m);
			}

			this._tabBar = new TabBar();
			_tabBar.Dock = DockStyle.Top;
			_tabBar.Height = 25;

			ApplyOptions(null, GApp.Options);
			ApplyHotKeys();

			_gStatusBar = new GStatusBar(_statusBar);
			AdjustTitle(null);
#endregion

        }
        #region
        public void ReloadIcon() {
			this.Icon = GApp.Options.GuevaraMode? GIcons.GetOldGuevaraIcon() : GIcons.GetAppIcon();
		}
		protected override void Dispose( bool disposing ) {
			base.Dispose( disposing );
        }
        #endregion
        #region Windows Form Designer generated code
        /// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GFrame));
			this._menu = new System.Windows.Forms.MainMenu();
			this._menuFile = new GMainMenuItem();
			this._menuNewConnection = new GMenuItem();
			this._menuSerialNewConnection = new GMenuItem();
			this._menuCygwinNewConnection = new GMenuItem();
			this._menuSFUNewConnection = new GMenuItem();
			this._menuBarFile1 = new GMenuItem();
			this._menuOpenShortcut = new GMenuItem();
			this._menuSaveShortcut = new GMenuItem();
			this._menuBarFile2 = new GMenuItem();
			this._menuReceiveFile = new GMenuItem();
			this._menuSendFile = new GMenuItem();
			this._menuBarBeforeMRU = new GMenuItem();
			this._menuBarAfterMRU = new GMenuItem();
			this._menuQuit = new GMenuItem();
			this._menuEdit = new GMainMenuItem();
			this._menuCopy = new GMenuItem();
			this._menuPaste = new GMenuItem();
			this._menuBarEdit1 = new GMenuItem();
			this._menuCopyAsLook = new GMenuItem();
			this._menuCopyToFile = new GMenuItem();
			this._menuPasteFromFile = new GMenuItem();
			this._menuBarEdit2 = new GMenuItem();
			this._menuClearScreen = new GMenuItem();
			this._menuClearBuffer = new GMenuItem();
			this._menuBarEdit3 = new GMenuItem();
			this._menuSelectAll = new GMenuItem();
			this._menuFreeSelectionMode = new GMenuItem();
			this._menuAutoSelectionMode = new GMenuItem();
			this._menuConsole = new GMainMenuItem();
			this._menuConsoleClose = new GMenuItem();
			this._menuRenameTab = new GMenuItem();
			this._menuConsoleReproduce = new GMenuItem();
			this._menuBarConsole1 = new GMenuItem();
			this._menuNewLine = new GMenuItem();
			this._menuNewLine_CR = new GMenuItem();
			this._menuNewLine_LF = new GMenuItem();
			this._menuNewLine_CRLF = new GMenuItem();
			this._menuLocalEcho = new GMenuItem();
			this._menuLineFeedRule = new GMenuItem();
			this._menuSendSpecial = new GMenuItem();
			this._menuSendBreak = new GMenuItem();
			this._menuAreYouThere = new GMenuItem();
			this._menuSerialConfig = new GMenuItem();
			this._menuResetTerminal = new GMenuItem();
			this._menuBarConsole2 = new GMenuItem();
			this._menuSuspendLog = new GMenuItem();
			this._menuCommentLog = new GMenuItem();
			this._menuChangeLog = new GMenuItem();
			this._menuBarConsole3 = new GMenuItem();
			this._menuServerInfo = new GMenuItem();
			this._menuTool = new GMainMenuItem();
			this._menuKeyGen = new GMenuItem();
			this._menuChangePassphrase = new GMenuItem();
			this._menuBarTool1 = new GMenuItem();
			this._menuMacro = new GMenuItem();
			this._menuBarTool2 = new GMenuItem();
			this._menuOption = new GMenuItem();
			this._menuMacroConfig = new GMenuItem();
			this._menuStopMacro = new GMenuItem();
			this._menuBarMacro = new GMenuItem();
			this._menuWindow = new GMainMenuItem();
			this._menuFrameStyle = new GMenuItem();
			this._menuFrameStyleSingle = new GMenuItem();
			this._menuFrameStyleDivHorizontal = new GMenuItem();
			this._menuFrameStyleDivVertical = new GMenuItem();
			this._menuFrameStyleDivHorizontal3 = new GMenuItem();
			this._menuFrameStyleDivVertical3 = new GMenuItem();
			this._menuPane = new GMenuItem();
			this._menuMovePane = new GMenuItem();
			this._menuMovePaneUp = new GMenuItem();
			this._menuMovePaneDown = new GMenuItem();
			this._menuMovePaneLeft = new GMenuItem();
			this._menuMovePaneRight = new GMenuItem();
			this._menuExpandPane = new GMenuItem();
			this._menuShrinkPane = new GMenuItem();
			this._menuBarWindow1 = new GMenuItem();
			this._menuTab = new GMenuItem();
			this._menuPrevTab = new GMenuItem();
			this._menuNextTab = new GMenuItem();
			this._menuMoveTabToPrev = new GMenuItem();
			this._menuMoveTabToNext = new GMenuItem();
			this._menuBarWindow2 = new GMenuItem();
			this._menuCloseAll = new GMenuItem();
			this._menuCloseAllDisconnected = new GMenuItem();
			this._menuBarWindow3 = new GMenuItem();
			this._menuHelp = new GMainMenuItem();
			this._menuAboutBox = new GMenuItem();
			this._menuProductWeb = new GMenuItem();
			this._statusBar = new System.Windows.Forms.StatusBar();
			this._textStatusBarPanel = new System.Windows.Forms.StatusBarPanel();
			this._bellIndicateStatusBarPanel = new System.Windows.Forms.StatusBarPanel();
			this._caretPosStatusBarPanel = new System.Windows.Forms.StatusBarPanel();
			this._menuEncoding = new GMenuItem();
			this._menuEditRenderProfile = new GMenuItem();
			this._menuLaunchPortforwarding = new GMenuItem();
			((System.ComponentModel.ISupportInitialize)(this._textStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._bellIndicateStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._caretPosStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// _menu
			// 
			this._menu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				  this._menuFile,
																				  this._menuEdit,
																				  this._menuConsole,
																				  this._menuTool,
																				  this._menuWindow,
																				  this._menuHelp});
			// 
			// _menuFile
			// 
			this._menuFile.Index = 0;
			this._menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this._menuNewConnection,
																					  this._menuSerialNewConnection,
																					  this._menuCygwinNewConnection,
																					  this._menuSFUNewConnection,
																					  this._menuBarFile1,
																					  this._menuOpenShortcut,
																					  this._menuSaveShortcut,
																					  this._menuBarFile2,
																					  this._menuReceiveFile,
																					  this._menuSendFile,
																					  this._menuBarBeforeMRU,
																					  this._menuBarAfterMRU,
																					  this._menuQuit});
			// 
			// _menuNewConnection
			// 
			this._menuNewConnection.Index = 0;
			this._menuNewConnection.Click += new System.EventHandler(this.OnMenu);
			this._menuNewConnection.CID = (int)CID.NewConnection;
			// 
			// _menuSerialNewConnection
			// 
			this._menuSerialNewConnection.Index = 1;
			this._menuSerialNewConnection.Click += new System.EventHandler(this.OnMenu);
			this._menuSerialNewConnection.CID = (int)CID.NewSerialConnection;
			// 
			// _menuCygwinNewConnection
			// 
			this._menuCygwinNewConnection.Index = 2;
			this._menuCygwinNewConnection.Click += new System.EventHandler(this.OnMenu);
			this._menuCygwinNewConnection.CID = (int)CID.NewCygwinConnection;
			// 
			// _menuSFUNewConnection
			// 
			this._menuSFUNewConnection.Index = 3;
			this._menuSFUNewConnection.Click += new System.EventHandler(this.OnMenu);
			this._menuSFUNewConnection.CID = (int)CID.NewSFUConnection;
			// 
			// _menuBarFile1
			// 
			this._menuBarFile1.Index = 4;
			this._menuBarFile1.Text = "-";
			// 
			// _menuOpenShortcut
			// 
			this._menuOpenShortcut.Index = 5;
			this._menuOpenShortcut.Click += new System.EventHandler(this.OnMenu);
			this._menuOpenShortcut.CID = (int)CID.OpenShortcut;
			// 
			// _menuSaveShortcut
			// 
			this._menuSaveShortcut.Enabled = false;
			this._menuSaveShortcut.Index = 6;
			this._menuSaveShortcut.Click += new System.EventHandler(this.OnMenu);
			this._menuSaveShortcut.CID = (int)CID.SaveShortcut;
			// 
			// _menuBarFile2
			// 
			this._menuBarFile2.Index = 7;
			this._menuBarFile2.Text = "-";
			// 
			// _menuReceiveFile
			// 
			this._menuReceiveFile.Enabled = false;
			this._menuReceiveFile.Index = 8;
			this._menuReceiveFile.Click += new System.EventHandler(this.OnMenu);
			this._menuReceiveFile.CID = (int)CID.ReceiveFile;
			// 
			// _menuSendFile
			// 
			this._menuSendFile.Enabled = false;
			this._menuSendFile.Index = 9;
			this._menuSendFile.Click += new System.EventHandler(this.OnMenu);
			this._menuSendFile.CID = (int)CID.SendFile;
			// 
			// _menuBarBeforeMRU
			// 
			this._menuBarBeforeMRU.Index = 10;
			this._menuBarBeforeMRU.Text = "-";
			// 
			// _menuBarAfterMRU
			// 
			this._menuBarAfterMRU.Index = 11;
			this._menuBarAfterMRU.Text = "-";
			// 
			// _menuQuit
			// 
			this._menuQuit.Index = 12;
			this._menuQuit.Click += new System.EventHandler(this.OnMenu);
			this._menuQuit.CID = (int)CID.Quit;
			// 
			// _menuEdit
			// 
			this._menuEdit.Index = 1;
			this._menuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this._menuCopy,
																					  this._menuPaste,
																					  this._menuBarEdit1,
																					  this._menuCopyAsLook,
																					  this._menuCopyToFile,
																					  this._menuPasteFromFile,
																					  this._menuBarEdit2,
																					  this._menuClearScreen,
																					  this._menuClearBuffer,
																					  this._menuBarEdit3,
																					  this._menuSelectAll,
																					  this._menuFreeSelectionMode,
																					  this._menuAutoSelectionMode});

			this._menuEdit.Popup += new System.EventHandler(this.AdjustEditMenu);
			// 
			// _menuCopy
			// 
			this._menuCopy.Enabled = false;
			this._menuCopy.Index = 0;
			this._menuCopy.Click += new System.EventHandler(this.OnMenu);
			this._menuCopy.CID = (int)CID.Copy;
			// 
			// _menuPaste
			// 
			this._menuPaste.Enabled = false;
			this._menuPaste.Index = 1;
			this._menuPaste.Click += new System.EventHandler(this.OnMenu);
			this._menuPaste.CID = (int)CID.Paste;
			// 
			// _menuBarEdit1
			// 
			this._menuBarEdit1.Index = 2;
			this._menuBarEdit1.Text = "-";
			// 
			// _menuCopyAsLook
			// 
			this._menuCopyAsLook.Enabled = false;
			this._menuCopyAsLook.Index = 3;
			this._menuCopyAsLook.Click += new System.EventHandler(this.OnMenu);
			this._menuCopyAsLook.CID = (int)CID.CopyAsLook;
			// 
			// _menuCopyToFile
			// 
			this._menuCopyToFile.Enabled = false;
			this._menuCopyToFile.Index = 4;
			this._menuCopyToFile.Click += new System.EventHandler(this.OnMenu);
			this._menuCopyToFile.CID = (int)CID.CopyToFile;
			// 
			// _menuPasteFromFile
			// 
			this._menuPasteFromFile.Enabled = false;
			this._menuPasteFromFile.Index = 5;
			this._menuPasteFromFile.Click += new System.EventHandler(this.OnMenu);
			this._menuPasteFromFile.CID = (int)CID.PasteFromFile;
			// 
			// _menuBarEdit2
			// 
			this._menuBarEdit2.Index = 6;
			this._menuBarEdit2.Text = "-";
			// 
			// _menuClearScreen
			// 
			this._menuClearScreen.Index = 7;
			this._menuClearScreen.Click += new System.EventHandler(this.OnMenu);
			this._menuClearScreen.CID = (int)CID.ClearScreen;
			// 
			// _menuClearBuffer
			// 
			this._menuClearBuffer.Index = 8;
			this._menuClearBuffer.Click += new System.EventHandler(this.OnMenu);
			this._menuClearBuffer.CID = (int)CID.ClearBuffer;
			// 
			// _menuBarEdit3
			// 
			this._menuBarEdit3.Index = 9;
			this._menuBarEdit3.Text = "-";

			// 
			// _menuSelectAll
			// 
			this._menuSelectAll.Index = 10;
			this._menuSelectAll.Click += new System.EventHandler(this.OnMenu);
			this._menuSelectAll.CID = (int)CID.SelectAll;
			// 
			// _menuFreeSelectionMode
			// 
			this._menuFreeSelectionMode.Index = 11;
			this._menuFreeSelectionMode.Click += new System.EventHandler(this.OnMenu);
			this._menuFreeSelectionMode.CID = (int)CID.ToggleFreeSelectionMode;
			// 
			// _menuAutoSelectionMode
			// 
			this._menuAutoSelectionMode.Index = 12;
			this._menuAutoSelectionMode.Click += new System.EventHandler(this.OnMenu);
			this._menuAutoSelectionMode.CID = (int)CID.ToggleAutoSelectionMode;
			// 
			// _menuConsole
			// 
			this._menuConsole.Index = 2;
			this._menuConsole.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this._menuConsoleClose,
																						 this._menuConsoleReproduce,
																						 this._menuBarConsole1,
																						 this._menuNewLine,
																						 this._menuLineFeedRule,
																						 this._menuEncoding,
																						 this._menuLocalEcho,
																						 this._menuSendSpecial,
																						 this._menuBarConsole2,
																						 this._menuSuspendLog,
																						 this._menuCommentLog,
																						 this._menuChangeLog,
																						 this._menuBarConsole3,
																						 this._menuEditRenderProfile,
																						 this._menuServerInfo,
																						 this._menuRenameTab});



			this._menuConsole.Popup += new EventHandler(AdjustConsoleMenu);

			// 
			// _menuConsoleClose
			// 
			this._menuConsoleClose.Enabled = false;
			this._menuConsoleClose.Index = 0;
			this._menuConsoleClose.Click += new System.EventHandler(this.OnMenu);
			this._menuConsoleClose.CID = (int)CID.Close;
			// 
			// _menuConsoleReproduce
			// 
			this._menuConsoleReproduce.Enabled = false;
			this._menuConsoleReproduce.Index = 1;
			this._menuConsoleReproduce.Click += new System.EventHandler(this.OnMenu);
			this._menuConsoleReproduce.CID = (int)CID.Reproduce;
			// 
			// _menuBarConsole1
			// 
			this._menuBarConsole1.Index = 2;
			this._menuBarConsole1.Text = "-";
			// 
			// _menuNewLine
			// 
			this._menuNewLine.Enabled = false;
			this._menuNewLine.Index = 3;
			this._menuNewLine.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this._menuNewLine_CR,
																						 this._menuNewLine_LF,
																						 this._menuNewLine_CRLF});




			// 
			// _menuNewLine_CR
			// 
			this._menuNewLine_CR.Index = 0;
			this._menuNewLine_CR.Text = "CR";
			this._menuNewLine_CR.Click += new System.EventHandler(this.OnChangeNewLine);
			// 
			// _menuNewLine_LF
			// 
			this._menuNewLine_LF.Index = 1;
			this._menuNewLine_LF.Text = "LF";
			this._menuNewLine_LF.Click += new System.EventHandler(this.OnChangeNewLine);
			// 
			// _menuNewLine_CRLF
			// 
			this._menuNewLine_CRLF.Index = 2;
			this._menuNewLine_CRLF.Text = "CR+LF";
			this._menuNewLine_CRLF.Click += new System.EventHandler(this.OnChangeNewLine);
			// 
			// _menuEncoding
			// 
			this._menuEncoding.Enabled = false;
			this._menuEncoding.Index = 4;
			// 
			// _menuLineFeedRule
			// 
			this._menuLineFeedRule.Enabled = false;
			this._menuLineFeedRule.Index = 5;
			this._menuLineFeedRule.Click += new System.EventHandler(this.OnMenu);
			this._menuLineFeedRule.CID = (int)CID.LineFeedRule;
			// 
			// _menuLocalEcho
			// 
			this._menuLocalEcho.Enabled = false;
			this._menuLocalEcho.Index = 6;
			this._menuLocalEcho.Click += new System.EventHandler(this.OnMenu);
			this._menuLocalEcho.CID = (int)CID.ToggleLocalEcho;
			// 
			// _menuSendSpecial
			// 
			this._menuSendSpecial.Enabled = false;
			this._menuSendSpecial.Index = 7;
			this._menuSendSpecial.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this._menuSendBreak,
																						 this._menuAreYouThere,
																						 this._menuSerialConfig,
																						 this._menuResetTerminal
			});




			// 
			// _menuSendBreak
			// 
			this._menuSendBreak.Index = 0;
			this._menuSendBreak.Click += new System.EventHandler(this.OnMenu);
			this._menuSendBreak.CID = (int)CID.SendBreak;
			// 
			// _menuAreYouThere
			// 
			this._menuAreYouThere.Index = 1;
			this._menuAreYouThere.Click += new System.EventHandler(this.OnMenu);
			this._menuAreYouThere.CID = (int)CID.AreYouThere;
			// 
			// _menuSerialConfig
			// 
			this._menuSerialConfig.Index = 2;
			this._menuSerialConfig.Click += new System.EventHandler(this.OnMenu);
			this._menuSerialConfig.CID = (int)CID.SerialConfig;
			// 
			// _menuResetTerminal
			// 
			this._menuResetTerminal.Index = 3;
			this._menuResetTerminal.Click += new System.EventHandler(this.OnMenu);
			this._menuResetTerminal.CID = (int)CID.ResetTerminal;
			// 
			// _menuBarConsole2
			// 
			this._menuBarConsole2.Index = 8;
			this._menuBarConsole2.Text = "-";
			// 
			// _menuSuspendLog
			// 
			this._menuSuspendLog.Enabled = false;
			this._menuSuspendLog.Index = 9;
			this._menuSuspendLog.Click += new System.EventHandler(this.OnMenu);
			this._menuSuspendLog.CID = (int)CID.ToggleLogSuspension;
			// 
			// _menuCommentLog
			// 
			this._menuCommentLog.Enabled = false;
			this._menuCommentLog.Index = 10;
			this._menuCommentLog.Click += new System.EventHandler(this.OnMenu);
			this._menuCommentLog.CID = (int)CID.CommentLog;
			// 
			// _menuChangeLog
			// 
			this._menuChangeLog.Enabled = false;
			this._menuChangeLog.Index = 11;
			this._menuChangeLog.Click += new System.EventHandler(this.OnMenu);
			this._menuChangeLog.CID = (int)CID.ChangeLogFile;
			// 
			// _menuBarConsole3
			// 
			this._menuBarConsole3.Index = 12;
			this._menuBarConsole3.Text = "-";
			// 
			// _menuEditRenderProfile
			// 
			this._menuEditRenderProfile.Enabled = false;
			this._menuEditRenderProfile.Index = 13;
			this._menuEditRenderProfile.Click += new System.EventHandler(this.OnMenu);
			this._menuEditRenderProfile.CID = (int)CID.EditRenderProfile;
			// 
			// _menuServerInfo
			// 
			this._menuServerInfo.Enabled = false;
			this._menuServerInfo.Index = 14;
			this._menuServerInfo.Click += new System.EventHandler(this.OnMenu);
			this._menuServerInfo.CID = (int)CID.ShowServerInfo;
			// 
			// _menuRenameTab
			// 
			this._menuRenameTab.Enabled = false;
			this._menuRenameTab.Index = 15;
			this._menuRenameTab.Click += new System.EventHandler(this.OnMenu);
			this._menuRenameTab.CID = (int)CID.RenameTab;
			// 
			// _menuTool
			// 
			this._menuTool.Index = 3;
			this._menuTool.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this._menuKeyGen,
																					  this._menuChangePassphrase,
																					  this._menuLaunchPortforwarding,
																					  this._menuBarTool1,
																					  this._menuMacro,
																					  this._menuBarTool2,
																					  this._menuOption});

			// 
			// _menuKeyGen
			// 
			this._menuKeyGen.Index = 0;
			this._menuKeyGen.Click += new System.EventHandler(this.OnMenu);
			this._menuKeyGen.CID = (int)CID.KeyGen;
			// 
			// _menuChangePassphrase
			// 
			this._menuChangePassphrase.Index = 1;
			this._menuChangePassphrase.Click += new System.EventHandler(this.OnMenu);
			this._menuChangePassphrase.CID = (int)CID.ChangePassphrase;
			// 
			// _menuLaunchPortforwarding
			// 
			this._menuLaunchPortforwarding.Index = 2;
			this._menuLaunchPortforwarding.Click += new System.EventHandler(this.OnMenu);
			this._menuLaunchPortforwarding.CID = (int)CID.Portforwarding;
			// 
			// _menuBarTool1
			// 
			this._menuBarTool1.Index = 3;
			this._menuBarTool1.Text = "-";
			// 
			// _menuMacro
			// 
			this._menuMacro.Index = 4;
			this._menuMacro.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this._menuMacroConfig,
																						this._menuStopMacro,
																						this._menuBarMacro});
			// 
			// _menuBarTool2
			// 
			this._menuBarTool2.Index = 5;
			this._menuBarTool2.Text = "-";
			// 
			// _menuOption
			// 
			this._menuOption.Index = 6;
			this._menuOption.Click += new System.EventHandler(this.OnMenu);
			this._menuOption.CID = (int)CID.OptionDialog;
			// 
			// _menuMacroConfig
			// 
			this._menuMacroConfig.Index = 0;
			this._menuMacroConfig.Click += new System.EventHandler(this.OnMenu);
			this._menuMacroConfig.CID = (int)CID.MacroConfig;
			// 
			// _menuStopMacro
			// 
			this._menuStopMacro.Index = 1;
			this._menuStopMacro.Enabled = false;
			this._menuStopMacro.Click += new System.EventHandler(this.OnMenu);
			this._menuStopMacro.CID = (int)CID.StopMacro;
			// 
			// _menuBarMacro
			// 
			this._menuBarMacro.Index = 2;
			this._menuBarMacro.Text = "-";
			// 
			// _menuWindow
			// 
			this._menuWindow.Popup += new EventHandler(OnWindowMenuClicked);
			this._menuWindow.Index = 4;
			this._menuWindow.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this._menuFrameStyle,
																						this._menuBarWindow1,
																						this._menuPane,
																						this._menuTab,
																						this._menuBarWindow2,
																						this._menuCloseAll,
																						this._menuCloseAllDisconnected,
																						this._menuBarWindow3});




			// 
			// _menuFrameStyle
			// 
			this._menuFrameStyle.Index = 0;
			this._menuFrameStyle.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this._menuFrameStyleSingle,
																							this._menuFrameStyleDivHorizontal,
																							this._menuFrameStyleDivVertical,
																							this._menuFrameStyleDivHorizontal3,
																							this._menuFrameStyleDivVertical3});




			// 
			// _menuFrameStyleSingle
			// 
			this._menuFrameStyleSingle.Index = 0;
			this._menuFrameStyleSingle.Click += new System.EventHandler(this.OnMenu);
			this._menuFrameStyleSingle.CID = (int)CID.FrameStyleSingle;
			// 
			// _menuFrameStyleDivHorizontal
			// 
			this._menuFrameStyleDivHorizontal.Index = 1;
			this._menuFrameStyleDivHorizontal.Click += new System.EventHandler(this.OnMenu);
			this._menuFrameStyleDivHorizontal.CID = (int)CID.FrameStyleDivHorizontal;
			// 
			// _menuFrameStyleDivVertical
			// 
			this._menuFrameStyleDivVertical.Index = 2;
			this._menuFrameStyleDivVertical.Click += new System.EventHandler(this.OnMenu);
			this._menuFrameStyleDivVertical.CID = (int)CID.FrameStyleDivVertical;
			// 
			// _menuFrameStyleDivHorizontal3
			// 
			this._menuFrameStyleDivHorizontal3.Index = 3;
			this._menuFrameStyleDivHorizontal3.Click += new System.EventHandler(this.OnMenu);
			this._menuFrameStyleDivHorizontal3.CID = (int)CID.FrameStyleDivHorizontal3;
			// 
			// _menuFrameStyleDivVertical3
			// 
			this._menuFrameStyleDivVertical3.Index = 4;
			this._menuFrameStyleDivVertical3.Click += new System.EventHandler(this.OnMenu);
			this._menuFrameStyleDivVertical3.CID = (int)CID.FrameStyleDivVertical3;
			// 
			// _menuBarWindow1
			// 
			this._menuBarWindow1.Index = 1;
			this._menuBarWindow1.Text = "-";
			// 
			// _menuPane
			// 
			this._menuPane.Index = 2;
			this._menuPane.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this._menuMovePane,
																							this._menuExpandPane,
																							this._menuShrinkPane});




			// 
			// _menuMovePane
			// 
			this._menuMovePane.Index = 0;
			this._menuMovePane.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						  this._menuMovePaneUp,
																						  this._menuMovePaneDown,
																						  this._menuMovePaneLeft,
																						  this._menuMovePaneRight});




			// 
			// _menuMovePaneUp
			// 
			this._menuMovePaneUp.Index = 0;
			this._menuMovePaneUp.Click += new System.EventHandler(this.OnMenu);
			this._menuMovePaneUp.CID = (int)CID.MovePaneUp;
			// 
			// _menuMovePaneDown
			// 
			this._menuMovePaneDown.Index = 1;
			this._menuMovePaneDown.Click += new System.EventHandler(this.OnMenu);
			this._menuMovePaneDown.CID = (int)CID.MovePaneDown;
			// 
			// _menuMovePaneLeft
			// 
			this._menuMovePaneLeft.Index = 2;
			this._menuMovePaneLeft.Click += new System.EventHandler(this.OnMenu);
			this._menuMovePaneLeft.CID = (int)CID.MovePaneLeft;
			// 
			// _menuMovePaneRight
			// 
			this._menuMovePaneRight.Index = 3;
			this._menuMovePaneRight.Click += new System.EventHandler(this.OnMenu);
			this._menuMovePaneRight.CID = (int)CID.MovePaneRight;
			// 
			// _menuExpandPane
			// 
			this._menuExpandPane.Index = 1;
			this._menuExpandPane.Click += new EventHandler(this.OnMenu);
			this._menuExpandPane.CID = (int)CID.ExpandPane;
			// 
			// _menuShrinkPane
			// 
			this._menuShrinkPane.Index = 2;
			this._menuShrinkPane.Click += new EventHandler(this.OnMenu);
			this._menuShrinkPane.CID = (int)CID.ShrinkPane;
			// 
			// _menuTab
			// 
			this._menuTab.Index = 3;
			this._menuTab.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this._menuPrevTab,
																					  this._menuNextTab,
																					  this._menuMoveTabToPrev,
																					  this._menuMoveTabToNext});



			// 
			// _menuPrevTab
			// 
			this._menuPrevTab.Index = 0;
			this._menuPrevTab.Click += new EventHandler(this.OnMenu);
			this._menuPrevTab.CID = (int)CID.PrevPane;
			// 
			// _menuNextTab
			// 
			this._menuNextTab.Index = 1;
			this._menuNextTab.Click += new EventHandler(this.OnMenu);
			this._menuNextTab.CID = (int)CID.NextPane;
			// 
			// _menuMoveTabToPrev
			// 
			this._menuMoveTabToPrev.Index = 2;
			this._menuMoveTabToPrev.Click += new EventHandler(this.OnMenu);
			this._menuMoveTabToPrev.CID = (int)CID.MoveTabToPrev;
			// 
			// _menuMoveTabToNext
			// 
			this._menuMoveTabToNext.Index = 3;
			this._menuMoveTabToNext.Click += new EventHandler(this.OnMenu);
			this._menuMoveTabToNext.CID = (int)CID.MoveTabToNext;
			// 
			// _menuBarWindow2
			// 
			this._menuBarWindow2.Index = 4;
			this._menuBarWindow2.Text = "-";
			// 
			// _menuCloseAll
			// 
			this._menuCloseAll.Index = 5;
			this._menuCloseAll.Click += new System.EventHandler(this.OnMenu);
			this._menuCloseAll.CID = (int)CID.CloseAll;
			// 
			// _menuCloseAllDisconnected
			// 
			this._menuCloseAllDisconnected.Index = 6;
			this._menuCloseAllDisconnected.Click += new System.EventHandler(this.OnMenu);
			this._menuCloseAllDisconnected.CID = (int)CID.CloseAllDisconnected;
			// 
			// _menuBarWindow3
			// 
			this._menuBarWindow3.Index = 7;
			this._menuBarWindow3.Text = "-";
			// 
			// _menuHelp
			// 
			this._menuHelp.Index = 5;
			this._menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this._menuAboutBox,this._menuProductWeb});


			// 
			// _menuAboutBox
			// 
			this._menuAboutBox.Index = 0;
			this._menuAboutBox.Click += new System.EventHandler(this.OnMenu);
			this._menuAboutBox.CID = (int)CID.AboutBox;
			// 
			// _menuProductWeb
			// 
			this._menuProductWeb.Index = 1;
			this._menuProductWeb.Click += new System.EventHandler(this.OnMenu);
			this._menuProductWeb.CID = (int)CID.ProductWeb;
			// 
			// _statusBar
			// 
			this._statusBar.Location = new System.Drawing.Point(0, 689);
			this._statusBar.Dock = DockStyle.Bottom;
			this._statusBar.Name = "_statusBar";
			this._statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						  this._textStatusBarPanel,
																						  this._bellIndicateStatusBarPanel,
																						  this._caretPosStatusBarPanel});
			this._statusBar.ShowPanels = true;
			this._statusBar.Size = new System.Drawing.Size(600, 24);
			this._statusBar.TabIndex = 5;
			// 
			// _textStatusBarPanel
			// 
			this._textStatusBarPanel.BorderStyle = StatusBarPanelBorderStyle.None;
			this._textStatusBarPanel.Width = 443;
			// 
			// _bellIndicateStatusBarPanel
			// 
			this._bellIndicateStatusBarPanel.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
			this._bellIndicateStatusBarPanel.Width = 31;

			// 
			// _caretPosStatusBarPanel
			// 
			this._caretPosStatusBarPanel.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
			this._caretPosStatusBarPanel.Width = 120;

			this._statusBar.SuspendLayout(); //このAutoSizeセットが極めて重いことが判明。これで軽くなるか？
			this._textStatusBarPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this._bellIndicateStatusBarPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.None;
			this._caretPosStatusBarPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.None;
			this._statusBar.ResumeLayout(false);

			// 
			// _multiPaneControl
			// 
			_multiPaneControl = new MultiPaneControl();
			_multiPaneControl.Dock = DockStyle.Fill;
			// 
			// GFrame
			// 
			this.AllowDrop = true;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._multiPaneControl,
																		  });
			this.Name = "GFrame";
			this.Text = "Poderosa";
			this.Menu = this._menu;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			((System.ComponentModel.ISupportInitialize)(this._textStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._bellIndicateStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._caretPosStatusBarPanel)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion
        #region 
        private void InitMenuText()
        {
            #region Poderosa string bullshit
            StringResources sr = GApp.Strings;
			this._menuFile.Text = sr.GetString("Menu._menuFile");
			this._menuNewConnection.Text = sr.GetString("Menu._menuNewConnection");
			this._menuSerialNewConnection.Text = sr.GetString("Menu._menuSerialNewConnection");
			this._menuCygwinNewConnection.Text = sr.GetString("Menu._menuCygwinNewConnection");
			this._menuSFUNewConnection.Text = sr.GetString("Menu._menuSFUNewConnection");
			this._menuOpenShortcut.Text = sr.GetString("Menu._menuOpenShortcut");
			this._menuSaveShortcut.Text = sr.GetString("Menu._menuSaveShortcut");
			this._menuReceiveFile.Text = sr.GetString("Menu._menuReceiveFile");
			this._menuSendFile.Text = sr.GetString("Menu._menuSendFile");
			this._menuQuit.Text = sr.GetString("Menu._menuQuit");
			this._menuEdit.Text = sr.GetString("Menu._menuEdit");
			this._menuCopy.Text = sr.GetString("Menu._menuCopy");
			this._menuPaste.Text = sr.GetString("Menu._menuPaste");
			this._menuCopyAsLook.Text = sr.GetString("Menu._menuCopyAsLook");
			this._menuCopyToFile.Text = sr.GetString("Menu._menuCopyToFile");
			this._menuPasteFromFile.Text = sr.GetString("Menu._menuPasteFromFile");
			this._menuClearScreen.Text = sr.GetString("Menu._menuClearScreen");
			this._menuClearBuffer.Text = sr.GetString("Menu._menuClearBuffer");
			this._menuSelectAll.Text = sr.GetString("Menu._menuSelectAll");
			this._menuFreeSelectionMode.Text = sr.GetString("Menu._menuFreeSelectionMode");
			this._menuAutoSelectionMode.Text = sr.GetString("Menu._menuAutoSelectionMode");
			this._menuConsole.Text = sr.GetString("Menu._menuConsole");
			this._menuConsoleClose.Text = sr.GetString("Menu._menuConsoleClose");
			this._menuConsoleReproduce.Text = sr.GetString("Menu._menuConsoleReproduce");
			this._menuNewLine.Text = sr.GetString("Menu._menuNewLine");
			this._menuEncoding.Text = sr.GetString("Menu._menuEncoding");
			this._menuLineFeedRule.Text = sr.GetString("Menu._menuLineFeedRule");
			this._menuLocalEcho.Text = sr.GetString("Menu._menuLocalEcho");
			this._menuSendSpecial.Text = sr.GetString("Menu._menuSendSpecial");
			this._menuSendBreak.Text = sr.GetString("Menu._menuSendBreak");
			this._menuAreYouThere.Text = sr.GetString("Menu._menuAreYouThere");
			this._menuSerialConfig.Text = sr.GetString("Menu._menuSerialConfig");
			this._menuResetTerminal.Text = sr.GetString("Menu._menuResetTerminal");
			this._menuSuspendLog.Text = sr.GetString("Menu._menuSuspendLog");
			this._menuCommentLog.Text = sr.GetString("Menu._menuCommentLog");
			this._menuChangeLog.Text = sr.GetString("Menu._menuChangeLog");
			this._menuEditRenderProfile.Text = sr.GetString("Menu._menuEditRenderProfile");
			this._menuServerInfo.Text = sr.GetString("Menu._menuServerInfo");
			this._menuRenameTab.Text = sr.GetString("Menu._menuRenameTab");
			this._menuTool.Text = sr.GetString("Menu._menuTool");
			this._menuKeyGen.Text = sr.GetString("Menu._menuKeyGen");
			this._menuChangePassphrase.Text = sr.GetString("Menu._menuChangePassphrase");
			this._menuLaunchPortforwarding.Text = sr.GetString("Menu._menuLaunchPortforwarding");
			this._menuMacro.Text = sr.GetString("Menu._menuMacro");
			this._menuOption.Text = sr.GetString("Menu._menuOption");
			this._menuMacroConfig.Text = sr.GetString("Menu._menuMacroConfig");
			this._menuStopMacro.Text = sr.GetString("Menu._menuStopMacro");
			this._menuWindow.Text = sr.GetString("Menu._menuWindow");
			this._menuFrameStyle.Text = sr.GetString("Menu._menuFrameStyle");
			this._menuFrameStyleSingle.Text = sr.GetString("Menu._menuFrameStyleSingle");
			this._menuFrameStyleDivHorizontal.Text = sr.GetString("Menu._menuFrameStyleDivHorizontal");
			this._menuFrameStyleDivVertical.Text = sr.GetString("Menu._menuFrameStyleDivVertical");
			this._menuFrameStyleDivHorizontal3.Text = sr.GetString("Menu._menuFrameStyleDivHorizontal3");
			this._menuFrameStyleDivVertical3.Text = sr.GetString("Menu._menuFrameStyleDivVertical3");
			this._menuPane.Text = sr.GetString("Menu._menuPane");
			this._menuMovePane.Text = sr.GetString("Menu._menuMovePane");
			this._menuMovePaneUp.Text = sr.GetString("Menu._menuMovePaneUp");
			this._menuMovePaneDown.Text = sr.GetString("Menu._menuMovePaneDown");
			this._menuMovePaneLeft.Text = sr.GetString("Menu._menuMovePaneLeft");
			this._menuMovePaneRight.Text = sr.GetString("Menu._menuMovePaneRight");
			this._menuExpandPane.Text = sr.GetString("Menu._menuExpandPane");
			this._menuShrinkPane.Text = sr.GetString("Menu._menuShrinkPane");
			this._menuTab.Text = sr.GetString("Menu._menuTab");
			this._menuPrevTab.Text = sr.GetString("Menu._menuPrevTab");
			this._menuNextTab.Text = sr.GetString("Menu._menuNextTab");
			this._menuMoveTabToPrev.Text = sr.GetString("Menu._menuMoveTabToPrev");
			this._menuMoveTabToNext.Text = sr.GetString("Menu._menuMoveTabToNext");
			this._menuCloseAll.Text = sr.GetString("Menu._menuCloseAll");
			this._menuCloseAllDisconnected.Text = sr.GetString("Menu._menuCloseAllDisconnected");
			this._menuHelp.Text = sr.GetString("Menu._menuHelp");
			this._menuAboutBox.Text = sr.GetString("Menu._menuAboutBox");
			this._menuProductWeb.Text = sr.GetString("Menu._menuProductWeb");
            #endregion
        }
		public void ReloadLanguage(Language l) {
			InitMenuText();
			MainMenu mm = new MainMenu();
			while(_menu.MenuItems.Count>0) {
				mm.MenuItems.Add(_menu.MenuItems[0]);
			}
			_menu = mm;
			this.Menu = mm;
			InitContextMenu();
			AdjustMRUMenu();
			if(_toolBar!=null) _toolBar.ReloadLanguage(l);
			AdjustTitle(GEnv.Connections.ActiveTag);
			AdjustMacroMenu();

			if(_xmodemDialog!=null) _xmodemDialog.ReloadLanguage();
		}
		private void CreateToolBar() {
			_toolBar = new GDialogBar();
			_toolBar.Dock = DockStyle.Top;
			_toolBar.Height = 27;
		}
		private void InitContextMenu() {
			
			_contextMenu = new ContextMenu();

			GMenuItem copy = new GMenuItem();
			copy.Text = GApp.Strings.GetString("Menu._menuCopy");
			copy.Click += new EventHandler(OnMenu);
			copy.ShortcutKey = GApp.Options.Commands.FindKey(CID.Copy);
			copy.CID = (int)CID.Copy;
			
			GMenuItem paste = new GMenuItem();
			paste.Text = GApp.Strings.GetString("Menu._menuPaste");
			paste.Click += new EventHandler(OnMenu);
			paste.ShortcutKey = GApp.Options.Commands.FindKey(CID.Paste);
			paste.CID = (int)CID.Paste;

			GMenuItem bar = new GMenuItem();
			bar.Text = "-";

			_contextMenu.MenuItems.Add(copy);
			_contextMenu.MenuItems.Add(paste);
			_contextMenu.MenuItems.Add(bar);

			foreach(MenuItem child in _menuConsole.MenuItems) {
				MenuItem mi = child.CloneMenu();
				_contextMenu.MenuItems.Add(mi);
				CloneMenuCommand(child, mi);
			}
		}
		private void CloneMenuCommand(MenuItem src, MenuItem dest) {
			//CloneMenuされた子メニューも含んでいるかもしれない
			int index = 0;
			foreach(MenuItem child in src.MenuItems)
				CloneMenuCommand(child, dest.MenuItems[index++]);
		}
		public GDialogBar ToolBar {
			get {
				return _toolBar;
			}
		}
		public MultiPaneControl PaneContainer {
			get {
				return _multiPaneControl;
			}
		}
		public XModemDialog XModemDialog {
			get {
				return _xmodemDialog;
			}
			set {
				_xmodemDialog = value;
			}
        }
        #endregion
        #region properties
        public override ContextMenu ContextMenu {
			get {
				if(_contextMenu==null) InitContextMenu();
				return _contextMenu;
			}
		}
		public GStatusBar StatusBar {
			get {
				return _gStatusBar;
			}
		}
		public TabBar TabBar {
			get {
				return _tabBar;
			}
		}
		public MenuItem MenuNewLineCR {
			get {
				return _menuNewLine_CR;
			}
		}
		public MenuItem MenuNewLineLF {
			get {
				return _menuNewLine_LF;
			}
		}
		public MenuItem MenuNewLineCRLF {
			get {
				return _menuNewLine_CRLF;
			}
		}
		public MenuItem MenuLocalEcho {
			get {
				return _menuLocalEcho;
			}
		}
		public MenuItem MenuSuspendLog {
			get {
				return _menuSuspendLog;
			}
		}
		public MenuItem MenuMacroStop {
			get {
				return this._menuStopMacro;
			}
		}
        #endregion
        #region
        public void AdjustTitle(ConnectionTag tag) {
			string title = "";
			if(tag!=null) {
				title = tag.FormatFrameText() + " - ";
			}

			if(GApp.MacroManager.MacroIsRunning) {
				title += String.Format(GApp.Strings.GetString("Caption.GFrame.MacroIsRunning"), GApp.MacroManager.CurrentMacro.Title);
			}
			title += "Poderosa";
		
			this.Text = title;
		}
		public void AddConnection(ConnectionTag ct) {
			//この順序には要注意
			_tabBar.AddTab(ct);
			AddWindowMenu(ct);
		}
		internal void ActivateConnection(ConnectionTag ct) {
			if(ct!=null) {
				if(_tabBar!=null) _tabBar.SetActiveTab(ct);
				AdjustTerminalUI(true, ct);
				_multiPaneControl.ActivateConnection(ct);
				AdjustTitle(ct);
				if(_xmodemDialog!=null && !_xmodemDialog.Executing) _xmodemDialog.ConnectionTag = ct;
			}
			else {
				AdjustTerminalUI(false, null);
				AdjustTitle(null);
				if(_xmodemDialog!=null) _xmodemDialog.Close();
			}

			IDictionaryEnumerator e = _windowMenuItemMap.GetEnumerator();
			while(e.MoveNext()) {
				((MenuItem)e.Key).Checked = (ct==e.Value);
			}
		}
		public void RefreshConnection(ConnectionTag ct) {
			if(ct!=null) {
				_tabBar.RefreshConnection(ct);
				_tabBar.ArrangeButtons();
			}
			
			if(GEnv.Connections.Count==0) {
				AdjustTerminalUI(false, null);
				AdjustTitle(null);
			}
			else if(ct==GEnv.Connections.ActiveTag) {
				_tabBar.SetActiveTab(ct);
				AdjustTitle(ct);
			}

			//Windowメニューの調整
			IDictionaryEnumerator e = _windowMenuItemMap.GetEnumerator();
			while(e.MoveNext()) {
				if(ct==e.Value) {
					((GMenuItem)e.Key).Text = ct.FormatTabText();
					break;
				}
			}
		}
		public void RemoveConnection(ConnectionTag ct) {
			_tabBar.RemoveTab(ct);
			RemoveWindowMenu(ct);
		}
		public void ReplaceConnection(ConnectionTag prev, ConnectionTag next) {
			IDictionaryEnumerator e = _windowMenuItemMap.GetEnumerator();
			while(e.MoveNext()) {
				if(prev==e.Value) {
					object k = e.Key;
					_windowMenuItemMap.Remove(k);
					_windowMenuItemMap.Add(k, next);
					break;
				}
			}
		}
		public void RemoveAllConnections() {
			_tabBar.Clear();
			ClearWindowMenu();
			AdjustTitle(null);
			_multiPaneControl.RemoveAllConnections();
		}
		public void AdjustTerminalUI(bool enabled, ConnectionTag ct) {
			TerminalConnection con = ct==null? null : ct.Connection;
			if(_toolBar!=null) _toolBar.EnableTerminalUI(enabled, con);
			
			bool e = GEnv.Connections.Count>0;
			_menuCloseAll.Enabled = e;
			_menuMovePane.Enabled = e;
			_menuNextTab.Enabled = e;
			_menuPrevTab.Enabled = e;


			_menuSaveShortcut.Enabled = (ct!=null);
			_menuSendFile.Enabled = (ct!=null);
			_menuReceiveFile.Enabled = (ct!=null);
			AdjustConsoleMenu(_menuConsole.MenuItems, enabled, con, 0);
		}
		internal void AdjustContextMenu(bool enabled, TerminalConnection con) {
			Menu.MenuItemCollection col = this.ContextMenu.MenuItems;
			col[0].Enabled = !GEnv.TextSelection.IsEmpty && GEnv.TextSelection.Owner.Connection==con;
			col[1].Enabled = !con.IsClosed && CanPaste();
			AdjustConsoleMenu(col, enabled, con, 3); //コピー、ペースト、区切り線の先がコンソールメニュー
		}
		internal TerminalConnection CommandTargetConnection {
			get {
				return _commandTargetConnection;
			}
			set {
				_commandTargetConnection = value;
			}
		}
		private void AdjustConsoleMenu(object sender, EventArgs args) {
			AdjustConsoleMenu(_menuConsole.MenuItems, GEnv.Connections.ActiveTag!=null, GEnv.Connections.ActiveConnection, 0);
		}
		private void AdjustConsoleMenu(Menu.MenuItemCollection target, bool enabled, TerminalConnection con, int baseIndex) {
			target[baseIndex + _menuServerInfo.Index].Enabled = enabled;
			target[baseIndex + _menuLocalEcho.Index].Enabled = enabled && !con.IsClosed;
			target[baseIndex + _menuLineFeedRule.Index].Enabled = enabled && !con.IsClosed;
			target[baseIndex + _menuEncoding.Index].Enabled = enabled && !con.IsClosed;
			target[baseIndex + _menuConsoleClose.Index].Enabled = enabled;
			//複製と再接続は動作はほとんど一緒
			target[baseIndex + _menuConsoleReproduce.Index].Enabled = enabled;
			target[baseIndex + _menuConsoleReproduce.Index].Text = GApp.Strings.GetString((con!=null && con.IsClosed)? "Menu._menuConsoleRevive" : "Menu._menuConsoleReproduce");
			target[baseIndex + _menuSendSpecial.Index].Enabled = enabled && !con.IsClosed;
			target[baseIndex + _menuNewLine.Index].Enabled = enabled && !con.IsClosed;
			target[baseIndex + _menuCommentLog.Index].Enabled = enabled && !con.IsClosed && con.TextLogger.IsActive;
			target[baseIndex + _menuSuspendLog.Index].Enabled = enabled && !con.IsClosed && con.TextLogger.IsActive;
			target[baseIndex + _menuChangeLog.Index].Enabled = enabled && !con.IsClosed;
			target[baseIndex + _menuEditRenderProfile.Index].Enabled = enabled;
			target[baseIndex + _menuRenameTab.Index].Enabled = enabled && !con.IsClosed;

			Menu.MenuItemCollection nls = target[baseIndex + _menuSendSpecial.Index].MenuItems;
			nls[_menuSerialConfig.Index].Enabled = enabled && (con is SerialTerminalConnection);
			nls[_menuResetTerminal.Index].Enabled = enabled && !con.IsClosed;

			if(enabled) {
				target[baseIndex + _menuLocalEcho.Index].Checked = con.Param.LocalEcho;
				target[baseIndex + _menuSuspendLog.Index].Checked = con.LogSuspended;

				nls = target[baseIndex + _menuNewLine.Index].MenuItems;
				nls[_menuNewLine_CR.Index].Checked = (con.Param.TransmitNL==NewLine.CR);
				nls[_menuNewLine_LF.Index].Checked = (con.Param.TransmitNL==NewLine.LF);
				nls[_menuNewLine_CRLF.Index].Checked = (con.Param.TransmitNL==NewLine.CRLF);
				
				nls = target[baseIndex + _menuEncoding.Index].MenuItems;
				for(int i=0; i<nls.Count; i++) {
					nls[i].Checked = (i==(int)con.Param.EncodingProfile.Type);
				}
			}
		}
		public void AdjustMacroMenu() {
			int n = _menuBarMacro.Index+1;
			while(n < _menuMacro.MenuItems.Count)
				_menuMacro.MenuItems.RemoveAt(n); //バー以降を全消去

			foreach(MacroModule mod in GApp.MacroManager.Modules) {
				GMenuItem mi = new GMenuItem();
				mi.Text = mod.Title;
				mi.ShortcutKey = mod.ShortCut;
				mi.Click += new EventHandler(OnExecMacro);
				_menuMacro.MenuItems.Add(mi);
			}
		}
		private void OnExecMacro(object sender, EventArgs args) {
			GMenuItem mi = (GMenuItem)sender;
			int i = mi.Index - (_menuBarMacro.Index+1);
			GApp.MacroManager.Execute(this, GApp.MacroManager.GetModule(i));
		}
		private void AddWindowMenu(ConnectionTag ct) {
			GMenuItem mi = new GMenuItem();
			mi.Text = ct.FormatTabText();
			mi.Checked = true;
			//このショートカットは固定で、カスタマイズ不可
			if(_windowMenuItemMap.Count<=8)
				mi.ShortcutKey = Keys.Alt | (Keys)((int)Keys.D1 + _windowMenuItemMap.Count);
			else if(_windowMenuItemMap.Count==9)
				mi.ShortcutKey = Keys.Alt | Keys.D0;

			foreach(MenuItem m in _windowMenuItemMap.Keys) {
				m.Checked = false;
			}

			_windowMenuItemMap.Add(mi, ct);
			mi.Click += new EventHandler(OnWindowItemMenuClicked);
			_menuWindow.MenuItems.Add(mi);
		}
		private void RemoveWindowMenu(ConnectionTag ct) {
			IDictionaryEnumerator e = _windowMenuItemMap.GetEnumerator();
			while(e.MoveNext()) {
				if(ct==e.Value) {
					_menuWindow.MenuItems.Remove((MenuItem)e.Key);
					_windowMenuItemMap.Remove(e.Key);
					break;
				}
			}

			for(int i=_menuBarWindow3.Index+1; i<_menuWindow.MenuItems.Count; i++) {
				GMenuItem mi = (GMenuItem)_menuWindow.MenuItems[i];
				int n = i - (_menuBarWindow3.Index+1);
				if(n<=8)
					mi.ShortcutKey = Keys.Alt | (Keys)((int)Keys.D1 + n);
				else if(n==9)
					mi.ShortcutKey = Keys.Alt | Keys.D0;
				else
					mi.ShortcutKey = Keys.None;
			}
		}
		public void ReorderWindowMenu(int index, int newindex, ConnectionTag active_tag) {
			GMenuItem mi1 = (GMenuItem)_menuWindow.MenuItems[TagIndexToWindowMenuItemIndex(index)];
			Keys mi1_key = mi1.ShortcutKey;
			GMenuItem mi2 = (GMenuItem)_menuWindow.MenuItems[TagIndexToWindowMenuItemIndex(newindex)];
			mi1.ShortcutKey = mi2.ShortcutKey;
			mi2.ShortcutKey = mi1_key;

			_menuWindow.MenuItems.Remove(mi1);
			_menuWindow.MenuItems.Add(TagIndexToWindowMenuItemIndex(newindex), mi1);
			if(_tabBar!=null) _tabBar.ReorderButton(index, newindex, active_tag);
		}
		private int TagIndexToWindowMenuItemIndex(int index) {
			return _menuBarWindow3.Index+1+index;
		}
		private void ClearWindowMenu() {
			int i = _menuBarWindow3.Index+1;
			while(_menuWindow.MenuItems.Count > i) {
				_menuWindow.MenuItems.RemoveAt(i);
			}
			_windowMenuItemMap.Clear();
		}			
		public void AdjustMRUMenu() {
			//まず既存MRUメニューのクリア
			_MRUMenuToParameter.Clear();
			int i = _menuBarBeforeMRU.Index + 1;
			Menu.MenuItemCollection mi = _menuFile.MenuItems;
			while(_menuBarAfterMRU.Index > i)
				mi.RemoveAt(i);

			//リストからセット
			int count = GApp.Options.MRUSize;
			i = 0;
			foreach(TerminalParam p in GApp.ConnectionHistory) {
				GMenuItem mru = new GMenuItem();
				_MRUMenuToParameter[mru] = p;
				string text = p.Caption;
				if(text==null || text.Length==0) text = p.ShortDescription;
				mru.Text = i<=8?
					String.Format("&{0} {1} - {2}", i+1, text, p.MethodName) :
					String.Format("{0} {1} - {2}",  i+1, text, p.MethodName);
				mru.Click += new EventHandler(OnMRUMenuClicked);
				mi.Add(_menuBarBeforeMRU.Index+i+1, mru);

				if(++i == count) break;
			}

			_menuBarAfterMRU.Visible = (i>0); //１つもないときはバーが連続してしまい見苦しい
		}
		public void ApplyOptions(ContainerOptions prev, ContainerOptions opt) {

			_contextMenu = null;
			_menuMovePaneUp.Enabled = _menuMovePaneDown.Enabled = (opt.FrameStyle==GFrameStyle.DivHorizontal || opt.FrameStyle==GFrameStyle.DivHorizontal3);
			_menuMovePaneLeft.Enabled = _menuMovePaneRight.Enabled = (opt.FrameStyle==GFrameStyle.DivVertical || opt.FrameStyle==GFrameStyle.DivVertical3);
			_menuFrameStyleSingle.Checked = opt.FrameStyle==GFrameStyle.Single;
			_menuFrameStyleDivHorizontal.Checked = opt.FrameStyle==GFrameStyle.DivHorizontal;
			_menuFrameStyleDivVertical.Checked = opt.FrameStyle==GFrameStyle.DivVertical;
			_menuFrameStyleDivHorizontal3.Checked = opt.FrameStyle==GFrameStyle.DivHorizontal3;
			_menuFrameStyleDivVertical3.Checked = opt.FrameStyle==GFrameStyle.DivVertical3;
			_menuExpandPane.Enabled = opt.FrameStyle!=GFrameStyle.Single;
			_menuShrinkPane.Enabled = opt.FrameStyle!=GFrameStyle.Single;

			if(prev!=null && prev.FrameStyle!=opt.FrameStyle) //起動直後(prev==null)だとまだレイアウトがされていないのでInitUIは実行できない
				_multiPaneControl.InitUI(prev, opt);

			bool toolbar = prev!=null && prev.ShowToolBar;
			bool tabbar = prev!=null && prev.ShowTabBar;
			bool statusbar = prev!=null && prev.ShowStatusBar;

			this.SuspendLayout();
			_multiPaneControl.ApplyOptions(opt);
			_tabBar.ApplyOptions(opt);

			if(!tabbar && opt.ShowTabBar) {
				this.Controls.Add(_tabBar);
				this.Controls.SetChildIndex(_tabBar, 1); //index 0は_multiPaneControl固定
			}
			else if(tabbar && !opt.ShowTabBar) {
				this.Controls.Remove(_tabBar);
			}

			if(!toolbar && opt.ShowToolBar) {
				if(_toolBar==null) CreateToolBar();
				this.Controls.Add(_toolBar);
				this.Controls.SetChildIndex(_toolBar, opt.ShowTabBar? 2 : 1); 
			}
			else if(toolbar && !opt.ShowToolBar) {
				if(_toolBar!=null) this.Controls.Remove(_toolBar);
			}
			if(opt.ShowToolBar)
				_toolBar.ApplyOptions(opt);

			if(!statusbar && opt.ShowStatusBar) {
				this.Controls.Add(_statusBar);
				this.Controls.SetChildIndex(_statusBar, this.Controls.Count-1);
			}
			else if(statusbar && !opt.ShowStatusBar) {
				this.Controls.Remove(_statusBar);
			}
			this.ResumeLayout(true);

		}
		public void ApplyHotKeys() {
			ApplyHotKeys(GApp.Options.Commands);
		}
		public void ApplyHotKeys(Commands cmds) {
			ApplyHotKeys(cmds, _menu.MenuItems);
			AdjustMacroMenu();
		}
		private void ApplyHotKeys(Commands km, Menu.MenuItemCollection items) {
			foreach(GMenuItemBase mib in items) {
				GMenuItem mi = mib as GMenuItem;
				if(mi!=null) {
					CID cid = (CID)mi.CID;
					mi.ShortcutKey = km.FindKey(cid);

				}
				if(mib.MenuItems.Count>0) ApplyHotKeys(km, mib.MenuItems);
			}
        }
        #endregion
        #region overrides
        protected override void OnActivated(EventArgs a) {
			base.OnActivated(a);
			if(_firstflag) {
				_firstflag = false; //以降は初回のみ実行

				_multiPaneControl.InitUI(null, GApp.Options); //サイズがフィックスしないとこれは実行できない
				
				//起動時にショートカットを開くには_multiPaneControl.InitUIが先でなければならず、
				//これは自身のウィンドウサイズが必要なのでOnLoadでは早すぎる
				if(_initialAction.ShortcutFile!=null) {
					if(File.Exists(_initialAction.ShortcutFile))
						GApp.GlobalCommandTarget.OpenShortCut(_initialAction.ShortcutFile);
					else
						GUtil.Warning(this, String.Format(GApp.Strings.GetString("Message.GFrame.FailedToOpen"), _initialAction.ShortcutFile));
				}
			}
			else {
				//GApp.GlobalCommandTarget.SetFocusToActiveConnection();
			}
		}
		protected override void OnLoad(EventArgs e) {
			base.OnLoad (e);
			GEnv.InterThreadUIService.MainFrameHandle = this.Handle; //ここでハンドルをセットし、IWin32Windowを他のスレッドがいじらないようにする

			foreach(string m in _initialAction.Messages)
				GUtil.Warning(this, m);
		
			CID cid = GApp.Options.ShowWelcomeDialog? CID.ShowWelcomeDialog : GApp.Options.ActionOnLaunch;
			if(cid!=CID.NOP)
				GApp.GlobalCommandTarget.DelayedExec(cid);
		}

		protected override void OnSizeChanged(EventArgs args) {
			base.OnSizeChanged(args);
			_tabBar.ArrangeButtons();
		}

		protected override void OnClosing(CancelEventArgs args) {
			if(GApp.GlobalCommandTarget.CloseAll()==CommandResult.Cancelled)
				args.Cancel = true;
			else {
				GApp.Options.FramePosition = this.DesktopBounds;
				GApp.Options.FrameState = this.WindowState;
				base.OnClosing(args);
			}
		}

		internal void OnDragEnterInternal(DragEventArgs a) {
			OnDragEnterBody(a);
		}
		internal void OnDragDropInternal(DragEventArgs a) {
			OnDragDropBody(a);
		}

		protected override void OnDragEnter(DragEventArgs a) {
			base.OnDragEnter(a);
			OnDragEnterBody(a);
		}
		protected override void OnDragDrop(DragEventArgs a) {
			base.OnDragDrop(a);
			OnDragDropBody(a);
		}
		private void OnDragEnterBody(DragEventArgs a) {
			if(a.Data.GetDataPresent("FileContents") || a.Data.GetDataPresent("FileDrop"))
				a.Effect = DragDropEffects.Link;
			else
				a.Effect = DragDropEffects.None;
		}
		private void OnDragDropBody(DragEventArgs a) {
			string[] fmts = a.Data.GetFormats();
			
			if(a.Data.GetDataPresent("FileDrop")) {
				string[] files = (string[])a.Data.GetData("FileDrop", true);
				//Debug.WriteLine("files="+files.Length);
				GApp.GlobalCommandTarget.DelayedOpenShortcut(files[0]);
			}
		}

		protected override bool IsInputKey(Keys key) {
			//Debug.WriteLine("Frame IsInputKey "+key);
			return false;
		}
		protected override bool ProcessDialogKey(Keys keyData) {
			//Debug.WriteLine("Frame ProcessDialogKey " + keyData);
			CommandResult r = GApp.Options.Commands.ProcessKey(keyData, GApp.MacroManager.MacroIsRunning);
			return r!=CommandResult.NOP;
        }
        #endregion
        #region menu event handlers
        private void AdjustEditMenu(object sender, System.EventArgs e) {
			_menuCopy.Enabled = _menuCopyAsLook.Enabled = _menuCopyToFile.Enabled = !GEnv.TextSelection.IsEmpty;
			ConnectionTag tag = GEnv.Connections.ActiveTag;
			bool enable = tag!=null;
			IPoderosaTerminalPane p = tag==null? null : tag.AttachedPane;
			_menuPaste.Enabled = CanPaste() && enable && tag.ModalTerminalTask==null;
			_menuPasteFromFile.Enabled = enable;
			_menuClearBuffer.Enabled = enable;
			_menuClearScreen.Enabled = enable;
			_menuSelectAll.Enabled = enable;
			_menuFreeSelectionMode.Enabled = enable;
			_menuFreeSelectionMode.Checked = enable && (p!=null && p.InFreeSelectionMode);
			_menuAutoSelectionMode.Enabled = enable;
			_menuAutoSelectionMode.Checked = enable && (p!=null && p.InAutoSelectionMode);
		}

		private void OnMenu(object sender, EventArgs args) {
			CID cmd = (CID)(((GMenuItem)sender).CID);
			Commands.Entry e = GApp.Options.Commands.FindEntry(cmd);
			if(e==null)
				Debug.WriteLine("Command Entry Not Found: "+cmd);
			else {
				if(GApp.MacroManager.MacroIsRunning && e.CID!=CID.StopMacro) return;

				if(e.Target==Commands.Target.Global)
					GApp.GlobalCommandTarget.Exec(cmd);
				else {
					bool context_menu = ((MenuItem)sender).Parent is ContextMenu;
					ContainerConnectionCommandTarget t = GApp.GetConnectionCommandTarget(context_menu? _commandTargetConnection : GEnv.Connections.ActiveConnection);
					if(t==null) return; //アクティブなコネクションがなければ無視
					t.Exec(cmd);
				}
			}
		}

		private void OnChangeEncoding(object sender, EventArgs args) {
			ContainerConnectionCommandTarget t = GApp.GetConnectionCommandTarget(_commandTargetConnection==null? GEnv.Connections.ActiveConnection : _commandTargetConnection);
			t.SetEncoding(EncodingProfile.Get((EncodingType)((GMenuItem)sender).Index));
		}

		private void OnChangeNewLine(object sender, EventArgs args) {
			ContainerConnectionCommandTarget t = GApp.GetConnectionCommandTarget(_commandTargetConnection==null? GEnv.Connections.ActiveConnection : _commandTargetConnection);
			int i = ((MenuItem)sender).Index;
			Debug.Assert(0<=i && i<=2);
			t.SetTransmitNewLine((NewLine)i);
		}

		private void OnMovePane(object sender, EventArgs args) {
			Keys key;
			if(sender==_menuMovePaneUp)
				key = Keys.Up;
			else if(sender==_menuMovePaneDown)
				key = Keys.Down;
			else if(sender==_menuMovePaneLeft)
				key = Keys.Left;
			else /*if(sender==_menuMovePaneRight)*/
				key = Keys.Right;

			GApp.GlobalCommandTarget.MoveActivePane(key);
		}

		private void OnMRUMenuClicked(object sender, EventArgs args) {
			TerminalParam p = (TerminalParam)_MRUMenuToParameter[sender];
			p = (TerminalParam)p.Clone();
			p.FeedLogOption();
			GApp.GlobalCommandTarget.NewConnection(p);
		}

		private void OnWindowMenuClicked(object sender, EventArgs args) {
			bool e = GEnv.Connections.Count>0;
			_menuCloseAll.Enabled = e;
			_menuCloseAllDisconnected.Enabled = e;
			_menuMovePane.Enabled = e;
			_menuTab.Enabled = e;
			_menuNextTab.Enabled = e;
			_menuPrevTab.Enabled = e;
			_menuMoveTabToPrev.Enabled = e;
			_menuMoveTabToNext.Enabled = e;
		}
		private void OnWindowItemMenuClicked(object sender, EventArgs args) {
			ConnectionTag ct = (ConnectionTag)_windowMenuItemMap[sender];
			GApp.GlobalCommandTarget.ActivateConnection(ct.Connection);
		}

		protected override void WndProc(ref Message msg) {
			base.WndProc(ref msg);
			//ショートカットを開いたときの処理
			if(msg.Msg==Win32.WM_COPYDATA) {
				unsafe {
					Win32.COPYDATASTRUCT* p = (Win32.COPYDATASTRUCT*)msg.LParam.ToPointer();
					if(p!=null && p->dwData==InterProcessService.OPEN_SHORTCUT) {
						string fn = new String((char*)p->lpData);
						msg.Result = new IntPtr(InterProcessService.OPEN_SHORTCUT_OK);
						GApp.GlobalCommandTarget.OpenShortCut(fn);
					}
				}
			}
			else if(msg.Msg==GConst.WMG_UIMESSAGE) {
				//受信スレッドからのUI処理要求
				GApp.InterThreadUIService.ProcessMessage(msg.WParam, msg.LParam);
			}
			else if(msg.Msg==GConst.WMG_DELAYED_COMMAND) {
				GApp.GlobalCommandTarget.DoDelayedExec();
			}
		}

		internal void CenteringDialog(Form frm) {
			frm.Left = this.Left + (this.Width - frm.Width) / 2;
			frm.Top  = this.Top  + (this.Height- frm.Height)/ 2;
		}

		private static bool CanPaste() {
			try {
				IDataObject data = Clipboard.GetDataObject();
				return data!=null && data.GetDataPresent("Text");
			}
			catch(Exception) { //クリップボードをロックしているアプリケーションがあるなどのときは例外になってしまう
				return false;
			}
        }
        #endregion
    }
}
