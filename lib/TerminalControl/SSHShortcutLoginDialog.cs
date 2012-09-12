/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: SSHShortcutLoginDialog.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

using Granados.SSHC;
using Poderosa.Toolkit;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using EnumDescAttributeT = Poderosa.Toolkit.EnumDescAttribute;
using Poderosa.Terminal;
using Poderosa.Communication;
using Poderosa.SSH;

namespace Poderosa.Forms
{
	internal class SSHShortcutLoginDialog : System.Windows.Forms.Form, ISocketWithTimeoutClient
    {
        #region fields
        private SSHTerminalParam _terminalParam;
		private ConnectionTag _result;
		private SocketWithTimeout _connector;
		private string _errorMessage;

		private TextBox _privateKeyBox;
		private TextBox _passphraseBox;
		private Button _privateKeySelect;
		private System.Windows.Forms.Label _hostLabel;
		private System.Windows.Forms.Label _hostBox;
		private System.Windows.Forms.Label _methodLabel;
		private System.Windows.Forms.Label _methodBox;
		private System.Windows.Forms.Label _accountLabel;
		private System.Windows.Forms.Label _accountBox;
		private System.Windows.Forms.Label _authTypeLabel;
		private System.Windows.Forms.Label _authTypeBox;
		private System.Windows.Forms.Label _encodingLabel;
		private System.Windows.Forms.Label _encodingBox;
		private ComboBox _logFileBox;
		private Button _selectlogButton;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _loginButton;
		private System.Windows.Forms.Label _privateKeyLabel;
		private System.Windows.Forms.Label _passphraseLabel;
		private System.Windows.Forms.Label _logFileLabel;
		private ComboBox _logTypeBox;
		private System.Windows.Forms.Label _logTypeLabel;
        #endregion

		private System.ComponentModel.Container components = null;
		public SSHShortcutLoginDialog(SSHTerminalParam param)
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			this._privateKeyLabel.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog._privateKeyLabel");
			this._passphraseLabel.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog._passphraseLabel");
			this._logFileLabel.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog._logFileLabel");
			this._hostLabel.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog._hostLabel");
			this._methodLabel.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog._methodLabel");
			this._accountLabel.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog._accountLabel");
			this._authTypeLabel.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog._authTypeLabel");
			this._encodingLabel.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog._encodingLabel");
			this._logTypeLabel.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog._logTypeLabel");
			this.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog.Text");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this._loginButton.Text = GApp.Strings.GetString("Common.OK");

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			_terminalParam = param;
			InitUI();
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
		public ConnectionTag Result {
			get {
				return _result;
			}
		}
		#region Windows Form Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// this._logTypeBox.Items.AddRange(EnumDescAttributeT.For(typeof(LogType)).DescriptionCollection());
		/// </summary>
		private void InitializeComponent()
		{
			this._privateKeyBox = new TextBox();
			this._privateKeyLabel = new System.Windows.Forms.Label();
			this._passphraseBox = new TextBox();
			this._passphraseLabel = new System.Windows.Forms.Label();
			this._privateKeySelect = new Button();
			this._logFileBox = new ComboBox();
			this._logFileLabel = new System.Windows.Forms.Label();
			this._selectlogButton = new Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this._loginButton = new System.Windows.Forms.Button();
			this._hostLabel = new System.Windows.Forms.Label();
			this._hostBox = new System.Windows.Forms.Label();
			this._methodLabel = new System.Windows.Forms.Label();
			this._methodBox = new System.Windows.Forms.Label();
			this._accountLabel = new System.Windows.Forms.Label();
			this._accountBox = new System.Windows.Forms.Label();
			this._authTypeLabel = new System.Windows.Forms.Label();
			this._authTypeBox = new System.Windows.Forms.Label();
			this._encodingLabel = new System.Windows.Forms.Label();
			this._encodingBox = new System.Windows.Forms.Label();
			this._logTypeBox = new ComboBox();
			this._logTypeLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _privateKeyBox
			// 
			this._privateKeyBox.Location = new System.Drawing.Point(104, 128);
			this._privateKeyBox.Name = "_privateKeyBox";
			this._privateKeyBox.Size = new System.Drawing.Size(160, 19);
			this._privateKeyBox.TabIndex = 3;
			this._privateKeyBox.Text = "";
			// 
			// _privateKeyLabel
			// 
			this._privateKeyLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._privateKeyLabel.Location = new System.Drawing.Point(8, 128);
			this._privateKeyLabel.Name = "_privateKeyLabel";
			this._privateKeyLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._privateKeyLabel.Size = new System.Drawing.Size(72, 16);
			this._privateKeyLabel.TabIndex = 2;
			this._privateKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _passphraseBox
			// 
			this._passphraseBox.Location = new System.Drawing.Point(104, 104);
			this._passphraseBox.Name = "_passphraseBox";
			this._passphraseBox.PasswordChar = '*';
			this._passphraseBox.Size = new System.Drawing.Size(184, 19);
			this._passphraseBox.TabIndex = 1;
			this._passphraseBox.Text = "";
			// 
			// _passphraseLabel
			// 
			this._passphraseLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._passphraseLabel.Location = new System.Drawing.Point(8, 104);
			this._passphraseLabel.Name = "_passphraseLabel";
			this._passphraseLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._passphraseLabel.Size = new System.Drawing.Size(80, 16);
			this._passphraseLabel.TabIndex = 0;
			this._passphraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _privateKeySelect
			// 
			this._privateKeySelect.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._privateKeySelect.ImageIndex = 0;
			this._privateKeySelect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._privateKeySelect.Location = new System.Drawing.Point(272, 128);
			this._privateKeySelect.Name = "_privateKeySelect";
			this._privateKeySelect.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._privateKeySelect.Size = new System.Drawing.Size(19, 19);
			this._privateKeySelect.TabIndex = 4;
			this._privateKeySelect.Text = "...";
			this._privateKeySelect.Click += new System.EventHandler(this.OnOpenPrivateKey);
			// 
			// _logFileBox
			// 
			this._logFileBox.Location = new System.Drawing.Point(104, 176);
			this._logFileBox.Name = "_logFileBox";
			this._logFileBox.Size = new System.Drawing.Size(160, 20);
			this._logFileBox.TabIndex = 8;
			// 
			// _logFileLabel
			// 
			this._logFileLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._logFileLabel.Location = new System.Drawing.Point(8, 176);
			this._logFileLabel.Name = "_logFileLabel";
			this._logFileLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._logFileLabel.Size = new System.Drawing.Size(88, 16);
			this._logFileLabel.TabIndex = 7;
			this._logFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _selectlogButton
			// 
			this._selectlogButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._selectlogButton.ImageIndex = 0;
			this._selectlogButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._selectlogButton.Location = new System.Drawing.Point(272, 176);
			this._selectlogButton.Name = "_selectlogButton";
			this._selectlogButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._selectlogButton.Size = new System.Drawing.Size(19, 19);
			this._selectlogButton.TabIndex = 9;
			this._selectlogButton.Text = "...";
			this._selectlogButton.Click += new System.EventHandler(this.SelectLog);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.ImageIndex = 0;
			this._cancelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._cancelButton.Location = new System.Drawing.Point(216, 208);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._cancelButton.Size = new System.Drawing.Size(72, 25);
			this._cancelButton.TabIndex = 11;
			// 
			// _loginButton
			// 
			this._loginButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._loginButton.ImageIndex = 0;
			this._loginButton.FlatStyle = FlatStyle.System;
			this._loginButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._loginButton.Location = new System.Drawing.Point(128, 208);
			this._loginButton.Name = "_loginButton";
			this._loginButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._loginButton.Size = new System.Drawing.Size(72, 25);
			this._loginButton.TabIndex = 10;
			this._loginButton.Click += new System.EventHandler(this.OnOK);
			// 
			// _hostLabel
			// 
			this._hostLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._hostLabel.Location = new System.Drawing.Point(8, 8);
			this._hostLabel.Name = "_hostLabel";
			this._hostLabel.Size = new System.Drawing.Size(80, 16);
			this._hostLabel.TabIndex = 0;
			this._hostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _hostBox
			// 
			this._hostBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._hostBox.Location = new System.Drawing.Point(104, 8);
			this._hostBox.Name = "_hostBox";
			this._hostBox.Size = new System.Drawing.Size(144, 16);
			this._hostBox.TabIndex = 35;
			this._hostBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _methodLabel
			// 
			this._methodLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._methodLabel.Location = new System.Drawing.Point(8, 24);
			this._methodLabel.Name = "_methodLabel";
			this._methodLabel.Size = new System.Drawing.Size(80, 16);
			this._methodLabel.TabIndex = 0;
			this._methodLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _methodBox
			// 
			this._methodBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._methodBox.Location = new System.Drawing.Point(104, 24);
			this._methodBox.Name = "_methodBox";
			this._methodBox.Size = new System.Drawing.Size(144, 16);
			this._methodBox.TabIndex = 0;
			this._methodBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _accountLabel
			// 
			this._accountLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._accountLabel.Location = new System.Drawing.Point(8, 40);
			this._accountLabel.Name = "_accountLabel";
			this._accountLabel.Size = new System.Drawing.Size(80, 16);
			this._accountLabel.TabIndex = 0;
			this._accountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _accountBox
			// 
			this._accountBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._accountBox.Location = new System.Drawing.Point(104, 40);
			this._accountBox.Name = "_accountBox";
			this._accountBox.Size = new System.Drawing.Size(144, 16);
			this._accountBox.TabIndex = 0;
			this._accountBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _authTypeLabel
			// 
			this._authTypeLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._authTypeLabel.Location = new System.Drawing.Point(8, 56);
			this._authTypeLabel.Name = "_authTypeLabel";
			this._authTypeLabel.Size = new System.Drawing.Size(80, 16);
			this._authTypeLabel.TabIndex = 0;
			this._authTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _authTypeBox
			// 
			this._authTypeBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._authTypeBox.Location = new System.Drawing.Point(104, 56);
			this._authTypeBox.Name = "_authTypeBox";
			this._authTypeBox.Size = new System.Drawing.Size(144, 16);
			this._authTypeBox.TabIndex = 0;
			this._authTypeBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _encodingLabel
			// 
			this._encodingLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._encodingLabel.Location = new System.Drawing.Point(8, 72);
			this._encodingLabel.Name = "_encodingLabel";
			this._encodingLabel.Size = new System.Drawing.Size(80, 16);
			this._encodingLabel.TabIndex = 0;
			this._encodingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _encodingBox
			// 
			this._encodingBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._encodingBox.Location = new System.Drawing.Point(104, 72);
			this._encodingBox.Name = "_encodingBox";
			this._encodingBox.Size = new System.Drawing.Size(144, 16);
			this._encodingBox.TabIndex = 0;
			this._encodingBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _logTypeBox
			// 
			this._logTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._logTypeBox.Items.AddRange(EnumDescAttributeT.For(typeof(LogType)).DescriptionCollection());
			this._logTypeBox.Location = new System.Drawing.Point(104, 152);
			this._logTypeBox.Name = "_logTypeBox";
			this._logTypeBox.Size = new System.Drawing.Size(96, 20);
			this._logTypeBox.TabIndex = 6;
			this._logTypeBox.SelectionChangeCommitted += new System.EventHandler(this.OnLogTypeChanged);
			// 
			// _logTypeLabel
			// 
			this._logTypeLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._logTypeLabel.Location = new System.Drawing.Point(8, 152);
			this._logTypeLabel.Name = "_logTypeLabel";
			this._logTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._logTypeLabel.Size = new System.Drawing.Size(80, 16);
			this._logTypeLabel.TabIndex = 5;
			this._logTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SSHShortcutLoginDialog
			// 
			this.AcceptButton = this._loginButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(298, 239);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._logTypeBox,
																		  this._logTypeLabel,
																		  this._cancelButton,
																		  this._loginButton,
																		  this._logFileBox,
																		  this._logFileLabel,
																		  this._selectlogButton,
																		  this._hostLabel,
																		  this._hostBox,
																		  this._methodLabel,
																		  this._methodBox,
																		  this._accountLabel,
																		  this._accountBox,
																		  this._authTypeLabel,
																		  this._authTypeBox,
																		  this._encodingLabel,
																		  this._encodingBox,
																		  this._privateKeyBox,
																		  this._privateKeyLabel,
																		  this._passphraseBox,
																		  this._passphraseLabel,
																		  this._privateKeySelect});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SSHShortcutLoginDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion
		private void InitUI() {
			_hostBox.Text = _terminalParam.Host;
			_methodBox.Text = _terminalParam.Method.ToString();
			if(_terminalParam.Port!=22) _methodBox.Text += String.Format(GApp.Strings.GetString("Caption.SSHShortcutLoginDialog.NotStandardPort"), _terminalParam.Port);
			_accountBox.Text = _terminalParam.Account;
			_authTypeBox.Text = EnumDescAttributeT.For(typeof(AuthType)).GetDescription(_terminalParam.AuthType);
			_encodingBox.Text = EnumDescAttributeT.For(typeof(EncodingType)).GetDescription(_terminalParam.EncodingProfile.Type);
			
			if(_terminalParam.AuthType==AuthType.Password) {
				_privateKeyBox.Enabled = false;
				_privateKeySelect.Enabled = false;
			}
			else if(_terminalParam.AuthType==AuthType.PublicKey) {
				_privateKeyBox.Text = _terminalParam.IdentityFile;
			}
			else if(_terminalParam.AuthType==AuthType.KeyboardInteractive) {
				_privateKeyBox.Enabled = false;
				_privateKeySelect.Enabled = false;
				_passphraseBox.Enabled = false;
			}

			_passphraseBox.Text = _terminalParam.Passphrase;

			StringCollection c = GApp.ConnectionHistory.LogPaths;
			foreach(string p in c) _logFileBox.Items.Add(p);

			if(GApp.Options.DefaultLogType!=LogType.None) {
				_logTypeBox.SelectedIndex = (int)GApp.Options.DefaultLogType;
				string t = GUtil.CreateLogFileName(_terminalParam.Host);
				_logFileBox.Items.Add(t);
				_logFileBox.Text = t;
			}
			else
				_logTypeBox.SelectedIndex = 0;

			AdjustUI();
		}
		private void AdjustUI() {
			_passphraseBox.Enabled = _terminalParam.AuthType!=AuthType.KeyboardInteractive;
			
			bool e = _logTypeBox.SelectedIndex!=(int)LogType.None;
			_logFileBox.Enabled = e;
			_selectlogButton.Enabled = e;
		}

		private void OnOK(object sender, System.EventArgs e) {
			this.DialogResult = DialogResult.None;
			TCPTerminalParam param = ValidateContent();
			if(param==null) return;  //パラメータに誤りがあれば即脱出

			_loginButton.Enabled = false;
			_cancelButton.Enabled = false;
			this.Cursor = Cursors.WaitCursor;
			this.Text = GApp.Strings.GetString("Caption.SSHShortcutLoginDialog.Connecting");

			HostKeyCheckCallback checker = new HostKeyCheckCallback(new HostKeyChecker(this, (SSHTerminalParam)param).CheckHostKeyCallback);
			_connector = CommunicationUtil.StartNewConnection(this, param, _passphraseBox.Text, checker);
			if(_connector==null) ClearConnectingState();
		}
		private SSHTerminalParam ValidateContent() {
			SSHTerminalParam p = _terminalParam;
			string msg = null;

			try {
				p.LogType = (LogType)EnumDescAttributeT.For(typeof(LogType)).FromDescription(_logTypeBox.Text, LogType.None);
				if(p.LogType!=LogType.None) {
					p.LogPath = _logFileBox.Text;
					LogFileCheckResult r = GCUtil.CheckLogFileName(p.LogPath, this);
					if(r==LogFileCheckResult.Cancel || r==LogFileCheckResult.Error) return null;
					p.LogAppend = (r==LogFileCheckResult.Append);
				}

				if(p.AuthType==AuthType.PublicKey) {
					if(!File.Exists(_privateKeyBox.Text))
						msg = GApp.Strings.GetString("Message.SSHShortcutLoginDialog.KeyFileNotExist");
					else
						p.IdentityFile = _privateKeyBox.Text;
				}


				if(msg!=null) {
					GUtil.Warning(this, msg);
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
		private void OnOpenPrivateKey(object sender, System.EventArgs e) {
			string fn = GCUtil.SelectPrivateKeyFileByDialog(this);
			if(fn!=null) _privateKeyBox.Text = fn;
		}
		private void SelectLog(object sender, System.EventArgs e) {
			string fn = GCUtil.SelectLogFileByDialog(this);
			if(fn!=null) _logFileBox.Text = fn;
		}
		private void OnLogTypeChanged(object sender, System.EventArgs args) {
			AdjustUI();
		}
		private void ShowError(string msg) {
			GUtil.Warning(this, msg, GApp.Strings.GetString("Message.SSHShortcutLoginDialog.ConnectionError"));
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
			this.Text = GApp.Strings.GetString("Form.SSHShortcutLoginDialog.Text");
			_connector = null;
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
		public void SuccessfullyExit(object result) {
			_result = (ConnectionTag)result;
			//_result.SetServerInfo(((TCPTerminalParam)_result.Param).Host, swt.IPAddress);
			Win32.SendMessage(this.Handle, GConst.WMG_ASYNCCONNECT, IntPtr.Zero, new IntPtr(1));
		}
		public void ConnectionFailed(string message) {
			_errorMessage = message;
			Win32.SendMessage(this.Handle, GConst.WMG_ASYNCCONNECT, IntPtr.Zero, IntPtr.Zero);
		}
		public void CancelTimer() {
		}
		public IWin32Window GetWindow() {
			return this;
		}

	}
}
