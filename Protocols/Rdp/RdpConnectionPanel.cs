using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Windows.Forms;
using AxMSTSCLib;

namespace EasyConnect.Protocols.Rdp
{
    public class RdpConnectionPanel : BaseConnectionPanel, IConnectionPanel<RdpConnection>
    {
        protected bool _connectClipboard = false;
        protected AxMsRdpClient2 _rdpWindow = new AxMsRdpClient2();

        public RdpConnectionPanel()
        {
            MemoryStream stream = new MemoryStream(Convert.FromBase64String("AAEAAAD/////AQAAAAAAAAAMAgAAAFdTeXN0ZW0uV2luZG93cy5Gb3JtcywgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkFAQAAACFTeXN0ZW0uV2luZG93cy5Gb3Jtcy5BeEhvc3QrU3RhdGUBAAAABERhdGEHAgIAAAAJAwAAAA8DAAAAKQAAAAIBAAAAAQAAAAAAAAAAAAAAABQAAAAAAwAACAACAAAAAAALAAAACwAAAAs="));
            BinaryFormatter formatter = new BinaryFormatter();

            ((ISupportInitialize)_rdpWindow).BeginInit();
            _rdpWindow.Enabled = true;
            _rdpWindow.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _rdpWindow.Location = new Point(-1, -1);
            _rdpWindow.Name = "_rdpWindow";
            _rdpWindow.OcxState = _rdpWindow.OcxState = formatter.Deserialize(stream) as AxHost.State;
            _rdpWindow.TabIndex = 0;
            _rdpWindow.OnDisconnected += _rdpWindow_OnDisconnected;

            Controls.Add(_rdpWindow);
            ((ISupportInitialize)_rdpWindow).EndInit();
        }

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

        public SecureString Password
        {
            set
            {
                IntPtr password = Marshal.SecureStringToGlobalAllocAnsi(value);
                _rdpWindow.AdvancedSettings3.ClearTextPassword = Marshal.PtrToStringAnsi(password);
            }
        }

        public override bool Focused
        {
            get
            {
                return _rdpWindow.Focused;
            }
        }

        public AudioMode AudioMode
        {
            get
            {
                return (AudioMode)_rdpWindow.SecuredSettings2.AudioRedirectionMode;
            }

            set
            {
                _rdpWindow.SecuredSettings2.AudioRedirectionMode = (int)value;
            }
        }

        public KeyboardMode KeyboardMode
        {
            get
            {
                return (KeyboardMode)_rdpWindow.SecuredSettings2.KeyboardHookMode;
            }

            set
            {
                _rdpWindow.SecuredSettings2.KeyboardHookMode = (int)value;
            }
        }

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

        public bool ConnectClipboard
        {
            get
            {
                return _connectClipboard;
            }

            set
            {
                _rdpWindow.AdvancedSettings.DisableRdpdr = (!(value || ConnectPrinters)
                                                                ? 1
                                                                : 0);
                _connectClipboard = value;
            }
        }

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

        public bool ConnectToAdminChannel
        {
            get
            {
                return _rdpWindow.AdvancedSettings3.ConnectToServerConsole;
            }

            set
            {
                _rdpWindow.AdvancedSettings3.ConnectToServerConsole = value;
            }
        }

        public override void Connect()
        {
            _rdpWindow.Size = new Size(Size.Width + 2, Size.Height + 2);

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

            if (!String.IsNullOrEmpty(Connection.Username))
                Username = Connection.Username;

            if (Connection.Password != null && Connection.Password.Length > 0)
                Password = Connection.Password;

            Host = Connection.Host;

            _rdpWindow.ConnectingText = "Connecting...";
            _rdpWindow.OnConnected += Connected;
            _rdpWindow.Connect();
        }

        public override event EventHandler Connected;

        private void _rdpWindow_OnDisconnected(object sender, IMsTscAxEvents_OnDisconnectedEvent e)
        {
            if (e.discReason > 3)
                MessageBox.Show("Unable to establish connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            ParentForm.Close();
        }

        public RdpConnection Connection
        {
            get;
            set;
        }
    }
}
