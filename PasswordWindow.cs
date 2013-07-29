using System;
using System.Security;
using System.Windows.Forms;

namespace EasyConnect
{
	/// <summary>
	/// Modal window that allows the user to enter a password and makes that password available to the calling window via the <see cref="Password"/>
	/// property.
	/// </summary>
	public partial class PasswordWindow : Form
	{
		/// <summary>
		/// Default constructor; hides the cancel button.
		/// </summary>
		public PasswordWindow()
		{
			InitializeComponent();

			cancelButton.Visible = false;
		}

		/// <summary>
		/// Flag indicating whether we should display the cancel button.
		/// </summary>
		public bool ShowCancelButton
		{
			get
			{
				return cancelButton.Visible;
			}

			set
			{
				// Move the OK button on top of the cancel button if we're hiding the cancel button
				okButton.Left = value
					                ? cancelButton.Left - okButton.Width - 7
					                : cancelButton.Left;

				cancelButton.Visible = value;
			}
		}

		/// <summary>
		/// Password entered by the user.
		/// </summary>
		public SecureString Password
		{
			get
			{
				return passwordTextBox.SecureText;
			}
		}

		/// <summary>
		/// Handler method that's called when the form is shown; focuses on <see cref="passwordTextBox"/>.
		/// </summary>
		/// <param name="e">Arguments associated with this event.</param>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			passwordTextBox.Focus();
		}

		/// <summary>
		/// Handler method that's called when the user clicks <see cref="okButton"/>; sets <see cref="DialogResult"/> to <see cref="DialogResult.OK"/> and
		/// closes the form.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="okButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		/// <summary>
		/// Handler method that's called when the user presses a key while in <see cref="passwordTextBox"/>; if that key is <see cref="Keys.Enter"/>, we 
		/// simulate the clicking of <see cref="okButton"/> by calling <see cref="okButton_Click"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="passwordTextBox"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void passwordTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				okButton_Click(null, null);
		}

		/// <summary>
		/// Handler method that's called when the user clicks <see cref="cancelButton"/>; sets <see cref="DialogResult"/> to <see cref="DialogResult.Cancel"/>
		/// and closes the form.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="cancelButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}