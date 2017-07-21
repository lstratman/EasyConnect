/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: RenderProfile.cs,v 1.10 2012/05/27 15:22:50 kzmi Exp $
 */
using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Forms;

#if !MACRODOC
using Poderosa.Util;
using Poderosa.Document;
using System.Globalization;
#endif

namespace Poderosa.View {

#if MACRODOC
    /// <summary>
    /// <ja>背景画像の位置を指定します。</ja>
    /// <en>Specifies the position of the background image.</en>
    /// </summary>
    public enum ImageStyle {
        /// <summary>
        /// <ja>中央</ja>
        /// <en>Center</en>
        /// </summary>
        Center,
        /// <summary>
        /// <ja>左上</ja>
        /// <en>Upper left corner</en>
        /// </summary>
        TopLeft,
        /// <summary>
        /// <ja>右上</ja>
        /// <en>Upper right corner</en>
        /// </summary>
        TopRight,
        /// <summary>
        /// <ja>左下</ja>
        /// <en>Lower left corner</en>
        /// </summary>
        BottomLeft,
        /// <summary>
        /// <ja>右下</ja>
        /// <en>Lower right corner</en>
        /// </summary>
        BottomRight,
        /// <summary>
        /// <ja>伸縮して全体に表示</ja>
        /// <en>The image covers the whole area of the console by expansion</en>
        /// </summary>
        Scaled
    }
#else
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public enum ImageStyle {
        [EnumValue(Description = "Enum.ImageStyle.Center")]
        Center,
        [EnumValue(Description = "Enum.ImageStyle.TopLeft")]
        TopLeft,
        [EnumValue(Description = "Enum.ImageStyle.TopRight")]
        TopRight,
        [EnumValue(Description = "Enum.ImageStyle.BottomLeft")]
        BottomLeft,
        [EnumValue(Description = "Enum.ImageStyle.BottomRight")]
        BottomRight,
        [EnumValue(Description = "Enum.ImageStyle.Scaled")]
        Scaled,
        [EnumValue(Description = "Enum.ImageStyle.HorizontalFit")]
        HorizontalFit,
        [EnumValue(Description = "Enum.ImageStyle.VerticalFit")]
        VerticalFit
    }
#endif

    internal class FontHandle {
        private readonly Font _font;
        private readonly bool _clearType;
        private IntPtr _hFont;

        public FontHandle(Font f, bool clearType) {
            _font = f;
            _clearType = clearType;
            _hFont = IntPtr.Zero;
        }

        public Font Font {
            get {
                return _font;
            }
        }

        public IntPtr HFONT {
            get {
                if (_hFont == IntPtr.Zero) {
                    CreateFont();
                }
                return _hFont;
            }
        }

        private void CreateFont() {
            lock (this) {
                if (_hFont == IntPtr.Zero) {
                    if (_clearType) {
                        Win32.LOGFONT lf = new Win32.LOGFONT();
                        _font.ToLogFont(lf);
                        Version osVer = Environment.OSVersion.Version;
                        int major = osVer.Major;
                        int minor = osVer.Minor;
                        if (major > 5 || (major == 5 && minor >= 1))
                            lf.lfQuality = Win32.CLEARTYPE_NATURAL_QUALITY;
                        else
                            lf.lfQuality = Win32.CLEARTYPE_QUALITY;
                        _hFont = Win32.CreateFontIndirect(lf);
                    }
                    else {
                        _hFont = _font.ToHfont();
                    }
                }
            }
        }

        public void Dispose() {
            if (_hFont != IntPtr.Zero)
                Win32.DeleteObject(_hFont);
            _hFont = IntPtr.Zero;
            _font.Dispose();
        }
    }

    /// <summary>
    /// <ja>コンソールの表示方法を指定するオブジェクトです。接続前にTerminalParamのRenderProfileプロパティにセットすることで、マクロから色・フォント・背景画像を指定できます。</ja>
    /// <en>Implements the parameters for displaying the console. By setting this object to the RenderProfile property of the TerminalParam object, the macro can control colors, fonts, and background images.</en>
    /// </summary>
    public class RenderProfile : ICloneable {
        private string _fontName;
        private string _cjkFontName;
        private float _fontSize;
        private bool _useClearType;
        private bool _enableBoldStyle;
        private bool _forceBoldStyle;
        private FontHandle _font;
        private FontHandle _boldfont;
        private FontHandle _underlinefont;
        private FontHandle _boldunderlinefont;
        private FontHandle _cjkFont;
        private FontHandle _cjkBoldfont;
        private FontHandle _cjkUnderlinefont;
        private FontHandle _cjkBoldUnderlinefont;
#if !MACRODOC
        private EscapesequenceColorSet _esColorSet;
#endif
        private bool _darkenEsColorForBackground;

        private Color _forecolor;
        private Color _bgcolor;

        private Brush _brush;
        private Brush _bgbrush;

        private string _backgroundImageFileName;
        private Image _backgroundImage;
        private bool _imageLoadIsAttempted;
        private ImageStyle _imageStyle;

        private SizeF _pitch;
        private int _lineSpacing;
        private float _chargap; //文字列を表示するときに左右につく余白
        private bool _usingIdenticalFont; //ASCII/CJKで同じフォントを使っているかどうか

        /// <summary>
        /// <ja>通常の文字を表示するためのフォント名です。</ja>
        /// <en>Gets or sets the font name for normal characters.</en>
        /// </summary>
        public string FontName {
            get {
                return _fontName;
            }
            set {
                _fontName = value;
                ClearFont();
            }
        }
        /// <summary>
        /// <ja>CJK文字を表示するためのフォント名です。</ja>
        /// <en>Gets or sets the font name for CJK characters.</en>
        /// </summary>
        public string CJKFontName {
            get {
                return _cjkFontName;
            }
            set {
                _cjkFontName = value;
                ClearFont();
            }
        }
        /// <summary>
        /// <ja>フォントサイズです。</ja>
        /// <en>Gets or sets the font size.</en>
        /// </summary>
        public float FontSize {
            get {
                return _fontSize;
            }
            set {
                _fontSize = value;
                ClearFont();
            }
        }
        /// <summary>
        /// <ja>trueにセットすると、フォントとOSでサポートされていれば、ClearTypeを使用して文字が描画されます。</ja>
        /// <en>If this property is true, the characters are drew by the ClearType when the font and the OS supports it.</en>
        /// </summary>
        public bool UseClearType {
            get {
                return _useClearType;
            }
            set {
                _useClearType = value;
            }
        }

        /// <summary>
        /// <ja>falseにするとエスケープシーケンスでボールドフォントが指定されていても通常フォントで描画します</ja>
        /// <en>If this property is false, bold fonts are replaced by normal fonts even if the escape sequence indicates bold.</en>
        /// </summary>
        public bool EnableBoldStyle {
            get {
                return _enableBoldStyle;
            }
            set {
                _enableBoldStyle = value;
            }
        }

        /// <summary>
        /// </summary>
        public bool ForceBoldStyle {
            get {
                return _forceBoldStyle;
            }
            set {
                _forceBoldStyle = value;
            }
        }

        /// <summary>
        /// <ja>文字色です。</ja>
        /// <en>Gets or sets the color of characters.</en>
        /// </summary>
        public Color ForeColor {
            get {
                return _forecolor;
            }
            set {
                _forecolor = value;
                ClearBrush();
            }
        }
        /// <summary>
        /// <ja>JScriptではColor構造体が使用できないので、ForeColorプロパティを設定するかわりにこのメソッドを使ってください。</ja>
        /// <en>Because JScript cannot handle the Color structure, please use this method instead of the ForeColor property.</en>
        /// </summary>
        public void SetForeColor(object value) {
            _forecolor = (Color)value;
            ClearBrush();
        }
        /// <summary>
        /// <ja>背景色です。</ja>
        /// <en>Gets or sets the background color.</en>
        /// </summary>
        public Color BackColor {
            get {
                return _bgcolor;
            }
            set {
                _bgcolor = value;
                ClearBrush();
            }
        }
        /// <summary>
        /// <ja>JScriptでは構造体が使用できないので、BackColorプロパティを設定するかわりにこのメソッドを使ってください。</ja>
        /// <en>Because JScript cannot handle the Color structure, please use this method instead of the BackColor property.</en>
        /// </summary>
        public void SetBackColor(object value) {
            _bgcolor = (Color)value;
            ClearBrush();
        }

        /// <summary>
        /// <ja>背景色を色テーブルから選択するときに、暗い色にするかどうかを設定または取得します。</ja>
        /// <en>Gets or sets whether the color is darken when the background color is chosen from the color table.</en>
        /// </summary>
        public bool DarkenEsColorForBackground {
            get {
                return _darkenEsColorForBackground;
            }
            set {
                _darkenEsColorForBackground = value;
            }
        }

        /// <summary>
        /// <ja>背景画像のファイル名です。</ja>
        /// <en>Gets or set the file name of the background image.</en>
        /// </summary>
        public string BackgroundImageFileName {
            get {
                return _backgroundImageFileName;
            }
            set {
                _backgroundImageFileName = value;
                _backgroundImage = null;
            }
        }
        /// <summary>
        /// <ja>背景画像の位置です。</ja>
        /// <en>Gets or sets the position of the background image.</en>
        /// </summary>
        public ImageStyle ImageStyle {
            get {
                return _imageStyle;
            }
            set {
                _imageStyle = value;
            }
        }
#if !MACRODOC

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public EscapesequenceColorSet ESColorSet {
            get {
                return _esColorSet;
            }
            set {
                Debug.Assert(value != null);
                _esColorSet = value;
            }
        }
#endif
        /// <summary>
        /// <ja>コピーして作成します。</ja>
        /// <en>Initializes with another instance.</en>
        /// </summary>
        public RenderProfile(RenderProfile src) {
            _fontName = src._fontName;
            _cjkFontName = src._cjkFontName;
            _fontSize = src._fontSize;
            _lineSpacing = src._lineSpacing;
            _useClearType = src._useClearType;
            _enableBoldStyle = src._enableBoldStyle;
            _forceBoldStyle = src._forceBoldStyle;
            _cjkFont = _font = null;

            _forecolor = src._forecolor;
            _bgcolor = src._bgcolor;
#if !MACRODOC
            _esColorSet = (EscapesequenceColorSet)src._esColorSet.Clone();
#endif
            _bgbrush = _brush = null;

            _backgroundImageFileName = src._backgroundImageFileName;
            _imageLoadIsAttempted = false;
            _imageStyle = src.ImageStyle;
        }
        public RenderProfile() {
            //do nothing. properties must be filled
            _backgroundImageFileName = "";
#if !MACRODOC
            _esColorSet = new EscapesequenceColorSet();
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exclude/>
        public object Clone() {
            return new RenderProfile(this);
        }



        private void ClearFont() {
            DisposeFontHandle(ref _font);
            DisposeFontHandle(ref _boldfont);
            DisposeFontHandle(ref _underlinefont);
            DisposeFontHandle(ref _boldunderlinefont);
            DisposeFontHandle(ref _cjkFont);
            DisposeFontHandle(ref _cjkBoldfont);
            DisposeFontHandle(ref _cjkUnderlinefont);
            DisposeFontHandle(ref _cjkBoldUnderlinefont);
        }
        private void DisposeFontHandle(ref FontHandle f) {
            if (f != null) {
                f.Dispose();
                f = null;
            }
        }
        private void ClearBrush() {
            if (_brush != null)
                _brush.Dispose();
            if (_bgbrush != null)
                _bgbrush.Dispose();
            _brush = null;
            _bgbrush = null;
        }

#if !MACRODOC
        private void CreateFonts() {
            _font = new FontHandle(RuntimeUtil.CreateFont(_fontName, _fontSize), _useClearType);
            FontStyle fs = _font.Font.Style;
            _boldfont = new FontHandle(new Font(_font.Font, fs | FontStyle.Bold), _useClearType);
            _underlinefont = new FontHandle(new Font(_font.Font, fs | FontStyle.Underline), _useClearType);
            _boldunderlinefont = new FontHandle(new Font(_font.Font, fs | FontStyle.Underline | FontStyle.Bold), _useClearType);

            _cjkFont = new FontHandle(new Font(_cjkFontName, _fontSize), _useClearType);
            fs = _cjkFont.Font.Style;
            _cjkBoldfont = new FontHandle(new Font(_cjkFont.Font, fs | FontStyle.Bold), _useClearType);
            _cjkUnderlinefont = new FontHandle(new Font(_cjkFont.Font, fs | FontStyle.Underline), _useClearType);
            _cjkBoldUnderlinefont = new FontHandle(new Font(_cjkFont.Font, fs | FontStyle.Underline | FontStyle.Bold), _useClearType);

            _usingIdenticalFont = (_font.Font.Name == _cjkFont.Font.Name);

            //通常版
            Graphics g = Graphics.FromHwnd(Win32.GetDesktopWindow());
            IntPtr hdc = g.GetHdc();
            Win32.SelectObject(hdc, _font.HFONT);
            Win32.SIZE charsize1, charsize2;
            Win32.GetTextExtentPoint32(hdc, "A", 1, out charsize1);
            Win32.GetTextExtentPoint32(hdc, "AAA", 3, out charsize2);

            _pitch = new SizeF((charsize2.width - charsize1.width) / 2, charsize1.height);
            _chargap = (charsize1.width - _pitch.Width) / 2;
            g.ReleaseHdc(hdc);
            g.Dispose();
        }
        private void CreateBrushes() {
            _brush = new SolidBrush(_forecolor);
            _bgbrush = new SolidBrush(_bgcolor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public Brush Brush {
            get {
                if (_brush == null)
                    CreateBrushes();
                return _brush;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public Brush BgBrush {
            get {
                if (_bgbrush == null)
                    CreateBrushes();
                return _bgbrush;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public SizeF Pitch {
            get {
                if (_font == null)
                    CreateFonts();
                return _pitch;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public int LineSpacing {
            get {
                return _lineSpacing;
            }
            set {
                _lineSpacing = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public Font DefaultFont {
            get {
                if (_font == null)
                    CreateFonts();
                return _font.Font;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exclude/>
        public Image GetImage() {
            try {
                if (!_imageLoadIsAttempted) {
                    _imageLoadIsAttempted = true;
                    _backgroundImage = null;
                    if (_backgroundImageFileName != null && _backgroundImageFileName.Length > 0) {
                        try {
                            _backgroundImage = Image.FromFile(_backgroundImageFileName);
                        }
                        catch (Exception) {
                            MessageBox.Show("Can't find the background image!", "Poderosa error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                    }
                }

                return _backgroundImage;
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public float CharGap {
            get {
                if (_font == null)
                    CreateFonts();
                return _chargap;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public bool UsingIdenticalFont {
            get {
                return _usingIdenticalFont;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dec"></param>
        /// <returns></returns>
        /// <exclude/>
        public Color CalcTextColor(TextDecoration dec) {
            if (_brush == null)
                CreateBrushes();
            if (dec == null)
                return _forecolor;

            switch (dec.TextColorType) {
                case ColorType.Custom:
                    return dec.TextColor;
                case ColorType.DefaultBack:
                    return _bgcolor;
                case ColorType.DefaultText:
                    return _forecolor;
                default:
                    throw new Exception("Unexpected decoration object");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dec"></param>
        /// <returns></returns>
        /// <exclude/>
        public Color CalcBackColor(TextDecoration dec) {
            if (_brush == null)
                CreateBrushes();
            if (dec == null)
                return _bgcolor;

            switch (dec.BackColorType) {
                case ColorType.Custom:
                    return dec.BackColor;
                case ColorType.DefaultBack:
                    return _bgcolor;
                case ColorType.DefaultText:
                    return _forecolor;
                default:
                    throw new Exception("Unexpected decoration object");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dec"></param>
        /// <returns></returns>
        /// <exclude/>
        public bool CalcBold(TextDecoration dec) {
            if (_forceBoldStyle)
                return true;

            if (_enableBoldStyle)
                return dec.Bold;
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dec"></param>
        /// <param name="cg"></param>
        /// <returns></returns>
        /// <exclude/>
        public Font CalcFont(TextDecoration dec, CharGroup cg) {
            return CalcFontInternal(dec, cg, false).Font;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dec"></param>
        /// <param name="cg"></param>
        /// <returns></returns>
        /// <exclude/>
        public IntPtr CalcHFONT_NoUnderline(TextDecoration dec, CharGroup cg) {
            return CalcFontInternal(dec, cg, true).HFONT;
        }

        private FontHandle CalcFontInternal(TextDecoration dec, CharGroup cg, bool ignore_underline) {
            if (_font == null)
                CreateFonts();

            if (CharGroupUtil.IsCJK(cg)) {
                if (dec == null)
                    return _cjkFont;

                if (CalcBold(dec)) {
                    if (!ignore_underline && dec.Underline)
                        return _cjkBoldUnderlinefont;
                    else
                        return _cjkBoldfont;
                }
                else if (!ignore_underline && dec.Underline)
                    return _cjkUnderlinefont;
                else
                    return _cjkFont;
            }
            else {
                if (dec == null)
                    return _font;

                if (CalcBold(dec)) {
                    if (!ignore_underline && dec.Underline)
                        return _boldunderlinefont;
                    else
                        return _boldfont;
                }
                else if (!ignore_underline && dec.Underline)
                    return _underlinefont;
                else
                    return _font;
            }
        }
#endif
    }

#if !MACRODOC

    /// <summary>
    /// Color palette element
    /// </summary>
    public struct ESColor {
        private readonly Color _color;
        private readonly bool _isExactColor;

        /// <summary>
        /// Gets a color value.
        /// </summary>
        public Color Color {
            get {
                return _color;
            }
        }

        /// <summary>
        /// Gets if this color must be displayed exectly.
        /// </summary>
        public bool IsExactColor {
            get {
                return _isExactColor;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="isExactColor">True if this color imust be displayed exectly.</param>
        public ESColor(Color color, bool isExactColor) {
            _color = color;
            _isExactColor = isExactColor;
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;
            if (!(obj is ESColor))
                return false;

            ESColor c = (ESColor)obj;
            return (_color.ToArgb() == c._color.ToArgb()) && (_isExactColor == c._isExactColor);
        }

        public override int GetHashCode() {
            return _color.GetHashCode();
        }

        public static bool operator ==(ESColor c1, ESColor c2)
        {
            return (c1._color.ToArgb() == c2._color.ToArgb()) && (c1._isExactColor == c2._isExactColor);
        }

        public static bool operator !=(ESColor c1, ESColor c2) {
            return !(c1 == c2);
        }
    }
    
    /// <summary>
    /// Color palette changeable by escape sequence.
    /// </summary>
    /// <exclude/>
    public class EscapesequenceColorSet : ICloneable {

        private bool _isDefault;
        private readonly ESColor[] _colors = new ESColor[256];

        public EscapesequenceColorSet() {
            ResetToDefault();
        }

        private EscapesequenceColorSet(EscapesequenceColorSet a) {
            _isDefault = a._isDefault;
            for (int i = 0; i < _colors.Length; i++) {
                _colors[i] = a._colors[i];
            }
        }

        public object Clone() {
            return new EscapesequenceColorSet(this);
        }

        public bool IsDefault {
            get {
                return _isDefault;
            }
        }

        public ESColor this[int index] {
            get {
                return _colors[index];
            }
            set {
                _colors[index] = value;
                if (_isDefault)
                    _isDefault = GetDefaultColor(index) == value;
            }
        }

        public void ResetToDefault() {
            for (int i = 0; i < _colors.Length; i++) {
                _colors[i] = GetDefaultColor(i);
            }
            _isDefault = true;
        }

        public string Format() {
            if (_isDefault)
                return String.Empty;
            StringBuilder bld = new StringBuilder();
            for (int i = 0; i < _colors.Length; i++) {
                if (i > 0)
                    bld.Append(',');

                ESColor color = _colors[i];
                if (color.IsExactColor)
                    bld.Append('!');
                bld.Append(color.Color.Name);
                // Note: Color.Name returns hex'ed ARGB value if it was not a named color.
            }
            return bld.ToString();
        }

        public void Load(string value) {
            if (!_isDefault)
                ResetToDefault();

            if (value == null)
                return; // use default colors

            string[] cols = value.Split(',');
            int overrides = 0;
            for (int i = 0; i < cols.Length; i++) {
                string w = cols[i].Trim();

                bool isExactColor;
                if (w.Length > 0 && w[0] == '!') {
                    isExactColor = true;
                    w = w.Substring(1);
                }
                else {
                    isExactColor = false;
                }

                if (w.Length == 0)
                    continue;   // use default color

                Color color = ParseUtil.ParseColor(w, Color.Empty);
                if (!color.IsEmpty) {
                    _colors[i] = new ESColor(color, isExactColor);
                    overrides++;
                }
            }
            if (overrides > 0)
                _isDefault = false;
        }

        public static EscapesequenceColorSet Parse(string s) {
            EscapesequenceColorSet r = new EscapesequenceColorSet();
            r.Load(s);
            return r;
        }

        public static ESColor GetDefaultColor(int index) {
            return new ESColor(GetDefaultColorValue(index), false);
        }

        private static Color GetDefaultColorValue(int index) {
            int r, g, b;
            switch (index) {
                case 0:
                    return Color.Black;
                case 1:
                    return Color.Red;
                case 2:
                    return Color.Green;
                case 3:
                    return Color.Yellow;
                case 4:
                    return Color.Blue;
                case 5:
                    return Color.Magenta;
                case 6:
                    return Color.Cyan;
                case 7:
                    return Color.White;
                case 8:
                    return Color.FromArgb(64, 64, 64);
                case 9:
                    return Color.FromArgb(255, 64, 64);
                case 10:
                    return Color.FromArgb(64, 255, 64);
                case 11:
                    return Color.FromArgb(255, 255, 64);
                case 12:
                    return Color.FromArgb(64, 64, 255);
                case 13:
                    return Color.FromArgb(255, 64, 255);
                case 14:
                    return Color.FromArgb(64, 255, 255);
                case 15:
                    return Color.White;
                default:
                    if (index >= 16 && index <= 231) {
                        r = (index - 16) / 36 % 6;
                        g = (index - 16) / 6 % 6;
                        b = (index - 16) % 6;
                        return Color.FromArgb((r == 0) ? 0 : r * 40 + 55, (g == 0) ? 0 : g * 40 + 55, (b == 0) ? 0 : b * 40 + 55);
                    }
                    else if (index >= 232 && index <= 255) {
                        r = (index - 232) * 10 + 8;
                        return Color.FromArgb(r, r, r);
                    }
                    else {
                        return Color.Empty;
                    }
            }
        }
    }
#endif
}
