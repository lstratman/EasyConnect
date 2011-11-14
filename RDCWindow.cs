using System;
using System.Windows.Forms;
using System.Security;
using System.Runtime.InteropServices;
using AxMSTSCLib;

namespace EasyConnect
{
    public partial class RDCWindow : Form
    {
        protected MSTSCLib.IMsRdpClientNonScriptable _nonScriptable = null;
        protected bool _connectClipboard = true;
        protected SecureString _password = null;

        public event EventHandler Connected;

        public RDCWindow(SecureString password)
        {
            InitializeComponent();

            _rdcWindow.ConnectingText = "Connecting...";
            _rdcWindow.OnDisconnected += rdcWindow_OnDisconnected;

            _nonScriptable = (MSTSCLib.IMsRdpClientNonScriptable)_rdcWindow.GetOcx();
            Password = password;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _rdcWindow.Focus();
        }

        void rdcWindow_OnDisconnected(object sender, IMsTscAxEvents_OnDisconnectedEvent e)
        {
            if (e.discReason > 3)
                MessageBox.Show("Unable to establish a connection to the remote system.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Close();
        }

        public string Host
        {
            get
            {
                return _rdcWindow.Server;
            }

            set
            {
                Text = value;
                _rdcWindow.Server = value;
            }
        }

        public int DesktopWidth
        {
            get
            {
                return _rdcWindow.DesktopWidth;
            }

            set
            {
                _rdcWindow.DesktopWidth = value;
            }
        }

        public int DesktopHeight
        {
            get
            {
                return _rdcWindow.DesktopHeight;
            }

            set
            {
                _rdcWindow.DesktopHeight = value;
            }
        }

        public string Username
        {
            get
            {
                return _rdcWindow.UserName;
            }

            set
            {
                _rdcWindow.UserName = value;
            }
        }

        public SecureString Password
        {
            get
            {
                return _password;
            }

            set
            {
                IntPtr password = Marshal.SecureStringToGlobalAllocAnsi(value);

                _rdcWindow.AdvancedSettings3.ClearTextPassword = Marshal.PtrToStringAnsi(password);
                _password = value;
            }
        }

        public override bool Focused
        {
            get
            {
                return _rdcWindow.Focused;
            }
        }

        public AudioMode AudioMode
        {
            get
            {
                return (AudioMode)_rdcWindow.SecuredSettings2.AudioRedirectionMode;
            }

            set
            {
                _rdcWindow.SecuredSettings2.AudioRedirectionMode = (int)value;
            }
        }

        public KeyboardMode KeyboardMode
        {
            get
            {
                return (KeyboardMode)_rdcWindow.SecuredSettings2.KeyboardHookMode;
            }

            set
            {
                _rdcWindow.SecuredSettings2.KeyboardHookMode = (int)value;
            }
        }

        public bool ConnectPrinters
        {
            get
            {
                return _rdcWindow.AdvancedSettings2.RedirectPrinters;
            }

            set
            {
                _rdcWindow.AdvancedSettings2.RedirectPrinters = value;
                _rdcWindow.AdvancedSettings2.DisableRdpdr = (!(value || ConnectClipboard) ? 1 : 0);
            }
        }

        public bool ConnectClipboard
        {
            get
            {
                return _connectClipboard;
            }

            set
            {
                _rdcWindow.AdvancedSettings.DisableRdpdr = (!(value || ConnectPrinters) ? 1 : 0);
                _connectClipboard = value;
            }
        }

        public bool ConnectDrives
        {
            get
            {
                return _rdcWindow.AdvancedSettings2.RedirectDrives;
            }

            set
            {
                _rdcWindow.AdvancedSettings2.RedirectDrives = value;
            }
        }

        public bool DesktopBackground
        {
            get
            {
                return !((_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000001) == 0x00000001);
            }

            set
            {
                if (value)
                    _rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000001;

                else
                    _rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000001;
            }
        }

        public bool FontSmoothing
        {
            get
            {
                return !((_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000080) == 0x00000080);
            }

            set
            {
                if (value)
                    _rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000080;

                else
                    _rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000080;
            }
        }

        public bool DesktopComposition
        {
            get
            {
                return !((_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000100) == 0x00000100);
            }

            set
            {
                if (value)
                    _rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000100;

                else
                    _rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000100;
            }
        }

        public bool WindowContentsWhileDragging
        {
            get
            {
                return !((_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000100) == 0x00000100);
            }

            set
            {
                if (value)
                    _rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000100;

                else
                    _rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000100;
            }
        }

        public bool Animations
        {
            get
            {
                return !((_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000004) == 0x00000004);
            }

            set
            {
                if (value)
                    _rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000004;

                else
                    _rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000004;
            }
        }

        public bool VisualStyles
        {
            get
            {
                return !((_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000008) == 0x00000008);
            }

            set
            {
                if (value)
                    _rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000008;

                else
                    _rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000008;
            }
        }

        public bool PersistentBitmapCaching
        {
            get
            {
                return !(_rdcWindow.AdvancedSettings2.CachePersistenceActive == 0);
            }

            set
            {
                _rdcWindow.AdvancedSettings2.CachePersistenceActive = (value ? 1 : 0);
            }
        }

        public void Connect()
        {
            _rdcWindow.Connect();
            _rdcWindow.OnConnected += Connected;
        }

        public void Connect(RDCConnection connection)
        {
            DesktopWidth = (connection.DesktopWidth == 0 ? ClientSize.Width : connection.DesktopWidth);
            DesktopHeight = (connection.DesktopHeight == 0 ? ClientSize.Height : connection.DesktopHeight);
            AudioMode = connection.AudioMode;
            KeyboardMode = connection.KeyboardMode;
            ConnectPrinters = connection.ConnectPrinters;
            ConnectClipboard = connection.ConnectClipboard;
            ConnectDrives = connection.ConnectDrives;
            DesktopBackground = connection.DesktopBackground;
            FontSmoothing = connection.FontSmoothing;
            DesktopComposition = connection.DesktopComposition;
            WindowContentsWhileDragging = connection.WindowContentsWhileDragging;
            Animations = connection.Animations;
            VisualStyles = connection.VisualStyles;
            PersistentBitmapCaching = connection.PersistentBitmapCaching;
            
            if (!String.IsNullOrEmpty(connection.Username))
                Username = connection.Username;

            if (connection.Password != null && connection.Password.Length > 0)
                Password = connection.Password;

            Host = connection.Host;

            if (!String.IsNullOrEmpty(connection.Name))
                Text = connection.Name;

            Connect();
        }

        private void urlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Connect(new RDCConnection(_password)
                            {
                                Host = urlTextBox.Text
                            });
        }
    }
}
