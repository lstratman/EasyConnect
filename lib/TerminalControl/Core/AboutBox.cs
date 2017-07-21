/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: AboutBox.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Resources;

using Poderosa.Commands;
using Poderosa.Preferences;
using Poderosa.Plugins;

namespace Poderosa.Forms {

    internal class AboutBox : System.Windows.Forms.Form {
        //private Image _bgImage;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _versionText;
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.PictureBox _pictureBox;
        private System.Windows.Forms.Button _creditButton;

        public AboutBox() {
            //
            // Windows フォーム デザイナ サポートに必要です。
            //
            InitializeComponent();

            StringResource strings = CoreUtil.Strings;
            this.Text = strings.GetString("Form.AboutBox.Text");
            _okButton.Text = strings.GetString("Common.OK");
            _creditButton.Text = strings.GetString("Form.AboutBox._creditButton");


        }

        /// <summary>
        /// 使用されているリソースに後処理を実行します。
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AboutBox));
            this._okButton = new System.Windows.Forms.Button();
            this._versionText = new System.Windows.Forms.TextBox();
            this._pictureBox = new System.Windows.Forms.PictureBox();
            this._creditButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.BackColor = System.Drawing.SystemColors.Control;
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._okButton.Location = new System.Drawing.Point(176, 192);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(88, 23);
            this._okButton.TabIndex = 0;
            // 
            // _versionText
            // 
            this._versionText.BackColor = System.Drawing.SystemColors.Window;
            this._versionText.Location = new System.Drawing.Point(0, 88);
            this._versionText.Multiline = true;
            this._versionText.Name = "_versionText";
            this._versionText.ReadOnly = true;
            this._versionText.Size = new System.Drawing.Size(280, 96);
            this._versionText.TabIndex = 2;
            this._versionText.Text = "";
            // 
            // _pictureBox
            // 
            this._pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("_pictureBox.Image")));
            this._pictureBox.Location = new System.Drawing.Point(0, 0);
            this._pictureBox.Name = "_pictureBox";
            this._pictureBox.Size = new System.Drawing.Size(280, 88);
            this._pictureBox.TabIndex = 3;
            this._pictureBox.TabStop = false;
            // 
            // _creditButton
            // 
            this._creditButton.BackColor = System.Drawing.SystemColors.Control;
            this._creditButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._creditButton.Location = new System.Drawing.Point(8, 192);
            this._creditButton.Name = "_creditButton";
            this._creditButton.Size = new System.Drawing.Size(88, 23);
            this._creditButton.TabIndex = 5;
            this._creditButton.Click += new EventHandler(OnCreditButton);
            // 
            // AboutBox
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
            this.CancelButton = this._okButton;
            this.ClientSize = new System.Drawing.Size(282, 224);
            this.Controls.Add(this._creditButton);
            this.Controls.Add(this._pictureBox);
            this.Controls.Add(this._versionText);
            this.Controls.Add(this._okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.OnLoad);
            this.ResumeLayout(false);

        }
        #endregion

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
        }


        private void OnLoad(object sender, System.EventArgs e) {
            _versionText.Lines = AboutBoxUtil.GetVersionInfoContent();
        }

        protected override bool ProcessDialogChar(char charCode) {
            if (AboutBoxUtil.ProcessDialogChar(charCode))
                this.Close();

            return base.ProcessDialogChar(charCode);
        }

        private void OnCreditButton(object sender, EventArgs args) {
            Credits dlg = new Credits();
            dlg.ShowDialog(this);
        }


    }

    internal class DefaultAboutBoxFactory : IPoderosaAboutBoxFactory {
        #region IPoderosaAboutBox
        public string AboutBoxID {
            get {
                return "default";
            }
        }

        public Form CreateAboutBox() {
            return new AboutBox();
        }

        private static Icon _mainFormIcon;
        public Icon ApplicationIcon {
            get {
                if (_mainFormIcon != null)
                    return _mainFormIcon;
                ResourceManager rm = new ResourceManager("Poderosa.Core.icons", typeof(WindowManagerPlugin).Assembly);
                _mainFormIcon = (Icon)rm.GetObject("app_icon");
                return _mainFormIcon;
            }
        }
        //デフォルトでは不要‘
        public string EnterMessage {
            get {
                return "";
            }
        }
        public string ExitMessage {
            get {
                return "";
            }
        }

        #endregion
    }

}
