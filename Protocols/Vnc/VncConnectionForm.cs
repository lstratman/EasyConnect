using MarcusW.VncClient;
using MarcusW.VncClient.Protocol.Implementation.Services.Transports;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Win32Interop.Enums;
using EasyConnect.Common;
using System.Threading;
using MarcusW.VncClient.Output;
using System.Media;
using MarcusW.VncClient.Protocol.Implementation.MessageTypes.Outgoing;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
#if INTEL_AMT
using Intel.Manageability;
using PowerState = Intel.Manageability.Power.PowerState;
#endif

namespace EasyConnect.Protocols.Vnc
{
    /// <summary>
    /// UI that displays a VNC connection via the <see cref="VncControl"/> class.
    /// </summary>
    public partial class VncConnectionForm : BaseConnectionForm<VncConnection>, IOutputHandler
	{
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool AddClipboardFormatListener(IntPtr hwnd);

		protected VncClient _vncClient = null;
		protected RfbConnection _vncConnection = null;
		protected CancellationTokenSource _connectionCancellation = null;
		protected VncProtocolImplementation _vncProtocol = null;
#if INTEL_KVM
		private AmtKvmProxyProcess _amtProxy;
		private string _amtProxyError;
#endif

		protected List<ToolStripItem> _toolsMenuItems = new List<ToolStripItem>();
        private bool _scaleToFit = false;
        private ToolStripMenuItem _scaleToFitMenuItem;
        private ToolStripMenuItem _originalSizeMenuItem;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public VncConnectionForm()
		{
			InitializeComponent();
			AddClipboardFormatListener(Handle);

			_vncProtocol = new VncProtocolImplementation();
			_vncClient = new VncClient(Logging.Factory, _vncProtocol);
			Disposed += async (sender, args) =>
			{
				_connectionCancellation?.Cancel();
#if INTEL_KVM
				await StopAmtProxyAsync();
#endif
				_connectionCancellation?.Dispose();
				_connectionCancellation = null;
			};

			Resize += VncConnectionForm_Resize;
		}

        private void VncConnectionForm_Resize(object sender, EventArgs e)
        {
            ApplyDisplayLayout();
        }

        private void ApplyDisplayLayout()
        {
            if (_vncConnection == null || _vncConnection.ConnectionState != ConnectionState.Connected)
                return;

            if (_scaleToFit)
            {
                _vncDesktop.ScaleToFit = true;
                _vncDesktop.Location = Point.Empty;
                _vncDesktop.Size = ClientSize;
                _vncDesktop.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                AutoScroll = false;
            }
            else
            {
                _vncDesktop.ScaleToFit = false;
                _vncDesktop.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                _vncDesktop.Size = new System.Drawing.Size(
                    _vncConnection.RemoteFramebufferSize.Width,
                    _vncConnection.RemoteFramebufferSize.Height);
                _vncDesktop.Left = Math.Max(ClientSize.Width - _vncDesktop.Width, 0) / 2;
                _vncDesktop.Top = Math.Max(ClientSize.Height - _vncDesktop.Height, 0) / 2;
                AutoScroll = true;
            }

            _vncDesktop.Invalidate();
        }

        private void SetScaleToFit(bool enabled)
        {
            _scaleToFit = enabled;
            if (_scaleToFitMenuItem != null)
                _scaleToFitMenuItem.Checked = enabled;
            if (_originalSizeMenuItem != null)
                _originalSizeMenuItem.Checked = !enabled;
            ApplyDisplayLayout();
        }

        protected override void WndProc(ref Message m)
        {
			if (m.Msg == (int)WM.WM_CLIPBOARDUPDATE)
            {
				if (Connection != null && Connection.ShareClipboard && _vncConnection != null && _vncConnection.ConnectionState == ConnectionState.Connected && Clipboard.ContainsText())
                {
					string text = Clipboard.GetText();

					if (!String.IsNullOrEmpty(text))
					{
						_vncConnection.EnqueueMessage(new ClientCutTextMessage(text));
					}
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Control instance that hosts the actual VNC display UI.
        /// </summary>
        protected override Control ConnectionWindow
		{
			get
			{
				return _vncDesktop;
			}
		}

		/// <summary>
		/// Establishes the connection to the remote server; initializes this <see cref="_vncConnection"/>'s properties from 
		/// <see cref="BaseConnectionForm{T}.Connection"/> and then calls <see cref="VncClient.Connect(string, int, VncClientConnectOptions)"/> on 
		/// <see cref="_vncConnection"/>.
		/// </summary>
		public override void Connect()
		{
			if (IsClosing || IsDisposed)
				return;

			OnResize(null);
			ParentForm.Closing += VncConnectionForm_FormClosing;
			_vncDesktop.Reset();
			_vncDesktop.Width = ClientSize.Width;
			_vncDesktop.Height = ClientSize.Height;
			_connectionCancellation?.Cancel();
			_connectionCancellation?.Dispose();
			_connectionCancellation = new CancellationTokenSource();
			if (!String.IsNullOrEmpty(Connection.PreferredEncoding))
				_vncProtocol.PreferredEncodingType = Connection.PreferredEncoding;
			ConnectAsync(_connectionCancellation.Token);
		}

		private async void ConnectAsync(CancellationToken cancellationToken)
		{
			try
			{
				string host = Connection.Host;
				int port = Connection.Port + Connection.Display;
#if INTEL_KVM
				if (Connection.UseAmtKvm)
				{
					bool hasUsername = !String.IsNullOrEmpty(Connection.AmtUsername);
					bool hasDomain = !String.IsNullOrEmpty(Connection.AmtDomain);
					bool hasPassword = Connection.AmtPassword != null && Connection.AmtPassword.Length > 0;
					if (hasDomain && !hasUsername)
						throw new InvalidOperationException("Enter an AMT username when a Kerberos domain is specified.");
					if (hasUsername != hasPassword)
						throw new InvalidOperationException("Enter both an AMT username and password, or leave all AMT credentials empty to use the current Windows identity.");
					SetConnectingStatus("Checking AMT power status...");
					bool poweredUp;
					using (SecureString password = Connection.AmtPassword == null ? null : Connection.AmtPassword.Copy())
						poweredUp = await Task.Run(() => CheckAmtPower(host, Connection.AmtUsername, Connection.AmtDomain, password, Connection.AmtUseTls, cancellationToken, SetConnectingStatus), cancellationToken);
					if (poweredUp)
					{
						SetConnectingStatus("Waiting for the AMT system to start...");
						await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
					}
					cancellationToken.ThrowIfCancellationRequested();
					SetConnectingStatus("Starting the Intel AMT KVM redirection proxy...");
					ushort amtPort = (ushort)(Connection.AmtUseTls ? 16995 : 16994);
					_amtProxy = await AmtKvmProxyProcess.StartAsync(host, amtPort, Connection.AmtUsername, Connection.AmtDomain, Connection.AmtPassword, cancellationToken);
					_amtProxy.StatusChanged += AmtProxy_StatusChanged;
					_amtProxy.Failed += AmtProxy_Failed;
					_amtProxyError = null;
					host = "127.0.0.1";
					port = _amtProxy.LocalPort;
				}
#endif
#if !INTEL_KVM
				if (Connection.UseAmtKvm)
					throw new InvalidOperationException("Intel AMT KVM support is not available in this build. Disable AMT KVM in the connection settings or use a build with Intel AMT support.");
#endif
				SetConnectingStatus("Connecting via VNC...");
				_vncConnection = await _vncClient.ConnectAsync(new ConnectParameters
				{
					TransportParameters = new TcpTransportParameters { Host = host, Port = port },
					AuthenticationHandler = new VncAuthenticationHandler(GetPassword()),
					InitialRenderTarget = _vncDesktop,
					JpegQualityLevel = Connection.PictureQuality * 10,
					InitialOutputHandler = this,
					MaxReconnectAttempts = 0
				}, cancellationToken);
				_vncDesktop.RfbConnection = _vncConnection;
				_vncDesktop.VncConnection = Connection;
				_vncConnection.PropertyChanged += _vncConnection_PropertyChanged;
				OnConnected(this, null);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
#if INTEL_KVM
				if (!String.IsNullOrEmpty(_amtProxyError) && !IsClosing && !IsDisposed && Parent != null)
					OnConnectionLost(this, new ErrorEventArgs(new InvalidOperationException(_amtProxyError)));
#endif
			}
			catch (Exception ex)
			{
				Debug.WriteLine("VNC connection failed: " + ex);
				if (!IsClosing && !IsDisposed && Parent != null)
					OnConnectionLost(this, new ErrorEventArgs(ex));
			}
			finally
			{
#if INTEL_KVM
				if (_vncConnection == null)
					await StopAmtProxyAsync();
#endif
			}
		}

#if INTEL_AMT
		private static bool CheckAmtPower(string host, string username, string domain, SecureString password, bool useTls, CancellationToken cancellation, Action<string> statusChanged)
		{
			cancellation.ThrowIfCancellationRequested();
			bool useKerberos = !String.IsNullOrEmpty(domain) || String.IsNullOrEmpty(username);
			ConnectionInfoEX.AuthMethod authentication = useKerberos ? ConnectionInfoEX.AuthMethod.Kerberos : ConnectionInfoEX.AuthMethod.Digest;
			string amtUsername = String.IsNullOrEmpty(domain) ? username : domain + "\\" + username;
			ConnectionInfoEX info = new ConnectionInfoEX(host, amtUsername, password, useTls, String.Empty, authentication, null, null, null);
			statusChanged("Connecting to AMT...");
			using (IAMTInstance amt = AMTInstanceFactory.CreateEX(info))
			{
				cancellation.ThrowIfCancellationRequested();
				statusChanged("Checking AMT power status...");
				PowerState state = amt.Power.CurrentOSPowerState.PowerState;
				cancellation.ThrowIfCancellationRequested();
				switch (state)
				{
					case PowerState.On: return false;
					case PowerState.StandBy:
					case PowerState.Hibernate:
					case PowerState.Off:
						statusChanged("Powering on the AMT system...");
						amt.Power.Powerup();
						return true;
					default: throw new InvalidOperationException("Unable to determine AMT power state: " + state);
				}
			}
		}
#endif

#if INTEL_KVM
		private void AmtProxy_StatusChanged(int status)
		{
			Debug.WriteLine("Intel AMT KVM proxy status: " + status);
			AmtProxy_Failed(new InvalidOperationException(GetAmtProxyStatusMessage(status)));
		}

		private void AmtProxy_Failed(Exception exception)
		{
			Debug.WriteLine("Intel AMT KVM proxy failed: " + exception);
			_amtProxyError = exception.Message;
			CancellationTokenSource cancellation = _connectionCancellation;
			if (cancellation != null)
				cancellation.Cancel();
			if (_vncConnection != null && _vncConnection.ConnectionState == ConnectionState.Connected && !IsClosing && !IsDisposed && Parent != null)
				OnConnectionLost(this, new ErrorEventArgs(exception));
		}

		private static string GetAmtProxyStatusMessage(int status)
		{
			switch (status)
			{
				case 2: return "Intel AMT session manager failed.";
				case 3: return "The Intel AMT TLS connection failed. Check the trusted certificate and host name.";
				case 4: return "The Intel AMT proxy connection failed.";
				case 5: return "Intel AMT proxy authentication failed.";
				case 6: return "The Intel AMT audit log is full or locked.";
				case 7: return "The Intel AMT connection settings are invalid.";
				case 8: return "Intel AMT authentication failed. Check the credentials and provisioning.";
				case 9: return "The Intel AMT KVM session is busy.";
				case 10: return "The Intel AMT KVM redirection is not supported.";
				default: return "The Intel AMT KVM connection failed (status " + status + ").";
			}
		}

		private async Task StopAmtProxyAsync()
		{
			AmtKvmProxyProcess proxy = _amtProxy;
			_amtProxy = null;
			if (proxy != null)
			{
				proxy.StatusChanged -= AmtProxy_StatusChanged;
				proxy.Failed -= AmtProxy_Failed;
				await proxy.StopAsync();
			}
		}
#endif

		private void _vncConnection_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RfbConnection.ConnectionState))
			{
				if (_vncConnection.ConnectionState != ConnectionState.Connected)
				{
					Debug.WriteLine("VNC connection state: " + _vncConnection.ConnectionState + "; interruption: " + _vncConnection.InterruptionCause);
					OnConnectionLost(this, new ErrorEventArgs(_vncConnection.InterruptionCause));
				}
			}
		}

        /// <summary>
        /// Decrypts the password (if any) associated with the connection (<see cref="BaseConnection.InheritedPassword"/> and provides it to 
        /// <see cref="_vncConnection"/> for use in the connection process.
        /// </summary>
        /// <returns>The decrypted contents, if any, of <see cref="BaseConnection.InheritedPassword"/>.</returns>
		private string GetPassword()
		{
			SecureString inheritedPassword = Connection.InheritedPassword;
			if (inheritedPassword == null || inheritedPassword.Length == 0)
				return null;

			IntPtr plaintext = Marshal.SecureStringToGlobalAllocAnsi(inheritedPassword);
			try
			{
				return Marshal.PtrToStringAnsi(plaintext);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocAnsi(plaintext);
			}
		}

	    private async void VncConnectionForm_FormClosing(object sender, CancelEventArgs e)
	    {
	        await CloseConnection();
	    }

		private async Task CloseConnection()
		{
			_connectionCancellation?.Cancel();
			if (_vncConnection != null && _vncConnection.ConnectionState == ConnectionState.Connected)
			{
				try
				{
					await _vncConnection.CloseAsync();
				}
				catch (Exception)
				{
				}
				finally
				{
					try
					{
						_vncConnection.Dispose();
					}
					catch (Exception)
					{
					}
				}
			}
#if INTEL_KVM
			await StopAmtProxyAsync();
#endif
		}

		protected override async void OnConnectionLost(object sender, EventArgs e)
		{
			base.OnConnectionLost(sender, e);
			if (InvokeRequired)
				BeginInvoke(new Action(() => BackColor = System.Drawing.Color.White));
			else
				BackColor = System.Drawing.Color.White;
#if INTEL_KVM
			await StopAmtProxyAsync();
#endif
		}

		protected override void OnConnected(object sender, EventArgs e)
		{
			base.OnConnected(sender, e);

			_vncDesktop.Width = _vncConnection.RemoteFramebufferSize.Width;
			_vncDesktop.Height = _vncConnection.RemoteFramebufferSize.Height;

            ApplyDisplayLayout();

			BackColor = System.Drawing.Color.FromArgb(171, 171, 171);
		}

        public void RingBell()
        {
			Invoke(new Action(() =>
			{
				SystemSounds.Beep.Play();
			}));
        }

        public void HandleServerClipboardUpdate(string text)
        {
			Invoke(new Action(() =>
			{
				Clipboard.SetText(text);
			}));
        }

        public override void AddToolsMenuItems(ContextMenuStrip toolsMenu)
        {
            _toolsMenuItems.Add(new ToolStripMenuItem("Send Ctrl+Alt+Del", null, new EventHandler(SendCtrlAltDel)));
            _toolsMenuItems.Add(new ToolStripSeparator());

            var displayMenu = new ToolStripMenuItem("Display scaling");
            _originalSizeMenuItem = new ToolStripMenuItem("Original resolution", null,
                (sender, e) => SetScaleToFit(false));
            _scaleToFitMenuItem = new ToolStripMenuItem("Automatically fit to window", null,
                (sender, e) => SetScaleToFit(true));
            _originalSizeMenuItem.Checked = true;
            displayMenu.DropDownItems.Add(_originalSizeMenuItem);
            displayMenu.DropDownItems.Add(_scaleToFitMenuItem);
            _toolsMenuItems.Add(displayMenu);

            toolsMenu.Items.Insert(0, _toolsMenuItems[0]);
            toolsMenu.Items.Insert(1, _toolsMenuItems[1]);
            toolsMenu.Items.Insert(2, _toolsMenuItems[2]);
		}

		protected void SendCtrlAltDel(object sender, EventArgs e)
        {
			if (_vncConnection != null && _vncConnection.ConnectionState == ConnectionState.Connected)
            {
				_vncConnection.EnqueueMessage(new KeyEventMessage(true, KeySymbol.Alt_L));
				_vncConnection.EnqueueMessage(new KeyEventMessage(true, KeySymbol.Control_L));
				_vncConnection.EnqueueMessage(new KeyEventMessage(true, KeySymbol.Delete));

				_vncConnection.EnqueueMessage(new KeyEventMessage(false, KeySymbol.Alt_L));
				_vncConnection.EnqueueMessage(new KeyEventMessage(false, KeySymbol.Control_L));
				_vncConnection.EnqueueMessage(new KeyEventMessage(false, KeySymbol.Delete));
			}
        }

        public override void RemoveToolsMenuItems(ContextMenuStrip toolsMenu)
        {
            foreach (ToolStripItem item in _toolsMenuItems)
            {
				toolsMenu.Items.Remove(item);
            }

			_toolsMenuItems.Clear();
        }
    }
}