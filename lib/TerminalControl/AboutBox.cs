/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: AboutBox.cs,v 1.2 2005/04/20 08:44:12 okajima Exp $
*/
using System;
using System.Diagnostics;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Poderosa.Forms
{
	/// <summary>
	/// AboutBox の概要の説明です。
	/// </summary>
	internal class AboutBox : System.Windows.Forms.Form
	{
		//private Image _bgImage;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.TextBox _versionText;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.PictureBox _pictureBox;
		private System.Windows.Forms.PictureBox _guevaraPicture;
		private System.Windows.Forms.Button _creditButton;

		private bool _guevaraMode;

		public AboutBox()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			this.Text = GApp.Strings.GetString("Form.AboutBox.Text");
			_okButton.Text = GApp.Strings.GetString("Common.OK");
			_creditButton.Text = GApp.Strings.GetString("Form.AboutBox._creditButton");
			_guevaraMode = GApp.Options.GuevaraMode;

			//Guevara Mode
			if(_guevaraMode) {
				System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AboutBox));
				this._creditButton.Visible = false;
				this._okButton.Location = new System.Drawing.Point(160, 216);
				this._versionText.BackColor = System.Drawing.Color.White;
				this._versionText.Location = new System.Drawing.Point(152, 8);
				this._pictureBox.Visible = false;
				this._guevaraPicture.Visible = true;
				this._guevaraPicture.Location = new Point(0,8);
				this._guevaraPicture.Size = new Size(280, 200);
				this.BackColor = System.Drawing.Color.White;
				this.ClientSize = new System.Drawing.Size(418, 240);
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

		#region Windows Form Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AboutBox));
			this._okButton = new System.Windows.Forms.Button();
			this._versionText = new System.Windows.Forms.TextBox();
			this._pictureBox = new System.Windows.Forms.PictureBox();
			this._guevaraPicture = new System.Windows.Forms.PictureBox();
			this._creditButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _okButton
			// 
			this._okButton.BackColor = System.Drawing.SystemColors.Control;
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._okButton.Location = new System.Drawing.Point(176, 192);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(88, 23);
			this._okButton.TabIndex = 0;
			// 
			// _versionText
			// 
			this._versionText.BackColor = System.Drawing.SystemColors.Window;
			this._versionText.Location = new System.Drawing.Point(0, 88);
			this._versionText.Multiline = true;
			this._versionText.Name = "_versionText";
			this._versionText.ReadOnly = true;
			this._versionText.Size = new System.Drawing.Size(280, 96);
			this._versionText.TabIndex = 2;
			this._versionText.Text = "";
			// 
			// _pictureBox
			// 
			this._pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("_pictureBox.Image")));
			this._pictureBox.Location = new System.Drawing.Point(0, 0);
			this._pictureBox.Name = "_pictureBox";
			this._pictureBox.Size = new System.Drawing.Size(280, 88);
			this._pictureBox.TabIndex = 3;
			this._pictureBox.TabStop = false;
			// 
			// _guevaraPicture
			// 
			this._guevaraPicture.Image = ((System.Drawing.Image)(resources.GetObject("_guevaraPicture.Image")));
			this._guevaraPicture.Location = new System.Drawing.Point(0, 216);
			this._guevaraPicture.Name = "_guevaraPicture";
			this._guevaraPicture.Size = new System.Drawing.Size(285, 400);
			this._guevaraPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this._guevaraPicture.TabIndex = 4;
			this._guevaraPicture.TabStop = false;
			this._guevaraPicture.Visible = false;
			// 
			// _creditButton
			// 
			this._creditButton.BackColor = System.Drawing.SystemColors.Control;
			this._creditButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._creditButton.Location = new System.Drawing.Point(8, 192);
			this._creditButton.Name = "_creditButton";
			this._creditButton.Size = new System.Drawing.Size(88, 23);
			this._creditButton.TabIndex = 5;
			this._creditButton.Click += new EventHandler(OnCreditButton);
			// 
			// AboutBox
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._okButton;
			this.ClientSize = new System.Drawing.Size(282, 224);
			this.Controls.Add(this._creditButton);
			this.Controls.Add(this._pictureBox);
			this.Controls.Add(this._versionText);
			this.Controls.Add(this._guevaraPicture);
			this.Controls.Add(this._okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutBox";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Load += new System.EventHandler(this.OnLoad);
			this.ResumeLayout(false);

		}
		#endregion

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint (e);
		}


		private void OnLoad(object sender, System.EventArgs e) {
			Assembly asm = Assembly.GetExecutingAssembly();
			string[] s = new string[5];
			s[0] = "Terminal Emulator <Poderosa>";
			s[1] = "Copyright(c) 2005 Poderosa Project. All Rights Reserved.";
			s[2] = "";
			object[] t = asm.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
			s[3] = " Version : " + ((AssemblyDescriptionAttribute)t[0]).Description;
			s[4] = " CLR     : " + System.Environment.Version.ToString();
			_versionText.Lines = s;
		}


		private int _guevaraIndex;
		private const string _guevaraString = "guevara";

		protected override bool ProcessDialogChar(char charCode) {
			if('A'<=charCode && charCode<='Z') charCode = (char)('a'+charCode-'A');
			if(charCode==_guevaraString[_guevaraIndex]) {
				if(++_guevaraIndex==_guevaraString.Length) {
					if(!GApp.Options.GuevaraMode)
						GUtil.Warning(this, GApp.Strings.GetString("Message.AboutBox.EnterGuevara"));
					else
						GUtil.Warning(this, GApp.Strings.GetString("Message.AboutBox.ExitGuevara"));
					GApp.Options.GuevaraMode = !GApp.Options.GuevaraMode;
					GApp.Frame.ReloadIcon();
					this.Close();
				}
			}
			else
				_guevaraIndex = 0;
			return base.ProcessDialogChar (charCode);
		}

		private bool _credit;
		private void OnCreditButton(object sender, EventArgs args) {
			_credit = true;
			this.Close();
		}
		public bool CreditButtonClicked {
			get {
				return _credit;
			}
		}

	}
}
