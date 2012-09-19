using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security;
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

            if (String.IsNullOrEmpty(Connection.Username) && !String.IsNullOrEmpty(Connection.InheritedUsername))
                _inheritedUsernameLabel.Text = "Inheriting " + Connection.InheritedUsername + " from parent folders";

            if ((Connection.Password == null || Connection.Password.Length == 0) && Connection.InheritedPassword != null && Connection.InheritedPassword.Length > 0)
                _inheritedPasswordLabel.Text = "Inheriting a password from parent folders";
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

        private void _userNameTextBox_TextChanged(object sender, EventArgs e)
        {
            _inheritedUsernameLabel.Text = String.IsNullOrEmpty(_userNameTextBox.Text) &&
                                           !String.IsNullOrEmpty(Connection.GetInheritedUsername(Connection.ParentFolder))
                                               ? "Inheriting " + Connection.InheritedUsername + " from parent folders"
                                               : "";
        }

        private void _passwordTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_passwordTextBox.SecureText != null && _passwordTextBox.SecureText.Length > 0)
                _inheritedPasswordLabel.Text = "";

            else
            {
                SecureString inheritedPassword = Connection.GetInheritedPassword(Connection.ParentFolder);

                _inheritedPasswordLabel.Text = inheritedPassword != null && inheritedPassword.Length > 0
                                                   ? "Inheriting a password from parent folders"
                                                   : "";
            }
        }
    }
}
