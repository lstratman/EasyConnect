using System;
using System.Security;
using System.Windows.Forms;

namespace EasyConnect
{
    public partial class UsernamePasswordWindow : Form
    {
        public UsernamePasswordWindow()
        {
            InitializeComponent();
        }

        public string Username
        {
            get
            {
                return _usernameTextBox.Text;
            }

            set
            {
                _usernameTextBox.Text = value;
            }
        }

        public SecureString Password
        {
            get
            {
                return _passwordTextBox.SecureText;
            }

            set
            {
                _passwordTextBox.SecureText = value;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void _usernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                okButton_Click(null, null);
        }

        private void _passwordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                okButton_Click(null, null);
        }

        private void UsernamePasswordWindow_Load(object sender, EventArgs e)
        {
            _usernameTextBox.Focus();
        }
    }
}
