/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TerminalDocument.cs,v 1.2 2005/04/20 08:45:47 okajima Exp $
*/
using System;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using Poderosa;
using Poderosa.Text;

using Poderosa.Communication;

namespace Poderosa.Text
{
	/// <summary>
	/// ターミナルとして動かしているときの
	/// </summary>
	public class TerminalDocument {
		private TerminalConnection _connection; //!!これは幅と高さ取得のためにのみ必要なパラメタなのでTerminalConnectionはやりすぎ

		private int _caretColumn;

		private int _scrollingTop;
		private int _scrollingBottom;

		//描画の必要のあるIDの範囲
		private int _invalidatedFrom;
		private int _invalidatedTo;
		private bool _invalidatedAll; //これが立っているときは無条件で全描画　正しいInvalidate範囲の計算が面倒なときはこれ

		private GLine _firstLine;
		private GLine _lastLine;
		private GLine _currentLine;
		private GLine _topLine;
		private int _size; //サイズは_firstLine/lastLineから計算可能だがよく使うのでキャッシュ

		internal TerminalDocument(TerminalConnection con) {
			_connection = con;
			Clear();
			_scrollingTop = -1;
			_scrollingBottom = -1;
		}

		public int InvalidatedFrom {
			get {
				return _invalidatedFrom;
			}
		}
		public int InvalidatedTo {
			get {
				return _invalidatedTo;
			}
		}
		public bool InvalidatedAll {
			get {
				return _invalidatedAll;
			}
		}
		public void ResetInvalidatedRegion() {
			_invalidatedAll = false;
			_invalidatedFrom = -1;
			_invalidatedTo = -1;
		}
		public void InvalidateLine(int id) {
			if(_invalidatedFrom==-1 || _invalidatedFrom > id) _invalidatedFrom = id;
			if(_invalidatedTo==-1   || _invalidatedTo   < id) _invalidatedTo   = id;
		}
		public void InvalidateAll() {
			_invalidatedAll = true;
		}

		internal void Clear() {
			_caretColumn = 0;
			_firstLine = null;
			_lastLine = null;
			_size = 0;
			AddLine(new GLine(_connection.TerminalWidth));
		}

		public int Size {
			get {
				return _size;
			}
		}

		//末尾に追加する
		internal void AddLine(GLine line) {
			if(_firstLine==null) { //空だった
				_firstLine = line;
				_lastLine = line;
				_currentLine = line;
				_topLine = line;
				_size = 1;
				line.ID = 0;
				InvalidateLine(0);
			}
			else { //通常の追加
				Debug.Assert(_lastLine.NextLine==null);
				int lastID = _lastLine.ID;
				_lastLine.NextLine = line;
				line.PrevLine = _lastLine;
				_lastLine = line;
				line.ID = lastID+1;
				_size++;
				InvalidateLine(lastID+1);
			}

		}

		//整数インデクスから見つける　CurrentLineからそう遠くない位置だろうとあたりをつける
		public GLine FindLine(int index) {
			//currentとtopの近い方から順にみていく
			int d1 = Math.Abs(index - _currentLine.ID);
			int d2 = Math.Abs(index - _topLine.ID);
			if(d1<d2)
				return FindLineByHint(index, _currentLine);
			else
				return FindLineByHint(index, _topLine);
		}

		public GLine FindLineOrNull(int index) {
			if(index < _firstLine.ID || index > _lastLine.ID) return null;
			else return FindLine(index);
		}
		public GLine FindLineOrEdge(int index) {
			if(index < _firstLine.ID) index = _firstLine.ID;
			else if(index > _lastLine.ID)  index = _lastLine.ID;
			
			return FindLine(index);
		}

		private GLine FindLineByHint(int index, GLine hintLine) {
			int h = hintLine.ID;
			GLine l = hintLine;
			if(index >= h) {
				for(int i=h; i<index; i++) {
					l = l.NextLine;
					if(l==null) FindLineByHintFailed(index, hintLine);
				}
			}
			else {
				for(int i=h; i>index; i--) {
					l = l.PrevLine;
					if(l==null) FindLineByHintFailed(index, hintLine);
				}
			}
			return l;
		}

		//FindLineByHintはしばしば失敗するのでデバッグ用に現在状態をダンプ
		private void FindLineByHintFailed(int index, GLine hintLine) {

#if DEBUG
			Dump(String.Format("FindLine {0}, hint_id={1}", index, hintLine.ID));
			Debugger.Break();
#endif
			GEnv.InterThreadUIService.InvalidDocumentOperation(this, GEnv.Strings.GetString("Message.TerminalDocument.UnexpectedCode"));
		}

		internal void SetScrollingRegion(int top_offset, int bottom_offset) {
			_scrollingTop = TopLineNumber+top_offset;
			_scrollingBottom = TopLineNumber+bottom_offset;
			//GLine l = FindLine(_scrollingTop);
		}
		internal void ClearScrollingRegion() {
			_scrollingTop = -1;
			_scrollingBottom = -1;
		}
		public void EnsureLine(int id) {
			while(id > _lastLine.ID) {
				AddLine(new GLine(_connection.TerminalWidth));
			}
		}

		public int CurrentLineNumber {
			get {
				return _currentLine.ID;
			}
			set {
				if(value < _firstLine.ID) value = _firstLine.ID; //リサイズ時の微妙なタイミングで負になってしまうことがあったようだ
				if(value > _lastLine.ID+100) value = _lastLine.ID+100; //極端に大きな値を食らって死ぬことがないようにする

				while(value > _lastLine.ID) {
					AddLine(new GLine(_connection.TerminalWidth));
				}

				_currentLine = FindLineOrEdge(value); //外部から変な値が渡されたり、あるいはどこかにバグがあるせいでこの中でクラッシュすることがまれにあるようだ。なのでOrEdgeバージョンにしてクラッシュは回避
				AssertValid();
			}
		}
		public int TopLineNumber {
			get {
				return _topLine.ID;
			}
			set {
				if(_topLine.ID!=value) _invalidatedAll = true;
				_topLine = FindLineOrEdge(value); //同上の理由でOrEdgeバージョンに変更
				AssertValid();
			}
		}
		public int FirstLineNumber {
			get {
				return _firstLine.ID;
			}
		}
		public int LastLineNumber {
			get {
				return _lastLine.ID;
			}
		}
		public int CaretColumn {
			get {
				return _caretColumn;
			}
			set {
				_caretColumn = value;
			}
		}

		public GLine CurrentLine {
			get {
				return _currentLine;
			}
		}
		public GLine TopLine {
			get {
				return _topLine;
			}
		}
		public GLine FirstLine {
			get {
				return _firstLine;
			}
		}
		public GLine LastLine {
			get {
				return _lastLine;
			}
		}
		public bool CurrentIsLast {
			get {
				return _currentLine.NextLine==null;
			}
		}

		public int ScrollingTop {
			get {
				return _scrollingTop;
			}
		}
		public int ScrollingBottom {
			get {
				return _scrollingBottom;
			}
		}

		internal void LineFeed() {
			if(_scrollingTop!=-1 && _currentLine.ID >= _scrollingBottom) { //ロックされていて下まで行っている
				ScrollDown(); 
			}
			else {
				if(_connection.TerminalHeight>1) { //極端に高さがないときはこれで変な値になってしまうのでスキップ
					if(_currentLine.ID >= _topLine.ID + _connection.TerminalHeight - 1)
						this.TopLineNumber = _currentLine.ID - _connection.TerminalHeight + 2; //これで次のCurrentLineNumber++と合わせて行送りになる
				}
				this.CurrentLineNumber++; //これでプロパティセットがなされ、必要なら行の追加もされる。
			}
			AssertValid();

			//Debug.WriteLine(String.Format("c={0} t={1} f={2} l={3}", _currentLine.ID, _topLine.ID, _firstLine.ID, _lastLine.ID));
		}

		//スクロール範囲の最も下を１行消し、最も上に１行追加。現在行はその新規行になる。
		internal void ScrollUp() {
			if(_scrollingTop!=-1 && _scrollingBottom!=-1)
				ScrollUp(_scrollingTop, _scrollingBottom);
			else
				ScrollUp(TopLineNumber, TopLineNumber + _connection.TerminalHeight - 1);
		}
		
		internal void ScrollUp(int from, int to) {
			GLine top = FindLineOrEdge(from);
			GLine bottom = FindLineOrEdge(to);
			if(top==null || bottom==null) return; //エラーハンドリングはFindLineの中で。ここではクラッシュ回避だけを行う
			int bottom_id = bottom.ID;
			int topline_id = _topLine.ID;
			GLine nextbottom = bottom.NextLine;

			if(from==to) {
				_currentLine = top;
				_currentLine.Clear();
			}
			else {
				Remove(bottom);
				_currentLine = new GLine(_connection.TerminalWidth);

				InsertBefore(top, _currentLine);
				GLine c = _currentLine;
				do {
					c.ID = from++;
					c = c.NextLine;
				} while(c!=nextbottom);
				Debug.Assert(nextbottom==null || nextbottom.ID==from);
			}
			/*
			//id maintainance
			GLine c = newbottom;
			GLine end = _currentLine.PrevLine;
			while(c != end) {
				c.ID = bottom_id--;
				c = c.PrevLine;
			}
			*/

			//!!次の２行はxtermをやっている間に発見して修正。 VT100では何かの必要があってこうなったはずなので後で調べること
			//if(_scrollingTop<=_topLine.ID && _topLine.ID<=_scrollingBottom)
			//	_topLine = _currentLine;
			while(topline_id<_topLine.ID)
				_topLine = _topLine.PrevLine;
			
			AssertValid();

			_invalidatedAll = true;
		}

		//スクロール範囲の最も上を１行消し、最も下に１行追加。現在行はその新規行になる。
		internal void ScrollDown() {
			if(_scrollingTop!=-1 && _scrollingBottom!=-1)
				ScrollDown(_scrollingTop, _scrollingBottom);
			else
				ScrollDown(TopLineNumber, TopLineNumber + _connection.TerminalHeight - 1);
		}

		internal void ScrollDown(int from, int to) {
			GLine top = FindLineOrEdge(from);
			GLine bottom = FindLineOrEdge(to);
			int top_id = top.ID;
			GLine newtop = top.NextLine;

			if(from==to) {
				_currentLine = top;
				_currentLine.Clear();
			}
			else {
				Remove(top); //_topLineの調整は必要ならここで行われる
				_currentLine = new GLine(_connection.TerminalWidth);
				InsertAfter(bottom, _currentLine);

				//id maintainance
				GLine c = newtop;
				GLine end = _currentLine.NextLine;
				while(c != end) {
					c.ID = top_id++;
					c = c.NextLine;
				}
			}
			AssertValid();

			_invalidatedAll = true;
		}

		internal void Replace(GLine target, GLine newline) {
			newline.NextLine = target.NextLine;
			newline.PrevLine = target.PrevLine;
			if(target.NextLine!=null) target.NextLine.PrevLine = newline;
			if(target.PrevLine!=null) target.PrevLine.NextLine = newline;

			if(target==_firstLine) _firstLine = newline;
			if(target==_lastLine)  _lastLine = newline;
			if(target==_topLine)  _topLine  = newline;
			if(target==_currentLine) _currentLine = newline;
			
			newline.ID = target.ID;
			InvalidateLine(newline.ID);
			AssertValid();
		}
		internal void ReplaceCurrentLine(GLine line) {
#if DEBUG
			Replace(_currentLine, line);
			AssertValid();
#else
			if(_currentLine!=null) //クラッシュレポートをみると、何かの拍子にnullになっていたとしか思えない
				Replace(_currentLine, line);
#endif
		}



		internal void Remove(GLine line) {
			if(_size<=1) {
				Clear();
				return;
			}

			if(line.PrevLine!=null) {
				line.PrevLine.NextLine = line.NextLine;
			}
			if(line.NextLine!=null) {
				line.NextLine.PrevLine = line.PrevLine;
			}

			if(line==_firstLine) _firstLine = line.NextLine;
			if(line==_lastLine)  _lastLine = line.PrevLine;
			if(line==_topLine)	{
				_topLine = line.NextLine;
			}
			if(line==_currentLine) {
				_currentLine = line.NextLine;
				if(_currentLine==null) _currentLine = _lastLine;
			}
			
			_size--;
			_invalidatedAll = true;
		}

		private void InsertBefore(GLine pos, GLine line) {
			if(pos.PrevLine!=null)
				pos.PrevLine.NextLine = line;

			line.PrevLine = pos.PrevLine;
			line.NextLine = pos;

			pos.PrevLine = line;

			if(pos==_firstLine)  _firstLine = line;
			_size++;
			_invalidatedAll = true;
		}
		private void InsertAfter(GLine pos, GLine line) {
			if(pos.NextLine!=null)
				pos.NextLine.PrevLine = line;

			line.NextLine = pos.NextLine;
			line.PrevLine = pos;

			pos.NextLine = line;

			if(pos==_lastLine)  _lastLine = line;
			_size++;
			_invalidatedAll = true;
		}
		
		internal void RemoveAfter(int from) {
			if(from > _lastLine.ID) return;
			GLine delete = FindLineOrEdge(from);
			if(delete==null) return;

			GLine remain = delete.PrevLine;
			delete.PrevLine = null;
			if(remain==null) {
				Clear();
			}
			else {
				remain.NextLine = null;
				_lastLine = remain;

				while(delete!=null) {
					_size--;
					if(delete==_topLine) _topLine = remain;
					if(delete==_currentLine) _currentLine = remain;
					delete = delete.NextLine;
				}
			}

			AssertValid();
			_invalidatedAll = true;
		}
		
		internal void ClearAfter(int from) {
			if(from > _lastLine.ID) return;
			GLine l = FindLineOrEdge(from);
			if(l==null) return;

			while(l!=null) {
				l.Clear();
				l = l.NextLine;
			}

			AssertValid();
			_invalidatedAll = true;
		}
		internal void ClearAfter(int from, TextDecoration dec) {
			if(from > _lastLine.ID) return;
			GLine l = FindLineOrEdge(from);
			if(l==null) return;

			while(l!=null) {
				l.Clear(dec);
				l = l.NextLine;
			}

			AssertValid();
			_invalidatedAll = true;
		}

		internal void ClearRange(int from, int to) {
			GLine l = FindLineOrEdge(from);
			if(l==null) return;

			while(l.ID < to) {
				l.Clear();
				InvalidateLine(l.ID);
				l = l.NextLine;
			}
			AssertValid();
		}
		internal void ClearRange(int from, int to, TextDecoration dec) {
			GLine l = FindLineOrEdge(from);
			if(l==null) return;

			while(l.ID < to) {
				l.Clear(dec);
				InvalidateLine(l.ID);
				l = l.NextLine;
			}
			AssertValid();
		}

		/// <summary>
		/// 最後のremain行以前を削除する
		/// </summary>
		internal int DiscardOldLines(int remain) {
			int delete_count = _size - remain;
			if(delete_count <= 0) return 0;

			GLine newfirst = _firstLine;
			for(int i=0; i<delete_count; i++)
				newfirst = newfirst.NextLine;

			//新しい先頭を決める
			_firstLine = newfirst;
			newfirst.PrevLine.NextLine = null;
			newfirst.PrevLine = null;
			_size -= delete_count;
			Debug.Assert(_size==remain);

			AssertValid();

			if(_topLine.ID<_firstLine.ID) _topLine=_firstLine;
			if(_currentLine.ID<_firstLine.ID) {
				_currentLine = _firstLine;
				_caretColumn = 0;
			}

			return delete_count;
		}

		//再接続用に現在ドキュメントの前に挿入
		public void InsertBefore(TerminalDocument olddoc, int paneheight) {
			lock(this) {
				GLine c = olddoc.LastLine;
				int offset = _currentLine.ID - _topLine.ID;
				bool flag = false;
				while(c!=null) {
					if(flag || c.Text[0]!='\0') {
						flag = true;
						GLine nl = c.Clone();
						nl.ID = _firstLine.ID-1;
						InsertBefore(_firstLine, nl); //最初に空でない行があれば以降は全部挿入
						offset++;
					}
					c = c.PrevLine;
				}

				//IDが負になるのはちょっと怖いので修正
				if(_firstLine.ID<0) {
					int t = -_firstLine.ID;
					c = _firstLine;
					while(c!=null) {
						c.ID += t;
						c = c.NextLine;
					}
				}

				_topLine = FindLineOrEdge(_currentLine.ID - Math.Min(offset, paneheight));
				//Dump("insert doc");
			}
		}


		public void Dump(string title) {
			Debug.WriteLine("<<<< DEBUG DUMP ["+title+"] >>>>");
			Debug.WriteLine(String.Format("[size={0} top={1} current={2} caret={3} first={4} last={5} region={6},{7}]", _size, TopLineNumber, CurrentLineNumber, _caretColumn, FirstLineNumber, LastLineNumber, _scrollingTop, _scrollingBottom));
			GLine gl = FindLineOrEdge(TopLineNumber);
			int count = 0;
			while(gl!=null && count++ < _connection.TerminalHeight) {
				Debug.Write(String.Format("{0,3}",gl.ID));
				Debug.Write(":");
				Debug.Write(GLineManipulator.SafeString(gl.Text));
				Debug.Write(":");
				Debug.WriteLine(gl.EOLType);
				gl = gl.NextLine;
			}
		}

		public virtual void AssertValid() {
#if false
			Debug.Assert(_currentLine.ID>=0);
			Debug.Assert(_currentLine.ID>=_topLine.ID);
			GLine l = _topLine;
			GLine n = l.NextLine;
			while(n!=null) {
				Debug.Assert(l.ID+1==n.ID);
				Debug.Assert(l==n.PrevLine);
				l = n;
				n = n.NextLine;
			}
#endif
		}

	}

}
