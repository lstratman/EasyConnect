using System;
using System.Security;
using System.Windows.Forms;

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

			_displayUpDown.Value = Connection.Display;
			_scaleCheckbox.Checked = Connection.Scale;
			_viewOnlyCheckbox.Checked = Connection.ViewOnly;

			if (Connection.Password != null)
				_passwordTextBox.SecureText = Connection.Password;

			if ((Connection.Password == null || Connection.Password.Length == 0) && Connection.InheritedPassword != null && Connection.InheritedPassword.Length > 0)
				_inheritedPasswordLabel.Text = "Inheriting a password from parent folders";
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
			Connection.Password = _passwordTextBox.SecureText;
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