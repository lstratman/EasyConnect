using System;
using System.Security;
using System.Windows.Forms;

namespace EasyConnect
{
	/// <summary>
	/// Displays a window that captures both a username and a password.
	/// </summary>
	public partial class UsernamePasswordWindow : Form
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public UsernamePasswordWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// The username that the user entered in the form.
		/// </summary>
		public string Username
		{
			get
			{
				return _usernameTextBox.Text;
			}

			set
			{
				_usernameTextBox.Text = value;
			}
		}

		/// <summary>
		/// Password that the user entered in the form.
		/// </summary>
		public SecureString Password
		{
			get
			{
				return _passwordTextBox.SecureText;
			}

			set
			{
				_passwordTextBox.SecureText = value;
			}
		}

		/// <summary>
		/// Handler method that's called when the user clicks <see cref="cancelButton"/>.  Sets <see cref="DialogResult"/> to <see cref="DialogResult.Cancel"/>
		/// and closes the window.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="cancelButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		/// <summary>
		/// Handler method that's called when the user clicks <see cref="okButton"/>.  Sets <see cref="DialogResult"/> to <see cref="DialogResult.OK"/> and 
		/// closes the window.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="okButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		/// <summary>
		/// Handler method that's called when the user presses a key while in the <see cref="_usernameTextBox"/> field.  If <see cref="KeyEventArgs.KeyCode"/>
		/// is <see cref="Keys.Enter"/>, we call <see cref="okButton_Click"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_usernameTextBox"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _usernameTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				okButton_Click(null, null);
		}

		/// <summary>
		/// Handler method that's called when the user presses a key while in the <see cref="_passwordTextBox"/> field.  If <see cref="KeyEventArgs.KeyCode"/>
		/// is <see cref="Keys.Enter"/>, we call <see cref="okButton_Click"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_passwordTextBox"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _passwordTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				okButton_Click(null, null);
		}

		/// <summary>
		/// Handler method that's called when the form loads; focuses on <see cref="_usernameTextBox"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void UsernamePasswordWindow_Load(object sender, EventArgs e)
		{
			_usernameTextBox.Focus();
		}
	}
}