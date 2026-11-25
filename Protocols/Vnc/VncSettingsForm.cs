using System;
using System.Drawing;
using System.Security;
using System.Windows.Forms;

namespace EasyConnect.Protocols.Vnc
{
	/// <summary>
	/// Form that captures settings for a particular <see cref="VncConnection"/> instance or defaults for the <see cref="VncProtocol"/> protocol.
	/// </summary>
	public partial class VncSettingsForm : Form, ISettingsForm<VncConnection>
	{
		private readonly CheckBox _useAmtKvm = new CheckBox();
		private readonly CheckBox _amtUseTls = new CheckBox();
		private readonly TextBox _amtUsername = new TextBox();
		private readonly TextBox _amtDomain = new TextBox();
		private readonly SecurePasswordTextBox.SecureTextBox _amtPassword = new SecurePasswordTextBox.SecureTextBox();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public VncSettingsForm()
		{
			InitializeComponent();
			InitializeAmtSettings();
		}

		private void InitializeAmtSettings()
		{
#if INTEL_KVM
			Label heading = new Label { Text = "Intel AMT KVM redirection", AutoSize = true, Font = _hostNameLabel.Font, Margin = _hostNameLabel.Margin };
#else
			Label heading = new Label { Text = "Intel AMT KVM redirection (not available in this build)", AutoSize = true, Font = _hostNameLabel.Font, Margin = _hostNameLabel.Margin };
#endif
			Common.MaterialCard card = new Common.MaterialCard { BackColor = Color.White, Size = new Size(692, 390), Margin = new Padding(15, 3, 3, 15) };
			TableLayoutPanel fields = new TableLayoutPanel { Location = new Point(5, 9), Size = new Size(682, 370), ColumnCount = 2, RowCount = 6, BackColor = Color.White };
			fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
			fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
			_useAmtKvm.Text = "Use AMT KVM redirection";
			_useAmtKvm.AutoSize = true;
			_amtUsername.Width = 184;
			_amtDomain.Width = 184;
			_amtPassword.Width = 184;
			_amtPassword.PasswordChar = '*';
			_amtUseTls.Text = "Use TLS (port 16995)";
			_amtUseTls.AutoSize = true;
			Control[] controls = { _useAmtKvm, _amtUsername, _amtDomain, _amtPassword, _amtUseTls };
			string[] labels = { "", "Username", "Domain (Kerberos)", "Password", "" };
			for (int i = 0; i < controls.Length; i++)
			{
				fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
				if (labels[i].Length != 0)
					fields.Controls.Add(new Label { Text = labels[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = _hostNameLabel.Font, Padding = new Padding(14, 0, 0, 0) }, 0, i);
				controls[i].Anchor = AnchorStyles.Left;
				fields.Controls.Add(controls[i], 1, i);
			}
			Label authentication = new Label
			{
				Text = "Authentication is negotiated automatically by Intel AMT. Leave credentials empty to use the current Windows identity. TLS validates the AMT certificate. The local proxy accepts unauthenticated connections from this computer.",
				AutoSize = false, Dock = DockStyle.Fill, Font = _hostNameLabel.Font, Padding = new Padding(14, 8, 8, 8)
			};
			fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
			fields.Controls.Add(authentication, 0, 5);
			fields.SetColumnSpan(authentication, 2);
			card.Controls.Add(fields);
			_rootLayoutPanel.AutoScroll = true;
			_rootLayoutPanel.WrapContents = false;
			_rootLayoutPanel.Controls.Add(heading);
			_rootLayoutPanel.Controls.Add(card);
			ClientSize = new Size(ClientSize.Width, 850);
			_useAmtKvm.CheckedChanged += (sender, args) => UpdateAmtControlState();
		}

		private void UpdateAmtControlState()
		{
#if INTEL_KVM
			_amtUsername.Enabled = _amtDomain.Enabled = _amtPassword.Enabled = _amtUseTls.Enabled = _useAmtKvm.Checked;
#else
			_useAmtKvm.Enabled = _useAmtKvm.Checked;
			_amtUsername.Enabled = _amtDomain.Enabled = _amtPassword.Enabled = _amtUseTls.Enabled = false;
#endif
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
			_useAmtKvm.Checked = Connection.UseAmtKvm;
			_amtUseTls.Checked = Connection.AmtUseTls;
			_amtUsername.Text = Connection.AmtUsername;
			_amtDomain.Text = Connection.AmtDomain;
			_amtPassword.SecureText = Connection.AmtPassword == null ? new SecureString() : Connection.AmtPassword.Copy();
			UpdateAmtControlState();

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
			Connection.UseAmtKvm = _useAmtKvm.Checked;
			Connection.AmtUseTls = _amtUseTls.Checked;
			Connection.AmtUsername = _amtUsername.Text;
			Connection.AmtDomain = _amtDomain.Text;
			Connection.AmtPassword = _amtPassword.SecureText;
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