using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using ViewerX;

namespace EasyConnect.Protocols.Vnc
{
    public partial class VncConnectionForm : BaseConnectionForm<VncConnection>
    {
        public VncConnectionForm()
        {
            InitializeComponent();
        }

        public override void Connect()
        {
            string password = null;
            SecureString inheritedPassword = Connection.InheritedPassword;

            if (inheritedPassword != null && inheritedPassword.Length > 0)
                password = Marshal.PtrToStringAnsi(Marshal.SecureStringToGlobalAllocAnsi(inheritedPassword));

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

	        _vncConnection.ConnectedEvent += OnConnected;
            _vncConnection.ConnectAsyncEx(Connection.Host, Connection.Port + Connection.Display, password);
        }

        protected override Control ConnectionWindow
        {
            get
            {
                return _vncConnection;
            }
        }
    }
}
