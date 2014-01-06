using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using Win32Interop.Methods;
using Win32Interop.Structs;

namespace EasyConnect.Protocols.Rdp
{
	/// <summary>
	/// Form that captures options for a particular <see cref="RdpConnection"/> instance or defaults for the <see cref="RdpProtocol"/> protocol.
	/// </summary>
	public partial class RdpOptionsForm : Form, IOptionsForm<RdpConnection>
	{
		/// <summary>
		/// Contains the previous width of <see cref="_flowLayoutPanel"/> prior to a resize operation.
		/// </summary>
		protected int _previousWidth;

		/// <summary>
		/// List of all possible desktop resolutions.
		/// </summary>
		protected List<DEVMODE> _resolutions = new List<DEVMODE>();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public RdpOptionsForm()
		{
			InitializeComponent();

			_previousWidth = _flowLayoutPanel.Width;
		}

		/// <summary>
		/// Connection instance that we're capturing option data for.
		/// </summary>
		public RdpConnection Connection
		{
			get;
			set;
		}

		/// <summary>
		/// Connection instance that we're capturing option data for.
		/// </summary>
		IConnection IOptionsForm.Connection
		{
			get
			{
				return Connection;
			}

			set
			{
				Connection = (RdpConnection) value;
			}
		}

		/// <summary>
		/// Flag indicating if the options should be for a specific connection or should be for the defaults for the protocol (i.e. should not capture 
		/// hostname).
		/// </summary>
		public bool DefaultsMode
		{
			get;
			set;
		}

		/// <summary>
		/// Handler method that's called when the form loads initially.  Initializes the UI from the data in <see cref="Connection"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void RdpOptionsForm_Load(object sender, EventArgs e)
		{
			Text = "Options for " + Connection.DisplayName;

			_hostNameTextBox.Text = Connection.Host;
			_userNameTextBox.Text = Connection.Username;
			_passwordTextBox.SecureText = (Connection.Password == null
				                               ? new SecureString()
				                               : Connection.Password.Copy());

			if (String.IsNullOrEmpty(Connection.Username) && !String.IsNullOrEmpty(Connection.InheritedUsername))
				_inheritedUsernameLabel.Text = "Inheriting " + Connection.InheritedUsername + " from parent folders";

			if ((Connection.Password == null || Connection.Password.Length == 0) && Connection.InheritedPassword != null && Connection.InheritedPassword.Length > 0)
				_inheritedPasswordLabel.Text = "Inheriting a password from parent folders";

			// Enumerate the desktop display modes and add them to the resolutions slider
			DEVMODE devMode = new DEVMODE();
			int modeNumber = 0;

			while (User32.EnumDisplaySettings(null, modeNumber, ref devMode))
			{
				if (!_resolutions.Exists((DEVMODE d) => d.dmPelsWidth == devMode.dmPelsWidth && d.dmPelsHeight == devMode.dmPelsHeight))
					_resolutions.Add(devMode);

				modeNumber++;
			}

			_resolutions = _resolutions.OrderBy(m => m.dmPelsWidth).ToList();
			_resolutionSlider.Maximum = _resolutions.Count;

			int resolutionIndex =
				_resolutions.FindIndex(
					(DEVMODE d) =>
					d.dmPelsWidth == Connection.DesktopWidth && d.dmPelsHeight == Connection.DesktopHeight);

			_resolutionSlider.Value = resolutionIndex != -1
				                          ? resolutionIndex
				                          : _resolutionSlider.Maximum;

			// Set the values of the various fields based on the properties in Connection
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
			_adminChannelCheckBox.Checked = Connection.ConnectToAdminChannel;

            _useProxyCheckBox.Checked = Connection.UseTSProxy;
		    _proxyHostNameTextBox.Text = Connection.ProxyName;
            _proxyUserNameTextBox.Text = Connection.ProxyUserName;
            _proxyPasswordTextBox.SecureText = (Connection.ProxyPassword == null
                                               ? new SecureString()
                                               : Connection.ProxyPassword.Copy());

            // Hide the host panel if we're in defaults mode
			if (DefaultsMode)
				_hostPanel.Visible = false;
		}

		/// <summary>
		/// Handler method that's called when the form is closing.  Copies the contents of the various fields back into <see cref="Connection"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void RdpOptionsForm_FormClosing(object sender, CancelEventArgs e)
		{
			Connection.Host = _hostNameTextBox.Text;
			Connection.Username = _userNameTextBox.Text;
			Connection.Password = _passwordTextBox.SecureText;
			Connection.DesktopWidth = _resolutionSlider.Value != _resolutionSlider.Maximum
				                          ? Convert.ToInt32(_resolutions[_resolutionSlider.Value].dmPelsWidth)
				                          : 0;
			Connection.DesktopHeight = _resolutionSlider.Value != _resolutionSlider.Maximum
				                           ? Convert.ToInt32(_resolutions[_resolutionSlider.Value].dmPelsHeight)
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
			Connection.ConnectToAdminChannel = _adminChannelCheckBox.Checked;

            Connection.UseTSProxy = _useProxyCheckBox.Checked;
            Connection.ProxyName = _proxyHostNameTextBox.Text;
            Connection.ProxyUserName = _proxyUserNameTextBox.Text;
            Connection.ProxyPassword = _proxyPasswordTextBox.SecureText;
		}

		/// <summary>
		/// Handler method that's called when the value of <see cref="_resolutionSlider"/> changes.  Updates the text below the slider with the corresponding
		/// desktop width and height of the value.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_resolutionSlider"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _resolutionSlider_ValueChanged(object sender, EventArgs e)
		{
			// The last position on the slider indicates "Full Screen"
			_resolutionSliderLabel.Text = (_resolutionSlider.Value == _resolutions.Count
				                               ? "Full Screen"
				                               : _resolutions[_resolutionSlider.Value].dmPelsWidth.ToString() + " by " +
				                                 _resolutions[_resolutionSlider.Value].dmPelsHeight.ToString() + " pixels");
		}

		/// <summary>
		/// Handler method that's called when the size of <see cref="_flowLayoutPanel"/> changes.  Resizes each child <see cref="Panel"/> instance accordingly.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_flowLayoutPanel"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _flowLayoutPanel_Resize(object sender, EventArgs e)
		{
			foreach (Panel panel in _flowLayoutPanel.Controls.Cast<Panel>())
				panel.Width += _flowLayoutPanel.Width - _previousWidth;

			_previousWidth = _flowLayoutPanel.Width;
		}

		/// <summary>
		/// Handler method that's called when the contents of <see cref="_userNameTextBox"/> change.  If the textbox is empty, display 
		/// <see cref="_inheritedUsernameLabel"/>, hide it otherwise.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_userNameTextBox"/> in this case.</param>
		/// <param name="e">Arguments associated with the event.</param>
		private void _userNameTextBox_TextChanged(object sender, EventArgs e)
		{
			_inheritedUsernameLabel.Text = String.IsNullOrEmpty(_userNameTextBox.Text) &&
			                               !String.IsNullOrEmpty(Connection.GetInheritedUsername(Connection.ParentFolder))
				                               ? "Inheriting " + Connection.InheritedUsername + " from parent folders"
				                               : "";
		}

		/// <summary>
		/// Handler method that's called when the contents of <see cref="_passwordTextBox"/> change.  If the textbox is empty, display 
		/// <see cref="_inheritedPasswordLabel"/>, hide it otherwise.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_passwordTextBox"/> in this case.</param>
		/// <param name="e">Arguments associated with the event.</param>
		private void _passwordTextBox_TextChanged(object sender, EventArgs e)
		{
			if (_passwordTextBox.SecureText != null && _passwordTextBox.SecureText.Length > 0)
				_inheritedPasswordLabel.Text = "";

			else
			{
				SecureString inheritedPassword = Connection.GetInheritedPassword(Connection.ParentFolder);

				_inheritedPasswordLabel.Text = inheritedPassword != null && inheritedPassword.Length > 0
					                               ? "Inheriting a password from parent folders"
					                               : "";
			}
		}
	}
}