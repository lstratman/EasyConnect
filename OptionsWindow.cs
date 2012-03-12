using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace EasyConnect
{
    public partial class OptionsWindow : Form
    {
        protected List<DEVMODE> _resolutions = new List<DEVMODE>();
        protected MainForm _applicationForm = null;

        public OptionsWindow(MainForm applicationForm)
        {
            _applicationForm = applicationForm;

            InitializeComponent();
        }

        private void OptionsWindow_Load(object sender, EventArgs e)
        {
            _userNameTextBox.Text = _applicationForm.Options.RdpDefaults.Username;
            _passwordTextBox.SecureText = (_applicationForm.Options.RdpDefaults.Password == null
                                                  ? new SecureString()
                                                  : _applicationForm.Options.RdpDefaults.Password.Copy());

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

            _resolutions = _resolutions.OrderBy(m => m.dmPelsWidth).ToList();
            _resolutionSlider.Maximum = _resolutions.Count;

            int resolutionIndex =
                    _resolutions.FindIndex(
                        (DEVMODE d) =>
                        d.dmPelsWidth == _applicationForm.Options.RdpDefaults.DesktopWidth && d.dmPelsHeight == _applicationForm.Options.RdpDefaults.DesktopHeight);

            _resolutionSlider.Value = resolutionIndex != -1 ? resolutionIndex : _resolutionSlider.Maximum;

            switch (_applicationForm.Options.RdpDefaults.ColorDepth)
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

            _audioPlaybackDropdown.SelectedIndex = (int) _applicationForm.Options.RdpDefaults.AudioMode;
            _audioRecordingDropdown.SelectedIndex = (int) _applicationForm.Options.RdpDefaults.RecordingMode;
            _windowsKeyDropdown.SelectedIndex = (int) _applicationForm.Options.RdpDefaults.KeyboardMode;
            _clipboardCheckbox.Checked = _applicationForm.Options.RdpDefaults.ConnectClipboard;
            _printersCheckbox.Checked = _applicationForm.Options.RdpDefaults.ConnectPrinters;
            _drivesCheckbox.Checked = _applicationForm.Options.RdpDefaults.ConnectDrives;
            _desktopBackgroundCheckbox.Checked = _applicationForm.Options.RdpDefaults.DesktopBackground;
            _fontSmoothingCheckbox.Checked = _applicationForm.Options.RdpDefaults.FontSmoothing;
            _desktopCompositionCheckbox.Checked = _applicationForm.Options.RdpDefaults.DesktopComposition;
            _windowContentsWhileDraggingCheckbox.Checked = _applicationForm.Options.RdpDefaults.WindowContentsWhileDragging;
            _menuAnimationCheckbox.Checked = _applicationForm.Options.RdpDefaults.Animations;
            _visualStylesCheckbox.Checked = _applicationForm.Options.RdpDefaults.VisualStyles;
            _bitmapCachingCheckbox.Checked = _applicationForm.Options.RdpDefaults.PersistentBitmapCaching;
        }

        private void OptionsWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            _applicationForm.Options.RdpDefaults.Username = _userNameTextBox.Text;
            _applicationForm.Options.RdpDefaults.Password = _passwordTextBox.SecureText;
            _applicationForm.Options.RdpDefaults.DesktopWidth = _resolutionSlider.Value != _resolutionSlider.Maximum
                                                                    ? _resolutions[_resolutionSlider.Value].dmPelsWidth
                                                                    : 0;
            _applicationForm.Options.RdpDefaults.DesktopHeight = _resolutionSlider.Value != _resolutionSlider.Maximum
                                                                     ? _resolutions[_resolutionSlider.Value].dmPelsHeight
                                                                     : 0;

            switch (_colorDepthDropdown.SelectedIndex)
            {
                case 0:
                    _applicationForm.Options.RdpDefaults.ColorDepth = 15;
                    break;

                case 1:
                    _applicationForm.Options.RdpDefaults.ColorDepth = 16;
                    break;

                case 2:
                    _applicationForm.Options.RdpDefaults.ColorDepth = 24;
                    break;

                case 3:
                    _applicationForm.Options.RdpDefaults.ColorDepth = 32;
                    break;
            }

            _applicationForm.Options.RdpDefaults.AudioMode = (AudioMode) _audioPlaybackDropdown.SelectedIndex;
            _applicationForm.Options.RdpDefaults.RecordingMode = (RecordingMode) _audioRecordingDropdown.SelectedIndex;
            _applicationForm.Options.RdpDefaults.KeyboardMode = (KeyboardMode) _windowsKeyDropdown.SelectedIndex;
            _applicationForm.Options.RdpDefaults.ConnectClipboard = _clipboardCheckbox.Checked;
            _applicationForm.Options.RdpDefaults.ConnectPrinters = _printersCheckbox.Checked;
            _applicationForm.Options.RdpDefaults.ConnectDrives = _drivesCheckbox.Checked;
            _applicationForm.Options.RdpDefaults.DesktopBackground = _desktopBackgroundCheckbox.Checked;
            _applicationForm.Options.RdpDefaults.FontSmoothing = _fontSmoothingCheckbox.Checked;
            _applicationForm.Options.RdpDefaults.DesktopComposition = _desktopCompositionCheckbox.Checked;
            _applicationForm.Options.RdpDefaults.WindowContentsWhileDragging = _windowContentsWhileDraggingCheckbox.Checked;
            _applicationForm.Options.RdpDefaults.Animations = _menuAnimationCheckbox.Checked;
            _applicationForm.Options.RdpDefaults.VisualStyles = _visualStylesCheckbox.Checked;
            _applicationForm.Options.RdpDefaults.PersistentBitmapCaching = _bitmapCachingCheckbox.Checked;

            _applicationForm.Options.Save();
        }

        private void _resolutionSlider_ValueChanged(object sender, EventArgs e)
        {
            _resolutionSliderLabel.Text = (_resolutionSlider.Value == _resolutions.Count
                                               ? "Full Screen"
                                               : _resolutions[_resolutionSlider.Value].dmPelsWidth.ToString() + " by " +
                                                 _resolutions[_resolutionSlider.Value].dmPelsHeight.ToString() + " pixels");
        }
    }
}
