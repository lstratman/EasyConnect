/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalDocument.cs,v 1.6 2011/12/10 12:36:51 kzmi Exp $
 */
using System;
using System.Collections;
using System.Drawing;
using System.Diagnostics;

using Poderosa.Document;
using Poderosa.Commands;

namespace Poderosa.Terminal {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TerminalDocument : CharacterDocument {
        private int _caretColumn;
        private int _scrollingTop;
        private int _scrollingBottom;
        //ウィンドウの表示用テキスト
        private string _windowTitle; //ホストOSCシーケンスで指定されたタイトル
        private GLine _topLine;
        private GLine _currentLine;

        //画面に見えている幅と高さ
        private int _width;
        private int _height;

        internal TerminalDocument(int width, int height) {
            Resize(width, height);
            Clear();
            _scrollingTop = -1;
            _scrollingBottom = -1;
        }

        public string WindowTitle {
            get {
                return _windowTitle;
            }
            set {
                _windowTitle = value;
            }
        }
        public int TerminalHeight {
            get {
                return _height;
            }
        }
        public int TerminalWidth {
            get {
                return _width;
            }
        }

        public override IPoderosaMenuGroup[] ContextMenu {
            get {
                return TerminalEmulatorPlugin.Instance.DocumentContextMenu;
            }
        }

        public void SetScrollingRegion(int top_offset, int bottom_offset) {
            _scrollingTop = TopLineNumber + top_offset;
            _scrollingBottom = TopLineNumber + bottom_offset;
            //GLine l = FindLine(_scrollingTop);
        }
        public void Clear() {
            _caretColumn = 0;
            _firstLine = null;
            _lastLine = null;
            _size = 0;
            AddLine(new GLine(_width));
        }
        public void Resize(int width, int height) {
            _width = width;
            _height = height;
        }

        public void ClearScrollingRegion() {
            _scrollingTop = -1;
            _scrollingBottom = -1;
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

        public int TopLineNumber {
            get {
                return _topLine.ID;
            }
            set {
                if (_topLine.ID != value)
                    _invalidatedRegion.InvalidatedAll = true;
                _topLine = FindLineOrEdge(value); //同上の理由でOrEdgeバージョンに変更
            }
        }

        public void EnsureLine(int id) {
            while (id > _lastLine.ID) {
                AddLine(new GLine(_width));
            }
        }

        public int CurrentLineNumber {
            get {
                return _currentLine.ID;
            }
            set {
                if (value < _firstLine.ID)
                    value = _firstLine.ID; //リサイズ時の微妙なタイミングで負になってしまうことがあったようだ
                if (value > _lastLine.ID + 100)
                    value = _lastLine.ID + 100; //極端に大きな値を食らって死ぬことがないようにする

                while (value > _lastLine.ID) {
                    AddLine(new GLine(_width));
                }

                _currentLine = FindLineOrEdge(value); //外部から変な値が渡されたり、あるいはどこかにバグがあるせいでこの中でクラッシュすることがまれにあるようだ。なのでOrEdgeバージョンにしてクラッシュは回避
            }
        }

        public bool CurrentIsLast {
            get {
                return _currentLine.NextLine == null;
            }
        }
        #region part of IPoderosaDocument
        public override Image Icon {
            get {
                return _owner.Icon;
            }
        }
        public override string Caption {
            get {
                return _owner.Caption;
            }
        }
        #endregion

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
            if (_scrollingTop != -1 && _currentLine.ID >= _scrollingBottom) { //ロックされていて下まで行っている
                ScrollDown();
            }
            else {
                if (_height > 1) { //極端に高さがないときはこれで変な値になってしまうのでスキップ
                    if (_currentLine.ID >= _topLine.ID + _height - 1)
                        this.TopLineNumber = _currentLine.ID - _height + 2; //これで次のCurrentLineNumber++と合わせて行送りになる
                }
                this.CurrentLineNumber++; //これでプロパティセットがなされ、必要なら行の追加もされる。
            }

            //Debug.WriteLine(String.Format("c={0} t={1} f={2} l={3}", _currentLine.ID, _topLine.ID, _firstLine.ID, _lastLine.ID));
        }

        //スクロール範囲の最も下を１行消し、最も上に１行追加。現在行はその新規行になる。
        internal void ScrollUp() {
            if (_scrollingTop != -1 && _scrollingBottom != -1)
                ScrollUp(_scrollingTop, _scrollingBottom);
            else
                ScrollUp(TopLineNumber, TopLineNumber + _height - 1);
        }

        internal void ScrollUp(int from, int to) {
            GLine top = FindLineOrEdge(from);
            GLine bottom = FindLineOrEdge(to);
            if (top == null || bottom == null)
                return; //エラーハンドリングはFindLineの中で。ここではクラッシュ回避だけを行う
            int bottom_id = bottom.ID;
            int topline_id = _topLine.ID;
            GLine nextbottom = bottom.NextLine;

            if (from == to) {
                _currentLine = top;
                _currentLine.Clear();
            }
            else {
                Remove(bottom);
                _currentLine = new GLine(_width);

                InsertBefore(top, _currentLine);
                GLine c = _currentLine;
                do {
                    c.ID = from++;
                    c = c.NextLine;
                } while (c != nextbottom);
                Debug.Assert(nextbottom == null || nextbottom.ID == from);
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
            while (topline_id < _topLine.ID)
                _topLine = _topLine.PrevLine;


            _invalidatedRegion.InvalidatedAll = true;
        }

        //スクロール範囲の最も上を１行消し、最も下に１行追加。現在行はその新規行になる。
        internal void ScrollDown() {
            if (_scrollingTop != -1 && _scrollingBottom != -1)
                ScrollDown(_scrollingTop, _scrollingBottom);
            else
                ScrollDown(TopLineNumber, TopLineNumber + _height - 1);
        }

        internal void ScrollDown(int from, int to) {
            GLine top = FindLineOrEdge(from);
            GLine bottom = FindLineOrEdge(to);
            int top_id = top.ID;
            GLine newtop = top.NextLine;

            if (from == to) {
                _currentLine = top;
                _currentLine.Clear();
            }
            else {
                Remove(top); //_topLineの調整は必要ならここで行われる
                _currentLine = new GLine(_width);
                InsertAfter(bottom, _currentLine);

                //id maintainance
                GLine c = newtop;
                GLine end = _currentLine.NextLine;
                while (c != end) {
                    c.ID = top_id++;
                    c = c.NextLine;
                }
            }

            _invalidatedRegion.InvalidatedAll = true;
        }

        //整数インデクスから見つける　CurrentLineからそう遠くない位置だろうとあたりをつける
        public override GLine FindLine(int index) {
            //currentとtopの近い方から順にみていく
            int d1 = Math.Abs(index - _currentLine.ID);
            int d2 = Math.Abs(index - _topLine.ID);
            if (d1 < d2)
                return FindLineByHint(index, _currentLine);
            else
                return FindLineByHint(index, _topLine);
        }


        public void Replace(GLine target, GLine newline) {
            newline.NextLine = target.NextLine;
            newline.PrevLine = target.PrevLine;
            if (target.NextLine != null)
                target.NextLine.PrevLine = newline;
            if (target.PrevLine != null)
                target.PrevLine.NextLine = newline;

            if (target == _firstLine)
                _firstLine = newline;
            if (target == _lastLine)
                _lastLine = newline;
            if (target == _topLine)
                _topLine = newline;
            if (target == _currentLine)
                _currentLine = newline;

            newline.ID = target.ID;
            _invalidatedRegion.InvalidateLine(newline.ID);
        }

        //末尾に追加する
        public override void AddLine(GLine line) {
            base.AddLine(line);
            if (_size == 1) {
                _currentLine = line;
                _topLine = line;
            }
        }

        public void Remove(GLine line) {
            if (_size <= 1) {
                Clear();
                return;
            }

            if (line.PrevLine != null) {
                line.PrevLine.NextLine = line.NextLine;
            }
            if (line.NextLine != null) {
                line.NextLine.PrevLine = line.PrevLine;
            }

            if (line == _firstLine)
                _firstLine = line.NextLine;
            if (line == _lastLine)
                _lastLine = line.PrevLine;
            if (line == _topLine) {
                _topLine = line.NextLine;
            }
            if (line == _currentLine) {
                _currentLine = line.NextLine;
                if (_currentLine == null)
                    _currentLine = _lastLine;
            }

            _size--;
            _invalidatedRegion.InvalidatedAll = true;
        }

        /// 最後のremain行以前を削除する
        public int DiscardOldLines(int remain) {
            int delete_count = _size - remain;
            if (delete_count <= 0)
                return 0;

            GLine newfirst = _firstLine;
            for (int i = 0; i < delete_count; i++)
                newfirst = newfirst.NextLine;

            //新しい先頭を決める
            _firstLine = newfirst;
            newfirst.PrevLine.NextLine = null;
            newfirst.PrevLine = null;
            _size -= delete_count;
            Debug.Assert(_size == remain);


            if (_topLine.ID < _firstLine.ID)
                _topLine = _firstLine;
            if (_currentLine.ID < _firstLine.ID) {
                _currentLine = _firstLine;
                _caretColumn = 0;
            }

            return delete_count;
        }

        public void RemoveAfter(int from) {
            GLine delete = FindLineOrNullClipTop(from);
            if (delete == null)
                return;

            GLine remain = delete.PrevLine;
            delete.PrevLine = null;
            if (remain == null) {
                Clear();
            }
            else {
                remain.NextLine = null;
                _lastLine = remain;

                while (delete != null) {
                    _size--;
                    if (delete == _topLine)
                        _topLine = remain;
                    if (delete == _currentLine)
                        _currentLine = remain;
                    delete = delete.NextLine;
                }
            }

            _invalidatedRegion.InvalidatedAll = true;
        }

        public void ClearAfter(int from, TextDecoration dec) {
            GLine l = FindLineOrNullClipTop(from);
            if (l == null)
                return;

            while (l != null) {
                l.Clear(dec);
                l = l.NextLine;
            }

            _invalidatedRegion.InvalidatedAll = true;
        }

        public void ClearRange(int from, int to, TextDecoration dec) {
            GLine l = FindLineOrNullClipTop(from);
            if (l == null)
                return;

            while (l != null && l.ID < to) {
                l.Clear(dec);
                _invalidatedRegion.InvalidateLine(l.ID);
                l = l.NextLine;
            }
        }

        //再接続用に現在ドキュメントの前に挿入
        public void InsertBefore(TerminalDocument olddoc, int paneheight) {
            lock (this) {
                GLine c = olddoc.LastLine;
                int offset = _currentLine.ID - _topLine.ID;
                bool flag = false;
                while (c != null) {
                    if (flag || c.DisplayLength == 0) {
                        flag = true;
                        GLine nl = c.Clone();
                        nl.ID = _firstLine.ID - 1;
                        InsertBefore(_firstLine, nl); //最初に空でない行があれば以降は全部挿入
                        offset++;
                    }
                    c = c.PrevLine;
                }

                //IDが負になるのはちょっと怖いので修正
                if (_firstLine.ID < 0) {
                    int t = -_firstLine.ID;
                    c = _firstLine;
                    while (c != null) {
                        c.ID += t;
                        c = c.NextLine;
                    }
                }

                _topLine = FindLineOrEdge(_currentLine.ID - Math.Min(offset, paneheight));
                //Dump("insert doc");
            }
        }

        public void ReplaceCurrentLine(GLine line) {
#if DEBUG
            Replace(_currentLine, line);
#else
            if (_currentLine != null) //クラッシュレポートをみると、何かの拍子にnullになっていたとしか思えない
                Replace(_currentLine, line);
#endif
        }
    }

}
