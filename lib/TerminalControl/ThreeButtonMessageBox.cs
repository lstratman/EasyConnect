/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ThreeButtonMessageBox.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Poderosa;

namespace Poderosa.Forms
{
	/// <summary>
	/// ThreeButtonMessageBox の概要の説明です。
	/// </summary>
	internal class ThreeButtonMessageBox : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button _button1;
		private System.Windows.Forms.Button _button2;
		private System.Windows.Forms.Button _button3;
		private System.Windows.Forms.Label _message;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ThreeButtonMessageBox()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
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
			this._button1 = new System.Windows.Forms.Button();
			this._button2 = new System.Windows.Forms.Button();
			this._button3 = new System.Windows.Forms.Button();
			this._message = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _button1
			// 
			this._button1.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this._button1.Location = new System.Drawing.Point(8, 56);
			this._button1.Name = "_button1";
			this._button1.FlatStyle = FlatStyle.System;
			this._button1.Size = new System.Drawing.Size(96, 23);
			this._button1.TabIndex = 0;
			// 
			// _button2
			// 
			this._button2.DialogResult = System.Windows.Forms.DialogResult.No;
			this._button2.Location = new System.Drawing.Point(112, 56);
			this._button2.Name = "_button2";
			this._button2.FlatStyle = FlatStyle.System;
			this._button2.Size = new System.Drawing.Size(96, 23);
			this._button2.TabIndex = 1;
			// 
			// _button3
			// 
			this._button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._button3.Location = new System.Drawing.Point(216, 56);
			this._button3.Name = "_button3";
			this._button3.FlatStyle = FlatStyle.System;
			this._button3.Size = new System.Drawing.Size(96, 23);
			this._button3.TabIndex = 2;
			// 
			// _message
			// 
			this._message.Location = new System.Drawing.Point(64, 8);
			this._message.Name = "_message";
			this._message.Size = new System.Drawing.Size(232, 48);
			this._message.TabIndex = 3;
			this._message.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ThreeButtonMessageBox
			// 
			this.AcceptButton = this._button1;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._button3;
			this.ClientSize = new System.Drawing.Size(322, 85);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._message,
																		  this._button3,
																		  this._button2,
																		  this._button1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ThreeButtonMessageBox";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion

		public string YesButtonText {
			get {
				return _button1.Text;
			}
			set {
				_button1.Text = value;
			}
		}
		public string NoButtonText {
			get {
				return _button2.Text;
			}
			set {
				_button2.Text = value;
			}
		}
		public string CancelButtonText {
			get {
				return _button3.Text;
			}
			set {
				_button3.Text = value;
			}
		}
		public string Message {
			get {
				return _message.Text;
			}
			set {
				_message.Text = value;
			}
		}
		protected override void OnPaint(PaintEventArgs a) {
			base.OnPaint(a);
			//アイコンの描画　.NET Frameworkだけでシステムで持っているアイコンのロードはできないようだ
			if(_questionIcon==null) LoadQuestionIcon();
			a.Graphics.DrawIcon(_questionIcon, 16, 8); 
		}
		private static Icon _questionIcon;
		private static void LoadQuestionIcon() {
			IntPtr hIcon = Win32.LoadIcon(IntPtr.Zero, new IntPtr(Win32.IDI_QUESTION));
			_questionIcon = Icon.FromHandle(hIcon);
		}
	}
}
