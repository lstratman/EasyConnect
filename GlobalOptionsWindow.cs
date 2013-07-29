using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using EasyConnect.Protocols;

namespace EasyConnect
{
	/// <summary>
	/// Displays the global options for the application:  the default protocol and whether they want the toolbar to auto-hide.
	/// </summary>
	public partial class GlobalOptionsWindow : Form
	{
		/// <summary>
		/// If the user selected an encryption type of <see cref="EncryptionType.Rijndael"/>, this is the encryption key to use with that method.
		/// </summary>
		protected SecureString _encryptionPassword = null;

		/// <summary>
		/// Main application form for this window.
		/// </summary>
		protected MainForm _parentTabs = null;

		/// <summary>
		/// Default constructor; populates the protocol dropdown.
		/// </summary>
		public GlobalOptionsWindow()
		{
			InitializeComponent();

			foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
				_defaultProtocolDropdown.Items.Add(protocol);

			_defaultProtocolDropdown.SelectedItem = ConnectionFactory.GetDefaultProtocol();
		}

		/// <summary>
		/// If the user selected an encryption type of <see cref="EncryptionType.Rijndael"/>, this is the encryption key to use with that method.
		/// </summary>
		public SecureString EncryptionPassword
		{
			get
			{
				return _encryptionPassword;
			}
		}

		/// <summary>
		/// Handler method that's called when the form is closing; sets the properties for various options and then saves them.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void GlobalOptionsWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_parentTabs == null)
				return;

			ConnectionFactory.SetDefaultProtocol((IProtocol) _defaultProtocolDropdown.SelectedItem);

			_parentTabs.Options.AutoHideToolbar = _autoHideCheckbox.Checked;
			_parentTabs.Options.EncryptionType = (EncryptionType) Enum.Parse(typeof (EncryptionType), ((ListItem) _encryptionTypeDropdown.SelectedItem).Value);
			_parentTabs.Options.Save();
		}

		/// <summary>
		/// Handler method that's called when the form is shown; sets the state of <see cref="_autoHideCheckbox"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void GlobalOptionsWindow_Shown(object sender, EventArgs e)
		{
			_parentTabs = Parent.TopLevelControl as MainForm;
			_autoHideCheckbox.Checked = _parentTabs.Options.AutoHideToolbar;

			List<ListItem> items = new List<ListItem>
				                       {
					                       new ListItem
						                       {
							                       Text = "Password",
							                       Value = "Rijndael"
						                       },
					                       new ListItem
						                       {
							                       Text = "RSA Key Container",
							                       Value = "Rsa"
						                       }
				                       };

			_encryptionTypeDropdown.Items.AddRange(items.Cast<object>().ToArray());
			// ReSharper disable PossibleInvalidOperationException
			_encryptionTypeDropdown.SelectedItem = items.First(i => i.Value == _parentTabs.Options.EncryptionType.Value.ToString("G"));
			// ReSharper restore PossibleInvalidOperationException
		}

		/// <summary>
		/// Handler method that's called when the selected item in <see cref="_encryptionTypeDropdown"/> is changed.  If the user selected 
		/// <see cref="EncryptionType.Rijndael"/>, we prompt them to enter a password and save it to <see cref="_encryptionPassword"/>, otherwise we set
		/// <see cref="_encryptionPassword"/> to null.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_encryptionTypeDropdown"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _encryptionTypeDropdown_SelectedIndexChanged(object sender, EventArgs e)
		{
			if ((_encryptionTypeDropdown.SelectedItem as ListItem).Value == "Rsa")
				_encryptionPassword = null;

				// ReSharper disable PossibleInvalidOperationException
			else if ((_encryptionTypeDropdown.SelectedItem as ListItem).Value != _parentTabs.Options.EncryptionType.Value.ToString("G"))
				// ReSharper restore PossibleInvalidOperationException
			{
				PasswordWindow passwordWindow = new PasswordWindow();

				if (passwordWindow.ShowDialog(this) == DialogResult.OK)
					_encryptionPassword = passwordWindow.Password;
			}
		}

		/// <summary>
		/// Class to provide text and value members to <see cref="ComboBox"/> instances.
		/// </summary>
		protected class ListItem
		{
			/// <summary>
			/// Text that should be displayed for the item.
			/// </summary>
			public string Text
			{
				get;
				set;
			}

			/// <summary>
			/// Value that should be used for the item.
			/// </summary>
			public string Value
			{
				get;
				set;
			}
		}
	}
}