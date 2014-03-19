using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EasyConnect.Properties;

namespace EasyConnect
{
	/// <summary>
	/// UI that allows the user to edit the global options associated with the application and the defaults for each connection protocol.
	/// </summary>
	public partial class OptionsWindow : Form
	{
		/// <summary>
		/// Main application form instance associated with this window.
		/// </summary>
		protected MainForm _applicationForm = null;

		/// <summary>
		/// Lookup that associates each left navigation label with the child form that it belongs to.
		/// </summary>
		protected Dictionary<Label, Form> _optionsFormLabels = new Dictionary<Label, Form>();

		/// <summary>
		/// List of all of the child options forms; one for the global options and one for each connection protocol.
		/// </summary>
		protected List<Form> _optionsForms = new List<Form>();

		/// <summary>
		/// Constructor; initializes <see cref="_applicationForm"/>.
		/// </summary>
		/// <param name="applicationForm">Main application form instance associated with this window.</param>
		public OptionsWindow(MainForm applicationForm)
		{
			_applicationForm = applicationForm;
			InitializeComponent();
		}

		/// <summary>
		/// List of all of the child options forms; one for the global options and one for each connection protocol.
		/// </summary>
		public List<Form> OptionsForms
		{
			get
			{
				return _optionsForms;
			}

			set
			{
				_optionsForms = value;
			}
		}

		/// <summary>
		/// Handler method that's called when the form loads initially.  Takes all of the entries in <see cref="OptionsForms"/> and creates left navigation
		/// labels that, when clicked will display that option form.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void OptionsWindow_Load(object sender, EventArgs e)
		{
			if (OptionsForms.Count > 0)
			{
				foreach (Form form in OptionsForms)
				{
					Label formLabel = new Label
						                  {
											  BackColor = Color.FromArgb(249, 249, 249),
							                  TextAlign = ContentAlignment.MiddleLeft,
							                  Padding = new Padding(0, 0, 0, 0),
							                  Size = new Size(233, 29),
							                  Text = "       " + form.Text,
							                  Font = new Font("Segoe UI", 8.0f),
							                  Margin = new Padding(0),
							                  Location = new Point(0, 0),
							                  ForeColor = Color.FromArgb(153, 153, 153),
											  Cursor = Cursors.Hand
						                  };

					formLabel.Click += (o, args) => ShowOptionsForm(formLabel);

					_optionsFormLabels[formLabel] = form;
					_sidebarFlowLayoutPanel.Controls.Add(formLabel);
				}

				// Show the global options form
				ShowOptionsForm((Label) _sidebarFlowLayoutPanel.Controls[1]);
			}
		}

		/// <summary>
		/// Called when a left navigation label is clicked and opens the option form associated with that label.
		/// </summary>
		/// <param name="navigationLabel"></param>
		private void ShowOptionsForm(Label navigationLabel)
		{
			if (navigationLabel.Image != null)
				return;

			// Get and show the corresponding option form
			Form optionsForm = _optionsFormLabels[navigationLabel];

			optionsForm.FormBorderStyle = FormBorderStyle.None;
			optionsForm.TopLevel = false;
			optionsForm.Dock = DockStyle.Fill;

			_containerPanel.Controls.Clear();
			_containerPanel.Controls.Add(optionsForm);
			_containerPanel.AutoScrollMinSize = optionsForm.Size;

			optionsForm.Show();

			// Set the background image for the label to the "focused" image
			foreach (Label label in _sidebarFlowLayoutPanel.Controls.Cast<Label>().Where(c => c.Name != "_optionsLabel"))
			{
				label.Image = null;
				label.ForeColor = Color.FromArgb(153, 153, 153);
				label.Cursor = Cursors.Hand;
			}

			navigationLabel.Image = Resources.SelectedOptionCategoryBackground;
			navigationLabel.ImageAlign = ContentAlignment.MiddleCenter;
			navigationLabel.ForeColor = Color.FromArgb(92, 97, 102);
			navigationLabel.Cursor = Cursors.Default;
		}

		/// <summary>
		/// Handler method that's called when the window starts to close.  Calls <see cref="Form.Close"/> on each item in <see cref="OptionsForms"/>, which
		/// will cause each window to save its data.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void OptionsWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			foreach (Form form in OptionsForms)
				form.Close();
		}
	}
}