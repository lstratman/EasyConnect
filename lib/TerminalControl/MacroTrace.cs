/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: MacroTrace.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Poderosa.MacroEnv
{
	/// <summary>
	/// MacroTrace の概要の説明です。
	/// </summary>
	internal class MacroTraceWindow : System.Windows.Forms.Form
	{
		internal static int  _instanceCount;
		internal static Size _lastWindowSize = new Size();

		private System.Windows.Forms.TextBox _textBox;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MacroTraceWindow()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			this.Icon = GApp.Options.GuevaraMode? GIcons.GetOldGuevaraIcon() : GIcons.GetAppIcon();

			//位置とサイズの調整
			int n = _instanceCount % 5;
			this.Location = new Point(GApp.Frame.Left + 30+20*n, GApp.Frame.Top  + 30+20*n);
			if(_instanceCount>0) this.Size = _lastWindowSize;
			_instanceCount++;

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
			this._textBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// _textBox
			// 
			this._textBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this._textBox.Multiline = true;
			this._textBox.Name = "_textBox";
			this._textBox.ReadOnly = true;
			this._textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this._textBox.Size = new System.Drawing.Size(352, 237);
			this._textBox.TabIndex = 0;
			this._textBox.Text = "";
			this._textBox.BackColor = Color.FromKnownColor(KnownColor.Window);
			// 
			// MacroTrace
			// 
			this.StartPosition = FormStartPosition.Manual;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.ClientSize = new System.Drawing.Size(352, 237);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._textBox});
			this.Name = "MacroTrace";
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);

		}
		#endregion

		public void AdjustTitle(MacroModule mod) {
			this.Text = GApp.Strings.GetString("Caption.MacroTrace.Title") + mod.Title;
		}

		private string _lineToAdd;
		public void AddLine(string t) {
			//これはマクロスレッドから呼ばれるのでSendMessageを使う必要がある
			if(_textBox.TextLength!=0) t = "\r\n"+t;
			_lineToAdd = t;
			Win32.SendMessage(this.Handle, GConst.WMG_MACRO_TRACE, IntPtr.Zero, IntPtr.Zero);
		}
		protected override void OnClosed(EventArgs args) {
			base.OnClosed(args);
			_lastWindowSize = this.Size;
		}

		protected override void WndProc(ref Message msg) {
			base.WndProc(ref msg);
			if(msg.Msg==GConst.WMG_MACRO_TRACE) {
				_textBox.AppendText(_lineToAdd);
			}
		}
	}
}
