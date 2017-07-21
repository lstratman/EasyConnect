/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TabBar.cs,v 1.7 2011/10/27 23:21:59 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using Poderosa.Forms;
using Poderosa.Util.Drawing;
using Poderosa.Util;

namespace Poderosa.Forms {

    //各タブ要素に関連付けるオブジェクト。Poderosaの場合はIPoderosaDocumentになる。
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public abstract class TabKey {
        protected object _element;

        public TabKey(object element) {
            Debug.Assert(element != null);
            _element = element;
        }
        public object Element {
            get {
                return _element;
            }
        }
        public abstract string Caption {
            get;
        }
        public abstract Image Icon {
            get;
        } //must be 16*16


        //中の要素で比較する
        public static bool operator ==(TabKey key1, TabKey key2) {
            return _Equals(key1, key2);
        }
        public static bool operator !=(TabKey key1, TabKey key2) {
            return !_Equals(key1, key2);
        }
        public override int GetHashCode() {
            return _element.GetHashCode();
        }
        public override bool Equals(object obj) {
            return _element.Equals(((TabKey)obj)._element);
        }

        private static bool _Equals(TabKey key1, TabKey key2) {
            if (Object.ReferenceEquals(key1, null))
                return Object.ReferenceEquals(key2, null);
            else
                return !Object.ReferenceEquals(key2, null) && key1._element == key2._element;
        }
    }

    internal enum DrawState {
        Normal,
        Hover,
        Selected
    }

    //タブの描画に必要な色・フォントを収録
    internal class TabBarDrawing {
        private Color _backgroundColor;
        private DrawUtil.RoundRectColors _roundRectColors;
        private Font _font;

        public Color BackgroundColor {
            get {
                return _backgroundColor;
            }
        }
        public DrawUtil.RoundRectColors RoundRectColors {
            get {
                return _roundRectColors;
            }
        }
        public Font Font {
            get {
                return _font;
            }
        }

        public static TabBarDrawing CreateNormalStyle(Graphics g) {
            TabBarDrawing t = new TabBarDrawing();
            t._font = new Font(SystemFonts.DefaultFont.Name, 9f);

            Color c = SystemColors.Control;
            t._backgroundColor = c;

            t._roundRectColors = new DrawUtil.RoundRectColors();
            t._roundRectColors.border_color = DrawUtil.ToCOLORREF(DrawUtil.DarkColor(Color.Orange));
            t._roundRectColors.inner_color = DrawUtil.ToCOLORREF(SystemColors.Control);
            t._roundRectColors.outer_color = DrawUtil.ToCOLORREF(SystemColors.Control);
            t._roundRectColors.lightlight_color = DrawUtil.MergeColor(t._roundRectColors.border_color, t._roundRectColors.outer_color);
            t._roundRectColors.light_color = DrawUtil.MergeColor(t._roundRectColors.lightlight_color, t._roundRectColors.border_color);

            return t;
        }
        public static TabBarDrawing CreateActiveStyle(TabBarDrawing normal, Graphics g) {
            TabBarDrawing t = new TabBarDrawing();
            t._font = new Font(normal.Font, normal.Font.Style | FontStyle.Bold);

            Color c = SystemColors.ControlLightLight;
            t._backgroundColor = c;

            t._roundRectColors = new DrawUtil.RoundRectColors();
            t._roundRectColors.border_color = DrawUtil.ToCOLORREF(SystemColors.ControlDarkDark);
            t._roundRectColors.inner_color = DrawUtil.ToCOLORREF(c);
            t._roundRectColors.outer_color = DrawUtil.ToCOLORREF(SystemColors.Control);
            t._roundRectColors.lightlight_color = DrawUtil.MergeColor(t._roundRectColors.border_color, t._roundRectColors.outer_color);
            t._roundRectColors.light_color = DrawUtil.MergeColor(t._roundRectColors.lightlight_color, t._roundRectColors.border_color);

            return t;
        }

        public void Dispose() {
            _font.Dispose();
        }
    }

    //TabBar本体
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TabBar : UserControl {

        private static IComparer<TabBarButton> _widthComparer; //幅順の並び替え
        private static DragAndDropSupport _dragAndDrop; //本当はstaticではなく別途もらってくる？
        private static TabBarDrawing _normalDrawing; //描画についての情報
        private static TabBarDrawing _activeDrawing;

        private TabBarTable _parentTable; //コンテナ
        private List<TabBarButton> _buttons;       //表示されている順のボタン
        private List<TabBarButton> _sortedButtons; //幅順にソートしたものを一時的に保存する
        private ToolTip _tabToolTip;      //ボタンのToolTip

        //ただクリックしただけでMouseMoveが発生してしまうので、正しくドラッグ開始を判定できない
        private int _dragStartPosX;
        private int _dragStartPosY;

        private AdjustEachWidthResult _lastAdjustmentResult;

        //レイアウト用定数
        private const int UNITHEIGHT = 21;
        private const int MINIMUM_BUTTON_WIDTH = 24;
        private const int BUTTON_MARGIN = 4;
        private const int EXTRA_FOR_ACTIVE = 16;
        private const int BUTTON_Y = 3;

        //初期化
        static TabBar() {
            _widthComparer = new TabBarButton.WidthComparer();
            _dragAndDrop = new DragAndDropSupport();
        }

        public TabBar(TabBarTable parent) {
            _parentTable = parent;
            _buttons = new List<TabBarButton>();
            _tabToolTip = new ToolTip();
            //例のAllowDrop問題で、NUnitのスレッドは[STAThread]してないらしいのでこれを回避
#if !UNITTEST
            this.AllowDrop = true;
#endif
            this.Height = UNITHEIGHT;
        }
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            if (_normalDrawing == null) {
                Graphics g = this.CreateGraphics();
                _normalDrawing = TabBarDrawing.CreateNormalStyle(g);
                _activeDrawing = TabBarDrawing.CreateActiveStyle(_normalDrawing, g);
                g.Dispose();
            }
        }

        //プロパティ
        internal TabBarDrawing NormalDrawing {
            get {
                return _normalDrawing;
            }
        }
        internal TabBarDrawing ActiveDrawing {
            get {
                return _activeDrawing;
            }
        }
        public int TabCount {
            get {
                return _buttons.Count;
            }
        }
        public TabBarTable ParentTable {
            get {
                return _parentTable;
            }
        }
        internal TabBarButton ButtonAt(int index) {
            return _buttons[index];
        }
        internal IEnumerable<TabBarButton> Buttons {
            get {
                return _buttons;
            }
        }
        internal AdjustEachWidthResult LastAdjustmentResult {
            get {
                return _lastAdjustmentResult;
            }
        }
        internal TabBarButton FindButton(TabKey key) {
            foreach (TabBarButton button in _buttons)
                if (button.TabKey == key)
                    return button;
            return null;
        }

        private int GetTabAreaWidth() {
            return this.Width - 4;
        }


        public void AddTab(TabBarUpdateState state, TabKey key, int index) {
            TabBarButton b = CreateButton(key, index);
            _buttons.Add(b);
            this.Controls.Add(b);
            state.MarkSufficientWidthIsChanged(this);
        }

        public void RemoveTab(TabBarUpdateState state, TabKey key) {
            TabBarButton b = FindButton(key);
            Debug.Assert(b != null);

            Controls.Remove(b);
            _buttons.Remove(b);
            state.MarkIndexAssignmentChanged(this);
        }

        private TabBarButton CreateButton(TabKey key, int index) {
            TabBarButton b = new TabBarButton(this, key, index);
            //TODO _tabToolTip.SetToolTip(b, document.Description);

            b.Visible = true;
            b.Top = 1;
            b.Height = UNITHEIGHT - 2;
            b.TabStop = false;
            b.Click += new EventHandler(RootActivator);
            b.DoubleClick += new EventHandler(OnDoubleClick);
            b.MouseDown += new MouseEventHandler(OnMouseDown);
            b.MouseUp += new MouseEventHandler(OnMouseUp);
            b.MouseMove += new MouseEventHandler(OnMouseMove);
            b.KeyDown += new KeyEventHandler(OnButtonKeyDown);
            return b;
        }


        private void OnMouseUp(object sender, MouseEventArgs args) {
            if (args.Button == MouseButtons.Right) {
                TabKey key = ((TabBarButton)sender).TabKey;
                _parentTable.DoRightButtonAction(key);
            }
            else if (args.Button == MouseButtons.Middle) {
                TabKey key = ((TabBarButton)sender).TabKey;
                _parentTable.DoMiddleButtonAction(key);
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs args) {
            if (args.Button != MouseButtons.Left)
                return;
            _dragStartPosX = args.X;
            _dragStartPosY = args.Y;
        }
        private void OnMouseMove(object sender, MouseEventArgs args) {
            if (args.Button != MouseButtons.Left)
                return;

            if (Math.Abs(_dragStartPosX - args.X) + Math.Abs(_dragStartPosY - args.Y) >= 3) {
                TabBarButton btn = sender as TabBarButton;
                Debug.Assert(btn != null);
                btn.ClearMouseTrackingFlags();
                _dragAndDrop.StartDrag(btn);
            }
        }
        private void RootActivator(object sender, EventArgs args) {
            try {
                TabKey key = ((TabBarButton)sender).TabKey;
                //既にアクティブなやつでも、対応するビューにフォーカスをもっていくなどの効果が必要なこともある。

                using (TabBarUpdateState state = new TabBarUpdateState("ui-activate")) {
                    _parentTable.Activate(key, true);
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void OnDoubleClick(object sender, EventArgs args) {
            try {
                TabBarButton btn = sender as TabBarButton;
                Debug.Assert(btn != null);
                _parentTable.CaptureState.StartCapture(this, btn.TabKey);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            try {
                if (_parentTable.CaptureState.IsCapturing)
                    _parentTable.CaptureState.EndCapture(this.PointToScreen(new Point(e.X, e.Y)));
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
        protected override void OnMouseCaptureChanged(EventArgs e) {
            base.OnMouseCaptureChanged(e);
            if (_parentTable.CaptureState.IsCapturing)
                _parentTable.CaptureState.CancelCapture();
        }
        private void OnButtonKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
                RootActivator(sender, e); //Enterでアクティベート。クリックと同様の効果
        }

        //各ボタンのSufficientWidthを計算
        internal void CalcSuffientWidths() {
            Graphics g = this.CreateGraphics();
            foreach (TabBarButton b in _buttons) {
                int t = b.CalcSufficientWidth(g);
            }
            g.Dispose();

            //縮小する必要があるときはまずソート　
            _sortedButtons = new List<TabBarButton>(_buttons);
            _sortedButtons.Sort(_widthComparer);
        }

        //ボタンの再配置
        internal void ArrangeButtons() {
            int total = 0;
            if (_buttons.Count == 0)
                return;

            Graphics g = this.CreateGraphics();
            foreach (TabBarButton b in _buttons) {
                int t = b.SufficientWidth; //これは必要なものが計算済みであるという前提
                b.TemporaryWidth = t;
                total += t;
            }

            int x = BUTTON_MARGIN;
            int remaining = this.ClientSize.Width - x * 2/*両端マージン*/ - (_buttons.Count - 1) * BUTTON_MARGIN;
            AdjustEachWidthResult adjust_result = AdjustEachWidth(total, remaining);
            _lastAdjustmentResult = adjust_result;

            foreach (TabBarButton button in _buttons) {
                button.AdjustTextAndWidth(g, button.TemporaryWidth);
                button.Left = x;
                x += button.Width + BUTTON_MARGIN;
                button.Top = BUTTON_Y;
                button.Visible = true;
            }

            g.Dispose();

            this.Invalidate(true);
        }

        //index振りなおし
        internal void AllocateIndex(int index) {
            foreach (TabBarButton button in _buttons)
                button.Index = index++;
            Invalidate(true);
        }

        internal enum AdjustEachWidthResult {
            Sufficient,
            Adjusted,
            Overflow
        }

        //バー全体でボタンの合計でsufficient_width必要、actual_widthが実際のサイズであるときうまく丸め込む
        private AdjustEachWidthResult AdjustEachWidth(int sufficient_width, int actual_width) {
            int to_be_shrinked = sufficient_width - actual_width;
            if (to_be_shrinked <= 0)
                return AdjustEachWidthResult.Sufficient; //何もしなくて十分サイズあり

            int order = 0;
            while (to_be_shrinked > 0) {
                //必要幅の多い第 order 位に注目
                TabBarButton b = _sortedButtons[order];
                //次順位との差、もしくは限界値までの差がこの段階で縮小できるリミット
                int max_shrink = order == _sortedButtons.Count - 1 ?
                    b.TemporaryWidth - MINIMUM_BUTTON_WIDTH :
                    b.TemporaryWidth - _sortedButtons[order + 1].TemporaryWidth;
                Debug.Assert(max_shrink >= 0);

                if ((order + 1) * max_shrink < to_be_shrinked) { //この順位まで全部縮小しても間に合わないときは
                    for (int i = 0; i <= order; i++)
                        _sortedButtons[i].TemporaryWidth -= max_shrink;
                }
                else { //このorderまでで縮小可能
                    int shrink = to_be_shrinked / (order + 1);
                    int mod = to_be_shrinked % (order + 1); //最初のmod個はさらに1shrinkしてぴったり均衡をとる
                    Debug.Assert(mod * (shrink + 1) + (order + 1 - mod) * shrink == to_be_shrinked);
                    for (int i = 0; i <= order; i++)
                        _sortedButtons[i].TemporaryWidth -= i < mod ? shrink + 1 : shrink;
                }

                to_be_shrinked -= (order + 1) * max_shrink;
                order++;

                if (order == _sortedButtons.Count && to_be_shrinked > 0)
                    return AdjustEachWidthResult.Overflow; //最後まで縮小してダメならあきらめる
            }
            //ここまできて、各_tempWidthは調整後の値が入っていることになる
            return AdjustEachWidthResult.Adjusted;
        }

        //終了時の実行 static constructorがデフォルトコンストラクタと同じ引数構成なので使えない。ありえないよ。
        public static void Init() {
            Application.ApplicationExit += new EventHandler(OnAppExit);
        }

        private static void OnAppExit(object sender, EventArgs args) {
            if (_normalDrawing != null)
                _normalDrawing.Dispose();
            if (_activeDrawing != null)
                _activeDrawing.Dispose();
        }

        protected override void OnPaint(PaintEventArgs arg) {
            using (TabBarUpdateState state = new TabBarUpdateState("repaint")) {
                base.OnPaint(arg);
                //上に区切り線を引く
                Graphics g = arg.Graphics;
                Pen p = SystemPens.WindowFrame;
                g.DrawLine(p, 0, 0, Width, 0);
                p = SystemPens.ControlLight;
                g.DrawLine(p, 0, 1, Width, 1);

                //DropPoint Effect
                if (_dragAndDrop.OwnsDropPoint(this)) {
                    DrawDropPointEffect(g, _dragAndDrop.CurrentDropPoint.PosX(2) - 1, BUTTON_Y);
                }
            }
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            this.ArrangeButtons();
        }

        //Drag & Drop関係
        protected override void OnDragEnter(DragEventArgs drgevent) {
            base.OnDragEnter(drgevent);
            _parentTable.ByPassDragEnter(drgevent);
        }
        protected override void OnDragOver(DragEventArgs drgevent) {
            base.OnDragOver(drgevent);

            DragAndDropSupport.DropPoint point = null;
            if (_dragAndDrop.CanDrop(drgevent.Data, this)) {
                const int DROP_CAPACITY_WIDTH = 6; //ボタンの境界からどの位置までをドロップ可能とするか
                if (_buttons.Count == 0) { //ボタンがないときは特例で
                    point = new DragAndDropSupport.DropPoint(this, null, true);
                }
                else {
                    Point pt = this.PointToClient(new Point(drgevent.X, drgevent.Y));
                    for (int i = 0; i < _buttons.Count; i++) {
                        TabBarButton btn = _buttons[i];
                        if (Math.Abs(btn.Top + btn.Height / 2 - pt.Y) < btn.Height / 2) {
                            if (Math.Abs(btn.Left - BUTTON_MARGIN / 2 - pt.X) < DROP_CAPACITY_WIDTH) {
                                point = new DragAndDropSupport.DropPoint(this, btn, true);
                                break;
                            }
                            else if (i == _buttons.Count - 1 && btn.Right + BUTTON_MARGIN / 2 < pt.X) { //右端設定ができるのは最後のコントロールだけ
                                point = new DragAndDropSupport.DropPoint(this, btn, false);
                                break;
                            }
                        }
                    }
                }

                if (point != null)
                    drgevent.Effect = DragDropEffects.Link;
                else
                    drgevent.Effect = DragDropEffects.Move;
            }

            _dragAndDrop.SetDropPoint(point);
        }

        protected override void OnDragDrop(DragEventArgs drgevent) {
            base.OnDragDrop(drgevent);
            if (!_dragAndDrop.OwnsDropPoint(this)) {
                _parentTable.ByPassDragDrop(drgevent);
                return; //Drop可能になっていなければダメ
            }

            try {
                DragAndDropSupport.DropResult r;
                using (TabBarUpdateState state = new TabBarUpdateState("drop")) {
                    r = _dragAndDrop.Drop(state);
                }
                if (r == DragAndDropSupport.DropResult.Ignored) {
                    Invalidate();
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent) {
            base.OnQueryContinueDrag(qcdevent);
            //Debug.WriteLine("QCD " + qcdevent.Action.ToString() + MousePosition.ToString());
            if (qcdevent.EscapePressed) {
                _dragAndDrop.ClearDropPoint();
            }
        }

        //indexの振りなおし：add/remove系などで実行する
        internal void AssignmentIndex() {
            for (int i = 0; i < _buttons.Count; i++) {
                TabBarButton b = _buttons[i];
                b.Index = i;
            }
        }

        //Drag&Dropの結果またはリバランスの結果に基づいてアサインメントをやり直す
        internal void AssignDocuments(TabBarUpdateState state, TabKey[] keys, int offset, int length) {
            while (_buttons.Count > length) { //余計なものがあれば削除
                TabBarButton t = _buttons[_buttons.Count - 1];
                this.Controls.Remove(t);
                _buttons.RemoveAt(_buttons.Count - 1);
            }

            //既存分を追加
            for (int i = 0; i < _buttons.Count; i++) {
                TabBarButton b = _buttons[i];
                b.Init(this, keys[offset + i], offset + i);
            }

            for (int i = _buttons.Count; i < length; i++) { //不足分があれば追加
                TabBarButton b = CreateButton(keys[offset + i], offset + i);
                _buttons.Add(b);
                this.Controls.Add(b);
            }

            state.MarkSufficientWidthIsChanged(this);

        }
        internal void AssignDocuments(TabBarUpdateState state, TabKey[] docs) {
            AssignDocuments(state, docs, 0, docs.Length);
        }

        private void DrawDropPointEffect(Graphics g, int x, int y) {
            //横線
            int height = UNITHEIGHT - 4;
            g.DrawLine(SystemPens.ControlText, x - 3, y, x + 4, y);
            g.DrawLine(SystemPens.ControlText, x - 3, y + height, x + 4, y + height);

            g.DrawLine(SystemPens.ControlText, x, y, x, y + height);
            g.DrawLine(SystemPens.ControlDark, x + 1, y, x + 1, y + height);
        }

    }


    internal class TabBarButton : UserControl {
        //幅の大きい順に並べる
        public class WidthComparer : IComparer<TabBarButton> {
            public int Compare(TabBarButton x, TabBarButton y) {
                return y._sufficientWidth - x._sufficientWidth;
            }
        }

        private const int TEXT_MARGIN = 2;

        private static readonly StringFormat _captionFormat;

        //Container
        private TabBar _parent;

        private TabKey _tabKey;
        private int _index;
        private string _indexText;
        private Image _image;
        private bool _selected;
#pragma warning disable 414
        private bool _mouseDown;
#pragma warning restore 414
        private bool _mouseEnter;

        private int _sufficientWidth; //ボタンの全テキストを表示するのに十分な幅

        //幅計算時にセットされるもの
        private bool _showIndexText;

        private int _temporaryWidth;

        static TabBarButton() {
            _captionFormat = new StringFormat(StringFormatFlags.NoWrap);
            _captionFormat.Alignment = StringAlignment.Near;
            _captionFormat.LineAlignment = StringAlignment.Center;
            _captionFormat.Trimming = StringTrimming.EllipsisCharacter;
        }

        public TabBarButton(TabBar parent, TabKey key, int index) {
            Init(parent, key, index);
        }
        public void Init(TabBar parent, TabKey key, int index) {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            //再作成することはある
            _parent = parent;
            _tabKey = key;
            _index = index;
            _image = key.Icon;

            _indexText = (index + 1).ToString();
        }


        public TabBar ParentTabBar {
            get {
                return _parent;
            }
        }
        public TabKey TabKey {
            get {
                return _tabKey;
            }
        }
        public int Index {
            get {
                return _index;
            }
            set {
                _index = value;
                _indexText = (_index + 1).ToString();
            }
        }
        public int SufficientWidth {
            get {
                return _sufficientWidth;
            }
        }
        public bool Selected {
            get {
                return _selected;
            }
        }
        public Image Image {
            get {
                return _image;
            }
        }
        public int TemporaryWidth {
            get {
                return _temporaryWidth;
            }
            set {
                _temporaryWidth = value;
            }
        }

        //TabBarからしか呼んではだめ
        public void SetSelectedInternal(bool value) {
            _selected = value;
            this.BackColor = (value ? _parent.ActiveDrawing : _parent.NormalDrawing).BackgroundColor;
        }
        public void ClearMouseTrackingFlags() {
            _mouseDown = false;
            _mouseEnter = false;
        }

        public void UpdateDescription(TabBarUpdateState state) {
            _image = _tabKey.Icon;
            state.MarkSufficientWidthIsChanged(_parent);
        }

        internal int CalcSufficientWidth(Graphics g) {
            int img_width = _image == null ? 0 : _image.Width;
            TabBarDrawing d = GetDrawing();
            float index_width = g.MeasureString(_indexText, d.Font).Width;
            float text_width = g.MeasureString(_tabKey.Caption, d.Font).Width;

            _sufficientWidth =
                img_width
                + (int)Math.Floor(index_width)
                + (int)Math.Ceiling(text_width)
                + TEXT_MARGIN * 4 /* 左、右、img-index, index-bodyで４こ */
                + 2 /* border lines */;
            if (_selected)
                _sufficientWidth += 2; //選択時は広めがよさそう
            return _sufficientWidth;
        }

        //全部でwidthに収まるように考慮してテキストを設定する
        internal void AdjustTextAndWidth(Graphics g, int width) {
            int remaining = width - (_image == null ? 0 : _image.Width + TEXT_MARGIN) - TEXT_MARGIN * 2 - 2; //両端部分のマージンとボーダーを除く
            remaining = Math.Max(0, remaining);

            TabBarDrawing drawing = GetDrawing();

            float indexTextWidth = g.MeasureString(_indexText + "MMM...", drawing.Font).Width;
            if (indexTextWidth <= remaining)
                _showIndexText = true;
            else
                _showIndexText = false;

            this.Text = _tabKey.Caption;
            this.Width = width;
        }

        protected override void OnPaint(PaintEventArgs e) {
            //ToDo あとで選択性にする
            Graphics g = e.Graphics;
#if false //旧Poderosa風描画
            base.OnPaint(e);
            //border
            if (_selected)
                DrawUtil.DrawRoundRect(g, 0, 0, this.Width - 1, this.Height - 1, _parent.ActiveDrawing.RoundRectColors);
            else if (_mouseEnter)
                DrawUtil.DrawRoundRect(g, 0, 0, this.Width - 1, this.Height - 1, _parent.NormalDrawing.RoundRectColors);
#else //FireFox風
            if (_selected) {
                base.OnPaint(e); //背景塗る
                DrawUtil.DrawRoundRect(g, 0, 0, this.Width - 1, this.Height - 1, _parent.ActiveDrawing.RoundRectColors);
                DrawOrangeBar(g);
            }
            else {
                DrawUtil.FillHorizontalGradation(g, 0, 0, this.Width - 1, this.Height - 1, SystemColors.ControlLightLight, SystemColors.Control);
                //常にボタン風にするため周りを常に描画
                DrawUtil.DrawRoundRect(g, 0, 0, this.Width - 1, this.Height - 1, _parent.NormalDrawing.RoundRectColors);
                if (_mouseEnter)
                    DrawOrangeBar(g);
            }
#endif

            DrawButtonInternal(g);
        }
        private void DrawOrangeBar(Graphics g) {
            //オレンジキャプション
            Brush b = new SolidBrush(Color.Orange);
            g.FillRectangle(b, 1, 0, this.Width - 2, 2);
            b.Dispose();
        }


        public void Reset() {
            _mouseDown = false;
            _mouseEnter = false;
            Debug.Assert(!this.InvokeRequired);
            Invalidate();
        }
        protected override void OnMouseEnter(EventArgs e) {
            base.OnMouseEnter(e);
            _mouseEnter = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            _mouseEnter = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            _mouseDown = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            _mouseDown = false;
            Invalidate();
        }

        private void DrawButtonInternal(Graphics g) {
            Size clientSize = this.ClientSize;

#if false	// this is not so nice effect...
            int offsetY = _mouseDown ? 1 : 0;
#else
            const int offsetY = 0;
#endif

            int x = TEXT_MARGIN;
            if (_image != null) {
                int y = (clientSize.Height - _image.Height) / 2 + offsetY;
                DrawImage(g, DrawState.Normal, _image, x, y);
                x += _image.Width + TEXT_MARGIN;
            }

            TabBarDrawing drawing = GetDrawing();

            if (_showIndexText) {
                SizeF indexTextSize = g.MeasureString(_indexText, drawing.Font);
                g.DrawString(
                    _indexText,
                    drawing.Font,
                    SystemBrushes.ControlDark,
                    x,
                    (clientSize.Height - (int)Math.Ceiling(indexTextSize.Height) + 1) / 2 + offsetY);
                x += (int)Math.Floor(indexTextSize.Width) + TEXT_MARGIN;
            }

            RectangleF captionRect =
                new RectangleF(
                    x,
                    offsetY,
                    Math.Max(0, clientSize.Width - x - TEXT_MARGIN),
                    clientSize.Height);

            g.DrawString(
                _tabKey.Caption,
                drawing.Font,
                SystemBrushes.ControlText,
                captionRect,
                _captionFormat);
        }

        private TabBarDrawing GetDrawing() {
            return _selected ? _parent.ActiveDrawing : _parent.NormalDrawing;
        }


        private static void DrawImage(Graphics g, DrawState state, Image image, int x, int y) {
            if (state == DrawState.Normal)
                g.DrawImage(image, x, y, image.Width, image.Height);
            else if (state == DrawState.Selected || state == DrawState.Hover) {
                ControlPaint.DrawImageDisabled(g, image, x + 1, y, SystemColors.Control);
                g.DrawImage(image, x - 1, y - 1, image.Width, image.Height);
            }
        }
    }


    internal class DragAndDropSupport {
        internal class DropPoint {
            private TabBar _tabBar;
            //全体の右端のみrightとなる
            private TabBarButton _target;
            private bool _left; //左右の区別

            //buttonはnullのこともある。
            public DropPoint(TabBar tabbar, TabBarButton button, bool left) {
                _tabBar = tabbar;
                _target = button;
                _left = left;
            }

            public TabBarButton Target {
                get {
                    return _target;
                }
            }
            public bool ForLeft {
                get {
                    return _left;
                }
            }
            public bool ForRight {
                get {
                    return !_left;
                }
            }
            //コントロールの端からdistance離れた位置を返す
            public int PosX(int distance) {
                if (_target == null)
                    return distance;
                else
                    return _left ? _target.Left - distance : _target.Right + distance;
            }
            //DropPointが指し示すインデックス
            public int Index {
                get {
                    return _target == null ? 0 : (_target.Index + (_left ? 0 : 1));
                }
            }
            public TabBar TabBar {
                get {
                    return _tabBar;
                }
            }


            public override bool Equals(object obj) {
                DropPoint p = obj as DropPoint;
                return p != null && p._target == _target && p._left == _left;
            }
            //コンパイルエラー回避
            public override int GetHashCode() {
                return base.GetHashCode();
            }


            //nullとnullは等しい、という要素を入れた比較
            public static bool Equals(DropPoint p1, DropPoint p2) {
                if (p1 == null)
                    return p2 == null;
                else
                    return p1.Equals(p2); //p2がnullなら上記によってfalseが返る
            }

        }

        //DragAndDropSupport本体
        private TabBarButton _draggingButton;
        private DropPoint _dropPoint;

        public void StartDrag(TabBarButton btn) {
            _draggingButton = btn;
            btn.ParentTabBar.ParentTable.OnStartButtonDragByUI(_draggingButton.TabKey);

            //DoDragDrop起動元のコントロールに対してQuery系が呼ばれるらしい
            btn.ParentTabBar.DoDragDrop("poderosa.tabkey", DragDropEffects.Move | DragDropEffects.Link); //TODO この文字列いい加減
        }
        public bool CanDrop(IDataObject data, TabBar target) {
            string dragging = data.GetData(typeof(string)) as string;
            //同一Table内でしかドロップできないようにする。ウィンドウが異なっていたらビューへドロップできないとだめなので。
            return "poderosa.tabkey" == dragging && _draggingButton.ParentTabBar.ParentTable == target.ParentTable;
        }
        public DropPoint CurrentDropPoint {
            get {
                return _dropPoint;
            }
        }
        public bool OwnsDropPoint(TabBar tabbar) {
            if (_dropPoint == null)
                return false;
            else
                return _dropPoint.TabBar == tabbar;
        }
        public void SetDropPoint(DropPoint point) {
            if (!DropPoint.Equals(_dropPoint, point)) { //nullのこともあるので注意
                if (_dropPoint != null)
                    _dropPoint.TabBar.Invalidate();
                _dropPoint = point;
                if (point != null)
                    point.TabBar.Invalidate();
            }
        }
        public void ClearDropPoint() {
            if (_dropPoint != null) {
                _dropPoint.TabBar.Invalidate();
                _dropPoint = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public enum DropResult {
            Ignored,
            Reordered,
            Moved
        }

        public DropResult Drop(TabBarUpdateState state) {
            DropPoint point = _dropPoint;
            Debug.Assert(point != null);
            _dropPoint = null; //このPointを処理したあとはnullに戻る

            if (_draggingButton.ParentTabBar == point.TabBar) { //同じバーでのDrop
                return DropToSameTabBar(state, point);
            }
            else { //異なるバーへの移動
                return DropToDifferentTabBar(state, point);
            }
        }

        private DropResult DropToSameTabBar(TabBarUpdateState state, DropPoint point) {
            TabKey dragging_key = _draggingButton.TabKey;
            TabBar tabbar = _draggingButton.ParentTabBar;
            int ti = point.Target.Index;
            int di = _draggingButton.Index;
            int newindex = point.ForLeft ? ti : ti + 1;
            if (newindex == di || newindex == di + 1)
                return DropResult.Ignored;

            //入れ替えた状態でdocsを作る
            TabKey[] keys = new TabKey[tabbar.TabCount];
            int cursor = 0;
            for (int i = 0; i <= keys.Length; i++) { //右端へのDropまで考えるとiはCountまでまわす
                if (i == di)
                    continue; //skip

                if (i == newindex)
                    keys[cursor++] = dragging_key;
                if (i < keys.Length)
                    keys[cursor++] = tabbar.ButtonAt(i).TabKey;
            }
            Debug.Assert(cursor == keys.Length);

            tabbar.AssignDocuments(state, keys); //再構築
            tabbar.ParentTable.Activate(dragging_key, true);
            return DropResult.Reordered;
        }
        private DropResult DropToDifferentTabBar(TabBarUpdateState state, DropPoint point) {
            TabKey dragging_key = _draggingButton.TabKey;
            //移動元からの削除
            TabBar bar = _draggingButton.ParentTabBar;
            TabKey[] keys_src = new TabKey[bar.TabCount - 1];
            int cursor = 0;
            for (int i = 0; i < bar.TabCount; i++) {
                TabBarButton b = bar.ButtonAt(i);
                if (b != _draggingButton)
                    keys_src[cursor++] = b.TabKey;
            }
            Debug.Assert(cursor == keys_src.Length);
            bar.ParentTable.Deactivate(true);
            bar.AssignDocuments(state, keys_src);
            state.MarkIndexAssignmentChanged(bar);

            //移動先への追加
            bar = point.TabBar;
            TabKey[] docs_dst = new TabKey[bar.TabCount + 1];
            cursor = 0;
            int newindex = point.Index;
            for (int i = 0; i <= bar.TabCount; i++) {
                if (i == newindex)
                    docs_dst[cursor++] = dragging_key;
                if (i < bar.TabCount)
                    docs_dst[cursor++] = bar.ButtonAt(i).TabKey;
            }
            Debug.Assert(cursor == docs_dst.Length);
            bar.AssignDocuments(state, docs_dst);

            bar.ParentTable.Activate(dragging_key, true);

            state.MarkIndexAssignmentChanged(bar);
            return DropResult.Moved;
        }
    }

    //多段構成にするときに使う、TabBarのコレクション。ActiveなものはTable内で一つだけになる
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TabBarTable : UserControl {
        public interface IUIHandler {
            void ActivateTab(TabKey key);
            void MouseMiddleButton(TabKey key);
            void MouseRightButton(TabKey key);
            void StartTabDrag(TabKey key);
            void AllocateTabToControl(TabKey key, Control target);
            void BypassDragEnter(DragEventArgs args);
            void BypassDragDrop(DragEventArgs args);
        }
        private IUIHandler _uiHandler;

        /*
        public delegate void ButtonActionHandler(TabKey key);
        public delegate void ButtonDragByUIHandler(TabKey key);
        public delegate void ButtonActionAtBarHandler();
        public delegate void SideButtonClickHandler(Point pt);
        public delegate void ButtonAllocationToControlHandler(TabKey key, Control target);
        */
        private TabBarButton _activeButton; //nullになることも
        private List<TabBar> _bars;
        private CaptureStyleReplaceState _captureState;
        /*
        private ButtonActionHandler _activateByUIHandler;
        private ButtonActionHandler _mouseMiddleButtonHandler;
        private ButtonActionHandler _mouseRightButtonHandler;
        private ButtonActionAtBarHandler _mouseRightButtonAtBarHandler;
        private ButtonDragByUIHandler _buttonDragByUIHandler;
        private SideButtonClickHandler _sideButtonHandler;
        private ButtonAllocationToControlHandler _buttonAllocationToControlHandler;
        */

        public const int ROW_HEIGHT = 26;

        public TabBarTable() {
            _bars = new List<TabBar>();
            _captureState = new CaptureStyleReplaceState(this);
            this.Enabled = true;
        }
        public TabKey ActiveTabKey {
            get {
                return _activeButton == null ? null : _activeButton.TabKey;
            }
        }
        public IUIHandler UIHandler {
            get {
                return _uiHandler;
            }
            set {
                _uiHandler = value;
            }
        }

        internal CaptureStyleReplaceState CaptureState {
            get {
                return _captureState;
            }
        }

        //indexベースの処理
        public TabKey GetAtOrNull(int index) {
            foreach (TabBar bar in _bars) {
                if (index < bar.TabCount)
                    return bar.ButtonAt(index).TabKey;
                index -= bar.TabCount;
            }
            return null;
        }
        public int TabCount {
            get {
                int c = 0;
                foreach (TabBar bar in _bars) {
                    c += bar.TabCount;
                }
                return c;
            }
        }
        public int IndexOf(TabKey key) {
            int count = 0;
            foreach (TabBar bar in _bars) {
                foreach (TabBarButton button in bar.Buttons) {
                    if (button.TabKey == key)
                        return count + button.Index;
                }
                count += bar.TabCount;
            }
            return -1;
        }

        public void Create(int rowcount) {
            SetTabBarCount(rowcount);
        }
        //これは重い動作になりかねない
        public void SetTabBarCount(int count) {
            if (count == _bars.Count)
                return;

            using (TabBarUpdateState state = new TabBarUpdateState("set-tabbarcount")) {
                if (_bars.Count < count) { //増加
                    for (int i = _bars.Count; i < count; i++) {
                        TabBar bar = new TabBar(this);
                        bar.Location = new System.Drawing.Point(0, ROW_HEIGHT * i);
                        bar.Height = ROW_HEIGHT;
                        bar.Width = this.Width;
                        bar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                        this.Controls.Add(bar);
                        _bars.Add(bar);
                    }
                }
                else if (count < _bars.Count) { //減少
                    TabKey[] docs = GetAllDocuments();
                    while (_bars.Count > count) {
                        TabBar bar = _bars[_bars.Count - 1];
                        _bars.Remove(bar);
                        this.Controls.Remove(bar);
                    }
                    Rebalance(state, docs);
                }

                this.Height = ROW_HEIGHT * count;
                for (int i = 0; i < _bars.Count; i++)
                    _bars[i].Top = ROW_HEIGHT * i; //this.Heightを変更したらこれを再調整してやらんといかんようだ
            }
        }

        public int TabBarCount {
            get {
                return _bars.Count;
            }
        }
        public int GetAllTabCount() {
            int t = 0;
            foreach (TabBar bar in _bars)
                t += bar.TabCount;
            return t;
        }

        public bool ContainsKey(TabKey key) {
            return FindButton(key) != null;
        }

        public void Activate(TabKey key, bool fireExternalEvent) {
            using (TabBarUpdateState state = new TabBarUpdateState("activate")) {
                ActivateInternal(state, FindButton(key));

                if (fireExternalEvent)
                    _uiHandler.ActivateTab(key);
            }
        }

        public void ActivateNextTab(bool fireExternalEvent) {
            if (_activeButton == null)
                return;
            using (TabBarUpdateState state = new TabBarUpdateState("nexttab")) {
                bool match = false;
                TabBarButton buttonToActivate = null;
                TabBarButton firstButton = null;
                foreach (TabBar bar in _bars) {
                    foreach (TabBarButton button in bar.Buttons) {
                        if (match) {
                            buttonToActivate = button;
                            goto ACTIVATE;
                        }
                        match = object.ReferenceEquals(button, _activeButton);
                        if (firstButton == null)
                            firstButton = button;
                    }
                }
                if (match)
                    buttonToActivate = firstButton;

            ACTIVATE:
                if (buttonToActivate != null && !object.ReferenceEquals(buttonToActivate, _activeButton)) {
                    ActivateInternal(state, buttonToActivate);
                    if (fireExternalEvent)
                        _uiHandler.ActivateTab(buttonToActivate.TabKey);
                }
            }
        }

        public void ActivatePrevTab(bool fireExternalEvent) {
            if (_activeButton == null)
                return;
            using (TabBarUpdateState state = new TabBarUpdateState("prevtab")) {
                TabBarButton buttonToActivate = null;
                foreach (TabBar bar in _bars) {
                    foreach (TabBarButton button in bar.Buttons) {
                        if (object.ReferenceEquals(button, _activeButton) && buttonToActivate != null)
                            goto ACTIVATE;
                        buttonToActivate = button;
                    }
                }

            ACTIVATE:
                if (buttonToActivate != null && !object.ReferenceEquals(buttonToActivate, _activeButton)) {
                    ActivateInternal(state, buttonToActivate);
                    if (fireExternalEvent)
                        _uiHandler.ActivateTab(buttonToActivate.TabKey);
                }
            }
        }

        public void DoMiddleButtonAction(TabKey key) {
            _uiHandler.MouseMiddleButton(key);
        }
        public void DoRightButtonAction(TabKey key) {
            _uiHandler.MouseRightButton(key);
        }

        private void ActivateInternal(TabBarUpdateState state, TabBarButton button) {
            Debug.Assert(button.ParentTabBar.ParentTable == this);
            Debug.Assert(button != null);

            if (_activeButton != null) {
                _activeButton.SetSelectedInternal(false);
                state.MarkSufficientWidthIsChanged(_activeButton.ParentTabBar);
            }

            if (button == null)
                _activeButton = null;
            else {
                _activeButton = button;
                _activeButton.SetSelectedInternal(true);
                state.MarkSufficientWidthIsChanged(button.ParentTabBar);
            }

        }


        public void Deactivate(bool fire_external_event) {
            using (TabBarUpdateState state = new TabBarUpdateState("deactivate")) {
                if (_activeButton != null) {
                    _activeButton.SetSelectedInternal(false);
                    state.MarkSufficientWidthIsChanged(_activeButton.ParentTabBar);
                    _activeButton = null;
                }
            }
        }

        //ユーザインタフェースによるActivate
        public void OnActivatedByUI(TabKey key) {
            _uiHandler.ActivateTab(key);
        }
        public void OnMouseMiddleButton(TabKey key) {
            DoMiddleButtonAction(key);
        }
        public void OnMouseRightButton(TabKey key) {
            DoRightButtonAction(key);
        }
        //ユーザインタフェースによるDragAndDrop
        public void OnStartButtonDragByUI(TabKey key) {
            _uiHandler.StartTabDrag(key);
        }

        public void AddTab(TabKey key) {
            using (TabBarUpdateState state = new TabBarUpdateState("addtab")) {
                TabBar bar = FindFirstFreeTabBar();
                if (bar != null) {
                    //index計算面倒
                    int tabcount = 0;
                    int barindex = 0;
                    for (int i = 0; i < _bars.Count; i++) {
                        tabcount += _bars[i].TabCount;
                        if (_bars[i] == bar) {
                            barindex = i;
                            break;
                        }
                    }

                    bar.AddTab(state, key, tabcount++); //こいつの末尾なので
                    for (int i = barindex + 1; i < _bars.Count; i++) {
                        _bars[i].AllocateIndex(tabcount);
                        tabcount += _bars[i].TabCount;
                    }
                }
                else {
                    bar = _bars[_bars.Count - 1]; //仕方なく最後を使う
                    bar.AddTab(state, key, GetAllTabCount());
                    Rebalance(state, GetAllDocuments());
                }
            }
        }

        public void RemoveTab(TabKey key, bool fire_external_event) {
            using (TabBarUpdateState state = new TabBarUpdateState("removetab")) {
                if (_activeButton != null && _activeButton.TabKey == key)
                    Deactivate(fire_external_event);

                foreach (TabBar bar in _bars) {
                    TabBarButton b = bar.FindButton(key);
                    if (b != null) {
                        bar.RemoveTab(state, key);
                        break;
                    }
                }
            }
        }

        public void UpdateDescription(TabKey key) {
            using (TabBarUpdateState state = new TabBarUpdateState("updatetab")) {
                foreach (TabBar bar in _bars) {
                    TabBarButton b = bar.FindButton(key);
                    if (b != null) {
                        bar.FindButton(key).UpdateDescription(state);
                        break;
                    }
                }
            }
        }

        public void AssignIndex() {
            int t = 0;
            foreach (TabBar bar in _bars) {
                bar.AllocateIndex(t);
                t += bar.TabCount;
            }
        }

        public void Rebalance(TabBarUpdateState state, TabKey[] keys) {
            //タブの個数に応じて均等になるようにバランスを取る
            int count = keys.Length / _bars.Count;
            int mod = keys.Length % _bars.Count;
            int index = 0;

            for (int i = 0; i < _bars.Count; i++) {
                TabBar bar = _bars[i];
                int length = i < mod ? count + 1 : count; //余りを考慮
                bar.AssignDocuments(state, keys, index, length);
                index += length;
            }
        }

        public TabKey[] GetAllDocuments() {
            List<TabKey> r = new List<TabKey>();
            foreach (TabBar bar in _bars) {
                foreach (TabBarButton btn in bar.Buttons)
                    r.Add(btn.TabKey);
            }
            return r.ToArray();
        }

        private TabBar FindFirstFreeTabBar() {
            foreach (TabBar bar in _bars) {
                if (bar.LastAdjustmentResult == TabBar.AdjustEachWidthResult.Sufficient || bar.TabCount == 0)
                    return bar;
            }
            return null;
        }
        private TabBarButton FindButton(TabKey key) {
            foreach (TabBar bar in _bars) {
                TabBarButton b = bar.FindButton(key);
                if (b != null)
                    return b;
            }
            return null;
        }

        //子のボタンでハンドルできなかったDragDrop
        public void ByPassDragEnter(DragEventArgs args) {
            _uiHandler.BypassDragEnter(args);
        }
        public void ByPassDragDrop(DragEventArgs args) {
            _uiHandler.BypassDragDrop(args);
        }

        /*
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Rectangle sidebutton_rect = new Rectangle(this.Width-24, this.Height-24, 24, 24);
            if(e.ClipRectangle.IntersectsWith(sidebutton_rect)) {
                VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.Rebar.Chevron.Hot);
                renderer.DrawBackground(e.Graphics, sidebutton_rect);
            }
        }
         */
    }

    //必要な更新を管理する。TabBar#ArrangeButton等を過剰に呼ばないようにするための仕組み
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TabBarUpdateState : IDisposable {

        private enum UpdateState {
            AdjustmentIsRequired,
            SufficientWidthIsChanged //この状態の方が強い
        }

        private class Entry {
            public readonly TabBar TabBar;
            public UpdateState State;
            public Entry(TabBar tabBar, UpdateState state) {
                TabBar = tabBar;
                State = state;
            }
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private bool _indexAssignmentChanged = false;
        private bool _committed = false;

        private static readonly Mutex _mutex = new Mutex();
        private static string _lastAction;  // for debug
        private static string _nextAction;  // for debug

        public TabBarUpdateState(string action) {
            _nextAction = action;
            _mutex.WaitOne();
            _lastAction = action;
        }

        public void MarkAdjustmentRequired(TabBar bar) {
            UpdateOrCreateEntry(bar, UpdateState.AdjustmentIsRequired);
        }
        public void MarkSufficientWidthIsChanged(TabBar bar) {
            UpdateOrCreateEntry(bar, UpdateState.SufficientWidthIsChanged);
        }
        public void MarkIndexAssignmentChanged(TabBar bar) {
            UpdateOrCreateEntry(bar, UpdateState.SufficientWidthIsChanged);
            _indexAssignmentChanged = true; //全BARに影響
        }

        //ArrangeButtonsを呼べるのはここだけ
        private void Commit() {
            if (_committed)
                return;

            foreach (Entry e in _entries) {
                if (_indexAssignmentChanged) {
                    e.TabBar.ParentTable.AssignIndex(); //全バーで同一
                    _indexAssignmentChanged = false;
                }

                if (e.State == UpdateState.SufficientWidthIsChanged) {
                    e.TabBar.CalcSuffientWidths();
                    e.TabBar.ArrangeButtons();
                }
                else if (e.State == UpdateState.AdjustmentIsRequired) {
                    e.TabBar.ArrangeButtons();
                }
            }

            _indexAssignmentChanged = false;
            _entries.Clear();
            _committed = true;
        }

        private void UpdateOrCreateEntry(TabBar bar, UpdateState newState) {
            foreach (Entry e in _entries) {
                if (Object.ReferenceEquals(e.TabBar, bar)) {
                    if (e.State < newState)
                        e.State = newState;
                    return;
                }
            }
            _entries.Add(new Entry(bar, newState));
        }

        public void Dispose() {
            Commit();
            _mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    internal class CaptureStyleReplaceState {
        //マウスキャプチャ型のDoc-View割り当ての状態管理
        private TabKey _key; //割り当て中のブツ。nullなら未使用
        private TabBarTable _table;

        public CaptureStyleReplaceState(TabBarTable table) {
            _table = table;
        }


        public bool IsCapturing {
            get {
                return _key != null;
            }
        }
        public void StartCapture(TabBar tabbar, TabKey key) {
            _key = key;
            tabbar.Capture = true;
            Cursor.Current = Cursors.Cross;
        }
        public void CancelCapture() {
            _key = null;
        }
        public void EndCapture(Point screen_pt) {
            if (_key != null) {
                Form f = _table.FindForm();
                Control c = WinFormsUtil.FindTopControl(f, screen_pt); //他のウィンドウへ持っていくことができていない
                if (c != null) {
                    _table.UIHandler.AllocateTabToControl(_key, c);
                }
            }
            _key = null;
        }
    }
}