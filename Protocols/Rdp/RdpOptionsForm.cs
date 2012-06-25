using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using EasyConnect.Common;

namespace EasyConnect.Protocols.Rdp
{
    public partial class RdpOptionsForm : Form, IOptionsForm<RdpConnection>
    {
        public RdpOptionsForm()
        {
            InitializeComponent();
        }

        protected List<DEVMODE> _resolutions = new List<DEVMODE>();

        private void RdpOptionsForm_Load(object sender, EventArgs e)
        {
            Text = "Options for " + Connection.DisplayName;

            _hostNameTextBox.Text = Connection.Host;
            _userNameTextBox.Text = Connection.Username;
            _passwordTextBox.SecureText = (Connection.Password == null
                                                  ? new SecureString()
                                                  : Connection.Password.Copy());

            DEVMODE devMode = new DEVMODE();
            int modeNumber = 0;

            while (DisplayUtilities.EnumDisplaySettings(null, modeNumber, ref devMode) > 0)
            {
                if (
                    !_resolutions.Exists(
                        (DEVMODE d) => d.dmPelsWidth == devMode.dmPelsWidth && d.dmPelsHeight == devMode.dmPelsHeight))
                    _resolutions.Add(devMode);

                modeNumber++;
            }

            _resolutions = _resolutions.OrderBy(m => m.dmPelsWidth).ToList();
            _resolutionSlider.Maximum = _resolutions.Count;

            int resolutionIndex =
                    _resolutions.FindIndex(
                        (DEVMODE d) =>
                        d.dmPelsWidth == Connection.DesktopWidth && d.dmPelsHeight == Connection.DesktopHeight);

            _resolutionSlider.Value = resolutionIndex != -1 ? resolutionIndex : _resolutionSlider.Maximum;

            switch (Connection.ColorDepth)
            {
                case 15:
                    _colorDepthDropdown.SelectedIndex = 0;
                    break;

                case 16:
                    _colorDepthDropdown.SelectedIndex = 1;
                    break;

                case 24:
                    _colorDepthDropdown.SelectedIndex = 2;
                    break;

                case 32:
                    _colorDepthDropdown.SelectedIndex = 3;
                    break;
            }

            _audioPlaybackDropdown.SelectedIndex = (int) Connection.AudioMode;
            _audioRecordingDropdown.SelectedIndex = (int) Connection.RecordingMode;
            _windowsKeyDropdown.SelectedIndex = (int) Connection.KeyboardMode;
            _clipboardCheckbox.Checked = Connection.ConnectClipboard;
            _printersCheckbox.Checked = Connection.ConnectPrinters;
            _drivesCheckbox.Checked = Connection.ConnectDrives;
            _desktopBackgroundCheckbox.Checked = Connection.DesktopBackground;
            _fontSmoothingCheckbox.Checked = Connection.FontSmoothing;
            _desktopCompositionCheckbox.Checked = Connection.DesktopComposition;
            _windowContentsWhileDraggingCheckbox.Checked = Connection.WindowContentsWhileDragging;
            _menuAnimationCheckbox.Checked = Connection.Animations;
            _visualStylesCheckbox.Checked = Connection.VisualStyles;
            _bitmapCachingCheckbox.Checked = Connection.PersistentBitmapCaching;

            if (DefaultsMode)
            {
                _hostPanel.Visible = false;
                _hostDividerPanel.Visible = false;
            }
        }

        private void RdpOptionsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Connection.Host = _hostNameTextBox.Text;
            Connection.Username = _userNameTextBox.Text;
            Connection.Password = _passwordTextBox.SecureText;
            Connection.DesktopWidth = _resolutionSlider.Value != _resolutionSlider.Maximum
                                                                    ? _resolutions[_resolutionSlider.Value].dmPelsWidth
                                                                    : 0;
            Connection.DesktopHeight = _resolutionSlider.Value != _resolutionSlider.Maximum
                                                                     ? _resolutions[_resolutionSlider.Value].dmPelsHeight
                                                                     : 0;

            switch (_colorDepthDropdown.SelectedIndex)
            {
                case 0:
                    Connection.ColorDepth = 15;
                    break;

                case 1:
                    Connection.ColorDepth = 16;
                    break;

                case 2:
                    Connection.ColorDepth = 24;
                    break;

                case 3:
                    Connection.ColorDepth = 32;
                    break;
            }

            Connection.AudioMode = (AudioMode) _audioPlaybackDropdown.SelectedIndex;
            Connection.RecordingMode = (RecordingMode) _audioRecordingDropdown.SelectedIndex;
            Connection.KeyboardMode = (KeyboardMode) _windowsKeyDropdown.SelectedIndex;
            Connection.ConnectClipboard = _clipboardCheckbox.Checked;
            Connection.ConnectPrinters = _printersCheckbox.Checked;
            Connection.ConnectDrives = _drivesCheckbox.Checked;
            Connection.DesktopBackground = _desktopBackgroundCheckbox.Checked;
            Connection.FontSmoothing = _fontSmoothingCheckbox.Checked;
            Connection.DesktopComposition = _desktopCompositionCheckbox.Checked;
            Connection.WindowContentsWhileDragging = _windowContentsWhileDraggingCheckbox.Checked;
            Connection.Animations = _menuAnimationCheckbox.Checked;
            Connection.VisualStyles = _visualStylesCheckbox.Checked;
            Connection.PersistentBitmapCaching = _bitmapCachingCheckbox.Checked;
        }

        private void _resolutionSlider_ValueChanged(object sender, EventArgs e)
        {
            _resolutionSliderLabel.Text = (_resolutionSlider.Value == _resolutions.Count
                                               ? "Full Screen"
                                               : _resolutions[_resolutionSlider.Value].dmPelsWidth.ToString() + " by " +
                                                 _resolutions[_resolutionSlider.Value].dmPelsHeight.ToString() + " pixels");
        }

        public RdpConnection Connection
        {
            get;
            set;
        }

        public bool DefaultsMode
        {
            get;
            set;
        }

        private void _flowLayoutPanel_Resize(object sender, EventArgs e)
        {

        }
    }
}
