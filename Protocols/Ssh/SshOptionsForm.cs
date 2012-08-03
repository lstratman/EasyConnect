using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;
using Poderosa;
using Poderosa.Config;

namespace EasyConnect.Protocols.Ssh
{
    public partial class SshOptionsForm : Form, IOptionsForm<SshConnection>
    {
        protected int _previousWidth;

        public SshOptionsForm()
        {
            InitializeComponent();

            _previousWidth = _flowLayoutPanel.Width;
        }

        IConnection IOptionsForm.Connection
        {
            get
            {
                return Connection;
            }
            set
            {
                Connection = (SshConnection)value;
            }
        }

        public SshConnection Connection
        {
            get;
            set;
        }

        public bool DefaultsMode
        {
            get;
            set;
        }

        private void SshOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Connection.Username = _userNameTextBox.Text;
            Connection.Host = _hostNameTextBox.Text;
            Connection.Password = _passwordTextBox.SecureText;
        }

        private void SshOptionsForm_Load(object sender, EventArgs e)
        {
            Text = "Options for " + Connection.DisplayName;

            _userNameTextBox.Text = Connection.Username;
            _hostNameTextBox.Text = Connection.Host;
            _passwordTextBox.SecureText = Connection.Password == null
                                              ? new SecureString()
                                              : Connection.Password.Copy();

            if (DefaultsMode)
            {
                _hostPanel.Visible = false;
                _hostDividerPanel.Visible = false;
            }
        }

        private void _flowLayoutPanel_Resize(object sender, EventArgs e)
        {
            foreach (Panel panel in _flowLayoutPanel.Controls.Cast<Panel>())
                panel.Width += _flowLayoutPanel.Width - _previousWidth;

            _previousWidth = _flowLayoutPanel.Width;
        }
    }
}
