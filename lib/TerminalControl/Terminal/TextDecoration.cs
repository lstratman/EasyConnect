/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TextDecoration.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Text;

using Poderosa.Config;
using Poderosa.Toolkit;

namespace Poderosa.Text
{
	//TextDecorationで色を指定するのか、外部で定義された色を使うのかの区別につかう。ColorのAプロパティの値で代用すればちょっと効率は上がりそうだが...
	internal enum ColorType {
		DefaultBack,
		DefaultText,
		Custom
	}

	internal class TextDecoration : ICloneable {
		private ColorType _bgColorType;
		private Color _bgColor;
		private ColorType _textColorType;
		private Color _textColor;
		private bool  _bold;
		private bool  _underline;

		private static TextDecoration _default;
		public static TextDecoration Default {
			get {
				if(_default==null) {
					_default = new TextDecoration(false, false);
				}
				return _default;
			}
		}
		public static TextDecoration ClonedDefault() {
			return new TextDecoration(false, false);
		}

		public object Clone() {
			TextDecoration t = new TextDecoration();
			t._bgColorType = _bgColorType;
			t._bgColor = _bgColor;
			t._textColorType = _textColorType;
			t._textColor = _textColor;
			t._bold = _bold;
			t._underline = _underline;
			return t;
		}
		private TextDecoration() {} //Clone()から使うためのコンストラクタ

		public Color TextColor {
			get {
				return _textColor;
			}
			set {
				_textColor = value;
				_textColorType = value==Color.Empty? ColorType.DefaultText : ColorType.Custom;
			}
		}
		public Color BackColor {
			get {
				return _bgColor;
			}
			set {
				_bgColor = value;
				_bgColorType = value==Color.Empty? ColorType.DefaultBack : ColorType.Custom;
			}
		}
		public ColorType TextColorType {
			get {
				return _textColorType;
			}
			set {
				_textColorType = value;
			}
		}
		public ColorType BackColorType {
			get {
				return _bgColorType;
			}
			set {
				_bgColorType = value;
			}
		}
		public bool Bold {
			get {
				return _bold;
			}
			set {
				_bold = value;
			}
		}
		public bool Underline {
			get {
				return _underline;
			}
			set {
				_underline = value;
			}
		}

		public bool IsDefault {
			get {
				return !_underline && !_bold && _bgColorType==ColorType.DefaultBack && _textColorType==ColorType.DefaultText;
			}
		}

		public void Inverse() {
			Color tmp = _textColor;
			_textColor = _bgColor;
			_bgColor = tmp;

			ColorType tmp2 = _textColorType;
			_textColorType = _bgColorType;
			_bgColorType = tmp2;
		}
		public void ToCaretStyle() {
			Inverse(); 
			if(GEnv.Options.CaretColor!=Color.Empty) {
				_bgColorType = ColorType.Custom;
				_bgColor = GEnv.Options.CaretColor;
			}
		}

		public TextDecoration(bool underline, bool bold) {
			_bgColorType = ColorType.DefaultBack;
			_textColorType = ColorType.DefaultText;
			_bold = bold;
			_underline = underline;
		}
		public TextDecoration(Color bg, Color txt, bool underline, bool bold) {
			Init(bg, txt, underline, bold);
		}
		public void Init(Color bg, Color txt, bool underline, bool bold) {
			_bgColor = bg;
			_bgColorType = ColorType.Custom;
			_textColor = txt;
			_textColorType = ColorType.Custom;
			_bold = bold;
			_underline = underline;
		}

		public override string ToString() {
			StringBuilder b = new StringBuilder();
			b.Append(_bgColor.ToString()); //これでまっとうな文字列が出るのか?
			b.Append('/');
			b.Append(_textColor.ToString());
			b.Append('/');
			if(_bold) b.Append('B');
			return b.ToString();
		}

	}

	internal class FontHandle {
		private Font _font;
		private IntPtr _hFont;

		public FontHandle(Font f) {
			_font = f;
		}
		public Font Font {
			get {
				return _font;
			}
		}
		public IntPtr HFONT {
			get {
				if(_hFont==IntPtr.Zero) _hFont = _font.ToHfont();
				return _hFont;
			}
		}
	}
}
