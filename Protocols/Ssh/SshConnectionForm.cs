using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Granados;
using Poderosa.Boot;
using Poderosa.Plugins;
using Poderosa.Preferences;
using Poderosa.Protocols;
using Poderosa.Sessions;
using Poderosa.Terminal;
using Poderosa.View;

namespace EasyConnect.Protocols.Ssh
{
	/// <summary>
	/// UI that displays a Secure Shell (SSH) connection via the <see cref="TerminalControl"/> class.
	/// </summary>
	public partial class SshConnectionForm : BaseConnectionForm<SshConnection>, IInterruptableConnectorClient
	{
	    private static readonly IPoderosaWorld PoderosaApplication = null;
	    private static readonly IProtocolService PoderosaProtocolService = null;
	    private static readonly ITerminalEmulatorService PoderosaTerminalEmulatorService = null;
	    private static readonly SessionManagerPlugin PoderosaSessionManagerPlugin = null;

        static SshConnectionForm()
	    {
	        PoderosaApplication = (IPoderosaWorld) PoderosaStartup.CreatePoderosaApplication(new string[] {});
	        PoderosaApplication.InitializePlugins();

	        IRootExtension preferencesPlugin = (IRootExtension) PoderosaApplication.PluginManager.FindPlugin("org.poderosa.core.preferences", typeof(IPreferences));
            preferencesPlugin.InitializeExtension();

            PoderosaProtocolService = (IProtocolService) PoderosaApplication.PluginManager.FindPlugin("org.poderosa.protocols", typeof(IProtocolService));
	        PoderosaTerminalEmulatorService = (ITerminalEmulatorService) PoderosaApplication.PluginManager.FindPlugin("org.poderosa.terminalemulator", typeof(ITerminalEmulatorService));
	        PoderosaSessionManagerPlugin = (SessionManagerPlugin) PoderosaApplication.PluginManager.FindPlugin("org.poderosa.core.sessions", typeof(ISessionManager));

            PoderosaTerminalEmulatorService.TerminalEmulatorOptions.RightButtonAction = MouseButtonAction.Paste;
        }

		/// <summary>
		/// Default constructor.  Turns off the warning that the terminal displays when it encounters an unknown terminal control code.
		/// </summary>
		public SshConnectionForm()
		{
			InitializeComponent();
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
        /// Establishes the connection to the remote server; initializes a <see cref="ISSHLoginParameter"/> with the connection properties from 
        /// <see cref="BaseConnectionForm{T}.Connection"/> and then calls <see cref="IProtocolService.AsyncSSHConnect"/> to start the connection
        /// process.
        /// </summary>
        public override void Connect()
		{
            _terminal.Font = Connection.Font;

		    ISSHLoginParameter sshParameters = PoderosaProtocolService.CreateDefaultSSHParameter();
		    ITCPParameter tcpParameters = (ITCPParameter) sshParameters.GetAdapter(typeof(ITCPParameter));

		    tcpParameters.Destination = Connection.Host;
            // TODO: allow user to specify this
		    tcpParameters.Port = 22;

            sshParameters.Account = Connection.InheritedUsername;

			SecureString password = Connection.InheritedPassword;

			// Set the auth file and the auth method to PublicKey if an identity file was specified
		    if (!String.IsNullOrEmpty(Connection.IdentityFile))
		    {
                sshParameters.AuthenticationType = AuthenticationType.PublicKey;
		        sshParameters.IdentityFileName = Connection.IdentityFile;
		    }

		    // Otherwise, set the auth type to Password
		    else if (password != null && password.Length > 0)
		    {
		        sshParameters.AuthenticationType = AuthenticationType.Password;
                
                // TODO: add the ability for Poderosa to set this securely
                IntPtr passwordPointer = IntPtr.Zero;

                try
		        {
		            passwordPointer = Marshal.SecureStringToGlobalAllocUnicode(password);
		            sshParameters.PasswordOrPassphrase = Marshal.PtrToStringUni(passwordPointer);
		        }

		        finally
		        {
		            Marshal.ZeroFreeGlobalAllocUnicode(passwordPointer);
		        }
		    }

		    PoderosaProtocolService.AsyncSSHConnect(this, sshParameters);
		}

	    protected override void OnConnected(object sender, EventArgs e)
	    {
	        base.OnConnected(sender, e);
	        _terminal.Focus();
	    }

	    public void SuccessfullyExit(ITerminalConnection connection)
	    {
	        ITerminalSettings terminalSettings = PoderosaTerminalEmulatorService.CreateDefaultTerminalSettings(Connection.DisplayName, null);

            TerminalSession session = new TerminalSession(connection, terminalSettings);
            SessionHost sessionHost = new SessionHost(PoderosaSessionManagerPlugin, session);
	        TerminalView terminalView = new TerminalView(null, _terminal);
	        RenderProfile renderProfile = new RenderProfile(_terminal.GetRenderProfile());

	        renderProfile.BackColor = Connection.BackgroundColor;
	        renderProfile.ForeColor = Connection.TextColor;

	        session.TerminalSettings.BeginUpdate();
	        session.TerminalSettings.RenderProfile = renderProfile;
	        session.TerminalSettings.EndUpdate();

            SSHTerminalConnection sshConnection = (SSHTerminalConnection) connection;
            
	        Invoke(
	            new Action(
	                () =>
	                {
	                    _terminal.Attach(session);

	                    session.InternalStart(sessionHost);
                        session.InternalAttachView(sessionHost.DocumentAt(0), terminalView);

                        sshConnection.ConnectionEventReceiver.NormalTermination += ConnectionEventReceiver_NormalTermination;
                        sshConnection.ConnectionEventReceiver.AbnormalTermination += ConnectionEventReceiver_AbnormalTermination;

	                    OnConnected(_terminal, null);
                    }));
	    }

        private void ConnectionEventReceiver_AbnormalTermination(object sender, Granados.SSH.AbnormalTerminationEventArgs e)
        {
            Invoke(new Action(() => OnConnectionLost(this, new ErrorEventArgs(new Exception(e.Message)))));
        }

        private void ConnectionEventReceiver_NormalTermination(object sender, EventArgs e)
        {
            Invoke(new Action(() => ParentForm.Close()));
        }

        public void ConnectionFailed(string message)
	    {
	        Invoke(new Action(() => OnConnectionLost(this, new ErrorEventArgs(new Exception(message)))));
	    }

	    protected override void OnGotFocus(EventArgs e)
	    {
	        base.OnGotFocus(e);

	        if (Connection != null)
	        {
	            _terminal.Focus();
	        }
	    }
    }
}