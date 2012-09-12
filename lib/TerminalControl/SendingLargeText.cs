/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: SendingLargeText.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

using Poderosa;
using Poderosa.Communication;
using Poderosa.Terminal;

namespace Poderosa.Forms
{
	/// <summary>
	/// SendingLargeText の概要の説明です。
	/// </summary>
	internal class SendingLargeText : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ProgressBar _progressBar;
		private System.Windows.Forms.Label _lineCountLabel;
		private System.Windows.Forms.Button _cancelButton;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		private PasteProcessor _proc;

		public SendingLargeText(PasteProcessor proc)
		{
			_proc = proc;

			Init();
		}
		
		private void Init() {

			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			this.Text = GApp.Strings.GetString("Form.SendingLargeText.Text");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			_progressBar.Maximum = _proc.LineCount;
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
			this._cancelButton = new System.Windows.Forms.Button();
			this._progressBar = new System.Windows.Forms.ProgressBar();
			this._lineCountLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._cancelButton.Location = new System.Drawing.Point(208, 56);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.TabIndex = 0;
			this._cancelButton.Click += new System.EventHandler(OnCancel);
			// 
			// _progressBar
			// 
			this._progressBar.Location = new System.Drawing.Point(8, 24);
			this._progressBar.Name = "_progressBar";
			this._progressBar.Size = new System.Drawing.Size(272, 23);
			this._progressBar.Step = 1;
			this._progressBar.TabIndex = 1;
			// 
			// _lineCountLabel
			// 
			this._lineCountLabel.Location = new System.Drawing.Point(8, 8);
			this._lineCountLabel.Name = "_lineCountLabel";
			this._lineCountLabel.Size = new System.Drawing.Size(144, 16);
			this._lineCountLabel.TabIndex = 2;
			// 
			// SendingLargeText
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.ClientSize = new System.Drawing.Size(292, 86);
			this.Controls.Add(this._lineCountLabel);
			this.Controls.Add(this._progressBar);
			this.Controls.Add(this._cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.CancelButton = _cancelButton;
			this.Name = "SendingLargeText";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);
		}
		#endregion

		protected override void OnLoad(EventArgs e) {
			base.OnLoad (e);
			_proc.LineProcessed += new PasteProcessor.EventHandler(OnLineProcessed);
			_proc.Perform();
		}


		private void OnLineProcessed(int i) {
			if(i==-1) // finish
				Win32.SendMessage(this.Handle, GConst.WMG_SENDLINE_PROGRESS, IntPtr.Zero, new IntPtr(-1));
			else
				Win32.SendMessage(this.Handle, GConst.WMG_SENDLINE_PROGRESS, IntPtr.Zero, new IntPtr(i));
		}

		private void OnCancel(object sender, EventArgs args) {
			_proc.SetAbortFlag();
		}

		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
			if(m.Msg==GConst.WMG_SENDLINE_PROGRESS) {
				if(m.LParam.ToInt32()==-1) {
					this.DialogResult = DialogResult.OK;
					Close();
				}
				else {
					_progressBar.Value = m.LParam.ToInt32();
					_lineCountLabel.Text = String.Format(GApp.Strings.GetString("Form.SendingLargeText._progressLabel"), _progressBar.Value+1, _proc.LineCount);
				}
			}
		}

	}
}
