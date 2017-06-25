/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PaneSplitter.cs,v 1.3 2011/12/17 11:35:04 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace Poderosa.UI {
    // Enumeration to specify the current animation state of the control.

    /// <exclude/>
    public enum SplitterState {
        Collapsed = 0,
        Expanding,
        Expanded,
        Collapsing
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class PaneSplitter : Splitter {
        public const int SPLITTER_WIDTH = 5;
        public const int TAG_LENGTH = 115;

        // declare and define some base properties
        private bool _isHot;
        private bool _enabledCollapse;
        private static Color _hotColor;
        private static Brush _hotBrush;
        private Control _targetControl;
        private Rectangle _tagRect;

        // Border added in version 1.3
        private Border3DStyle _borderStyle = Border3DStyle.RaisedInner;

        // animation controls introduced in version 1.22
        private int _savedControlWidth;
        private int _savedControlHeight;
        private SplitterState _state;

        static PaneSplitter() {
            _hotColor = CalculateColor(SystemColors.Highlight, SystemColors.Window, 70);
            _hotBrush = new SolidBrush(_hotColor);
        }

        /// <summary>
        /// The initial state of the Splitter. Set to True if the control to hide is not visible by default
        /// </summary>
        public bool IsCollapsed {
            get {
                return !_targetControl.Visible;
            }
        }

        public bool IsVSplitter {
            get {
                return this.Dock == DockStyle.Left || this.Dock == DockStyle.Right;
            }
        }
        public bool IsHSplitter {
            get {
                return this.Dock == DockStyle.Top || this.Dock == DockStyle.Bottom;
            }
        }
        public bool IsTimerActive {
            get {
                return _state == SplitterState.Collapsing || _state == SplitterState.Expanding;
            }
        }

        /// <summary>
        /// The System.Windows.Forms.Control that the splitter will collapse
        /// </summary>
        public Control TargetControl {
            get {
                return _targetControl;
            }
            set {
                _targetControl = value;
                this.Dock = _targetControl.Dock;
            }
        }
        public bool EnabledCollapse {
            get {
                return _enabledCollapse;
            }
            set {
                _enabledCollapse = value;
            }
        }

        /// <summary>
        /// An optional border style to paint on the control. Set to Flat for no border
        /// </summary>
        public Border3DStyle BorderStyle3D {
            get {
                return _borderStyle;
            }
            set {
                _borderStyle = value;
                Invalidate();
            }
        }

        public void ToggleState() {
            ToggleSplitter();
        }

        /// <summary>
        /// 全体のリサイズなどで元のサイズを保持することができなくなったときにこれを呼んでおく
        /// </summary>
        public void ClearSavedSize() {
            if (this.IsHSplitter)
                _savedControlHeight = this.MinSize;
            else
                _savedControlWidth = this.MinSize;
        }


        public PaneSplitter() {
            // Register mouse events
            //Click += new System.EventHandler(OnClick);
            DoubleClick += new System.EventHandler(OnDoubleClick);
            Resize += new System.EventHandler(OnResize);
            MouseLeave += new System.EventHandler(OnMouseLeave);
            MouseMove += new MouseEventHandler(OnMouseMove);

            //for both of H/V splitters
            this.Width = SPLITTER_WIDTH;
            this.Height = SPLITTER_WIDTH;

            this.MinSize = 32;
            this.MinExtra = 32;
        }

        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            if (_targetControl == null)
                throw new Exception("_targetControl must be set");
            if (this.Dock == DockStyle.Fill || this.Dock == DockStyle.None)
                throw new Exception("invalid DockStyle");

            // set the current state
            _state = _targetControl.Visible ? SplitterState.Expanded : SplitterState.Collapsed;
        }

        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        private void OnResize(object sender, EventArgs e) {
            // create a new rectangle in the vertical center of the splitter for our collapse control button
            _tagRect = this.IsVSplitter ?
                new Rectangle(0, ((this.ClientSize.Height - TAG_LENGTH) / 2), SPLITTER_WIDTH, TAG_LENGTH) :
                new Rectangle(((this.ClientSize.Width - TAG_LENGTH) / 2), 0, TAG_LENGTH, SPLITTER_WIDTH);
            //Invalidate();
        }

        // this method was updated in version 1.11 to fix a flickering problem
        // discovered by John O'Byrne
        private void OnMouseMove(object sender, MouseEventArgs e) {
            // check to see if the mouse cursor position is within the bounds of our control
            if (_tagRect.Contains(e.X, e.Y)) {
                if (!_isHot) {
                    _isHot = true;
                    //this.Cursor = Cursors.Hand;
                    Invalidate();
                }
            }
            else {
                if (_isHot) {
                    _isHot = false;
                    Invalidate();
                }
            }

            //カーソルは変えない
            if (!_targetControl.Visible)
                this.Cursor = Cursors.Default;
            else // Changed in v1.2 to support Horizontal Splitters
                this.Cursor = this.IsVSplitter ? Cursors.VSplit : Cursors.HSplit;
        }

        private void OnMouseLeave(object sender, System.EventArgs e) {
            // ensure that the _isHot state is removed
            _isHot = false;
            Invalidate();
        }

        private void OnDoubleClick(object sender, System.EventArgs e) {
            if (_enabledCollapse && _isHot && !this.IsTimerActive)
                ToggleSplitter();
        }

        private void ToggleSplitter() {
            _savedControlWidth = _targetControl.Width;
            _savedControlHeight = _targetControl.Height;

            if (_targetControl.Visible) {
                // no animations, so just toggle the visible state
                _state = SplitterState.Collapsed;
                _targetControl.Visible = false;
            }
            else {
                _state = SplitterState.Expanded;
                _targetControl.Visible = true;
            }
        }

        // OnPaint is now an override rather than an event in version 1.1
        protected override void OnPaint(PaintEventArgs e) {
            // create a Graphics object
            Graphics g = e.Graphics;

            // find the rectangle for the splitter and paint it
            Rectangle r = ClientRectangle; // fixed in version 1.1
            //g.FillRectangle(new SolidBrush(BackColor), r);

            //Vertical Splitter
            // Check the docking style and create the control rectangle accordingly
            if (this.IsVSplitter) {
                // draw the background color for our control image
                g.FillRectangle(_isHot ? _hotBrush : SystemBrushes.Control, new Rectangle(_tagRect.X, _tagRect.Y, SPLITTER_WIDTH, TAG_LENGTH));

                // draw the top & bottom lines for our control image
                g.DrawLine(SystemPens.ControlDark, _tagRect.X + 1, _tagRect.Y, _tagRect.X + _tagRect.Width - 2, _tagRect.Y);
                g.DrawLine(SystemPens.ControlDark, _tagRect.X + 1, _tagRect.Bottom, _tagRect.X + _tagRect.Width - 2, _tagRect.Bottom);

                // draw the dots for our control image using a loop
                int x = _tagRect.X + 1;
                int y = _tagRect.Y + 14;

                for (int i = 0; i < 30; i++) {
                    // light dot
                    g.DrawLine(SystemPens.ControlLightLight, x, y, x, y + 1);
                    // dark dot
                    g.DrawLine(SystemPens.ControlDarkDark, x + 1, y + 1, x + 1, y + 2);

                    y += 3;
                }
            }
            else { //HSplitter
                // draw the background color for our control image
                g.FillRectangle(_isHot ? _hotBrush : SystemBrushes.Control, new Rectangle(_tagRect.X, _tagRect.Y, TAG_LENGTH, SPLITTER_WIDTH));

                // draw the left & right lines for our control image
                g.DrawLine(SystemPens.ControlDark, _tagRect.X, _tagRect.Y + 1, _tagRect.X, _tagRect.Bottom - 2);
                g.DrawLine(SystemPens.ControlDark, _tagRect.Right, _tagRect.Y + 1, _tagRect.Right, _tagRect.Bottom - 2);

                // draw the dots for our control image using a loop
                int x = _tagRect.X + 14;
                int y = _tagRect.Y + 1;

                for (int i = 0; i < 30; i++) {
                    // light dot
                    g.DrawLine(SystemPens.ControlLightLight, x, y, x + 1, y);
                    // dark dot
                    g.DrawLine(SystemPens.ControlDarkDark, x + 1, y + 1, x + 2, y + 1);

                    x += 3;
                }

            }

            // Added in version 1.3
            if (_borderStyle != Border3DStyle.Flat) {
                // Paint the control border
                ControlPaint.DrawBorder3D(e.Graphics, r, _borderStyle, Border3DSide.Top);
                ControlPaint.DrawBorder3D(e.Graphics, r, _borderStyle, Border3DSide.Bottom);
            }

        }

        // this method was borrowed from the RichUI Control library by Sajith M
        //TODO: Util化
        private static Color CalculateColor(Color front, Color back, int alpha) {
            // solid color obtained as a result of alpha-blending

            Color frontColor = Color.FromArgb(255, front);
            Color backColor = Color.FromArgb(255, back);

            float frontRed = frontColor.R;
            float frontGreen = frontColor.G;
            float frontBlue = frontColor.B;
            float backRed = backColor.R;
            float backGreen = backColor.G;
            float backBlue = backColor.B;

            float fRed = frontRed * alpha / 255 + backRed * ((float)(255 - alpha) / 255);
            byte newRed = (byte)fRed;
            float fGreen = frontGreen * alpha / 255 + backGreen * ((float)(255 - alpha) / 255);
            byte newGreen = (byte)fGreen;
            float fBlue = frontBlue * alpha / 255 + backBlue * ((float)(255 - alpha) / 255);
            byte newBlue = (byte)fBlue;

            return Color.FromArgb(255, newRed, newGreen, newBlue);
        }

    }

    //分割マークの描画系
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class SplitMarkSupport {
        private enum MarkState {
            None,
            TopBottom,
            LeftRight
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public interface ISite {
            SplitMarkSupport SplitMark {
                get;
            }
            bool CanSplit {
                get;
            }
            int SplitClientWidth {
                get;
            }
            int SplitClientHeight {
                get;
            }
            void SplitVertically();
            void SplitHorizontally();
            void OverrideCursor(Cursor cursor);
            void RevertCursor();
        }

        //VisualStyleElementとScrollButtonのペア
        private struct ArrowElement {
            public VisualStyleElement VSElement;
            public ScrollButton ScrollButton;

            public ArrowElement(ScrollButton sb) {
                this.ScrollButton = sb;
                VSElement = null;
                if (VisualStyleInformation.IsSupportedByOS) { //OSでサポートがなければVisualStyleは設定しない
                    if (sb == ScrollButton.Left)
                        VSElement = VisualStyleElement.Spin.DownHorizontal.Hot;
                    else if (sb == ScrollButton.Right)
                        VSElement = VisualStyleElement.Spin.UpHorizontal.Hot;
                    else if (sb == ScrollButton.Down)
                        VSElement = VisualStyleElement.Spin.Down.Hot;
                    else if (sb == ScrollButton.Up)
                        VSElement = VisualStyleElement.Spin.Up.Hot;
                }
            }
        }

        private const int MIN_DISTANCE = 12; //ターゲットの点からこれだけ以下の距離だったら状態変更

        private bool _enabled;
        private Pen _pen;
        private Control _target;
        private MarkState _state;
        private ISite _output;
        private bool _markCancelling; //選択したくなったときとの兼ね合いで、マーク表示中に特定操作(右クリック)でマーク非表示にする

        private static ArrowElement _elementTop;
        private static ArrowElement _elementBottom;
        private static ArrowElement _elementLeft;
        private static ArrowElement _elementRight;

        //一度だけ計算
        private static Size _markSizeTopBottom;
        private static Size _markSizeLeftRight;

        static SplitMarkSupport() {
            _elementTop = new ArrowElement(ScrollButton.Down);
            _elementBottom = new ArrowElement(ScrollButton.Up);
            _elementLeft = new ArrowElement(ScrollButton.Right);
            _elementRight = new ArrowElement(ScrollButton.Left);
        }

        public SplitMarkSupport(Control target, ISite output) {
            _target = target;
            _output = output;
            _enabled = true;
        }

        public bool Enabled {
            get {
                return _enabled;
            }
            set {
                _enabled = value;
            }
        }
        public Pen Pen {
            get {
                return _pen;
            }
            set {
                _pen = value;
            }
        }
        public bool IsSplitMarkVisible {
            get {
                return _state != MarkState.None;
            }
        }
        public void ClearMark() {
            if (_state != MarkState.None) {
                _state = MarkState.None;
                _output.RevertCursor();
                _target.Invalidate();
                _markCancelling = false;
            }
        }


        //描画
        public void OnPaint(PaintEventArgs args) {
            if (_state == MarkState.None)
                return;

            Graphics g = args.Graphics;
            if (_markSizeTopBottom.IsEmpty)
                CalcSize(g);

            if (_state == MarkState.TopBottom) {
                Size size = _markSizeTopBottom;
                int mid = _output.SplitClientWidth / 2;
                DrawMark(g, _elementTop, new Rectangle(mid - size.Width / 2, 0, size.Width, size.Height));
                DrawMark(g, _elementBottom, new Rectangle(mid - size.Width / 2, _output.SplitClientHeight - size.Height, size.Width, size.Height));
                if (_pen != null)
                    g.DrawLine(_pen, mid, size.Height, mid, _output.SplitClientHeight - size.Height);
            }
            else { //LeftRight
                Size size = _markSizeLeftRight;
                int mid = _output.SplitClientHeight / 2;
                DrawMark(g, _elementLeft, new Rectangle(0, mid - size.Height / 2, size.Width, size.Height));
                DrawMark(g, _elementRight, new Rectangle(_output.SplitClientWidth - size.Width, mid - size.Height / 2, size.Width, size.Height));
                if (_pen != null)
                    g.DrawLine(_pen, size.Width, mid, _output.SplitClientWidth - size.Width, mid);
            }
        }

        private void DrawMark(Graphics g, ArrowElement element, Rectangle rect) {
            if (VisualStyleInformation.IsEnabledByUser) {
                VisualStyleRenderer renderer = new VisualStyleRenderer(element.VSElement); //TODO new避ける
                renderer.DrawBackground(g, rect);
                //背景がSystemColors.ControlであることをVisualStyleは想定しているらしく、枠が見えて見苦しいことがある。
                //VisualStyleRendererに背景色を指示する方法はないみたいなので手動で。

                //TODO 方向により異なる三辺を塗る必要があるようだ。めんどうくさい!
                /*Pen pen = new Pen(_target.BackColor);
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width-1, rect.Height-1);
                pen.Dispose();*/
            }
            else {
                ControlPaint.DrawScrollButton(g, rect, element.ScrollButton, ButtonState.Normal);
            }
        }

        //マウスの動きに従って状態通知
        public void OnMouseMove(MouseEventArgs args) {
            MarkState previous = _state;

            _state = MarkState.None;
            if (_enabled && _output.CanSplit) {
                int x = args.X;
                int y = args.Y;
                if (Near(x, y, _output.SplitClientWidth / 2, MIN_DISTANCE))
                    _state = MarkState.TopBottom;
                else if (Near(x, y, _output.SplitClientWidth / 2, _output.SplitClientHeight - MIN_DISTANCE))
                    _state = MarkState.TopBottom;
                else if (Near(x, y, MIN_DISTANCE, _output.SplitClientHeight / 2))
                    _state = MarkState.LeftRight;
                else if (Near(x, y, _output.SplitClientWidth - MIN_DISTANCE, _output.SplitClientHeight / 2))
                    _state = MarkState.LeftRight;

                if (_state == MarkState.None)
                    _markCancelling = false; //マウス移動の結果分割用意でなくなった場合キャンセル解除
                else if (_markCancelling)
                    _state = MarkState.None; //キャンセル発動中
            }

            //状態変化があったら
            if (previous != _state) {
                _target.Invalidate(); //再描画を促す
                if (_state == MarkState.None)
                    _output.RevertCursor();
                else
                    _output.OverrideCursor(Cursors.Hand);
            }
        }

        public void OnMouseUp(MouseEventArgs args) {
            if (args.Button == MouseButtons.Left) {
                if (_state == MarkState.TopBottom) {
                    ClearMark();
                    _output.SplitVertically();
                }
                else if (_state == MarkState.LeftRight) {
                    ClearMark();
                    _output.SplitHorizontally();
                }
            }

            if (args.Button == MouseButtons.Right) {
                ClearMark();
                _markCancelling = true;
            }
        }

        private static bool Near(int mx, int my, int x, int y) {
            return Math.Abs(mx - x) <= MIN_DISTANCE && Math.Abs(my - y) <= MIN_DISTANCE;
        }


        private static void CalcSize(Graphics g) {
            if (VisualStyleInformation.IsEnabledByUser) {
                VisualStyleRenderer renderer = new VisualStyleRenderer(_elementTop.VSElement);
                _markSizeTopBottom = renderer.GetPartSize(g, ThemeSizeType.True);
                renderer.SetParameters(_elementLeft.VSElement);
                _markSizeLeftRight = renderer.GetPartSize(g, ThemeSizeType.True);
            }
            else {
                _markSizeTopBottom = new Size(16, 16); //このサイズでは設定によってはダメかも
                _markSizeLeftRight = new Size(16, 16);
            }
        }
    }
}
