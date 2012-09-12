using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols.Vnc
{
    public partial class VncOptionsForm : Form, IOptionsForm<VncConnection>
    {
        public VncOptionsForm()
        {
            InitializeComponent();
        }

        IConnection IOptionsForm.Connection
        {
            get
            {
                return Connection;
            }

            set
            {
                Connection = (VncConnection)value;
            }
        }

        public VncConnection Connection
        {
            get;
            set;
        }

        public bool DefaultsMode
        {
            get;
            set;
        }

        private void VncOptionsForm_Load(object sender, EventArgs e)
        {
            Text = "Options for " + Connection.DisplayName;

            _hostNameTextBox.Text = Connection.Host;
            _portUpDown.Value = Connection.Port;
            _userNameTextBox.Text = Connection.Username;

            _displayUpDown.Value = Connection.Display;
            _scaleCheckbox.Checked = Connection.Scale;
            _viewOnlyCheckbox.Checked = Connection.ViewOnly;

            if (Connection.Password != null)
                _passwordTextBox.SecureText = Connection.Password;
        }

        private void VncOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Connection.Host = _hostNameTextBox.Text;
            Connection.Port = Convert.ToInt32(_portUpDown.Value);
            Connection.Display = Convert.ToInt32(_displayUpDown.Value);
            Connection.Scale = _scaleCheckbox.Checked;
            Connection.ViewOnly = _viewOnlyCheckbox.Checked;
            Connection.Username = _userNameTextBox.Text;
            Connection.Password = _passwordTextBox.SecureText;
        }
    }
}
