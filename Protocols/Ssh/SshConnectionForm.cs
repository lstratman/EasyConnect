using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Granados.SSHC;
using Poderosa;
using Poderosa.Config;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using WalburySoftware;

namespace EasyConnect.Protocols.Ssh
{
    /// <summary>
    /// UI that displays a Secure Shell (SSH) connection via the <see cref="TerminalControl"/> class.
    /// </summary>
    public partial class SshConnectionForm : BaseConnectionForm<SshConnection>
    {
        /// <summary>
        /// Default constructor.  Turns off the warning that the terminal displays when it encounters an unknown terminal control code.
        /// </summary>
        public SshConnectionForm()
        {
            InitializeComponent();

            GEnv.Options.WarningOption = WarningOption.Ignore;
            Shown += SshConnectionForm_Shown;
        }

        /// <summary>
        /// Control instance that hosts the actual SSH prompt UI.
        /// </summary>
        protected override Control ConnectionWindow
        {
            get
            {
                return _terminal;
            }
        }

        /// <summary>
        /// Handler method that's called when the connection window is shown, either initially or after the user switches to another tab and then back to this
        /// one.  It's necessary to call <see cref="Connections.BringToActivationOrderTop"/> so that shortcut keys entered by the user are sent to the correct
        /// window.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void SshConnectionForm_Shown(object sender, EventArgs e)
        {
            if (IsConnected)
                GEnv.Connections.BringToActivationOrderTop(_terminal.TerminalPane.ConnectionTag);
        }

        /// <summary>
        /// Handler method that's called when the connection is established.  Adds this connection to <see cref="Connections"/> and sends it to the top of the
        /// activation queue via a call to <see cref="Connections.BringToActivationOrderTop"/>.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        protected override void OnConnected(object sender, EventArgs e)
        {
            GEnv.Connections.Add(_terminal.TerminalPane.ConnectionTag);
            GEnv.Connections.BringToActivationOrderTop(_terminal.TerminalPane.ConnectionTag);

            base.OnConnected(sender, e);
        }

        /// <summary>
        /// Establishes the connection to the remote server; initializes this <see cref="_terminal"/>'s properties from 
        /// <see cref="BaseConnectionForm{T}.Connection"/> and then calls <see cref="TerminalControl.Connect"/> on <see cref="_terminal"/>.
        /// </summary>
        public override void Connect()
        {
            GEnv.Options.Font = Connection.Font;

            _terminal.UserName = Connection.InheritedUsername;
            _terminal.Host = Connection.Host;

            SecureString password = Connection.InheritedPassword;

            // Set the auth file and the auth method to PublicKey if an identity file was specified
            if (!String.IsNullOrEmpty(Connection.IdentityFile))
            {
                _terminal.IdentifyFile = Connection.IdentityFile;
                _terminal.AuthType = AuthType.PublicKey;
            }

            // Otherwise, set the auth type to Password
            else if (password != null && password.Length > 0)
            {
                IntPtr passwordBytes = Marshal.SecureStringToGlobalAllocAnsi(password);

                _terminal.Password = Marshal.PtrToStringAnsi(passwordBytes);
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

            // Wire up the connection event handlers and set the text and background colors
            ((ISSHChannelEventReceiver) _terminal.TerminalPane.Connection).Connected += OnConnected;
            ((ISSHConnectionEventReceiver) _terminal.TerminalPane.Connection).Disconnected += OnDisconnected;
            _terminal.SetPaneColors(Connection.TextColor, Connection.BackgroundColor);

            _terminal.TerminalPane.Focus();
        }

        /// <summary>
        /// Handler method that's called when an established connection is broken.  Closes the parent window if necessary.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="eventArgs">Arguments associated with this event.</param>
        private void OnDisconnected(object sender, EventArgs eventArgs)
        {
            IsConnected = false;

            if (CloseParentFormOnDisconnect)
                ParentForm.Invoke(new Action(() => ParentForm.Close()));
        }
    }
}