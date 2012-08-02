/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: InputBox.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Poderosa.Forms
{
	/// <summary>
	/// InputBox の概要の説明です。
	/// </summary>
	internal class InputBox : System.Windows.Forms.Form
	{
		private bool _allowsZeroLenString;

		private System.Windows.Forms.TextBox _textBox;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public InputBox()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
		}
		public bool AllowsZeroLenString {
			get {
				return _allowsZeroLenString;
			}
			set {
				_allowsZeroLenString = value;
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
			this._textBox = new System.Windows.Forms.TextBox();
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _textBox
			// 
			this._textBox.Location = new System.Drawing.Point(8, 8);
			this._textBox.MaxLength = 30;
			this._textBox.Name = "_textBox";
			this._textBox.Size = new System.Drawing.Size(192, 19);
			this._textBox.TabIndex = 0;
			this._textBox.Text = "";
			this._textBox.GotFocus += new EventHandler(OnTextBoxGotFocus);
			this._textBox.TextChanged += new EventHandler(OnTextChanged);
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.Location = new System.Drawing.Point(48, 32);
			this._okButton.Name = "_okButton";
			this._okButton.FlatStyle = FlatStyle.System;
			this._okButton.Size = new System.Drawing.Size(64, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = GApp.Strings.GetString("Common.OK");
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(128, 32);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.Size = new System.Drawing.Size(72, 23);
			this._cancelButton.TabIndex = 2;
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			// 
			// InputBox
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(208, 61);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._textBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InputBox";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion

		public string Content {
			get {
				return _textBox.Text;
			}
			set {
				_textBox.Text = value;
				_okButton.Enabled = _allowsZeroLenString || (value!=null && value.Length!=0);
			}
		}

		private void OnTextBoxGotFocus(object sender, EventArgs args) {
			_textBox.SelectAll(); //この挙動が望ましくない場合もあるかもしれないが、最初の用途がタブのテキスト変更なので...
		}
		private void OnTextChanged(object sender, EventArgs args) {
			_okButton.Enabled = _allowsZeroLenString || (_textBox.Text!=null && _textBox.Text.Length!=0);
		}
	}
}
