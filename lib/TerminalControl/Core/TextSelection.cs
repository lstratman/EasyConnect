/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TextSelection.cs,v 1.4 2011/12/10 09:59:38 kzmi Exp $
 */
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

using Poderosa.Sessions;
using Poderosa.Document;
using Poderosa.Forms;
using Poderosa.Commands;

namespace Poderosa.View {
    internal enum RangeType {
        Char,
        Word,
        Line
    }
    internal enum SelectionState {
        Empty,     //無選択
        Pivot,     //選択開始
        Expansion, //選択中
        Fixed      //選択領域確定
    }

    //CharacterDocumentの一部を選択するための機能
    internal class TextSelection : ITextSelection {

        //端点
        internal class TextPoint : ICloneable {
            private int _line;
            private int _column;

            public int Line {
                get {
                    return _line;
                }
                set {
                    _line = value;
                }
            }
            public int Column {
                get {
                    return _column;
                }
                set {
                    _column = value;
                }
            }

            public TextPoint() {
                Clear();
            }
            public TextPoint(int line, int column) {
                _line = line;
                _column = column;
            }


            public void Clear() {
                Line = -1;
                Column = 0;
            }

            public object Clone() {
                return MemberwiseClone();
            }
        }

        private SelectionState _state;

        private List<ISelectionListener> _listeners;

        private CharacterDocumentViewer _owner;
        //最初の選択点。単語や行を選択したときのために２つ(forward/backward)設ける。
        private TextPoint _forwardPivot;
        private TextPoint _backwardPivot;
        //選択の最終点
        private TextPoint _forwardDestination;
        private TextPoint _backwardDestination;

        //pivotの状態
        private RangeType _pivotType;

        //選択を開始したときのマウス座標
        private int _startX;
        private int _startY;

        //ちょっと汚いフラグ
        //private bool _disabledTemporary;

        public TextSelection(CharacterDocumentViewer owner) {
            _owner = owner;
            _forwardPivot = new TextPoint();
            _backwardPivot = new TextPoint();
            _forwardDestination = new TextPoint();
            _backwardDestination = new TextPoint();
            _listeners = new List<ISelectionListener>();
        }

        public SelectionState State {
            get {
                return _state;
            }
        }
        public RangeType PivotType {
            get {
                return _pivotType;
            }
        }

        //マウスを動かさなくてもクリックだけでMouseMoveイベントが発生してしまうので、位置のチェックのためにマウス座標記憶が必要
        public int StartX {
            get {
                return _startX;
            }
        }
        public int StartY {
            get {
                return _startY;
            }
        }


        public void Clear() {
            //if(_owner!=null)
            //	_owner.ExitTextSelection();
            _state = SelectionState.Empty;
            _forwardPivot.Clear();
            _backwardPivot.Clear();
            _forwardDestination.Clear();
            _backwardDestination.Clear();
            //_disabledTemporary = false;
        }

        /*
        public void DisableTemporary() {
            _disabledTemporary = true;
        }*/

        #region ISelection
        public IPoderosaView OwnerView {
            get {
                return (IPoderosaView)_owner.GetAdapter(typeof(IPoderosaView));
            }
        }
        public IPoderosaCommand TranslateCommand(IGeneralCommand command) {
            return null;
        }
        public IAdaptable GetAdapter(Type adapter) {
            return WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion

        //ドキュメントがDiscardされたときに呼ばれる。first_lineより前に選択領域が重なっていたらクリアする
        public void ClearIfOverlapped(int first_line) {
            if (_forwardPivot.Line != -1 && _forwardPivot.Line < first_line) {
                _forwardPivot.Line = first_line;
                _forwardPivot.Column = 0;
                _backwardPivot.Line = first_line;
                _backwardPivot.Column = 0;
            }

            if (_forwardDestination.Line != -1 && _forwardDestination.Line < first_line) {
                _forwardDestination.Line = first_line;
                _forwardDestination.Column = 0;
                _backwardDestination.Line = first_line;
                _backwardDestination.Column = 0;
            }
        }

        public bool IsEmpty {
            get {
                return _forwardPivot.Line == -1 || _backwardPivot.Line == -1 ||
                    _forwardDestination.Line == -1 || _backwardDestination.Line == -1;
            }
        }

        public bool StartSelection(GLine line, int position, RangeType type, int x, int y) {
            Debug.Assert(position >= 0);
            //日本語文字の右側からの選択は左側に修正
            line.ExpandBuffer(position + 1);
            if (line.IsRightSideOfZenkaku(position))
                position--;

            //_disabledTemporary = false;
            _pivotType = type;
            _forwardPivot.Line = line.ID;
            _backwardPivot.Line = line.ID;
            _forwardDestination.Line = line.ID;
            _forwardDestination.Column = position;
            _backwardDestination.Line = line.ID;
            _backwardDestination.Column = position;
            switch (type) {
                case RangeType.Char:
                    _forwardPivot.Column = position;
                    _backwardPivot.Column = position;
                    break;
                case RangeType.Word:
                    _forwardPivot.Column = line.FindPrevWordBreak(position) + 1;
                    _backwardPivot.Column = line.FindNextWordBreak(position);
                    break;
                case RangeType.Line:
                    _forwardPivot.Column = 0;
                    _backwardPivot.Column = line.DisplayLength;
                    break;
            }
            _state = SelectionState.Pivot;
            _startX = x;
            _startY = y;
            FireSelectionStarted();
            return true;
        }

        public bool ExpandTo(GLine line, int position, RangeType type) {
            line.ExpandBuffer(position + 1);
            //_disabledTemporary = false;
            _state = SelectionState.Expansion;

            _forwardDestination.Line = line.ID;
            _backwardDestination.Line = line.ID;
            //Debug.WriteLine(String.Format("ExpandTo Line{0} Position{1}", line.ID, position));
            switch (type) {
                case RangeType.Char:
                    _forwardDestination.Column = position;
                    _backwardDestination.Column = position;
                    break;
                case RangeType.Word:
                    _forwardDestination.Column = line.FindPrevWordBreak(position) + 1;
                    _backwardDestination.Column = line.FindNextWordBreak(position);
                    break;
                case RangeType.Line:
                    _forwardDestination.Column = 0;
                    _backwardDestination.Column = line.DisplayLength;
                    break;
            }

            return true;
        }

        public void SelectAll() {
            //_disabledTemporary = false;
            _forwardPivot.Line = _owner.CharacterDocument.FirstLine.ID;
            _forwardPivot.Column = 0;
            _backwardPivot = (TextPoint)_forwardPivot.Clone();
            _forwardDestination.Line = _owner.CharacterDocument.LastLine.ID;
            _forwardDestination.Column = _owner.CharacterDocument.LastLine.DisplayLength;
            _backwardDestination = (TextPoint)_forwardDestination.Clone();

            _pivotType = RangeType.Char;
            FixSelection();
        }

        //選択モードに応じて範囲を定める。マウスでドラッグすることもあるので、column<0のケースも存在する
        public TextPoint ConvertSelectionPosition(GLine line, int column) {
            TextPoint result = new TextPoint(line.ID, column);

            int line_length = line.DisplayLength;
            if (_pivotType == RangeType.Line) {
                //行選択のときは、選択開始点以前のであったらその行の先頭、そうでないならその行のラスト。
                //言い換えると(Pivot-Destination)を行頭・行末方向に拡大したものになるように
                if (result.Line <= _forwardPivot.Line)
                    result.Column = 0;
                else
                    result.Column = line.DisplayLength;
            }
            else { //Word,Char選択
                if (result.Line < _forwardPivot.Line) { //開始点より前のときは
                    if (result.Column < 0)
                        result.Column = 0; //行頭まで。
                    else if (result.Column >= line_length) { //行の右端の右まで選択しているときは、次行の先頭まで
                        result.Line++;
                        result.Column = 0;
                    }
                }
                else if (result.Line == _forwardPivot.Line) { //同一行内選択.その行におさまるように
                    result.Column = RuntimeUtil.AdjustIntRange(result.Column, 0, line_length);
                }
                else { //開始点の後方への選択
                    if (result.Column < 0) {
                        result.Line--;
                        result.Column = line.PrevLine == null ? 0 : line.PrevLine.DisplayLength;
                    }
                    else if (result.Column >= line_length)
                        result.Column = line_length;
                }
            }

            return result;
        }

        public void FixSelection() {
            _state = SelectionState.Fixed;
            FireSelectionFixed();
        }

        public string GetSelectedText(TextFormatOption opt) {
            //if(_owner==null || _disabledTemporary) return null;

            StringBuilder bld = new StringBuilder();
            TextPoint a = HeadPoint;
            TextPoint b = TailPoint;

            GLine l = _owner.CharacterDocument.FindLineOrEdge(a.Line);
            int pos = a.Column;
            if (pos < 0)
                return "";

            do {
                bool eol_required = (opt == TextFormatOption.AsLook || l.EOLType != EOLType.Continue);
                if (l.ID == b.Line) { //最終行
                    //末尾にNULL文字が入るケースがあるようだ
                    AppendTrim(bld, l, pos, b.Column - pos);
                    if (_pivotType == RangeType.Line && eol_required)
                        bld.Append("\r\n");
                    break;
                }
                else { //最終以外の行
                    if (l.Length - pos > 0) { //l.CharLength==posとなるケースがあった。真の理由は納得していないが
                        AppendTrim(bld, l, pos, l.Length - pos);
                    }
                    if (eol_required && bld.Length > 0) //bld.Length>0は行単位選択で余計な改行が入るのを避けるための処置
                        bld.Append("\r\n"); //LFのみをクリップボードに持っていっても他のアプリの混乱があるだけなのでやめておく
                    l = l.NextLine;
                    if (l == null)
                        break; //!!本来これはないはずだがクラッシュレポートのため回避
                    pos = 0;
                }
            } while (true);

            //Debug.WriteLine("Selected Text Len="+bld.Length);

            return bld.ToString();
        }
        private void AppendTrim(StringBuilder bld, GLine line, int pos, int length) {
            Debug.Assert(pos >= 0);
            if (line.IsRightSideOfZenkaku(pos)) { //日本語文字の右端からのときは拡大する
                pos--;
                length++;
            }

            line.WriteTo(
                delegate(char[] buff, int len) {
                    bld.Append(buff, 0, len);
                },
                pos, length);
        }

        internal TextPoint HeadPoint {
            get {
                return Min(_forwardPivot, _forwardDestination);
            }
        }
        internal TextPoint TailPoint {
            get {
                return Max(_backwardPivot, _backwardDestination);
            }
        }
        private static TextPoint Min(TextPoint p1, TextPoint p2) {
            int id1 = p1.Line;
            int id2 = p2.Line;
            if (id1 == id2) {
                int pos1 = p1.Column;
                int pos2 = p2.Column;
                if (pos1 == pos2)
                    return p1;
                else
                    return pos1 < pos2 ? p1 : p2;
            }
            else
                return id1 < id2 ? p1 : p2;

        }
        private static TextPoint Max(TextPoint p1, TextPoint p2) {
            int id1 = p1.Line;
            int id2 = p2.Line;
            if (id1 == id2) {
                int pos1 = p1.Column;
                int pos2 = p2.Column;
                if (pos1 == pos2)
                    return p1;
                else
                    return pos1 > pos2 ? p1 : p2;
            }
            else
                return id1 > id2 ? p1 : p2;

        }

        //Listener系
        public void AddSelectionListener(ISelectionListener listener) {
            _listeners.Add(listener);
        }
        public void RemoveSelectionListener(ISelectionListener listener) {
            _listeners.Remove(listener);
        }

        void FireSelectionStarted() {
            foreach (ISelectionListener listener in _listeners)
                listener.OnSelectionStarted();
        }
        void FireSelectionFixed() {
            foreach (ISelectionListener listener in _listeners)
                listener.OnSelectionFixed();
        }


    }
}
