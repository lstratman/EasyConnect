using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;
using AxMSTSCLib;

namespace EasyConnect.Protocols.Rdp
{
    public partial class RdpConnectionForm : BaseConnectionForm<RdpConnection>
    {
        protected bool _connectClipboard = false;

        public RdpConnectionForm()
        {
            InitializeComponent();
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

        protected override Control ConnectionWindow
        {
            get
            {
                return _rdpWindow;
            }
        }

        protected override void OnConnected(object sender, EventArgs e)
        {
            if (KeyboardMode == KeyboardMode.Remotely)
            {
                //IntPtr inputCatpureWindow = Win32Interop.FindWindowEx(Parent.Handle, IntPtr.Zero, "IHWindowClass", "Input Capture Window");


            }

            base.OnConnected(sender, e);
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
            _rdpWindow.OnConnected += OnConnected;
            _rdpWindow.Connect();
        }

        private void _rdpWindow_OnDisconnected(object sender, IMsTscAxEvents_OnDisconnectedEvent e)
        {
            if (e.discReason > 3)
                MessageBox.Show("Unable to establish connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            IsConnected = false;

            if (CloseParentFormOnDisconnect)
                ParentForm.Close();
        }

        void RdpConnectionForm_GotFocus(object sender, EventArgs e)
        {
            _rdpWindow.Focus();
        }
    }
}
