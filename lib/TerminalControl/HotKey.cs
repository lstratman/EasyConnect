/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: HotKey.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.UI;

namespace Poderosa.Forms
{
	/*
	 * ホットキーコントロール暫定版
	 * 
	 * キーを次の４種類に区別する。
	 * 1. ModifierKey: Ctrl, Shift, Alt。何かと組み合わせるキー。
	 * 2. CharKey: 文字と関連付けられたキー。テンキー,スペース含む。
	 * 3. TerminalKey: ターミナルにとって意味のあるキー。Enter,BS,ESC,Tab,カーソルキー,ファンクションキー。
	 * 4. GenericKey:  その他のキー
	 * 
	 * ホットキーに有効な入力はmodifierにより異なり、
	 * modifierなしかShiftのみ：4のみ
	 * ControlまたはAltあり： 2,3,4
	 * となる。
	 * 
	 * 確認済み怪現象：Shift+F4, Control+F10で変なキーが来る
	 * 
	 * デバッグオプションで、来たキーのリストをリストボックスあたりにダンプする機能があるとよいだろう。
	*/


	/// <summary>
	/// HotKey の概要の説明です。
	/// </summary>
	internal class HotKey : System.Windows.Forms.TextBox
	{
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		private TextBox _debugTextBox;

		private Keys _key;

		public HotKey()
		{
			// この呼び出しは、Windows.Forms フォーム デザイナで必要です。
			InitializeComponent();

			// TODO: InitForm を呼び出しの後に初期化処理を追加します。

		}

		public TextBox DebugTextBox {
			get {
				return _debugTextBox;
			}
			set {
				_debugTextBox = value;
			}
		}

		public Keys Key {
			get {
				return _key;
			}
			set {
				_key = value;
				Text = FormatKey(value);
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

		#region Component Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ImeMode = ImeMode.Disable;
		}
		#endregion

		protected override bool IsInputKey(Keys key) {
			return false;
		}

		protected override void OnKeyUp(KeyEventArgs args) {
			base.OnKeyUp(args);

			Keys body = _key & Keys.KeyCode;
			if(body==Keys.Menu || body==Keys.ShiftKey || body==Keys.ControlKey) { //modifierのみは認めない
				_key = Keys.None;
				this.Text = "";
			}
		}
		protected override bool ProcessDialogKey(Keys key) {
			if(_debugTextBox!=null) AppendDebugText(key.ToString()+" "+(int)key);
			string t = FormatKey(key);
			if(t!=null) {
				this.Text = t;
			}
			else
				_key = Keys.None;
			return true;
		}

		private string FormatKey(Keys key) {
			Keys body = key & Keys.KeyCode;
			Keys modifiers = key & Keys.Modifiers;

			//modifierは常に表示する
			StringBuilder b = new StringBuilder();
			if((modifiers & Keys.Control)!=Keys.None) {
				b.Append("Ctrl");
			}
			if((modifiers & Keys.Shift)!=Keys.None) {
				if(b.Length>0) b.Append('+');
				b.Append("Shift");
			}
			if((modifiers & Keys.Alt)!=Keys.None) {
				if(b.Length>0) b.Append('+');
				b.Append("Alt");
			}
			if(b.Length>0)
				b.Append('+');

			if(IsCharKey(body)) {
				if(modifiers!=Keys.None && modifiers!=Keys.Shift) {
					if(modifiers==Keys.Alt && (Keys.D0 <= body && body <= Keys.D9))
						_key = Keys.None;
					else {
						b.Append(UILibUtil.KeyString(body));
						_key = key;
					}
				}
				else
					_key = Keys.None;
			}
			else if(IsTerminalKey(body)) {
				if(modifiers!=Keys.None) {
					//カスタマイズ不能で固定されたショートカットキーは登録できない
					if(modifiers==Keys.Control && IsScrollKey(body))
						_key = Keys.None;
					else {
						b.Append(UILibUtil.KeyString(body));
						_key = key;
					}
				}
				else
					_key = Keys.None;
			}
			else if(IsFunctionKey(body)) {
				b.Append(UILibUtil.KeyString(body));
				_key = key;
			}
			else if(!IsModifierKey(body)) {
				_key = Keys.None;
			}

			return b.ToString();
		}

		private static bool IsCharKey(Keys key) {
			int n = (int)key;
			return ((int)Keys.A <= n && n <= (int)Keys.Z) ||
				((int)Keys.D0 <= n && n <= (int)Keys.D9) ||
				((int)Keys.NumPad0 <= n && n <= (int)Keys.NumPad9) ||
				((int)Keys.OemSemicolon <=n && n <= (int)Keys.Oemtilde) ||
				((int)Keys.OemOpenBrackets <=n && n <= (int)Keys.OemQuotes) ||
				key==Keys.Divide || key==Keys.Multiply || key==Keys.Subtract || key==Keys.Add || key==Keys.Decimal ||
				key==Keys.Space || key==Keys.Enter;
		}
		private static bool IsModifierKey(Keys key) {
			return key==Keys.Menu || key==Keys.ShiftKey || key==Keys.ControlKey ||
				key==Keys.LMenu || key==Keys.RMenu ||
				key==Keys.LShiftKey || key==Keys.RShiftKey ||
				key==Keys.LControlKey || key==Keys.RControlKey;
		}
		private static bool IsFunctionKey(Keys key) {
			return (int)Keys.F1 <= (int)key && (int)key <= (int)Keys.F24;
		}
		private static bool IsTerminalKey(Keys key) {
			return key==Keys.Escape || key==Keys.Back || key==Keys.Tab ||
				key==Keys.Up || key==Keys.Down || key==Keys.Left || key==Keys.Right ||
				((int)Keys.F1 <= (int)key && (int)key <= (int)Keys.F12) || 
				key==Keys.Home || key==Keys.End || key==Keys.Next || key==Keys.Prior || key==Keys.PageDown || key==Keys.PageUp ||
				key==Keys.Insert || key==Keys.Delete;
		}
		private static bool IsScrollKey(Keys key) { //TerminalKeyのサブセットで、Ctrlとの組み合わせでバッファのスクロールをする
			return key==Keys.Up || key==Keys.Down ||
				key==Keys.Home || key==Keys.End ||
				key==Keys.PageDown || key==Keys.PageUp;
		}


		private void AppendDebugText(string text) {
			string[] data = _debugTextBox.Lines;
			if(data.Length>=5) {
				string[] n = new string[5];
				Array.Copy(data, data.Length-4, n, 0, 4);
				n[4] = text;
				_debugTextBox.Lines = n;
			}
			else {
				string[] n = new string[data.Length+1];
				Array.Copy(data, 0, n, 0, data.Length);
				n[data.Length] = text;
				_debugTextBox.Lines = n;
			}
		}
	}
}
