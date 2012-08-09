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
        protected Font _font;

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
            Connection.BackgroundColor = _backgroundColorPanel.BackColor;
            Connection.TextColor = _textColorPanel.BackColor;
            Connection.IdentityFile = _identityFileTextbox.Text;
            Connection.Font = _font;
        }

        private void SshOptionsForm_Load(object sender, EventArgs e)
        {
            Text = "Options for " + Connection.DisplayName;

            _font = Connection.Font;
            _backgroundColorPanel.BackColor = Connection.BackgroundColor;
            _textColorPanel.BackColor = Connection.TextColor;
            _fontTextBox.Text = Connection.Font.FontFamily.GetName(0);
            _userNameTextBox.Text = Connection.Username;
            _hostNameTextBox.Text = Connection.Host;
            _identityFileTextbox.Text = Connection.IdentityFile;
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

        private void _backgroundColorPanel_Click(object sender, EventArgs e)
        {
            _colorDialog.Color = _backgroundColorPanel.BackColor;

            DialogResult result = _colorDialog.ShowDialog();

            if (result == DialogResult.OK)
                _backgroundColorPanel.BackColor = _colorDialog.Color;
        }

        private void _textColorPanel_Click(object sender, EventArgs e)
        {
            _colorDialog.Color = _textColorPanel.BackColor;

            DialogResult result = _colorDialog.ShowDialog();

            if (result == DialogResult.OK)
                _textColorPanel.BackColor = _colorDialog.Color;
        }

        private void _identityFileBrowseButton_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(_identityFileTextbox.Text))
                _openFileDialog.FileName = _identityFileTextbox.Text;

            DialogResult result = _openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
                _identityFileTextbox.Text = _openFileDialog.FileName;
        }
    }
}
