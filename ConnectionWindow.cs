using System;
using System.Linq;
using System.Collections.Generic;
using System.Security;
using System.Windows.Forms;

namespace EasyConnect
{
    public partial class ConnectionWindow : Form
    {
        protected RDCConnection _connection = null;
        protected MainForm _applicationForm = null;
        protected List<DEVMODE> _resolutions = new List<DEVMODE>();
        protected BookmarksFolder _currentFolder = null;

        public ConnectionWindow(MainForm applicationForm, BookmarksFolder currentFolder = null)
            : this(applicationForm, new RDCConnection(applicationForm.Password), currentFolder)
        {
        }

        public ConnectionWindow(MainForm applicationForm, RDCConnection connection, BookmarksFolder currentFolder = null)
        {
            InitializeComponent();

            _applicationForm = applicationForm;
            _connection = connection;
            _currentFolder = currentFolder;

            keyboardDropdown.SelectedIndex = 1;

            rdcImage.Parent = gradientBackground;
            rdcLabel1.Parent = gradientBackground;
            rdcLabel2.Parent = gradientBackground;

            DEVMODE devMode = new DEVMODE();
            int modeNumber = 0;

            while (DisplayHelper.EnumDisplaySettings(null, modeNumber, ref devMode) > 0)
            {
                if (
                    !_resolutions.Exists(
                        (DEVMODE d) => d.dmPelsWidth == devMode.dmPelsWidth && d.dmPelsHeight == devMode.dmPelsHeight))
                    _resolutions.Add(devMode);

                modeNumber++;
            }

            resolutionTrackBar.Maximum = _resolutions.Count;
            resolutionTrackBar.Value = _resolutions.Count;

            if (connection != null)
            {
                hostBox.Text = connection.Host;
                usernameTextBox.Text = connection.Username;
                passwordTextBox.SecureText = (connection.Password == null
                                                  ? new SecureString()
                                                  : connection.Password.Copy());
                keyboardDropdown.SelectedIndex = (int) connection.KeyboardMode;
                printersCheckBox.Checked = connection.ConnectPrinters;
                clipboardCheckBox.Checked = connection.ConnectClipboard;
                drivesCheckBox.Checked = connection.ConnectDrives;
                desktopBackgroundCheckBox.Checked = connection.DesktopBackground;
                fontSmoothingCheckBox.Checked = connection.FontSmoothing;
                desktopCompositionCheckBox.Checked = connection.DesktopComposition;
                windowContentsCheckBox.Checked = connection.WindowContentsWhileDragging;
                animationCheckBox.Checked = connection.Animations;
                visualStylesCheckBox.Checked = connection.VisualStyles;
                bitmapCachingCheckBox.Checked = connection.PersistentBitmapCaching;

                if (connection.AudioMode == AudioMode.Remotely)
                    playRemotelyRadioButton.Checked = true;

                else if (connection.AudioMode == AudioMode.None)
                    dontPlayRadioButton.Checked = true;

                int resolutionIndex =
                    _resolutions.FindIndex(
                        (DEVMODE d) =>
                        d.dmPelsWidth == connection.DesktopWidth && d.dmPelsHeight == connection.DesktopHeight);

                if (resolutionIndex != -1)
                    resolutionTrackBar.Value = resolutionIndex;

                switch (connection.ColorDepth)
                {
                    case 15:
                        colorDepthDropdown.SelectedIndex = 0;
                        break;

                    case 16:
                        colorDepthDropdown.SelectedIndex = 1;
                        break;

                    case 24:
                        colorDepthDropdown.SelectedIndex = 2;
                        break;

                    case 32:
                        colorDepthDropdown.SelectedIndex = 3;
                        break;
                }
            }
        }

        private void resolutionTrackBar_ValueChanged(object sender, EventArgs e)
        {
            resolutionLabel.Text = (resolutionTrackBar.Value == _resolutions.Count
                                        ? "Full Screen"
                                        : _resolutions[resolutionTrackBar.Value].dmPelsWidth.ToString() + " by " +
                                          _resolutions[resolutionTrackBar.Value].dmPelsHeight.ToString() + " pixels");
        }

        private void saveAsButton_Click(object sender, EventArgs e)
        {
            SaveConnectionWindow saveWindow = new SaveConnectionWindow(_applicationForm, _currentFolder);
            saveWindow.ShowDialog(this);

            if (saveWindow.DialogResult != DialogResult.OK)
                return;

            string[] pathComponents = saveWindow.DestinationFolderPath.Split('/');
            TreeNode currentNode = _applicationForm.Bookmarks.FoldersTreeView.Nodes[0];

            for (int i = 2; i < pathComponents.Length; i++)
                currentNode = currentNode.Nodes[Convert.ToInt32(pathComponents[i])];

            _connection =
                _applicationForm.Bookmarks.TreeNodeFolders[currentNode].Bookmarks.SingleOrDefault(
                    b =>
                    (b.Name == saveWindow.ConnectionName && !String.IsNullOrEmpty(b.Name)) ||
                    (String.IsNullOrEmpty(b.Name) && b.Host == hostBox.Text));

            if (_connection == null)
            {
                _connection = new RDCConnection(_applicationForm.Password)
                                  {
                                      Name = saveWindow.ConnectionName,
                                      Host = hostBox.Text,
                                      IsBookmark = true
                                  };
                _applicationForm.Bookmarks.TreeNodeFolders[currentNode].Bookmarks.Add(_connection);
            }

            SaveConnection();
            _applicationForm.Bookmarks.Save();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (_connection == null || _connection.IsBookmark == false)
                saveAsButton_Click(sender, e);

            else
                SaveConnection();

            _applicationForm.Bookmarks.Save();
        }

        protected void SaveConnection()
        {
            _connection.Host = hostBox.Text;
            _connection.Username = usernameTextBox.Text;
            _connection.Password = passwordTextBox.SecureText;
            _connection.DesktopWidth = (resolutionTrackBar.Value == resolutionTrackBar.Maximum
                                            ? 0
                                            : _resolutions[resolutionTrackBar.Value].dmPelsWidth);
            _connection.DesktopHeight = (resolutionTrackBar.Value == resolutionTrackBar.Maximum
                                             ? 0
                                             : _resolutions[resolutionTrackBar.Value].dmPelsHeight);
            _connection.KeyboardMode = (KeyboardMode) keyboardDropdown.SelectedIndex;
            _connection.ConnectPrinters = printersCheckBox.Checked;
            _connection.ConnectClipboard = clipboardCheckBox.Checked;
            _connection.ConnectDrives = drivesCheckBox.Checked;
            _connection.DesktopBackground = desktopBackgroundCheckBox.Checked;
            _connection.FontSmoothing = fontSmoothingCheckBox.Checked;
            _connection.DesktopComposition = desktopCompositionCheckBox.Checked;
            _connection.WindowContentsWhileDragging = windowContentsCheckBox.Checked;
            _connection.Animations = animationCheckBox.Checked;
            _connection.VisualStyles = visualStylesCheckBox.Checked;
            _connection.PersistentBitmapCaching = bitmapCachingCheckBox.Checked;

            if (playLocallyRadioButton.Checked)
                _connection.AudioMode = AudioMode.Locally;

            else if (playRemotelyRadioButton.Checked)
                _connection.AudioMode = AudioMode.Remotely;

            else
                _connection.AudioMode = AudioMode.None;

            switch (colorDepthDropdown.SelectedIndex)
            {
                case 0:
                    _connection.ColorDepth = 15;
                    break;

                case 1:
                    _connection.ColorDepth = 16;
                    break;

                case 2:
                    _connection.ColorDepth = 24;
                    break;

                case 3:
                    _connection.ColorDepth = 32;
                    break;
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            RDCConnection newConnection = new RDCConnection(_applicationForm.Password)
                                              {
                                                  Name = _connection.Name
                                              };
            _connection = newConnection;

            SaveConnection();
            _applicationForm.SelectedTab = _applicationForm.Connect(_connection);

            Close();
        }
    }
}