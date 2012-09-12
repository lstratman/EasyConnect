using System;
using System.Runtime.InteropServices;
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

            if (Connection.Password != null)
                password = Marshal.PtrToStringAnsi(Marshal.SecureStringToGlobalAllocAnsi(Connection.Password));

            _vncConnection.MsUser = Connection.Username;
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
