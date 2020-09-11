using Poderosa.ConnectionParam;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Windows.Forms;

namespace EasyConnect.Protocols.Ssh
{
	/// <summary>
	/// Form that captures settings for a particular <see cref="SshConnection"/> instance or defaults for the <see cref="SshProtocol"/> protocol.
	/// </summary>
	public partial class SshSettingsForm : Form, ISettingsForm<SshConnection>
	{
		/// <summary>
		/// Font to be used when displaying the connection prompt.
		/// </summary>
		protected Font _font;

        private Dictionary<string, EncodingType> _encodingTypes = new Dictionary<string, EncodingType>
        {
            {"Big-5", EncodingType.BIG5},
            {"EUC-CN", EncodingType.EUC_CN},
            {"EUC-JP", EncodingType.EUC_JP},
            {"EUC-KR", EncodingType.EUC_KR},
            {"GB 2312", EncodingType.GB2312},
            {"ISO 8859-1", EncodingType.ISO8859_1},
            {"OEM 850", EncodingType.OEM850},
            {"Shift JIS", EncodingType.SHIFT_JIS},
            {"UTF-8", EncodingType.UTF8},
            {"UTF-8 Latin", EncodingType.UTF8_Latin}
        };

		/// <summary>
		/// Default constructor.
		/// </summary>
		public SshSettingsForm()
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
				Connection = (SshConnection) value;
			}
		}

		/// <summary>
		/// Connection instance that we're capturing option data for.
		/// </summary>
		public SshConnection Connection
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
		/// Handler method that's called when the form is closing.  Copies the contents of the various fields back into <see cref="Connection"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void SshSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Connection.Username = _userNameTextBox.Text;
			Connection.Host = _hostNameTextBox.Text;
			Connection.Password = _passwordTextBox.SecureText;
			Connection.BackgroundColor = _backgroundColorPanel.BackColor;
			Connection.TextColor = _textColorPanel.BackColor;
			Connection.IdentityFile = _identityFileTextBox.Text;
			Connection.Font = _font;
            Connection.Encoding = _encodingTypes[_encodingDropdown.Text];
            Connection.Port = Convert.ToInt32(_portTextBox.Text);
		}

		/// <summary>
		/// Handler method that's called when the form loads initially.  Initializes the UI from the data in <see cref="Connection"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void SshSettingsForm_Load(object sender, EventArgs e)
		{
			Text = "Settings for " + Connection.DisplayName;

			_font = Connection.Font;
			_backgroundColorPanel.BackColor = Connection.BackgroundColor;
			_textColorPanel.BackColor = Connection.TextColor;
			_fontTextBox.Text = Connection.Font.FontFamily.GetName(0);
			_userNameTextBox.Text = Connection.Username;
			_hostNameTextBox.Text = Connection.Host;
			_identityFileTextBox.Text = Connection.IdentityFile;
            _encodingDropdown.Text = _encodingTypes.Single(p => p.Value == Connection.Encoding).Key;
            _portTextBox.Text = Connection.Port.ToString();
			_passwordTextBox.SecureText = Connection.Password == null
				                              ? new SecureString()
				                              : Connection.Password.Copy();

			// Hide the host panel if we're in defaults mode
			if (DefaultsMode)
			{
				_hostNameLabel.Visible = false;
				_hostNameTextBox.Visible = false;
				_divider1.Visible = false;

				_settingsCard.Height -= 60;
				_settingsLayoutPanel.Height -= 60;
			}

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
		}

		/// <summary>
		/// Handler method that's called when the <see cref="_fontBrowseButton"/> is clicked.  Displays a font selection dialog and saves the resulting data
		/// to <see cref="_font"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_fontBrowseButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _fontBrowseButton_Click(object sender, EventArgs e)
		{
			_fontDialog.Font = _font;

			DialogResult result = _fontDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				_font = _fontDialog.Font;
				_fontTextBox.Text = _font.FontFamily.GetName(0);
			}
		}

		/// <summary>
		/// Handler method that's called when the <see cref="_backgroundColorPanel"/> is clicked.  Displays a color selection dialog and sets the 
		/// <see cref="Control.BackColor"/> of <see cref="_backgroundColorPanel"/> to the selected value.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_backgroundColorPanel"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _backgroundColorPanel_Click(object sender, EventArgs e)
		{
			_colorDialog.Color = _backgroundColorPanel.BackColor;

			DialogResult result = _colorDialog.ShowDialog();

			if (result == DialogResult.OK)
				_backgroundColorPanel.BackColor = _colorDialog.Color;
		}

		/// <summary>
		/// Handler method that's called when the <see cref="_textColorPanel"/> is clicked.  Displays a color selection dialog and sets the 
		/// <see cref="Control.BackColor"/> of <see cref="_textColorPanel"/> to the selected value.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_textColorPanel"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _textColorPanel_Click(object sender, EventArgs e)
		{
			_colorDialog.Color = _textColorPanel.BackColor;

			DialogResult result = _colorDialog.ShowDialog();

			if (result == DialogResult.OK)
				_textColorPanel.BackColor = _colorDialog.Color;
		}

		/// <summary>
		/// Handler method that's called when the <see cref="_identityFileBrowseButton"/> is clicked.  Displays a file selection dialog and saves the resulting 
		/// file path to <see cref="_identityFileTextBox"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_identityFileBrowseButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _identityFileBrowseButton_Click(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(_identityFileTextBox.Text))
				_openFileDialog.FileName = _identityFileTextBox.Text;

			DialogResult result = _openFileDialog.ShowDialog();

			if (result == DialogResult.OK)
				_identityFileTextBox.Text = _openFileDialog.FileName;
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