/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PopupViewContainer.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Poderosa.Sessions;
using Poderosa.Commands;

namespace Poderosa.Forms {

    //ポップアップして、単一のビューを内部に持つフォームを
    internal class PopupViewContainer : PoderosaForm, IPoderosaPopupWindow {
        private System.ComponentModel.IContainer components = null;
        private IPoderosaView _view;

        public PopupViewContainer(PopupViewCreationParam cp) {
            this.SuspendLayout();
            InitializeComponent();
            _view = cp.ViewFactory.CreateNew(this);
            _view.AsControl().Dock = DockStyle.Fill;
            _view.AsControl().Size = cp.InitialSize;
            this.Controls.Add(_view.AsControl());
            this.ClientSize = cp.InitialSize;
            this.ResumeLayout(false);
        }


        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            // 
            // PopupViewContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "PopupViewContainer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        }


        public IPoderosaView InternalView {
            get {
                return _view;
            }
        }
        public void UpdateStatus() {
            IPoderosaDocument doc = _view == null ? null : _view.Document;
            if (doc == null)
                this.Text = "";
            else
                this.Text = doc.Caption;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
            base.OnClosing(e);
            try {
                if (SessionManagerPlugin.Instance == null)
                    return; //単体テストではSessionなしで起動することもありだ
                IPoderosaDocument doc = _view.Document;
                if (doc == null)
                    return;

                PrepareCloseResult r = SessionManagerPlugin.Instance.CloseDocument(doc);
                if (r == PrepareCloseResult.Cancel) {
                    _closeCancelled = true;
                    e.Cancel = true;
                }
                else
                    e.Cancel = false;
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
                e.Cancel = false; //バグのためにウィンドウを閉じることもできない、というのはまずい
            }
        }

    }
}