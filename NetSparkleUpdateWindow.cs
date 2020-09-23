using NetSparkle;
using NetSparkle.Enums;
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
    public partial class NetSparkleUpdateWindow : Form, IUpdateAvailable
    {
        private AppCastItem[] _items;
        private Timer _ensureDialogShownTimer = null;

        public NetSparkleUpdateWindow(AppCastItem[] items, Sparkle sparkle)
        {
            InitializeComponent();

            _items = items;
            _updateAvailableLabel.Text = $"Version {items[0].Version} of EasyConnect is available, you have version {items[0].AppVersionInstalled}.  Would you like to update?";

            WebClient webClient = new WebClient();
            string releaseNotes = "";

            foreach (AppCastItem item in items)
            {
                string currentReleaseNotes = webClient.DownloadString(item.ReleaseNotesLink);
                currentReleaseNotes = currentReleaseNotes.Substring(currentReleaseNotes.IndexOf("<body>") + 6);
                currentReleaseNotes = currentReleaseNotes.Substring(0, currentReleaseNotes.IndexOf("</body>"));
                currentReleaseNotes = currentReleaseNotes.Replace("<h3>Change log</h3>", "<h3>" + item.Version + "</h3>");

                releaseNotes += currentReleaseNotes;
            }

            releaseNotes = "<html><body style=\"font-family: 'Segoe UI', 'sans-serif'; font-size: 10pt\">" + releaseNotes + "</body></html>";

            _changeLogText.Invoke((MethodInvoker)delegate
            {
                _changeLogText.Navigate("about:blank");
                _changeLogText.Document.OpenNew(true);
                _changeLogText.Document.Write(releaseNotes);
                _changeLogText.DocumentText = releaseNotes;
            });

            EnsureDialogShown();
        }

        public void EnsureDialogShown()
        {
            _ensureDialogShownTimer = new Timer();
            _ensureDialogShownTimer.Tick += new EventHandler(_ensureDialogShownTimer_Tick);
            _ensureDialogShownTimer.Interval = 250; // in milliseconds
            _ensureDialogShownTimer.Start();
        }

        private void _ensureDialogShownTimer_Tick(object sender, EventArgs e)
        {
            Activate();
            TopMost = true;
            TopMost = false;
            Focus();

            _ensureDialogShownTimer.Enabled = false;
            _ensureDialogShownTimer = null;
        }

        public UpdateAvailableResult Result
        {
            get
            {
                return DefaultUIFactory.ConvertDialogResultToUpdateAvailableResult(DialogResult);
            }
        }

        public AppCastItem CurrentItem
        {
            get
            {
                return _items[0];
            }
        }

        public event EventHandler UserResponded;

        public void HideReleaseNotes()
        {
            _changeLogText.Invoke((MethodInvoker)delegate
            {
                _changeLogText.Navigate("about:blank");
            });
        }

        public void HideRemindMeLaterButton()
        {
            _remindButton.Enabled = false;
        }

        public void HideSkipButton()
        {
            _skipButton.Enabled = false;
        }

        private void _skipButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void _remindButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void _updateButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        void IUpdateAvailable.Show()
        {
            ShowDialog();
            UserResponded.Invoke(this, new EventArgs());
        }

        private void NetSparkleUpdateWindow_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
