/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ChangePassphrase.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

using Granados.SSHCV2;

namespace Poderosa.Forms
{
	/// <summary>
	/// ChangePassphrase の概要の説明です。
	/// </summary>
	internal class ChangePassphrase : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label _lKeyFile;
		private TextBox _tKeyFile;
		private System.Windows.Forms.Button _selectKeyFile;
		private System.Windows.Forms.Label _lCurrentPassphrase;
		private TextBox _tCurrentPassphrase;
		private System.Windows.Forms.Label _lNewPassphrase;
		private TextBox _tNewPassphrase;
		private System.Windows.Forms.Label _lNewPassphraseAgain;
		private TextBox _tNewPassphraseAgain;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ChangePassphrase()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			this._lKeyFile.Text = GApp.Strings.GetString("Form.ChangePassphrase._lKeyFile");
			this._lCurrentPassphrase.Text = GApp.Strings.GetString("Form.ChangePassphrase._lCurrentPassphrase");
			this._lNewPassphrase.Text = GApp.Strings.GetString("Form.ChangePassphrase._lNewPassphrase");
			this._lNewPassphraseAgain.Text = GApp.Strings.GetString("Form.ChangePassphrase._lNewPassphraseAgain");
			this._okButton.Text = GApp.Strings.GetString("Common.OK");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this.Text = GApp.Strings.GetString("Form.ChangePassphrase.Text");
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
		/// </summary>
		private void InitializeComponent()
		{
			this._lKeyFile = new System.Windows.Forms.Label();
			this._tKeyFile = new TextBox();
			this._selectKeyFile = new System.Windows.Forms.Button();
			this._tCurrentPassphrase = new TextBox();
			this._lCurrentPassphrase = new System.Windows.Forms.Label();
			this._tNewPassphrase = new TextBox();
			this._lNewPassphrase = new System.Windows.Forms.Label();
			this._tNewPassphraseAgain = new TextBox();
			this._lNewPassphraseAgain = new System.Windows.Forms.Label();
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _lKeyFile
			// 
			this._lKeyFile.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._lKeyFile.Location = new System.Drawing.Point(8, 8);
			this._lKeyFile.Name = "_lKeyFile";
			this._lKeyFile.Size = new System.Drawing.Size(112, 16);
			this._lKeyFile.TabIndex = 0;
			// 
			// _tKeyFile
			// 
			this._tKeyFile.Location = new System.Drawing.Point(136, 8);
			this._tKeyFile.Name = "_tKeyFile";
			this._tKeyFile.Size = new System.Drawing.Size(120, 19);
			this._tKeyFile.TabIndex = 1;
			// 
			// _selectKeyFile
			// 
			this._selectKeyFile.Location = new System.Drawing.Point(264, 8);
			this._selectKeyFile.Name = "_selectKeyFile";
			this._selectKeyFile.FlatStyle = FlatStyle.System;
			this._selectKeyFile.Size = new System.Drawing.Size(19, 19);
			this._selectKeyFile.TabIndex = 2;
			this._selectKeyFile.Text = "...";
			this._selectKeyFile.Click += new EventHandler(OpenKeyFile);
			// 
			// _lCurrentPassphrase
			// 
			this._lCurrentPassphrase.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._lCurrentPassphrase.Location = new System.Drawing.Point(8, 32);
			this._lCurrentPassphrase.Name = "_lCurrentPassphrase";
			this._lCurrentPassphrase.Size = new System.Drawing.Size(120, 16);
			this._lCurrentPassphrase.TabIndex = 3;
			// 
			// _tCurrentPassphrase
			// 
			this._tCurrentPassphrase.Location = new System.Drawing.Point(136, 32);
			this._tCurrentPassphrase.Name = "_tCurrentPassphrase";
			this._tCurrentPassphrase.PasswordChar = '*';
			this._tCurrentPassphrase.Size = new System.Drawing.Size(152, 19);
			this._tCurrentPassphrase.TabIndex = 4;
			// 
			// _lNewPassphrase
			// 
			this._lNewPassphrase.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._lNewPassphrase.Location = new System.Drawing.Point(8, 56);
			this._lNewPassphrase.Name = "_lNewPassphrase";
			this._lNewPassphrase.Size = new System.Drawing.Size(120, 16);
			this._lNewPassphrase.TabIndex = 5;
			// 
			// _tNewPassphrase
			// 
			this._tNewPassphrase.Location = new System.Drawing.Point(136, 56);
			this._tNewPassphrase.Name = "_tNewPassphrase";
			this._tNewPassphrase.PasswordChar = '*';
			this._tNewPassphrase.Size = new System.Drawing.Size(152, 19);
			this._tNewPassphrase.TabIndex = 6;
			// 
			// _lNewPassphraseAgain
			// 
			this._lNewPassphraseAgain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._lNewPassphraseAgain.Location = new System.Drawing.Point(8, 80);
			this._lNewPassphraseAgain.Name = "_lNewPassphraseAgain";
			this._lNewPassphraseAgain.Size = new System.Drawing.Size(120, 16);
			this._lNewPassphraseAgain.TabIndex = 7;
			// 
			// _tNewPassphraseAgain
			// 
			this._tNewPassphraseAgain.Location = new System.Drawing.Point(136, 80);
			this._tNewPassphraseAgain.Name = "_tNewPassphraseAgain";
			this._tNewPassphraseAgain.PasswordChar = '*';
			this._tNewPassphraseAgain.Size = new System.Drawing.Size(152, 19);
			this._tNewPassphraseAgain.TabIndex = 8;
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.Location = new System.Drawing.Point(152, 112);
			this._okButton.Name = "_okButton";
			this._okButton.FlatStyle = FlatStyle.System;
			this._okButton.Size = new System.Drawing.Size(64, 24);
			this._okButton.TabIndex = 9;
			this._okButton.Click += new EventHandler(OnOK);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(224, 112);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.Size = new System.Drawing.Size(64, 23);
			this._cancelButton.TabIndex = 10;
			// 
			// ChangePassphrase
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(292, 141);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._cancelButton,
																		  this._okButton,
																		  this._tNewPassphraseAgain,
																		  this._lNewPassphraseAgain,
																		  this._tNewPassphrase,
																		  this._lNewPassphrase,
																		  this._tCurrentPassphrase,
																		  this._lCurrentPassphrase,
																		  this._selectKeyFile,
																		  this._tKeyFile,
																		  this._lKeyFile});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ChangePassphrase";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion

		private void OpenKeyFile(object sender, EventArgs args) {
			string fn = GCUtil.SelectPrivateKeyFileByDialog(this);
			if(fn!=null) _tKeyFile.Text = fn;
		}

		private void OnOK(object sender, EventArgs args) {
			this.DialogResult = DialogResult.None;
			
			try {
				SSH2UserAuthKey key = SSH2UserAuthKey.FromSECSHStyleFile(_tKeyFile.Text, _tCurrentPassphrase.Text);
				if(_tNewPassphrase.Text!=_tNewPassphraseAgain.Text)
					GUtil.Warning(this, GApp.Strings.GetString("Message.ChangePassphrase.PassphraseMismatch"));
				else {
					if(_tNewPassphrase.Text.Length>0 || GUtil.AskUserYesNo(this, GApp.Strings.GetString("Message.ChangePassphrase.AskEmptyPassphrase"))==DialogResult.Yes) {
						FileStream s = new FileStream(_tKeyFile.Text, FileMode.Create);
						key.WritePrivatePartInSECSHStyleFile(s, "", _tNewPassphrase.Text);
						s.Close();
						GUtil.Warning(this, GApp.Strings.GetString("Message.ChangePassphrase.NotifyChanged"), MessageBoxIcon.Information);
						this.DialogResult = DialogResult.OK;
						this.Close();
					}
				}
			}
			catch(Exception ex) {
				GUtil.Warning(this, ex.Message);
			}
		}
	}
}
