/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Encoding.cs,v 1.7 2012/01/29 14:42:06 kzmi Exp $
 */
using System;
using System.Text;

using Poderosa.ConnectionParam;
using Poderosa.Util;
using Poderosa.Document;

namespace Poderosa.Terminal {
    //encoding関係
    internal abstract class EncodingProfile {

        private Encoding _encoding;
        private EncodingType _type;
        private byte[] _buffer;
        private int _cursor;
        private int _byte_len;
        private char[] _tempOneCharArray;

        protected EncodingProfile(EncodingType t, Encoding enc) {
            _type = t;
            _encoding = enc;
            _buffer = new byte[3]; //今は１文字は最大３バイト
            _cursor = 0;
            _tempOneCharArray = new char[1]; //APIの都合で長さ１のchar[]が必要なとき使う
        }

        //Check if the byte is the first byte of a character which should be converted the character code.
        protected abstract bool IsLeadByte(byte b);

        //先頭バイトから、文字が何バイトで構成されているかを返す
        protected abstract int GetCharLength(byte b);

        //UTFのBOMなど、デコードの結果出てきても無視すべき文字かを判定
        protected abstract bool IsIgnoreableChar(char c);

        // Do mapping from original character to alternative character
        protected abstract char Map(char c);

        public Encoding Encoding {
            get {
                return _encoding;
            }
        }

        public EncodingType Type {
            get {
                return _type;
            }
        }

        public byte[] Buffer {
            get {
                return _buffer;
            }
        }

        public byte[] GetBytes(char[] chars) {
            return _encoding.GetBytes(chars);
        }

        //NOTE 潜在的には_tempOneCharArrayの使用でマルチスレッドでの危険がある。
        public byte[] GetBytes(char ch) {
            _tempOneCharArray[0] = ch;
            return _encoding.GetBytes(_tempOneCharArray);
        }

        public bool IsInterestingByte(byte b) {
            //"b>=33"のところはもうちょっとまじめに判定するべき。
            //文字の間にエスケープシーケンスが入るケースへの対応。
            return _cursor == 0 ? IsLeadByte(b) : b >= 33;
        }

        public void Reset() {
            _cursor = 0;
            _byte_len = 0;
        }

        /// <summary>
        /// <para>Append one byte.</para>
        /// <para>If the byte sequence is representing a character in this encoding, this method returns the character.
        /// Otherwise, this method returns \0 to indicate that more bytes are required.</para>
        /// <para>A character to be returned may be converted to a character in Unicode's private-use area by <see cref="Poderosa.Document.Unicode"/>.</para>
        /// </summary>
        /// <remarks>
        /// <para>By convert the character code, informations about font-type or character's width, that are appropriate for the current encoding,
        /// can get from the character code. See <see cref="Poderosa.Document.Unicode"/>.</para>
        /// </remarks>
        /// <param name="b"></param>
        /// <returns></returns>
        public char PutByte(byte b) {
            if (_cursor == 0)
                _byte_len = GetCharLength(b);
            _buffer[_cursor++] = b;
            if (_cursor == _byte_len) {
                _encoding.GetChars(_buffer, 0, _byte_len, _tempOneCharArray, 0);
                _cursor = 0;
                if (IsIgnoreableChar(_tempOneCharArray[0]))
                    return '\0';
                char ch = _tempOneCharArray[0];
                return Map(ch);
            }
            return '\0';
        }

        public static EncodingProfile Get(EncodingType et) {
            EncodingProfile p = null;
            switch (et) {
                case EncodingType.ISO8859_1:
                    p = new ISO8859_1Profile();
                    break;
                case EncodingType.EUC_JP:
                    p = new EUCJPProfile();
                    break;
                case EncodingType.SHIFT_JIS:
                    p = new ShiftJISProfile();
                    break;
                case EncodingType.UTF8:
                    p = new UTF8Profile();
                    break;
                case EncodingType.UTF8_Latin:
                    p = new UTF8_LatinProfile();
                    break;
                case EncodingType.GB2312:
                    p = new GB2312Profile();
                    break;
                case EncodingType.BIG5:
                    p = new Big5Profile();
                    break;
                case EncodingType.EUC_CN:
                    p = new EUCCNProfile();
                    break;
                case EncodingType.EUC_KR:
                    p = new EUCKRProfile();
                    break;
                case EncodingType.OEM850:
                    p = new OEM850Profile();
                    break;
            }
            return p;
        }

        private abstract class DirectMapEncodingProfile : EncodingProfile {

            protected DirectMapEncodingProfile(EncodingType t, Encoding enc)
                : base(t, enc) {
            }

            protected override char Map(char c) {
                if (Unicode.IsPrivateUseArea(c))
                    return '\u0020';
                else
                    return c;
            }
        }

        private abstract class CJKMapEncodingProfile : EncodingProfile {

            protected CJKMapEncodingProfile(EncodingType t, Encoding enc)
                : base(t, enc) {
            }

            protected override char Map(char c) {
                if (Unicode.IsPrivateUseArea(c)) // Private use area
                    return '\u0020';
                else
                    return Unicode.ToPrivate(c);
            }
        }

        //NOTE これらはメソッドのoverrideでなくdelegateでまわしたほうが効率は若干よいのかも
        private class ISO8859_1Profile : DirectMapEncodingProfile {
            public ISO8859_1Profile()
                : base(EncodingType.ISO8859_1, Encoding.GetEncoding("iso-8859-1")) {
            }
            protected override int GetCharLength(byte b) {
                return 1;
            }
            protected override bool IsLeadByte(byte b) {
                return b >= 0xA0;
            }
            protected override bool IsIgnoreableChar(char c) {
                return false;
            }
        }
        private class ShiftJISProfile : CJKMapEncodingProfile {
            public ShiftJISProfile()
                : base(EncodingType.SHIFT_JIS, Encoding.GetEncoding("shift_jis")) {
            }
            protected override int GetCharLength(byte b) {
                return (b >= 0xA1 && b <= 0xDF) ? 1 : 2;
            }
            protected override bool IsLeadByte(byte b) {
                return b >= 0x81 && b <= 0xFC;
            }
            protected override bool IsIgnoreableChar(char c) {
                return false;
            }
        }
        private class EUCJPProfile : CJKMapEncodingProfile {
            public EUCJPProfile()
                : base(EncodingType.EUC_JP, Encoding.GetEncoding("euc-jp")) {
            }
            protected override int GetCharLength(byte b) {
                return b == 0x8F ? 3 : b >= 0x8E ? 2 : 1;
            }
            protected override bool IsLeadByte(byte b) {
                return b >= 0x8E && b <= 0xFE;
            }
            protected override bool IsIgnoreableChar(char c) {
                return false;
            }
        }
        private class UTF8Profile : CJKMapEncodingProfile {
            public UTF8Profile()
                : base(EncodingType.UTF8, Encoding.UTF8) {
            }
            protected override int GetCharLength(byte b) {
                return b >= 0xE0 ? 3 : b >= 0x80 ? 2 : 1;
            }
            protected override bool IsLeadByte(byte b) {
                return b >= 0x80;
            }
            protected override bool IsIgnoreableChar(char c) {
                return c == '\uFFFE' || c == '\uFEFF';
            }
        }
        private class UTF8_LatinProfile : DirectMapEncodingProfile {
            public UTF8_LatinProfile()
                : base(EncodingType.UTF8_Latin, Encoding.UTF8) {
            }
            protected override int GetCharLength(byte b) {
                return b >= 0xE0 ? 3 : b >= 0x80 ? 2 : 1;
            }
            protected override bool IsLeadByte(byte b) {
                return b >= 0x80;
            }
            protected override bool IsIgnoreableChar(char c) {
                return c == '\uFFFE' || c == '\uFEFF';
            }
        }
        private class GB2312Profile : CJKMapEncodingProfile {
            public GB2312Profile()
                : base(EncodingType.GB2312, Encoding.GetEncoding("gb2312")) {
            }
            protected override int GetCharLength(byte b) {
                return 2;
            }
            protected override bool IsLeadByte(byte b) {
                return b >= 0xA1 && b <= 0xF7;
            }
            protected override bool IsIgnoreableChar(char c) {
                return false;
            }
        }
        private class Big5Profile : CJKMapEncodingProfile {
            public Big5Profile()
                : base(EncodingType.BIG5, Encoding.GetEncoding("big5")) {
            }
            protected override int GetCharLength(byte b) {
                return 2;
            }
            protected override bool IsLeadByte(byte b) {
                return b >= 0x81 && b <= 0xFE;
            }
            protected override bool IsIgnoreableChar(char c) {
                return false;
            }
        }
        private class EUCCNProfile : CJKMapEncodingProfile {
            public EUCCNProfile()
                : base(EncodingType.EUC_CN, Encoding.GetEncoding("euc-cn")) {
            }
            protected override int GetCharLength(byte b) {
                return 2;
            }
            protected override bool IsLeadByte(byte b) {
                return b >= 0xA1 && b <= 0xF7;
            }
            protected override bool IsIgnoreableChar(char c) {
                return false;
            }
        }
        private class EUCKRProfile : CJKMapEncodingProfile {
            public EUCKRProfile()
                : base(EncodingType.EUC_KR, Encoding.GetEncoding("euc-kr")) {
            }
            protected override int GetCharLength(byte b) {
                return 2;
            }
            protected override bool IsLeadByte(byte b) {
                return b >= 0xA1 && b <= 0xFE;
            }
            protected override bool IsIgnoreableChar(char c) {
                return false;
            }
        }
        private class OEM850Profile : DirectMapEncodingProfile {
            public OEM850Profile()
                : base(EncodingType.OEM850, Encoding.GetEncoding(850)) {
            }
            protected override int GetCharLength(byte b) {
                return 1;
            }
            protected override bool IsLeadByte(byte b) {
                return b >= 0x80;
            }
            protected override bool IsIgnoreableChar(char c) {
                return false;
            }
        }
    }
}
