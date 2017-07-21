/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: GButton.cs,v 1.2 2011/10/27 23:21:59 kzmi Exp $
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace Poderosa.UI {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public enum DrawState {
        Normal,
        Disabled,
        Hover,
        Focused
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class GButton : UserControl {
        protected bool _mouseDown;
        protected bool _mouseEnter;
        protected bool _showComboStyle;
        protected bool _checked;
        protected Image _image;
        private const int COMBOAREA_WIDTH = 8;
        private BorderStyle _borderstyle;

        public BorderStyle BorderStyleEx {
            get {
                return _borderstyle;
            }
            set {
                _borderstyle = value;
            }
        }
        public bool Checked {
            get {
                return _checked;
            }
            set {
                _checked = value;
            }
        }
        public Image Image {
            get {
                return _image;
            }
            set {
                _image = value;
            }
        }
        public bool ShowComboStyle {
            get {
                return _showComboStyle;
            }
            set {
                _showComboStyle = value;
            }
        }

        public GButton() {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            _borderstyle = BorderStyle.None;
            Debug.Assert(!this.InvokeRequired);
        }

        public void Reset() {
            _mouseDown = false;
            _mouseEnter = false;
            this.Cursor = Cursors.Default;
            Debug.Assert(!this.InvokeRequired);
            Invalidate();
        }

        public int BodyWidth {
            get {
                int w = this.Width;
                if (_showComboStyle)
                    w -= COMBOAREA_WIDTH;
                return w;
            }
        }

        protected override void OnPaint(PaintEventArgs pe) {
            base.OnPaint(pe);
            DrawState st;
            if (_mouseDown)
                st = DrawState.Focused;
            else if (_mouseEnter)
                st = DrawState.Hover;
            else if (this.Enabled)
                st = DrawState.Normal;
            else
                st = DrawState.Disabled;

            DrawButtonState(pe.Graphics, st);
        }

        protected override void OnMouseEnter(EventArgs e) {
            try {
                base.OnMouseEnter(e);
                _mouseEnter = true;
                this.Cursor = Cursors.Hand;
                Invalidate();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.StackTrace);
            }

        }

        protected override void OnMouseLeave(EventArgs e) {
            try {
                base.OnMouseLeave(e);
                _mouseEnter = false;
                this.Cursor = Cursors.Default;
                Invalidate();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            try {
                base.OnMouseDown(e);
                _mouseDown = true;
                Invalidate();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            try {
                base.OnMouseUp(e);
                _mouseDown = false;
                Invalidate();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        protected virtual void DrawButtonState(Graphics g, DrawState state) {
            DrawBackground(g, state);

            bool has_text = false;
            bool has_image = this.Image != null;

            int x, y;
            if (has_text && !has_image) {
                x = BodyWidth / 2;
                y = Height / 2;
                DrawText(g, Text, state, x, y);
            }
            else if (has_image && !has_text) {
                x = (BodyWidth - this.Image.Width) / 2;
                y = (Height - this.Image.Height) / 2;
                if (_checked) {
                    x++;
                    y++;
                }
                DrawImage(g, state, this.Image, x, y);
            }
            else if (has_image && has_text) {
                x = 1;
                y = (Height - this.Image.Height) / 2;
                if (_checked) {
                    x++;
                    y++;
                }
                DrawImage(g, state, this.Image, x, y);
                x += this.Image.Width + 2;
                DrawText(g, this.Text, state, x, y);
            }

            if (_showComboStyle) {
                DrawComboStyleTriangle(g, state);
            }

        }

        protected void DrawBackground(Graphics g, DrawState state) {
            Rectangle rc = this.ClientRectangle;
            if (_focusedBackgroundBrush == null)
                CreateBrushes();


            if (state == DrawState.Normal || state == DrawState.Disabled) {
                g.FillRectangle(_checked ? SystemBrushes.ControlLight : SystemBrushes.Control, rc);
                if (_checked) {
                    ControlPaint.DrawBorder3D(g, rc, Border3DStyle.Sunken);
                }
                else if (_borderstyle != BorderStyle.None) {
                    //!!g.FillRectangle(new SolidBrush(this.BackColor), rc);
                    g.DrawRectangle(state == DrawState.Disabled ? SystemPens.ControlDark : SystemPens.ControlDarkDark, rc.Left, rc.Top, rc.Width - 1, rc.Height - 1);
                }
            }
            else if (state == DrawState.Hover || state == DrawState.Focused) {
                if (state == DrawState.Hover)
                    g.FillRectangle(_hoverBackgroundBrush, rc);
                else
                    g.FillRectangle(_focusedBackgroundBrush, rc);
                g.DrawRectangle(SystemPens.Highlight, rc.Left, rc.Top, rc.Width - 1, rc.Height - 1);
            }
        }

        protected static void DrawImage(Graphics g, DrawState state, Image image, int x, int y) {
            if (state == DrawState.Normal)
                g.DrawImage(image, x, y, image.Width, image.Height);
            else if (state == DrawState.Disabled)
                ControlPaint.DrawImageDisabled(g, image, x, y, SystemColors.Control);
            else if (state == DrawState.Focused || state == DrawState.Hover) {
                ControlPaint.DrawImageDisabled(g, image, x + 1, y, SystemColors.Control);
                g.DrawImage(image, x - 1, y - 1, image.Width, image.Height);
            }
        }

        protected void DrawText(Graphics g, string text, DrawState state, int x, int y) {
            if (state == DrawState.Disabled)
                g.DrawString(text, this.Font, SystemBrushes.ControlDark, new Point(x, y));
            else
                g.DrawString(text, this.Font, SystemBrushes.ControlText, new Point(x, y));
        }

        protected void DrawComboStyleTriangle(Graphics g, DrawState state) {
            Pen p = state == DrawState.Disabled ? SystemPens.ControlDark : SystemPens.ControlText;
            int x = this.Width - COMBOAREA_WIDTH;
            int y = this.Height / 2;
            g.DrawLine(p, x, y - 1, x + 5, y - 1);
            g.DrawLine(p, x + 1, y, x + 4, y);
            g.DrawLine(p, x + 2, y + 1, x + 3, y + 1);
        }

        private static Brush _focusedBackgroundBrush;
        private static Brush _hoverBackgroundBrush;

        private static void CreateBrushes() {
            _focusedBackgroundBrush = new SolidBrush(ColorUtil.VSNetPressedColor);
            _hoverBackgroundBrush = new SolidBrush(ColorUtil.VSNetSelectionColor);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class ToggleButton : GButton {
        private bool _autoToggle;

        public bool AutoToggle {
            get {
                return _autoToggle;
            }
            set {
                _autoToggle = value;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            if (_autoToggle)
                _checked = !_checked;
        }
    }


}
