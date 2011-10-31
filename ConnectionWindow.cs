using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Security;

namespace UltraRDC
{
    public partial class ConnectionWindow : Form
    {
        protected List<DEVMODE> _resolutions = new List<DEVMODE>();
        protected FavoritesWindow _favorites = null;
        protected RDCConnection _connection = null;
        protected MainForm.ConnectionDelegate _connectionDelegate = null;
        protected SecureString _password = null;

        public ConnectionWindow(FavoritesWindow favorites, MainForm.ConnectionDelegate connectionDelegate, SecureString password)
            : this(favorites, new RDCConnection(password), connectionDelegate, password)
        {
        }

        public ConnectionWindow(FavoritesWindow favorites, RDCConnection connection, MainForm.ConnectionDelegate connectionDelegate, SecureString password)
        {
            InitializeComponent();

            _connection = connection;
            _favorites = favorites;
            _connectionDelegate = connectionDelegate;
            _password = password;

            keyboardDropdown.SelectedIndex = 1;

            rdcImage.Parent = gradientBackground;
            rdcLabel1.Parent = gradientBackground;
            rdcLabel2.Parent = gradientBackground;

            DEVMODE devMode = new DEVMODE();
            int modeNumber = 0;

            while (DisplayHelper.EnumDisplaySettings(null, modeNumber, ref devMode) > 0)
            {
                if (!_resolutions.Exists((DEVMODE d) => d.dmPelsWidth == devMode.dmPelsWidth && d.dmPelsHeight == devMode.dmPelsHeight))
                    _resolutions.Add(devMode);

                modeNumber++;
            }

            resolutionTrackBar.Maximum = _resolutions.Count;
            resolutionTrackBar.Value = _resolutions.Count;

            if (connection != null)
            {
                hostBox.Text = connection.Host;
                usernameTextBox.Text = connection.Username;
                passwordTextBox.SecureText = (connection.Password == null ? new SecureString() : connection.Password.Copy());
                keyboardDropdown.SelectedIndex = (int)connection.KeyboardMode;
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
                
                int resolutionIndex = _resolutions.FindIndex((DEVMODE d) => d.dmPelsWidth == connection.DesktopWidth && d.dmPelsHeight == connection.DesktopHeight);

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
            resolutionLabel.Text = (resolutionTrackBar.Value == _resolutions.Count ? "Full Screen" : _resolutions[resolutionTrackBar.Value].dmPelsWidth.ToString() + " by " + _resolutions[resolutionTrackBar.Value].dmPelsHeight.ToString() + " pixels");
        }

        private void saveAsButton_Click(object sender, EventArgs e)
        {
            SaveConnectionWindow saveWindow = new SaveConnectionWindow((TreeNode)_favorites.TreeRoot.Clone());
            saveWindow.ShowDialog(this);

            if (saveWindow.DialogResult != DialogResult.OK)
                return;

            string[] pathComponents = saveWindow.DestinationFolderPath.Split('/');
            TreeNode currentNode = _favorites.TreeRoot;
            
            for (int i = 2; i < pathComponents.Length; i++)
                currentNode = currentNode.Nodes[Convert.ToInt32(pathComponents[i])];

            TreeNode connectionNode = new TreeNode(saveWindow.ConnectionName, 1, 1);
            bool overwriteExisting = false;

            foreach (TreeNode node in currentNode.Nodes)
            {
                if (node.Text == connectionNode.Text)
                {
                    overwriteExisting = true;
                    connectionNode = node;
                }
            }

            if (!overwriteExisting)
                _favorites.AddTreeNode(currentNode.Nodes, connectionNode);

            _connection = new RDCConnection(_password) { Name = saveWindow.ConnectionName, IsFavorite = true };

            SaveConnection();
            _favorites.Connections[connectionNode] = _connection;

            _favorites.Save();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (_connection == null || _connection.IsFavorite == false)
                saveAsButton_Click(sender, e);

            else
                SaveConnection();

            _favorites.Save();
        }

        protected void SaveConnection()
        {
            _connection.Host = hostBox.Text;
            _connection.Username = usernameTextBox.Text;
            _connection.Password = passwordTextBox.SecureText;
            _connection.DesktopWidth = (resolutionTrackBar.Value == resolutionTrackBar.Maximum ? 0 : _resolutions[resolutionTrackBar.Value].dmPelsWidth);
            _connection.DesktopHeight = (resolutionTrackBar.Value == resolutionTrackBar.Maximum ? 0 : _resolutions[resolutionTrackBar.Value].dmPelsHeight);
            _connection.KeyboardMode = (KeyboardMode)keyboardDropdown.SelectedIndex;
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
            RDCConnection newConnection = new RDCConnection(_password) { Name = _connection.Name };
            _connection = newConnection;

            SaveConnection();
            _connectionDelegate(_connection);

            Close();
        }
    }
}
