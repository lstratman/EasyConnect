using System;
using System.Security;
using System.Windows.Forms;
using ViewerX;

namespace EasyConnect.Protocols.Vnc
{
    public partial class VncOptionsForm : Form, IOptionsForm<VncConnection>
    {
        public VncOptionsForm()
        {
            InitializeComponent();
        }

        IConnection IOptionsForm.Connection
        {
            get
            {
                return Connection;
            }

            set
            {
                Connection = (VncConnection)value;
            }
        }

        public VncConnection Connection
        {
            get;
            set;
        }

        public bool DefaultsMode
        {
            get;
            set;
        }

        private void VncOptionsForm_Load(object sender, EventArgs e)
        {
            Text = "Options for " + Connection.DisplayName;

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
        }

        private void VncOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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
        }

        private void _userNameTextBox_TextChanged(object sender, EventArgs e)
        {
            _inheritedUsernameLabel.Text = String.IsNullOrEmpty(_userNameTextBox.Text) &&
                                           !String.IsNullOrEmpty(Connection.GetInheritedUsername(Connection.ParentFolder))
                                               ? "Inheriting " + Connection.InheritedUsername + " from parent folders"
                                               : "";
        }

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

		private void _keyFileBrowseButton_Click(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(_keyFileTextBox.Text))
				_openFileDialog.FileName = _keyFileTextBox.Text;

			switch (_encryptionTypeDropdown.SelectedIndex)
			{
				case 1:
					_openFileDialog.Filter = "MSRC4 key files (*.key)|*.key|All files (*.*)|*.*";
					break;

				case 2:
					_openFileDialog.Filter = "SecureVNC key files (*.pkey)|*.pkey|All files (*.*)|*.*";
					break;
			}

			DialogResult result = _openFileDialog.ShowDialog();

			if (result == DialogResult.OK)
				_keyFileTextBox.Text = _openFileDialog.FileName;
		}

		private void _encryptionTypeDropdown_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_encryptionTypeDropdown.SelectedIndex == 0)
			{
				_hostPanel.Height = 93;
				_keyFileLabel.Visible = false;
				_keyFileTextBox.Visible = false;
				_keyFileBrowseButton.Visible = false;
			}

			else
			{
				_hostPanel.Height = 119;
				_keyFileLabel.Visible = true;
				_keyFileTextBox.Visible = true;
				_keyFileBrowseButton.Visible = true;
			}
		}
    }
}
