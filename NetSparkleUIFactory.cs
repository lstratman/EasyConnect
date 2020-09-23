using NetSparkle;
using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyConnect
{
    public class NetSparkleUIFactory : IUIFactory
    {
        public IDownloadProgress CreateProgressWindow(AppCastItem item, Icon applicationIcon)
        {
            return new NetSparkleProgressWindow(item);
        }

        public IUpdateAvailable CreateSparkleForm(Sparkle sparkle, AppCastItem[] updates, Icon applicationIcon, bool isUpdateAlreadyDownloaded = false)
        {
            return new NetSparkleUpdateWindow(updates, sparkle);
        }

        public void Init()
        {
        }

        public void ShowCannotDownloadAppcast(string appcastUrl, Icon applicationIcon = null)
        {
            MessageBox.Show("Unable to download the EasyConnect update manifest from " + appcastUrl + ".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowDownloadErrorMessage(string message, string appcastUrl, Icon applicationIcon = null)
        {
            MessageBox.Show("Error occurred while downloading the update: " + message + ".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowToast(AppCastItem[] updates, Icon applicationIcon, Action<AppCastItem[]> clickHandler)
        {
        }

        public void ShowUnknownInstallerFormatMessage(string downloadFileName, Icon applicationIcon = null)
        {
            MessageBox.Show("Unknown installer format for " + downloadFileName + ".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowVersionIsSkippedByUserRequest(Icon applicationIcon = null)
        {
        }

        public void ShowVersionIsUpToDate(Icon applicationIcon = null)
        {
        }
    }
}
