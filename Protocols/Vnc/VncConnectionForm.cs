using System.Runtime.InteropServices;
using System.Security;
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
		/// <see cref="BaseConnectionForm{T}.Connection"/> and then calls <see cref="VncDesktop.Connect(string, int, bool, bool)"/> on <see cref="_vncConnection"/>.
		/// </summary>
		public override void Connect()
		{
			// Set the various properties for the connection
			_vncConnection.VncPort = Connection.Port;
			
			// Establish the actual connection
			_vncConnection.VncUpdated += _vncConnection_VncUpdated;
			_vncConnection.ConnectionLost += OnConnectionLost;
			_vncConnection.GetPassword = GetPassword;

			_vncConnection.Connect(Connection.Host, Connection.Display, Connection.ViewOnly, Connection.Scale);
		}

		void _vncConnection_VncUpdated(object sender, VncEventArgs e)
		{
			if (!IsConnected)
				OnConnected(sender, e);
		}

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