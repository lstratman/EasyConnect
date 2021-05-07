using MarcusW.VncClient;
using MarcusW.VncClient.Protocol.Implementation.Services.Transports;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Win32Interop.Enums;
using EasyConnect.Common;

namespace EasyConnect.Protocols.Vnc
{
    /// <summary>
    /// UI that displays a VNC connection via the <see cref="VncControl"/> class.
    /// </summary>
    public partial class VncConnectionForm : BaseConnectionForm<VncConnection>
	{
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool AddClipboardFormatListener(IntPtr hwnd);

		protected VncClient _vncClient = null;
		protected RfbConnection _vncConnection = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public VncConnectionForm()
		{
			InitializeComponent();
			AddClipboardFormatListener(Handle);

			_vncClient = new VncClient(Logging.Factory);
		}

        protected override void WndProc(ref Message m)
        {
			if (m.Msg == (int)WM.WM_CLIPBOARDUPDATE)
            {
				if (Connection != null && Connection.ShareClipboard && _vncConnection != null && _vncConnection.ConnectionState == ConnectionState.Connected)
                {
					// TODO: figure out how to fill the clipboard
					//_vncConnection.FillServerClipboard();
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
		    OnResize(null);

		    ParentForm.Closing += VncConnectionForm_FormClosing;

			_vncDesktop.Width = ClientSize.Width;
			_vncDesktop.Height = ClientSize.Height;

			_vncClient.ConnectAsync(new ConnectParameters
			{
				TransportParameters = new TcpTransportParameters
                {
					Host = Connection.Host,
					Port = Connection.Port + Connection.Display
                },
				AuthenticationHandler = new VncAuthenticationHandler(GetPassword()),
				InitialRenderTarget = _vncDesktop
			}).ContinueWith(connectionTask =>
			{
				if (connectionTask.IsFaulted)
				{
					Invoke(new Action(() =>
					{
						OnConnectionLost(this, new ErrorEventArgs(connectionTask.Exception));
					}));
				}

				else
				{
					_vncConnection = connectionTask.Result;
					// TODO: wire up connection lost handlers

					Invoke(new Action(() =>
					{
						OnConnected(this, null);
					}));
				}
			});
		}

        /// <summary>
        /// Decrypts the password (if any) associated with the connection (<see cref="BaseConnection.InheritedPassword"/> and provides it to 
        /// <see cref="_vncConnection"/> for use in the connection process.
        /// </summary>
        /// <returns>The decrypted contents, if any, of <see cref="BaseConnection.InheritedPassword"/>.</returns>
        private string GetPassword()
		{
			string password = null;
			SecureString inheritedPassword = Connection.InheritedPassword;

		    if (inheritedPassword != null && inheritedPassword.Length > 0)
		        password = Marshal.PtrToStringAnsi(Marshal.SecureStringToGlobalAllocAnsi(inheritedPassword));

			return password;
		}

	    private void VncConnectionForm_FormClosing(object sender, CancelEventArgs e)
	    {
	        if (_vncConnection.ConnectionState == ConnectionState.Connected)
	        {
				try
				{
					_vncConnection.CloseAsync().ContinueWith(task =>
					{
						_vncConnection.Dispose();
					});
				}

				catch (Exception)
                {
                }
	        }
        }

	    protected override void OnConnected(object sender, EventArgs e)
	    {
	        base.OnConnected(sender, e);

            _vncDesktop.Left = Math.Max(ClientSize.Width - _vncConnection.RemoteFramebufferSize.Width, 0) / 2;
	        _vncDesktop.Top = Math.Max(ClientSize.Height - _vncConnection.RemoteFramebufferSize.Height, 0) / 2;

			_vncDesktop.Width = _vncConnection.RemoteFramebufferSize.Width;
			_vncDesktop.Height = _vncConnection.RemoteFramebufferSize.Height;
		}
    }
}