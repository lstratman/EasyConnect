/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SSHShortcutLoginDialog.cs,v 1.11 2012/03/18 10:46:40 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

using Poderosa.Util;
using Poderosa.Terminal;
using Poderosa.ConnectionParam;
using Poderosa.Protocols;
using Poderosa.Forms;
using Poderosa.View;

using Granados;

namespace Poderosa.Sessions {
    /// <summary>
    /// SSHShortcutLoginDialog の概要の説明です。
    /// </summary>
    internal class SSHShortcutLoginDialog : LoginDialogBase {

        private ISSHLoginParameter _sshParam;

        private TextBox _privateKeyBox;
        private TextBox _passphraseBox;
        private Button _privateKeySelect;
        private Label _hostLabel;
        private Label _hostBox;
        private Label _methodLabel;
        private Label _methodBox;
        private Label _accountLabel;
        private Label _accountBox;
        private Label _authenticationTypeLabel;
        private Label _authenticationTypeBox;
        private Label _encodingLabel;
        private Label _encodingBox;
        private ComboBox _logFileBox;
        private Button _selectlogButton;
        private Label _privateKeyLabel;
        private Label _passphraseLabel;
        private Label _logFileLabel;
        private ComboBox _logTypeBox;
        private Label _logTypeLabel;
        private Label _autoExecMacroPathLabel;
        private TextBox _autoExecMacroPathBox;
        private Button _selectAutoExecMacroButton;
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.Container components = null;

        public SSHShortcutLoginDialog(IPoderosaMainWindow parent, ISSHLoginParameter param, ITerminalSettings settings)
            : base(parent) {
            this.TerminalSettings = settings;

            //
            // Windows フォーム デザイナ サポートに必要です。
            //
            InitializeComponent();

            this._privateKeyLabel.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog._privateKeyLabel");
            this._passphraseLabel.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog._passphraseLabel");
            this._logFileLabel.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog._logFileLabel");
            this._hostLabel.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog._hostLabel");
            this._methodLabel.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog._methodLabel");
            this._accountLabel.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog._accountLabel");
            this._authenticationTypeLabel.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog._authenticationTypeLabel");
            this._encodingLabel.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog._encodingLabel");
            this._logTypeLabel.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog._logTypeLabel");
            this.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog.Text");
            this._autoExecMacroPathLabel.Text = TEnv.Strings.GetString("Form.SSHShortcutLoginDialog._autoExecMacroPathLabel");
            this._cancelButton.Text = TEnv.Strings.GetString("Common.Cancel");
            this._loginButton.Text = TEnv.Strings.GetString("Common.OK");

            this._logTypeBox.Items.AddRange(EnumListItem<LogType>.GetListItems());

            _sshParam = param;
            InitUI();
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
            this._privateKeyBox = new System.Windows.Forms.TextBox();
            this._privateKeyLabel = new System.Windows.Forms.Label();
            this._passphraseBox = new System.Windows.Forms.TextBox();
            this._passphraseLabel = new System.Windows.Forms.Label();
            this._privateKeySelect = new System.Windows.Forms.Button();
            this._logFileBox = new System.Windows.Forms.ComboBox();
            this._logFileLabel = new System.Windows.Forms.Label();
            this._selectlogButton = new System.Windows.Forms.Button();
            this._hostLabel = new System.Windows.Forms.Label();
            this._hostBox = new System.Windows.Forms.Label();
            this._methodLabel = new System.Windows.Forms.Label();
            this._methodBox = new System.Windows.Forms.Label();
            this._accountLabel = new System.Windows.Forms.Label();
            this._accountBox = new System.Windows.Forms.Label();
            this._authenticationTypeLabel = new System.Windows.Forms.Label();
            this._authenticationTypeBox = new System.Windows.Forms.Label();
            this._encodingLabel = new System.Windows.Forms.Label();
            this._encodingBox = new System.Windows.Forms.Label();
            this._logTypeBox = new System.Windows.Forms.ComboBox();
            this._logTypeLabel = new System.Windows.Forms.Label();
            this._autoExecMacroPathLabel = new System.Windows.Forms.Label();
            this._autoExecMacroPathBox = new System.Windows.Forms.TextBox();
            this._selectAutoExecMacroButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _loginButton
            // 
            this._loginButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._loginButton.ImageIndex = 0;
            this._loginButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._loginButton.Location = new System.Drawing.Point(123, 229);
            this._loginButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._loginButton.Size = new System.Drawing.Size(72, 25);
            this._loginButton.TabIndex = 13;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.ImageIndex = 0;
            this._cancelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._cancelButton.Location = new System.Drawing.Point(211, 229);
            this._cancelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._cancelButton.Size = new System.Drawing.Size(72, 25);
            this._cancelButton.TabIndex = 14;
            // 
            // _privateKeyBox
            // 
            this._privateKeyBox.Location = new System.Drawing.Point(110, 128);
            this._privateKeyBox.Name = "_privateKeyBox";
            this._privateKeyBox.Size = new System.Drawing.Size(154, 19);
            this._privateKeyBox.TabIndex = 3;
            // 
            // _privateKeyLabel
            // 
            this._privateKeyLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._privateKeyLabel.Location = new System.Drawing.Point(8, 129);
            this._privateKeyLabel.Name = "_privateKeyLabel";
            this._privateKeyLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._privateKeyLabel.Size = new System.Drawing.Size(96, 18);
            this._privateKeyLabel.TabIndex = 2;
            this._privateKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _passphraseBox
            // 
            this._passphraseBox.Location = new System.Drawing.Point(110, 104);
            this._passphraseBox.Name = "_passphraseBox";
            this._passphraseBox.PasswordChar = '*';
            this._passphraseBox.Size = new System.Drawing.Size(178, 19);
            this._passphraseBox.TabIndex = 1;
            // 
            // _passphraseLabel
            // 
            this._passphraseLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._passphraseLabel.Location = new System.Drawing.Point(8, 105);
            this._passphraseLabel.Name = "_passphraseLabel";
            this._passphraseLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._passphraseLabel.Size = new System.Drawing.Size(96, 18);
            this._passphraseLabel.TabIndex = 0;
            this._passphraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _privateKeySelect
            // 
            this._privateKeySelect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._privateKeySelect.ImageIndex = 0;
            this._privateKeySelect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._privateKeySelect.Location = new System.Drawing.Point(272, 128);
            this._privateKeySelect.Name = "_privateKeySelect";
            this._privateKeySelect.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._privateKeySelect.Size = new System.Drawing.Size(19, 19);
            this._privateKeySelect.TabIndex = 4;
            this._privateKeySelect.Text = "...";
            this._privateKeySelect.Click += new System.EventHandler(this.OnOpenPrivateKey);
            // 
            // _logFileBox
            // 
            this._logFileBox.Location = new System.Drawing.Point(110, 176);
            this._logFileBox.Name = "_logFileBox";
            this._logFileBox.Size = new System.Drawing.Size(154, 20);
            this._logFileBox.TabIndex = 8;
            // 
            // _logFileLabel
            // 
            this._logFileLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._logFileLabel.Location = new System.Drawing.Point(8, 177);
            this._logFileLabel.Name = "_logFileLabel";
            this._logFileLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._logFileLabel.Size = new System.Drawing.Size(96, 18);
            this._logFileLabel.TabIndex = 7;
            this._logFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _selectlogButton
            // 
            this._selectlogButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._selectlogButton.ImageIndex = 0;
            this._selectlogButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._selectlogButton.Location = new System.Drawing.Point(272, 176);
            this._selectlogButton.Name = "_selectlogButton";
            this._selectlogButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._selectlogButton.Size = new System.Drawing.Size(19, 19);
            this._selectlogButton.TabIndex = 9;
            this._selectlogButton.Text = "...";
            this._selectlogButton.Click += new System.EventHandler(this.SelectLog);
            // 
            // _hostLabel
            // 
            this._hostLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._hostLabel.Location = new System.Drawing.Point(8, 8);
            this._hostLabel.Name = "_hostLabel";
            this._hostLabel.Size = new System.Drawing.Size(94, 16);
            this._hostLabel.TabIndex = 0;
            this._hostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _hostBox
            // 
            this._hostBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._hostBox.Location = new System.Drawing.Point(108, 8);
            this._hostBox.Name = "_hostBox";
            this._hostBox.Size = new System.Drawing.Size(144, 16);
            this._hostBox.TabIndex = 0;
            this._hostBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _methodLabel
            // 
            this._methodLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._methodLabel.Location = new System.Drawing.Point(8, 24);
            this._methodLabel.Name = "_methodLabel";
            this._methodLabel.Size = new System.Drawing.Size(94, 16);
            this._methodLabel.TabIndex = 0;
            this._methodLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _methodBox
            // 
            this._methodBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._methodBox.Location = new System.Drawing.Point(108, 24);
            this._methodBox.Name = "_methodBox";
            this._methodBox.Size = new System.Drawing.Size(144, 16);
            this._methodBox.TabIndex = 0;
            this._methodBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _accountLabel
            // 
            this._accountLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._accountLabel.Location = new System.Drawing.Point(8, 40);
            this._accountLabel.Name = "_accountLabel";
            this._accountLabel.Size = new System.Drawing.Size(94, 16);
            this._accountLabel.TabIndex = 0;
            this._accountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _accountBox
            // 
            this._accountBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._accountBox.Location = new System.Drawing.Point(108, 40);
            this._accountBox.Name = "_accountBox";
            this._accountBox.Size = new System.Drawing.Size(144, 16);
            this._accountBox.TabIndex = 0;
            this._accountBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _authenticationTypeLabel
            // 
            this._authenticationTypeLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._authenticationTypeLabel.Location = new System.Drawing.Point(8, 56);
            this._authenticationTypeLabel.Name = "_authenticationTypeLabel";
            this._authenticationTypeLabel.Size = new System.Drawing.Size(94, 16);
            this._authenticationTypeLabel.TabIndex = 0;
            this._authenticationTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _authenticationTypeBox
            // 
            this._authenticationTypeBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._authenticationTypeBox.Location = new System.Drawing.Point(108, 56);
            this._authenticationTypeBox.Name = "_authenticationTypeBox";
            this._authenticationTypeBox.Size = new System.Drawing.Size(144, 16);
            this._authenticationTypeBox.TabIndex = 0;
            this._authenticationTypeBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _encodingLabel
            // 
            this._encodingLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._encodingLabel.Location = new System.Drawing.Point(8, 72);
            this._encodingLabel.Name = "_encodingLabel";
            this._encodingLabel.Size = new System.Drawing.Size(94, 16);
            this._encodingLabel.TabIndex = 0;
            this._encodingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _encodingBox
            // 
            this._encodingBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._encodingBox.Location = new System.Drawing.Point(108, 72);
            this._encodingBox.Name = "_encodingBox";
            this._encodingBox.Size = new System.Drawing.Size(144, 16);
            this._encodingBox.TabIndex = 0;
            this._encodingBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _logTypeBox
            // 
            this._logTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._logTypeBox.Location = new System.Drawing.Point(110, 151);
            this._logTypeBox.Name = "_logTypeBox";
            this._logTypeBox.Size = new System.Drawing.Size(154, 20);
            this._logTypeBox.TabIndex = 6;
            this._logTypeBox.SelectionChangeCommitted += new System.EventHandler(this.OnLogTypeChanged);
            // 
            // _logTypeLabel
            // 
            this._logTypeLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._logTypeLabel.Location = new System.Drawing.Point(8, 152);
            this._logTypeLabel.Name = "_logTypeLabel";
            this._logTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._logTypeLabel.Size = new System.Drawing.Size(96, 19);
            this._logTypeLabel.TabIndex = 5;
            this._logTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _autoExecMacroPathLabel
            // 
            this._autoExecMacroPathLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._autoExecMacroPathLabel.Location = new System.Drawing.Point(8, 202);
            this._autoExecMacroPathLabel.Name = "_autoExecMacroPathLabel";
            this._autoExecMacroPathLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._autoExecMacroPathLabel.Size = new System.Drawing.Size(96, 18);
            this._autoExecMacroPathLabel.TabIndex = 10;
            this._autoExecMacroPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _autoExecMacroPathBox
            // 
            this._autoExecMacroPathBox.Location = new System.Drawing.Point(110, 202);
            this._autoExecMacroPathBox.Name = "_autoExecMacroPathBox";
            this._autoExecMacroPathBox.Size = new System.Drawing.Size(154, 19);
            this._autoExecMacroPathBox.TabIndex = 11;
            // 
            // _selectAutoExecMacroButton
            // 
            this._selectAutoExecMacroButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._selectAutoExecMacroButton.ImageIndex = 0;
            this._selectAutoExecMacroButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._selectAutoExecMacroButton.Location = new System.Drawing.Point(272, 202);
            this._selectAutoExecMacroButton.Name = "_selectAutoExecMacroButton";
            this._selectAutoExecMacroButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._selectAutoExecMacroButton.Size = new System.Drawing.Size(19, 19);
            this._selectAutoExecMacroButton.TabIndex = 12;
            this._selectAutoExecMacroButton.Text = "...";
            this._selectAutoExecMacroButton.Click += new System.EventHandler(this._selectAutoExecMacroButton_Click);
            // 
            // SSHShortcutLoginDialog
            // 
            this.AcceptButton = this._loginButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(298, 265);
            this.Controls.Add(this._selectAutoExecMacroButton);
            this.Controls.Add(this._autoExecMacroPathBox);
            this.Controls.Add(this._autoExecMacroPathLabel);
            this.Controls.Add(this._logTypeBox);
            this.Controls.Add(this._logTypeLabel);
            this.Controls.Add(this._logFileBox);
            this.Controls.Add(this._logFileLabel);
            this.Controls.Add(this._selectlogButton);
            this.Controls.Add(this._hostLabel);
            this.Controls.Add(this._hostBox);
            this.Controls.Add(this._methodLabel);
            this.Controls.Add(this._methodBox);
            this.Controls.Add(this._accountLabel);
            this.Controls.Add(this._accountBox);
            this.Controls.Add(this._authenticationTypeLabel);
            this.Controls.Add(this._authenticationTypeBox);
            this.Controls.Add(this._encodingLabel);
            this.Controls.Add(this._encodingBox);
            this.Controls.Add(this._privateKeyBox);
            this.Controls.Add(this._privateKeyLabel);
            this.Controls.Add(this._passphraseBox);
            this.Controls.Add(this._passphraseLabel);
            this.Controls.Add(this._privateKeySelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SSHShortcutLoginDialog";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this._privateKeySelect, 0);
            this.Controls.SetChildIndex(this._passphraseLabel, 0);
            this.Controls.SetChildIndex(this._passphraseBox, 0);
            this.Controls.SetChildIndex(this._privateKeyLabel, 0);
            this.Controls.SetChildIndex(this._privateKeyBox, 0);
            this.Controls.SetChildIndex(this._encodingBox, 0);
            this.Controls.SetChildIndex(this._encodingLabel, 0);
            this.Controls.SetChildIndex(this._authenticationTypeBox, 0);
            this.Controls.SetChildIndex(this._authenticationTypeLabel, 0);
            this.Controls.SetChildIndex(this._accountBox, 0);
            this.Controls.SetChildIndex(this._accountLabel, 0);
            this.Controls.SetChildIndex(this._methodBox, 0);
            this.Controls.SetChildIndex(this._methodLabel, 0);
            this.Controls.SetChildIndex(this._hostBox, 0);
            this.Controls.SetChildIndex(this._hostLabel, 0);
            this.Controls.SetChildIndex(this._selectlogButton, 0);
            this.Controls.SetChildIndex(this._logFileLabel, 0);
            this.Controls.SetChildIndex(this._logFileBox, 0);
            this.Controls.SetChildIndex(this._logTypeLabel, 0);
            this.Controls.SetChildIndex(this._logTypeBox, 0);
            this.Controls.SetChildIndex(this._loginButton, 0);
            this.Controls.SetChildIndex(this._cancelButton, 0);
            this.Controls.SetChildIndex(this._autoExecMacroPathLabel, 0);
            this.Controls.SetChildIndex(this._autoExecMacroPathBox, 0);
            this.Controls.SetChildIndex(this._selectAutoExecMacroButton, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void InitUI() {
            ITCPParameter tcp = (ITCPParameter)_sshParam.GetAdapter(typeof(ITCPParameter));
            _hostBox.Text = tcp.Destination;
            _methodBox.Text = _sshParam.Method.ToString();
            //if(_sshParam.Port!=22) _methodBox.Text += String.Format(TEnv.Strings.GetString("Caption.SSHShortcutLoginDialog.NotStandardPort"), _sshParam.Port);
            _accountBox.Text = _sshParam.Account;
            _authenticationTypeBox.Text = _sshParam.AuthenticationType.ToString(); //さぼり
            _encodingBox.Text = EnumListItem<EncodingType>.CreateListItem(this.TerminalSettings.Encoding).Text;
            _logTypeBox.SelectedItem = LogType.None;    // select EnumListItem<T> by T

            if (_sshParam.AuthenticationType == AuthenticationType.Password) {
                _privateKeyBox.Enabled = false;
                _privateKeySelect.Enabled = false;
            }
            else if (_sshParam.AuthenticationType == AuthenticationType.PublicKey) {
                _privateKeyBox.Text = _sshParam.IdentityFileName;
            }
            else if (_sshParam.AuthenticationType == AuthenticationType.KeyboardInteractive) {
                _privateKeyBox.Enabled = false;
                _privateKeySelect.Enabled = false;
                _passphraseBox.Enabled = false;
            }

            _passphraseBox.Text = "";
            if (_sshParam.PasswordOrPassphrase.Length == 0 && TerminalSessionsPlugin.Instance.ProtocolService.ProtocolOptions.RetainsPassphrase) {
                string p = TerminalSessionsPlugin.Instance.ProtocolService.PassphraseCache.GetOrEmpty(tcp.Destination, _sshParam.Account);
                _passphraseBox.Text = p;
            }

            IAutoExecMacroParameter autoExecParams = _sshParam.GetAdapter(typeof(IAutoExecMacroParameter)) as IAutoExecMacroParameter;
            if (autoExecParams != null && TelnetSSHPlugin.Instance.MacroEngine != null) {
                _autoExecMacroPathBox.Text = (autoExecParams.AutoExecMacroPath != null) ? autoExecParams.AutoExecMacroPath : String.Empty;
            }
            else {
                _autoExecMacroPathLabel.Enabled = false;
                _autoExecMacroPathBox.Enabled = false;
                _selectAutoExecMacroButton.Enabled = false;
            }

            AdjustUI();
        }
        private void AdjustUI() {
            _passphraseBox.Enabled = _sshParam.AuthenticationType != AuthenticationType.KeyboardInteractive;

            bool e = ((EnumListItem<LogType>)_logTypeBox.SelectedItem).Value != LogType.None;
            _logFileBox.Enabled = e;
            _selectlogButton.Enabled = e;
        }

        //入力内容に誤りがあればそれを警告してnullを返す。なければ必要なところを埋めたTCPTerminalParamを返す
        private ISSHLoginParameter ValidateContent() {
            ISSHLoginParameter p = (ISSHLoginParameter)_sshParam.Clone();
            string msg = null;

            try {
                LogType logtype = ((EnumListItem<LogType>)_logTypeBox.SelectedItem).Value;
                ISimpleLogSettings logsettings = null;
                if (logtype != LogType.None) {
                    logsettings = CreateSimpleLogSettings(logtype, _logFileBox.Text);
                    if (logsettings == null)
                        return null; //動作キャンセル
                }

                ITerminalSettings settings = this.TerminalSettings;

                if (logsettings != null) {
                    settings.BeginUpdate();
                    settings.LogSettings.Reset(logsettings);
                    settings.EndUpdate();
                }

                ITerminalParameter param = (ITerminalParameter)p.GetAdapter(typeof(ITerminalParameter));
                param.SetTerminalName(ToTerminalName(settings.TerminalType));

                if (p.AuthenticationType == AuthenticationType.PublicKey) {
                    if (!File.Exists(_privateKeyBox.Text))
                        msg = TEnv.Strings.GetString("Message.SSHShortcutLoginDialog.KeyFileNotExist");
                    else
                        p.IdentityFileName = _privateKeyBox.Text;
                }

                p.PasswordOrPassphrase = _passphraseBox.Text;

                string autoExecMacroPath = null;
                if (_autoExecMacroPathBox.Text.Length != 0)
                    autoExecMacroPath = _autoExecMacroPathBox.Text;

                IAutoExecMacroParameter autoExecParams = p.GetAdapter(typeof(IAutoExecMacroParameter)) as IAutoExecMacroParameter;
                if (autoExecParams != null)
                    autoExecParams.AutoExecMacroPath = autoExecMacroPath;

                if (msg != null) {
                    GUtil.Warning(this, msg);
                    return null;
                }
                else
                    return p;
            }
            catch (Exception ex) {
                GUtil.Warning(this, ex.Message);
                return null;
            }
        }
        private void OnOpenPrivateKey(object sender, System.EventArgs e) {
            string fn = TerminalUtil.SelectPrivateKeyFileByDialog(this);
            if (fn != null)
                _privateKeyBox.Text = fn;
        }
        private void OnLogTypeChanged(object sender, System.EventArgs args) {
            AdjustUI();
        }

        protected override ITerminalParameter PrepareTerminalParameter() {
            _sshParam = ValidateContent();

            return _sshParam == null ? null : (ITerminalParameter)_sshParam.GetAdapter(typeof(ITerminalParameter));
        }
        protected override void StartConnection() {
            IProtocolService protocolservice = TerminalSessionsPlugin.Instance.ProtocolService;
            _connector = protocolservice.AsyncSSHConnect(this, _sshParam);

            if (_connector == null)
                ClearConnectingState();
        }
        protected override void ShowError(string msg) {
            GUtil.Warning(this, msg, TEnv.Strings.GetString("Caption.LoginDialog.ConnectionError"));
        }
        private void SelectLog(object sender, System.EventArgs e) {
            string fn = LogUtil.SelectLogFileByDialog(this);
            if (fn != null)
                _logFileBox.Text = fn;
        }

        private int ToAuthenticationIndex(AuthenticationType at) {
            if (at == AuthenticationType.Password)
                return 0;
            else if (at == AuthenticationType.PublicKey)
                return 1;
            else //if(at==AuthenticationType.KeyboardInteractive)
                return 2;
        }

        private void _selectAutoExecMacroButton_Click(object sender, EventArgs e) {
            if (TelnetSSHPlugin.Instance.MacroEngine != null) {
                string path = TelnetSSHPlugin.Instance.MacroEngine.SelectMacro(this);
                if (path != null)
                    _autoExecMacroPathBox.Text = path;
            }
        }
    }
}
