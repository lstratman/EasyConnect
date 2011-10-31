using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;

namespace EasyConnect
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            string openHistory = arguments.FirstOrDefault((string s) => s.StartsWith("/openHistory:"));
            Guid historyGuid = Guid.Empty;

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

                    HistoryMethods historyMethods = (HistoryMethods)Activator.GetObject(typeof(HistoryMethods), "ipc://EasyConnect/HistoryMethods");
                    historyMethods.OpenToHistoryGuid(historyGuid);

                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm mainForm = new MainForm() { OpenToHistory = historyGuid };

            if (!mainForm.Closing)
                Application.Run(mainForm);
        }
    }
}
