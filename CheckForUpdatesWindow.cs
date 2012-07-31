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
            ParentTabs.AutomaticUpdater.UpdateAvailable += AutomaticUpdater_ReadyToBeInstalled;
            ParentTabs.AutomaticUpdater.BeforeDownloading += AutomaticUpdater_BeforeDownloading;
            ParentTabs.AutomaticUpdater.ProgressChanged += AutomaticUpdater_ProgressChanged;
            ParentTabs.AutomaticUpdater.ReadyToBeInstalled += AutomaticUpdater_ReadyToBeInstalled;

            if (!ParentTabs.CheckForUpdate())
            {
                if (ParentTabs.AutomaticUpdater.UpdateStepOn == UpdateStepOn.UpdateDownloaded)
                    AutomaticUpdater_ReadyToBeInstalled(null, null);

                else
                    AutomaticUpdater_CheckingFailed(null, null);
            }
        }

        void AutomaticUpdater_ProgressChanged(object sender, int progress)
        {
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = progress;
        }

        void AutomaticUpdater_BeforeDownloading(object sender, BeforeArgs e)
        {
            _statusLabel.Text = "Downloading updates...";
        }

        void AutomaticUpdater_ReadyToBeInstalled(object sender, EventArgs e)
        {
            _statusLabel.Text = "An update is available.  Click \"OK\" to install.";
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = 100;

            _okButton.Enabled = true;
        }

        private void AutomaticUpdater_CheckingFailed(object sender, FailArgs e)
        {
            _statusLabel.Text = "Error occurred while checking for updates.  Please try again later.";
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = 100;

            _okButton.Enabled = true;
        }

        void AutomaticUpdater_UpToDate(object sender, SuccessArgs e)
        {
            _statusLabel.Text = "Application is up-to-date.";
            _progressBar.Style = ProgressBarStyle.Continuous;
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
            ParentTabs.AutomaticUpdater.UpdateAvailable -= AutomaticUpdater_ReadyToBeInstalled;
            ParentTabs.AutomaticUpdater.BeforeDownloading -= AutomaticUpdater_BeforeDownloading;
            ParentTabs.AutomaticUpdater.ProgressChanged -= AutomaticUpdater_ProgressChanged;
        }

        private void _okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
