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

            _vncConnection.MsUser = Connection.InheritedUsername;
            _vncConnection.ScaleEnable = Connection.Scale;
            _vncConnection.StretchMode = Connection.Scale
                                             ? ScreenStretchMode.SSM_ASPECT
                                             : ScreenStretchMode.SSM_NONE;
            _vncConnection.ViewOnly = Connection.ViewOnly;
            _vncConnection.MsPassword = password;
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
