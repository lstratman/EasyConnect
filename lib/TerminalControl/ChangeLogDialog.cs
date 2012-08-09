/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ChangeLogDialog.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Poderosa;
using EnumDescAttributeT = Poderosa.Toolkit.EnumDescAttribute;
using Poderosa.Communication;
using Poderosa.ConnectionParam;
using Poderosa.Terminal;

namespace Poderosa.Forms
{
	/// <summary>
	/// ChangeLogDialog の概要の説明です。
	/// </summary>
	internal class ChangeLogDialog : System.Windows.Forms.Form
	{
		private TerminalConnection _connection;

		private ComboBox _logTypeBox;
		private System.Windows.Forms.Label _logTypeLabel;
		private ComboBox _fileNameBox;
		private System.Windows.Forms.Label _fileNameLabel;
		private Button _selectlogButton;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ChangeLogDialog(TerminalConnection current)
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();
			this._logTypeLabel.Text = GApp.Strings.GetString("Form.ChangeLog._logTypeLabel");
			this._fileNameLabel.Text = GApp.Strings.GetString("Form.ChangeLog._fileNameLabel");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this._okButton.Text = GApp.Strings.GetString("Common.OK");
			this.Text = GApp.Strings.GetString("Form.ChangeLog.Text");

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			_connection = current;
			_logTypeBox.SelectedIndex = _logTypeBox.FindStringExact(EnumDescAttributeT.For(typeof(LogType)).GetDescription(_connection.LogType));

			if(_connection.LogType!=LogType.None) {
				_fileNameBox.Items.Add(_connection.LogPath);
				_fileNameBox.SelectedIndex = 0;
			}

			foreach(string p in GApp.ConnectionHistory.LogPaths)
				_fileNameBox.Items.Add(p);
			
			
			AdjustUI();
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

		#region Windows Form Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// 消えちゃ困るコードの避難場所
		/// this._logTypeBox.Items.AddRange(EnumDescAttributeT.For(typeof(LogType)).DescriptionCollection());
		/// </summary>
		private void InitializeComponent()
		{
			this._logTypeBox = new ComboBox();
			this._logTypeLabel = new System.Windows.Forms.Label();
			this._fileNameBox = new ComboBox();
			this._fileNameLabel = new System.Windows.Forms.Label();
			this._selectlogButton = new Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _logTypeBox
			// 
			this._logTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._logTypeBox.Location = new System.Drawing.Point(104, 8);
			this._logTypeBox.Name = "_logTypeBox";
			this._logTypeBox.Size = new System.Drawing.Size(96, 20);
			this._logTypeBox.TabIndex = 1;
			this._logTypeBox.SelectedIndexChanged += new System.EventHandler(this.OnLogTypeChanged);
			this._logTypeBox.Items.AddRange(EnumDescAttributeT.For(typeof(LogType)).DescriptionCollection());
			// 
			// _logTypeLabel
			// 
			this._logTypeLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._logTypeLabel.Location = new System.Drawing.Point(5, 8);
			this._logTypeLabel.Name = "_logTypeLabel";
			this._logTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._logTypeLabel.Size = new System.Drawing.Size(80, 16);
			this._logTypeLabel.TabIndex = 0;
			this._logTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _fileNameBox
			// 
			this._fileNameBox.Location = new System.Drawing.Point(104, 32);
			this._fileNameBox.Name = "_fileNameBox";
			this._fileNameBox.Size = new System.Drawing.Size(160, 20);
			this._fileNameBox.TabIndex = 3;
			// 
			// _fileNameLabel
			// 
			this._fileNameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._fileNameLabel.Location = new System.Drawing.Point(5, 32);
			this._fileNameLabel.Name = "_fileNameLabel";
			this._fileNameLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._fileNameLabel.Size = new System.Drawing.Size(88, 16);
			this._fileNameLabel.TabIndex = 2;
			this._fileNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _selectlogButton
			// 
			this._selectlogButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._selectlogButton.ImageIndex = 0;
			this._selectlogButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._selectlogButton.Location = new System.Drawing.Point(269, 32);
			this._selectlogButton.Name = "_selectlogButton";
			this._selectlogButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._selectlogButton.Size = new System.Drawing.Size(19, 19);
			this._selectlogButton.TabIndex = 4;
			this._selectlogButton.Text = "...";
			this._selectlogButton.Click += new System.EventHandler(this.OnSelectLogFile);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.ImageIndex = 0;
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._cancelButton.Location = new System.Drawing.Point(216, 56);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._cancelButton.Size = new System.Drawing.Size(72, 25);
			this._cancelButton.TabIndex = 6;
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.ImageIndex = 0;
			this._okButton.FlatStyle = FlatStyle.System;
			this._okButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._okButton.Location = new System.Drawing.Point(128, 56);
			this._okButton.Name = "_okButton";
			this._okButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._okButton.Size = new System.Drawing.Size(72, 25);
			this._okButton.TabIndex = 5;
			this._okButton.Click += new System.EventHandler(this.OnOK);
			// 
			// ChangeLogDialog
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(292, 85);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._cancelButton,
																		  this._okButton,
																		  this._logTypeBox,
																		  this._logTypeLabel,
																		  this._fileNameBox,
																		  this._fileNameLabel,
																		  this._selectlogButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ChangeLogDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion

		private void AdjustUI() {
			bool e = ((LogType)EnumDescAttributeT.For(typeof(LogType)).FromDescription(_logTypeBox.Text, LogType.None)!=LogType.None);
			_fileNameBox.Enabled = e;
			_selectlogButton.Enabled = e;
		}
		private void OnLogTypeChanged(object sender, EventArgs args) {
			AdjustUI();
		}
		private void OnSelectLogFile(object sender, EventArgs args) {
			string fn = GCUtil.SelectLogFileByDialog(this);
			if(fn!=null) _fileNameBox.Text = fn;
		}
		private void OnOK(object sender, EventArgs args) {
			this.DialogResult = DialogResult.None;
			LogType t = (LogType)EnumDescAttributeT.For(typeof(LogType)).FromDescription(_logTypeBox.Text, LogType.None);
			string path = null;

			bool append = false;
			if(t!=LogType.None) {
				path = _fileNameBox.Text;
				LogFileCheckResult r = GCUtil.CheckLogFileName(path, this);
				if(r==LogFileCheckResult.Cancel || r==LogFileCheckResult.Error) return;
				append = (r==LogFileCheckResult.Append);
			}

			_connection.ResetLog(t, path, append);
			this.DialogResult = DialogResult.OK;
		}
	}
}
