using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Granados.SSHC;
using Poderosa;
using Poderosa.Config;
using Poderosa.ConnectionParam;

namespace EasyConnect.Protocols.Ssh
{
    public partial class SshConnectionForm : BaseConnectionForm<SshConnection>
    {
        public SshConnectionForm()
        {
            InitializeComponent();

            GEnv.Options.WarningOption = WarningOption.Ignore;

            Connected += SshConnectionForm_Connected;
            Shown += SshConnectionForm_Shown;
            _terminal.GotFocus += _terminal_GotFocus;
        }

        void SshConnectionForm_Shown(object sender, EventArgs e)
        {
            if (IsConnected)
                GEnv.Connections.BringToActivationOrderTop(_terminal.TerminalPane.ConnectionTag);
        }

        void _terminal_GotFocus(object sender, EventArgs e)
        {
            if (ConnectionFormFocused != null)
                ConnectionFormFocused(_terminal, e);
        }

        void SshConnectionForm_Connected(object sender, EventArgs e)
        {
            IsConnected = true;

            GEnv.Connections.Add(_terminal.TerminalPane.ConnectionTag);
            GEnv.Connections.BringToActivationOrderTop(_terminal.TerminalPane.ConnectionTag);
        }

        public override void Connect()
        {
            GEnv.Options.Font = Connection.Font;
            
            _terminal.UserName = Connection.Username;
            _terminal.Host = Connection.Host;

            if (!String.IsNullOrEmpty(Connection.IdentityFile))
            {
                _terminal.IdentifyFile = Connection.IdentityFile;
                _terminal.AuthType = AuthType.PublicKey;
            }

            else if (Connection.Password != null && Connection.Password.Length > 0)
            {
                IntPtr password = Marshal.SecureStringToGlobalAllocAnsi(Connection.Password);
                
                _terminal.Password = Marshal.PtrToStringAnsi(password);
                _terminal.AuthType = AuthType.Password;
            }

            try
            {
                _terminal.Connect();
            }

            catch (SSHException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            
            ((ISSHChannelEventReceiver)_terminal.TerminalPane.Connection).Connected += Connected;
            ((ISSHConnectionEventReceiver)_terminal.TerminalPane.Connection).Disconnected += OnDisconnected;
            _terminal.SetPaneColors(Connection.TextColor, Connection.BackgroundColor);

            _terminal.TerminalPane.Focus();
        }

        private void OnDisconnected(object sender, EventArgs eventArgs)
        {
            IsConnected = false;
            ParentForm.Invoke(new Action(() => ParentForm.Close()));
        }

        public override event EventHandler Connected;
        public override event EventHandler ConnectionFormFocused;
    }
}
