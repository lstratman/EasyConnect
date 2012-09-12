/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TerminalUtil.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

using Granados.SSHC;

using Poderosa.Config;
using Poderosa.ConnectionParam;
using Poderosa.Toolkit;
using Poderosa.Communication;
using Poderosa.SSH;

namespace Poderosa.Terminal {

	public enum TerminalMode { Normal, Application }


	public class TerminalUtil {
		public static char[] NewLineChars(NewLine nl) {
			switch(nl) {
				case NewLine.CR:
					return new char[1] { '\r' };
				case NewLine.LF:
					return new char[1] { '\n' };
				case NewLine.CRLF:
					return new char[2] { '\r','\n' };
				default:
					throw new ArgumentException("Unknown NewLine "+nl);
			}
		}
		public static NewLine NextNewLineOption(NewLine nl) {
			switch(nl) {
				case NewLine.CR:
					return NewLine.LF;
				case NewLine.LF:
					return NewLine.CRLF;
				case NewLine.CRLF:
					return NewLine.CR;
				default:
					throw new ArgumentException("Unknown NewLine "+nl);
			}
		}


		//有効なボーレートのリスト
		public static string[] BaudRates {
			get {
				return new string[] {"110", "300", "600", "1200", "2400", "4800",
										"9600", "14400", "19200", "38400", "57600", "115200"};
			}
		}
	}


	//これと同等の処理はToAscii APIを使ってもできるが、ちょっとやりづらいので逆引きマップをstaticに持っておく
	internal class KeyboardInfo {
		public static char[] _defaultGroup;
		public static char[] _shiftGroup;

		public static void Init() {
			_defaultGroup = new char[256];
			_shiftGroup   = new char[256];
			for(int i=32; i<128; i++) {
				short v = Win32.VkKeyScan((char)i);
				bool shift = (v & 0x0100)!=0;
				short body = (short)(v & 0x00FF);
				if(shift)
					_shiftGroup[body] = (char)i;
				else
					_defaultGroup[body] = (char)i;
			}
		}

		public static char Scan(Keys body, bool shift) {
			if(_defaultGroup==null) Init();

			//制御文字のうち単品のキーで送信できるもの
			if(body==Keys.Escape)
				return (char)0x1B;
			else if(body==Keys.Tab)
				return (char)0x09;
			else if(body==Keys.Back)
				return (char)0x08;
			else if(body==Keys.Delete)
				return (char)0x7F;

			if(shift)
				return _shiftGroup[(int)body];
			else
				return _defaultGroup[(int)body];
		}
	}
}
