using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security;
using System.Runtime.InteropServices;
using WeifenLuo.WinFormsUI.Docking;
using AxMSTSCLib;

namespace EasyConnect
{
    public partial class RDCWindow : DockContent
    {
        protected MSTSCLib.IMsRdpClientNonScriptable _nonScriptable = null;
        protected bool _connectClipboard = true;

        public event EventHandler Connected;

        public RDCWindow()
        {
            InitializeComponent();

            rdcWindow.ConnectingText = "Connecting...";
            rdcWindow.OnDisconnected += new IMsTscAxEvents_OnDisconnectedEventHandler(rdcWindow_OnDisconnected);

            _nonScriptable = (MSTSCLib.IMsRdpClientNonScriptable)rdcWindow.GetOcx();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            rdcWindow.Focus();
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
                return rdcWindow.Server;
            }

            set
            {
                Text = value;
                rdcWindow.Server = value;
            }
        }

        public int DesktopWidth
        {
            get
            {
                return rdcWindow.DesktopWidth;
            }

            set
            {
                rdcWindow.DesktopWidth = value;
            }
        }

        public int DesktopHeight
        {
            get
            {
                return rdcWindow.DesktopHeight;
            }

            set
            {
                rdcWindow.DesktopHeight = value;
            }
        }

        public string Username
        {
            get
            {
                return rdcWindow.UserName;
            }

            set
            {
                rdcWindow.UserName = value;
            }
        }

        public SecureString Password
        {
            set
            {
                IntPtr password = Marshal.SecureStringToGlobalAllocAnsi(value);
                rdcWindow.AdvancedSettings3.ClearTextPassword = Marshal.PtrToStringAnsi(password);
            }
        }

        public override bool Focused
        {
            get
            {
                return rdcWindow.Focused;
            }
        }

        public AudioMode AudioMode
        {
            get
            {
                return (AudioMode)rdcWindow.SecuredSettings2.AudioRedirectionMode;
            }

            set
            {
                rdcWindow.SecuredSettings2.AudioRedirectionMode = (int)value;
            }
        }

        public KeyboardMode KeyboardMode
        {
            get
            {
                return (KeyboardMode)rdcWindow.SecuredSettings2.KeyboardHookMode;
            }

            set
            {
                rdcWindow.SecuredSettings2.KeyboardHookMode = (int)value;
            }
        }

        public bool ConnectPrinters
        {
            get
            {
                return rdcWindow.AdvancedSettings2.RedirectPrinters;
            }

            set
            {
                rdcWindow.AdvancedSettings2.RedirectPrinters = value;
                rdcWindow.AdvancedSettings2.DisableRdpdr = (!(value || ConnectClipboard) ? 1 : 0);
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
                rdcWindow.AdvancedSettings.DisableRdpdr = (!(value || ConnectPrinters) ? 1 : 0);
                _connectClipboard = value;
            }
        }

        public bool ConnectDrives
        {
            get
            {
                return rdcWindow.AdvancedSettings2.RedirectDrives;
            }

            set
            {
                rdcWindow.AdvancedSettings2.RedirectDrives = value;
            }
        }

        public bool DesktopBackground
        {
            get
            {
                return !((rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000001) == 0x00000001);
            }

            set
            {
                if (value)
                    rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000001;

                else
                    rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000001;
            }
        }

        public bool FontSmoothing
        {
            get
            {
                return !((rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000080) == 0x00000080);
            }

            set
            {
                if (value)
                    rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000080;

                else
                    rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000080;
            }
        }

        public bool DesktopComposition
        {
            get
            {
                return !((rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000100) == 0x00000100);
            }

            set
            {
                if (value)
                    rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000100;

                else
                    rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000100;
            }
        }

        public bool WindowContentsWhileDragging
        {
            get
            {
                return !((rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000100) == 0x00000100);
            }

            set
            {
                if (value)
                    rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000100;

                else
                    rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000100;
            }
        }

        public bool Animations
        {
            get
            {
                return !((rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000004) == 0x00000004);
            }

            set
            {
                if (value)
                    rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000004;

                else
                    rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000004;
            }
        }

        public bool VisualStyles
        {
            get
            {
                return !((rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000008) == 0x00000008);
            }

            set
            {
                if (value)
                    rdcWindow.AdvancedSettings2.PerformanceFlags &= ~0x00000008;

                else
                    rdcWindow.AdvancedSettings2.PerformanceFlags |= 0x00000008;
            }
        }

        public bool PersistentBitmapCaching
        {
            get
            {
                return !(rdcWindow.AdvancedSettings2.CachePersistenceActive == 0);
            }

            set
            {
                rdcWindow.AdvancedSettings2.CachePersistenceActive = (value ? 1 : 0);
            }
        }

        public void Connect()
        {
            rdcWindow.Connect();
            rdcWindow.OnConnected += Connected;
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
    }
}
