using System;
using System.Windows.Forms;
using EasyConnect.Protocols;

namespace EasyConnect
{
    /// <summary>
    /// Displays the global options for the application:  the default protocol and whether they want the toolbar to auto-hide.
    /// </summary>
    public partial class GlobalOptionsWindow : Form
    {
        /// <summary>
        /// Main application form for this window.
        /// </summary>
        protected MainForm _parentTabs = null;

        /// <summary>
        /// Default constructor; populates the protocol dropdown.
        /// </summary>
        public GlobalOptionsWindow()
        {
            InitializeComponent();

            foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
                _defaultProtocolDropdown.Items.Add(protocol);

            _defaultProtocolDropdown.SelectedItem = ConnectionFactory.GetDefaultProtocol();
        }

        /// <summary>
        /// Handler method that's called when the form is closing; sets the properties for various options and then saves them.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void GlobalOptionsWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_parentTabs == null)
                return;

            ConnectionFactory.SetDefaultProtocol((IProtocol)_defaultProtocolDropdown.SelectedItem);
            
            _parentTabs.Options.AutoHideToolbar = _autoHideCheckbox.Checked;
            _parentTabs.Options.Save();
        }

        /// <summary>
        /// Handler method that's called when the form is shown; sets the state of <see cref="_autoHideCheckbox"/>.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void GlobalOptionsWindow_Shown(object sender, EventArgs e)
        {
            _parentTabs = Parent.TopLevelControl as MainForm;
            _autoHideCheckbox.Checked = _parentTabs.Options.AutoHideToolbar;
        }
    }
}
