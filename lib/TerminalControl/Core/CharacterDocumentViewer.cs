/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CharacterDocumentViewer.cs,v 1.16 2012/05/27 15:02:26 kzmi Exp $
 */
#if DEBUG
#define ONPAINT_TIME_MEASUREMENT
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

using Poderosa.Util;
using Poderosa.Document;
using Poderosa.Forms;
using Poderosa.UI;
using Poderosa.Sessions;
using Poderosa.Commands;

namespace Poderosa.View {
    /*
     * CharacterDocumentの表示を行うコントロール。機能としては次がある。
     * 　縦方向のみスクロールバーをサポート
     * 　再描画の最適化
     * 　キャレットの表示。ただしキャレットを適切に移動する機能は含まれない
     * 
     * 　今後あってもいいかもしれない機能は、行間やPadding(HTML用語の)、行番号表示といったところ
     */
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class CharacterDocumentViewer : Control, IPoderosaControl, ISelectionListener, SplitMarkSupport.ISite {

        public const int BORDER = 2; //内側の枠線のサイズ
        internal const int TIMER_INTERVAL = 50; //再描画最適化とキャレット処理を行うタイマーの間隔

        private CharacterDocument _document;
        private bool _errorRaisedInDrawing;
        private List<GLine> _transientLines; //再描画するGLineを一時的に保管する
        private TextSelection _textSelection;
        private SplitMarkSupport _splitMark;
        private bool _enabled; //ドキュメントがアタッチされていないときを示す 変更するときはEnabledExプロパティで！

        private Cursor _documentCursor = Cursors.IBeam;

        protected MouseHandlerManager _mouseHandlerManager;
        protected VScrollBar _VScrollBar;
        protected bool _enableAutoScrollBarAdjustment; //リサイズ時に自動的に_VScrollBarの値を調整するかどうか
        protected Caret _caret;
        protected ITimerSite _timer;
        protected int _tickCount;

        public delegate void OnPaintTimeObserver(Stopwatch s);

#if ONPAINT_TIME_MEASUREMENT
        private OnPaintTimeObserver _onPaintTimeObserver = null;
#endif

        public CharacterDocumentViewer() {
            _enableAutoScrollBarAdjustment = true;
            _transientLines = new List<GLine>();
            InitializeComponent();
            //SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.DoubleBuffer, true);
            this.DoubleBuffered = true;
            _caret = new Caret();

            _splitMark = new SplitMarkSupport(this, this);
            Pen p = new Pen(SystemColors.ControlDark);
            p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            _splitMark.Pen = p;

            _textSelection = new TextSelection(this);
            _textSelection.AddSelectionListener(this);

            _mouseHandlerManager = new MouseHandlerManager();
            _mouseHandlerManager.AddLastHandler(new TextSelectionUIHandler(this));
            _mouseHandlerManager.AddLastHandler(new SplitMarkUIHandler(_splitMark));
            _mouseHandlerManager.AttachControl(this);

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        public CharacterDocument CharacterDocument {
            get {
                return _document;
            }
        }
        internal TextSelection TextSelection {
            get {
                return _textSelection;
            }
        }
        public ITextSelection ITextSelection {
            get {
                return _textSelection;
            }
        }
        internal MouseHandlerManager MouseHandlerManager {
            get {
                return _mouseHandlerManager;
            }
        }

        public Caret Caret {
            get {
                return _caret;
            }
        }

        public bool EnabledEx {
            get {
                return _enabled;
            }
            set {
                _enabled = value;
                _VScrollBar.Visible = value; //スクロールバーとは連動
                _splitMark.Pen.Color = value ? SystemColors.ControlDark : SystemColors.Window; //このBackColorと逆で
                this.Cursor = GetDocumentCursor(); //Splitter.ISiteを援用
                this.BackColor = value ? GetRenderProfile().BackColor : SystemColors.ControlDark;
                this.ImeMode = value ? ImeMode.NoControl : ImeMode.Disable;
            }
        }
        public VScrollBar VScrollBar {
            get {
                return _VScrollBar;
            }
        }

        public void ShowVScrollBar() {
            _VScrollBar.Visible = true;
        }

        public void HideVScrollBar() {
            _VScrollBar.Visible = false;
        }

        public void SetDocumentCursor(Cursor cursor) {
            if (this.InvokeRequired) {
                this.BeginInvoke((MethodInvoker)delegate() {
                    SetDocumentCursor(cursor);
                });
                return;
            }
            _documentCursor = cursor;
            if (_enabled)
                this.Cursor = cursor;
        }

        public void ResetDocumentCursor() {
            if (this.InvokeRequired) {
                this.BeginInvoke((MethodInvoker)delegate() {
                    ResetDocumentCursor();
                });
                return;
            }
            SetDocumentCursor(Cursors.IBeam);
        }

        private Cursor GetDocumentCursor() {
            return _enabled ? _documentCursor : Cursors.Default;
        }


        #region IAdaptable
        public virtual IAdaptable GetAdapter(Type adapter) {
            return SessionManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion

        #region OnPaint time measurement

        public void SetOnPaintTimeObserver(OnPaintTimeObserver observer) {
#if ONPAINT_TIME_MEASUREMENT
            _onPaintTimeObserver = observer;
#endif
        }

        #endregion

        //派生型であることを強制することなどのためにoverrideすることを許す
        public virtual void SetContent(CharacterDocument doc) {
            RenderProfile prof = GetRenderProfile();
            this.BackColor = prof.BackColor;
            _document = doc;
            this.EnabledEx = doc != null;

            if (_timer != null)
                _timer.Close();
            if (this.EnabledEx) {
                _timer = WindowManagerPlugin.Instance.CreateTimer(TIMER_INTERVAL, new TimerDelegate(OnWindowManagerTimer));
                _tickCount = 0;
            }

            if (_enableAutoScrollBarAdjustment)
                AdjustScrollBar();
        }
        //タイマーの受信
        private void CaretTick() {
            if (_enabled && _caret.Blink) {
                _caret.Tick();
                _document.InvalidatedRegion.InvalidateLine(GetTopLine().ID + _caret.Y);
                InvalidateEx();
            }
        }
        protected virtual void OnWindowManagerTimer() {
            //タイマーはTIMER_INTERVALごとにカウントされるので。
            int q = WindowManagerPlugin.Instance.WindowPreference.OriginalPreference.CaretInterval / TIMER_INTERVAL;
            if (q == 0)
                q = 1;
            if (++_tickCount % q == 0)
                CaretTick();
        }


        //自己サイズからScrollBarを適切にいじる
        public void AdjustScrollBar() {
            if (_document == null)
                return;
            RenderProfile prof = GetRenderProfile();
            float ch = prof.Pitch.Height + prof.LineSpacing;
            int largechange = (int)Math.Floor((this.ClientSize.Height - BORDER * 2 + prof.LineSpacing) / ch); //きちんと表示できる行数をLargeChangeにセット
            int current = GetTopLine().ID - _document.FirstLineNumber;
            int size = Math.Max(_document.Size, current + largechange);
            if (size <= largechange) {
                _VScrollBar.Enabled = false;
            }
            else {
                _VScrollBar.Enabled = true;
                _VScrollBar.LargeChange = largechange;
                _VScrollBar.Maximum = size - 1; //この-1が必要なのが妙な仕様だ
            }
        }

        //このあたりの処置定まっていない
        private RenderProfile _privateRenderProfile = null;
        public void SetPrivateRenderProfile(RenderProfile prof) {
            _privateRenderProfile = prof;
        }

        //overrideして別の方法でRenderProfileを取得することもある
        public virtual RenderProfile GetRenderProfile() {
            return _privateRenderProfile;
        }

        protected virtual void CommitTransientScrollBar() {
            //ViewerはUIによってしか切り取れないからここでは何もしなくていい
        }

        //行数で表示可能な高さを返す
        protected virtual int GetHeightInLines() {
            RenderProfile prof = GetRenderProfile();
            float ch = prof.Pitch.Height + prof.LineSpacing;
            int height = (int)Math.Floor((this.ClientSize.Height - BORDER * 2 + prof.LineSpacing) / ch);
            return (height > 0) ? height : 0;
        }

        //_documentのうちどれを先頭(1行目)として表示するかを返す
        public virtual GLine GetTopLine() {
            return _document.FindLine(_document.FirstLine.ID + _VScrollBar.Value);
        }

        public void MousePosToTextPos(int mouseX, int mouseY, out int textX, out int textY) {
            SizeF pitch = GetRenderProfile().Pitch;
            textX = RuntimeUtil.AdjustIntRange((int)Math.Floor((mouseX - CharacterDocumentViewer.BORDER) / pitch.Width), 0, Int32.MaxValue);
            textY = RuntimeUtil.AdjustIntRange((int)Math.Floor((mouseY - CharacterDocumentViewer.BORDER) / (pitch.Height + GetRenderProfile().LineSpacing)), 0, Int32.MaxValue);
        }

        public void MousePosToTextPos_AllowNegative(int mouseX, int mouseY, out int textX, out int textY) {
            SizeF pitch = GetRenderProfile().Pitch;
            textX = (int)Math.Floor((mouseX - CharacterDocumentViewer.BORDER) / pitch.Width);
            textY = (int)Math.Floor((mouseY - CharacterDocumentViewer.BORDER) / (pitch.Height + GetRenderProfile().LineSpacing));
        }

        //_VScrollBar.ValueChangedイベント
        protected virtual void VScrollBarValueChanged() {
            if (_enableAutoScrollBarAdjustment)
                Invalidate();
        }

        //キャレットの座標設定、表示の可否を設定
        protected virtual void AdjustCaret(Caret caret) {
        }

        //_documentの更新状況を見て適切な領域のControl.Invalidate()を呼ぶ。
        //また、コントロールを所有していないスレッドから呼んでもOKなようになっている。
        protected void InvalidateEx() {
            if (this.IsDisposed)
                return;
            bool full_invalidate = true;
            Rectangle r = new Rectangle();

            if (_document != null) {
                if (_document.InvalidatedRegion.IsEmpty)
                    return;
                InvalidatedRegion rgn = _document.InvalidatedRegion.GetCopyAndReset();
                if (rgn.IsEmpty)
                    return;
                if (!rgn.InvalidatedAll) {
                    full_invalidate = false;
                    r.X = 0;
                    r.Width = this.ClientSize.Width;
                    int topLine = GetTopLine().ID;
                    int y1 = rgn.LineIDStart - topLine;
                    int y2 = rgn.LineIDEnd + 1 - topLine;
                    RenderProfile prof = GetRenderProfile();
                    r.Y = BORDER + (int)(y1 * (prof.Pitch.Height + prof.LineSpacing));
                    r.Height = (int)((y2 - y1) * (prof.Pitch.Height + prof.LineSpacing)) + 1;
                }
            }

            if (this.InvokeRequired) {
                if (full_invalidate)
                    this.BeginInvoke((MethodInvoker)delegate() {
                        Invalidate();
                    });
                else {
                    this.BeginInvoke((MethodInvoker)delegate() {
                        Invalidate(r);
                    });
                }
            }
            else {
                if (full_invalidate)
                    Invalidate();
                else
                    Invalidate(r);
            }
        }

        private void InitializeComponent() {
            this.SuspendLayout();
            this._VScrollBar = new System.Windows.Forms.VScrollBar();
            // 
            // _VScrollBar
            // 
            this._VScrollBar.Enabled = false;
            //this._VScrollBar.Dock = DockStyle.Right;
            this._VScrollBar.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            this._VScrollBar.LargeChange = 1;
            this._VScrollBar.Minimum = 0;
            this._VScrollBar.Value = 0;
            this._VScrollBar.Maximum = 2;
            this._VScrollBar.Name = "_VScrollBar";
            this._VScrollBar.TabIndex = 0;
            this._VScrollBar.TabStop = false;
            this._VScrollBar.Cursor = Cursors.Default;
            this._VScrollBar.Visible = false;
            this._VScrollBar.ValueChanged += delegate(object sender, EventArgs args) {
                VScrollBarValueChanged();
            };
            this.Controls.Add(_VScrollBar);

            this.ImeMode = ImeMode.NoControl;
            //this.BorderStyle = BorderStyle.Fixed3D; //IMEPROBLEM
            AdjustScrollBarPosition();
            this.ResumeLayout();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing) {
                _caret.Dispose();
                if (_timer != null)
                    _timer.Close();
                _splitMark.Pen.Dispose();
            }
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            if (_VScrollBar.Visible)
                AdjustScrollBarPosition();
            if (_enableAutoScrollBarAdjustment && _enabled)
                AdjustScrollBar();

            Invalidate();
        }

        //NOTE 自分のDockがTopかLeftのとき、スクロールバーの位置が追随してくれないみたい
        private void AdjustScrollBarPosition() {
            _VScrollBar.Height = this.ClientSize.Height;
            _VScrollBar.Left = this.ClientSize.Width - _VScrollBar.Width;
        }

        //描画の本体
        protected override sealed void OnPaint(PaintEventArgs e) {
#if ONPAINT_TIME_MEASUREMENT
            Stopwatch onPaintSw = (_onPaintTimeObserver != null) ? Stopwatch.StartNew() : null;
#endif

            base.OnPaint(e);

            try {
                if (_document != null)
                    ShowVScrollBar();
                else
                    HideVScrollBar();

                if (_enabled && !this.DesignMode) {
                    Rectangle clip = e.ClipRectangle;
                    Graphics g = e.Graphics;
                    RenderProfile profile = GetRenderProfile();
                    int paneheight = GetHeightInLines();

                    // determine background color of the view
                    Color backColor;
                    if (_document.IsApplicationMode) {
                        backColor = _document.ApplicationModeBackColor;
                        if (backColor.IsEmpty)
                            backColor = profile.BackColor;
                    }
                    else {
                        backColor = profile.BackColor;
                    }

                    if (this.BackColor != backColor)
                        this.BackColor = backColor; // set background color of the view

                    // draw background image if it is required.
                    if (!_document.IsApplicationMode) {
                        Image img = profile.GetImage();
                        if (img != null) {
                            DrawBackgroundImage(g, img, profile.ImageStyle, clip);
                        }
                    }

                    //描画用にテンポラリのGLineを作り、描画中にdocumentをロックしないようにする
                    //!!ここは実行頻度が高いのでnewを毎回するのは避けたいところだ
                    RenderParameter param = new RenderParameter();
                    _caret.Enabled = _caret.Enabled && this.Focused; //TODO さらにIME起動中はキャレットを表示しないように. TerminalControlだったらAdjustCaretでIMEをみてるので問題はない
                    lock (_document) {
                        CommitTransientScrollBar();
                        BuildTransientDocument(e, param);
                    }

                    DrawLines(g, param, backColor);

                    if (_caret.Enabled && (!_caret.Blink || _caret.IsActiveTick)) { //点滅しなければEnabledによってのみ決まる
                        if (_caret.Style == CaretType.Line)
                            DrawBarCaret(g, param, _caret.X, _caret.Y);
                        else if (_caret.Style == CaretType.Underline)
                            DrawUnderLineCaret(g, param, _caret.X, _caret.Y);
                    }
                }
                //マークの描画
                _splitMark.OnPaint(e);
            }
            catch (Exception ex) {
                if (!_errorRaisedInDrawing) { //この中で一度例外が発生すると繰り返し起こってしまうことがままある。なので初回のみ表示してとりあえず切り抜ける
                    _errorRaisedInDrawing = true;
                    RuntimeUtil.ReportException(ex);
                }
            }

#if ONPAINT_TIME_MEASUREMENT
            if (onPaintSw != null) {
                onPaintSw.Stop();
                if (_onPaintTimeObserver != null) {
                    _onPaintTimeObserver(onPaintSw);
                }
            }
#endif
        }

        private void BuildTransientDocument(PaintEventArgs e, RenderParameter param) {
            Rectangle clip = e.ClipRectangle;
            RenderProfile profile = GetRenderProfile();
            _transientLines.Clear();

            //Win32.SystemMetrics sm = GEnv.SystemMetrics;
            //param.TargetRect = new Rectangle(sm.ControlBorderWidth+1, sm.ControlBorderHeight,
            //	this.Width - _VScrollBar.Width - sm.ControlBorderWidth + 8, //この８がない値が正当だが、.NETの文字サイズ丸め問題のため行の最終文字が表示されないことがある。これを回避するためにちょっと増やす
            //	this.Height - sm.ControlBorderHeight);
            param.TargetRect = this.ClientRectangle;

            int offset1 = (int)Math.Floor((clip.Top - BORDER) / (profile.Pitch.Height + profile.LineSpacing));
            if (offset1 < 0)
                offset1 = 0;
            param.LineFrom = offset1;
            int offset2 = (int)Math.Floor((clip.Bottom - BORDER) / (profile.Pitch.Height + profile.LineSpacing));
            if (offset2 < 0)
                offset2 = 0;

            param.LineCount = offset2 - offset1 + 1;
            //Debug.WriteLine(String.Format("{0} {1} ", param.LineFrom, param.LineCount));

            int topline_id = GetTopLine().ID;
            GLine l = _document.FindLineOrNull(topline_id + param.LineFrom);
            if (l != null) {
                for (int i = param.LineFrom; i < param.LineFrom + param.LineCount; i++) {
                    _transientLines.Add(l.Clone()); //TODO クローンはきついよなあ　だが描画の方が時間かかるので、その間ロックをしないためには仕方ない点もある
                    l = l.NextLine;
                    if (l == null)
                        break;
                }
            }

            //以下、_transientLinesにはparam.LineFromから示される値が入っていることに注意

            //選択領域の描画
            if (!_textSelection.IsEmpty) {
                TextSelection.TextPoint from = _textSelection.HeadPoint;
                TextSelection.TextPoint to = _textSelection.TailPoint;
                l = _document.FindLineOrNull(from.Line);
                GLine t = _document.FindLineOrNull(to.Line);
                if (l != null && t != null) { //本当はlがnullではいけないはずだが、それを示唆するバグレポートがあったので念のため
                    t = t.NextLine;
                    int pos = from.Column; //たとえば左端を越えてドラッグしたときの選択範囲は前行末になるので pos==TerminalWidthとなるケースがある。
                    do {
                        int index = l.ID - (topline_id + param.LineFrom);
                        if (pos >= 0 && pos < l.DisplayLength && index >= 0 && index < _transientLines.Count) {
                            GLine r = null;
                            if (l.ID == to.Line) {
                                if (pos != to.Column)
                                    r = _transientLines[index].CreateInvertedClone(pos, to.Column);
                            }
                            else
                                r = _transientLines[index].CreateInvertedClone(pos, l.DisplayLength);

                            if (r != null) {
                                _transientLines[index] = r;
                            }
                        }
                        pos = 0; //２行目からの選択は行頭から
                        l = l.NextLine;
                    } while (l != t);
                }
            }

            AdjustCaret(_caret);
            _caret.Enabled = _caret.Enabled && (param.LineFrom <= _caret.Y && _caret.Y < param.LineFrom + param.LineCount);

            //Caret画面外にあるなら処理はしなくてよい。２番目の条件は、Attach-ResizeTerminalの流れの中でこのOnPaintを実行した場合にTerminalHeight>lines.Countになるケースがあるのを防止するため
            if (_caret.Enabled) {
                //ヒクヒク問題のため、キャレットを表示しないときでもこの操作は省けない
                if (_caret.Style == CaretType.Box) {
                    int y = _caret.Y - param.LineFrom;
                    if (y >= 0 && y < _transientLines.Count)
                        _transientLines[y].InvertCharacter(_caret.X, _caret.IsActiveTick, _caret.Color);
                }
            }
        }


        private void DrawLines(Graphics g, RenderParameter param, Color baseBackColor) {
            RenderProfile prof = GetRenderProfile();
            //Rendering Core
            if (param.LineFrom <= _document.LastLineNumber) {
                IntPtr hdc = g.GetHdc();
                try {
                    float y = (prof.Pitch.Height + prof.LineSpacing) * param.LineFrom + BORDER;
                    for (int i = 0; i < _transientLines.Count; i++) {
                        GLine line = _transientLines[i];
                        line.Render(hdc, prof, baseBackColor, BORDER, (int)y);
                        y += prof.Pitch.Height + prof.LineSpacing;
                    }
                }
                finally {
                    g.ReleaseHdc(hdc);
                }
            }
        }

        private void DrawBarCaret(Graphics g, RenderParameter param, int x, int y) {
            RenderProfile profile = GetRenderProfile();
            PointF pt1 = new PointF(profile.Pitch.Width * x + BORDER, (profile.Pitch.Height + profile.LineSpacing) * y + BORDER + 2);
            PointF pt2 = new PointF(pt1.X, pt1.Y + profile.Pitch.Height - 2);
            Pen p = _caret.ToPen(profile);
            g.DrawLine(p, pt1, pt2);
            pt1.X += 1;
            pt2.X += 1;
            g.DrawLine(p, pt1, pt2);
        }
        private void DrawUnderLineCaret(Graphics g, RenderParameter param, int x, int y) {
            RenderProfile profile = GetRenderProfile();
            PointF pt1 = new PointF(profile.Pitch.Width * x + BORDER + 2, (profile.Pitch.Height + profile.LineSpacing) * y + BORDER + profile.Pitch.Height);
            PointF pt2 = new PointF(pt1.X + profile.Pitch.Width - 2, pt1.Y);
            Pen p = _caret.ToPen(profile);
            g.DrawLine(p, pt1, pt2);
            pt1.Y += 1;
            pt2.Y += 1;
            g.DrawLine(p, pt1, pt2);
        }

        private void DrawBackgroundImage(Graphics g, Image img, ImageStyle style, Rectangle clip) {
            if (style == ImageStyle.HorizontalFit) {
                this.DrawBackgroundImage_Scaled(g, img, clip, true, false);
            }
            else if (style == ImageStyle.VerticalFit) {
                this.DrawBackgroundImage_Scaled(g, img, clip, false, true);
            }
            else if (style == ImageStyle.Scaled) {
                this.DrawBackgroundImage_Scaled(g, img, clip, true, true);
            }
            else {
                DrawBackgroundImage_Normal(g, img, style, clip);
            }
        }
        private void DrawBackgroundImage_Scaled(Graphics g, Image img, Rectangle clip, bool fitWidth, bool fitHeight) {
            Size clientSize = this.ClientSize;
            PointF drawPoint;
            SizeF drawSize;

            if (fitWidth && fitHeight) {
                drawSize = new SizeF(clientSize.Width - _VScrollBar.Width, clientSize.Height);
                drawPoint = new PointF(0, 0);
            }
            else if (fitWidth) {
                float drawWidth = clientSize.Width - _VScrollBar.Width;
                float drawHeight = drawWidth * img.Height / img.Width;
                drawSize = new SizeF(drawWidth, drawHeight);
                drawPoint = new PointF(0, (clientSize.Height - drawSize.Height) / 2f);
            }
            else {
                float drawHeight = clientSize.Height;
                float drawWidth = drawHeight * img.Width / img.Height;
                drawSize = new SizeF(drawWidth, drawHeight);
                drawPoint = new PointF((clientSize.Width - _VScrollBar.Width - drawSize.Width) / 2f, 0);
            }

            Region oldClip = g.Clip;
            using (Region newClip = new Region(clip)) {
                g.Clip = newClip;
                g.DrawImage(img, new RectangleF(drawPoint, drawSize), new RectangleF(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                g.Clip = oldClip;
            }
        }

        private void DrawBackgroundImage_Normal(Graphics g, Image img, ImageStyle style, Rectangle clip) {
            int offset_x, offset_y;
            if (style == ImageStyle.Center) {
                offset_x = (this.Width - _VScrollBar.Width - img.Width) / 2;
                offset_y = (this.Height - img.Height) / 2;
            }
            else {
                offset_x = (style == ImageStyle.TopLeft || style == ImageStyle.BottomLeft) ? 0 : (this.ClientSize.Width - _VScrollBar.Width - img.Width);
                offset_y = (style == ImageStyle.TopLeft || style == ImageStyle.TopRight) ? 0 : (this.ClientSize.Height - img.Height);
            }
            //if(offset_x < BORDER) offset_x = BORDER;
            //if(offset_y < BORDER) offset_y = BORDER;

            //画像内のコピー開始座標
            Rectangle target = Rectangle.Intersect(new Rectangle(clip.Left - offset_x, clip.Top - offset_y, clip.Width, clip.Height), new Rectangle(0, 0, img.Width, img.Height));
            if (target != Rectangle.Empty)
                g.DrawImage(img, new Rectangle(target.Left + offset_x, target.Top + offset_y, target.Width, target.Height), target, GraphicsUnit.Pixel);
        }

        //IPoderosaControl
        public Control AsControl() {
            return this;
        }

        //マウスホイールでのスクロール
        protected virtual void OnMouseWheelCore(MouseEventArgs e) {
            if (!this.EnabledEx)
                return;

            int d = e.Delta / 120; //開発環境だとDeltaに120。これで1か-1が入るはず
            d *= 3; //可変にしてもいいかも

            int newval = _VScrollBar.Value - d;
            if (newval < 0)
                newval = 0;
            if (newval > _VScrollBar.Maximum - _VScrollBar.LargeChange)
                newval = _VScrollBar.Maximum - _VScrollBar.LargeChange + 1;
            _VScrollBar.Value = newval;
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);
            OnMouseWheelCore(e);
        }


        //SplitMark関係
        #region SplitMark.ISite
        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            if (_splitMark.IsSplitMarkVisible)
                _mouseHandlerManager.EndCapture();
            _splitMark.ClearMark();
        }

        public bool CanSplit {
            get {
                return false;
            }
        }
        public int SplitClientWidth {
            get {
                return this.ClientSize.Width - (_enabled ? _VScrollBar.Width : 0);
            }
        }
        public int SplitClientHeight {
            get {
                return this.ClientSize.Height;
            }
        }
        public void OverrideCursor(Cursor cursor) {
            this.Cursor = cursor;
        }
        public void RevertCursor() {
            this.Cursor = GetDocumentCursor();
        }

        public void SplitVertically() {
            GetSplittableViewManager().SplitVertical(AsControlReplaceableView(), null);
        }
        public void SplitHorizontally() {
            GetSplittableViewManager().SplitHorizontal(AsControlReplaceableView(), null);
        }

        public SplitMarkSupport SplitMark {
            get {
                return _splitMark;
            }
        }

        #endregion

        private ISplittableViewManager GetSplittableViewManager() {
            IContentReplaceableView v = AsControlReplaceableView();
            if (v == null)
                return null;
            else
                return (ISplittableViewManager)v.ViewManager.GetAdapter(typeof(ISplittableViewManager));
        }
        private IContentReplaceableView AsControlReplaceableView() {
            IContentReplaceableViewSite site = (IContentReplaceableViewSite)this.GetAdapter(typeof(IContentReplaceableViewSite));
            return site == null ? null : site.CurrentContentReplaceableView;
        }

        #region ISelectionListener
        public void OnSelectionStarted() {
        }
        public void OnSelectionFixed() {
            if (WindowManagerPlugin.Instance.WindowPreference.OriginalPreference.AutoCopyByLeftButton) {
                ICommandTarget ct = (ICommandTarget)this.GetAdapter(typeof(ICommandTarget));
                if (ct != null) {
                    CommandManagerPlugin cm = CommandManagerPlugin.Instance;
                    if (Control.ModifierKeys == Keys.Shift) { //CopyAsLook
                        //Debug.WriteLine("CopyAsLook");
                        cm.Execute(cm.Find("org.poderosa.terminalemulator.copyaslook"), ct);
                    }
                    else {
                        //Debug.WriteLine("NormalCopy");
                        IGeneralViewCommands gv = (IGeneralViewCommands)GetAdapter(typeof(IGeneralViewCommands));
                        if (gv != null)
                            cm.Execute(gv.Copy, ct);
                    }
                }
            }

        }
        #endregion

    }

    /*
     * 何行目から何行目までを描画すべきかの情報を収録
     */
    internal class RenderParameter {
        private int _linefrom;
        private int _linecount;
        private Rectangle _targetRect;

        public int LineFrom {
            get {
                return _linefrom;
            }
            set {
                _linefrom = value;
            }
        }

        public int LineCount {
            get {
                return _linecount;
            }
            set {
                _linecount = value;
            }
        }
        public Rectangle TargetRect {
            get {
                return _targetRect;
            }
            set {
                _targetRect = value;
            }
        }
    }

    //テキスト選択のハンドラ
    internal class TextSelectionUIHandler : DefaultMouseHandler {
        private CharacterDocumentViewer _viewer;
        public TextSelectionUIHandler(CharacterDocumentViewer v)
            : base("textselection") {
            _viewer = v;
        }

        public override UIHandleResult OnMouseDown(MouseEventArgs args) {
            if (args.Button != MouseButtons.Left || !_viewer.EnabledEx)
                return UIHandleResult.Pass;

            //テキスト選択ではないのでちょっと柄悪いが。UserControl->Controlの置き換えに伴う
            if (!_viewer.Focused)
                _viewer.Focus();


            CharacterDocument document = _viewer.CharacterDocument;
            lock (document) {
                int col, row;
                _viewer.MousePosToTextPos(args.X, args.Y, out col, out row);
                int target_id = _viewer.GetTopLine().ID + row;
                TextSelection sel = _viewer.TextSelection;
                if (sel.State == SelectionState.Fixed)
                    sel.Clear(); //変なところでMouseDownしたとしてもClearだけはする
                if (target_id <= document.LastLineNumber) {
                    //if(InFreeSelectionMode) ExitFreeSelectionMode();
                    //if(InAutoSelectionMode) ExitAutoSelectionMode();
                    RangeType rt;
                    //Debug.WriteLine(String.Format("MouseDown {0} {1}", sel.State, sel.PivotType));

                    //同じ場所でポチポチと押すとChar->Word->Line->Charとモード変化する
                    if (sel.StartX != args.X || sel.StartY != args.Y)
                        rt = RangeType.Char;
                    else
                        rt = sel.PivotType == RangeType.Char ? RangeType.Word : sel.PivotType == RangeType.Word ? RangeType.Line : RangeType.Char;

                    //マウスを動かしていなくても、MouseDownとともにMouseMoveが来てしまうようだ
                    GLine tl = document.FindLine(target_id);
                    sel.StartSelection(tl, col, rt, args.X, args.Y);
                }
            }
            _viewer.Invalidate(); //NOTE 選択状態に変化のあった行のみ更新すればなおよし
            return UIHandleResult.Capture;
        }
        public override UIHandleResult OnMouseMove(MouseEventArgs args) {
            if (args.Button != MouseButtons.Left)
                return UIHandleResult.Pass;
            TextSelection sel = _viewer.TextSelection;
            if (sel.State == SelectionState.Fixed || sel.State == SelectionState.Empty)
                return UIHandleResult.Pass;
            //クリックだけでもなぜかMouseDownの直後にMouseMoveイベントが来るのでこのようにしてガード。でないと単発クリックでも選択状態になってしまう
            if (sel.StartX == args.X && sel.StartY == args.Y)
                return UIHandleResult.Capture;

            CharacterDocument document = _viewer.CharacterDocument;
            lock (document) {
                int topline_id = _viewer.GetTopLine().ID;
                SizeF pitch = _viewer.GetRenderProfile().Pitch;
                int row, col;
                _viewer.MousePosToTextPos_AllowNegative(args.X, args.Y, out col, out row);
                int viewheight = (int)Math.Floor(_viewer.ClientSize.Height / pitch.Width);
                int target_id = topline_id + row;

                GLine target_line = document.FindLineOrEdge(target_id);
                TextSelection.TextPoint point = sel.ConvertSelectionPosition(target_line, col);

                point.Line = RuntimeUtil.AdjustIntRange(point.Line, document.FirstLineNumber, document.LastLineNumber);

                if (_viewer.VScrollBar.Enabled) { //スクロール可能なときは
                    VScrollBar vsc = _viewer.VScrollBar;
                    if (target_id < topline_id) //前方スクロール
                        vsc.Value = point.Line - document.FirstLineNumber;
                    else if (point.Line >= topline_id + vsc.LargeChange) { //後方スクロール
                        int newval = point.Line - document.FirstLineNumber - vsc.LargeChange + 1;
                        if (newval < 0)
                            newval = 0;
                        if (newval > vsc.Maximum - vsc.LargeChange)
                            newval = vsc.Maximum - vsc.LargeChange + 1;
                        vsc.Value = newval;
                    }
                }
                else { //スクロール不可能なときは見えている範囲で
                    point.Line = RuntimeUtil.AdjustIntRange(point.Line, topline_id, topline_id + viewheight - 1);
                } //ここさぼっている
                //Debug.WriteLine(String.Format("MouseMove {0} {1} {2}", sel.State, sel.PivotType, args.X));
                RangeType rt = sel.PivotType;
                if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                    rt = RangeType.Word;
                else if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                    rt = RangeType.Line;

                GLine tl = document.FindLine(point.Line);
                sel.ExpandTo(tl, point.Column, rt);
            }
            _viewer.Invalidate(); //TODO 選択状態に変化のあった行のみ更新するようにすればなおよし
            return UIHandleResult.Capture;

        }
        public override UIHandleResult OnMouseUp(MouseEventArgs args) {
            TextSelection sel = _viewer.TextSelection;
            if (args.Button == MouseButtons.Left) {
                if (sel.State == SelectionState.Expansion || sel.State == SelectionState.Pivot)
                    sel.FixSelection();
                else
                    sel.Clear();
            }
            return _viewer.MouseHandlerManager.CapturingHandler == this ? UIHandleResult.EndCapture : UIHandleResult.Pass;

        }
    }

    //スプリットマークのハンドラ
    internal class SplitMarkUIHandler : DefaultMouseHandler {
        private SplitMarkSupport _splitMark;
        public SplitMarkUIHandler(SplitMarkSupport split)
            : base("splitmark") {
            _splitMark = split;
        }

        public override UIHandleResult OnMouseDown(MouseEventArgs args) {
            return UIHandleResult.Pass;
        }
        public override UIHandleResult OnMouseMove(MouseEventArgs args) {
            bool v = _splitMark.IsSplitMarkVisible;
            if (v || WindowManagerPlugin.Instance.WindowPreference.OriginalPreference.ViewSplitModifier == Control.ModifierKeys)
                _splitMark.OnMouseMove(args);
            //直前にキャプチャーしていたらEndCapture
            return _splitMark.IsSplitMarkVisible ? UIHandleResult.Capture : v ? UIHandleResult.EndCapture : UIHandleResult.Pass;
        }
        public override UIHandleResult OnMouseUp(MouseEventArgs args) {
            bool visible = _splitMark.IsSplitMarkVisible;
            if (visible) {
                //例えば、マーク表示位置から選択したいような場合を考慮し、マーク上で右クリックすると選択が消えるようにする。
                _splitMark.OnMouseUp(args);
                return UIHandleResult.EndCapture;
            }
            else
                return UIHandleResult.Pass;
        }
    }


}
