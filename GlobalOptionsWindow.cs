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
        protected MainForm _parentTabs = null;

        public GlobalOptionsWindow()
        {
            InitializeComponent();

            foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
                _defaultProtocolDropdown.Items.Add(protocol);

            _defaultProtocolDropdown.SelectedItem = ConnectionFactory.GetDefaultProtocol();
        }

        private void GlobalOptionsWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_parentTabs == null)
                return;

            ConnectionFactory.SetDefaultProtocol((IProtocol)_defaultProtocolDropdown.SelectedItem);
            
            _parentTabs.Options.AutoHideToolbar = _autoHideCheckbox.Checked;
            _parentTabs.Options.Save();
        }

        private void GlobalOptionsWindow_Shown(object sender, EventArgs e)
        {
            _parentTabs = Parent.TopLevelControl as MainForm;
            _autoHideCheckbox.Checked = _parentTabs.Options.AutoHideToolbar;
        }
    }
}
