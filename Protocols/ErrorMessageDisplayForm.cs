using System;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
	/// <summary>
	/// Displays a dialog form with a scrollable text box that is populated with an error message that should be displayed to the user.
	/// </summary>
	public partial class ErrorMessageDisplayForm : Form
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public ErrorMessageDisplayForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Error message that should be displayed to the user.
		/// </summary>
		public string ErrorMessage
		{
			get
			{
				return _errorMessageTextBox.Text;
			}

			set
			{
				_errorMessageTextBox.Text = value;
			}
		}

		/// <summary>
		/// Event handler that's called when <see cref="_okButton"/> is clicked; closes the form.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _okButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}