using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using AxViewerX;
using ViewerX;

namespace EasyConnect.Protocols.Vnc
{
	/// <summary>
	/// UI that displays a VNC connection via the <see cref="AxCSC_ViewerXControl"/> class.
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
		/// <see cref="BaseConnectionForm{T}.Connection"/> and then calls <see cref="AxCSC_ViewerXControl.ConnectAsyncEx"/> on <see cref="_vncConnection"/>.
		/// </summary>
		public override void Connect()
		{
			string password = null;
			SecureString inheritedPassword = Connection.InheritedPassword;

			if (inheritedPassword != null && inheritedPassword.Length > 0)
				password = Marshal.PtrToStringAnsi(Marshal.SecureStringToGlobalAllocAnsi(inheritedPassword));

			// Set the various properties for the connection
			_vncConnection.ScaleEnable = Connection.Scale;
			_vncConnection.StretchMode = Connection.Scale
				                             ? ScreenStretchMode.SSM_ASPECT
				                             : ScreenStretchMode.SSM_NONE;
			_vncConnection.ViewOnly = Connection.ViewOnly;
			_vncConnection.Encoding = Connection.EncodingType;
			_vncConnection.ColorDepth = Connection.ColorDepth;
			_vncConnection.LoginType = Connection.AuthenticationType;

			switch (Connection.AuthenticationType)
			{
				case ViewerLoginType.VLT_VNC:
					_vncConnection.Password = password;
					break;

				case ViewerLoginType.VLT_MSWIN:
					_vncConnection.MsUser = Connection.InheritedUsername;
					_vncConnection.MsPassword = password;
					break;
			}

			// Set the encryption type and key file
			_vncConnection.EncryptionPlugin = Connection.EncryptionType;

			switch (Connection.EncryptionType)
			{
				case EncryptionPluginType.EPT_MSRC4:
					_vncConnection.UltraVNCSecurity_MSRC4.KeyStorage = DsmKeyStorage.DKS_FILE;
					_vncConnection.UltraVNCSecurity_MSRC4.KeyFilePath = Connection.KeyFile;
					break;

				case EncryptionPluginType.EPT_SECUREVNC:
					_vncConnection.UltraVNCSecurity_SecureVNC.KeyStorage = DsmKeyStorage.DKS_FILE;
					_vncConnection.UltraVNCSecurity_SecureVNC.PrivateKeyFilePath = Connection.KeyFile;
					break;
			}

			// Establish the actual connection
			_vncConnection.ConnectedEvent += OnConnected;
			_vncConnection.ConnectAsyncEx(Connection.Host, Connection.Port + Connection.Display, password);
		}
	}
}