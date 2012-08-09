/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: GLine.cs,v 1.3 2005/04/27 08:48:50 okajima Exp $
*/
using System;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using Poderosa.Forms;

using Poderosa.Terminal;
using Poderosa.UI;

namespace Poderosa.Text
{
	internal class RenderParameter {
		private int _linefrom;
		private int _linecount;
		private Rectangle _targetRect;

		public int LineFrom { 
			get {
				return _linefrom;
			}
			set {
				_linefrom=value;
			}
		}

		public int LineCount { 
			get {
				return _linecount;
			}
			set {
				_linecount=value;
			}
		}
		public Rectangle TargetRect { 
			get {
				return _targetRect;
			}
			set {
				_targetRect=value;
			}
		}
	}

	/// <summary>
	/// 行末の種類
	/// </summary>
	public enum EOLType {
		Continue,
		CRLF,
		CR,
		LF
	}

	public enum CharGroup {
		SingleByte, //unicodeで0x100未満の文字
		TwoBytes    //0x100以上の文字
	}

	/// <summary>
	/// GLineの構成要素。GWordはTextDecorationなどを共有する。
	/// </summary>
	public class GWord
	{
		private TextDecoration _decoration;
		private int _offset;
		private CharGroup _charGroup;
		private GWord _next;

		//しばしば参照するのでキャッシュする値
		internal int nextOffsetCache;
		internal int displayLengthCache;

		/// <summary>
		/// 表示用の装飾
		/// </summary>
		internal TextDecoration Decoration {
			get {
				return _decoration;
			}
		}
		/// <summary>
		/// 所属するGLineの中で何文字目から始まっているか
		/// </summary>
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
			set {
				_charGroup = value;
			}
		}

		/// <summary>
		/// 文字列、デコレーション、オフセットを指定するコンストラクタ。TypeはNormalになる。
		/// </summary>
		internal GWord(TextDecoration d, int o, CharGroup chargroup) {
			Debug.Assert(d!=null);
			_offset = o;
			_decoration = d;
			_next = null;
			_charGroup = chargroup;
			nextOffsetCache = -1;
			displayLengthCache = -1;
		}

		//Nextの値以外をコピーする
		public GWord StandAloneClone() {
			return new GWord(_decoration, _offset, _charGroup);
		}

		public GWord DeepClone() {
			GWord w = new GWord(_decoration, _offset, _charGroup);
			if(_next!=null)
				w._next = _next.DeepClone();
			return w;
		}

	}


	/// <summary>
	/// １行のデータ
	/// GWordへの分解は遅延評価される。
	/// </summary>
	public class GLine {
		static GLine() {
			InitLengthMap();
		}

		public const char WIDECHAR_PAD = '\uFFFF';

		private char[] _text;
		private EOLType _eolType;
		private int _id;

		private GWord _firstWord;

		private GLine _nextLine;
		private GLine _prevLine;

		public GLine(int length) {
			Debug.Assert(length>0);
			_text = new char[length];
			_firstWord = new GWord(TextDecoration.ClonedDefault(), 0, CharGroup.SingleByte);
			_id = -1;
		}
		public GLine(char[] data, GWord firstWord) {
			_text = (char[])data.Clone();
			_firstWord = firstWord;
			_id = -1;
		}
		public GLine Clone() {
			GLine nl = new GLine(_text, _firstWord.DeepClone());
			nl._eolType = _eolType;
			nl._id = _id;
			return nl;
		}


		public void Clear() {
			for(int i=0; i<_text.Length; i++)
				_text[i] = '\0';
			_firstWord = new GWord(TextDecoration.ClonedDefault(), 0, CharGroup.SingleByte);
		}
		internal void Clear(TextDecoration dec) {
			for(int i=0; i<_text.Length; i++)
				_text[i] = ' ';
			_firstWord = new GWord(dec, 0, CharGroup.SingleByte);
		}
		public int Length {
			get {
				return _text.Length;
			}
		}

		/// <summary>
		/// インデクスを指定してGWordを返す。レンダリング済みかどうかは考慮していない。
		/// </summary>
		public GWord FirstWord { 
			get {
				return _firstWord;
			}
		}
		public char[] Text {
			get {
				return _text;
			}
		}

		public int DisplayLength {
			get {
				int i = 0;
				int m = _text.Length;
				for(i=0; i<m; i++) {
					if(_text[i]=='\0') break;
				}
				return i;
			}
		}
		public int CharLength {
			get {
				int n = _text.Length-1;
				while(n>=0 && _text[n]=='\0') n--;
				return n+1;
			}
		}

		//前後の単語区切りを見つける。返す位置は、posとGetWordBreakGroupの値が一致する中で遠い地点
		public int FindPrevWordBreak(int pos) {
			int v = ToCharGroupForWordBreak(_text[pos]);
			while(pos>=0) {
				if(v!=ToCharGroupForWordBreak(_text[pos])) return pos;
				pos--;
			}
			return -1;
		}
		public int FindNextWordBreak(int pos) {
			int v = ToCharGroupForWordBreak(_text[pos]);
			while(pos<_text.Length) {
				if(v!=ToCharGroupForWordBreak(_text[pos])) return pos;
				pos++;
			}
			return _text.Length;
		}
		private static int ToCharGroupForWordBreak(char ch) {
			if(('0'<=ch && ch<='9') || ('a'<=ch && ch<='z') || ('A'<=ch && ch<='Z') || ch=='_' || GEnv.Options.AdditionalWordElement.IndexOf(ch)!=-1)
				return 1;
			else if(ch<=0x20 || ch==0x40)
				return 2;
			else if(ch<=0x100)
				return 3;
			else //さらにここをUnicodeCategory等をみて適当にこしらえることもできるが
				return 4;
		}


		public int ID {
			get {
				return _id;
			}
			set {
				_id = value;
			}
		}

		//隣接行の設定　この変更はTerminalDocumentからのみ行うこと！
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

		internal void ExpandBuffer(int length) {
			if(length<=_text.Length) return;

			char[] current = _text;
			_text = new char[length];
			Array.Copy(current, 0, _text, 0, current.Length);
		}

		internal void Render(IntPtr hdc, RenderParameter param, RenderProfile prof, int y) {
			if(_text[0]=='\0') return; //何も描かなくてよい
			float fx = (float)param.TargetRect.Left;

			RectangleF rect = new RectangleF();
			rect.Y = param.TargetRect.Top+y;
			rect.Height = prof.Pitch.Height;

			GWord word = _firstWord;
			while(word != null) {
				
				rect.X = fx /*- prof.CharGap*/; //Nativeな描画では不要？
				rect.Width = param.TargetRect.Right - rect.X;
				int ix = (int)rect.X;
				int iy = (int)rect.Y;

				TextDecoration dec = word.Decoration;

				//Brush brush = prof.CalcTextBrush(dec);
				uint forecolorref = DrawUtil.ToCOLORREF(prof.CalcTextColor(dec));
				Color bkcolor = prof.CalcBackColor(dec);
				uint bkcolorref   = DrawUtil.ToCOLORREF(bkcolor);
				IntPtr hfont = prof.CalcHFONT_NoUnderline(dec, word.CharGroup);
				Win32.SelectObject(hdc, hfont);
				Win32.SetTextColor(hdc, forecolorref);
				Win32.SetBkColor(hdc, bkcolorref);
				Win32.SetBkMode(hdc, bkcolor==prof.BackColor? 1 : 2); //基本背景色と一緒ならTRANSPARENT, 異なればOPAQUE
				IntPtr bkbrush = bkcolor==prof.BackColor? IntPtr.Zero : Win32.CreateSolidBrush(bkcolorref);
				
				int display_length = WordDisplayLength(word);
				if(word.Decoration==null) { //装飾なし
					//g.DrawString(WordText(word), font, brush, rect);
					DrawWord(hdc, ix, iy, word);
				}
				else {
					//if(dec.Bold || (!prof.UsingIdenticalFont && word.CharGroup==CharGroup.TwoBytes))
					if(dec.Bold || word.CharGroup==CharGroup.TwoBytes) //同じフォント指定でも日本語が半角の２倍でない場合あり。パフォーマンス問題はクリアされつつあるので確実に１文字ずつ描画
						DrawStringByOneChar2(hdc, word, display_length, bkbrush, rect.X, iy, prof);
					else
						DrawWord(hdc, ix, iy, word); //いまやアホな描画エンジンの問題からは解放された！
				}
				//Debug.WriteLine("PW="+p.Pitch.Width+",TL="+(pb.Text.Length*p.Pitch.Width)+", real="+g.MeasureString(pb.Text, p.Font).Width);
				if(dec.Underline)
					DrawUnderline(hdc, forecolorref, ix, iy+(int)prof.Pitch.Height-1, (int)(prof.Pitch.Width * display_length));

				fx += prof.Pitch.Width * display_length;
				word = word.Next;
				if(bkbrush!=IntPtr.Zero) Win32.DeleteObject(bkbrush);
			}

		}

		private void DrawUnderline(IntPtr hdc, uint col, int x, int y, int length) {
			//Underlineがつくことはあまりないだろうから毎回Penを作る。問題になりそうだったらそのときに考えよう
			IntPtr pen = Win32.CreatePen(0, 1, col);
			IntPtr prev = Win32.SelectObject(hdc, pen);
			Win32.POINT pt = new Win32.POINT();
			Win32.MoveToEx(hdc, x, y, out pt);
			Win32.LineTo(hdc, x+length, y);
			Win32.SelectObject(hdc, prev);
			Win32.DeleteObject(pen);
		}

		private void DrawWord(IntPtr hdc, int x, int y, GWord word) {
			unsafe {
				int len;

				if(word.CharGroup==CharGroup.SingleByte) {
					fixed(char* p = &_text[0]) {
						len = WordNextOffset(word) - word.Offset;
						Win32.TextOut(hdc, x, y, p+word.Offset, len);
						//Win32.ExtTextOut(hdc, x, y, 4, null, p+word.Offset, len, null);
					}
				}
				else {
					string t = WordText(word);
					fixed(char* p = t) {
						len = t.Length;
						Win32.TextOut(hdc, x, y, p, len);
						//Win32.ExtTextOut(hdc, x, y, 4, null, p, len, null);
					}
				}
			
			}
		}
		private void DrawStringByOneChar2(IntPtr hdc, GWord word, int display_length, IntPtr bkbrush, float fx, int y, RenderProfile prof) {
			float pitch = prof.Pitch.Width;
			int nextoffset = WordNextOffset(word);
			if(bkbrush!=IntPtr.Zero) { //これがないと日本語文字ピッチが小さいとき選択時のすきまができる場合がある
				Win32.RECT rect = new Win32.RECT();
				rect.left = (int)fx;
				rect.top  = y;
				rect.right = (int)(fx + pitch*display_length);
				rect.bottom = y + (int)prof.Pitch.Height;
				Win32.FillRect(hdc, ref rect, bkbrush);
			}
			for(int i=word.Offset; i<nextoffset; i++) {
				char ch = _text[i];
				if(ch=='\0') break;
				if(ch==GLine.WIDECHAR_PAD) continue;
				unsafe {
					Win32.TextOut(hdc, (int)fx, y, &ch, 1);
				}
				fx += pitch * CalcDisplayLength(ch);
			}
		}

		/*
		 * //!!.NETの描画バグ
		 * Graphics#DrawStringに渡す文字列は、スペースのみで構成されていると無視されてしまうようだ。
		 * これだとアンダーラインだけを引きたいときなどに困る。調べたところ、末尾にタブをつけるとこの仕組みをだますことができることが判明
		 * .NETの次のバージョンでは直っていることを期待
		 */
		private string WordTextForFuckingDotNet(GWord word) {
			int nextoffset = WordNextOffset(word);
			if(nextoffset==0)
				return "";
			else {
				bool last_is_space = false;
				Debug.Assert(nextoffset-word.Offset >= 0);
				if(word.CharGroup==CharGroup.SingleByte) {
					last_is_space = _text[nextoffset-1]==' ';
					if(last_is_space)
						return new string(_text, word.Offset, nextoffset-word.Offset)+'\t';
					else
						return new string(_text, word.Offset, nextoffset-word.Offset);
				}
				else {
					char[] buf = new char[256];
					int o = word.Offset, i=0;
					while(o < nextoffset) {
						char ch = _text[o];
						if(ch!=GLine.WIDECHAR_PAD) {
							last_is_space = ch==' ';
							buf[i++] = ch;
						}
						o++;
					}

					if(last_is_space)
						buf[i++] = (char)'\t';

					return new string(buf, 0, i);
				}
			}
		}

		private string WordText(GWord word) {
			int nextoffset = WordNextOffset(word);
			if(nextoffset==0)
				return "";
			else {
				Debug.Assert(nextoffset-word.Offset >= 0);
				if(word.CharGroup==CharGroup.SingleByte)
					return new string(_text, word.Offset, nextoffset-word.Offset);
				else {
					char[] buf = new char[256];
					int o = word.Offset, i=0;
					while(o < nextoffset) {
						char ch = _text[o];
						if(ch!=GLine.WIDECHAR_PAD)
							buf[i++] = ch;
						o++;
					}
					return new string(buf, 0, i);
				}
			}
		}
		private int WordDisplayLength(GWord word) {
			//ここは呼ばれることがとても多いのでキャッシュを設ける
			int cache = word.displayLengthCache;
			if(cache < 0) {
				int nextoffset = WordNextOffset(word);
				int l = nextoffset - word.Offset;
				word.displayLengthCache = l;
				return l;
			}
			else
				return cache;
		}

		internal int WordNextOffset(GWord word) {
			//ここは呼ばれることがとても多いのでキャッシュを設ける
			int cache = word.nextOffsetCache;
			if(cache < 0) {
				if(word.Next==null) {
					int i = _text.Length-1;
					while(i>=0 && _text[i]=='\0')
						i--;
					word.nextOffsetCache = i+1;
					return i+1;
				}
				else {
					word.nextOffsetCache = word.Next.Offset;
					return word.Next.Offset;
				}
			}
			else
				return cache;
		}
		internal void Append(GWord w) {
			if(_firstWord==null)
				_firstWord = w;
			else
				this.LastWord.Next = w;
		}
		public GWord LastWord {
			get {
				GWord w = _firstWord;
				while(w.Next!=null)
					w = w.Next;
				return w;
			}
		}


		//indexの位置の表示を反転した新しいGLineを返す
		//inverseがfalseだと、GWordの分割はするがDecorationの反転はしない。ヒクヒク問題の対処として実装。
		internal GLine InverseCaret(int index, bool inverse, bool underline) {
			ExpandBuffer(index+1);
			if(_text[index]==WIDECHAR_PAD) index--;
			GLine ret = new GLine(_text, null);
			ret.ID = _id;
			ret.EOLType = _eolType;
			
			GWord w = _firstWord;
			int nextoffset = 0;
			while(w!=null) {
				nextoffset = WordNextOffset(w);
				if(w.Offset<=index && index<nextoffset) {
					//!!tailから順に連結した方が効率はよい
					if(w.Offset<index) {
						GWord head = new GWord(w.Decoration, w.Offset, w.CharGroup);
						ret.Append(head);
					}

					TextDecoration dec = (TextDecoration)w.Decoration.Clone();
					if(inverse) {
						//色つきキャレットのサポート
						dec.ToCaretStyle();
					}
					if(underline) dec.Underline = true;
					GWord mid = new GWord(dec, index, w.CharGroup);
					ret.Append(mid);

					if(index+CalcDisplayLength(_text[index]) < nextoffset) {
						GWord tail = new GWord(w.Decoration, index+CalcDisplayLength(_text[index]), w.CharGroup);
						ret.Append(tail);
					}
				}
				else
					ret.Append(w.StandAloneClone());

				w = w.Next;
			}

			//!!この、キャレット位置にスペースを入れるのはInverseとは違う処理であるから分離すること
			if(nextoffset<=index) {
				while(nextoffset<=index) {
					Debug.Assert(nextoffset < ret.Text.Length);
					ret.Text[nextoffset++] = ' ';
				}
				TextDecoration dec = TextDecoration.ClonedDefault();
				if(inverse) {
					dec.ToCaretStyle();
				}
				if(underline) dec.Underline = true;
				ret.Append(new GWord(dec, index, CharGroup.SingleByte));
			}

			return ret;

		}

		internal GLine InverseRange(int from, int to) {
			ExpandBuffer(Math.Max(from+1,to)); //激しくリサイズしたときなどにこの条件が満たせないことがある
			Debug.Assert(from>=0 && from<_text.Length);
			if(from<_text.Length && _text[from]==WIDECHAR_PAD) from--;
			if(to>0 && to-1<_text.Length && _text[to-1]==WIDECHAR_PAD) to--;

			GLine ret = new GLine(_text, null);
			ret.ID = _id;
			ret.EOLType = _eolType;
			//装飾の配列をセット
			TextDecoration[] dec = new TextDecoration[_text.Length];
			GWord w = _firstWord;
			while(w!=null) {
				Debug.Assert(w.Decoration!=null);
				dec[w.Offset] = w.Decoration;
				w = w.Next;
			}

			//反転開始点
			TextDecoration original = null;
			TextDecoration inverse = null;
			for(int i=from; i>=0; i--) {
				if(dec[i]!=null) {
					original = dec[i];
					break;
				}
			}
			Debug.Assert(original!=null);
			inverse = (TextDecoration)original.Clone();
			inverse.Inverse();
			dec[from] = inverse;

			//範囲に渡って反転作業
			for(int i=from+1; i<to; i++) {
				if(i<dec.Length && dec[i]!=null) {
					original = dec[i];
					inverse = (TextDecoration)original.Clone();
					inverse.Inverse();
					dec[i] = inverse;
				}
			}

			if(to<dec.Length && dec[to]==null) dec[to] = original;

			//これに従ってGWordを作る
			w = null;
			for(int i=dec.Length-1; i>=0; i--) {
				char ch = _text[i];
				if(dec[i]!=null && ch!='\0') {
					int j = i;
					if(ch==WIDECHAR_PAD) j++;
					GWord ww = new GWord(dec[i], j, CalcCharGroup(ch));
					ww.Next = w;
					w = ww;
				}
			}
			ret.Append(w);

			return ret;
		}

		//この領域の文字の幅は本当はフォント依存だが、多くの日本語環境では全角として扱われる模様。BSで消すと２個来たりする
		private static byte[] _length_map_0x80_0xFF;
		private static byte[] _length_map_0x2500_0x25FF;

		private static void InitLengthMap() {
			_length_map_0x80_0xFF = new byte[0x80];
			for(int i=0; i<_length_map_0x80_0xFF.Length; i++) {
				int t = i+0x80;
				//    §         ¨         °         ±         ´         ¶          ×         ÷
				if(t==0xA7 || t==0xA8 || t==0xB0 || t==0xB1 || t==0xB4 || t==0xB6 || t==0xD7 || t==0xF7)
					_length_map_0x80_0xFF[i] = 2;
				else
					_length_map_0x80_0xFF[i] = 1;
			}

            //全角半角混在ゾーン
			_length_map_0x2500_0x25FF = new byte[] {
              //─ ━ │ ┃                        ┌       ┏
                2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, //2500-0F
              //┐       ┓ └       ┗ ┘       ┛ ├ ┝
                2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 2, 1, 1, //2510-1F
              //┠       ┣ ┤ ┥       ┨       ┫ ┬       ┯
                2, 1, 1, 2, 2, 2, 1, 1, 2, 1, 1, 2, 2, 1, 1, 2, //2520-2F
              //┰       ┳ ┴       ┷ ┸       ┻ ┼       ┿
                2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, //2530-3F
              //      ╂                         ╋
                1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, //2540-4F
              //
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //2550-5F
              //
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //2560-6F
              //
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //2570-7F
              //
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //2580-8F
              //
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //2590-9F
              //■ □
                2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //25A0-AF
              //      ▲ △                         ▼ ▽
                1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, //25B0-BF
              //                  ◆ ◇          ○       ◎ ●
                1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 2, 1, 1, 2, 2, //25C0-CF
              //
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //25D0-DF
              //                                             ◯
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, //25E0-EF
              //
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1  //25F0-FF
			};
			Debug.Assert(_length_map_0x2500_0x25FF.Length==256);
			
		}

		//文字によって描画幅を決める
		internal static int CalcDisplayLength(char ch) {
			if(ch >= 0x100) {
				if(0xFF61<=ch && ch<=0xFF9F) //半角カナ
					return 1;
				else if(0x2500<=ch && ch<=0x25FF) //罫線等特殊記号
					return _length_map_0x2500_0x25FF[ch-0x2500];
				else
					return 2;
			}
			else if(ch >= 0x80) {
				return _length_map_0x80_0xFF[ch-0x80];
			}
			else
				return 1; //本当はtabなどあるかもしれないのでもう少し真面目に計算すべき
		}
		//ASCIIか日本語文字か フォントの選択に使う
		internal static CharGroup CalcCharGroup(char ch) {
			if(ch < 0x80)
				return CharGroup.SingleByte; 
			else if(ch < 0x100)
				return _length_map_0x80_0xFF[ch-0x80]==1? CharGroup.SingleByte : CharGroup.TwoBytes;
			else {
				if(0x2500 <= ch && ch <= 0x25FF) //罫線は日本語フォントは使わない
					return _length_map_0x2500_0x25FF[ch-0x2500]==1? CharGroup.SingleByte : CharGroup.TwoBytes; 
				else
					return CharGroup.TwoBytes;
			}
		}
	}

	/// <summary>
	/// 文字の追加削除などをしてGLineを操作する。例えばターミナルがこのクラスを経由してドキュメントの特定のGLineを書き換えるのに使う。
	/// </summary>
	internal class GLineManipulator {

		private char[] _text;
		private TextDecoration[] _decorations;

		private int _caretColumn;

		private TextDecoration _defaultDecoration;

		private EOLType _eolType;

		/// <summary>
		/// 空で構築
		/// </summary>
		public GLineManipulator(int length) {
			_decorations = new TextDecoration[length];
			_text = new char[length];
			Clear(length);
		}
		/// <summary>
		/// 全内容を破棄する
		/// </summary>
		public void Clear(int length) {
			if(length!=_text.Length) {
				_decorations = new TextDecoration[length];
				_text = new char[length];
			}
			else {
				for(int i=0; i<_decorations.Length; i++) _decorations[i] = null;
				for(int i=0; i<_text.Length;        i++) _text[i] = '\0';
			}
			_caretColumn = 0;
			_eolType = EOLType.Continue;
		}

		public int CaretColumn {
			get {
				return _caretColumn;
			}
			set {
				Debug.Assert(value>=0 && value<=_text.Length);
				_caretColumn = value;
				value--;
				while(value>=0 && _text[value]=='\0')
					_text[value--] = ' ';
			}
		}

		public void CarriageReturn() {
			_caretColumn = 0;
			_eolType = EOLType.CR;
		}

		public bool IsEmpty {
			get {
				//_textを全部見る必要はないだろう
				return _caretColumn==0 && _text[0]=='\0';
			}
		}
		public TextDecoration DefaultDecoration {
			get {
				return _defaultDecoration;
			}
			set {
				_defaultDecoration = value;
			}
		}

		/// <summary>
		/// 引数と同じ内容で初期化する。lineの内容は破壊されない。
		/// 引数がnullのときは引数なしのコンストラクタと同じ結果になる。
		/// </summary>
		public void Load(GLine line, int cc) {
			if(line==null) { //これがnullになっているとしか思えないクラッシュレポートがあった。本来はないはずなんだが...
				Clear(80);
				return;
			}

			Clear(line.Length);
			GWord w = line.FirstWord;
			Debug.Assert(line.Text.Length==_text.Length);
			Array.Copy(line.Text, 0, _text, 0, _text.Length);
			
			int n = 0;
			while(w != null) {
				int nextoffset = line.WordNextOffset(w);
				while(n < nextoffset)
					_decorations[n++] = w.Decoration;
				w = w.Next;
			}

			_eolType = line.EOLType;
			ExpandBuffer(cc+1);
			this.CaretColumn = cc; //' 'で埋めることもあるのでプロパティセットを使う
		}
		public int BufferSize {
			get {
				return _text.Length;
			}
		}

		public void ExpandBuffer(int length) {
			if(length<=_text.Length) return;

			char[] current = _text;
			_text = new char[length];
			Array.Copy(current, 0, _text, 0, current.Length);
			TextDecoration[] current2 = _decorations;
			_decorations = new TextDecoration[length];
			Array.Copy(current2, 0, _decorations, 0, current2.Length);
		}

		public void PutChar(char ch, TextDecoration dec) {
			Debug.Assert(dec!=null);
			Debug.Assert(_caretColumn>=0);
			Debug.Assert(_caretColumn<_text.Length);

			//以下わかりにくいが、要は場合分け。これの仕様を書いた資料あり
			bool onZenkakuRight = (_text[_caretColumn] == GLine.WIDECHAR_PAD);
			bool onZenkaku = onZenkakuRight || (_text.Length>_caretColumn+1 && _text[_caretColumn+1] == GLine.WIDECHAR_PAD);

			if(onZenkaku) {
				//全角の上に書く
				if(!onZenkakuRight) {
					_text[_caretColumn] = ch;
					_decorations[_caretColumn] = dec;
					if(GLine.CalcDisplayLength(ch)==1) {
						//全角の上に半角を書いた場合、隣にスペースを入れないと表示が乱れる
						if(_caretColumn+1<_text.Length) _text[_caretColumn+1] = ' ';
						_caretColumn++;
					}
					else {
						_decorations[_caretColumn+1] = dec;
						_caretColumn+=2;
					}
				}
				else {
					_text[_caretColumn-1] = ' ';
					_text[_caretColumn]   = ch;
					_decorations[_caretColumn] = dec;
					if(GLine.CalcDisplayLength(ch)==2) {
						if(GLine.CalcDisplayLength(_text[_caretColumn+1])==2)
							if(_caretColumn+2<_text.Length) _text[_caretColumn+2] = ' ';
						_text[_caretColumn+1] = GLine.WIDECHAR_PAD;
						_decorations[_caretColumn+1] = _decorations[_caretColumn];
						_caretColumn += 2;
					}
					else
						_caretColumn++;
				}
			}
			else { //半角の上に書く
				_text[_caretColumn] = ch;
				_decorations[_caretColumn] = dec;
				if(GLine.CalcDisplayLength(ch)==2) {
					if(GLine.CalcDisplayLength(_text[_caretColumn+1])==2) //半角、全角となっているところに全角を書いたら
						if(_caretColumn+2<_text.Length) _text[_caretColumn+2] = ' ';
					_text[_caretColumn+1] = GLine.WIDECHAR_PAD;
					_decorations[_caretColumn+1] = _decorations[_caretColumn];
					_caretColumn += 2;
				}
				else
					_caretColumn++; //これが最もcommonなケースだが
			}
		}
		public void SetDecoration(TextDecoration dec) {
			if(_caretColumn<_decorations.Length)
				_decorations[_caretColumn] = dec;
		}
		
		public char CharAt(int index) {
			return _text[index];
		}

		public void BackCaret() {
			if(_caretColumn>0) { //最左端にあるときは何もしない
				_caretColumn--;
			}
		}

		public void RemoveAfterCaret() {
			for(int i=_caretColumn; i<_text.Length; i++) {
				_text[i] = '\0';
				_decorations[i] = null;
			}
		}
		public void FillSpace(int from, int to) {
			if(to>_text.Length) to = _text.Length;
			for(int i=from; i<to; i++) {
				_text[i] = ' ';
				_decorations[i] = null;
			}
		}
		public void FillSpace(int from, int to, TextDecoration dec) {
			if(to>_text.Length) to = _text.Length;
			for(int i=from; i<to; i++) {
				_text[i] = ' ';
				_decorations[i] = dec;
			}
		}
		//startからcount文字を消去して詰める。右端にはnullを入れる
		public void DeleteChars(int start, int count) {
			for(int i = start; i<_text.Length; i++) {
				int j = i + count;
				if(j < _text.Length) {
					_text[i] = _text[j];
					_decorations[i] = _decorations[j];
				}
				else {
					_text[i] = '\0';
					_decorations[i] = null;
				}
			}
		}
		public void InsertBlanks(int start, int count) {
			for(int i=_text.Length-1; i>=_caretColumn; i--) {
				int j = i - count;
				if(j >= _caretColumn) {
					_text[i] = _text[j];
					_decorations[i] = _decorations[j];
				}
				else {
					_text[i] = ' ';
					_decorations[i] = null;
				}
			}
		}

		public GLine Export() {
			GWord w = new GWord(_decorations[0]==null? TextDecoration.ClonedDefault() : _decorations[0], 0, GLine.CalcCharGroup(_text[0]));

			GLine line = new GLine(_text, w);
			line.EOLType = _eolType;
			int m = _text.Length;
			for(int offset=1; offset<m; offset++) {
				char ch = _text[offset];
				if(ch=='\0') break;
				else if(ch==GLine.WIDECHAR_PAD) continue;

				TextDecoration dec = _decorations[offset];
				if(_decorations[offset-1]!=dec || w.CharGroup!=GLine.CalcCharGroup(ch)) {
					if(dec==null) dec = TextDecoration.ClonedDefault(); //!!本当はここがnullになっているのはありえないはず。後で調査すること
					GWord ww = new GWord(dec, offset, GLine.CalcCharGroup(ch));
					w.Next = ww;
					w = ww;
				}
			}
			return line;
		}

		public override string ToString() {
			StringBuilder b = new StringBuilder();
			b.Append(_text);
			//アトリビュートまわりの表示はまだしていない
			return b.ToString();
		}
        /*
		public static void TestPutChar() {
			TestPutChar(1, "aaaaz ", 0, 'い');
			TestPutChar(2, "aあ\uFFFFaz ", 0, 'い');
			TestPutChar(3, "あ\uFFFFあ\uFFFFz ", 0, 'b');
			TestPutChar(4, "あ\uFFFFあ\uFFFFz ", 0, 'い');
			TestPutChar(5, "あ\uFFFFあ\uFFFFz ", 1, 'b');
			TestPutChar(6, "あ\uFFFFaaz ", 1, 'い');
			TestPutChar(7, "あ\uFFFFあ\uFFFFz ", 1, 'い');
		}*/
		private static void TestPutChar(int num, string initial, int col, char ch) {
			GLineManipulator m = new GLineManipulator(10);
			m._text = initial.ToCharArray();
			m.CaretColumn = col;
			Debug.WriteLine(String.Format("Test{0}  [{1}] col={2} char={3}", num, SafeString(m._text), m.CaretColumn, ch));
			m.PutChar(ch, TextDecoration.Default);
			Debug.WriteLine(String.Format("Result [{0}] col={1}", SafeString(m._text), m.CaretColumn));
		}
		public static string SafeString(char[] d) {
			StringBuilder bld = new StringBuilder();
			for(int i=0; i<d.Length; i++) {
				char ch = d[i];
				if(ch=='\0') break;
				if(ch!=GLine.WIDECHAR_PAD) bld.Append(ch);
			}
			return bld.ToString();
		}

	}
}
