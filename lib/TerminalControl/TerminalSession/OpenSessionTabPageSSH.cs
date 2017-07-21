// Copyright 2016 The Poderosa Project.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

using Granados;
using Granados.X11Forwarding;
using Poderosa.ConnectionParam;
using Poderosa.Forms;
using Poderosa.Plugins;
using Poderosa.Protocols;
using Poderosa.Terminal;
using Poderosa.UI;
using Poderosa.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Poderosa.Sessions {

    /// <summary>
    /// SSH settings page for <see cref="OpenSessionDialog"/>.
    /// </summary>
    internal partial class OpenSessionTabPageSSH : UserControl, IOpenSessionTabPage {

        private const string DEFAULT_SSH_PORT = "22";

        private readonly List<ParameterItem> _historyItems = new List<ParameterItem>();

        private IPoderosaMainWindow _mainWindow;
        private bool _preventUpdateControlStatus;

        /// <summary>
        /// Wrapper class for items in the host combobox.
        /// </summary>
        private class ParameterItem {
            public readonly ISSHLoginParameter SSHParameter;
            public readonly ITCPParameter TCPParameter;
            public readonly ITerminalParameter TerminalParameter;
            public readonly ITerminalSettings TerminalSettings;

            public ParameterItem(ISSHLoginParameter sshParam, ITCPParameter tcpParam, ITerminalParameter terminalParam, ITerminalSettings terminalSettings) {
                SSHParameter = sshParam;
                TCPParameter = tcpParam;
                TerminalParameter = terminalParam;
                TerminalSettings = terminalSettings;
            }

            public override string ToString() {
                return TCPParameter.Destination;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OpenSessionTabPageSSH() {
            InitializeComponent();
            SetIcons();
        }

        private void SetIcons() {
            _icons.ColorDepth = ColorDepth.Depth32Bit;
            _icons.ImageSize = new Size(12, 12);
            var color = SystemColors.ControlText;
            _icons.Images.Add(IconUtil.CreateColoredIcon(Poderosa.TerminalSession.Properties.Resources.TerminalSmall, color));
            _icons.Images.Add(IconUtil.CreateColoredIcon(Poderosa.TerminalSession.Properties.Resources.X11Small, color));
            _icons.Images.Add(IconUtil.CreateColoredIcon(Poderosa.TerminalSession.Properties.Resources.AuthSmall, color));
            _icons.Images.Add(IconUtil.CreateColoredIcon(Poderosa.TerminalSession.Properties.Resources.MacroSmall, color));

            _optionsTab.ImageList = _icons;

            _terminalTabPage.ImageIndex = 0;
            _x11ForwardingTabPage.ImageIndex = 1;
            _agentForwardingTabPage.ImageIndex = 2;
            _macroTabPage.ImageIndex = 3;
        }

        #region IOpenSessionTabPage

        /// <summary>
        /// Session type name
        /// </summary>
        public string SessionTypeName {
            get {
                return "SSH";
            }
        }

        /// <summary>
        /// Initialize the page
        /// </summary>
        /// <remarks>
        /// This method will be called in the constructor of the container dialog.
        /// </remarks>
        /// <param name="mainWindow">main window</param>
        public void Initialize(IPoderosaMainWindow mainWindow) {
            _mainWindow = mainWindow;
            _preventUpdateControlStatus = true;
            Localize();
            SetDefaultValues();
            LoadHistory();
            _preventUpdateControlStatus = false;
            UpdateControlStatus();
        }

        /// <summary>
        /// Set focus to the appropriate control.
        /// </summary>
        /// <returns>
        /// true if a control in this tab page was focused.
        /// If false was returned, parent form will set the focus on the "OK" button.
        /// </returns>
        public bool SetFocus() {
            if (String.IsNullOrEmpty(_hostBox.Text)) {
                _hostBox.Focus();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Start opening session
        /// </summary>
        /// <remarks>
        /// The implementation of this method also do validation of the input values.
        /// </remarks>
        /// <param name="client">an instance who receive the result of opening session.</param>
        /// <param name="terminalSettings">terminal settings is set if this method returns true.</param>
        /// <param name="interruptable">an object for cancellation is set if this method returns true.</param>
        /// <returns>true if the opening session has been started, or false if failed.</returns>
        public bool OpenSession(IInterruptableConnectorClient client, out ITerminalSettings terminalSettings, out IInterruptable interruptable) {
            ISSHLoginParameter loginParam;
            ITerminalSettings termSettings;
            string errorMessage;
            if (!Validate(out loginParam, out termSettings, out errorMessage)) {
                client.ConnectionFailed(errorMessage);
                terminalSettings = null;
                interruptable = null;
                return false;
            }

            IProtocolService protocolservice = TerminalSessionsPlugin.Instance.ProtocolService;
            interruptable = protocolservice.AsyncSSHConnect(client, loginParam);
            terminalSettings = termSettings;
            return true;
        }

        #endregion

        /// <summary>
        /// Localize controls
        /// </summary>
        private void Localize() {
            this._hostLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._hostLabel");
            this._portLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._portLabel");
            this._privateKeyLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._privateKeyLabel");
            this._authenticationLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._authenticationLabel");
            this._passphraseLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._passphraseLabel");
            this._usernameLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._usernameLabel");
            this._autoExecMacroPathLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._autoExecMacroPathLabel");
            this._localEchoLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._localEchoLabel");
            this._newLineLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._newLineLabel");
            this._logFileLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._logFileLabel");
            this._encodingLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._encodingLabel");
            this._logTypeLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._logTypeLabel");
            this._terminalTypeLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._terminalTypeLabel");

            this._portBox.Items.Add(DEFAULT_SSH_PORT);

            this._logTypeBox.Items.AddRange(EnumListItem<LogType>.GetListItems());
            this._terminalTypeBox.Items.AddRange(EnumListItem<TerminalType>.GetListItems());
            this._encodingBox.Items.AddRange(EnumListItem<EncodingType>.GetListItems());

            this._localEchoBox.Items.AddRange(new object[] {
                new ListItem<bool>(false, TEnv.Strings.GetString("Common.DoNot")),
                new ListItem<bool>(true, TEnv.Strings.GetString("Common.Do")),
            });

            this._newLineBox.Items.AddRange(EnumListItem<NewLine>.GetListItems());
            this._authOptions.Items.AddRange(EnumListItem<AuthType>.GetListItems());

            this._useX11ForwardingCheckBox.Text = TEnv.Strings.GetString("Form.LoginDialog._useX11ForwardingCheckBox");
            this._x11DisplayLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._x11DisplayLabel");
            this._x11DisplayNote.Text = TEnv.Strings.GetString("Form.LoginDialog._x11DisplayNote");
            this._toolTip.SetToolTip(this._x11DisplayText, TEnv.Strings.GetString("Form.LoginDialog._x11DisplayText_ToolTip"));
            this._x11ScreenLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._x11ScreenLabel");
            this._toolTip.SetToolTip(this._x11ScreenText, TEnv.Strings.GetString("Form.LoginDialog._x11ScreenText_ToolTip"));
            this._x11NeedAuthCheckBox.Text = TEnv.Strings.GetString("Form.LoginDialog._x11NeedAuthCheckBox");
            this._x11XauthorityLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._x11XauthorityLabel");
            this._x11UseCygwinDomainSocketCheckBox.Text = TEnv.Strings.GetString("Form.LoginDialog._x11UseCygwinDomainSocketCheckBox");
            this._x11CygwinX11UnixFolderLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._x11CygwinX11UnixFolderLabel");
            this._x11CygwinX11UnixFolderExampleLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._x11CygwinX11UnixFolderExampleLabel");
            this._x11CygwinX11UnixFolderExampleLabel.Font = new Font(this._x11CygwinX11UnixFolderExampleLabel.Font.FontFamily, 8f);

            this._useAgentForwardingCheckBox.Text = TEnv.Strings.GetString("Form.LoginDialog._useAgentForwardingCheckBox");
            this._agentForwardingConfigButton.Text = TEnv.Strings.GetString("Form.LoginDialog._agentForwardingConfigButton");
        }

        /// <summary>
        /// Sets default values.
        /// </summary>
        private void SetDefaultValues() {
            this._ssh2RadioButton.Checked = true;
            this._portBox.Text = DEFAULT_SSH_PORT;
            this._logTypeBox.SelectedItem = LogType.None;
            this._terminalTypeBox.SelectedItem = TerminalType.XTerm;
            this._encodingBox.SelectedItem = EncodingType.UTF8;
            this._newLineBox.SelectedItem = NewLine.CR;
            this._authOptions.SelectedItem = AuthType.Password;
            this._localEchoBox.SelectedItem = false;

            if (TelnetSSHPlugin.Instance.MacroEngine == null) {
                _autoExecMacroPathLabel.Enabled =
                    _autoExecMacroPathBox.Enabled = false;
            }
        }

        /// <summary>
        /// Load connection parameter history
        /// </summary>
        private void LoadHistory() {
            _hostBox.Items.Clear();
            _historyItems.Clear();

            IExtensionPoint extp = TerminalSessionsPlugin.Instance.PoderosaWorld.PluginManager.FindExtensionPoint("org.poderosa.terminalsessions.terminalParameterStore");
            if (extp != null) {
                var stores = extp.GetExtensions() as ITerminalSessionParameterStore[];
                if (stores != null) {
                    foreach (var store in stores) {
                        _historyItems.AddRange(
                            // create combobox items from SSH parameters
                            store.FindTerminalParameter<ISSHLoginParameter>()
                                .Select((sessionParams) => {
                                    ISSHLoginParameter sshParam = sessionParams.ConnectionParameter;
                                    ITCPParameter tcpParam = sshParam.GetAdapter(typeof(ITCPParameter)) as ITCPParameter;
                                    if (tcpParam != null) {
                                        return new ParameterItem(sshParam, tcpParam, sessionParams.TerminalParameter, sessionParams.TerminalSettings);
                                    }
                                    else {
                                        return null;
                                    }
                                })
                                .Where((item) => {
                                    return item != null;
                                })
                        );
                    }
                }
            }

            if (_historyItems.Count > 0) {
                _hostBox.Items.AddRange(_historyItems.ToArray());
                _hostBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Apply specified parameters to the input controls.
        /// </summary>
        /// <param name="item">parameters to apply</param>
        private void ApplyParameters(ParameterItem item) {
            UpdateUserNameCandidates(item);

            if (item.SSHParameter.Method == Granados.SSHProtocol.SSH1) {
                this._ssh1RadioButton.Checked = true;
            }
            else {
                this._ssh2RadioButton.Checked = true;
            }

            _portBox.Text = item.TCPParameter.Port.ToString();
            _userNameBox.Text = item.SSHParameter.Account;
            _authOptions.SelectedItem = item.SSHParameter.AuthenticationType.ToAuthType();
            _passphraseBox.Text = item.SSHParameter.PasswordOrPassphrase;
            _privateKeyFile.Text = item.SSHParameter.IdentityFileName;

            _encodingBox.SelectedItem = item.TerminalSettings.Encoding;
            _localEchoBox.SelectedItem = item.TerminalSettings.LocalEcho;
            _newLineBox.SelectedItem = item.TerminalSettings.TransmitNL;
            _terminalTypeBox.SelectedItem = item.TerminalSettings.TerminalType;

            _useX11ForwardingCheckBox.Checked = item.SSHParameter.EnableX11Forwarding;

            if (item.SSHParameter.X11Forwarding != null) {
                _x11DisplayText.Text = item.SSHParameter.X11Forwarding.Display.ToString();
                _x11ScreenText.Text = item.SSHParameter.X11Forwarding.Screen.ToString();
                _x11NeedAuthCheckBox.Checked = item.SSHParameter.X11Forwarding.NeedAuth;
                _x11XauthorityText.Text = item.SSHParameter.X11Forwarding.XauthorityFile;
                _x11UseCygwinDomainSocketCheckBox.Checked = item.SSHParameter.X11Forwarding.UseCygwinUnixDomainSocket;
                _x11CygwinX11UnixFolderText.Text = item.SSHParameter.X11Forwarding.X11UnixFolder;
            }
            else {
                _x11DisplayText.Text = "0";
                _x11ScreenText.Text = "0";
                _x11NeedAuthCheckBox.Checked = false;
                _x11XauthorityText.Text = "";
                _x11UseCygwinDomainSocketCheckBox.Checked = false;
                _x11CygwinX11UnixFolderText.Text = "";
            }

            _useAgentForwardingCheckBox.Checked = item.SSHParameter.EnableAgentForwarding;

            UpdateControlStatus();
        }

        private void UpdateUserNameCandidates(ParameterItem currentItem) {
            _userNameBox.Items.Clear();
            _userNameBox.Items.AddRange(
                _historyItems.Where(item => item.TCPParameter.Destination == currentItem.TCPParameter.Destination)
                        .Select(item => item.SSHParameter.Account)
                        .Distinct()
                        .OrderBy(s => s)
                        .ToArray()
            );
        }

        /// <summary>
        /// Updates "Enabled" status of the controls.
        /// </summary>
        private void UpdateControlStatus() {
            if (_preventUpdateControlStatus) {
                return;
            }

            if (_ssh1RadioButton.Checked) {
                // Note:
                // agent forwarding on SSH1 is not available
                // because the class KeyAgent doesn't support SSH1 private key.
                _useAgentForwardingCheckBox.Checked = false;
                _useAgentForwardingCheckBox.Enabled = false;

                // keyboard interactive authentication is not available on SSH1
                if (SelectedItemEquals(_authOptions, AuthType.KeyboardInteractive)) {
                    _authOptions.SelectedItem = AuthType.Password;
                }
            }
            else {
                _useAgentForwardingCheckBox.Enabled = true;
            }

            bool usePassword = SelectedItemEquals(_authOptions, AuthType.Password);
            bool usePublicKey = SelectedItemEquals(_authOptions, AuthType.PublicKey);

            _passphraseLabel.Enabled =
                _passphraseBox.Enabled = (usePassword || usePublicKey);

            _privateKeyLabel.Enabled =
                _privateKeyFile.Enabled =
                _privateKeySelect.Enabled = usePublicKey;

            bool noLog = SelectedItemEquals(_logTypeBox, LogType.None);
            _logFileLabel.Enabled =
                _logFileBox.Enabled =
                _selectLogButton.Enabled = !noLog;

            _x11ForwardingOptionsPanel.Enabled = _useX11ForwardingCheckBox.Checked;

            _x11XauthorityLabel.Enabled =
                _x11XauthorityText.Enabled =
                _x11XauthorityButton.Enabled = _x11NeedAuthCheckBox.Checked;

            _x11CygwinX11UnixFolderLabel.Enabled =
                _x11CygwinX11UnixFolderText.Enabled =
                _x11CygwinX11UnixFolderButton.Enabled =
                _x11CygwinX11UnixFolderExampleLabel.Enabled = _x11UseCygwinDomainSocketCheckBox.Checked;

            _agentForwardingConfigButton.Enabled = _useAgentForwardingCheckBox.Checked;
        }

        /// <summary>
        /// Utility method that checks if the specified value is selected on the combobox.
        /// </summary>
        /// <typeparam name="T">type of the value</typeparam>
        /// <param name="comboBox">target combobox</param>
        /// <param name="value">value to be compared</param>
        /// <returns>true if the values is selected. otherwise false.</returns>
        private bool SelectedItemEquals<T>(ComboBox comboBox, T value) {
            object item = comboBox.SelectedItem;
            if (item == null) {
                return false;
            }

            EnumListItem<T> enumListItem = item as EnumListItem<T>;
            if (enumListItem != null) {
                return enumListItem.Equals(value);
            }

            ListItem<T> listItem = item as ListItem<T>;
            if (listItem != null) {
                return listItem.Equals(value);
            }

            return false;
        }

        /// <summary>
        /// Validates input values and constructs parameter objects.
        /// </summary>
        /// <param name="loginParam">SSH parameter object is set when this method returns true.</param>
        /// <param name="terminalSettings">terminal settings object is set when this method returns true.</param>
        /// <param name="errorMessage">validation error message is set when this method returns false. this can be null when displaying error message is not needed.</param>
        /// <returns>true if all validations passed and parameter objects were created.</returns>
        private bool Validate(out ISSHLoginParameter loginParam, out ITerminalSettings terminalSettings, out string errorMessage) {
            loginParam = null;
            terminalSettings = null;
            errorMessage = null;

            var ssh = TerminalSessionsPlugin.Instance.ProtocolService.CreateDefaultSSHParameter();
            var tcp = (ITCPParameter)ssh.GetAdapter(typeof(ITCPParameter));

            //--- SSH connection settings

            if (_ssh1RadioButton.Checked) {
                ssh.Method = SSHProtocol.SSH1;
            }
            else if (_ssh2RadioButton.Checked) {
                ssh.Method = SSHProtocol.SSH2;
            }
            else {
                errorMessage = TEnv.Strings.GetString("Message.LoginDialog.ProtocolVersionIsNotSpecified");
                return false;
            }

            tcp.Destination = _hostBox.Text;
            if (String.IsNullOrEmpty(tcp.Destination)) {
                errorMessage = TEnv.Strings.GetString("Message.LoginDialog.HostIsEmpty");
                return false;
            }

            int port;
            if (Int32.TryParse(_portBox.Text, out port) && port >= 0 && port <= 65535) {
                tcp.Port = port;
            }
            else {
                errorMessage = String.Format(
                                    TEnv.Strings.GetString("Message.LoginDialog.InvalidPort"),
                                    _portBox.Text);
                return false;
            }

            ssh.Account = _userNameBox.Text;

            AuthType authType = ((EnumListItem<AuthType>)_authOptions.SelectedItem).Value;
            ssh.AuthenticationType = authType.ToAuthenticationType();

            if (ssh.AuthenticationType == AuthenticationType.PublicKey) {
                ssh.IdentityFileName = _privateKeyFile.Text;

                if (String.IsNullOrEmpty(ssh.IdentityFileName)) {
                    errorMessage = TEnv.Strings.GetString("Message.LoginDialog.PrivateKeyFileIsNotSpecified");
                    return false;
                }

                if (!File.Exists(ssh.IdentityFileName)) {
                    errorMessage = TEnv.Strings.GetString("Message.LoginDialog.KeyFileNotExist");
                    return false;
                }
            }

            if (ssh.AuthenticationType == AuthenticationType.Password || ssh.AuthenticationType == AuthenticationType.PublicKey) {
                ssh.PasswordOrPassphrase = _passphraseBox.Text;
            }

            //--- Log settings

            ISimpleLogSettings logSettings = TerminalSessionsPlugin.Instance.TerminalEmulatorService.CreateDefaultSimpleLogSettings();

            logSettings.LogType = ((EnumListItem<LogType>)_logTypeBox.SelectedItem).Value;
            if (logSettings.LogType != LogType.None) {
                logSettings.LogPath = _logFileBox.Text;
                LogFileCheckResult r = LogUtil.CheckLogFileName(logSettings.LogPath, this.ParentForm);
                if (r == LogFileCheckResult.Cancel || r == LogFileCheckResult.Error) {
                    errorMessage = null;
                    return false;
                }
                logSettings.LogAppend = (r == LogFileCheckResult.Append);
            }

            //--- Terminal settings

            ITerminalParameter termParam = (ITerminalParameter)tcp.GetAdapter(typeof(ITerminalParameter));
            TerminalType terminalType = ((EnumListItem<TerminalType>)_terminalTypeBox.SelectedItem).Value;
            termParam.SetTerminalName(terminalType.ToTermValue());

            string terminalCaption = tcp.Destination;
            Image terminalIcon = Poderosa.TerminalSession.Properties.Resources.NewConnection16x16;
            ITerminalSettings termSettings = TerminalSessionsPlugin.Instance.TerminalEmulatorService.CreateDefaultTerminalSettings(terminalCaption, terminalIcon);
            termSettings.BeginUpdate();
            termSettings.Encoding = ((EnumListItem<EncodingType>)_encodingBox.SelectedItem).Value;
            termSettings.LocalEcho = ((ListItem<bool>)_localEchoBox.SelectedItem).Value;
            termSettings.TransmitNL = ((EnumListItem<NewLine>)_newLineBox.SelectedItem).Value;
            termSettings.TerminalType = terminalType;
            termSettings.LogSettings.Reset(logSettings);
            termSettings.EndUpdate();

            //--- X11 forwarding settings

            if (_useX11ForwardingCheckBox.Checked) {
                if (String.IsNullOrEmpty(_x11DisplayText.Text)) {
                    errorMessage = TEnv.Strings.GetString("Message.LoginDialog.X11DisplayIsNotEntered");
                    return false;
                }
                int display;
                if (!Int32.TryParse(_x11DisplayText.Text, out display) || display < 0 || display > (65535 - 6000)) {
                    errorMessage = TEnv.Strings.GetString("Message.LoginDialog.InvalidX11Display");
                    return false;
                }

                X11ForwardingParams x11Param = new X11ForwardingParams(display);

                if (String.IsNullOrEmpty(_x11ScreenText.Text)) {
                    errorMessage = TEnv.Strings.GetString("Message.LoginDialog.X11ScreenIsNotEntered");
                    return false;
                }
                int screen;
                if (!Int32.TryParse(_x11ScreenText.Text, out screen) || screen < 0) {
                    errorMessage = TEnv.Strings.GetString("Message.LoginDialog.InvalidX11Screen");
                    return false;
                }
                x11Param.Screen = screen;

                if (_x11NeedAuthCheckBox.Checked) {
                    x11Param.NeedAuth = true;
                    x11Param.XauthorityFile = _x11XauthorityText.Text;
                    if (String.IsNullOrEmpty(x11Param.XauthorityFile)) {
                        errorMessage = TEnv.Strings.GetString("Message.LoginDialog.XauthorityFileIsNotSpecified");
                        return false;
                    }
                }
                else {
                    x11Param.NeedAuth = false;
                }

                if (_x11UseCygwinDomainSocketCheckBox.Checked) {
                    x11Param.UseCygwinUnixDomainSocket = true;
                    x11Param.X11UnixFolder = _x11CygwinX11UnixFolderText.Text;
                    if (String.IsNullOrEmpty(x11Param.X11UnixFolder)) {
                        errorMessage = TEnv.Strings.GetString("Message.LoginDialog.X11UnixFolderIsNotSpecified");
                        return false;
                    }
                }

                ssh.EnableX11Forwarding = true;
                ssh.X11Forwarding = x11Param;
            }
            else {
                ssh.EnableX11Forwarding = false;
                ssh.X11Forwarding = null;
            }

            //--- Agent forwarding settings

            if (_useAgentForwardingCheckBox.Checked) {
                ssh.EnableAgentForwarding = true;
                ssh.AgentForwardingAuthKeyProvider = null;  // set later
            }
            else {
                ssh.EnableAgentForwarding = false;
                ssh.AgentForwardingAuthKeyProvider = null;
            }

            //--- Macro

            IAutoExecMacroParameter autoExecParams = tcp.GetAdapter(typeof(IAutoExecMacroParameter)) as IAutoExecMacroParameter;
            if (autoExecParams != null) {   // macro plugin is enabled
                if (!String.IsNullOrEmpty(_autoExecMacroPathBox.Text)) {
                    autoExecParams.AutoExecMacroPath = _autoExecMacroPathBox.Text;
                }
                else {
                    autoExecParams.AutoExecMacroPath = null;
                }
            }

            loginParam = ssh;
            terminalSettings = termSettings;
            return true;
        }

        private void _ssh1RadioButton_CheckedChanged(object sender, EventArgs e) {
            if (_ssh1RadioButton.Checked) {
                UpdateControlStatus();
            }
        }

        private void _ssh2RadioButton_CheckedChanged(object sender, EventArgs e) {
            if (_ssh2RadioButton.Checked) {
                UpdateControlStatus();
            }
        }

        private void _authOptions_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateControlStatus();
        }

        private void _hostBox_SelectedIndexChanged(object sender, EventArgs e) {
            var item = _hostBox.SelectedItem as ParameterItem;
            if (item != null) {
                ApplyParameters(item);
            }
        }

        private void _logTypeBox_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateControlStatus();
        }

        private void _useX11ForwardingCheckBox_CheckedChanged(object sender, EventArgs e) {
            UpdateControlStatus();
        }

        private void _x11NeedAuthCheckBox_CheckedChanged(object sender, EventArgs e) {
            UpdateControlStatus();
        }

        private void _x11UseCygwinDomainSocketCheckBox_CheckedChanged(object sender, EventArgs e) {
            UpdateControlStatus();
        }

        private void _useAgentForwardingCheckBox_CheckedChanged(object sender, EventArgs e) {
            UpdateControlStatus();
        }

        private void _agentForwardingConfigButton_Click(object sender, EventArgs e) {
            if (_mainWindow == null) {
                return;
            }

            var commandManager = TerminalSessionsPlugin.Instance.CommandManager;
            if (commandManager == null) {
                return;
            }

            var command = commandManager.Find("org.poderosa.sshutil.agentkeylistdialog");
            if (command == null) {
                return;
            }

            commandManager.Execute(command, _mainWindow);
        }

        private void _privateKeySelect_Click(object sender, EventArgs e) {
            string fn = TerminalUtil.SelectPrivateKeyFileByDialog(this.ParentForm);
            if (fn != null) {
                _privateKeyFile.Text = fn;
            }
        }

        private void _selectLogButton_Click(object sender, EventArgs e) {
            string fn = LogUtil.SelectLogFileByDialog(this.ParentForm);
            if (fn != null) {
                _logFileBox.Text = fn;
            }
        }

        private void _x11XauthorityButton_Click(object sender, EventArgs e) {
            using (OpenFileDialog dlg = new OpenFileDialog()) {
                dlg.CheckFileExists = true;
                dlg.Multiselect = false;
                dlg.AddExtension = false;
                dlg.Title = TEnv.Strings.GetString("Caption.LoginDialog.SelectXauthorityFile");
                dlg.Filter = ".Xauthority|.Xauthority|All files|*.*";
                dlg.FileName = _x11XauthorityText.Text;
                if (dlg.ShowDialog(this.ParentForm) == DialogResult.OK) {
                    _x11XauthorityText.Text = dlg.FileName;
                }
            }
        }

        private void _x11CygwinX11UnixFolderButton_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog()) {
                dlg.Description = null;
                dlg.ShowNewFolderButton = false;
                dlg.SelectedPath = _x11CygwinX11UnixFolderText.Text;
                if (dlg.ShowDialog(this.ParentForm) == DialogResult.OK) {
                    _x11CygwinX11UnixFolderText.Text = dlg.SelectedPath;
                }
            }
        }

        private void _selectAutoExecMacroButton_Click(object sender, EventArgs e) {
            string path = TelnetSSHPlugin.Instance.MacroEngine.SelectMacro(this.ParentForm);
            if (path != null) {
                _autoExecMacroPathBox.Text = path;
            }
        }
    }
}
