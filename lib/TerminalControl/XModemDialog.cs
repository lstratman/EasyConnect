/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: XModemDialog.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Communication;
using Poderosa.Connection;
using Poderosa.Terminal;

namespace Poderosa.Forms
{
	/// <summary>
	/// XModemDialog の概要の説明です。
	/// </summary>
	internal class XModemDialog : System.Windows.Forms.Form
	{
		private bool _receiving; //受信ならtrue,送信ならfalse これは表示前にのみ設定可能
		private bool _executing;
		private ConnectionTag _connectionTag;
		private XModem _xmodemTask;

		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Label _fileNameLabel;
		private System.Windows.Forms.TextBox _fileNameBox;
		private System.Windows.Forms.Button _selectButton;
		private System.Windows.Forms.Label _progressText;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public XModemDialog()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			ReloadLanguage();
		}
		public bool Receiving {
			get {
				return _receiving;
			}
			set {
				_receiving = value;
			}
		}
		public bool Executing {
			get {
				return _executing;
			}
		}
		public ConnectionTag ConnectionTag {
			get {
				return _connectionTag;
			}
			set {
				if(_executing) throw new Exception("illegal!");
				_connectionTag = value;
				FormatText();
			}
		}
		public void ReloadLanguage() {
			this._okButton.Text = GApp.Strings.GetString("Form.XModemDialog._okButton");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this._fileNameLabel.Text = GApp.Strings.GetString("Form.XModemDialog._fileNameLabel");
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
			this._fileNameLabel = new System.Windows.Forms.Label();
			this._fileNameBox = new System.Windows.Forms.TextBox();
			this._selectButton = new System.Windows.Forms.Button();
			this._progressText = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._okButton.Location = new System.Drawing.Point(152, 64);
			this._okButton.Name = "_okButton";
			this._okButton.TabIndex = 0;
			this._okButton.Click += new EventHandler(OnOK);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._cancelButton.Location = new System.Drawing.Point(240, 64);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.TabIndex = 1;
			this._cancelButton.Click += new EventHandler(OnCancel);
			// 
			// _fileNameLabel
			// 
			this._fileNameLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._fileNameLabel.Location = new System.Drawing.Point(8, 8);
			this._fileNameLabel.Name = "_fileNameLabel";
			this._fileNameLabel.Size = new System.Drawing.Size(80, 16);
			this._fileNameLabel.TabIndex = 2;
			// 
			// _fileNameBox
			// 
			this._fileNameBox.Location = new System.Drawing.Point(96, 8);
			this._fileNameBox.Name = "_fileNameBox";
			this._fileNameBox.Size = new System.Drawing.Size(192, 19);
			this._fileNameBox.TabIndex = 3;
			this._fileNameBox.Text = "";
			// 
			// _selectButton
			// 
			this._selectButton.Location = new System.Drawing.Point(296, 8);
			this._selectButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._selectButton.Name = "_selectButton";
			this._selectButton.Size = new System.Drawing.Size(19, 19);
			this._selectButton.TabIndex = 4;
			this._selectButton.Text = "...";
			this._selectButton.Click += new EventHandler(OnSelectFile);
			// 
			// _progressText
			// 
			this._progressText.Location = new System.Drawing.Point(8, 28);
			this._progressText.Name = "_progressText";
			this._progressText.Size = new System.Drawing.Size(296, 32);
			this._progressText.TabIndex = 5;
			this._progressText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// XModemDialog
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(328, 86);
			this.Controls.Add(this._progressText);
			this.Controls.Add(this._selectButton);
			this.Controls.Add(this._fileNameBox);
			this.Controls.Add(this._fileNameLabel);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "XModemDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.ResumeLayout(false);

		}
		#endregion

		private void FormatText() {
			this.Text = String.Format(GApp.Strings.GetString("Caption.XModemDialog.DialogTitle"), GApp.Strings.GetString(_receiving? "Common.Reception" : "Common.Transmission"), _connectionTag.FormatTabText());
			this._progressText.Text = String.Format(GApp.Strings.GetString("Caption.XModemDialog.InitialPrompt"), GApp.Strings.GetString(_receiving? "Common.Transmission" : "Common.Reception").ToLower());
		}
		private void OnSelectFile(object sender, EventArgs args) {
			FileDialog dlg = null;
			if(_receiving) {
				SaveFileDialog sf = new SaveFileDialog();
				sf.Title = GApp.Strings.GetString("Caption.XModemDialog.ReceptionFileSelect");
				dlg = sf;
			}
			else {
				OpenFileDialog of = new OpenFileDialog();
				of.Title = GApp.Strings.GetString("Caption.XModemDialog.TransmissionFileSelect");
				of.CheckFileExists = true;
				of.Multiselect = false;
				dlg = of;
			}
			dlg.Filter = "All Files(*)|*";
			if(GCUtil.ShowModalDialog(this, dlg)==DialogResult.OK)
				_fileNameBox.Text = dlg.FileName;
		}

		private void OnOK(object sedner, EventArgs args) {
			Debug.Assert(!_executing);
			this.DialogResult = DialogResult.None;
			if(_receiving) {
				if(!StartReceive()) return;
			}
			else {
				if(!StartSend()) return;
			}

			_executing = true;
			_okButton.Enabled = false;
			_fileNameBox.Enabled = false;
			_selectButton.Enabled = false;
			_progressText.Text = GApp.Strings.GetString("Caption.XModemDialog.Negotiating");
		}
		private bool StartReceive() {
			try {
				_xmodemTask = new XModemReceiver(_connectionTag, _fileNameBox.Text);
				_xmodemTask.NotifyTarget = this.Handle;
				_xmodemTask.Start();
				return true;
			}
			catch(Exception ex) {
				GUtil.Warning(this, ex.Message);
				return false;
			}
		}
		private bool StartSend() {
			try {
				_xmodemTask = new XModemSender(_connectionTag, _fileNameBox.Text);
				_xmodemTask.NotifyTarget = this.Handle;
				_xmodemTask.Start();
				return true;
			}
			catch(Exception ex) {
				GUtil.Warning(this, ex.Message);
				return false;
			}
		}

		private void Exit() {
			if(_xmodemTask!=null) {
				_xmodemTask.Abort();
				_xmodemTask = null;
			}
			_executing = false;
			_okButton.Enabled = true;
			_fileNameBox.Enabled = true;
			_selectButton.Enabled = true;
		}

		private void OnCancel(object sender, EventArgs args) {
			if(_executing)
				Exit();
			else
				Close();
		}
		protected override void OnClosed(EventArgs e) {
			base.OnClosed (e);
			if(_executing) Exit();
			GApp.Frame.XModemDialog = null;
		}

		private void UpdateStatusText(int wparam, int lparam) {
			if(wparam==XModem.NOTIFY_PROGRESS) {
				if(_receiving)
					_progressText.Text = String.Format(GApp.Strings.GetString("Caption.XModemDialog.ReceptionProgress"), lparam);
				else
					_progressText.Text = String.Format(GApp.Strings.GetString("Caption.XModemDialog.TransmissionProgress"), lparam);
			}
			else {
				//PROGRESS以外は単に閉じる。ダイアログボックスの表示などはプロトコル実装側がやる
				this.DialogResult = DialogResult.Abort;
				this._progressText.Text = String.Format(GApp.Strings.GetString("Caption.XModemDialog.InitialPrompt"), GApp.Strings.GetString(_receiving? "Common.Transmission" : "Common.Reception").ToLower());
				Exit();
				if(wparam==XModem.NOTIFY_SUCCESS)
					Close();
			}

		}

		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
			if(m.Msg==GConst.WMG_XMODEM_UPDATE_STATUS)
				UpdateStatusText(m.WParam.ToInt32(), m.LParam.ToInt32());
		}

	}
}
