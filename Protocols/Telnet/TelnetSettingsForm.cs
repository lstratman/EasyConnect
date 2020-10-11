using Poderosa.ConnectionParam;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Windows.Forms;

namespace EasyConnect.Protocols.Telnet
{
	/// <summary>
	/// Form that captures settings for a particular <see cref="TelnetConnection"/> instance or defaults for the <see cref="TelnetProtocol"/> protocol.
	/// </summary>
	public partial class TelnetSettingsForm : Form, ISettingsForm<TelnetConnection>
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
		public TelnetSettingsForm()
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
				Connection = (TelnetConnection) value;
			}
		}

		/// <summary>
		/// Connection instance that we're capturing option data for.
		/// </summary>
		public TelnetConnection Connection
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
		private void TelnetSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Connection.Host = _hostNameTextBox.Text;
			Connection.BackgroundColor = _backgroundColorPanel.BackColor;
			Connection.TextColor = _textColorPanel.BackColor;
			Connection.Font = _font;
            Connection.Encoding = _encodingTypes[_encodingDropdown.Text];
            Connection.Port = Convert.ToInt32(_portTextBox.Text);
		}

		/// <summary>
		/// Handler method that's called when the form loads initially.  Initializes the UI from the data in <see cref="Connection"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void TelnetSettingsForm_Load(object sender, EventArgs e)
		{
			Text = "Settings for " + Connection.DisplayName;

			_font = Connection.Font;
			_backgroundColorPanel.BackColor = Connection.BackgroundColor;
			_textColorPanel.BackColor = Connection.TextColor;
			_fontTextBox.Text = Connection.Font.FontFamily.GetName(0);
			_hostNameTextBox.Text = Connection.Host;
            _encodingDropdown.Text = _encodingTypes.Single(p => p.Value == Connection.Encoding).Key;
            _portTextBox.Text = Connection.Port.ToString();

			// Hide the host panel if we're in defaults mode
			if (DefaultsMode)
			{
				_hostNameLabel.Visible = false;
				_hostNameTextBox.Visible = false;
				_divider1.Visible = false;

				_settingsCard.Height -= 60;
				_settingsLayoutPanel.Height -= 60;
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
	}
}