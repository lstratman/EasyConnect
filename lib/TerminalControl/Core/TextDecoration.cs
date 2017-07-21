/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TextDecoration.cs,v 1.5 2012/05/20 09:17:25 kzmi Exp $
 */
using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Text;

//using Poderosa.Util;

namespace Poderosa.Document {
    //TextDecorationで色を指定するのか、外部で定義された色を使うのかの区別につかう。ColorのAプロパティの値で代用すればちょっと効率は上がりそうだが...
    /// <exclude/>
    public enum ColorType {
        DefaultBack,
        DefaultText,
        Custom
    }

    //テキストの描画情報.
    //標準背景色を使うときは_bgColorがColor.Empty, 標準テキスト色を使うときは_textColorがColor.Emptyになることに注意

    /// <summary>
    /// Text decoration.
    /// </summary>
    /// <remarks>
    /// The instance is immutable.
    /// </remarks>
    /// <exclude/>
    public sealed class TextDecoration {
        private readonly ColorType _bgColorType;
        private readonly Color _bgColor;
        private readonly ColorType _textColorType;
        private readonly Color _textColor;
        private readonly bool _bold;
        private readonly bool _underline;

        private static readonly TextDecoration _default =
            new TextDecoration(ColorType.DefaultBack, Color.Empty, ColorType.DefaultText, Color.Empty, false, false);

        /// <summary>
        /// Get a default decoration.
        /// "default decoration" means that text is displayed
        /// with default text color, default background color,
        /// no underline, and no bold.
        /// </summary>
        public static TextDecoration Default {
            get {
                return _default;
            }
        }

        private TextDecoration(ColorType ctbg, Color bg, ColorType cttxt, Color txt, bool underline, bool bold) {
            _bgColor = bg;
            _bgColorType = ctbg;
            _textColor = txt;
            _textColorType = cttxt;
            _underline = underline;
            _bold = bold;
        }

        public Color TextColor {
            get {
                return _textColor;
            }
        }
        public Color BackColor {
            get {
                return _bgColor;
            }
        }
        public ColorType TextColorType {
            get {
                return _textColorType;
            }
        }
        public ColorType BackColorType {
            get {
                return _bgColorType;
            }
        }
        public bool Bold {
            get {
                return _bold;
            }
        }
        public bool Underline {
            get {
                return _underline;
            }
        }

        public bool IsDefault {
            get {
                return !_underline && !_bold && _bgColorType == ColorType.DefaultBack && _textColorType == ColorType.DefaultText;
            }
        }

        /// <summary>
        /// Get a new copy whose text color and background color were swapped.
        /// </summary>
        /// <returns>new instance</returns>
        public TextDecoration GetInvertedCopy() {
            return new TextDecoration(_textColorType, _textColor, _bgColorType, _bgColor, _underline, _bold);
        }

        /// <summary>
        /// Get a new copy whose text color and background color were swapped.
        /// If a specified bgColor was not empty, it is used as a background color.
        /// </summary>
        /// <returns>new instance</returns>
        public TextDecoration GetInvertedCopyForCaret(Color bgColor) {
            ColorType newBgColorType;
            Color newBgColor;
            if (bgColor.IsEmpty) {
                newBgColorType = _textColorType;
                newBgColor = _textColor;
            }
            else {
                newBgColorType = ColorType.Custom;
                newBgColor = bgColor;
            }

            return new TextDecoration(newBgColorType, newBgColor, _bgColorType, _bgColor, _underline, _bold);
        }

        /// <summary>
        /// Get a new copy whose text color was set to the default text color.
        /// </summary>
        /// <returns>new instance</returns>
        public TextDecoration GetCopyWithDefaultTextColor() {
            return GetCopyWithTextColor(Color.Empty);
        }

        /// <summary>
        /// Get a new copy whose text color was set.
        /// </summary>
        /// <param name="textColor">new text color</param>
        /// <returns>new instance</returns>
        public TextDecoration GetCopyWithTextColor(Color textColor) {
            ColorType textColorType = textColor.IsEmpty ? ColorType.DefaultText : ColorType.Custom;
            return new TextDecoration(_bgColorType, _bgColor, textColorType, textColor, _underline, _bold);
        }

        /// <summary>
        /// Get a new copy whose background color was set to the default backgeound color.
        /// </summary>
        /// <returns>new instance</returns>
        public TextDecoration GetCopyWithDefaultBackColor() {
            return GetCopyWithBackColor(Color.Empty);
        }

        /// <summary>
        /// Get a new copy whose background color was set.
        /// </summary>
        /// <param name="bgColor">new background color</param>
        /// <returns>new instance</returns>
        public TextDecoration GetCopyWithBackColor(Color bgColor) {
            ColorType bgColorType = bgColor.IsEmpty ? ColorType.DefaultBack : ColorType.Custom;
            return new TextDecoration(bgColorType, bgColor, _textColorType, _textColor, _underline, _bold);
        }

        /// <summary>
        /// Get a new copy whose underline status was set.
        /// </summary>
        /// <param name="underline">new underline status</param>
        /// <returns>new instance</returns>
        public TextDecoration GetCopyWithUnderline(bool underline) {
            return new TextDecoration(_bgColorType, _bgColor, _textColorType, _textColor, underline, _bold);
        }

        /// <summary>
        /// Get a new copy whose bold status was set.
        /// </summary>
        /// <param name="bold">new bold status</param>
        /// <returns>new instance</returns>
        public TextDecoration GetCopyWithBold(bool bold) {
            return new TextDecoration(_bgColorType, _bgColor, _textColorType, _textColor, _underline, bold);
        }

        /// <summary>
        /// Get a new instance whose attributes except BackColor were reset to the default.
        /// </summary>
        /// <returns></returns>
        public TextDecoration RetainBackColor() {
            return new TextDecoration(
                _bgColorType, _bgColor,
                _default._textColorType, _default._textColor, _default._underline, _default._bold);
        }

        public override string ToString() {
            StringBuilder b = new StringBuilder();
            b.Append(_bgColor.ToString()); //これでまっとうな文字列が出るのか?
            b.Append('/');
            b.Append(_textColor.ToString());
            b.Append('/');
            if (_bold)
                b.Append('B');
            return b.ToString();
        }

    }

}
