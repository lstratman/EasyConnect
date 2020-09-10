using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EasyConnect.Properties;
using System.Configuration;
using TheArtOfDev.HtmlRenderer.WinForms;
using EasyConnect.Protocols;

namespace EasyConnect
{
	/// <summary>
	/// UI that allows the user to edit the global options associated with the application and the defaults for each connection protocol.
	/// </summary>
	public partial class SettingsWindow : Form
	{
		/// <summary>
		/// Main application form instance associated with this window.
		/// </summary>
		protected MainForm _applicationForm = null;

		/// <summary>
		/// Lookup that associates each left navigation label with the child form that it belongs to.
		/// </summary>
		protected Dictionary<Label, Form> _settingsFormLabels = new Dictionary<Label, Form>();

		/// <summary>
		/// List of all of the child settings forms; one for the global options and one for each connection protocol.
		/// </summary>
		protected List<Form> _settingsForms = new List<Form>();

        protected HtmlPanel _urlPanel;

		protected Color _focusedColor = Color.FromArgb(51, 103, 214);
		protected Color _unFocusedColor = Color.FromArgb(95, 99, 104);

		/// <summary>
		/// Constructor; initializes <see cref="_applicationForm"/>.
		/// </summary>
		/// <param name="applicationForm">Main application form instance associated with this window.</param>
		public SettingsWindow(MainForm applicationForm)
		{
			_applicationForm = applicationForm;
			InitializeComponent();

            _toolsMenu.Renderer = new EasyConnectToolStripRender();
            _iconPictureBox.Image = new Icon(Icon, 16, 16).ToBitmap();

            _urlPanel = new HtmlPanel
            {
                AutoScroll = false,
                Width = _urlPanelContainer.Width,
                Height = _urlPanelContainer.Height,
                Left = 0,
                Top = 0,
                Font = urlTextBox.Font,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
				BackColor = Color.FromArgb(241, 243, 244)
			};

            _urlPanelContainer.Controls.Add(_urlPanel);
            _urlPanel.Text = String.Format(
					@"<span style=""background-color: #F1F3F4; font-family: {2}; font-size: {1}pt; height: {0}px; color: #707172"">easyconnect://<font color=""black"">settings</font></span>",
                    _urlPanel.Height, urlTextBox.Font.SizeInPoints, urlTextBox.Font.FontFamily.GetName(0));

#if APPX
            _updatesMenuItem.Visible = false;
            _toolsMenuSeparator2.Visible = false;
#else
            _updatesMenuItem.Visible = ConfigurationManager.AppSettings["checkForUpdates"] != "false";
            _toolsMenuSeparator2.Visible = ConfigurationManager.AppSettings["checkForUpdates"] != "false";
#endif
        }

        /// <summary>
        /// List of all of the child settings forms; one for the global options and one for each connection protocol.
        /// </summary>
        public List<Form> SettingsForms
		{
			get
			{
				return _settingsForms;
			}

			set
			{
				_settingsForms = value;
			}
		}

		/// <summary>
		/// Handler method that's called when the form loads initially.  Takes all of the entries in <see cref="SettingsForms"/> and creates left navigation
		/// labels that, when clicked will display that settings form.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void SettingsWindow_Load(object sender, EventArgs e)
		{
			if (SettingsForms.Count > 0)
			{
				foreach (Form form in SettingsForms)
				{
					Icon icon = Resources.Tools;

					if (form is IOptionsForm)
                    {
						icon = ConnectionFactory.GetProtocol((form as IOptionsForm).Connection).ProtocolIcon;
                    }

					PictureBox formIcon = new PictureBox
					{
						Image = new Icon(icon, 16, 16).ToBitmap(),
						Width = 16,
						Height = 16,
						Location = new Point(0, 7)
					};

					Label formLabel = new Label
						                  {
							                  TextAlign = ContentAlignment.MiddleLeft,
							                  Padding = new Padding(0, 0, 0, 0),
							                  Size = new Size(233, 29),
							                  Text = form.Text,
							                  Font = new Font("Segoe UI Semibold", 10.0f),
							                  Location = new Point(43, 0),
							                  ForeColor = _unFocusedColor,
											  Cursor = Cursors.Hand
						                  };

					Panel formLabelPanel = new Panel
					{
						Size = new Size(300, 29),
						Margin = new Padding(0, 0, 0, 10)
					};

					formLabelPanel.Controls.Add(formIcon);
					formLabelPanel.Controls.Add(formLabel);

					formLabelPanel.Click += (o, args) => ShowSettingsForm(formLabel);
					formIcon.Click += (o, args) => ShowSettingsForm(formLabel);
					formLabel.Click += (o, args) => ShowSettingsForm(formLabel);

					_settingsFormLabels[formLabel] = form;
					_sidebarFlowLayoutPanel.Controls.Add(formLabelPanel);
				}

				// Show the global settings form
				ShowSettingsForm((Label) _sidebarFlowLayoutPanel.Controls[0].Controls[1]);
			}
		}

		/// <summary>
		/// Called when a left navigation label is clicked and opens the option form associated with that label.
		/// </summary>
		/// <param name="navigationLabel"></param>
		private void ShowSettingsForm(Label navigationLabel)
		{
			if (navigationLabel.ForeColor == _focusedColor)
				return;

			// Get and show the corresponding option form
			Form settingsForm = _settingsFormLabels[navigationLabel];

			settingsForm.FormBorderStyle = FormBorderStyle.None;
			settingsForm.TopLevel = false;
			settingsForm.Dock = DockStyle.Fill;

			_containerPanel.Controls.Clear();
			_containerPanel.Controls.Add(settingsForm);
			_containerPanel.AutoScrollMinSize = settingsForm.Size;

			settingsForm.Show();

            // Set the background image for the label to the "focused" image
            foreach (Label label in _settingsFormLabels.Keys)
            {
                label.ForeColor = _unFocusedColor;
                label.Cursor = Cursors.Hand;
            }

            navigationLabel.ForeColor = _focusedColor;
            navigationLabel.Cursor = Cursors.Default;
		}

        /// <summary>
		/// Main application instance that this window is associated with, which is used to call back into application functionality.
		/// </summary>
		protected MainForm ParentTabs
        {
            get
            {
                return (MainForm)Parent;
            }
        }

        /// <summary>
        /// Handler method that's called when the window starts to close.  Calls <see cref="Form.Close"/> on each item in <see cref="SettingsForms"/>, which
        /// will cause each window to save its data.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void SettingsWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			foreach (Form form in SettingsForms)
				form.Close();
		}

        /// <summary>
		/// Handler method that's called when the user's cursor goes over <see cref="_toolsButton"/>.  Sets the button's background to the standard
		/// "hover" image.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_toolsButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _toolsButton_MouseEnter(object sender, EventArgs e)
        {
            _toolsButton.BackgroundImage = Resources.ButtonHoverBackground;
        }

        /// <summary>
        /// Handler method that's called when the user's cursor leaves <see cref="_toolsButton"/>.  Sets the button's background to nothing.
        /// </summary>
        /// <param name="sender">Object from which this event originated, <see cref="_toolsButton"/> in this case.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void _toolsButton_MouseLeave(object sender, EventArgs e)
        {
            if (!_toolsMenu.Visible)
                _toolsButton.BackgroundImage = null;
        }

        /// <summary>
        /// Handler method that's called when the user clicks the "Exit" menu item in the tools menu.  Exits the entire application.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void _exitMenuItem_Click(object sender, EventArgs e)
        {
            ((Form)Parent).Close();
        }

        /// <summary>
        /// Handler method that's called when the user clicks the "Tools" icon in the toolbar.  Opens up <see cref="_toolsMenu"/>.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void _toolsButton_Click(object sender, EventArgs e)
        {
            _toolsButton.BackgroundImage = Resources.ButtonPressedBackground;
            _toolsMenu.DefaultDropDownDirection = ToolStripDropDownDirection.Left;
            _toolsMenu.Show(_toolsButton, -1 * _toolsMenu.Width + _toolsButton.Width, _toolsButton.Height);
        }

        private void _aboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog(ParentTabs);
        }

        /// <summary>
		/// Handler method that's called when the user clicks on the "Check for updates" menu item under the tools menu.  Starts the update check process by
		/// calling <see cref="MainForm.CheckForUpdate"/> on <see cref="ParentTabs"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _updatesMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.CheckForUpdate();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the "History" menu item under the tools menu.  Creates the history tab if one doesn't exist 
		/// already and then switches to it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.OpenHistory();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the <see cref="_newWindowMenuItem"/> in the tools menu.  Creates a new <see cref="MainForm"/>
		/// instance and opens it.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_newWindowMenuItem"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _newWindowMenuItem_Click(object sender, EventArgs e)
        {
            MainForm newWindow = new MainForm(new List<Guid>());
            ParentTabs.ApplicationContext.OpenWindow(newWindow);

            newWindow.Show();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the "New tab" menu item under the tools menu.  Creates a new tab and then switches to it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _newTabMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.AddNewTab();
        }

        private void _toolsMenu_VisibleChanged(object sender, EventArgs e)
        {
            if (!_toolsMenu.Visible)
                _toolsButton.BackgroundImage = null;
        }

        private void urlBackground_Resize(object sender, EventArgs e)
        {
            if (_urlPanel != null)
            {
                _urlPanel.AutoScroll = false;
            }
        }

		private void _toolsButton_MouseDown(object sender, MouseEventArgs e)
		{
			_toolsButton.BackgroundImage = Resources.ButtonPressedBackground;
		}
	}
}