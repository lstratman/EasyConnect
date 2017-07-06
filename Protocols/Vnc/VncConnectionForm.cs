using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using RemoteViewing.Vnc;
using RemoteViewing.Windows.Forms;

namespace EasyConnect.Protocols.Vnc
{
	/// <summary>
	/// UI that displays a VNC connection via the <see cref="VncControl"/> class.
	/// </summary>
	public partial class VncConnectionForm : BaseConnectionForm<VncConnection>
	{
        /// <summary>
		/// Default constructor.
		/// </summary>
		public VncConnectionForm()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Control instance that hosts the actual VNC display UI.
        /// </summary>
        protected override Control ConnectionWindow
		{
			get
			{
				return _vncConnection;
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

            VncClientConnectOptions connectionOptions = new VncClientConnectOptions()
            {
                PasswordRequiredCallback = GetPassword
            };

            // Establish the actual connection
            _vncConnection.Connected += OnConnected;
			_vncConnection.ConnectionFailed += OnConnectionLost;
		    _vncConnection.AllowInput = !Connection.ViewOnly;
		    _vncConnection.AllowRemoteCursor = false;
		    _vncConnection.AllowClipboardSharingFromServer = Connection.ShareClipboard;
		    _vncConnection.AllowClipboardSharingToServer = Connection.ShareClipboard;

            // Spin the connection process up on a different thread to avoid blocking the UI.
            Thread connectionThread = new Thread(
				() =>
				{
					try
					{
						_vncConnection.Client.Connect(Connection.Host, Connection.Port + Connection.Display, connectionOptions);
					}

					catch (Exception e)
					{
                        Invoke(new Action(() => OnConnectionLost(this, new ErrorEventArgs(e))));
					}
				});

			connectionThread.Start();
		}

		/// <summary>
		/// Decrypts the password (if any) associated with the connection (<see cref="BaseConnection.InheritedPassword"/> and provides it to 
		/// <see cref="_vncConnection"/> for use in the connection process.
		/// </summary>
		/// <returns>The decrypted contents, if any, of <see cref="BaseConnection.InheritedPassword"/>.</returns>
		private char[] GetPassword(VncClient client)
		{
			string password = null;
			SecureString inheritedPassword = Connection.InheritedPassword;

		    if (inheritedPassword != null && inheritedPassword.Length > 0)
		        password = Marshal.PtrToStringAnsi(Marshal.SecureStringToGlobalAllocAnsi(inheritedPassword));

			return password.ToCharArray();
		}

	    private void VncConnectionForm_FormClosing(object sender, FormClosingEventArgs e)
	    {
	        if (_vncConnection.Client.IsConnected)
	        {
	            _vncConnection.Client.Close();
                _vncConnection.Dispose();
	        }
        }

	    protected override void OnConnected(object sender, EventArgs e)
	    {
	        base.OnConnected(sender, e);

            _vncConnection.Width = _vncConnection.Client.Framebuffer.Width;
	        _vncConnection.Height = _vncConnection.Client.Framebuffer.Height;

            _vncConnection.Left = Math.Max(ClientSize.Width - _vncConnection.Width, 0) / 2;
	        _vncConnection.Top = Math.Max(ClientSize.Height - _vncConnection.Height, 0) / 2;
        }
	}
}