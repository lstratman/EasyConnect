using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using Win32Interop.Methods;
using Win32Interop.Structs;

namespace EasyConnect.Protocols.Rdp
{
	/// <summary>
	/// Form that captures settings for a particular <see cref="RdpConnection"/> instance or defaults for the <see cref="RdpProtocol"/> protocol.
	/// </summary>
	public partial class RdpSettingsForm : Form, ISettingsForm<RdpConnection>
	{
		/// <summary>
		/// List of all possible desktop resolutions.
		/// </summary>
		protected List<DEVMODE> _resolutions = new List<DEVMODE>();

		private readonly CheckBox _useAmtControl = new CheckBox();
		private readonly CheckBox _amtUseTls = new CheckBox();
		private readonly ComboBox _amtAuthentication = new ComboBox();
		private readonly TextBox _amtUsername = new TextBox();
		private readonly SecurePasswordTextBox.SecureTextBox _amtPassword = new SecurePasswordTextBox.SecureTextBox();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public RdpSettingsForm()
		{
			InitializeComponent();
			InitializeAmtSettings();
		}

		private void InitializeAmtSettings()
		{
#if INTEL_AMT
			Label heading = new Label { Text = "AMT Control", AutoSize = true, Font = _gatewayLabel.Font, Margin = _gatewayLabel.Margin };
#else
			Label heading = new Label { Text = "AMT Control (not available in this build)", AutoSize = true, Font = _gatewayLabel.Font, Margin = _gatewayLabel.Margin };
#endif
			Common.MaterialCard card = new Common.MaterialCard { BackColor = Color.White, Size = new Size(692, 315), Margin = new Padding(15, 3, 3, 3) };
			TableLayoutPanel fields = new TableLayoutPanel
			{
				Location = new Point(5, 9), Size = new Size(682, 295), ColumnCount = 2, RowCount = 5,
				BackColor = Color.White
			};
			fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
			fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
			_useAmtControl.AutoSize = true;
			_amtUseTls.AutoSize = true;
			_amtAuthentication.DropDownStyle = ComboBoxStyle.DropDownList;
			_amtAuthentication.Items.AddRange(new object[] { "Digest", "Kerberos" });
			_amtUsername.Width = 154;
			_amtPassword.Width = 154;
			_amtPassword.PasswordChar = '*';
			Control[] controls = { _useAmtControl, _amtUsername, _amtPassword, _amtUseTls, _amtAuthentication };
			string[] labels = { "Use AMT Control", "Username", "Password", "Use TLS", "Authentication" };
			for (int i = 0; i < controls.Length; i++)
			{
				fields.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
				fields.Controls.Add(new Label { Text = labels[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = _gatewayLabel.Font, Padding = new Padding(14, 0, 0, 0) }, 0, i);
				controls[i].Anchor = AnchorStyles.Left;
				fields.Controls.Add(controls[i], 1, i);
			}
			card.Controls.Add(fields);
			_rootLayoutPanel.Controls.Add(heading);
			_rootLayoutPanel.Controls.Add(card);
			_rootLayoutPanel.Height += heading.PreferredHeight + heading.Margin.Vertical + card.Height + card.Margin.Vertical;
			_useAmtControl.CheckedChanged += (sender, args) =>
			{
#if INTEL_AMT
				_amtUsername.Enabled = _amtPassword.Enabled = _amtUseTls.Enabled = _amtAuthentication.Enabled = _useAmtControl.Checked;
#else
				_useAmtControl.Enabled = _useAmtControl.Checked;
#endif
			};
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
		IConnection ISettingsForm.Connection
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
		/// Flag indicating if the settings should be for a specific connection or should be for the defaults for the protocol (i.e. should not capture 
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
		private void RdpSettingsForm_Load(object sender, EventArgs e)
		{
			Text = "Settings for " + Connection.DisplayName;

			_hostNameTextBox.Text = Connection.Host;
			_userNameTextBox.Text = Connection.Username;
			_passwordTextBox.SecureText = (Connection.Password == null
				                               ? new SecureString()
				                               : Connection.Password.Copy());

			if (String.IsNullOrEmpty(Connection.Username) && !String.IsNullOrEmpty(Connection.InheritedUsername))
			{
				_userNameTextBox.ForeColor = Color.LightGray;
				_userNameTextBox.Text = "Inheriting " + Connection.InheritedUsername;
			}

			if ((Connection.Password == null || Connection.Password.Length == 0) && Connection.InheritedPassword != null && Connection.InheritedPassword.Length > 0)
			{
				_inheritedPasswordTextBox.Visible = true;
				_passwordTextBox.Visible = false;
			}

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
            _smartCardsCheckBox.Checked = Connection.ConnectSmartCards;
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
			_amtUsername.Text = Connection.AmtUsername;
			_amtPassword.SecureText = Connection.AmtPassword == null ? new SecureString() : Connection.AmtPassword.Copy();
			_amtUseTls.Checked = Connection.AmtUseTls;
			_amtAuthentication.SelectedIndex = (int)Connection.AmtAuthentication;
			_useAmtControl.Checked = Connection.UseAmtControl;
#if INTEL_AMT
			_amtUsername.Enabled = _amtPassword.Enabled = _amtUseTls.Enabled = _amtAuthentication.Enabled = _useAmtControl.Checked;
#else
			_useAmtControl.Enabled = _useAmtControl.Checked;
			_amtUsername.Enabled = _amtPassword.Enabled = _amtUseTls.Enabled = _amtAuthentication.Enabled = false;
#endif

			_portTextBox.Text = Connection.Port.ToString();

			// Hide the host panel if we're in defaults mode
			if (DefaultsMode)
			{
				_hostNameLabel.Visible = false;
				_hostNameTextBox.Visible = false;
				_divider1.Visible = false;

				_settingsCard.Height -= 60;
				_settingsLayoutPanel.Height -= 60;
				_rootLayoutPanel.Height -= 60;
			}
		}

		/// <summary>
		/// Handler method that's called when the form is closing.  Copies the contents of the various fields back into <see cref="Connection"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void RdpSettingsForm_FormClosing(object sender, CancelEventArgs e)
		{
			Connection.Host = _hostNameTextBox.Text;
			Connection.Username = _userNameTextBox.ForeColor == Color.LightGray ? "" : _userNameTextBox.Text;
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
            Connection.ConnectSmartCards = _smartCardsCheckBox.Checked;
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
			Connection.UseAmtControl = _useAmtControl.Checked;
			Connection.AmtUsername = _amtUsername.Text;
			Connection.AmtPassword = _amtPassword.SecureText;
			Connection.AmtUseTls = _amtUseTls.Checked;
			Connection.AmtAuthentication = (AmtAuthentication)_amtAuthentication.SelectedIndex;

			int port = 0;

            if (Int32.TryParse(_portTextBox.Text, out port))
		        Connection.Port = port;
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

		private void _userNameTextBox_Leave(object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty(_userNameTextBox.Text) && !String.IsNullOrEmpty(Connection.InheritedUsername))
			{
				_userNameTextBox.ForeColor = Color.LightGray;
				_userNameTextBox.Text = "Inheriting " + Connection.InheritedUsername;
			}
		}

		private void _userNameTextBox_Enter(object sender, EventArgs e)
		{
			if (_userNameTextBox.ForeColor == Color.LightGray)
			{
				_userNameTextBox.ForeColor = Color.Black;
				_userNameTextBox.Text = "";
			}
		}

		private void _inheritedPasswordTextBox_Enter(object sender, EventArgs e)
		{
			_inheritedPasswordTextBox.Visible = false;
			_passwordTextBox.Visible = true;
			_passwordTextBox.Focus();
		}

		private void _passwordTextBox_Leave(object sender, EventArgs e)
		{
			if (_passwordTextBox.Focused)
			{
				return;
			}

			if (_passwordTextBox.SecureText == null || _passwordTextBox.SecureText.Length == 0)
			{
				SecureString inheritedPassword = Connection.GetInheritedPassword(Connection.ParentFolder);

				if (inheritedPassword != null && inheritedPassword.Length > 0)
				{
					_passwordTextBox.Visible = false;
					_inheritedPasswordTextBox.Visible = true;
				}
			}
		}
	}
}