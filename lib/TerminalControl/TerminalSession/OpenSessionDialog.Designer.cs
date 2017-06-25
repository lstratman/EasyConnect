// Copyright 2016 The Poderosa Project.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

namespace Poderosa.Sessions {
    partial class OpenSessionDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this._bottomPanel = new System.Windows.Forms.Panel();
            this._cancelButton = new System.Windows.Forms.Button();
            this._loginButton = new System.Windows.Forms.Button();
            this._sessionTypeTab = new System.Windows.Forms.TabControl();
            this._bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _bottomPanel
            // 
            this._bottomPanel.Controls.Add(this._cancelButton);
            this._bottomPanel.Controls.Add(this._loginButton);
            this._bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._bottomPanel.Location = new System.Drawing.Point(0, 210);
            this._bottomPanel.Margin = new System.Windows.Forms.Padding(0);
            this._bottomPanel.Name = "_bottomPanel";
            this._bottomPanel.Size = new System.Drawing.Size(312, 38);
            this._bottomPanel.TabIndex = 1;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._cancelButton.Location = new System.Drawing.Point(231, 9);
            this._cancelButton.Margin = new System.Windows.Forms.Padding(6);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
            // 
            // _loginButton
            // 
            this._loginButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._loginButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._loginButton.Location = new System.Drawing.Point(131, 9);
            this._loginButton.Name = "_loginButton";
            this._loginButton.Size = new System.Drawing.Size(75, 23);
            this._loginButton.TabIndex = 0;
            this._loginButton.UseVisualStyleBackColor = true;
            this._loginButton.Click += new System.EventHandler(this._loginButton_Click);
            // 
            // _sessionTypeTab
            // 
            this._sessionTypeTab.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this._sessionTypeTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sessionTypeTab.Location = new System.Drawing.Point(0, 0);
            this._sessionTypeTab.Name = "_sessionTypeTab";
            this._sessionTypeTab.SelectedIndex = 0;
            this._sessionTypeTab.Size = new System.Drawing.Size(312, 210);
            this._sessionTypeTab.TabIndex = 0;
            this._sessionTypeTab.SelectedIndexChanged += new System.EventHandler(this._sessionTypeTab_SelectedIndexChanged);
            // 
            // OpenSessionDialog
            // 
            this.AcceptButton = this._loginButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(312, 248);
            this.Controls.Add(this._sessionTypeTab);
            this.Controls.Add(this._bottomPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpenSessionDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpenSessionDialog_FormClosing);
            this.Load += new System.EventHandler(this.OpenSessionDialog_Load);
            this.Shown += new System.EventHandler(this.OpenSessionDialog_Shown);
            this._bottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _bottomPanel;
        protected System.Windows.Forms.Button _cancelButton;
        protected System.Windows.Forms.Button _loginButton;
        private System.Windows.Forms.TabControl _sessionTypeTab;


    }
}