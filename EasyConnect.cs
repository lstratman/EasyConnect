using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Windows.Forms;

namespace EasyConnect
{
	/// <summary>
	/// Main class for this application.
	/// </summary>
	internal static class EasyConnect
	{
		/// <summary>
		/// Application context that keeps track of opened windows.
		/// </summary>
		internal static EasyConnectApplicationContext ApplicationContext = null;

		/// <summary>
		/// Opens <paramref name="window"/> by calling <see cref="EasyConnectApplicationContext.OpenWindow"/>.
		/// </summary>
		/// <param name="window">Window that we're opening.</param>
		public static void OpenWindow(MainForm window)
		{
			ApplicationContext.OpenWindow(window);
		}

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

			ApplicationContext = new EasyConnectApplicationContext(bookmarkGuids, historyGuid);

			Application.Run(ApplicationContext);
		}

		/// <summary>
		/// Custom application context that keeps the application open while at least one <see cref="MainForm"/> instance is still open.
		/// </summary>
		internal class EasyConnectApplicationContext : ApplicationContext
		{
			/// <summary>
			/// List of all opened windows.
			/// </summary>
			protected List<MainForm> _openWindows = new List<MainForm>();

			/// <summary>
			/// Constructor; provides the arguments to <see cref="MainForm"/> constructor in the form of <paramref name="bookmarkGuids"/> and 
			/// <paramref name="historyGuid"/>.
			/// </summary>
			/// <param name="bookmarkGuids">List of all bookmarks to open by default.</param>
			/// <param name="historyGuid">The history item to open by default.</param>
			public EasyConnectApplicationContext(IEnumerable<Guid> bookmarkGuids, Guid historyGuid)
			{
				MainForm mainForm = new MainForm(bookmarkGuids)
					                    {
						                    OpenToHistory = historyGuid
					                    };

				if (mainForm.Closing)
					ExitThread();

				else
					OpenWindow(mainForm);
			}

			/// <summary>
			/// Adds <paramref name="window"/> to <see cref="_openWindows"/> and attaches event handlers to its <see cref="Form.FormClosed"/> event to keep
			/// track of it.
			/// </summary>
			/// <param name="window">Window that we're opening.</param>
			// ReSharper disable MemberHidesStaticFromOuterClass
			public void OpenWindow(MainForm window)
			// ReSharper restore MemberHidesStaticFromOuterClass
			{
				_openWindows.Add(window);

				window.FormClosed += window_FormClosed;
				window.Show();
			}

			/// <summary>
			/// Handler method that's called when an item in <see cref="_openWindows"/> has its <see cref="Form.FormClosed"/> event invoked.  Removes the 
			/// window from <see cref="_openWindows"/> and, if there are no more windows open, calls <see cref="ApplicationContext.ExitThread"/>.
			/// </summary>
			/// <param name="sender">Object from which this event originated.</param>
			/// <param name="e">Arguments associated with the event.</param>
			protected void window_FormClosed(object sender, FormClosedEventArgs e)
			{
				_openWindows.Remove((MainForm) sender);

				if (_openWindows.Count == 0)
					ExitThread();
			}
		}
	}
}