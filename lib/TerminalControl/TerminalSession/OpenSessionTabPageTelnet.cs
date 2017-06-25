// Copyright 2016 The Poderosa Project.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

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
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Poderosa.Sessions {

    /// <summary>
    /// TELNET settings page for <see cref="OpenSessionDialog"/>.
    /// </summary>
    internal partial class OpenSessionTabPageTelnet : UserControl, IOpenSessionTabPage {

        private const string DEFAULT_TELNET_PORT = "23";

        private readonly List<ParameterItem> _historyItems = new List<ParameterItem>();

        private IPoderosaMainWindow _mainWindow;
        private bool _preventUpdateControlStatus;

        /// <summary>
        /// Wrapper class for items in the host combobox.
        /// </summary>
        private class ParameterItem {
            public readonly ITelnetParameter TelnetParameter;
            public readonly ITCPParameter TCPParameter;
            public readonly ITerminalParameter TerminalParameter;
            public readonly ITerminalSettings TerminalSettings;

            public ParameterItem(ITelnetParameter telnetParam, ITCPParameter tcpParam, ITerminalParameter terminalParam, ITerminalSettings terminalSettings) {
                TelnetParameter = telnetParam;
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
        public OpenSessionTabPageTelnet() {
            InitializeComponent();
            SetIcons();
        }

        private void SetIcons() {
            _icons.ColorDepth = ColorDepth.Depth32Bit;
            _icons.ImageSize = new Size(12, 12);
            var color = SystemColors.ControlText;
            _icons.Images.Add(IconUtil.CreateColoredIcon(Poderosa.TerminalSession.Properties.Resources.TerminalSmall, color));
            _icons.Images.Add(IconUtil.CreateColoredIcon(Poderosa.TerminalSession.Properties.Resources.MacroSmall, color));

            _optionsTab.ImageList = _icons;

            _terminalTabPage.ImageIndex = 0;
            _macroTabPage.ImageIndex = 1;
        }

        #region IOpenSessionTabPage

        /// <summary>
        /// Session type name
        /// </summary>
        public string SessionTypeName {
            get {
                return "TELNET";
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
            ITCPParameter telnetParam;
            ITerminalSettings termSettings;
            string errorMessage;
            if (!Validate(out telnetParam, out termSettings, out errorMessage)) {
                client.ConnectionFailed(errorMessage);
                terminalSettings = null;
                interruptable = null;
                return false;
            }

            IProtocolService protocolservice = TerminalSessionsPlugin.Instance.ProtocolService;
            interruptable = protocolservice.AsyncTelnetConnect(client, telnetParam);
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
            this._autoExecMacroPathLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._autoExecMacroPathLabel");
            this._localEchoLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._localEchoLabel");
            this._newLineLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._newLineLabel");
            this._logFileLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._logFileLabel");
            this._encodingLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._encodingLabel");
            this._logTypeLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._logTypeLabel");
            this._terminalTypeLabel.Text = TEnv.Strings.GetString("Form.LoginDialog._terminalTypeLabel");
            this._telnetNewLine.Text = TEnv.Strings.GetString("Form.LoginDialog._telnetNewLine");

            this._portBox.Items.Add(DEFAULT_TELNET_PORT);

            this._logTypeBox.Items.AddRange(EnumListItem<LogType>.GetListItems());
            this._terminalTypeBox.Items.AddRange(EnumListItem<TerminalType>.GetListItems());
            this._encodingBox.Items.AddRange(EnumListItem<EncodingType>.GetListItems());

            this._localEchoBox.Items.AddRange(new object[] {
                new ListItem<bool>(false, TEnv.Strings.GetString("Common.DoNot")),
                new ListItem<bool>(true, TEnv.Strings.GetString("Common.Do")),
            });

            this._newLineBox.Items.AddRange(EnumListItem<NewLine>.GetListItems());
        }

        /// <summary>
        /// Sets default values.
        /// </summary>
        private void SetDefaultValues() {
            this._portBox.Text = DEFAULT_TELNET_PORT;
            this._logTypeBox.SelectedItem = LogType.None;
            this._terminalTypeBox.SelectedItem = TerminalType.XTerm;
            this._encodingBox.SelectedItem = EncodingType.UTF8;
            this._newLineBox.SelectedItem = NewLine.CR;
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
                            // create combobox items from TELNET parameters
                            store.FindTerminalParameter<ITCPParameter>()
                                .Select((sessionParams) => {
                                    ITCPParameter tcpParam = sessionParams.ConnectionParameter;
                                    ITelnetParameter telnetParam = tcpParam.GetAdapter(typeof(ITelnetParameter)) as ITelnetParameter;
                                    if (telnetParam != null) {
                                        return new ParameterItem(telnetParam, tcpParam, sessionParams.TerminalParameter, sessionParams.TerminalSettings);
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
            _portBox.Text = item.TCPParameter.Port.ToString();

            _encodingBox.SelectedItem = item.TerminalSettings.Encoding;
            _localEchoBox.SelectedItem = item.TerminalSettings.LocalEcho;
            _newLineBox.SelectedItem = item.TerminalSettings.TransmitNL;
            _telnetNewLine.Checked =
                (item.TerminalSettings.TransmitNL == NewLine.CRLF) ? item.TelnetParameter.TelnetNewLine : false;
            _terminalTypeBox.SelectedItem = item.TerminalSettings.TerminalType;

            UpdateControlStatus();
        }

        /// <summary>
        /// Updates "Enabled" status of the controls.
        /// </summary>
        private void UpdateControlStatus() {
            if (_preventUpdateControlStatus) {
                return;
            }

            bool noLog = SelectedItemEquals(_logTypeBox, LogType.None);
            _logFileLabel.Enabled =
                _logFileBox.Enabled =
                _selectLogButton.Enabled = !noLog;

            _telnetNewLine.Enabled = SelectedItemEquals(_newLineBox, NewLine.CRLF);
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
        /// <param name="telnetParam">Telnet parameter object is set when this method returns true.</param>
        /// <param name="terminalSettings">terminal settings object is set when this method returns true.</param>
        /// <param name="errorMessage">validation error message is set when this method returns false. this can be null when displaying error message is not needed.</param>
        /// <returns>true if all validations passed and parameter objects were created.</returns>
        private bool Validate(out ITCPParameter telnetParam, out ITerminalSettings terminalSettings, out string errorMessage) {
            telnetParam = null;
            terminalSettings = null;
            errorMessage = null;

            var telnet = TerminalSessionsPlugin.Instance.ProtocolService.CreateDefaultTelnetParameter();
            var tcp = (ITCPParameter)telnet.GetAdapter(typeof(ITCPParameter));
            var protocolParam = (ITelnetParameter)telnet.GetAdapter(typeof(ITelnetParameter));

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

            //--- TELNET protocol settings

            protocolParam.TelnetNewLine =
                (termSettings.TransmitNL == NewLine.CRLF) ? _telnetNewLine.Checked : false;

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

            telnetParam = tcp;
            terminalSettings = termSettings;
            return true;
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

        private void _newLineBox_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateControlStatus();
        }

        private void _selectLogButton_Click(object sender, EventArgs e) {
            string fn = LogUtil.SelectLogFileByDialog(this.ParentForm);
            if (fn != null) {
                _logFileBox.Text = fn;
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
