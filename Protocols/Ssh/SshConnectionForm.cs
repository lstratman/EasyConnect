using System;
using System.Runtime.InteropServices;
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
            Shown += SshConnectionForm_Shown;
        }

        void SshConnectionForm_Shown(object sender, EventArgs e)
        {
            if (IsConnected)
                GEnv.Connections.BringToActivationOrderTop(_terminal.TerminalPane.ConnectionTag);
        }

        protected override void OnConnected(object sender, EventArgs e)
        {
            GEnv.Connections.Add(_terminal.TerminalPane.ConnectionTag);
            GEnv.Connections.BringToActivationOrderTop(_terminal.TerminalPane.ConnectionTag);

            base.OnConnected(sender, e);
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
            
            ((ISSHChannelEventReceiver)_terminal.TerminalPane.Connection).Connected += OnConnected;
            ((ISSHConnectionEventReceiver)_terminal.TerminalPane.Connection).Disconnected += OnDisconnected;
            _terminal.SetPaneColors(Connection.TextColor, Connection.BackgroundColor);

            _terminal.TerminalPane.Focus();
        }

        private void OnDisconnected(object sender, EventArgs eventArgs)
        {
            IsConnected = false;

            if (CloseParentFormOnDisconnect)
                ParentForm.Invoke(new Action(() => ParentForm.Close()));
        }

        protected override Control ConnectionWindow
        {
            get
            {
                return _terminal;
            }
        }
    }
}
