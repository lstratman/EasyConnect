using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Windows.Forms;
using Stratman.Windows.Forms.TitleBarTabs;

namespace EasyConnect
{
	/// <summary>
	/// Main class for this application.
	/// </summary>
	internal static class EasyConnect
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			string[] arguments = Environment.GetCommandLineArgs();
			string openHistory = arguments.FirstOrDefault((string s) => s.StartsWith("/openHistory:"));
			string openBookmarks = arguments.FirstOrDefault((string s) => s.StartsWith("/openBookmarks:"));
			Guid historyGuid = Guid.Empty;
			Guid[] bookmarkGuids = null;

			// If a history GUID was passed in on the command line
			if (openHistory != null)
			{
				historyGuid = new Guid(openHistory.Substring(openHistory.IndexOf(":", StringComparison.Ordinal) + 1));

				List<Process> existingProcesses = new List<Process>(Process.GetProcessesByName("EasyConnect"));

				if (existingProcesses.Count == 0)
					existingProcesses.AddRange(Process.GetProcessesByName("EasyConnect.vshost"));

				// If a process is already open, call the method in its IPC channel to tell it to open the given history entry and then exit this process
				if (existingProcesses.Count > 1)
				{
					IpcChannel ipcChannel = new IpcChannel("EasyConnectClient");
					ChannelServices.RegisterChannel(ipcChannel, false);

					HistoryMethods historyMethods = (HistoryMethods) Activator.GetObject(typeof (HistoryMethods), "ipc://EasyConnect/HistoryMethods");
					historyMethods.OpenToHistoryGuid(historyGuid);

					return;
				}
			}

				// If the user is trying to open bookmarks via the command line
			else if (openBookmarks != null)
			{
				string bookmarks = openBookmarks.Substring(openBookmarks.IndexOf(":", StringComparison.Ordinal) + 1);
				bookmarkGuids = (from bookmark in bookmarks.Split(',')
				                 where !String.IsNullOrEmpty(bookmark)
				                 select new Guid(bookmark)).ToArray();
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm mainForm = new MainForm(bookmarkGuids)
				                    {
					                    OpenToHistory = historyGuid
				                    };

			TitleBarTabsApplicationContext applicationContext = new TitleBarTabsApplicationContext();
			applicationContext.Start(mainForm);

			Application.Run(applicationContext);
		}
	}
}