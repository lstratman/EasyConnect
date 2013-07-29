using System;
using System.Security;
using System.Windows.Forms;
using ViewerX;
using ColorDepth = ViewerX.ColorDepth;

namespace EasyConnect.Protocols.Vnc
{
	/// <summary>
	/// Form that captures options for a particular <see cref="VncConnection"/> instance or defaults for the <see cref="VncProtocol"/> protocol.
	/// </summary>
	public partial class VncOptionsForm : Form, IOptionsForm<VncConnection>
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public VncOptionsForm()
		{
			InitializeComponent();
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
		private void VncOptionsForm_Load(object sender, EventArgs e)
		{
			Text = "Options for " + Connection.DisplayName;

			// Initialize the values in the UI from the properties in the connection
			_hostNameTextBox.Text = Connection.Host;
			_portUpDown.Value = Connection.Port;
			_userNameTextBox.Text = Connection.Username;

			_displayUpDown.Value = Connection.Display;
			_scaleCheckbox.Checked = Connection.Scale;
			_viewOnlyCheckbox.Checked = Connection.ViewOnly;

			if (Connection.Password != null)
				_passwordTextBox.SecureText = Connection.Password;

			if (String.IsNullOrEmpty(Connection.Username) && !String.IsNullOrEmpty(Connection.InheritedUsername))
				_inheritedUsernameLabel.Text = "Inheriting " + Connection.InheritedUsername + " from parent folders";

			if ((Connection.Password == null || Connection.Password.Length == 0) && Connection.InheritedPassword != null && Connection.InheritedPassword.Length > 0)
				_inheritedPasswordLabel.Text = "Inheriting a password from parent folders";

			switch (Connection.AuthenticationType)
			{
				case ViewerLoginType.VLT_VNC:
					_authenticationTypeDropdown.SelectedIndex = 0;
					break;

				case ViewerLoginType.VLT_MSWIN:
					_authenticationTypeDropdown.SelectedIndex = 1;
					break;
			}

			switch (Connection.EncryptionType)
			{
				case EncryptionPluginType.EPT_NONE:
					_encryptionTypeDropdown.SelectedIndex = 0;
					break;

				case EncryptionPluginType.EPT_MSRC4:
					_encryptionTypeDropdown.SelectedIndex = 1;
					break;

				case EncryptionPluginType.EPT_SECUREVNC:
					_encryptionTypeDropdown.SelectedIndex = 2;
					break;
			}

			_keyFileTextBox.Text = Connection.KeyFile;

			switch (Connection.EncodingType)
			{
				case VNCEncoding.RFB_RAW:
					_encodingTypeDropdown.SelectedIndex = 0;
					break;

				case VNCEncoding.RFB_RRE:
					_encodingTypeDropdown.SelectedIndex = 1;
					break;

				case VNCEncoding.RFB_CORRE:
					_encodingTypeDropdown.SelectedIndex = 2;
					break;

				case VNCEncoding.RFB_HEXTILE:
					_encodingTypeDropdown.SelectedIndex = 3;
					break;

				case VNCEncoding.RFB_ZLIB:
					_encodingTypeDropdown.SelectedIndex = 4;
					break;

				case VNCEncoding.RFB_TIGHT:
					_encodingTypeDropdown.SelectedIndex = 5;
					break;

				case VNCEncoding.RFB_ZLIBHEX:
					_encodingTypeDropdown.SelectedIndex = 6;
					break;

				case VNCEncoding.RFB_ULTRA:
					_encodingTypeDropdown.SelectedIndex = 7;
					break;

				case VNCEncoding.RFB_ZRLE:
					_encodingTypeDropdown.SelectedIndex = 8;
					break;

				case VNCEncoding.RFB_ZYWRLE:
					_encodingTypeDropdown.SelectedIndex = 9;
					break;
			}

			switch (Connection.ColorDepth)
			{
				case ColorDepth.COLOR_FULL:
					_colorDepthDropdown.SelectedIndex = 0;
					break;

				case ColorDepth.COLOR_256:
					_colorDepthDropdown.SelectedIndex = 1;
					break;

				case ColorDepth.COLOR_64:
					_colorDepthDropdown.SelectedIndex = 2;
					break;

				case ColorDepth.COLOR_8:
					_colorDepthDropdown.SelectedIndex = 3;
					break;
			}
		}

		/// <summary>
		/// Handler method that's called when the form is closing.  Copies the contents of the various fields back into <see cref="Connection"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void VncOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Copy the values from the UI back into the connection object
			Connection.Host = _hostNameTextBox.Text;
			Connection.Port = Convert.ToInt32(_portUpDown.Value);
			Connection.Display = Convert.ToInt32(_displayUpDown.Value);
			Connection.Scale = _scaleCheckbox.Checked;
			Connection.ViewOnly = _viewOnlyCheckbox.Checked;
			Connection.Username = _userNameTextBox.Text;
			Connection.Password = _passwordTextBox.SecureText;

			switch (_authenticationTypeDropdown.SelectedIndex)
			{
				case 0:
					Connection.AuthenticationType = ViewerLoginType.VLT_VNC;
					break;

				case 1:
					Connection.AuthenticationType = ViewerLoginType.VLT_MSWIN;
					break;
			}

			switch (_encryptionTypeDropdown.SelectedIndex)
			{
				case 0:
					Connection.EncryptionType = EncryptionPluginType.EPT_NONE;
					Connection.KeyFile = null;
					break;

				case 1:
					Connection.EncryptionType = EncryptionPluginType.EPT_MSRC4;
					Connection.KeyFile = _keyFileTextBox.Text;
					break;

				case 2:
					Connection.EncryptionType = EncryptionPluginType.EPT_SECUREVNC;
					Connection.KeyFile = _keyFileTextBox.Text;
					break;
			}

			switch (_encodingTypeDropdown.SelectedIndex)
			{
				case 0:
					Connection.EncodingType = VNCEncoding.RFB_RAW;
					break;

				case 1:
					Connection.EncodingType = VNCEncoding.RFB_RRE;
					break;

				case 2:
					Connection.EncodingType = VNCEncoding.RFB_CORRE;
					break;

				case 3:
					Connection.EncodingType = VNCEncoding.RFB_HEXTILE;
					break;

				case 4:
					Connection.EncodingType = VNCEncoding.RFB_ZLIB;
					break;

				case 5:
					Connection.EncodingType = VNCEncoding.RFB_TIGHT;
					break;

				case 6:
					Connection.EncodingType = VNCEncoding.RFB_ZLIBHEX;
					break;

				case 7:
					Connection.EncodingType = VNCEncoding.RFB_ULTRA;
					break;

				case 8:
					Connection.EncodingType = VNCEncoding.RFB_ZRLE;
					break;

				case 9:
					Connection.EncodingType = VNCEncoding.RFB_ZYWRLE;
					break;
			}

			switch (_colorDepthDropdown.SelectedIndex)
			{
				case 0:
					Connection.ColorDepth = ColorDepth.COLOR_FULL;
					break;

				case 1:
					Connection.ColorDepth = ColorDepth.COLOR_256;
					break;

				case 2:
					Connection.ColorDepth = ColorDepth.COLOR_64;
					break;

				case 3:
					Connection.ColorDepth = ColorDepth.COLOR_8;
					break;
			}
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

		/// <summary>
		/// Handler method that's called when <see cref="_keyFileBrowseButton"/> is clicked.  Displays the file browse dialog that allows the user to choose
		/// the key file to use during connection encryption.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_keyFileBrowseButton"/> in this case.</param>
		/// <param name="e">Arguments associated with the event.</param>
		private void _keyFileBrowseButton_Click(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(_keyFileTextBox.Text))
				_openFileDialog.FileName = _keyFileTextBox.Text;

			switch (_encryptionTypeDropdown.SelectedIndex)
			{
					// Set the extension filter to *.key for MSRC4 encryption
				case 1:
					_openFileDialog.Filter = "MSRC4 key files (*.key)|*.key|All files (*.*)|*.*";
					break;

					// Set the extension filter to *.pkey for SecureVNC encryption
				case 2:
					_openFileDialog.Filter = "SecureVNC key files (*.pkey)|*.pkey|All files (*.*)|*.*";
					break;
			}

			DialogResult result = _openFileDialog.ShowDialog();

			if (result == DialogResult.OK)
				_keyFileTextBox.Text = _openFileDialog.FileName;
		}

		/// <summary>
		/// Handler method that's called when the selected item in <see cref="_encryptionTypeDropdown"/> changes.  If "None" was the item selected, hide
		/// <see cref="_keyFileLabel"/>, <see cref="_keyFileTextBox"/>, and <see cref="_keyFileBrowseButton"/>, show those fields otherwise.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_encryptionTypeDropdown"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _encryptionTypeDropdown_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_encryptionTypeDropdown.SelectedIndex == 0)
			{
				_hostPanel.Height = 123;
				_keyFileLabel.Visible = false;
				_keyFileTextBox.Visible = false;
				_keyFileBrowseButton.Visible = false;
			}

			else
			{
				_hostPanel.Height = 149;
				_keyFileLabel.Visible = true;
				_keyFileTextBox.Visible = true;
				_keyFileBrowseButton.Visible = true;
			}
		}
	}
}