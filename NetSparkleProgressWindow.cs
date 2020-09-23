using NetSparkle;
using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyConnect
{
    public partial class NetSparkleProgressWindow : Form, IDownloadProgress
    {
        private AppCastItem _appCastItem;

        public NetSparkleProgressWindow(AppCastItem item)
        {
            InitializeComponent();
            _appCastItem = item;
        }

        public event EventHandler InstallAndRelaunch;

        public bool DisplayErrorMessage(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return true;
        }

        public void FinishedDownloadingFile(bool isDownloadedFileValid)
        {
            _progressBar.Value = 100;
            _progressLabel.Text = "100%";

            if (isDownloadedFileValid)
            {
                DialogResult = DialogResult.OK;
                InstallAndRelaunch.Invoke(this, new EventArgs());
            }

            else
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        public void ForceClose()
        {
            DialogResult = DialogResult.Abort;
            Close();
        }

        public void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _progressBar.Value = e.ProgressPercentage;

            if (e.ProgressPercentage > 0)
            {
                _progressBar.Value = e.ProgressPercentage - 1;
            }

            if (e.BytesReceived > 0)
            {
                _progressLabel.Visible = true;
                _progressLabel.Text = e.ProgressPercentage + "%";
            }
        }

        public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {
        }

        bool IDownloadProgress.ShowDialog()
        {
            return DefaultUIFactory.ConvertDialogResultToDownloadProgressResult(ShowDialog());
        }
    }
}
