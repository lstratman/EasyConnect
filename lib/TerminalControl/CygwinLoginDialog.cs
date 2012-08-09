/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: CygwinLoginDialog.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;

using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Toolkit;
using EnumDescAttributeT = Poderosa.Toolkit.EnumDescAttribute;
using Poderosa.Communication;
using Poderosa.LocalShell;

namespace Poderosa.Forms
{
	internal class LocalShellLoginDialog : System.Windows.Forms.Form, ISocketWithTimeoutClient
	{
		private string _errorMessage;
		private LocalShellTerminalParam _param;
		private ConnectionTag _result;
		private LocalShellUtil.Connector _connector;
		private IntPtr _savedHWND;

		private System.Windows.Forms.Label _logTypeLabel;
		private ComboBox _logTypeBox;
		private System.Windows.Forms.Label _logFileLabel;
		private ComboBox _logFileBox;
		private Button _selectlogButton;
		private CheckBox _advancedOptionCheck;
		private GroupBox _advancedOptionGroup;
		private System.Windows.Forms.Label _homeDirectoryLabel;
		private System.Windows.Forms.Label _shellLabel;
		private System.Windows.Forms.TextBox _homeDirectoryBox;
		private System.Windows.Forms.TextBox _shellBox;
		private System.Windows.Forms.Label _lMessage;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public LocalShellLoginDialog()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			this._okButton.Text = GApp.Strings.GetString("Common.OK");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this._homeDirectoryLabel.Text = GApp.Strings.GetString("Form.CygwinLoginDialog._homeDirectoryLabel");
			this._lMessage.Text = GApp.Strings.GetString("Form.CygwinLoginDialog._lMessage");
			this._advancedOptionCheck.Text = GApp.Strings.GetString("Form.CygwinLoginDialog._advancedOptionCheck");
			this._shellLabel.Text = GApp.Strings.GetString("Form.CygwinLoginDialog._shellLabel");
			this._logFileLabel.Text = GApp.Strings.GetString("Form.CygwinLoginDialog._logFileLabel");
			this._logTypeLabel.Text = GApp.Strings.GetString("Form.CygwinLoginDialog._logTypeLabel");
		}

		public ConnectionTag Result {
			get {
				return _result;
			}
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
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

		#region Windows フォーム デザイナで生成されたコード 
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this._logTypeBox = new ComboBox();
			this._logTypeLabel = new System.Windows.Forms.Label();
			this._logFileBox = new ComboBox();
			this._logFileLabel = new System.Windows.Forms.Label();
			this._selectlogButton = new Button();
			this._advancedOptionCheck = new CheckBox();
			this._advancedOptionGroup = new GroupBox();
			this._homeDirectoryLabel = new System.Windows.Forms.Label();
			this._homeDirectoryBox = new System.Windows.Forms.TextBox();
			this._shellLabel = new System.Windows.Forms.Label();
			this._shellBox = new System.Windows.Forms.TextBox();
			this._lMessage = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._okButton.Location = new System.Drawing.Point(144, 176);
			this._okButton.Name = "_okButton";
			this._okButton.TabIndex = 0;
			this._okButton.Click += new System.EventHandler(this.OnOK);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._cancelButton.Location = new System.Drawing.Point(232, 176);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.TabIndex = 1;
			// 
			// _logTypeLabel
			// 
			this._logTypeLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._logTypeLabel.Location = new System.Drawing.Point(8, 8);
			this._logTypeLabel.Name = "_logTypeLabel";
			this._logTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._logTypeLabel.Size = new System.Drawing.Size(80, 16);
			this._logTypeLabel.TabIndex = 2;
			this._logTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _logTypeBox
			// 
			this._logTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._logTypeBox.Items.AddRange(EnumDescAttributeT.For(typeof(LogType)).DescriptionCollection());
			this._logTypeBox.Location = new System.Drawing.Point(104, 8);
			this._logTypeBox.Name = "_logTypeBox";
			this._logTypeBox.Size = new System.Drawing.Size(96, 20);
			this._logTypeBox.TabIndex = 3;
			this._logTypeBox.SelectionChangeCommitted += new System.EventHandler(this.OnLogTypeChanged);
			// 
			// _logFileLabel
			// 
			this._logFileLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._logFileLabel.Location = new System.Drawing.Point(8, 32);
			this._logFileLabel.Name = "_logFileLabel";
			this._logFileLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._logFileLabel.Size = new System.Drawing.Size(88, 16);
			this._logFileLabel.TabIndex = 4;
			this._logFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _logFileBox
			// 
			this._logFileBox.Location = new System.Drawing.Point(104, 32);
			this._logFileBox.Name = "_logFileBox";
			this._logFileBox.Size = new System.Drawing.Size(160, 20);
			this._logFileBox.TabIndex = 5;
			// 
			// _selectlogButton
			// 
			this._selectlogButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._selectlogButton.ImageIndex = 0;
			this._selectlogButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._selectlogButton.Location = new System.Drawing.Point(272, 32);
			this._selectlogButton.Name = "_selectlogButton";
			this._selectlogButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._selectlogButton.Size = new System.Drawing.Size(19, 19);
			this._selectlogButton.TabIndex = 6;
			this._selectlogButton.Text = "...";
			this._selectlogButton.Click += new System.EventHandler(this.SelectLog);
			// 
			// _advancedOptionCheck
			// 
			this._advancedOptionCheck.Location = new System.Drawing.Point(20, 56);
			this._advancedOptionCheck.Size = new System.Drawing.Size(152, 20);
			this._advancedOptionCheck.TabIndex = 7;
			this._advancedOptionCheck.FlatStyle = FlatStyle.System;
			this._advancedOptionCheck.CheckedChanged += new EventHandler(OnAdvancedOptionCheckedChanged);
			// 
			// _advancedOptionGroup
			// 
			this._advancedOptionGroup.Location = new System.Drawing.Point(8, 58);
			this._advancedOptionGroup.Size = new System.Drawing.Size(300, 110);
			this._advancedOptionGroup.TabIndex = 8;
			this._advancedOptionGroup.Enabled = false;
			this._advancedOptionGroup.FlatStyle = FlatStyle.System;
			// 
			// _homeDirectoryLabel
			// 
			this._homeDirectoryLabel.Location = new System.Drawing.Point(8, 24);
			this._homeDirectoryLabel.Name = "_homeDirectoryLabel";
			this._homeDirectoryLabel.Size = new System.Drawing.Size(112, 23);
			this._homeDirectoryLabel.TabIndex = 9;
			this._homeDirectoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _homeDirectoryBox
			// 
			this._homeDirectoryBox.Location = new System.Drawing.Point(120, 24);
			this._homeDirectoryBox.Name = "_homeDirectoryBox";
			this._homeDirectoryBox.Size = new System.Drawing.Size(172, 19);
			this._homeDirectoryBox.TabIndex = 10;
			this._homeDirectoryBox.Text = "";
			// 
			// _shellLabel
			// 
			this._shellLabel.Location = new System.Drawing.Point(8, 48);
			this._shellLabel.Name = "_shellLabel";
			this._shellLabel.TabIndex = 11;
			this._shellLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _shellBox
			// 
			this._shellBox.Location = new System.Drawing.Point(120, 48);
			this._shellBox.Name = "_shellBox";
			this._shellBox.Size = new System.Drawing.Size(172, 19);
			this._shellBox.TabIndex = 12;
			this._shellBox.Text = "";
			// 
			// _lMessage
			// 
			this._lMessage.Location = new System.Drawing.Point(8, 72);
			this._lMessage.Name = "_lMessage";
			this._lMessage.Size = new System.Drawing.Size(288, 32);
			this._lMessage.TabIndex = 6;
			this._lMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// CygwinLoginDialog
			// 
			_advancedOptionGroup.Controls.Add(this._lMessage);
			_advancedOptionGroup.Controls.Add(this._shellBox);
			_advancedOptionGroup.Controls.Add(this._shellLabel);
			_advancedOptionGroup.Controls.Add(this._homeDirectoryBox);
			_advancedOptionGroup.Controls.Add(this._homeDirectoryLabel);
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(314, 208);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._logTypeLabel);
			this.Controls.Add(this._logTypeBox);
			this.Controls.Add(this._logFileLabel);
			this.Controls.Add(this._logFileBox);
			this.Controls.Add(this._selectlogButton);
			this.Controls.Add(this._advancedOptionCheck);
			this.Controls.Add(this._advancedOptionGroup);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CygwinLoginDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion

		public void ApplyParam(LocalShellTerminalParam param) {
			_param = param;
			if(param is CygwinTerminalParam)
				this.Text = GApp.Strings.GetString("Form.CygwinLoginDialog.TextCygwin");
			else
				this.Text = GApp.Strings.GetString("Form.CygwinLoginDialog.TextSFU");

		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad (e);
			if(_param==null)
				_param = new CygwinTerminalParam(); //デフォルト値で埋める
			else
				_param = (LocalShellTerminalParam)_param.Clone();
			_param.RenderProfile = null;
			_param.Caption = null;

			_homeDirectoryBox.Text = _param.Home;
			_shellBox.Text = _param.Shell;

			StringCollection c = GApp.ConnectionHistory.LogPaths;
			foreach(string p in c) _logFileBox.Items.Add(p);

			if(GApp.Options.DefaultLogType!=LogType.None) {
				_logTypeBox.SelectedIndex = (int)GApp.Options.DefaultLogType;
				string t = GUtil.CreateLogFileName("cygwin");
				_logFileBox.Items.Add(t);
				_logFileBox.Text = t;
			}
			else
				_logTypeBox.SelectedIndex = 0;

			AdjustUI();
		}

		private void OnOK(object sender, EventArgs args) {
			this.DialogResult = DialogResult.None;
			if(_homeDirectoryBox.Text.Length==0)
				GUtil.Warning(this, GApp.Strings.GetString("Message.CygwinLoginDialog.EmptyHomeDirectory"));
			else if(_shellBox.Text.Length==0)
				GUtil.Warning(this, GApp.Strings.GetString("Message.CygwinLoginDialog.EmptyShell"));

			_param.LogType = (LogType)EnumDescAttributeT.For(typeof(LogType)).FromDescription(_logTypeBox.Text, LogType.None);
			if(_param.LogType!=LogType.None) {
				_param.LogPath = _logFileBox.Text;
				LogFileCheckResult r = GCUtil.CheckLogFileName(_param.LogPath, this);
				if(r==LogFileCheckResult.Cancel || r==LogFileCheckResult.Error) return;
				_param.LogAppend = (r==LogFileCheckResult.Append);
			}

			_param.Home = _homeDirectoryBox.Text;
			_param.Shell = _shellBox.Text;

			_okButton.Enabled = false;
			_cancelButton.Enabled = false;
			this.Cursor = Cursors.WaitCursor;
			_savedHWND = this.Handle;
			if(_param is CygwinTerminalParam)
				this.Text = GApp.Strings.GetString("Caption.CygwinLoginDialog.ConnectingCygwin");
			else
				this.Text = GApp.Strings.GetString("Caption.CygwinLoginDialog.ConnectingSFU");

			_connector = CygwinUtil.AsyncPrepareSocket(this, _param);
			if(_connector==null) ClearConnectingState();
		}

		private void OnAdvancedOptionCheckedChanged(object sender, EventArgs args) {
			_advancedOptionGroup.Enabled = _advancedOptionCheck.Checked;
		}
		private void OnLogTypeChanged(object sender, System.EventArgs args) {
			AdjustUI();
		}
		private void AdjustUI() {
			bool e = _logTypeBox.SelectedIndex!=(int)LogType.None;
			_logFileBox.Enabled = e;
			_selectlogButton.Enabled = e;
		}
		private void SelectLog(object sender, System.EventArgs e) {
			string fn = GCUtil.SelectLogFileByDialog(this);
			if(fn!=null) _logFileBox.Text = fn;
		}

		private void ShowError(string msg) {
			GUtil.Warning(this, msg, GApp.Strings.GetString("Message.CygwinLoginDialog.ConnectionError"));
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
			_okButton.Enabled = true;
			_cancelButton.Enabled = true;
			this.Cursor = Cursors.Default;
			this.Text = GApp.Strings.GetString("Form.CygwinLoginDialog.Text");
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
		//ISocketWithTimeoutClient これらはこのウィンドウとは別のスレッドで実行されるので慎重に
		public void SuccessfullyExit(object result) {
			_result = (ConnectionTag)result;
			//_result.SetServerInfo(((TCPTerminalParam)_result.Param).Host, swt.IPAddress);
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
