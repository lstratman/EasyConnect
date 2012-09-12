/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: Encoding.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Text;

using Poderosa.ConnectionParam;
using Poderosa.Toolkit;

namespace Poderosa.Communication
{
	//encoding関係
	public abstract class EncodingProfile {
		
		private Encoding _encoding;
		private EncodingType _type;
		private byte[] _buffer;
		private int _cursor;
		private int _byte_len;

		protected EncodingProfile(EncodingType t, Encoding enc) {
			_type = t;
			_encoding = enc;
			_buffer = new byte[3]; //今は１文字は最大３バイト
			_cursor = 0;
		}

		public abstract bool IsLeadByte(byte b);
		public abstract int  GetCharLength(byte b);

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
		internal byte[] Buffer {
			get {
				return _buffer;
			}
		}

		internal byte[] GetBytes(char[] chars) {
			return _encoding.GetBytes(chars);
		}
		internal byte[] GetBytes(char ch) {
			char[] t = new char[1];
			t[0] = ch;
			return _encoding.GetBytes(t);
		}

		internal bool IsInterestingByte(byte b) {
			return _cursor==0? IsLeadByte(b) : b>=33; //"b>=33"のところはもうちょっとまじめに判定するべき
		}

		internal int Decode(byte[] data, char[] result) {
			return _encoding.GetChars(data, 0, data.Length, result, 0);
		}

		internal void Reset() {
			_cursor = 0;
			_byte_len = 0;
		}

		//１バイトを追加する。文字が完成すればデコードしてその文字を返す。まだ続きのバイトが必要なら\0を返す
		internal char PutByte(byte b) {
			if(_cursor==0)
				_byte_len = GetCharLength(b);
			_buffer[_cursor++] = b;
			if(_cursor==_byte_len) {
				char[] result = new Char[1];
				_encoding.GetChars(_buffer, 0, _byte_len, result, 0);
				_cursor = 0;
				return result[0];
			}
			return '\0';
		}

		public static EncodingProfile Get(EncodingType et) {
			EncodingProfile p = null;
			switch(et) {
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
			}
			return p;
		}

		class ISO8859_1Profile : EncodingProfile {
			public ISO8859_1Profile() : base(EncodingType.ISO8859_1, Encoding.GetEncoding("iso-8859-1")) {
			}
			public override int GetCharLength(byte b) {
				return 1;
			}
			public override bool IsLeadByte(byte b) {
				return b>=0xA0 && b<=0xFE;
			}
		}
		class ShiftJISProfile : EncodingProfile {
			public ShiftJISProfile() : base(EncodingType.SHIFT_JIS, Encoding.GetEncoding("shift_jis")) {
			}
			public override int GetCharLength(byte b) {
				return (b>=0xA1 && b<=0xDF)? 1 : 2;
			}
			public override bool IsLeadByte(byte b) {
				return b>=0x81 && b<=0xFC;
			}
		}
		class EUCJPProfile : EncodingProfile {
			public EUCJPProfile() : base(EncodingType.EUC_JP, Encoding.GetEncoding("euc-jp")) {
			}
			public override int GetCharLength(byte b) {
				return b==0x8F? 3 : b>=0x8E? 2 : 1;
			}
			public override bool IsLeadByte(byte b) {
				return b>=0x8E && b<=0xFE;
			}
		}
		class UTF8Profile : EncodingProfile {
			public UTF8Profile() : base(EncodingType.UTF8, Encoding.UTF8) {
			}
			public override int GetCharLength(byte b) {
				return b>=0xE0? 3 : b>=0x80? 2 : 1;
			}
			public override bool IsLeadByte(byte b) {
				return b>=0x80;
			}
		}
	}
}
