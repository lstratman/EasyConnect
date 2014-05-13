using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using VncSharp;

namespace EasyConnect.Protocols.Vnc
{
	/// <summary>
	/// UI that displays a VNC connection via the <see cref="VncDesktop"/> class.
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
		/// <see cref="BaseConnectionForm{T}.Connection"/> and then calls <see cref="VncDesktop.Connect(string, int, bool, bool)"/> on 
		/// <see cref="_vncConnection"/>.
		/// </summary>
		public override void Connect()
		{
			// Set the various properties for the connection
			_vncConnection.VncPort = Connection.Port;
			
			// Establish the actual connection
			_vncConnection.VncUpdated += _vncConnection_VncUpdated;
			_vncConnection.ConnectionLost += OnConnectionLost;
			_vncConnection.GetPassword = GetPassword;

			// Spin the connection process up on a different thread to avoid blocking the UI.
			Thread connectionThread = new Thread(
				() =>
				{
					try
					{
						_vncConnection.Connect(Connection.Host, Connection.Display, Connection.ViewOnly, Connection.Scale);
					}

					catch (Exception e)
					{
						OnConnectionLost(this, new ErrorEventArgs(e));
					}
				});

			connectionThread.Start();
		}

		/// <summary>
		/// Callback that's invoked whenever the client (<see cref="_vncConnection"/>) receives screen data from the server.  We use this method to determine
		/// when we are *really* connected:  we have successfully established a network connection to the server *and* we have started receiving screen data.
		/// It's only at that point that we actually display the VNC UI.
		/// </summary>
		/// <param name="sender">Object from which this event originated, in this case, <see cref="_vncConnection"/>.</param>
		/// <param name="e">Arguments associated with this event.</param>
		void _vncConnection_VncUpdated(object sender, VncEventArgs e)
		{
			if (!IsConnected)
				OnConnected(sender, e);
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
	}
}