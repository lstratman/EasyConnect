// Copyright 2016 The Poderosa Project.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

namespace Poderosa.Sessions {
    partial class OpenSessionTabPageSSH {
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
            this._ssh2RadioButton = new System.Windows.Forms.RadioButton();
            this._ssh1RadioButton = new System.Windows.Forms.RadioButton();
            this._sshTypePanel = new System.Windows.Forms.Panel();
            this._hostLabel = new System.Windows.Forms.Label();
            this._hostBox = new System.Windows.Forms.ComboBox();
            this._portLabel = new System.Windows.Forms.Label();
            this._usernameLabel = new System.Windows.Forms.Label();
            this._userNameBox = new System.Windows.Forms.ComboBox();
            this._authenticationLabel = new System.Windows.Forms.Label();
            this._authOptions = new System.Windows.Forms.ComboBox();
            this._passphraseLabel = new System.Windows.Forms.Label();
            this._passphraseBox = new System.Windows.Forms.TextBox();
            this._privateKeyLabel = new System.Windows.Forms.Label();
            this._privateKeyFile = new System.Windows.Forms.TextBox();
            this._privateKeySelect = new System.Windows.Forms.Button();
            this._optionsTab = new System.Windows.Forms.TabControl();
            this._terminalTabPage = new System.Windows.Forms.TabPage();
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
            this._x11ForwardingTabPage = new System.Windows.Forms.TabPage();
            this._x11ForwardingOptionsPanel = new System.Windows.Forms.Panel();
            this._x11CygwinX11UnixFolderButton = new System.Windows.Forms.Button();
            this._x11XauthorityButton = new System.Windows.Forms.Button();
            this._x11CygwinX11UnixFolderExampleLabel = new System.Windows.Forms.Label();
            this._x11CygwinX11UnixFolderLabel = new System.Windows.Forms.Label();
            this._x11XauthorityLabel = new System.Windows.Forms.Label();
            this._x11CygwinX11UnixFolderText = new System.Windows.Forms.TextBox();
            this._x11UseCygwinDomainSocketCheckBox = new System.Windows.Forms.CheckBox();
            this._x11XauthorityText = new System.Windows.Forms.TextBox();
            this._x11NeedAuthCheckBox = new System.Windows.Forms.CheckBox();
            this._x11ScreenText = new System.Windows.Forms.TextBox();
            this._x11ScreenLabel = new System.Windows.Forms.Label();
            this._x11DisplayText = new System.Windows.Forms.TextBox();
            this._x11DisplayNote = new System.Windows.Forms.Label();
            this._x11DisplayLabel = new System.Windows.Forms.Label();
            this._useX11ForwardingCheckBox = new System.Windows.Forms.CheckBox();
            this._agentForwardingTabPage = new System.Windows.Forms.TabPage();
            this._agentForwardingConfigButton = new System.Windows.Forms.Button();
            this._useAgentForwardingCheckBox = new System.Windows.Forms.CheckBox();
            this._macroTabPage = new System.Windows.Forms.TabPage();
            this._autoExecMacroPathLabel = new System.Windows.Forms.Label();
            this._autoExecMacroPathBox = new System.Windows.Forms.TextBox();
            this._selectAutoExecMacroButton = new System.Windows.Forms.Button();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._portBox = new System.Windows.Forms.ComboBox();
            this._sshTypePanel.SuspendLayout();
            this._optionsTab.SuspendLayout();
            this._terminalTabPage.SuspendLayout();
            this._x11ForwardingTabPage.SuspendLayout();
            this._x11ForwardingOptionsPanel.SuspendLayout();
            this._agentForwardingTabPage.SuspendLayout();
            this._macroTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _icons
            // 
            this._icons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this._icons.ImageSize = new System.Drawing.Size(12, 12);
            this._icons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _ssh2RadioButton
            // 
            this._ssh2RadioButton.AutoSize = true;
            this._ssh2RadioButton.Checked = true;
            this._ssh2RadioButton.Location = new System.Drawing.Point(0, 0);
            this._ssh2RadioButton.Name = "_ssh2RadioButton";
            this._ssh2RadioButton.Size = new System.Drawing.Size(51, 16);
            this._ssh2RadioButton.TabIndex = 0;
            this._ssh2RadioButton.TabStop = true;
            this._ssh2RadioButton.Text = "SSH2";
            this._ssh2RadioButton.UseVisualStyleBackColor = true;
            this._ssh2RadioButton.CheckedChanged += new System.EventHandler(this._ssh2RadioButton_CheckedChanged);
            // 
            // _ssh1RadioButton
            // 
            this._ssh1RadioButton.AutoSize = true;
            this._ssh1RadioButton.Location = new System.Drawing.Point(70, 0);
            this._ssh1RadioButton.Name = "_ssh1RadioButton";
            this._ssh1RadioButton.Size = new System.Drawing.Size(51, 16);
            this._ssh1RadioButton.TabIndex = 1;
            this._ssh1RadioButton.Text = "SSH1";
            this._ssh1RadioButton.UseVisualStyleBackColor = true;
            this._ssh1RadioButton.CheckedChanged += new System.EventHandler(this._ssh1RadioButton_CheckedChanged);
            // 
            // _sshTypePanel
            // 
            this._sshTypePanel.Controls.Add(this._ssh2RadioButton);
            this._sshTypePanel.Controls.Add(this._ssh1RadioButton);
            this._sshTypePanel.Location = new System.Drawing.Point(89, 3);
            this._sshTypePanel.Name = "_sshTypePanel";
            this._sshTypePanel.Size = new System.Drawing.Size(167, 18);
            this._sshTypePanel.TabIndex = 0;
            // 
            // _hostLabel
            // 
            this._hostLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._hostLabel.Location = new System.Drawing.Point(3, 28);
            this._hostLabel.Name = "_hostLabel";
            this._hostLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._hostLabel.Size = new System.Drawing.Size(80, 16);
            this._hostLabel.TabIndex = 1;
            this._hostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _hostBox
            // 
            this._hostBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._hostBox.Location = new System.Drawing.Point(89, 27);
            this._hostBox.Name = "_hostBox";
            this._hostBox.Size = new System.Drawing.Size(232, 20);
            this._hostBox.TabIndex = 2;
            this._hostBox.SelectedIndexChanged += new System.EventHandler(this._hostBox_SelectedIndexChanged);
            // 
            // _portLabel
            // 
            this._portLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._portLabel.Location = new System.Drawing.Point(3, 54);
            this._portLabel.Name = "_portLabel";
            this._portLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._portLabel.Size = new System.Drawing.Size(80, 16);
            this._portLabel.TabIndex = 3;
            this._portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _usernameLabel
            // 
            this._usernameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._usernameLabel.Location = new System.Drawing.Point(3, 89);
            this._usernameLabel.Name = "_usernameLabel";
            this._usernameLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._usernameLabel.Size = new System.Drawing.Size(80, 16);
            this._usernameLabel.TabIndex = 5;
            this._usernameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _userNameBox
            // 
            this._userNameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._userNameBox.Location = new System.Drawing.Point(89, 88);
            this._userNameBox.Name = "_userNameBox";
            this._userNameBox.Size = new System.Drawing.Size(232, 20);
            this._userNameBox.TabIndex = 6;
            // 
            // _authenticationLabel
            // 
            this._authenticationLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._authenticationLabel.Location = new System.Drawing.Point(3, 113);
            this._authenticationLabel.Name = "_authenticationLabel";
            this._authenticationLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._authenticationLabel.Size = new System.Drawing.Size(80, 16);
            this._authenticationLabel.TabIndex = 7;
            this._authenticationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _authOptions
            // 
            this._authOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._authOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._authOptions.Location = new System.Drawing.Point(89, 112);
            this._authOptions.Name = "_authOptions";
            this._authOptions.Size = new System.Drawing.Size(232, 20);
            this._authOptions.TabIndex = 8;
            this._authOptions.SelectedIndexChanged += new System.EventHandler(this._authOptions_SelectedIndexChanged);
            // 
            // _passphraseLabel
            // 
            this._passphraseLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._passphraseLabel.Location = new System.Drawing.Point(3, 137);
            this._passphraseLabel.Name = "_passphraseLabel";
            this._passphraseLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._passphraseLabel.Size = new System.Drawing.Size(80, 16);
            this._passphraseLabel.TabIndex = 9;
            this._passphraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _passphraseBox
            // 
            this._passphraseBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._passphraseBox.Location = new System.Drawing.Point(89, 136);
            this._passphraseBox.Name = "_passphraseBox";
            this._passphraseBox.PasswordChar = '*';
            this._passphraseBox.Size = new System.Drawing.Size(232, 19);
            this._passphraseBox.TabIndex = 10;
            // 
            // _privateKeyLabel
            // 
            this._privateKeyLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._privateKeyLabel.Location = new System.Drawing.Point(3, 161);
            this._privateKeyLabel.Name = "_privateKeyLabel";
            this._privateKeyLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._privateKeyLabel.Size = new System.Drawing.Size(80, 16);
            this._privateKeyLabel.TabIndex = 11;
            this._privateKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _privateKeyFile
            // 
            this._privateKeyFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._privateKeyFile.Location = new System.Drawing.Point(89, 160);
            this._privateKeyFile.Name = "_privateKeyFile";
            this._privateKeyFile.Size = new System.Drawing.Size(212, 19);
            this._privateKeyFile.TabIndex = 12;
            // 
            // _privateKeySelect
            // 
            this._privateKeySelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._privateKeySelect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._privateKeySelect.ImageIndex = 0;
            this._privateKeySelect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._privateKeySelect.Location = new System.Drawing.Point(301, 160);
            this._privateKeySelect.Name = "_privateKeySelect";
            this._privateKeySelect.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._privateKeySelect.Size = new System.Drawing.Size(20, 19);
            this._privateKeySelect.TabIndex = 13;
            this._privateKeySelect.Text = "...";
            this._privateKeySelect.Click += new System.EventHandler(this._privateKeySelect_Click);
            // 
            // _optionsTab
            // 
            this._optionsTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._optionsTab.Controls.Add(this._terminalTabPage);
            this._optionsTab.Controls.Add(this._x11ForwardingTabPage);
            this._optionsTab.Controls.Add(this._agentForwardingTabPage);
            this._optionsTab.Controls.Add(this._macroTabPage);
            this._optionsTab.ImageList = this._icons;
            this._optionsTab.ItemSize = new System.Drawing.Size(30, 18);
            this._optionsTab.Location = new System.Drawing.Point(6, 188);
            this._optionsTab.Name = "_optionsTab";
            this._optionsTab.SelectedIndex = 0;
            this._optionsTab.Size = new System.Drawing.Size(312, 225);
            this._optionsTab.TabIndex = 14;
            // 
            // _terminalTabPage
            // 
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
            this._terminalTabPage.Size = new System.Drawing.Size(304, 199);
            this._terminalTabPage.TabIndex = 0;
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
            // 
            // _terminalTypeLabel
            // 
            this._terminalTypeLabel.Location = new System.Drawing.Point(4, 134);
            this._terminalTypeLabel.Name = "_terminalTypeLabel";
            this._terminalTypeLabel.Size = new System.Drawing.Size(96, 23);
            this._terminalTypeLabel.TabIndex = 11;
            this._terminalTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _terminalTypeBox
            // 
            this._terminalTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._terminalTypeBox.Location = new System.Drawing.Point(108, 134);
            this._terminalTypeBox.Name = "_terminalTypeBox";
            this._terminalTypeBox.Size = new System.Drawing.Size(96, 20);
            this._terminalTypeBox.TabIndex = 12;
            // 
            // _x11ForwardingTabPage
            // 
            this._x11ForwardingTabPage.Controls.Add(this._x11ForwardingOptionsPanel);
            this._x11ForwardingTabPage.Controls.Add(this._useX11ForwardingCheckBox);
            this._x11ForwardingTabPage.Location = new System.Drawing.Point(4, 22);
            this._x11ForwardingTabPage.Name = "_x11ForwardingTabPage";
            this._x11ForwardingTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._x11ForwardingTabPage.Size = new System.Drawing.Size(304, 199);
            this._x11ForwardingTabPage.TabIndex = 1;
            // 
            // _x11ForwardingOptionsPanel
            // 
            this._x11ForwardingOptionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11CygwinX11UnixFolderButton);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11XauthorityButton);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11CygwinX11UnixFolderExampleLabel);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11CygwinX11UnixFolderLabel);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11XauthorityLabel);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11CygwinX11UnixFolderText);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11UseCygwinDomainSocketCheckBox);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11XauthorityText);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11NeedAuthCheckBox);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11ScreenText);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11ScreenLabel);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11DisplayText);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11DisplayNote);
            this._x11ForwardingOptionsPanel.Controls.Add(this._x11DisplayLabel);
            this._x11ForwardingOptionsPanel.Location = new System.Drawing.Point(0, 26);
            this._x11ForwardingOptionsPanel.Name = "_x11ForwardingOptionsPanel";
            this._x11ForwardingOptionsPanel.Size = new System.Drawing.Size(304, 173);
            this._x11ForwardingOptionsPanel.TabIndex = 1;
            // 
            // _x11CygwinX11UnixFolderButton
            // 
            this._x11CygwinX11UnixFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._x11CygwinX11UnixFolderButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._x11CygwinX11UnixFolderButton.ImageIndex = 0;
            this._x11CygwinX11UnixFolderButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._x11CygwinX11UnixFolderButton.Location = new System.Drawing.Point(280, 131);
            this._x11CygwinX11UnixFolderButton.Name = "_x11CygwinX11UnixFolderButton";
            this._x11CygwinX11UnixFolderButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._x11CygwinX11UnixFolderButton.Size = new System.Drawing.Size(20, 19);
            this._x11CygwinX11UnixFolderButton.TabIndex = 12;
            this._x11CygwinX11UnixFolderButton.Text = "...";
            this._x11CygwinX11UnixFolderButton.Click += new System.EventHandler(this._x11CygwinX11UnixFolderButton_Click);
            // 
            // _x11XauthorityButton
            // 
            this._x11XauthorityButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._x11XauthorityButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._x11XauthorityButton.ImageIndex = 0;
            this._x11XauthorityButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._x11XauthorityButton.Location = new System.Drawing.Point(280, 80);
            this._x11XauthorityButton.Name = "_x11XauthorityButton";
            this._x11XauthorityButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._x11XauthorityButton.Size = new System.Drawing.Size(20, 19);
            this._x11XauthorityButton.TabIndex = 8;
            this._x11XauthorityButton.Text = "...";
            this._x11XauthorityButton.Click += new System.EventHandler(this._x11XauthorityButton_Click);
            // 
            // _x11CygwinX11UnixFolderExampleLabel
            // 
            this._x11CygwinX11UnixFolderExampleLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._x11CygwinX11UnixFolderExampleLabel.Location = new System.Drawing.Point(108, 153);
            this._x11CygwinX11UnixFolderExampleLabel.Name = "_x11CygwinX11UnixFolderExampleLabel";
            this._x11CygwinX11UnixFolderExampleLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._x11CygwinX11UnixFolderExampleLabel.Size = new System.Drawing.Size(170, 14);
            this._x11CygwinX11UnixFolderExampleLabel.TabIndex = 13;
            this._x11CygwinX11UnixFolderExampleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _x11CygwinX11UnixFolderLabel
            // 
            this._x11CygwinX11UnixFolderLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._x11CygwinX11UnixFolderLabel.Location = new System.Drawing.Point(5, 132);
            this._x11CygwinX11UnixFolderLabel.Name = "_x11CygwinX11UnixFolderLabel";
            this._x11CygwinX11UnixFolderLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._x11CygwinX11UnixFolderLabel.Size = new System.Drawing.Size(100, 16);
            this._x11CygwinX11UnixFolderLabel.TabIndex = 10;
            this._x11CygwinX11UnixFolderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _x11XauthorityLabel
            // 
            this._x11XauthorityLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._x11XauthorityLabel.Location = new System.Drawing.Point(5, 81);
            this._x11XauthorityLabel.Name = "_x11XauthorityLabel";
            this._x11XauthorityLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._x11XauthorityLabel.Size = new System.Drawing.Size(100, 16);
            this._x11XauthorityLabel.TabIndex = 6;
            this._x11XauthorityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _x11CygwinX11UnixFolderText
            // 
            this._x11CygwinX11UnixFolderText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._x11CygwinX11UnixFolderText.Location = new System.Drawing.Point(110, 131);
            this._x11CygwinX11UnixFolderText.Name = "_x11CygwinX11UnixFolderText";
            this._x11CygwinX11UnixFolderText.Size = new System.Drawing.Size(168, 19);
            this._x11CygwinX11UnixFolderText.TabIndex = 11;
            // 
            // _x11UseCygwinDomainSocketCheckBox
            // 
            this._x11UseCygwinDomainSocketCheckBox.AutoSize = true;
            this._x11UseCygwinDomainSocketCheckBox.Location = new System.Drawing.Point(3, 109);
            this._x11UseCygwinDomainSocketCheckBox.Name = "_x11UseCygwinDomainSocketCheckBox";
            this._x11UseCygwinDomainSocketCheckBox.Size = new System.Drawing.Size(15, 14);
            this._x11UseCygwinDomainSocketCheckBox.TabIndex = 9;
            this._x11UseCygwinDomainSocketCheckBox.UseVisualStyleBackColor = true;
            this._x11UseCygwinDomainSocketCheckBox.CheckedChanged += new System.EventHandler(this._x11UseCygwinDomainSocketCheckBox_CheckedChanged);
            // 
            // _x11XauthorityText
            // 
            this._x11XauthorityText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._x11XauthorityText.Location = new System.Drawing.Point(110, 80);
            this._x11XauthorityText.Name = "_x11XauthorityText";
            this._x11XauthorityText.Size = new System.Drawing.Size(168, 19);
            this._x11XauthorityText.TabIndex = 7;
            // 
            // _x11NeedAuthCheckBox
            // 
            this._x11NeedAuthCheckBox.AutoSize = true;
            this._x11NeedAuthCheckBox.Location = new System.Drawing.Point(3, 58);
            this._x11NeedAuthCheckBox.Name = "_x11NeedAuthCheckBox";
            this._x11NeedAuthCheckBox.Size = new System.Drawing.Size(15, 14);
            this._x11NeedAuthCheckBox.TabIndex = 5;
            this._x11NeedAuthCheckBox.UseVisualStyleBackColor = true;
            this._x11NeedAuthCheckBox.CheckedChanged += new System.EventHandler(this._x11NeedAuthCheckBox_CheckedChanged);
            // 
            // _x11ScreenText
            // 
            this._x11ScreenText.Location = new System.Drawing.Point(110, 28);
            this._x11ScreenText.Name = "_x11ScreenText";
            this._x11ScreenText.Size = new System.Drawing.Size(60, 19);
            this._x11ScreenText.TabIndex = 4;
            // 
            // _x11ScreenLabel
            // 
            this._x11ScreenLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._x11ScreenLabel.Location = new System.Drawing.Point(5, 29);
            this._x11ScreenLabel.Name = "_x11ScreenLabel";
            this._x11ScreenLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._x11ScreenLabel.Size = new System.Drawing.Size(100, 16);
            this._x11ScreenLabel.TabIndex = 3;
            this._x11ScreenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _x11DisplayText
            // 
            this._x11DisplayText.Location = new System.Drawing.Point(110, 3);
            this._x11DisplayText.Name = "_x11DisplayText";
            this._x11DisplayText.Size = new System.Drawing.Size(60, 19);
            this._x11DisplayText.TabIndex = 1;
            // 
            // _x11DisplayNote
            // 
            this._x11DisplayNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._x11DisplayNote.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._x11DisplayNote.Location = new System.Drawing.Point(178, 4);
            this._x11DisplayNote.Name = "_x11DisplayNote";
            this._x11DisplayNote.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._x11DisplayNote.Size = new System.Drawing.Size(120, 18);
            this._x11DisplayNote.TabIndex = 2;
            this._x11DisplayNote.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _x11DisplayLabel
            // 
            this._x11DisplayLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._x11DisplayLabel.Location = new System.Drawing.Point(5, 4);
            this._x11DisplayLabel.Name = "_x11DisplayLabel";
            this._x11DisplayLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._x11DisplayLabel.Size = new System.Drawing.Size(100, 18);
            this._x11DisplayLabel.TabIndex = 0;
            this._x11DisplayLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _useX11ForwardingCheckBox
            // 
            this._useX11ForwardingCheckBox.AutoSize = true;
            this._useX11ForwardingCheckBox.Location = new System.Drawing.Point(3, 6);
            this._useX11ForwardingCheckBox.Name = "_useX11ForwardingCheckBox";
            this._useX11ForwardingCheckBox.Size = new System.Drawing.Size(15, 14);
            this._useX11ForwardingCheckBox.TabIndex = 0;
            this._useX11ForwardingCheckBox.UseVisualStyleBackColor = true;
            this._useX11ForwardingCheckBox.CheckedChanged += new System.EventHandler(this._useX11ForwardingCheckBox_CheckedChanged);
            // 
            // _agentForwardingTabPage
            // 
            this._agentForwardingTabPage.Controls.Add(this._agentForwardingConfigButton);
            this._agentForwardingTabPage.Controls.Add(this._useAgentForwardingCheckBox);
            this._agentForwardingTabPage.Location = new System.Drawing.Point(4, 22);
            this._agentForwardingTabPage.Name = "_agentForwardingTabPage";
            this._agentForwardingTabPage.Size = new System.Drawing.Size(304, 199);
            this._agentForwardingTabPage.TabIndex = 2;
            // 
            // _agentForwardingConfigButton
            // 
            this._agentForwardingConfigButton.Location = new System.Drawing.Point(13, 44);
            this._agentForwardingConfigButton.Name = "_agentForwardingConfigButton";
            this._agentForwardingConfigButton.Size = new System.Drawing.Size(132, 23);
            this._agentForwardingConfigButton.TabIndex = 2;
            this._agentForwardingConfigButton.UseVisualStyleBackColor = true;
            this._agentForwardingConfigButton.Click += new System.EventHandler(this._agentForwardingConfigButton_Click);
            // 
            // _useAgentForwardingCheckBox
            // 
            this._useAgentForwardingCheckBox.AutoSize = true;
            this._useAgentForwardingCheckBox.Location = new System.Drawing.Point(3, 14);
            this._useAgentForwardingCheckBox.Name = "_useAgentForwardingCheckBox";
            this._useAgentForwardingCheckBox.Size = new System.Drawing.Size(15, 14);
            this._useAgentForwardingCheckBox.TabIndex = 1;
            this._useAgentForwardingCheckBox.UseVisualStyleBackColor = true;
            this._useAgentForwardingCheckBox.CheckedChanged += new System.EventHandler(this._useAgentForwardingCheckBox_CheckedChanged);
            // 
            // _macroTabPage
            // 
            this._macroTabPage.Controls.Add(this._autoExecMacroPathLabel);
            this._macroTabPage.Controls.Add(this._autoExecMacroPathBox);
            this._macroTabPage.Controls.Add(this._selectAutoExecMacroButton);
            this._macroTabPage.Location = new System.Drawing.Point(4, 22);
            this._macroTabPage.Name = "_macroTabPage";
            this._macroTabPage.Size = new System.Drawing.Size(304, 199);
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
            this._portBox.Location = new System.Drawing.Point(89, 53);
            this._portBox.Name = "_portBox";
            this._portBox.Size = new System.Drawing.Size(84, 20);
            this._portBox.TabIndex = 4;
            // 
            // OpenSessionTabPageSSH
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._optionsTab);
            this.Controls.Add(this._usernameLabel);
            this.Controls.Add(this._portBox);
            this.Controls.Add(this._userNameBox);
            this.Controls.Add(this._authenticationLabel);
            this.Controls.Add(this._authOptions);
            this.Controls.Add(this._passphraseLabel);
            this.Controls.Add(this._passphraseBox);
            this.Controls.Add(this._privateKeyLabel);
            this.Controls.Add(this._privateKeyFile);
            this.Controls.Add(this._privateKeySelect);
            this.Controls.Add(this._portLabel);
            this.Controls.Add(this._hostBox);
            this.Controls.Add(this._hostLabel);
            this.Controls.Add(this._sshTypePanel);
            this.Name = "OpenSessionTabPageSSH";
            this.Size = new System.Drawing.Size(324, 416);
            this._sshTypePanel.ResumeLayout(false);
            this._sshTypePanel.PerformLayout();
            this._optionsTab.ResumeLayout(false);
            this._terminalTabPage.ResumeLayout(false);
            this._terminalTabPage.PerformLayout();
            this._x11ForwardingTabPage.ResumeLayout(false);
            this._x11ForwardingTabPage.PerformLayout();
            this._x11ForwardingOptionsPanel.ResumeLayout(false);
            this._x11ForwardingOptionsPanel.PerformLayout();
            this._agentForwardingTabPage.ResumeLayout(false);
            this._agentForwardingTabPage.PerformLayout();
            this._macroTabPage.ResumeLayout(false);
            this._macroTabPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton _ssh2RadioButton;
        private System.Windows.Forms.RadioButton _ssh1RadioButton;
        private System.Windows.Forms.Panel _sshTypePanel;
        private System.Windows.Forms.Label _hostLabel;
        private System.Windows.Forms.ComboBox _hostBox;
        private System.Windows.Forms.Label _portLabel;
        private System.Windows.Forms.Label _usernameLabel;
        private System.Windows.Forms.ComboBox _userNameBox;
        private System.Windows.Forms.Label _authenticationLabel;
        private System.Windows.Forms.ComboBox _authOptions;
        private System.Windows.Forms.Label _passphraseLabel;
        private System.Windows.Forms.TextBox _passphraseBox;
        private System.Windows.Forms.Label _privateKeyLabel;
        private System.Windows.Forms.TextBox _privateKeyFile;
        private System.Windows.Forms.Button _privateKeySelect;
        private System.Windows.Forms.TabControl _optionsTab;
        private System.Windows.Forms.TabPage _terminalTabPage;
        private System.Windows.Forms.TabPage _x11ForwardingTabPage;
        private System.Windows.Forms.TabPage _agentForwardingTabPage;
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
        private System.Windows.Forms.CheckBox _useX11ForwardingCheckBox;
        private System.Windows.Forms.Panel _x11ForwardingOptionsPanel;
        private System.Windows.Forms.TextBox _x11DisplayText;
        private System.Windows.Forms.Label _x11DisplayLabel;
        private System.Windows.Forms.TextBox _x11ScreenText;
        private System.Windows.Forms.Label _x11ScreenLabel;
        private System.Windows.Forms.CheckBox _x11NeedAuthCheckBox;
        private System.Windows.Forms.Label _x11XauthorityLabel;
        private System.Windows.Forms.TextBox _x11XauthorityText;
        private System.Windows.Forms.Button _x11XauthorityButton;
        private System.Windows.Forms.Button _x11CygwinX11UnixFolderButton;
        private System.Windows.Forms.Label _x11CygwinX11UnixFolderLabel;
        private System.Windows.Forms.TextBox _x11CygwinX11UnixFolderText;
        private System.Windows.Forms.CheckBox _x11UseCygwinDomainSocketCheckBox;
        private System.Windows.Forms.CheckBox _useAgentForwardingCheckBox;
        private System.Windows.Forms.Button _agentForwardingConfigButton;
        private System.Windows.Forms.Label _autoExecMacroPathLabel;
        private System.Windows.Forms.TextBox _autoExecMacroPathBox;
        private System.Windows.Forms.Button _selectAutoExecMacroButton;
        private System.Windows.Forms.ToolTip _toolTip;
        private System.Windows.Forms.TextBox _logFileBox;
        private System.Windows.Forms.ComboBox _portBox;
        private System.Windows.Forms.Label _x11CygwinX11UnixFolderExampleLabel;
        private System.Windows.Forms.ImageList _icons;
        private System.Windows.Forms.Label _x11DisplayNote;
    }
}
