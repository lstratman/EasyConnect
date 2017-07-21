/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: WarningWithDisableOption.cs,v 1.3 2011/10/27 23:21:58 kzmi Exp $
 */
using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace Poderosa.Forms {
    /// <summary>
    /// WarningWithDisableOption の概要の説明です。
    /// </summary>
    internal class WarningWithDisableOption : System.Windows.Forms.Form {
        private static Icon _warningIcon;

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label _messageLabel;
        private CheckBox _disableCheckBox;
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.Container components = null;

        public WarningWithDisableOption(string message) {
            //
            // Windows フォーム デザイナ サポートに必要です。
            //
            InitializeComponent();

            _messageLabel.Text = message;
            this.Text = GEnv.Strings.GetString("Form.WarningWithDisableOption.Text");
            this._disableCheckBox.Text = GEnv.Strings.GetString("Form.WarningWithDisableOption._disableCheckBox");
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
            this._okButton = new System.Windows.Forms.Button();
            this._messageLabel = new System.Windows.Forms.Label();
            this._disableCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._okButton.Location = new System.Drawing.Point(120, 72);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 0;
            this._okButton.Text = "OK";
            // 
            // _messageLabel
            // 
            this._messageLabel.Location = new System.Drawing.Point(56, 8);
            this._messageLabel.Name = "_messageLabel";
            this._messageLabel.Size = new System.Drawing.Size(248, 40);
            this._messageLabel.TabIndex = 1;
            this._messageLabel.Text = "a";
            this._messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _disableCheckBox
            // 
            this._disableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._disableCheckBox.Location = new System.Drawing.Point(56, 48);
            this._disableCheckBox.Name = "_disableCheckBox";
            this._disableCheckBox.Size = new System.Drawing.Size(248, 24);
            this._disableCheckBox.TabIndex = 2;
            // 
            // WarningWithDisableOption
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
            this.CancelButton = this._okButton;
            this.ClientSize = new System.Drawing.Size(314, 103);
            this.Controls.Add(this._disableCheckBox);
            this.Controls.Add(this._messageLabel);
            this.Controls.Add(this._okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WarningWithDisableOption";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);

        }
        #endregion

        protected override void OnPaint(PaintEventArgs a) {
            base.OnPaint(a);
            //アイコンの描画　.NET Frameworkだけでシステムで持っているアイコンのロードはできないようだ
            if (_warningIcon == null)
                LoadWarningIcon();
            a.Graphics.DrawIcon(_warningIcon, 12, 24);
        }

        public bool CheckedDisableOption {
            get {
                return _disableCheckBox.Checked;
            }
        }

        private static void LoadWarningIcon() {
            IntPtr hIcon = Win32.LoadIcon(IntPtr.Zero, new IntPtr(Win32.IDI_EXCLAMATION));
            _warningIcon = Icon.FromHandle(hIcon);
        }

    }
}
