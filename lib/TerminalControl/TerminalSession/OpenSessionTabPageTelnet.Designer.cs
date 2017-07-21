// Copyright 2016 The Poderosa Project.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

namespace Poderosa.Sessions {
    partial class OpenSessionTabPageTelnet {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this._icons = new System.Windows.Forms.ImageList(this.components);
            this._hostLabel = new System.Windows.Forms.Label();
            this._hostBox = new System.Windows.Forms.ComboBox();
            this._portLabel = new System.Windows.Forms.Label();
            this._optionsTab = new System.Windows.Forms.TabControl();
            this._terminalTabPage = new System.Windows.Forms.TabPage();
            this._telnetNewLine = new System.Windows.Forms.CheckBox();
            this._logTypeLabel = new System.Windows.Forms.Label();
            this._logTypeBox = new System.Windows.Forms.ComboBox();
            this._logFileLabel = new System.Windows.Forms.Label();
            this._selectLogButton = new System.Windows.Forms.Button();
            this._encodingLabel = new System.Windows.Forms.Label();
            this._encodingBox = new System.Windows.Forms.ComboBox();
            this._localEchoLabel = new System.Windows.Forms.Label();
            this._localEchoBox = new System.Windows.Forms.ComboBox();
            this._logFileBox = new System.Windows.Forms.TextBox();
            this._newLineLabel = new System.Windows.Forms.Label();
            this._newLineBox = new System.Windows.Forms.ComboBox();
            this._terminalTypeLabel = new System.Windows.Forms.Label();
            this._terminalTypeBox = new System.Windows.Forms.ComboBox();
            this._macroTabPage = new System.Windows.Forms.TabPage();
            this._autoExecMacroPathLabel = new System.Windows.Forms.Label();
            this._autoExecMacroPathBox = new System.Windows.Forms.TextBox();
            this._selectAutoExecMacroButton = new System.Windows.Forms.Button();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._portBox = new System.Windows.Forms.ComboBox();
            this._optionsTab.SuspendLayout();
            this._terminalTabPage.SuspendLayout();
            this._macroTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _icons
            // 
            this._icons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this._icons.ImageSize = new System.Drawing.Size(12, 12);
            this._icons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _hostLabel
            // 
            this._hostLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._hostLabel.Location = new System.Drawing.Point(3, 6);
            this._hostLabel.Name = "_hostLabel";
            this._hostLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._hostLabel.Size = new System.Drawing.Size(80, 16);
            this._hostLabel.TabIndex = 0;
            this._hostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _hostBox
            // 
            this._hostBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._hostBox.Location = new System.Drawing.Point(89, 5);
            this._hostBox.Name = "_hostBox";
            this._hostBox.Size = new System.Drawing.Size(232, 20);
            this._hostBox.TabIndex = 1;
            this._hostBox.SelectedIndexChanged += new System.EventHandler(this._hostBox_SelectedIndexChanged);
            // 
            // _portLabel
            // 
            this._portLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._portLabel.Location = new System.Drawing.Point(3, 32);
            this._portLabel.Name = "_portLabel";
            this._portLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._portLabel.Size = new System.Drawing.Size(80, 16);
            this._portLabel.TabIndex = 2;
            this._portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _optionsTab
            // 
            this._optionsTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._optionsTab.Controls.Add(this._terminalTabPage);
            this._optionsTab.Controls.Add(this._macroTabPage);
            this._optionsTab.ImageList = this._icons;
            this._optionsTab.ItemSize = new System.Drawing.Size(30, 18);
            this._optionsTab.Location = new System.Drawing.Point(6, 71);
            this._optionsTab.Name = "_optionsTab";
            this._optionsTab.SelectedIndex = 0;
            this._optionsTab.Size = new System.Drawing.Size(312, 189);
            this._optionsTab.TabIndex = 4;
            // 
            // _terminalTabPage
            // 
            this._terminalTabPage.Controls.Add(this._telnetNewLine);
            this._terminalTabPage.Controls.Add(this._logTypeLabel);
            this._terminalTabPage.Controls.Add(this._logTypeBox);
            this._terminalTabPage.Controls.Add(this._logFileLabel);
            this._terminalTabPage.Controls.Add(this._selectLogButton);
            this._terminalTabPage.Controls.Add(this._encodingLabel);
            this._terminalTabPage.Controls.Add(this._encodingBox);
            this._terminalTabPage.Controls.Add(this._localEchoLabel);
            this._terminalTabPage.Controls.Add(this._localEchoBox);
            this._terminalTabPage.Controls.Add(this._logFileBox);
            this._terminalTabPage.Controls.Add(this._newLineLabel);
            this._terminalTabPage.Controls.Add(this._newLineBox);
            this._terminalTabPage.Controls.Add(this._terminalTypeLabel);
            this._terminalTabPage.Controls.Add(this._terminalTypeBox);
            this._terminalTabPage.Location = new System.Drawing.Point(4, 22);
            this._terminalTabPage.Name = "_terminalTabPage";
            this._terminalTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._terminalTabPage.Size = new System.Drawing.Size(304, 163);
            this._terminalTabPage.TabIndex = 0;
            // 
            // _telnetNewLine
            // 
            this._telnetNewLine.Location = new System.Drawing.Point(208, 103);
            this._telnetNewLine.Name = "_telnetNewLine";
            this._telnetNewLine.Size = new System.Drawing.Size(90, 36);
            this._telnetNewLine.TabIndex = 11;
            this._telnetNewLine.UseVisualStyleBackColor = true;
            // 
            // _logTypeLabel
            // 
            this._logTypeLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._logTypeLabel.Location = new System.Drawing.Point(4, 14);
            this._logTypeLabel.Name = "_logTypeLabel";
            this._logTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._logTypeLabel.Size = new System.Drawing.Size(96, 16);
            this._logTypeLabel.TabIndex = 0;
            this._logTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _logTypeBox
            // 
            this._logTypeBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._logTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._logTypeBox.Location = new System.Drawing.Point(108, 14);
            this._logTypeBox.Name = "_logTypeBox";
            this._logTypeBox.Size = new System.Drawing.Size(172, 20);
            this._logTypeBox.TabIndex = 1;
            this._logTypeBox.SelectedIndexChanged += new System.EventHandler(this._logTypeBox_SelectedIndexChanged);
            // 
            // _logFileLabel
            // 
            this._logFileLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._logFileLabel.Location = new System.Drawing.Point(4, 38);
            this._logFileLabel.Name = "_logFileLabel";
            this._logFileLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._logFileLabel.Size = new System.Drawing.Size(88, 16);
            this._logFileLabel.TabIndex = 2;
            this._logFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _selectLogButton
            // 
            this._selectLogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._selectLogButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._selectLogButton.ImageIndex = 0;
            this._selectLogButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._selectLogButton.Location = new System.Drawing.Point(281, 39);
            this._selectLogButton.Name = "_selectLogButton";
            this._selectLogButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._selectLogButton.Size = new System.Drawing.Size(19, 19);
            this._selectLogButton.TabIndex = 4;
            this._selectLogButton.Text = "...";
            this._selectLogButton.Click += new System.EventHandler(this._selectLogButton_Click);
            // 
            // _encodingLabel
            // 
            this._encodingLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._encodingLabel.Location = new System.Drawing.Point(4, 62);
            this._encodingLabel.Name = "_encodingLabel";
            this._encodingLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._encodingLabel.Size = new System.Drawing.Size(96, 16);
            this._encodingLabel.TabIndex = 5;
            this._encodingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _encodingBox
            // 
            this._encodingBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._encodingBox.Location = new System.Drawing.Point(108, 62);
            this._encodingBox.Name = "_encodingBox";
            this._encodingBox.Size = new System.Drawing.Size(96, 20);
            this._encodingBox.TabIndex = 6;
            // 
            // _localEchoLabel
            // 
            this._localEchoLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._localEchoLabel.Location = new System.Drawing.Point(4, 86);
            this._localEchoLabel.Name = "_localEchoLabel";
            this._localEchoLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._localEchoLabel.Size = new System.Drawing.Size(96, 16);
            this._localEchoLabel.TabIndex = 7;
            this._localEchoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _localEchoBox
            // 
            this._localEchoBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._localEchoBox.Location = new System.Drawing.Point(108, 86);
            this._localEchoBox.Name = "_localEchoBox";
            this._localEchoBox.Size = new System.Drawing.Size(96, 20);
            this._localEchoBox.TabIndex = 8;
            // 
            // _logFileBox
            // 
            this._logFileBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._logFileBox.Location = new System.Drawing.Point(108, 38);
            this._logFileBox.Name = "_logFileBox";
            this._logFileBox.Size = new System.Drawing.Size(172, 19);
            this._logFileBox.TabIndex = 3;
            // 
            // _newLineLabel
            // 
            this._newLineLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._newLineLabel.Location = new System.Drawing.Point(4, 110);
            this._newLineLabel.Name = "_newLineLabel";
            this._newLineLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._newLineLabel.Size = new System.Drawing.Size(96, 16);
            this._newLineLabel.TabIndex = 9;
            this._newLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _newLineBox
            // 
            this._newLineBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._newLineBox.Location = new System.Drawing.Point(108, 110);
            this._newLineBox.Name = "_newLineBox";
            this._newLineBox.Size = new System.Drawing.Size(96, 20);
            this._newLineBox.TabIndex = 10;
            this._newLineBox.SelectedIndexChanged += new System.EventHandler(this._newLineBox_SelectedIndexChanged);
            // 
            // _terminalTypeLabel
            // 
            this._terminalTypeLabel.Location = new System.Drawing.Point(4, 134);
            this._terminalTypeLabel.Name = "_terminalTypeLabel";
            this._terminalTypeLabel.Size = new System.Drawing.Size(96, 23);
            this._terminalTypeLabel.TabIndex = 12;
            this._terminalTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _terminalTypeBox
            // 
            this._terminalTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._terminalTypeBox.Location = new System.Drawing.Point(108, 134);
            this._terminalTypeBox.Name = "_terminalTypeBox";
            this._terminalTypeBox.Size = new System.Drawing.Size(96, 20);
            this._terminalTypeBox.TabIndex = 13;
            // 
            // _macroTabPage
            // 
            this._macroTabPage.Controls.Add(this._autoExecMacroPathLabel);
            this._macroTabPage.Controls.Add(this._autoExecMacroPathBox);
            this._macroTabPage.Controls.Add(this._selectAutoExecMacroButton);
            this._macroTabPage.Location = new System.Drawing.Point(4, 22);
            this._macroTabPage.Name = "_macroTabPage";
            this._macroTabPage.Size = new System.Drawing.Size(304, 163);
            this._macroTabPage.TabIndex = 3;
            // 
            // _autoExecMacroPathLabel
            // 
            this._autoExecMacroPathLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._autoExecMacroPathLabel.Location = new System.Drawing.Point(4, 16);
            this._autoExecMacroPathLabel.Name = "_autoExecMacroPathLabel";
            this._autoExecMacroPathLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._autoExecMacroPathLabel.Size = new System.Drawing.Size(100, 16);
            this._autoExecMacroPathLabel.TabIndex = 0;
            this._autoExecMacroPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _autoExecMacroPathBox
            // 
            this._autoExecMacroPathBox.Location = new System.Drawing.Point(108, 15);
            this._autoExecMacroPathBox.Name = "_autoExecMacroPathBox";
            this._autoExecMacroPathBox.Size = new System.Drawing.Size(172, 19);
            this._autoExecMacroPathBox.TabIndex = 1;
            // 
            // _selectAutoExecMacroButton
            // 
            this._selectAutoExecMacroButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._selectAutoExecMacroButton.ImageIndex = 0;
            this._selectAutoExecMacroButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._selectAutoExecMacroButton.Location = new System.Drawing.Point(281, 15);
            this._selectAutoExecMacroButton.Name = "_selectAutoExecMacroButton";
            this._selectAutoExecMacroButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._selectAutoExecMacroButton.Size = new System.Drawing.Size(19, 19);
            this._selectAutoExecMacroButton.TabIndex = 2;
            this._selectAutoExecMacroButton.Text = "...";
            this._selectAutoExecMacroButton.Click += new System.EventHandler(this._selectAutoExecMacroButton_Click);
            // 
            // _portBox
            // 
            this._portBox.Location = new System.Drawing.Point(89, 31);
            this._portBox.Name = "_portBox";
            this._portBox.Size = new System.Drawing.Size(84, 20);
            this._portBox.TabIndex = 3;
            // 
            // OpenSessionTabPageTelnet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._optionsTab);
            this.Controls.Add(this._portBox);
            this.Controls.Add(this._portLabel);
            this.Controls.Add(this._hostBox);
            this.Controls.Add(this._hostLabel);
            this.Name = "OpenSessionTabPageTelnet";
            this.Size = new System.Drawing.Size(324, 263);
            this._optionsTab.ResumeLayout(false);
            this._terminalTabPage.ResumeLayout(false);
            this._terminalTabPage.PerformLayout();
            this._macroTabPage.ResumeLayout(false);
            this._macroTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label _hostLabel;
        private System.Windows.Forms.ComboBox _hostBox;
        private System.Windows.Forms.Label _portLabel;
        private System.Windows.Forms.TabControl _optionsTab;
        private System.Windows.Forms.TabPage _terminalTabPage;
        private System.Windows.Forms.TabPage _macroTabPage;
        private System.Windows.Forms.Label _logTypeLabel;
        private System.Windows.Forms.ComboBox _logTypeBox;
        private System.Windows.Forms.Label _logFileLabel;
        private System.Windows.Forms.Button _selectLogButton;
        private System.Windows.Forms.Label _encodingLabel;
        private System.Windows.Forms.ComboBox _encodingBox;
        private System.Windows.Forms.Label _localEchoLabel;
        private System.Windows.Forms.ComboBox _localEchoBox;
        private System.Windows.Forms.Label _newLineLabel;
        private System.Windows.Forms.ComboBox _newLineBox;
        private System.Windows.Forms.Label _terminalTypeLabel;
        private System.Windows.Forms.ComboBox _terminalTypeBox;
        private System.Windows.Forms.Label _autoExecMacroPathLabel;
        private System.Windows.Forms.TextBox _autoExecMacroPathBox;
        private System.Windows.Forms.Button _selectAutoExecMacroButton;
        private System.Windows.Forms.ToolTip _toolTip;
        private System.Windows.Forms.TextBox _logFileBox;
        private System.Windows.Forms.ComboBox _portBox;
        private System.Windows.Forms.ImageList _icons;
        private System.Windows.Forms.CheckBox _telnetNewLine;
    }
}
