using System;
using System.Windows.Forms;
using wyDay.Controls;

namespace EasyConnect
{
	/// <summary>
	/// Modal window that displays the progress while checking/downloading updates to the application and, if one is available, asks the user if they want to
	/// install it.
	/// </summary>
	public partial class CheckForUpdatesWindow : Form
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public CheckForUpdatesWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Main application instance that this window belongs to.
		/// </summary>
		protected MainForm ParentTabs
		{
			get
			{
				return Owner as MainForm;
			}
		}

		/// <summary>
		/// Handler method that's called when the window is shown.  Attaches various event handlers to <see cref="MainForm.AutomaticUpdater"/> and then starts
		/// the update checking process.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with the event.</param>
		private void CheckForUpdatesWindow_Shown(object sender, EventArgs e)
		{
			_progressBar.Style = ProgressBarStyle.Marquee;

			ParentTabs.AutomaticUpdater.UpToDate += AutomaticUpdater_UpToDate;
			ParentTabs.AutomaticUpdater.CheckingFailed += AutomaticUpdater_CheckingFailed;
			ParentTabs.AutomaticUpdater.UpdateAvailable += AutomaticUpdater_ReadyToBeInstalled;
			ParentTabs.AutomaticUpdater.BeforeDownloading += AutomaticUpdater_BeforeDownloading;
			ParentTabs.AutomaticUpdater.ProgressChanged += AutomaticUpdater_ProgressChanged;
			ParentTabs.AutomaticUpdater.ReadyToBeInstalled += AutomaticUpdater_ReadyToBeInstalled;

			// If CheckForUpdate returns false, it means that we've already checked and we should display the appropriate buttons/text based on what step
			// the updater is on
			if (!ParentTabs.CheckForUpdate())
			{
				if (ParentTabs.AutomaticUpdater.UpdateStepOn == UpdateStepOn.UpdateDownloaded)
					AutomaticUpdater_ReadyToBeInstalled(null, null);

				else
					AutomaticUpdater_CheckingFailed(null, null);
			}
		}

		/// <summary>
		/// Handler method that's called when <see cref="MainForm.AutomaticUpdater"/> reports progress while downloading an update.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="MainForm.AutomaticUpdater"/> in this case.</param>
		/// <param name="progress">Current progress percentage.</param>
		private void AutomaticUpdater_ProgressChanged(object sender, int progress)
		{
			_progressBar.Style = ProgressBarStyle.Continuous;
			_progressBar.Value = progress;
		}

		/// <summary>
		/// Handler method that's called when <see cref="MainForm.AutomaticUpdater"/> starts downloading an update.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="MainForm.AutomaticUpdater"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void AutomaticUpdater_BeforeDownloading(object sender, BeforeArgs e)
		{
			_statusLabel.Text = "Downloading updates...";
		}

		/// <summary>
		/// Handler method that's called when <see cref="MainForm.AutomaticUpdater"/> finishes downloading an update.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="MainForm.AutomaticUpdater"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void AutomaticUpdater_ReadyToBeInstalled(object sender, EventArgs e)
		{
			_statusLabel.Text = "An update is available.  Click \"OK\" to install.";
			_progressBar.Style = ProgressBarStyle.Continuous;
			_progressBar.Value = 100;

			_okButton.Enabled = true;
		}

		/// <summary>
		/// Handler method that's called when <see cref="MainForm.AutomaticUpdater"/> reports an error checking for updates.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="MainForm.AutomaticUpdater"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void AutomaticUpdater_CheckingFailed(object sender, FailArgs e)
		{
			_statusLabel.Text = "Error occurred while checking for updates.  Please try again later.";
			_progressBar.Style = ProgressBarStyle.Continuous;
			_progressBar.Value = 100;

			_okButton.Enabled = true;
		}

		/// <summary>
		/// Handler method that's called when <see cref="MainForm.AutomaticUpdater"/> discovers that the application is already up to date.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="MainForm.AutomaticUpdater"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void AutomaticUpdater_UpToDate(object sender, SuccessArgs e)
		{
			_statusLabel.Text = "Application is up-to-date.";
			_progressBar.Style = ProgressBarStyle.Continuous;
			_progressBar.Value = 100;

			_okButton.Enabled = true;
		}

		/// <summary>
		/// Handler method that's called when the user clicks on <see cref="_cancelButton"/>; closes the window.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Handler method that's called when the user closes the window; unsubscribes the window from the various <see cref="MainForm.AutomaticUpdater"/>
		/// events.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void CheckForUpdatesWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			ParentTabs.AutomaticUpdater.UpToDate -= AutomaticUpdater_UpToDate;
			ParentTabs.AutomaticUpdater.CheckingFailed -= AutomaticUpdater_CheckingFailed;
			ParentTabs.AutomaticUpdater.UpdateAvailable -= AutomaticUpdater_ReadyToBeInstalled;
			ParentTabs.AutomaticUpdater.BeforeDownloading -= AutomaticUpdater_BeforeDownloading;
			ParentTabs.AutomaticUpdater.ProgressChanged -= AutomaticUpdater_ProgressChanged;
		}

		/// <summary>
		/// Handler method that's called when the user clicks on <see cref="_cancelButton"/>; sets <see cref="DialogResult"/> to <see cref="DialogResult.OK"/> 
		/// and closes the window.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}