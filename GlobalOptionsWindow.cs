using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EasyConnect.Protocols;

namespace EasyConnect
{
    public partial class GlobalOptionsWindow : Form
    {
        public GlobalOptionsWindow()
        {
            InitializeComponent();

            foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
                _defaultProtocolDropdown.Items.Add(protocol);

            _defaultProtocolDropdown.SelectedItem = ConnectionFactory.GetDefaultProtocol();
        }

        private void GlobalOptionsWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            ConnectionFactory.SetDefaultProtocol((IProtocol)_defaultProtocolDropdown.SelectedItem);
        }
    }
}
