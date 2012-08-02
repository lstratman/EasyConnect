/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: LoginDialog.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

using Poderosa.Toolkit;
using EnumDescAttributeT = Poderosa.Toolkit.EnumDescAttribute;
using Poderosa.Communication;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Terminal;
using Poderosa.Config;
using Poderosa.SSH;
using Poderosa.UI;

using Granados.SSHC;


namespace Poderosa.Forms
{
	internal class LoginDialog : System.Windows.Forms.Form, ISocketWithTimeoutClient
    {
        #region fields
        private string _errorMessage;
		private bool _initializing;
		private bool _firstFlag;
		private ConnectionHistory _history;
		private ConnectionTag _result;
		private SocketWithTimeout _connector;
		private IntPtr _savedHWND;

		private System.ComponentModel.Container components = null;

		private System.Windows.Forms.Label _hostLabel;
		public ComboBox _hostBox;
		private System.Windows.Forms.Label _portLabel;
        public ComboBox _portBox;
		private System.Windows.Forms.Label _methodLabel;
        public ComboBox _methodBox;

		private System.Windows.Forms.GroupBox _sshGroup;
		private System.Windows.Forms.Label _usernameLabel;
        public ComboBox _userNameBox;
		private System.Windows.Forms.Label _authenticationLabel;
        public ComboBox _authOptions;
		private System.Windows.Forms.Label _passphraseLabel;
        public TextBox _passphraseBox;
		private System.Windows.Forms.Label _privateKeyLabel;
        public TextBox _privateKeyFile;
		private Button _privateKeySelect;

		private System.Windows.Forms.GroupBox _terminalGroup;
        public ComboBox _encodingBox;
		private System.Windows.Forms.Label _encodingLabel;
		private System.Windows.Forms.Label _logFileLabel;
        public ComboBox _logFileBox;
		private Button _selectLogButton;
		private System.Windows.Forms.Label _newLineLabel;
		private System.Windows.Forms.Label _localEchoLabel;
        public ComboBox _localEchoBox;
        public ComboBox _newLineBox;
		private System.Windows.Forms.Label _logTypeLabel;
        public ComboBox _logTypeBox;
        public ComboBox _terminalTypeBox;
		private System.Windows.Forms.Label _terminalTypeLabel;

		private Button _loginButton;
		private Button _cancelButton;
        #endregion
        public LoginDialog()
		{
			_firstFlag = true;
			_initializing = true;
			_history = GApp.ConnectionHistory;
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();
			InitializeText();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			InitializeLoginParams();
			_initializing = false;
		}
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
        #region Windows Form Designer generated code
		private void InitializeComponent()
		{
			this._loginButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this._sshGroup = new System.Windows.Forms.GroupBox();

			this._hostLabel = new System.Windows.Forms.Label();
			this._hostBox = new ComboBox();
			this._methodLabel = new System.Windows.Forms.Label();
			this._methodBox = new ComboBox();
			this._portLabel = new System.Windows.Forms.Label();
			this._portBox = new ComboBox();

			this._authenticationLabel = new System.Windows.Forms.Label();
			this._authOptions = new ComboBox();
			this._passphraseLabel = new System.Windows.Forms.Label();
			this._passphraseBox = new TextBox();
			this._privateKeyLabel = new System.Windows.Forms.Label();
			this._privateKeyFile = new TextBox();
			this._privateKeySelect = new Button();
			this._usernameLabel = new System.Windows.Forms.Label();
			this._userNameBox = new ComboBox();

			this._terminalGroup = new System.Windows.Forms.GroupBox();
			this._newLineBox = new ComboBox();
			this._localEchoBox = new ComboBox();
			this._localEchoLabel = new System.Windows.Forms.Label();
			this._newLineLabel = new System.Windows.Forms.Label();
			this._logFileBox = new ComboBox();
			this._logFileLabel = new System.Windows.Forms.Label();
			this._encodingBox = new ComboBox();
			this._encodingLabel = new System.Windows.Forms.Label();
			this._selectLogButton = new Button();
			this._logTypeLabel = new System.Windows.Forms.Label();
			this._logTypeBox = new ComboBox();
			this._terminalTypeBox = new ComboBox();
			this._terminalTypeLabel = new Label();

			this._sshGroup.SuspendLayout();
			this._terminalGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// _hostLabel
			// 
			this._hostLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._hostLabel.Location = new System.Drawing.Point(16, 12);
			this._hostLabel.Name = "_hostLabel";
			this._hostLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._hostLabel.Size = new System.Drawing.Size(80, 16);
			this._hostLabel.TabIndex = 0;
			this._hostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _hostBox
			// 
			this._hostBox.Location = new System.Drawing.Point(104, 8);
			this._hostBox.Name = "_hostBox";
			this._hostBox.Size = new System.Drawing.Size(208, 20);
			this._hostBox.TabIndex = 1;
			this._hostBox.SelectedIndexChanged += new System.EventHandler(this.OnHostIsSelected);
			// 
			// _methodLabel
			// 
			this._methodLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._methodLabel.Location = new System.Drawing.Point(16, 36);
			this._methodLabel.Name = "_methodLabel";
			this._methodLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._methodLabel.Size = new System.Drawing.Size(80, 16);
			this._methodLabel.TabIndex = 2;
			this._methodLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _methodBox
			// 
			this._methodBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._methodBox.Items.AddRange(new object[] { "Telnet",
															"SSH1",
															"SSH2"
															});
			this._methodBox.Location = new System.Drawing.Point(104, 32);
			this._methodBox.Name = "_methodBox";
			this._methodBox.Size = new System.Drawing.Size(208, 20);
			this._methodBox.TabIndex = 3;
			this._methodBox.SelectedIndexChanged += new System.EventHandler(this.AdjustConnectionUI);
			// 
			// _portLabel
			// 
			this._portLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._portLabel.Location = new System.Drawing.Point(16, 60);
			this._portLabel.Name = "_portLabel";
			this._portLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._portLabel.Size = new System.Drawing.Size(80, 16);
			this._portLabel.TabIndex = 4;
			this._portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _portBox
			// 
			this._portBox.Location = new System.Drawing.Point(104, 56);
			this._portBox.Name = "_portBox";
			this._portBox.Size = new System.Drawing.Size(208, 20);
			this._portBox.TabIndex = 5;
			// 
			// _sshGroup
			// 
			this._sshGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this._privateKeyFile,
																					this._authOptions,
																					this._privateKeyLabel,
																					this._passphraseBox,
																					this._userNameBox,
																					this._authenticationLabel,
																					this._passphraseLabel,
																					this._usernameLabel,
																					this._privateKeySelect});
			this._sshGroup.Location = new System.Drawing.Point(8, 88);
			this._sshGroup.Name = "_sshGroup";
			this._sshGroup.FlatStyle = FlatStyle.System;
			this._sshGroup.Size = new System.Drawing.Size(312, 112);
			this._sshGroup.TabIndex = 6;
			this._sshGroup.TabStop = false;
			// 
			// _usernameLabel
			// 
			this._usernameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._usernameLabel.Location = new System.Drawing.Point(8, 16);
			this._usernameLabel.Name = "_usernameLabel";
			this._usernameLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._usernameLabel.Size = new System.Drawing.Size(80, 16);
			this._usernameLabel.TabIndex = 7;
			this._usernameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _userNameBox
			// 
			this._userNameBox.Location = new System.Drawing.Point(96, 16);
			this._userNameBox.Name = "_userNameBox";
			this._userNameBox.Size = new System.Drawing.Size(200, 20);
			this._userNameBox.TabIndex = 8;
			// 
			// _authenticationLabel
			// 
			this._authenticationLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._authenticationLabel.Location = new System.Drawing.Point(8, 40);
			this._authenticationLabel.Name = "_authenticationLabel";
			this._authenticationLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._authenticationLabel.Size = new System.Drawing.Size(80, 16);
			this._authenticationLabel.TabIndex = 9;
			this._authenticationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _authOptions
			// 
			this._authOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._authOptions.Location = new System.Drawing.Point(96, 40);
			this._authOptions.Name = "_authOptions";
			this._authOptions.Size = new System.Drawing.Size(200, 20);
			this._authOptions.TabIndex = 10;
			this._authOptions.SelectedIndexChanged += new System.EventHandler(this.AdjustAuthenticationUI);
			// 
			// _passphraseLabel
			// 
			this._passphraseLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._passphraseLabel.Location = new System.Drawing.Point(8, 64);
			this._passphraseLabel.Name = "_passphraseLabel";
			this._passphraseLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._passphraseLabel.Size = new System.Drawing.Size(80, 16);
			this._passphraseLabel.TabIndex = 11;
			this._passphraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _passphraseBox
			// 
			this._passphraseBox.Location = new System.Drawing.Point(96, 64);
			this._passphraseBox.Name = "_passphraseBox";
			this._passphraseBox.PasswordChar = '*';
			this._passphraseBox.Size = new System.Drawing.Size(200, 19);
			this._passphraseBox.TabIndex = 12;
			this._passphraseBox.Text = "";
			// 
			// _privateKeyLabel
			// 
			this._privateKeyLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._privateKeyLabel.Location = new System.Drawing.Point(8, 88);
			this._privateKeyLabel.Name = "_privateKeyLabel";
			this._privateKeyLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._privateKeyLabel.Size = new System.Drawing.Size(72, 16);
			this._privateKeyLabel.TabIndex = 14;
			this._privateKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _privateKeyFile
			// 
			this._privateKeyFile.Location = new System.Drawing.Point(96, 88);
			this._privateKeyFile.Name = "_privateKeyFile";
			this._privateKeyFile.Size = new System.Drawing.Size(176, 19);
			this._privateKeyFile.TabIndex = 15;
			this._privateKeyFile.Text = "";
			// 
			// _privateKeySelect
			// 
			this._privateKeySelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._privateKeySelect.ImageIndex = 0;
			this._privateKeySelect.FlatStyle = FlatStyle.System;
			this._privateKeySelect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._privateKeySelect.Location = new System.Drawing.Point(272, 88);
			this._privateKeySelect.Name = "_privateKeySelect";
			this._privateKeySelect.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._privateKeySelect.Size = new System.Drawing.Size(19, 19);
			this._privateKeySelect.TabIndex = 16;
			this._privateKeySelect.Text = "...";
			this._privateKeySelect.Click += new System.EventHandler(this.OnOpenPrivateKey);
			// 
			// _terminalGroup
			// 
			this._terminalGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this._logTypeBox,
																						 this._logTypeLabel,
																						 this._newLineBox,
																						 this._localEchoBox,
																						 this._localEchoLabel,
																						 this._newLineLabel,
																						 this._logFileBox,
																						 this._logFileLabel,
																						 this._encodingBox,
																						 this._encodingLabel,
																						 this._selectLogButton, this._terminalTypeLabel, this._terminalTypeBox});
			this._terminalGroup.Location = new System.Drawing.Point(8, 208);
			this._terminalGroup.Name = "_terminalGroup";
			this._terminalGroup.FlatStyle = FlatStyle.System;
			this._terminalGroup.Size = new System.Drawing.Size(312, 168);
			this._terminalGroup.TabIndex = 17;
			this._terminalGroup.TabStop = false;
			// 
			// _logTypeLabel
			// 
			this._logTypeLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._logTypeLabel.Location = new System.Drawing.Point(8, 16);
			this._logTypeLabel.Name = "_logTypeLabel";
			this._logTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._logTypeLabel.Size = new System.Drawing.Size(96, 16);
			this._logTypeLabel.TabIndex = 18;
			this._logTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _logTypeBox
			// 
			this._logTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._logTypeBox.Items.AddRange(EnumDescAttributeT.For(typeof(LogType)).DescriptionCollection());
			this._logTypeBox.Location = new System.Drawing.Point(112, 16);
			this._logTypeBox.Name = "_logTypeBox";
			this._logTypeBox.Size = new System.Drawing.Size(96, 20);
			this._logTypeBox.TabIndex = 19;
			this._logTypeBox.SelectionChangeCommitted += new EventHandler(OnLogTypeChanged);
			// 
			// _logFileLabel
			// 
			this._logFileLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._logFileLabel.Location = new System.Drawing.Point(8, 40);
			this._logFileLabel.Name = "_logFileLabel";
			this._logFileLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._logFileLabel.Size = new System.Drawing.Size(88, 16);
			this._logFileLabel.TabIndex = 20;
			this._logFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _logFileBox
			// 
			this._logFileBox.Location = new System.Drawing.Point(112, 40);
			this._logFileBox.Name = "_logFileBox";
			this._logFileBox.Size = new System.Drawing.Size(160, 20);
			this._logFileBox.TabIndex = 21;
			// 
			// _selectLogButton
			// 
			this._selectLogButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._selectLogButton.ImageIndex = 0;
			this._selectLogButton.FlatStyle = FlatStyle.System;
			this._selectLogButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._selectLogButton.Location = new System.Drawing.Point(272, 40);
			this._selectLogButton.Name = "_selectLogButton";
			this._selectLogButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._selectLogButton.Size = new System.Drawing.Size(19, 19);
			this._selectLogButton.TabIndex = 22;
			this._selectLogButton.Text = "...";
			this._selectLogButton.Click += new System.EventHandler(this.SelectLog);
			// 
			// _encodingLabel
			// 
			this._encodingLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._encodingLabel.Location = new System.Drawing.Point(8, 64);
			this._encodingLabel.Name = "_encodingLabel";
			this._encodingLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._encodingLabel.Size = new System.Drawing.Size(96, 16);
			this._encodingLabel.TabIndex = 23;
			this._encodingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _encodingBox
			// 
			this._encodingBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._encodingBox.Items.AddRange(EnumDescAttributeT.For(typeof(EncodingType)).DescriptionCollection());
			this._encodingBox.Location = new System.Drawing.Point(112, 64);
			this._encodingBox.Name = "_encodingBox";
			this._encodingBox.Size = new System.Drawing.Size(96, 20);
			this._encodingBox.TabIndex = 24;
			// 
			// _localEchoLabel
			// 
			this._localEchoLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._localEchoLabel.Location = new System.Drawing.Point(8, 88);
			this._localEchoLabel.Name = "_localEchoLabel";
			this._localEchoLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._localEchoLabel.Size = new System.Drawing.Size(96, 16);
			this._localEchoLabel.TabIndex = 25;
			this._localEchoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _localEchoBox
			// 
			this._localEchoBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._localEchoBox.Items.AddRange(new object[] {
															   GApp.Strings.GetString("Common.DoNot"),
															   GApp.Strings.GetString("Common.Do")});
			this._localEchoBox.Location = new System.Drawing.Point(112, 88);
			this._localEchoBox.Name = "_localEchoBox";
			this._localEchoBox.Size = new System.Drawing.Size(96, 20);
			this._localEchoBox.TabIndex = 26;
			// 
			// _newLineLabel
			// 
			this._newLineLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._newLineLabel.Location = new System.Drawing.Point(8, 112);
			this._newLineLabel.Name = "_newLineLabel";
			this._newLineLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._newLineLabel.Size = new System.Drawing.Size(96, 16);
			this._newLineLabel.TabIndex = 27;
			this._newLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _newLineBox
			// 
			this._newLineBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._newLineBox.Items.AddRange(EnumDescAttributeT.For(typeof(NewLine)).DescriptionCollection());
			this._newLineBox.Location = new System.Drawing.Point(112, 112);
			this._newLineBox.Name = "_newLineBox";
			this._newLineBox.Size = new System.Drawing.Size(96, 20);
			this._newLineBox.TabIndex = 28;
			// 
			// _terminalTypeLabel
			// 
			this._terminalTypeLabel.Location = new System.Drawing.Point(8, 136);
			this._terminalTypeLabel.Name = "_terminalTypeLabel";
			this._terminalTypeLabel.Size = new System.Drawing.Size(96, 23);
			this._terminalTypeLabel.TabIndex = 29;
			this._terminalTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _terminalTypeBox
			// 
			this._terminalTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._terminalTypeBox.Items.AddRange(EnumDescAttributeT.For(typeof(TerminalType)).DescriptionCollection());
			this._terminalTypeBox.Location = new System.Drawing.Point(112, 136);
			this._terminalTypeBox.Name = "_terminalType";
			this._terminalTypeBox.Size = new System.Drawing.Size(96, 20);
			this._terminalTypeBox.TabIndex = 30;
			// 
			// _loginButton
			// 
			this._loginButton.ImageIndex = 0;
			this._loginButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._loginButton.Location = new System.Drawing.Point(160, 384);
			this._loginButton.Name = "_loginButton";
			this._loginButton.FlatStyle = FlatStyle.System;
			this._loginButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._loginButton.Size = new System.Drawing.Size(72, 25);
			this._loginButton.TabIndex = 29;
			this._loginButton.Click += new System.EventHandler(this.OnOK);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.ImageIndex = 0;
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._cancelButton.Location = new System.Drawing.Point(248, 384);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._cancelButton.Size = new System.Drawing.Size(72, 25);
			this._cancelButton.TabIndex = 30;
			// 
			// LoginDialog
			// 
			this.AcceptButton = this._loginButton;
			this.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(330, 415);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._terminalGroup,
																		  this._sshGroup,
																		  this._hostBox,
																		  this._methodBox,
																		  this._portBox,
																		  this._cancelButton,
																		  this._loginButton,
																		  this._methodLabel,
																		  this._portLabel,
																		  this._hostLabel});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LoginDialog";
			this.ShowInTaskbar = false;
			//this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this._sshGroup.ResumeLayout(false);
			this._terminalGroup.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
		private void InitializeText() {
			this._hostLabel.Text = GApp.Strings.GetString("Form.LoginDialog._hostLabel");
			this._portLabel.Text = GApp.Strings.GetString("Form.LoginDialog._portLabel");
			this._methodLabel.Text = GApp.Strings.GetString("Form.LoginDialog._methodLabel");
			this._sshGroup.Text = GApp.Strings.GetString("Form.LoginDialog._sshGroup");
			this._privateKeyLabel.Text = GApp.Strings.GetString("Form.LoginDialog._privateKeyLabel");
			this._authenticationLabel.Text = GApp.Strings.GetString("Form.LoginDialog._authenticationLabel");
			this._passphraseLabel.Text = GApp.Strings.GetString("Form.LoginDialog._passphraseLabel");
			this._usernameLabel.Text = GApp.Strings.GetString("Form.LoginDialog._usernameLabel");
			this._terminalGroup.Text = GApp.Strings.GetString("Form.LoginDialog._terminalGroup");
			this._localEchoLabel.Text = GApp.Strings.GetString("Form.LoginDialog._localEchoLabel");
			this._newLineLabel.Text = GApp.Strings.GetString("Form.LoginDialog._newLineLabel");
			this._logFileLabel.Text = GApp.Strings.GetString("Form.LoginDialog._logFileLabel");
			this._encodingLabel.Text = GApp.Strings.GetString("Form.LoginDialog._encodingLabel");
			this._logTypeLabel.Text = GApp.Strings.GetString("Form.LoginDialog._logTypeLabel");
			this._terminalTypeLabel.Text = GApp.Strings.GetString("Form.LoginDialog._terminalTypeLabel");
			this._loginButton.Text = GApp.Strings.GetString("Common.OK");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this.Text = GApp.Strings.GetString("Form.LoginDialog.Text");

			_authOptions.Items.AddRange(EnumDescAttributeT.For(typeof(AuthType)).DescriptionCollection());
		}
		private void AdjustConnectionUI(object sender, System.EventArgs e) {
			if(_initializing) return;
			if(_methodBox.Text=="Telnet") {
				_portBox.SelectedIndex = 0; //Telnet:23
			}
			else {
				_portBox.SelectedIndex = 1; //SSH:22
				if(_authOptions.SelectedIndex==-1) _authOptions.SelectedIndex=0;
			}
			EnableValidControls();
		}
		private void AdjustAuthenticationUI(object sender, System.EventArgs e) {
			EnableValidControls();
		}
		private void InitializeLoginParams() {
			StringCollection c = _history.Hosts;
			foreach(string h in c) _hostBox.Items.Add(h);
			if(_hostBox.Items.Count>0) _hostBox.SelectedIndex = 0;

			c = _history.Accounts;
			foreach(string a in c) _userNameBox.Items.Add(a);
			if(_userNameBox.Items.Count>0) _userNameBox.SelectedIndex = 0;

			int[] ic = _history.Ports;
			foreach(int p in ic) _portBox.Items.Add(PortDescription(p));

			if(_hostBox.Items.Count > 0) {
				TCPTerminalParam last = _history.SearchByHost((string)_hostBox.Items[0]);
				if(last!=null) ApplyParam(last);
			}

			c = _history.LogPaths;
			foreach(string p in c) _logFileBox.Items.Add(p);

			if(GApp.Options.DefaultLogType!=LogType.None) {
				_logTypeBox.SelectedIndex = (int)GApp.Options.DefaultLogType;
				string t = GUtil.CreateLogFileName(null);
				_logFileBox.Items.Add(t);
				_logFileBox.Text = t;
			}
			else
				_logTypeBox.SelectedIndex = 0;

		}
		private void EnableValidControls() {
			bool ssh = _methodBox.Text!="Telnet";
			bool pubkey = _authOptions.SelectedIndex==(int)AuthType.PublicKey;
			bool kbd = _authOptions.SelectedIndex==(int)AuthType.KeyboardInteractive;

			_userNameBox.Enabled = ssh;
			_authOptions.Enabled = ssh;
			_passphraseBox.Enabled = ssh && (pubkey || !kbd);
			_privateKeyFile.Enabled = ssh && pubkey;
			_privateKeySelect.Enabled = ssh && pubkey;
			
			bool e = ((LogType)EnumDescAttributeT.For(typeof(LogType)).FromDescription(_logTypeBox.Text, LogType.None)!=LogType.None);
			_logFileBox.Enabled = e;
			_selectLogButton.Enabled = e;
		}
		
		public void ApplyParam(TCPTerminalParam param) {
			_initializing = true;
			_methodBox.SelectedIndex = (int)param.Method;
			_portBox.SelectedIndex = _portBox.FindStringExact(PortDescription(param.Port));
			_methodBox.SelectedIndex = _methodBox.FindStringExact(param.Method.ToString());
			if(param.IsSSH) {
				SSHTerminalParam sp = (SSHTerminalParam)param;
				_userNameBox.SelectedIndex = _userNameBox.FindStringExact(sp.Account);
				_passphraseBox.Text = sp.Passphrase;
				
				if(sp.AuthType==AuthType.PublicKey)
					_privateKeyFile.Text = sp.IdentityFile;
				else
					_privateKeyFile.Text = "";
				_authOptions.SelectedIndex = (int)sp.AuthType;
			}

			_encodingBox.SelectedIndex = (int)param.EncodingProfile.Type;
			_newLineBox.SelectedIndex = (int)param.TransmitNL;
			_localEchoBox.SelectedIndex = param.LocalEcho? 1 : 0;
			_terminalTypeBox.SelectedIndex = (int)param.TerminalType;
			_initializing = false;
			
			EnableValidControls();
		}

		public ConnectionTag Result {
			get {
				return _result;
			}
		}

		private void OnHostIsSelected(object sender, System.EventArgs e) {
			if(_initializing) return;
			string host = _hostBox.Text;
			TCPTerminalParam param = _history.SearchByHost(host);
			Debug.Assert(param!=null);
			ApplyParam(param);
		}

		private static int ParsePort(string text) {
			//頻出のやつ
			if(text.IndexOf("(22)")!=-1)
				return 22;
			if(text.IndexOf("(23)")!=-1)
				return 23;
			
			try {
				return Int32.Parse(text);
			}
			catch(FormatException) {
				throw new FormatException(String.Format(GApp.Strings.GetString("Message.LoginDialog.InvalidPort"), text));
			}
		}
		private static ConnectionMethod ParseMethod(string text) {
			if(text.IndexOf("SSH1")!=-1)
				return ConnectionMethod.SSH1;
			else if(text.IndexOf("SSH2")!=-1)
				return ConnectionMethod.SSH2;
			else if(text.IndexOf("Telnet")!=-1)
				return ConnectionMethod.Telnet;
			else
				throw new ArgumentException("unknown method "+text);
		}

		private static string PortDescription(int port) {
			if(port==22)
				return "SSH(22)";
			else if(port==23)
				return "Telnet(23)";
			else
				return port.ToString();
		}


		private void OnOpenPrivateKey(object sender, System.EventArgs e) {
			string fn = GCUtil.SelectPrivateKeyFileByDialog(this);
			if(fn!=null) _privateKeyFile.Text = fn;
			_privateKeySelect.Focus(); //どっちにしても次のフォーカスは鍵選択ボタンへ
		}

		public void OnOK(object sender, System.EventArgs e) {
			this.DialogResult = DialogResult.None;
			TCPTerminalParam param = ValidateContent();
			if(param==null) return;  //パラメータに誤りがあれば即脱出

			_loginButton.Enabled = false;
			_cancelButton.Enabled = false;
			this.Cursor = Cursors.WaitCursor;
			this.Text = GApp.Strings.GetString("Caption.LoginDialog.Connecting");
			_savedHWND = this.Handle;

			HostKeyCheckCallback checker = null;
			if(param.IsSSH)
				checker = new HostKeyCheckCallback(new HostKeyChecker(this, (SSHTerminalParam)param).CheckHostKeyCallback);

			_connector = CommunicationUtil.StartNewConnection(this, param, _passphraseBox.Text, checker);
			if(_connector==null) ClearConnectingState();
		}

		//入力内容に誤りがあればそれを警告してnullを返す。なければ必要なところを埋めたTCPTerminalParamを返す
		private TCPTerminalParam ValidateContent() {
			string msg = null;
			TCPTerminalParam p = null;
			SSHTerminalParam sp = null;
			try {
				ConnectionMethod m = ParseMethod(_methodBox.Text);
				if(m==ConnectionMethod.Telnet)
					p = new TelnetTerminalParam("");
				else {
					p = sp = new SSHTerminalParam(ConnectionMethod.SSH2, "", "", "");
					sp.Method = m;
					sp.Account = _userNameBox.Text;
				}

				p.Host = _hostBox.Text;
				try {
					p.Port = ParsePort(_portBox.Text);
				}
				catch(FormatException ex) {
					msg = ex.Message;
				}

				if(_hostBox.Text.Length==0)
					msg = GApp.Strings.GetString("Message.LoginDialog.HostIsEmpty");

				p.LogType = (LogType)EnumDescAttributeT.For(typeof(LogType)).FromDescription(_logTypeBox.Text, LogType.None);

                if(p.LogType!=LogType.None) {
					p.LogPath = _logFileBox.Text;
					if(p.LogPath==GUtil.CreateLogFileName(null)) p.LogPath = GUtil.CreateLogFileName(_hostBox.Text);
					LogFileCheckResult r = GCUtil.CheckLogFileName(p.LogPath, this);
					if(r==LogFileCheckResult.Cancel || r==LogFileCheckResult.Error) return null;
					p.LogAppend = (r==LogFileCheckResult.Append);
				}

				if(p.IsSSH) {
					Debug.Assert(sp!=null);
					sp.AuthType = (AuthType)_authOptions.SelectedIndex;
					if(sp.AuthType==AuthType.PublicKey) {
						if(!File.Exists(_privateKeyFile.Text))
							msg = GApp.Strings.GetString("Message.LoginDialog.KeyFileNotExist");
						else
							sp.IdentityFile = _privateKeyFile.Text;
					}
				}
				p.EncodingProfile = EncodingProfile.Get((EncodingType)_encodingBox.SelectedIndex);

				p.LocalEcho = _localEchoBox.SelectedIndex==1;
				p.TransmitNL = (NewLine)EnumDescAttributeT.For(typeof(NewLine)).FromDescription(_newLineBox.Text, NewLine.CR);
				p.TerminalType = (TerminalType)_terminalTypeBox.SelectedIndex;

				if(msg!=null) {
					ShowError(msg);
					return null;
				}
				else
					return p;
			}
			catch(Exception ex) {
				GUtil.Warning(this, ex.Message);
				return null;
			}

		}

		protected override void OnActivated(EventArgs args) {
			if(_firstFlag) {
				_firstFlag = false;
				_hostBox.Focus();
			}
		}

		private void SelectLog(object sender, System.EventArgs e) {
			string fn = GCUtil.SelectLogFileByDialog(this);
			if(fn!=null) _logFileBox.Text = fn;
		}
		private void OnLogTypeChanged(object sender, System.EventArgs e) {
			if(_initializing) return;
			EnableValidControls();
		}

		protected override bool ProcessDialogKey(Keys key) {
			if(_connector!=null && key==(Keys.Control | Keys.C)) {
				_connector.Interrupt();
				ClearConnectingState();
				return true;
			}
			else
				return base.ProcessDialogKey(key);
		}
		private void ClearConnectingState() {
			_loginButton.Enabled = true;
			_cancelButton.Enabled = true;
			this.Cursor = Cursors.Default;
			this.Text = GApp.Strings.GetString("Form.LoginDialog.Text");
			_connector = null;
		}

		private void ShowError(string msg) {
			GUtil.Warning(this, msg, GApp.Strings.GetString("Caption.LoginDialog.ConnectionError"));
		}

		protected override void WndProc(ref Message msg) {
			base.WndProc(ref msg);
			if(msg.Msg==GConst.WMG_ASYNCCONNECT) {
				if(msg.LParam.ToInt32()==1) {
					this.DialogResult = DialogResult.OK;
					Close();
				}
				else {
					ClearConnectingState();
					ShowError(_errorMessage);
				}
			}
		}
		//ISocketWithTimeoutClient これらはこのウィンドウとは別のスレッドで実行されるので慎重に
		public void SuccessfullyExit(object result) {
			_result = (ConnectionTag)result;
			Win32.SendMessage(_savedHWND, GConst.WMG_ASYNCCONNECT, IntPtr.Zero, new IntPtr(1));
		}
		public void ConnectionFailed(string message) {
			_errorMessage = message;
			Win32.SendMessage(_savedHWND, GConst.WMG_ASYNCCONNECT, IntPtr.Zero, IntPtr.Zero);
		}
		public void CancelTimer() {
		}
		public IWin32Window GetWindow() {
			return this;
		}
	}
}
