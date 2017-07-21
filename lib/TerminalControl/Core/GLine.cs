/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: GLine.cs,v 1.23 2012/05/27 15:02:26 kzmi Exp $
 */
/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: GLine.cs,v 1.23 2012/05/27 15:02:26 kzmi Exp $
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Text;

#if UNITTEST
using NUnit.Framework;
#endif

using Poderosa.Util.Drawing;
using Poderosa.Forms;
using Poderosa.View;

namespace Poderosa.Document {
    // GLineの構成要素。１つのGWordは同じ描画がなされ、シングルリンクリストになっている。
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public sealed class GWord {
        private readonly TextDecoration _decoration; //描画情報
        private readonly int _offset;                //コンテナのGLineの何文字目から始まっているか
        private readonly CharGroup _charGroup;       //文字グループ
        private GWord _next;                //次のGWord

        /// 表示用の装飾
        internal TextDecoration Decoration {
            get {
                return _decoration;
            }
        }
        /// 所属するGLineの中で何文字目から始まっているか
        public int Offset {
            get {
                return _offset;
            }
        }

        ///次のWord
        public GWord Next {
            get {
                return _next;
            }
            set {
                _next = value;
            }
        }

        public CharGroup CharGroup {
            get {
                return _charGroup;
            }
        }

        /// 文字列、デコレーション、オフセットを指定するコンストラクタ。
        public GWord(TextDecoration d, int o, CharGroup chargroup) {
            Debug.Assert(d != null);
            _offset = o;
            _decoration = d;
            _next = null;
            _charGroup = chargroup;
        }

        //Nextの値以外をコピーする
        internal GWord StandAloneClone() {
            return new GWord(_decoration, _offset, _charGroup);
        }

        internal GWord DeepClone() {
            GWord w = StandAloneClone();
            if (_next != null)
                w._next = _next.DeepClone();
            return w;
        }

    }


    /// １行のデータ
    /// GWordへの分解は遅延評価される。両隣の行はダブルリンクリスト
    /// <exclude/>
    public sealed class GLine {

        /// <summary>
        /// Delegate for copying characters in GLine.
        /// </summary>
        /// <remarks>
        /// WIDECHAR_PAD is not contained in buff.
        /// </remarks>
        /// <param name="buff">An array of char which contains characters to copy.</param>
        /// <param name="length">Number of characters to copy from buff.</param>
        public delegate void BufferWriter(char[] buff, int length);

        internal const char WIDECHAR_PAD = '\uFFFF';

        private char[] _text; //本体：\0は行末を示す
        private int _displayLength;
        private EOLType _eolType;
        private int _id;
        private GWord _firstWord;
        private GLine _nextLine;
        private GLine _prevLine;

        [ThreadStatic]
        private static char[] _copyTempBuff;

        // Returns thread-local temporary buffer
        // for copying characters in _text.
        private char[] GetInternalTemporaryBufferForCopy() {
            char[] buff = _copyTempBuff;
            int minLen = _text.Length;
            if (buff == null || buff.Length < minLen) {
                buff = _copyTempBuff = new char[minLen];
            }
            return buff;
        }


        public int Length {
            get {
                return _text.Length;
            }
        }

        public int DisplayLength {
            get {
                return _displayLength;
            }
        }

        public GWord FirstWord {
            get {
                return _firstWord;
            }
        }

        //ID, 隣接行の設定　この変更は慎重さ必要！
        public int ID {
            get {
                return _id;
            }
            set {
                _id = value;
            }
        }

        public GLine NextLine {
            get {
                return _nextLine;
            }
            set {
                _nextLine = value;
            }
        }

        public GLine PrevLine {
            get {
                return _prevLine;
            }
            set {
                _prevLine = value;
            }
        }

        public EOLType EOLType {
            get {
                return _eolType;
            }
            set {
                _eolType = value;
            }
        }

        public GLine(int length) {
            Debug.Assert(length > 0);
            _text = new char[length];
            _displayLength = 0;
            _firstWord = new GWord(TextDecoration.Default, 0, CharGroup.LatinHankaku);
            _id = -1;
        }

        //GLineManipulatorなどのためのコンストラクタ
        internal GLine(char[] data, int dataLength, GWord firstWord) {
            _text = data;
            _displayLength = dataLength;
            _firstWord = firstWord;
            _id = -1;
        }

        private static int GetDisplayLength(char[] data) {
            int limit = data.Length;
            for (int len = 0; len < limit; len++) {
                if (data[len] == '\0')
                    return len;
            }
            return limit;
        }

        internal char[] DuplicateBuffer(char[] reusableBuffer) {
            if (reusableBuffer != null && reusableBuffer.Length == _text.Length) {
                Buffer.BlockCopy(_text, 0, reusableBuffer, 0, _text.Length * sizeof(char));
                return reusableBuffer;
            }
            else {
                return (char[])_text.Clone();
            }
        }

        public GLine Clone() {
            GLine nl = new GLine((char[])_text.Clone(), _displayLength, _firstWord.DeepClone());
            nl._eolType = _eolType;
            nl._id = _id;
            return nl;
        }

        public void Clear() {
            Clear(null);
        }

        public void Clear(TextDecoration dec) {
            TextDecoration fillDec = (dec != null) ? dec.RetainBackColor() : TextDecoration.Default;
            char fill = fillDec.IsDefault ? '\0' : ' '; // 色指定付きのことがあるのでスペース
            for (int i = 0; i < _text.Length; i++)
                _text[i] = fill;
            _displayLength = fillDec.IsDefault ? 0 : _text.Length;
            _firstWord = new GWord(fillDec, 0, CharGroup.LatinHankaku);
        }

        //前後の単語区切りを見つける。返す位置は、posとGetWordBreakGroupの値が一致する中で遠い地点
        public int FindPrevWordBreak(int pos) {
            int v = ToCharGroupForWordBreak(_text[pos]);
            while (pos >= 0) {
                if (v != ToCharGroupForWordBreak(_text[pos]))
                    return pos;
                pos--;
            }
            return -1;
        }

        public int FindNextWordBreak(int pos) {
            int v = ToCharGroupForWordBreak(_text[pos]);
            while (pos < _text.Length) {
                if (v != ToCharGroupForWordBreak(_text[pos]))
                    return pos;
                pos++;
            }
            return _text.Length;
        }

        private static int ToCharGroupForWordBreak(char ch) {
            if (ch < 0x80)
                return ASCIIWordBreakTable.Default.GetAt(ch);
            else if (ch == '\u3000') //全角スペース
                return ASCIIWordBreakTable.SPACE;
            else //さらにここをUnicodeCategory等をみて適当にこしらえることもできるが
                return ASCIIWordBreakTable.NOT_ASCII;
        }

        public void ExpandBuffer(int length) {
            if (length <= _text.Length)
                return;

            char[] current = _text;
            char[] newBuff = new char[length];
            Buffer.BlockCopy(current, 0, newBuff, 0, current.Length * sizeof(char));
            _text = newBuff;
            // Note; _displayLength is not changed.
        }

        private void Append(GWord w) {
            if (_firstWord == null)
                _firstWord = w;
            else
                this.LastWord.Next = w;
        }

        private GWord LastWord {
            get {
                GWord w = _firstWord;
                while (w.Next != null)
                    w = w.Next;
                return w;
            }
        }

        internal void Render(IntPtr hdc, RenderProfile prof, Color baseBackColor, int x, int y) {
            if (_text.Length == 0 || _text[0] == '\0')
                return; //何も描かなくてよい。これはよくあるケース

            float fx0 = (float)x;
            float fx1 = fx0;
            int y1 = y;
            int y2 = y1 + (int)prof.Pitch.Height;

            float pitch = prof.Pitch.Width;
            int defaultBackColorArgb = baseBackColor.ToArgb();

            Win32.SetBkMode(hdc, Win32.TRANSPARENT);

            GWord word = _firstWord;
            while (word != null) {
                TextDecoration dec = word.Decoration;

                IntPtr hFont = prof.CalcHFONT_NoUnderline(dec, word.CharGroup);
                Win32.SelectObject(hdc, hFont);

                uint foreColorRef = DrawUtil.ToCOLORREF(prof.CalcTextColor(dec));
                Win32.SetTextColor(hdc, foreColorRef);

                Color bkColor = prof.CalcBackColor(dec);
                bool isOpaque = (bkColor.ToArgb() != defaultBackColorArgb);
                if (isOpaque) {
                    uint bkColorRef = DrawUtil.ToCOLORREF(bkColor);
                    Win32.SetBkColor(hdc, bkColorRef);
                }

                int nextOffset = GetNextOffset(word);

                float fx2 = fx0 + pitch * nextOffset;

                if (prof.CalcBold(dec) || CharGroupUtil.IsCJK(word.CharGroup)) {
                    // It is not always true that width of a character in the CJK font is twice of a character in the ASCII font.
                    // Characters are drawn one by one to adjust pitch.

                    int step = CharGroupUtil.GetColumnsPerCharacter(word.CharGroup);
                    float charPitch = pitch * step;
                    int offset = word.Offset;
                    float fx = fx1;
                    if (isOpaque) {
                        // If background fill is required, we call ExtTextOut() with ETO_OPAQUE to draw the first character.
                        if (offset < nextOffset) {
                            Win32.RECT rect = new Win32.RECT((int)fx1, y1, (int)fx2, y2);
                            char ch = _text[offset];
                            Debug.Assert(ch != GLine.WIDECHAR_PAD);
                            unsafe {
                                Win32.ExtTextOut(hdc, rect.left, rect.top, Win32.ETO_OPAQUE, &rect, &ch, 1, null);
                            }
                        }
                        offset += step;
                        fx += charPitch;
                    }

                    for (; offset < nextOffset; offset += step) {
                        char ch = _text[offset];
                        Debug.Assert(ch != GLine.WIDECHAR_PAD);
                        unsafe {
                            Win32.ExtTextOut(hdc, (int)fx, y1, 0, null, &ch, 1, null);
                        }
                        fx += charPitch;
                    }
                }
                else {
                    int offset = word.Offset;
                    int displayLength = nextOffset - offset;
                    if (isOpaque) {
                        Win32.RECT rect = new Win32.RECT((int)fx1, y1, (int)fx2, y2);
                        unsafe {
                            fixed (char* p = &_text[offset]) {
                                Win32.ExtTextOut(hdc, rect.left, rect.top, Win32.ETO_OPAQUE, &rect, p, displayLength, null);
                            }
                        }
                    }
                    else {
                        unsafe {
                            fixed (char* p = &_text[offset]) {
                                Win32.ExtTextOut(hdc, (int)fx1, y1, 0, null, p, displayLength, null);
                            }
                        }
                    }
                }

                if (dec.Underline)
                    DrawUnderline(hdc, foreColorRef, (int)fx1, y2 - 1, (int)fx2 - (int)fx1);

                fx1 = fx2;
                word = word.Next;
            }

        }

        private void DrawUnderline(IntPtr hdc, uint col, int x, int y, int length) {
            //Underlineがつくことはあまりないだろうから毎回Penを作る。問題になりそうだったらそのときに考えよう
            IntPtr pen = Win32.CreatePen(0, 1, col);
            IntPtr prev = Win32.SelectObject(hdc, pen);
            Win32.MoveToEx(hdc, x, y, IntPtr.Zero);
            Win32.LineTo(hdc, x + length, y);
            Win32.SelectObject(hdc, prev);
            Win32.DeleteObject(pen);
        }

        internal int GetNextOffset(GWord word) {
            if (word.Next == null)
                return _displayLength;
            else
                return word.Next.Offset;
        }


        /// <summary>
        /// Invert text attribute at the specified position.
        /// </summary>
        /// <remarks>
        /// <para>If doInvert was false, only splitting of GWords will be performed.
        /// It is required to avoid problem when the text which conatins blinking cursor is drawn by DrawWord().</para>
        /// <para>DrawWord() draws contiguous characters at once,
        /// and the character pitch depends how the character in the font was designed.</para>
        /// <para>By split GWord even if inversion is not required,
        /// the position of a character of the blinking cursor will be constant.</para>
        /// </remarks>
        /// <param name="index">Column index to invert.</param>
        /// <param name="doInvert">Whether inversion is really applied.</param>
        /// <param name="color">Background color of the inverted character.</param>
        internal void InvertCharacter(int index, bool doInvert, Color color) {
            //先にデータのあるところより先の位置を指定されたらバッファを広げておく
            if (index >= _displayLength) {
                int prevLength = _displayLength;
                ExpandBuffer(index + 1);
                for (int i = prevLength; i < index + 1; i++)
                    _text[i] = ' ';
                _displayLength = index + 1;
                this.LastWord.Next = new GWord(TextDecoration.Default, prevLength, CharGroup.LatinHankaku);
            }
            if (_text[index] == WIDECHAR_PAD)
                index--;

            GWord prev = null;
            GWord word = _firstWord;
            int nextoffset = 0;
            while (word != null) {
                nextoffset = GetNextOffset(word);
                if (word.Offset <= index && index < nextoffset) {
                    GWord next = word.Next;

                    //キャレットの反転
                    TextDecoration inv_dec = word.Decoration;
                    if (doInvert)
                        inv_dec = inv_dec.GetInvertedCopyForCaret(color);

                    //GWordは最大３つ(head:indexの前、middle:index、tail:indexの次)に分割される
                    GWord head = word.Offset < index ? new GWord(word.Decoration, word.Offset, word.CharGroup) : null;
                    GWord mid = new GWord(inv_dec, index, word.CharGroup);
                    int nextIndex = index + CharGroupUtil.GetColumnsPerCharacter(word.CharGroup);
                    GWord tail = nextIndex < nextoffset ? new GWord(word.Decoration, nextIndex, word.CharGroup) : null;

                    //連結 head,tailはnullのこともあるのでややこしい
                    List<GWord> list = new List<GWord>(3);
                    if (head != null) {
                        list.Add(head);
                        head.Next = mid;
                    }

                    list.Add(mid);
                    mid.Next = tail == null ? next : tail;

                    if (tail != null)
                        list.Add(tail);

                    //前後との連結
                    if (prev == null)
                        _firstWord = list[0];
                    else
                        prev.Next = list[0];

                    list[list.Count - 1].Next = next;

                    break;
                }

                prev = word;
                word = word.Next;
            }
        }

        /// <summary>
        /// Clone this instance that text attributes in the specified range are inverted.
        /// </summary>
        /// <param name="from">start column index of the range. (inclusive)</param>
        /// <param name="to">end column index of the range. (exclusive)</param>
        /// <returns>new instance</returns>
        internal GLine CreateInvertedClone(int from, int to) {
            ExpandBuffer(Math.Max(from + 1, to)); //激しくリサイズしたときなどにこの条件が満たせないことがある
            Debug.Assert(from >= 0 && from < _text.Length);
            if (from < _text.Length && _text[from] == WIDECHAR_PAD)
                from--;
            if (to < _text.Length && _text[to] == WIDECHAR_PAD)
                to++;

            const int PHASE_LEFT = 0;
            const int PHASE_MIDDLE = 1;
            const int PHASE_RIGHT = 2;

            int phase = PHASE_LEFT;
            int inverseIndex = from;

            GWord first = null;
            GWord last = null;

            for (GWord word = _firstWord; word != null; word = word.Next) {
                TextDecoration originalDecoration = word.Decoration;
                if (originalDecoration == null)
                    originalDecoration = TextDecoration.Default;

                int wordStart = word.Offset;
                int wordEnd = GetNextOffset(word);

                do {
                    GWord newWord;

                    if (phase == PHASE_RIGHT || inverseIndex < wordStart || wordEnd <= inverseIndex) {
                        TextDecoration newDec = (phase == PHASE_MIDDLE) ? originalDecoration.GetInvertedCopy() : originalDecoration;
                        newWord = new GWord(newDec, wordStart, word.CharGroup);
                        wordStart = wordEnd;
                    }
                    else {
                        TextDecoration leftDec = (phase == PHASE_LEFT) ? originalDecoration : originalDecoration.GetInvertedCopy();

                        if (wordStart < inverseIndex)
                            newWord = new GWord(leftDec, wordStart, word.CharGroup);
                        else
                            newWord = null;

                        wordStart = inverseIndex;

                        // update phase
                        if (phase == PHASE_LEFT) {
                            phase = PHASE_MIDDLE;
                            inverseIndex = to;
                        }
                        else if (phase == PHASE_MIDDLE) {
                            phase = PHASE_RIGHT;
                        }
                    }

                    // append new GWord to the list.
                    if (newWord != null) {
                        if (last == null)
                            first = newWord;
                        else
                            last.Next = newWord;

                        last = newWord;
                    }
                } while (wordStart < wordEnd);
            }

            GLine ret = new GLine((char[])_text.Clone(), _displayLength, first);
            ret.ID = _id;
            ret.EOLType = _eolType;

            return ret;
        }

        public bool IsRightSideOfZenkaku(int index) {
            return _text[index] == WIDECHAR_PAD;
        }

        public void WriteTo(BufferWriter writer, int index) {
            WriteToInternal(writer, index, _text.Length);
        }

        public void WriteTo(BufferWriter writer, int index, int length) {
            int limit = index + length;
            if (limit > _text.Length)
                limit = _text.Length;
            WriteToInternal(writer, index, limit);
        }

        private void WriteToInternal(BufferWriter writer, int index, int limit) {
            char[] temp = GetInternalTemporaryBufferForCopy();
            // Note: must be temp.Length >= limit here
            int tempIndex = 0;
            for (int i = index; i < limit; i++) {
                char ch = _text[i];
                if (ch == '\0')
                    break;
                if (ch != WIDECHAR_PAD)
                    temp[tempIndex++] = ch;
            }
            if (writer != null)
                writer(temp, tempIndex);
        }

        public string ToNormalString() {
            string s = null;
            WriteToInternal(
                delegate(char[] buff, int length) {
                    s = new string(buff, 0, length);
                },
                0, _text.Length);
            return s;
        }

        public static GLine CreateSimpleGLine(string text, TextDecoration dec) {
            char[] buff = new char[text.Length * 2];
            int offset = 0;
            int start = 0;
            CharGroup prevType = CharGroup.LatinHankaku;
            GWord firstWord = null;
            GWord lastWord = null;
            for (int i = 0; i < text.Length; i++) {
                char originalChar = text[i];
                char privateChar = Unicode.ToPrivate(originalChar);
                CharGroup nextType = Unicode.GetCharGroup(privateChar);
                int size = CharGroupUtil.GetColumnsPerCharacter(nextType);
                if (nextType != prevType) {
                    if (offset > start) {
                        GWord newWord = new GWord(dec, start, prevType);
                        if (lastWord == null) {
                            firstWord = lastWord = newWord;
                        }
                        else {
                            lastWord.Next = newWord;
                            lastWord = newWord;
                        }
                    }
                    prevType = nextType;
                    start = offset;
                }

                buff[offset++] = originalChar;
                if (size == 2)
                    buff[offset++] = WIDECHAR_PAD;
            }

            GWord w = new GWord(dec, start, prevType);
            if (lastWord == null) {
                firstWord = w;
            }
            else {
                lastWord.Next = w;
            }

            return new GLine(buff, offset, firstWord);
        }
    }

    /// <summary>
    /// <ja>
    /// <seealso cref="GLine">GLine</seealso>に対して、文字の追加／削除などを操作します。
    /// </ja>
    /// <en>
    /// Addition/deletion of the character etc. are operated for <seealso cref="GLine">GLine</seealso>. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このクラスは、たとえばターミナルがドキュメントの特定のGlineを置き換える場合などに使います。
    /// </ja>
    /// <en>
    /// When the terminal replaces specific Gline of the document for instance, this class uses it. 
    /// </en>
    /// </remarks>
    /// <exclude/>
    public class GLineManipulator {

        private struct CharAttr {
            public TextDecoration Decoration;
            public CharGroup CharGroup;

            public CharAttr(TextDecoration dec, CharGroup cg) {
                Decoration = dec;
                CharGroup = cg;
            }
        }

        private char[] _text;
        private CharAttr[] _attrs;
        private int _caretColumn;
        private TextDecoration _defaultDecoration;
        private EOLType _eolType;

        /// <summary>
        /// <ja>
        /// バッファサイズです。
        /// </ja>
        /// <en>
        /// Buffer size.
        /// </en>
        /// </summary>
        public int BufferSize {
            get {
                return _text.Length;
            }
        }

        /// <summary>
        /// <ja>
        /// キャレット位置を取得／設定します。
        /// </ja>
        /// <en>
        /// Get / set the position of the caret.
        /// </en>
        /// </summary>
        public int CaretColumn {
            get {
                return _caretColumn;
            }
            set {
                Debug.Assert(value >= 0);
                ExpandBuffer(value);
                Debug.Assert(value <= _text.Length);
                _caretColumn = value;
                value--;
                while (value >= 0 && _text[value] == '\0')
                    _text[value--] = ' ';
            }
        }

        /// <summary>
        /// <ja>
        /// キャリッジリターンを挿入します。
        /// </ja>
        /// <en>
        /// Insert the carriage return.
        /// </en>
        /// </summary>
        public void CarriageReturn() {
            _caretColumn = 0;
            _eolType = EOLType.CR;
        }

        /// <summary>
        /// <ja>
        /// 内容が空かどうかを示します。trueであれば空、falseなら何らかの文字が入っています。
        /// </ja>
        /// <en>
        /// It is shown whether the content is empty. Return false if here are some characters in it. True retuens if it is empty.
        /// </en>
        /// </summary>
        public bool IsEmpty {
            get {
                //_textを全部見る必要はないだろう
                return _caretColumn == 0 && _text[0] == '\0';
            }
        }
        /// <summary>
        /// <ja>
        /// テキストの描画情報を取得／設定します。
        /// </ja>
        /// <en>
        /// Drawing information in the text is get/set. 
        /// </en>
        /// </summary>
        public TextDecoration DefaultDecoration {
            get {
                return _defaultDecoration;
            }
            set {
                _defaultDecoration = value;
            }
        }

        // 全内容を破棄する
        /// <summary>
        /// <ja>
        /// 保持しているテキストをクリアします。
        /// </ja>
        /// <en>
        /// Clear the held text.
        /// </en>
        /// </summary>
        /// <param name="length"><ja>クリアする長さ</ja><en>Length to clear</en></param>
        public void Clear(int length) {
            if (_text == null || length != _text.Length) {
                _text = new char[length];
                _attrs = new CharAttr[length];
            }
            else {
                for (int i = 0; i < _attrs.Length; i++) {
                    _attrs[i] = new CharAttr(null, CharGroup.LatinHankaku);
                }
                for (int i = 0; i < _text.Length; i++) {
                    _text[i] = '\0';
                }
            }
            _caretColumn = 0;
            _eolType = EOLType.Continue;
        }

        /// 引数と同じ内容で初期化する。lineの内容は破壊されない。
        /// 引数がnullのときは引数なしのコンストラクタと同じ結果になる。
        /// <summary>
        /// <ja>
        /// 引数と同じ内容で初期化します。
        /// </ja>
        /// <en>
        /// Initialize same as argument.
        /// </en>
        /// </summary>
        /// <param name="cc">
        /// <ja>
        /// 設定するキャレット位置
        /// </ja>
        /// <en>
        /// The caret position to set.
        /// </en>
        /// </param>
        /// <param name="line">
        /// <ja>コピー元となるGLineオブジェクト</ja>
        /// <en>GLine object that becomes copy origin</en>
        /// </param>
        /// <remarks>
        /// <ja>
        /// <paramref name="line"/>がnullのときには、引数なしのコンストラクタと同じ結果になります。
        /// </ja>
        /// <en>
        /// The same results with the constructor who doesn't have the argument when <paramref name="line"/> is null. 
        /// </en>
        /// </remarks>
        public void Load(GLine line, int cc) {
            if (line == null) { //これがnullになっているとしか思えないクラッシュレポートがあった。本来はないはずなんだが...
                Clear(80);
                return;
            }

            Clear(line.Length);
            GWord w = line.FirstWord;
            _text = line.DuplicateBuffer(_text);

            int n = 0;
            while (w != null) {
                int nextoffset = line.GetNextOffset(w);
                while (n < nextoffset) {
                    _attrs[n++] = new CharAttr(w.Decoration, w.CharGroup);
                }
                w = w.Next;
            }

            _eolType = line.EOLType;
            ExpandBuffer(cc + 1);
            this.CaretColumn = cc; //' 'で埋めることもあるのでプロパティセットを使う
        }
#if UNITTEST
        public void Load(char[] text, int cc) {
            _text = text;
            _decorations = new TextDecoration[text.Length];
            _eolType = EOLType.Continue;
            _caretColumn = cc;
        }
        public char[] InternalBuffer {
            get {
                return _text;
            }
        }
#endif

        /// <summary>
        /// <ja>
        /// バッファを拡張します。
        /// </ja>
        /// <en>
        /// Expand the buffer.
        /// </en>
        /// </summary>
        /// <param name="length">Expanded buffer size.</param>
        public void ExpandBuffer(int length) {
            if (length <= _text.Length)
                return;

            char[] oldText = _text;
            _text = new char[length];
            Buffer.BlockCopy(oldText, 0, _text, 0, oldText.Length * sizeof(char)); 

            CharAttr[] oldAttrs = _attrs;
            _attrs = new CharAttr[length];
            Array.Copy(oldAttrs, 0, _attrs, 0, oldAttrs.Length);
        }

        /// <summary>
        /// <ja>
        /// 指定位置に1文字書き込みます。
        /// </ja>
        /// <en>
        /// Write one character to specified position.
        /// </en>
        /// </summary>
        /// <param name="ch"><ja>書き込む文字</ja><en>Character to write.</en></param>
        /// <param name="dec"><ja>テキスト書式を指定するTextDecorationオブジェクト</ja>
        /// <en>TextDecoration object that specifies text format
        /// </en></param>
        public void PutChar(char ch, TextDecoration dec) {
            Debug.Assert(dec != null);
            Debug.Assert(_caretColumn >= 0);
            Debug.Assert(_caretColumn < _text.Length);

            char originalChar = Unicode.ToOriginal(ch);
            CharGroup charGroup = Unicode.GetCharGroup(ch);

            bool onZenkaku = (_attrs[_caretColumn].CharGroup == CharGroup.CJKZenkaku);
            bool onZenkakuRight = (_text[_caretColumn] == GLine.WIDECHAR_PAD);

            if (onZenkaku) {
                //全角の上に書く
                if (!onZenkakuRight) {
                    _text[_caretColumn] = originalChar;
                    _attrs[_caretColumn] = new CharAttr(dec, charGroup);
                    if (CharGroupUtil.GetColumnsPerCharacter(charGroup) == 1) {
                        //全角の上に半角を書いた場合、隣にスペースを入れないと表示が乱れる
                        _caretColumn++;
                        if (_caretColumn < _text.Length) {
                            _text[_caretColumn] = ' ';
                            _attrs[_caretColumn].CharGroup = CharGroup.LatinHankaku;
                        }
                    }
                    else {
                        _attrs[_caretColumn + 1] = new CharAttr(dec, charGroup);
                        _caretColumn += 2;
                    }
                }
                else {
                    _text[_caretColumn - 1] = ' ';
                    _attrs[_caretColumn - 1].CharGroup = CharGroup.LatinHankaku;
                    _text[_caretColumn] = originalChar;
                    _attrs[_caretColumn] = new CharAttr(dec, charGroup);
                    if (CharGroupUtil.GetColumnsPerCharacter(charGroup) == 2) {
                        if (CharGroupUtil.GetColumnsPerCharacter(_attrs[_caretColumn + 1].CharGroup) == 2) {
                            if (_caretColumn + 2 < _text.Length) {
                                _text[_caretColumn + 2] = ' ';
                                _attrs[_caretColumn + 2].CharGroup = CharGroup.LatinHankaku;
                            }
                        }
                        _text[_caretColumn + 1] = GLine.WIDECHAR_PAD;
                        _attrs[_caretColumn + 1] = _attrs[_caretColumn];
                        _caretColumn += 2;
                    }
                    else {
                        _caretColumn++;
                    }
                }
            }
            else { //半角の上に書く
                _text[_caretColumn] = originalChar;
                _attrs[_caretColumn] = new CharAttr(dec, charGroup);
                if (CharGroupUtil.GetColumnsPerCharacter(charGroup) == 2) {
                    if (CharGroupUtil.GetColumnsPerCharacter(_attrs[_caretColumn + 1].CharGroup) == 2) { //半角、全角となっているところに全角を書いたら
                        if (_caretColumn + 2 < _text.Length) {
                            _text[_caretColumn + 2] = ' ';
                            _attrs[_caretColumn + 2].CharGroup = CharGroup.LatinHankaku;
                        }
                    }
                    _text[_caretColumn + 1] = GLine.WIDECHAR_PAD;
                    _attrs[_caretColumn + 1] = _attrs[_caretColumn];
                    _caretColumn += 2;
                }
                else {
                    _caretColumn++; //これが最もcommonなケースだが
                }
            }
        }

        /// <summary>
        /// <ja>
        /// テキスト書式を指定するTextDecorationオブジェクトを設定します。
        /// </ja>
        /// <en>
        /// Set the TextDecoration object that specifies the text format.
        /// </en>
        /// </summary>
        /// <param name="dec"><ja>設定するTextDecorationオブジェクト</ja><en>Set TextDecoration object</en></param>
        public void SetDecoration(TextDecoration dec) {
            if (_caretColumn < _attrs.Length)
                _attrs[_caretColumn].Decoration = dec;
        }

        /// <summary>
        /// <ja>
        /// キャレットをひとつ手前に戻します。
        /// </ja>
        /// <en>
        /// Move the caret to the left of one character. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>キャレットがすでに最左端にあるときには、何もしません。</ja>
        /// <en>Nothing is done when there is already a caret at the high order end. </en>
        /// </remarks>
        public void BackCaret() {
            if (_caretColumn > 0) { //最左端にあるときは何もしない
                _caretColumn--;
            }
        }

        /// <summary>
        /// <ja>
        /// 指定範囲を半角スペースで埋めます。
        /// </ja>
        /// <en>
        /// Fill the range of specification with space. 
        /// </en>
        /// </summary>
        /// <param name="from"><ja>埋める開始位置（この位置を含みます）</ja><en>Start position(include this position)</en></param>
        /// <param name="to"><ja>埋める終了位置（この位置は含みません）</ja><en>End position(exclude this position)</en></param>
        /// <param name="dec"><ja>テキスト書式を指定するTextDecorationオブジェクト</ja><en>TextDecoration object that specifies text format
        /// </en></param>
        public void FillSpace(int from, int to, TextDecoration dec) {
            if (to > _text.Length)
                to = _text.Length;
            TextDecoration fillDec = dec;
            if (fillDec != null) {
                fillDec = fillDec.RetainBackColor();
                if (fillDec.IsDefault)
                    fillDec = null;
            }
            for (int i = from; i < to; i++) {
                _text[i] = ' ';
                _attrs[i] = new CharAttr(fillDec, CharGroup.LatinHankaku);
            }
        }
        //startからcount文字を消去して詰める。右端にはnullを入れる
        /// <summary>
        /// <ja>
        /// 指定された場所から指定された文字数を削除し、その後ろを詰めます。
        /// </ja>
        /// <en>
        /// The number of characters specified from the specified place is deleted, and the furnace is packed afterwards. 
        /// </en>
        /// </summary>
        /// <param name="start"><ja>削除する開始位置</ja><en>Start position</en></param>
        /// <param name="count"><ja>削除する文字数</ja><en>Count to delete</en></param>
        /// <param name="dec"><ja>末尾の新しい空白領域のテキスト装飾</ja><en>text decoration for the new empty spaces at the tail of the line</en></param>
        public void DeleteChars(int start, int count, TextDecoration dec) {
            char fillChar;
            TextDecoration fillDec = dec;
            if (fillDec != null) {
                fillDec = fillDec.RetainBackColor();
                if (fillDec.IsDefault) {
                    fillDec = null;
                    fillChar = '\0';
                }
                else {
                    fillChar = ' ';
                }
            }
            else {
                fillChar = '\0';
            }
            for (int i = start; i < _text.Length; i++) {
                int j = i + count;
                if (j < _text.Length) {
                    _text[i] = _text[j];
                    _attrs[i] = _attrs[j];
                }
                else {
                    _text[i] = fillChar;
                    _attrs[i] = new CharAttr(fillDec, CharGroup.LatinHankaku);
                }
            }
        }

        /// <summary>
        /// <ja>指定位置に指定された数だけの半角スペースを挿入します。</ja>
        /// <en>The half angle space only of the number specified for a specified position is inserted. </en>
        /// </summary>
        /// <param name="start"><ja>削除する開始位置</ja><en>Start position</en></param>
        /// <param name="count"><ja>挿入する半角スペースの数</ja><en>Count space to insert</en></param>
        /// <param name="dec"><ja>空白領域のテキスト装飾</ja><en>text decoration for the new empty spaces</en></param>
        public void InsertBlanks(int start, int count, TextDecoration dec) {
            TextDecoration fillDec = dec;
            if (fillDec != null) {
                fillDec = fillDec.RetainBackColor();
                if (fillDec.IsDefault)
                    fillDec = null;
            }
            for (int i = _text.Length - 1; i >= _caretColumn; i--) {
                int j = i - count;
                if (j >= _caretColumn) {
                    _text[i] = _text[j];
                    _attrs[i] = _attrs[j];
                }
                else {
                    _text[i] = ' ';
                    _attrs[i] = new CharAttr(fillDec, CharGroup.LatinHankaku);
                }
            }
        }

        /// <summary>
        /// <ja>
        /// データをエクスポートします。
        /// </ja>
        /// <en>
        /// Export the data.
        /// </en>
        /// </summary>
        /// <returns><ja>エクスポートされたGLineオブジェクト</ja><en>Exported GLine object</en></returns>
        public GLine Export() {
            GWord firstWord;
            GWord lastWord;

            CharAttr firstAttr = _attrs[0];
            if (firstAttr.Decoration == null)
                firstAttr.Decoration = TextDecoration.Default;
            firstWord = lastWord = new GWord(firstAttr.Decoration, 0, firstAttr.CharGroup);

            int limit = _text.Length;
            int offset;
            if (_text[0] == '\0') {
                offset = 0;
            }
            else {
                CharAttr prevAttr = firstAttr;
                for (offset = 1; offset < limit; offset++) {
                    char ch = _text[offset];
                    if (ch == '\0')
                        break;
                    else if (ch == GLine.WIDECHAR_PAD)
                        continue;

                    CharAttr attr = _attrs[offset];
                    if (attr.Decoration != prevAttr.Decoration || attr.CharGroup != prevAttr.CharGroup) {
                        if (attr.Decoration == null)
                            attr.Decoration = TextDecoration.Default;
                        GWord w = new GWord(attr.Decoration, offset, attr.CharGroup);
                        lastWord.Next = w;
                        lastWord = w;
                        prevAttr = attr;
                    }
                }
            }

            GLine line = new GLine((char[])_text.Clone(), offset, firstWord);
            line.EOLType = _eolType;
            return line;
        }
    }

#if UNITTEST
    [TestFixture]
    public class GLineManipulatorTests {

        [Test]
        public void PutChar1() {
            Assert.AreEqual("いaaz", TestPutChar("aaaaz", 0, 'い'));
        }
        [Test]
        public void PutChar2() {
            Assert.AreEqual("い az", TestPutChar("aあaz", 0, 'い'));
        }
        [Test]
        public void PutChar3() {
            Assert.AreEqual("b あz", TestPutChar("ああz", 0, 'b'));
        }
        [Test]
        public void PutChar4() {
            Assert.AreEqual("いあz", TestPutChar("ああz", 0, 'い'));
        }
        [Test]
        public void PutChar5() {
            Assert.AreEqual(" bあz", TestPutChar("ああz", 1, 'b'));
        }
        [Test]
        public void PutChar6() {
            Assert.AreEqual(" いaz", TestPutChar("あaaz", 1, 'い'));
        }
        [Test]
        public void PutChar7() {
            Assert.AreEqual(" い z", TestPutChar("ああz", 1, 'い'));
        }

        private static string TestPutChar(string initial, int col, char ch) {
            GLineManipulator m = new GLineManipulator();
            m.Load(GLine.ToCharArray(initial), col);
            //Debug.WriteLine(String.Format("Test{0}  [{1}] col={2} char={3}", num, SafeString(m._text), m.CaretColumn, ch));
            m.PutChar(ch, TextDecoration.ClonedDefault());
            //Debug.WriteLine(String.Format("Result [{0}] col={1}", SafeString(m._text), m.CaretColumn));
            return SafeString(m.InternalBuffer);
        }
    }
#endif

    /// <summary>
    /// <ja>
    /// 改行コードの種類を示します。
    /// </ja>
    /// <en>
    /// Kind of Line feed code
    /// </en>
    /// </summary>
    public enum EOLType {
        /// <summary>
        /// <ja>改行せずに継続します。</ja><en>It continues without changing line.</en>
        /// </summary>
        Continue,
        /// <summary>
        /// <ja>CRLFで改行します。</ja><en>It changes line with CRLF. </en>
        /// </summary>
        CRLF,
        /// <summary>
        /// <ja>CRで改行します。</ja><en>It changes line with CR. </en>
        /// </summary>
        CR,
        /// <summary>
        /// <ja>LFで改行します。</ja><en>It changes line with LF. </en>
        /// </summary>
        LF
    }

    /// <summary>
    /// <ja>文字がどのように表示されるかを示します。</ja>
    /// <en>Represents how the characters will be displayed.</en>
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Hankaku" and "Zenkaku" are representing the width of the character.
    /// A "Hankaku" character will be displayed using single column width,
    /// and a "Zenkaku" character will be displayed using two column width.
    /// </para>
    /// </remarks>
    public enum CharGroup {
        /// <summary>
        /// <ja>メインフォントで表示される半角文字。</ja>
        /// <en>Hankaku characters to be displayed using main font.</en>
        /// </summary>
        LatinHankaku,
        /// <summary>
        /// <ja>CJKフォントで表示される半角文字。</ja>
        /// <en>Hankaku characters to be displayed using CJK font.</en>
        /// </summary>
        CJKHankaku,
        /// <summary>
        /// <ja>CJKフォントで表示される全角文字。</ja>
        /// <en>Zenkaku characters to be displayed using CJK font.</en>
        /// </summary>
        CJKZenkaku,
    }

    public static class CharGroupUtil {
        public static int GetColumnsPerCharacter(CharGroup cg) {
            if (cg == CharGroup.CJKZenkaku)
                return 2;
            else
                return 1;
        }

        public static bool IsCJK(CharGroup cg) {
            return (cg == CharGroup.CJKHankaku || cg == CharGroup.CJKZenkaku);
        }
    }


    //単語区切り設定。まあPreferenceにする間でもないだろう
    /// <exclude/>
    public class ASCIIWordBreakTable {
        public const int LETTER = 0;
        public const int SYMBOL = 1;
        public const int SPACE = 2;
        public const int NOT_ASCII = 3;

        private int[] _data;

        public ASCIIWordBreakTable() {
            _data = new int[0x80];
            Reset();
        }

        public void Reset() { //通常設定にする
            //制御文字パート
            for (int i = 0; i <= 0x20; i++)
                _data[i] = SPACE;
            _data[0x7F] = SPACE; //DEL

            //通常文字パート
            for (int i = 0x21; i <= 0x7E; i++) {
                char c = (char)i;
                if (('0' <= c && c <= '9') || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '_')
                    _data[i] = LETTER;
                else
                    _data[i] = SYMBOL;
            }
        }

        public int GetAt(char ch) {
            Debug.Assert(ch < 0x80);
            return _data[(int)ch];
        }

        //一文字設定
        public void Set(char ch, int type) {
            Debug.Assert(ch < 0x80);
            _data[(int)ch] = type;
        }

        //インスタンス
        private static ASCIIWordBreakTable _instance;

        public static ASCIIWordBreakTable Default {
            get {
                if (_instance == null)
                    _instance = new ASCIIWordBreakTable();
                return _instance;
            }
        }
    }

}
