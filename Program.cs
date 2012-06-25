using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Windows.Forms;

namespace EasyConnect
{
    internal static class Program
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

            if (openHistory != null)
            {
                historyGuid = new Guid(openHistory.Substring(openHistory.IndexOf(":") + 1));

                List<Process> existingProcesses = new List<Process>(Process.GetProcessesByName("EasyConnect"));

                if (existingProcesses.Count == 0)
                    existingProcesses.AddRange(Process.GetProcessesByName("EasyConnect.vshost"));

                if (existingProcesses.Count > 1)
                {
                    IpcChannel ipcChannel = new IpcChannel("EasyConnectClient");
                    ChannelServices.RegisterChannel(ipcChannel, false);

                    HistoryMethods historyMethods =
                        (HistoryMethods)
                        Activator.GetObject(typeof (HistoryMethods), "ipc://EasyConnect/HistoryMethods");
                    historyMethods.OpenToHistoryGuid(historyGuid);

                    return;
                }
            }

            else if (openBookmarks != null)
            {
                string bookmarks = openBookmarks.Substring(openBookmarks.IndexOf(":") + 1);
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

            if (!mainForm.Closing)
                Application.Run(mainForm);
        }
    }
}