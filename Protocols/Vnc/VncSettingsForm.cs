using System;
using System.Security;
using System.Windows.Forms;

namespace EasyConnect.Protocols.Vnc
{
	/// <summary>
	/// Form that captures settings for a particular <see cref="VncConnection"/> instance or defaults for the <see cref="VncProtocol"/> protocol.
	/// </summary>
	public partial class VncSettingsForm : Form, ISettingsForm<VncConnection>
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public VncSettingsForm()
		{
			InitializeComponent();
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
				Connection = (VncConnection) value;
			}
		}

		/// <summary>
		/// Connection instance that we're capturing option data for.
		/// </summary>
		public VncConnection Connection
		{
			get;
			set;
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
		private void VncSettingsForm_Load(object sender, EventArgs e)
		{
			Text = "Settings for " + Connection.DisplayName;

			// Initialize the values in the UI from the properties in the connection
			_hostNameTextBox.Text = Connection.Host;
			_portTextBox.Text = Connection.Port.ToString();

			_displayUpDown.Value = Connection.Display;
			_viewOnlyCheckbox.Checked = Connection.ViewOnly;
		    _clipboardCheckbox.Checked = Connection.ShareClipboard;
			_showLocalCursorCheckbox.Checked = Connection.ShowLocalCursor;

			if (!String.IsNullOrEmpty(Connection.PreferredEncoding))
			{
				_preferredEncodingDropdown.SelectedItem = Connection.PreferredEncoding;
			}

			_pictureQualitySlider.Value = Connection.PictureQuality;

			if (Connection.Password != null)
				_passwordTextBox.SecureText = Connection.Password;

			if ((Connection.Password == null || Connection.Password.Length == 0) && Connection.InheritedPassword != null && Connection.InheritedPassword.Length > 0)
			{
				_inheritedPasswordTextBox.Visible = true;
				_passwordTextBox.Visible = false;
			}

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
		private void VncSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Copy the values from the UI back into the connection object
			Connection.Host = _hostNameTextBox.Text;
			Connection.Port = Convert.ToInt32(_portTextBox.Text);
			Connection.Display = Convert.ToInt32(_displayUpDown.Value);
			Connection.ViewOnly = _viewOnlyCheckbox.Checked;
			Connection.Password = _passwordTextBox.SecureText;
		    Connection.ShareClipboard = _clipboardCheckbox.Checked;
			Connection.PictureQuality = _pictureQualitySlider.Value;
			Connection.ShowLocalCursor = _showLocalCursorCheckbox.Checked;
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