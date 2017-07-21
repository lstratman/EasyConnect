/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: StatusBar.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Poderosa.Forms {
    internal class PoderosaStatusBar : StatusStrip, IPoderosaStatusBar {
        private Timer _timer;

        //TODO この構成はExtPからもってこないと
        private ToolStripStatusLabel _message;
        private ToolStripStatusLabel _bell;
        private int _defaultHeight;

        public PoderosaStatusBar() {
            _message = CreateMessagePane();
            _bell = CreateIconPane();
            _defaultHeight = this.Height;

            this.Dock = DockStyle.Bottom;
            this.Items.Add(_message);
            this.Items.Add(_bell);

            _timer = new Timer();
            _timer.Interval = 800;
            _timer.Tick += new EventHandler(OnTimer);
        }

        private ToolStripStatusLabel CreateIconPane() {
            ToolStripStatusLabel l = CreateDefaultStipStatusLabel();
            l.DisplayStyle = ToolStripItemDisplayStyle.Image;
            l.Size = new Size(18, 18);
            return l;
        }

        private ToolStripStatusLabel CreateMessagePane() {
            ToolStripStatusLabel l = new ToolStripStatusLabel();
            l.Spring = true;
            l.TextAlign = ContentAlignment.MiddleLeft;
            return l;
        }

        //分離された領域を作成
        private static ToolStripStatusLabel CreateDefaultStipStatusLabel() {
            ToolStripStatusLabel l = new ToolStripStatusLabel();
            l.AutoSize = false;
            l.BorderSides = ToolStripStatusLabelBorderSides.All;
            l.BorderStyle = Border3DStyle.SunkenInner;
            return l;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing)
                _timer.Dispose();
        }

        #region IPoderosaStatusBar

        private delegate void SetMainTextDelegate(string msg);
        private delegate void SetStatusIconDelegate(Image icon);

        public void SetMainText(string msg) {
            if (this.InvokeRequired) {
                this.Invoke(new SetMainTextDelegate(SetMainText), msg);
                return;
            }

            _message.Text = msg;
            _timer.Stop();
            _timer.Start();
        }

        public void SetStatusIcon(Image icon) {
            if (this.InvokeRequired) {
                this.Invoke(new SetStatusIconDelegate(SetStatusIcon), icon);
                return;
            }

            _bell.Image = icon;
            _timer.Stop();
            _timer.Start();
        }
        #endregion

        //TODO ちょっと雑。ベルと同じタイマーでいいのか？
        private void OnTimer(object sender, EventArgs args) {
            Debug.Assert(!this.InvokeRequired);
            _bell.Image = null;
            _message.Text = "";
            this.Height = _defaultHeight; //複数行テキストのときなどで高さが拡大してしまうことがある
        }
    }
}
