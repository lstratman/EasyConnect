using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using EasyConnect.Common;
using Granados;
using Poderosa.Boot;
using Poderosa.ConnectionParam;
using Poderosa.Plugins;
using Poderosa.Preferences;
using Poderosa.Protocols;
using Poderosa.Sessions;
using Poderosa.Terminal;
using Poderosa.View;

namespace EasyConnect.Protocols.Telnet
{
	/// <summary>
	/// UI that displays a Telnet connection via the <see cref="TerminalControl"/> class.
	/// </summary>
	public partial class TelnetConnectionForm : BaseConnectionForm<TelnetConnection>, IInterruptableConnectorClient
	{
	    private static readonly IPoderosaWorld PoderosaApplication = null;
	    private static readonly IProtocolService PoderosaProtocolService = null;
	    private static readonly ITerminalEmulatorService PoderosaTerminalEmulatorService = null;
	    private static readonly SessionManagerPlugin PoderosaSessionManagerPlugin = null;

	    protected TelnetTerminalConnection _telnetConnection = null;
	    protected bool _parentFormClosing = false;

        static TelnetConnectionForm()
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
		public TelnetConnectionForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Control instance that hosts the actual Telnet prompt UI.
		/// </summary>
		protected override Control ConnectionWindow
		{
			get
			{
				return _terminal;
			}
		}

        /// <summary>
        /// Establishes the connection to the remote server; initializes a <see cref="ITCPParameter"/> with the connection properties from 
        /// <see cref="BaseConnectionForm{T}.Connection"/> and then calls <see cref="IProtocolService.AsyncTelnetConnect(IInterruptableConnectorClient, ITCPParameter)"/> 
		/// to start the connection process.
        /// </summary>
        public override void Connect()
		{
		    ITCPParameter tcpParameters = PoderosaProtocolService.CreateDefaultTelnetParameter();

			tcpParameters.Destination = Connection.Host;
			tcpParameters.Port = Connection.Port;

			PoderosaProtocolService.AsyncTelnetConnect(this, tcpParameters);
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
            renderProfile.FontName = Connection.FontFamily;
            renderProfile.FontSize = Connection.FontSize;

	        session.TerminalSettings.BeginUpdate();
            session.TerminalSettings.Encoding = Connection.Encoding;
	        session.TerminalSettings.RenderProfile = renderProfile;
	        session.TerminalSettings.EndUpdate();

            _telnetConnection = (TelnetTerminalConnection) connection;
            
	        Invoke(
	            new Action(
	                () =>
	                {
	                    _terminal.Attach(session);

	                    session.InternalStart(sessionHost);
                        session.InternalAttachView(sessionHost.DocumentAt(0), terminalView);

	                    _telnetConnection.ConnectionEventReceiver.NormalTermination += ConnectionEventReceiver_NormalTermination;
	                    _telnetConnection.ConnectionEventReceiver.AbnormalTermination += ConnectionEventReceiver_AbnormalTermination;

                        ParentForm.Closing += ParentForm_OnClosing;
						connection.TerminalOutput.Resize(session.Terminal.GetDocument().TerminalWidth, session.Terminal.GetDocument().TerminalHeight);

	                    OnConnected(_terminal, null);
                    }));
	    }

	    private void ParentForm_OnClosing(object sender, CancelEventArgs cancelEventArgs)
	    {
	        if (_telnetConnection != null && _telnetConnection.Socket != null)
	        {
	            _parentFormClosing = true;
	            _telnetConnection.Close();
                _telnetConnection.Socket.Close();
	        }
	    }

	    private void ConnectionEventReceiver_AbnormalTermination(object sender, AbnormalTerminationEventArgs e)
        {
            if (!_parentFormClosing)
                Invoke(new Action(() => OnConnectionLost(this, new ErrorEventArgs(new Exception(e.Message)))));
        }

        private void ConnectionEventReceiver_NormalTermination(object sender, EventArgs e)
        {
            if (!_parentFormClosing)
                Invoke(new Action(() => ParentForm.Close()));
        }

        public void ConnectionFailed(string message)
	    {
	        if (!_parentFormClosing)
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