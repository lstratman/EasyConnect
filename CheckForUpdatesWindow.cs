using System;
using System.Windows.Forms;
using wyDay.Controls;

namespace EasyConnect
{
    public partial class CheckForUpdatesWindow : Form
    {
        public CheckForUpdatesWindow()
        {
            InitializeComponent();
        }

        protected MainForm ParentTabs
        {
            get
            {
                return Owner as MainForm;
            }
        }

        private void CheckForUpdatesWindow_Shown(object sender, EventArgs e)
        {
            _progressBar.Style = ProgressBarStyle.Marquee;
            
            ParentTabs.AutomaticUpdater.UpToDate += AutomaticUpdater_UpToDate;
            ParentTabs.AutomaticUpdater.CheckingFailed += AutomaticUpdater_CheckingFailed;
            ParentTabs.AutomaticUpdater.UpdateAvailable += AutomaticUpdater_UpdateAvailable;

            ParentTabs.CheckForUpdate();
        }

        void AutomaticUpdater_UpdateAvailable(object sender, EventArgs e)
        {
            _statusLabel.Text = "An update is available.  Click \"OK\" to install.";
            _progressBar.Style = ProgressBarStyle.Blocks;
            _progressBar.Value = 100;

            _okButton.Enabled = true;
        }

        private void AutomaticUpdater_CheckingFailed(object sender, FailArgs e)
        {
            _statusLabel.Text = "Error occurred while checking for updates.  Please try again later.";
            _progressBar.Style = ProgressBarStyle.Blocks;
            _progressBar.Value = 100;

            _okButton.Enabled = true;
        }

        void AutomaticUpdater_UpToDate(object sender, SuccessArgs e)
        {
            _statusLabel.Text = "Application is up-to-date.";
            _progressBar.Style = ProgressBarStyle.Blocks;
            _progressBar.Value = 100;

            _okButton.Enabled = true;
        }

        private void _cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CheckForUpdatesWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            ParentTabs.AutomaticUpdater.UpToDate -= AutomaticUpdater_UpToDate;
            ParentTabs.AutomaticUpdater.CheckingFailed -= AutomaticUpdater_CheckingFailed;
            ParentTabs.AutomaticUpdater.UpdateAvailable -= AutomaticUpdater_UpdateAvailable;
        }

        private void _okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
