using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using AxMSTSCLib;

namespace EasyConnect.Protocols.Rdp
{
	/// <summary>
	/// UI that displays a Microsoft Remote Desktop (RDP) connection via the built-in <see cref="AxMsRdpClient2"/> class.
	/// </summary>
	public partial class RdpConnectionForm : BaseConnectionForm<RdpConnection>
	{
		/// <summary>
		/// Flag indicating whether the remote session should use the local clipboard.
		/// </summary>
		protected bool _connectClipboard = false;

		/// <summary>
		/// Flag indicating if the RDP window's size has been set explicitly.
		/// </summary>
		protected bool _rdpWindowSizeSet = false;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public RdpConnectionForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Host name of the server that we are to connect to.
		/// </summary>
		public string Host
		{
			get
			{
				return _rdpWindow.Server;
			}

			set
			{
				Text = value;
				_rdpWindow.Server = value;
			}
		}

		/// <summary>
		/// Width of the resulting desktop.  0 means that it should fill the available window space.
		/// </summary>
		public int DesktopWidth
		{
			get
			{
				return _rdpWindow.DesktopWidth;
			}

			set
			{
				_rdpWindow.DesktopWidth = value;
			}
		}

		/// <summary>
		/// Height of the resulting desktop.  0 means that it should fill the available window space.
		/// </summary>
		public int DesktopHeight
		{
			get
			{
				return _rdpWindow.DesktopHeight;
			}

			set
			{
				_rdpWindow.DesktopHeight = value;
			}
		}

		/// <summary>
		/// Username, if any, that should be presented when establishing the connection.
		/// </summary>
		public string Username
		{
			get
			{
				return _rdpWindow.UserName;
			}

			set
			{
				_rdpWindow.UserName = value;
			}
		}

		/// <summary>
		/// Password, if any, used when establishing this connection.
		/// </summary>
		public SecureString Password
		{
			set
			{
				IntPtr password = Marshal.SecureStringToGlobalAllocAnsi(value);
				_rdpWindow.AdvancedSettings3.ClearTextPassword = Marshal.PtrToStringAnsi(password);
			}
		}

		/// <summary>
		/// Flag indicating whether the connection window is currently focused.
		/// </summary>
		public override bool Focused
		{
			get
			{
				return _rdpWindow.Focused;
			}
		}

		/// <summary>
		/// Where sounds originating from the remote server should be played (locally or remotely).
		/// </summary>
		public AudioMode AudioMode
		{
			get
			{
				return (AudioMode) _rdpWindow.SecuredSettings2.AudioRedirectionMode;
			}

			set
			{
				_rdpWindow.SecuredSettings2.AudioRedirectionMode = (int) value;
			}
		}

		/// <summary>
		/// Which system (locally or remotely) Windows shortcut keys like Alt+Tab should be directed to.
		/// </summary>
		public KeyboardMode KeyboardMode
		{
			get
			{
				return (KeyboardMode) _rdpWindow.SecuredSettings2.KeyboardHookMode;
			}

			set
			{
				_rdpWindow.SecuredSettings2.KeyboardHookMode = (int) value;
			}
		}

		/// <summary>
		/// Flag indicating whether the remote system should connect to local printers.
		/// </summary>
		public bool ConnectPrinters
		{
			get
			{
				return _rdpWindow.AdvancedSettings2.RedirectPrinters;
			}

			set
			{
				_rdpWindow.AdvancedSettings2.RedirectPrinters = value;
				_rdpWindow.AdvancedSettings2.DisableRdpdr = (!(value || ConnectClipboard)
					                                             ? 1
					                                             : 0);
			}
		}

		/// <summary>
		/// Flag indicating whether the remote session should use the local clipboard.
		/// </summary>
		public bool ConnectClipboard
		{
			get
			{
				return _connectClipboard;
			}

			set
			{
				_rdpWindow.AdvancedSettings6.RedirectClipboard = value;
				_rdpWindow.AdvancedSettings.DisableRdpdr = (!(value || ConnectPrinters)
					                                            ? 1
					                                            : 0);
				_connectClipboard = value;
			}
		}

		/// <summary>
		/// Flag indicating whether the remote system should map network drives to the user's local hard drive instances.
		/// </summary>
		public bool ConnectDrives
		{
			get
			{
				return _rdpWindow.AdvancedSettings2.RedirectDrives;
			}

			set
			{
				_rdpWindow.AdvancedSettings2.RedirectDrives = value;
			}
		}

		/// <summary>
		/// Flag indicating whether we should display the remote desktop background.
		/// </summary>
		public bool DesktopBackground
		{
			get
			{
				return (_rdpWindow.AdvancedSettings2.PerformanceFlags & 0x00000001) != 0x00000001;
			}

			set
			{
				if (value)
					_rdpWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000001;

				else
					_rdpWindow.AdvancedSettings2.PerformanceFlags |= 0x00000001;
			}
		}

		/// <summary>
		/// Flag indicating whether we should use font smoothing when rendering text from the remote system.
		/// </summary>
		public bool FontSmoothing
		{
			get
			{
				return (_rdpWindow.AdvancedSettings2.PerformanceFlags & 0x00000080) != 0x00000080;
			}

			set
			{
				if (value)
					_rdpWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000080;

				else
					_rdpWindow.AdvancedSettings2.PerformanceFlags |= 0x00000080;
			}
		}

		/// <summary>
		/// Flag indicating whether advanced visual effects like Aero Glass should be enabled.
		/// </summary>
		public bool DesktopComposition
		{
			get
			{
				return (_rdpWindow.AdvancedSettings2.PerformanceFlags & 0x00000100) != 0x00000100;
			}

			set
			{
				if (value)
					_rdpWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000100;

				else
					_rdpWindow.AdvancedSettings2.PerformanceFlags |= 0x00000100;
			}
		}

		/// <summary>
		/// Flag indicating whether a window's contents should be displayed while it is being dragged around the screen.
		/// </summary>
		public bool WindowContentsWhileDragging
		{
			get
			{
				return (_rdpWindow.AdvancedSettings2.PerformanceFlags & 0x00000100) != 0x00000100;
			}

			set
			{
				if (value)
					_rdpWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000100;

				else
					_rdpWindow.AdvancedSettings2.PerformanceFlags |= 0x00000100;
			}
		}

		/// <summary>
		/// Flag indicating whether we should animate the showing and hiding of menus.
		/// </summary>
		public bool Animations
		{
			get
			{
				return (_rdpWindow.AdvancedSettings2.PerformanceFlags & 0x00000004) != 0x00000004;
			}

			set
			{
				if (value)
					_rdpWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000004;

				else
					_rdpWindow.AdvancedSettings2.PerformanceFlags |= 0x00000004;
			}
		}

		/// <summary>
		/// Flag indicating whether the Windows Basic theme should be used when displaying the user's desktop.
		/// </summary>
		public bool VisualStyles
		{
			get
			{
				return (_rdpWindow.AdvancedSettings2.PerformanceFlags & 0x00000008) != 0x00000008;
			}

			set
			{
				if (value)
					_rdpWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000008;

				else
					_rdpWindow.AdvancedSettings2.PerformanceFlags |= 0x00000008;
			}
		}

		/// <summary>
		/// Flag indicating whether we should use bitmap caching during the rendering process.
		/// </summary>
		public bool PersistentBitmapCaching
		{
			get
			{
				return _rdpWindow.AdvancedSettings2.CachePersistenceActive != 0;
			}

			set
			{
				_rdpWindow.AdvancedSettings2.CachePersistenceActive = (value
					                                                       ? 1
					                                                       : 0);
			}
		}

		/// <summary>
		/// Flag indicating whether we should connect to the admin channel (the local, physical desktop display) when establishing a connection.
		/// </summary>
		public bool ConnectToAdminChannel
		{
			get
			{
				return _rdpWindow.AdvancedSettings3.ConnectToServerConsole;
			}

			set
			{
				_rdpWindow.AdvancedSettings3.ConnectToServerConsole = value;
				_rdpWindow.AdvancedSettings7.ConnectToAdministerServer = value;
			}
		}

        /// <summary>
        /// Flag indicating whether we should connect with a TS proxy.
        /// </summary>
        public bool UseTSProxy
        {
            get
            {
                return _rdpWindow.TransportSettings.GatewayUsageMethod == 1;
            }

            set
            {
                if (value)
                {
                    _rdpWindow.TransportSettings.GatewayUsageMethod = 1; //TSC_PROXY_MODE_DIRECT
                }
                else
                {
                    _rdpWindow.TransportSettings.GatewayUsageMethod = 0; //TSC_PROXY_MODE_NONE_DIRECT
                }
            }
        }

        /// <summary>
        /// The host name for the proxy server.
        /// </summary>
        public string ProxyName
        {
            get
            {
                return _rdpWindow.TransportSettings.GatewayHostname;
            }

            set
            {
                if (value == null || String.IsNullOrWhiteSpace(value))
                {
                    _rdpWindow.TransportSettings.GatewayHostname = "";
                }
                else
                {
                    _rdpWindow.TransportSettings.GatewayHostname = value;
                }
            }
        }

        /// <summary>
        /// The user name for the proxy server.
        /// </summary>
        public string ProxyUserName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_rdpWindow.TransportSettings2.GatewayDomain))
                {
                    return _rdpWindow.TransportSettings2.GatewayUsername;
                }
                else
                {
                    return _rdpWindow.TransportSettings2.GatewayDomain + '\\' + _rdpWindow.TransportSettings2.GatewayUsername;
                }
            }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    _rdpWindow.TransportSettings2.GatewayDomain = "";
                    _rdpWindow.TransportSettings2.GatewayUsername = "";
                }
                else if (value.Contains("\\"))
                {
                    string[] str = value.Split(
                        new char[]
                        {
                            '\\'
                        });
                    _rdpWindow.TransportSettings2.GatewayDomain = str[0];
                    _rdpWindow.TransportSettings2.GatewayUsername = str[1];
                }
                else
                {
                    _rdpWindow.TransportSettings2.GatewayDomain = "";
                    _rdpWindow.TransportSettings2.GatewayUsername = value;
                }
            }
        }

		/// <summary>
		/// The password for the proxy server.
		/// </summary>
		public SecureString ProxyPassword
		{
			set
			{
                if (value == null || value.Length == 0)
                {
                    _rdpWindow.TransportSettings2.GatewayPassword = "";
                }
                else
                {
                    IntPtr password = Marshal.SecureStringToGlobalAllocAnsi(value);
                    _rdpWindow.TransportSettings2.GatewayPassword = Marshal.PtrToStringAnsi(password);
                }
			}
		}
        
        /// <summary>
		/// Control instance that hosts the actual remote desktop UI.
		/// </summary>
		protected override Control ConnectionWindow
		{
			get
			{
				return _rdpWindow;
			}
		}

		/// <summary>
		/// Establishes the connection to the remote server; initializes this <see cref="_rdpWindow"/>'s properties from 
		/// <see cref="BaseConnectionForm{T}.Connection"/> and then calls <see cref="AxMsRdpClient2.Connect"/> on <see cref="_rdpWindow"/>.
		/// </summary>
		public override void Connect()
		{
			if (!_rdpWindowSizeSet)
			{
				_rdpWindow.Size = new Size(Size.Width, Size.Height);
				_rdpWindowSizeSet = true;
			}

			DesktopWidth = (Connection.DesktopWidth == 0
				                ? _rdpWindow.Width
				                : Connection.DesktopWidth);
			DesktopHeight = (Connection.DesktopHeight == 0
				                 ? _rdpWindow.Height - 1
				                 : Connection.DesktopHeight);
			AudioMode = Connection.AudioMode;
			KeyboardMode = Connection.KeyboardMode;
			ConnectPrinters = Connection.ConnectPrinters;
			ConnectClipboard = Connection.ConnectClipboard;
			ConnectDrives = Connection.ConnectDrives;
			DesktopBackground = Connection.DesktopBackground;
			FontSmoothing = Connection.FontSmoothing;
			DesktopComposition = Connection.DesktopComposition;
			WindowContentsWhileDragging = Connection.WindowContentsWhileDragging;
			Animations = Connection.Animations;
			VisualStyles = Connection.VisualStyles;
			PersistentBitmapCaching = Connection.PersistentBitmapCaching;
			ConnectToAdminChannel = Connection.ConnectToAdminChannel;

			if (!String.IsNullOrEmpty(Connection.InheritedUsername))
				Username = Connection.InheritedUsername;

			SecureString password = Connection.InheritedPassword;

			if (password != null && password.Length > 0)
				Password = password;

			Host = Connection.Host;

			_rdpWindow.AdvancedSettings3.EnableAutoReconnect = false;
			_rdpWindow.Visible = false;
			_rdpWindow.OnConnected += OnConnected;

			// support NLA
			_rdpWindow.AdvancedSettings5.AuthenticationLevel = 2;
			_rdpWindow.AdvancedSettings7.EnableCredSspSupport = true;

			_rdpWindow.TransportSettings.GatewayProfileUsageMethod = 1; // TSC_PROXY_PROFILE_MODE_EXPLICIT
			_rdpWindow.TransportSettings.GatewayCredsSource = 0; // TSC_PROXY_CREDS_MODE_USERPASS
			_rdpWindow.TransportSettings2.GatewayCredSharing = 0;
			UseTSProxy = Connection.UseTSProxy;
			ProxyName = Connection.ProxyName;
			ProxyUserName = Connection.ProxyUserName;
			ProxyPassword = Connection.ProxyPassword;

			_rdpWindow.Connect();
		}

		/// <summary>
		/// Handler method that's called when an established connection is broken.  Displays an error message to the user if this was not the result of a
		/// logoff process.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_rdpWindow"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _rdpWindow_OnDisconnected(object sender, IMsTscAxEvents_OnDisconnectedEvent e)
		{
			if (e.discReason > 3)
			{
				OnConnectionLost(
					sender, new ErrorEventArgs(new Exception(_rdpWindow.GetErrorDescription((uint) e.discReason, (uint) _rdpWindow.ExtendedDisconnectReason))));
			}

			else
				OnLoggedOff(sender, null);

			_rdpWindowSizeSet = false;
		}

		/// <summary>
		/// Handler method that's called when this control gains focus.  Automatically focuses on <see cref="_rdpWindow"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void RdpConnectionForm_GotFocus(object sender, EventArgs e)
		{
			_rdpWindow.Focus();
		}
	}
}
