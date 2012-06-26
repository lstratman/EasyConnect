using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using EasyConnect.Common;
using EasyConnect.Protocols.Rdp;

namespace EasyConnect
{
    public partial class OptionsWindow : Form
    {
        protected MainForm _applicationForm = null;
        protected List<Form> _optionsForms = new List<Form>();

        public OptionsWindow(MainForm applicationForm)
        {
            _applicationForm = applicationForm;
            InitializeComponent();
        }

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

        private void OptionsWindow_Load(object sender, EventArgs e)
        {
            if (OptionsForms.Count > 0)
            {
                foreach (Form form in OptionsForms)
                {
                    Label formLabel = new Label
                        {
                            BackColor = Color.Transparent,
                            TextAlign = ContentAlignment.MiddleRight,
                            Padding = new Padding(0, 0, 17, 0),
                            Size = new Size(233, 33),
                            Text = form.Text + "    ",
                            Font = new Font("Arial", 9.75f),
                            Margin = new Padding(0),
                            Location = new Point(0, 0)
                        };

                    formLabel.Click += (o, args) => ShowOptionsForm(form, formLabel);
                    _sidebarFlowLayoutPanel.Controls.Add(formLabel);
                }

                ShowOptionsForm(OptionsForms[0], (Label)_sidebarFlowLayoutPanel.Controls[1]);
            }
        }

        private void ShowOptionsForm(Form optionsForm, Label navigationLabel)
        {
            if (navigationLabel.Image != null)
                return;

            optionsForm.FormBorderStyle = FormBorderStyle.None;
            optionsForm.TopLevel = false;
            optionsForm.Dock = DockStyle.Fill;

            _containerPanel.Controls.Clear();
            _containerPanel.Controls.Add(optionsForm);
            
            optionsForm.Show();

            foreach (Label label in _sidebarFlowLayoutPanel.Controls.Cast<Label>())
                label.Image = null;

            navigationLabel.Image = Properties.Resources.SelectedOptionCategoryBackground;
            navigationLabel.ImageAlign = ContentAlignment.MiddleCenter;
        }

        private void OptionsWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Form form in OptionsForms)
                form.Close();
        }
    }
}
