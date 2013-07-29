using System;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Windows.Forms;

namespace EasyConnect.Protocols.Ssh
{
	/// <summary>
	/// Form that captures options for a particular <see cref="SshConnection"/> instance or defaults for the <see cref="SshProtocol"/> protocol.
	/// </summary>
	public partial class SshOptionsForm : Form, IOptionsForm<SshConnection>
	{
		/// <summary>
		/// Font to be used when displaying the connection prompt.
		/// </summary>
		protected Font _font;

		/// <summary>
		/// Contains the previous width of <see cref="_flowLayoutPanel"/> prior to a resize operation.
		/// </summary>
		protected int _previousWidth;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public SshOptionsForm()
		{
			InitializeComponent();

			_previousWidth = _flowLayoutPanel.Width;
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
		/// Flag indicating if the options should be for a specific connection or should be for the defaults for the protocol (i.e. should not capture 
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
		private void SshOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Connection.Username = _userNameTextBox.Text;
			Connection.Host = _hostNameTextBox.Text;
			Connection.Password = _passwordTextBox.SecureText;
			Connection.BackgroundColor = _backgroundColorPanel.BackColor;
			Connection.TextColor = _textColorPanel.BackColor;
			Connection.IdentityFile = _identityFileTextbox.Text;
			Connection.Font = _font;
		}

		/// <summary>
		/// Handler method that's called when the form loads initially.  Initializes the UI from the data in <see cref="Connection"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void SshOptionsForm_Load(object sender, EventArgs e)
		{
			Text = "Options for " + Connection.DisplayName;

			_font = Connection.Font;
			_backgroundColorPanel.BackColor = Connection.BackgroundColor;
			_textColorPanel.BackColor = Connection.TextColor;
			_fontTextBox.Text = Connection.Font.FontFamily.GetName(0);
			_userNameTextBox.Text = Connection.Username;
			_hostNameTextBox.Text = Connection.Host;
			_identityFileTextbox.Text = Connection.IdentityFile;
			_passwordTextBox.SecureText = Connection.Password == null
				                              ? new SecureString()
				                              : Connection.Password.Copy();

			// Hide the host panel if we're in defaults mode
			if (DefaultsMode)
				_hostPanel.Visible = false;

			if (String.IsNullOrEmpty(Connection.Username) && !String.IsNullOrEmpty(Connection.InheritedUsername))
				_inheritedUsernameLabel.Text = "Inheriting " + Connection.InheritedUsername + " from parent folders";

			if ((Connection.Password == null || Connection.Password.Length == 0) && Connection.InheritedPassword != null && Connection.InheritedPassword.Length > 0)
				_inheritedPasswordLabel.Text = "Inheriting a password from parent folders";
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
		/// file path to <see cref="_identityFileTextbox"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_identityFileBrowseButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _identityFileBrowseButton_Click(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(_identityFileTextbox.Text))
				_openFileDialog.FileName = _identityFileTextbox.Text;

			DialogResult result = _openFileDialog.ShowDialog();

			if (result == DialogResult.OK)
				_identityFileTextbox.Text = _openFileDialog.FileName;
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