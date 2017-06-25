/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: IntelliSenseWindow.cs,v 1.4 2011/12/10 12:14:42 kzmi Exp $
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Poderosa.View;

namespace Poderosa.Terminal {

    //インテリセンスポップアップウィンドウ
    internal class IntelliSenseWindow : ToolStripDropDown {
        public delegate void CancelDelegateT();

        public enum ComplementStatus {
            Selecting,
            Hidden,
            Exiting
        }
        private ComplementStatus _status;
        private IntelliSenseBox _listBox;
        private IntelliSenseContext _context;
        private CancelDelegateT _cancelDelegate;

        public IntelliSenseWindow() {
            _listBox = new IntelliSenseBox(this);
            ToolStripControlHost tch = new ToolStripControlHost(_listBox);
            this.Items.Add(tch);
            tch.Padding = Padding.Empty;
            this.Padding = Padding.Empty;
            this.ImeMode = ImeMode.Disable;
            _cancelDelegate = new CancelDelegateT(this.Cancel);
        }
        public IntelliSenseContext CurrentContext {
            get {
                return _context;
            }
        }
        public CancelDelegateT CancelDelegate {
            get {
                return _cancelDelegate;
            }
        }

        public void Popup(IntelliSenseContext ctx) {
            Debug.Assert(_context == null);
            _context = ctx;
            AdjustListBox();
            this.Show(_context.OwnerControl, ToControlPoint(ctx.CommandStartPoint));
            _listBox.Focus();
            _status = ComplementStatus.Selecting;
        }

        private void AdjustListBox() {
            this.SuspendLayout();
            _listBox.Size = new Size(0, 0); //こうしておくと余白が生じてしまうことはなくなる
            RenderProfile rp = _context.RenderProfile;
            SizeF pitch = rp.Pitch;
            _listBox.Font = rp.DefaultFont;
            _listBox.BackColor = rp.BackColor;
            _listBox.ForeColor = rp.ForeColor;
            _listBox.MaximumSize = new Size((int)(pitch.Width * 80), (int)(pitch.Height * 10)); //何文字分かはPreference化してもいい
            _listBox.Items.Clear();
            foreach (IntelliSenseItem item in _context.Candidates) {
                _listBox.Items.Add(item.Format(_context.CurrentScheme.DefaultDelimiter));
            }
            _listBox.SelectedIndexEx = _context.InitialSelectedIndex;
            this.ResumeLayout(true);
        }

        //ListBoxからのコールバック系
        public void Cancel() {
            if (_status == ComplementStatus.Exiting || _status == ComplementStatus.Hidden)
                return; //このExit内でClose()するとLostFocusなどを経由して再帰してくる場合がある
            _status = ComplementStatus.Hidden;
            Exit();
        }
        public void ManualCancel() {
            _context.Owner.SetCancelLockFlag();
            Cancel();
        }

        public void DoChar(char ch) {
            Debug.WriteLineIf(DebugOpt.IntelliSense, "DoChar " + (int)ch);
            if (_context.CurrentScheme.IsDelimiter(ch)) { //本当はここでクオーテーション等コマンドパース中のコンテキストによって単語終端を判定したい
                if (_listBox.SelectedIndexEx != -1) {
                    PartialComplement(ch);
                    return;
                }
            }

            //SendBack()の反応が来る前に補完を試みることに備えて
            _context.CharQueue.LockedPushChar(ch);
            SendBack(ch);

            if (ch == (char)0x08) { //BS
                if (_context.RemoveChar().Length == 0)
                    Cancel();
            }
            if (ch == '\n') { //Enter このときはTerminalControlのキーイベントを経由しないので
                _context.UpdateCommandList(_context.Owner.WholeCommand);
            }
            else if (0x20 <= (int)ch && (int)ch <= 0x7E) { //printableなやつ限定
                string current = _context.AppendChar(ch);
                int i = FindCandidateIndex(current);
                if (_context.CurrentScheme.IsDelimiter(ch) && i == -1)
                    Cancel();
                else
                    _listBox.SelectedIndexEx = i;
            }

        }
        public void Complement() {
            string text = _listBox.Items[_listBox.SelectedIndex].ToString();
            Debug.WriteLineIf(DebugOpt.IntelliSense, "Complement " + text);
            string sendback = FormatSendBackText(text, '\0');
            if (sendback.Length == 0) //既に既存候補を手入力してたときは、確認用Enterは不要。即コマンド確定でよい。
                SendBack('\n');
            else
                SendBack(sendback);
            _status = ComplementStatus.Hidden;
            Exit();
        }
        public void PartialComplement(char appending) {
            bool is_partial;
            string h = FindPartialComplementCandidate(_listBox.Items[_listBox.SelectedIndex].ToString(), out is_partial);
            _context.Complement(h);

            //候補の一部だけを選択した状態で、自動デリミタであればデフォルトデリミタ文字を追加
            if (is_partial && appending == '\0')
                appending = _context.CurrentScheme.DefaultDelimiter;
            string sendback = FormatSendBackText(h, appending);
            Debug.WriteLineIf(DebugOpt.IntelliSense, "PartialComplement " + sendback);
            SendBack(sendback);

            if (_context.IsEmpty)
                Cancel();
            else {
                Point pt = _context.CommandStartPoint;
                pt.X += h.Length + 1;
                if (pt.X < _context.Owner.Terminal.GetDocument().TerminalWidth) { //移動後も位置OKなら
                    _context.CommandStartPoint = pt;
                    PopupAgain();
                }
                else
                    Cancel(); //でなければあきらめる
            }
        }
        public void DoSpecialKey(Keys modifier, Keys key) {
            Debug.WriteLineIf(DebugOpt.IntelliSense, "DoSpecial " + key.ToString());
            if (modifier == Keys.Control && key == Keys.S) {
                _context.SortStyle = _context.SortStyle == IntelliSenseSort.Alphabet ? IntelliSenseSort.Historical : IntelliSenseSort.Alphabet;
                string t = _listBox.SelectedIndexEx == -1 ? null : _listBox.Items[_listBox.SelectedIndexEx].ToString();
                _context.BuildCandidates();
                PopupAgain();
                _listBox.SelectedIndexEx = t == null ? -1 : _listBox.FindStringExact(t);
            }
            else if (key == Keys.Delete) { //項目削除
                if (_listBox.SelectedIndex == -1)
                    return;

                string t = _listBox.Items[_listBox.SelectedIndex].ToString();
                string[] full_command = _context.ConcatToCurrentInput(_context.CurrentScheme.ParseCommandInput(t));
                IntelliSenseItemCollection col = (IntelliSenseItemCollection)_context.CurrentScheme.CommandHistory.GetAdapter(typeof(IntelliSenseItemCollection));
                col.RemoveItem(full_command);

                _listBox.Items.RemoveAt(_listBox.SelectedIndex);
            }
        }
        //ここまで

        private void Exit() {
            Debug.WriteLineIf(DebugOpt.IntelliSense, "Exit");
            Debug.Assert(_context != null);

            ComplementStatus s = _status;
            _status = ComplementStatus.Exiting;
            this.Hide();
            _context = null;
            _status = s;
        }

        private void PopupAgain() {
            IntelliSenseContext ctx = _context;
            Exit();
            Popup(ctx);
            _listBox.Focus();
        }

        private string FindPartialComplementCandidate(string text, out bool is_partial) {
            return _context.CurrentScheme.ParseFirstCommand(text, out is_partial);
        }
        private int FindCandidateIndex(string input) {
            int i = 0;
            foreach (object t in _listBox.Items) {
                string x = t.ToString();
                //前方一致で
                if (x.StartsWith(input))
                    return i;
                i++;
            }
            return -1;
        }

        private string FormatSendBackText(string value, char delim) {
            StringBuilder bld = new StringBuilder();
            //一致しているところまで検索し、必要に応じてバックスペースを入れる
            int corresponding_len = 0;
            int caret_column = _context.Owner.Terminal.GetDocument().CaretColumn;
            string current_text = _context.Owner.PromptLine.ToNormalString();
            while (caret_column > current_text.Length) {
                current_text += " ";
            }

            //送信と受信のはざまにいる奴をケア
            CheckCharQueue(ref current_text, ref caret_column);

            for (int x = _context.CommandStartPoint.X; x < caret_column; x++) {
                if (value.Length > corresponding_len && current_text[x] != value[corresponding_len])
                    break;
                corresponding_len++;
            }

            int backspace_count = caret_column - _context.CommandStartPoint.X - corresponding_len;
            Debug.Assert(backspace_count >= 0);
            if (backspace_count > 0)
                bld.Append(_context.CurrentScheme.BackSpaceChar, backspace_count);

            //一致部分を除いた本体
            if (value.Length > corresponding_len)
                bld.Append(value, corresponding_len, value.Length - corresponding_len);

            if (delim != '\0')
                bld.Append(delim);
            return bld.ToString();
        }

        private void CheckCharQueue(ref string text, ref int caret) {
            CharQueue cq = _context.CharQueue;
            lock (cq) {
                if (cq.IsEmpty)
                    return;

                StringBuilder edit = new StringBuilder(text);

                while (!cq.IsEmpty) {
                    char ch = cq.PopChar();
                    if (ch == (char)0x08) {
                        if (caret > 0) {
                            caret--;
                            if (caret < edit.Length) {
                                edit.Remove(caret, 1);
                            }
                        }
                    }
                    else if ((char)0x21 <= ch && ch <= (char)0x7E) { //TODO 別関数。IsPrintableとか
                        if (caret <= edit.Length) {
                            edit.Insert(caret, ch);
                            caret++;
                        }
                    }
                    Debug.WriteLineIf(DebugOpt.IntelliSense, "Queue Worked");
                }

                text = edit.ToString();
            }
        }

        //ターミナル側への送信
        private void SendBack(char ch) {
            _context.OwnerControl.SendChar(ch);
        }
        private void SendBack(string text) {
            _context.OwnerControl.SendCharArray(text.ToCharArray());
        }
        private Point ToControlPoint(Point textPoint) {
            SizeF pitch = _context.RenderProfile.Pitch;
            int x = (int)(textPoint.X * pitch.Width) - 2; //Y座標は現在行の上側に出すケースもあるが、ContextMenu.Show()で適当に何とかしてくれる
            int y = (int)((textPoint.Y + 1) * pitch.Height) + 1;
            Point pt = new Point(x, y);
            //下にはみ出たら
            if (_context.OwnerControl.PointToScreen(pt).Y + this.Height > Screen.PrimaryScreen.Bounds.Height) {
                pt.Y = pt.Y - (int)pitch.Height - this.Height;
            }
            return pt;
        }
    }

    //インテリセンス用リストボックス
    internal class IntelliSenseBox : ListBox {

        private IntelliSenseWindow _parent;
        private int _selectedIndexEx;

        public IntelliSenseBox(IntelliSenseWindow parent) {
            _parent = parent;
            this.IntegralHeight = false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            Keys key = keyData & Keys.KeyCode;
            if ((keyData & Keys.Modifiers) == Keys.Control) { //Ctrlは内部処理。親へは渡さない
                _parent.DoSpecialKey(Keys.Control, key);
                return true;
            }

            switch (key) {
                case Keys.Escape:
                    _parent.ManualCancel();
                    return base.ProcessCmdKey(ref msg, keyData);
                case Keys.Tab:
                    if (this.SelectedIndex == -1) //Tab, Enterは項目が選択されていなければ普通に送信
                        _parent.DoChar('\t');
                    else
                        _parent.PartialComplement('\0');
                    return true;
                case Keys.Enter:
                    if (this.SelectedIndex == -1) { //一致するものがない状態でのEnterはそれを送信してキャンセル
                        _parent.DoChar('\n');
                        _parent.Cancel();
                    }
                    else
                        _parent.Complement();
                    return true;
                case Keys.Left:
                case Keys.Right:
                case Keys.Delete:
                    _parent.DoSpecialKey(Keys.None, key); //左右は渡す必要なし
                    return true;
                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                    return base.ProcessCmdKey(ref msg, keyData); //リストボックスに処理させる
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        //スペースキー等で勝手にSelectedIndexが変わるのを防ぐ
        public int SelectedIndexEx {
            get {
                return _selectedIndexEx;
            }
            set {
                _selectedIndexEx = value;
                this.SelectedIndex = value;
            }
        }


        protected override void OnKeyPress(KeyPressEventArgs e) {
            _parent.DoChar(e.KeyChar);
            e.Handled = true;
            base.OnKeyPress(e);
        }

        protected override void OnDoubleClick(EventArgs e) {
            base.OnDoubleClick(e);
            if (this.SelectedIndex != -1)
                _parent.Complement();
        }

        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            _parent.Cancel();
        }


    }

}
