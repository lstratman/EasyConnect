using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Security;
using System.Runtime.InteropServices;
using AxMSTSCLib;
using EasyConnect.Properties;
using Stratman.Windows.Forms.TitleBarTabs;

namespace EasyConnect
{
    public partial class RDCWindow : Form
    {
        protected MSTSCLib.IMsRdpClientNonScriptable _nonScriptable = null;
        protected bool _connectClipboard = true;
        protected SecureString _password = null;
        protected RDCConnection _connection = null;
        protected Dictionary<ToolStripMenuItem, RDCConnection> _menuItemConnections =
            new Dictionary<ToolStripMenuItem, RDCConnection>();

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

        protected MainForm ParentTabs
        {
            get
            {
                return (MainForm) Parent;
            }
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
                return (_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000001) != 0x00000001;
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
                return (_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000080) != 0x00000080;
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
                return (_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000100) != 0x00000100;
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
                return (_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000100) != 0x00000100;
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
                return (_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000004) != 0x00000004;
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
                return (_rdcWindow.AdvancedSettings2.PerformanceFlags & 0x00000008) != 0x00000008;
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
                return _rdcWindow.AdvancedSettings2.CachePersistenceActive != 0;
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
            Text = (String.IsNullOrEmpty(connection.Name) ? connection.Host : connection.Name);
            urlTextBox.Text = connection.Host;

            _connection = connection;
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

        private void _closeButton_MouseEnter(object sender, EventArgs e)
        {
            _closeButton.BackgroundImage = Resources.ButtonHoverBackground;
        }

        private void _closeButton_MouseLeave(object sender, EventArgs e)
        {
            _closeButton.BackgroundImage = null;
        }

        private void _bookmarksButton_MouseEnter(object sender, EventArgs e)
        {
            _bookmarksButton.BackgroundImage = Resources.ButtonHoverBackground;
        }

        private void _bookmarksButton_MouseLeave(object sender, EventArgs e)
        {
            _bookmarksButton.BackgroundImage = null;
        }

        private void _toolsButton_MouseEnter(object sender, EventArgs e)
        {
            _toolsButton.BackgroundImage = Resources.ButtonHoverBackground;
        }

        private void _toolsButton_MouseLeave(object sender, EventArgs e)
        {
            _toolsButton.BackgroundImage = null;
        }

        private void _closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _exitMenuItem_Click(object sender, EventArgs e)
        {
            ((Form)Parent).Close();
        }

        private void _toolsButton_Click(object sender, EventArgs e)
        {
            _toolsMenu.DefaultDropDownDirection = ToolStripDropDownDirection.Left;
            _toolsMenu.Show(_toolsButton, -1 * _toolsMenu.Width + _toolsButton.Width, _toolsButton.Height);
        }

        private void _bookmarksButton_Click(object sender, EventArgs e)
        {
            while (_bookmarksMenu.Items.Count > 2)
                _bookmarksMenu.Items.RemoveAt(2);

            if (ParentTabs.Bookmarks.RootFolder.ChildFolders.Count > 0 || ParentTabs.Bookmarks.RootFolder.Bookmarks.Count > 0)
                _bookmarksMenu.Items.Add(new ToolStripSeparator());

            _menuItemConnections.Clear();
            PopulateBookmarks(ParentTabs.Bookmarks.RootFolder, _bookmarksMenu.Items, true);

            _bookmarksMenu.DefaultDropDownDirection = ToolStripDropDownDirection.Left;
            _bookmarksMenu.Show(_bookmarksButton, -1 * _bookmarksMenu.Width + _bookmarksButton.Width, _bookmarksButton.Height);
        }

        private void PopulateBookmarks(BookmarksFolder currentFolder, ToolStripItemCollection menuItems, bool root)
        {
            ToolStripItemCollection addLocation = menuItems;

            if (!root)
            {
                ToolStripMenuItem folderMenuItem = new ToolStripMenuItem(currentFolder.Name, Resources.Folder);
                folderMenuItem.DropDownDirection = ToolStripDropDownDirection.Left;

                menuItems.Add(folderMenuItem);
                addLocation = folderMenuItem.DropDownItems;
            }

            foreach (BookmarksFolder childFolder in currentFolder.ChildFolders)
                PopulateBookmarks(childFolder, addLocation, false);

            foreach (RDCConnection bookmark in currentFolder.Bookmarks)
            {
                ToolStripMenuItem bookmarkMenuItem = new ToolStripMenuItem(String.IsNullOrEmpty(bookmark.Name)
                                                                               ? bookmark.Host
                                                                               : bookmark.Name, Resources.RDCSmall,
                                                                           (object sender, EventArgs e) =>
                                                                           Connect(
                                                                               _menuItemConnections[
                                                                                   (ToolStripMenuItem) sender]));

                _menuItemConnections[bookmarkMenuItem] = bookmark;
                addLocation.Add(bookmarkMenuItem);
            }
        }

        private void _bookmarkMenuItem_Click(object sender, EventArgs e)
        {
            if (_connection == null)
                return;

            ParentTabs.Bookmarks.RootFolder.Bookmarks.Add(_connection);
        }

        private void _newTabMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.AddNewTab();
        }

        private void _bookmarksManagerMenuItem2_Click(object sender, EventArgs e)
        {
            ParentTabs.OpenBookmarkManager();
        }
    }
}
