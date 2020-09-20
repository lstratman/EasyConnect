using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Windows.Forms;
using EasyTabs;
using System.Threading.Tasks;
using EasyConnect.Protocols;
using EasyConnect.Properties;
using System.Security.Cryptography;
#if !APPX
using System.IO;
#endif

namespace EasyConnect
{
	/// <summary>
	/// Main class for this application.
	/// </summary>
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
#if APPX
        static async Task Main()
#else
        static void Main()
#endif
        {
			string[] arguments = Environment.GetCommandLineArgs();
			string openHistory = arguments.FirstOrDefault((string s) => s.StartsWith("/openHistory:", StringComparison.CurrentCulture));
			string openBookmarks = arguments.FirstOrDefault((string s) => s.StartsWith("/openBookmarks:", StringComparison.CurrentCulture));
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

            bool encryptionSetup = false;

#if !APPX
            Task.Run(async () =>
            {
#endif
                await GlobalSettings.Init();
                encryptionSetup = await SetupEncryption();
#if !APPX
            }).Wait();
#endif

            if (!encryptionSetup)
            {
                return;
            }

            MainForm mainForm = new MainForm(bookmarkGuids)
            {
                OpenToHistory = historyGuid
            };

            TitleBarTabsApplicationContext applicationContext = new TitleBarTabsApplicationContext();
            applicationContext.Start(mainForm);

            Application.Run(applicationContext);
		}

        private static async Task<bool> SetupEncryption()
        {
            bool convertingToRsa = false;

            // If the user hasn't formally selected an encryption type (either they're starting the application for the first time or are running a legacy
            // version that explicitly used Rijndael), ask them if they want to use RSA
            if (GlobalSettings.Instance.EncryptionType == null)
            {
                string messageBoxText = @"Do you want to use an RSA key container to encrypt your passwords?

The RSA encryption mode uses cryptographic keys associated with your Windows user account to encrypt sensitive data without having to enter an encryption password every time you start this application. However, your bookmarks file will be tied uniquely to this user account and you will be unable to share them between multiple users.";

                if (GlobalSettings.Instance.FirstLaunch)
                    messageBoxText += @"

The alternative is to derive an encryption key from a password that you will need to enter every time that this application starts.";

                else
                    messageBoxText += @"

Since you've already encrypted your data with a password once, you would need to enter it one more time to decrypt it before RSA can be used.";

                GlobalSettings.Instance.EncryptionType = MessageBox.Show(messageBoxText, "Use RSA?", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes
                                             ? EncryptionType.Rsa
                                             : EncryptionType.Rijndael;

                // Since they want to use RSA but already have connection data encrypted with Rijndael, we'll have to capture that password so that we can
                // decrypt it using Rijndael and then re-encrypt it using the RSA keypair
                convertingToRsa = GlobalSettings.Instance.EncryptionType == EncryptionType.Rsa && !GlobalSettings.Instance.FirstLaunch;
            }

            // If this is the first time that the user is running the application, pop up and information box informing them that they're going to enter a
            // password used to encrypt sensitive connection details
            if (GlobalSettings.Instance.FirstLaunch)
            {
                if (GlobalSettings.Instance.EncryptionType == EncryptionType.Rijndael)
                    MessageBox.Show(Resources.FirstRunPasswordText, Resources.FirstRunPasswordTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

#if !APPX
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyConnect"));
#endif
            }

            if (GlobalSettings.Instance.EncryptionType != null)
                await GlobalSettings.Instance.Save();

            bool encryptionTypeSet = false;

            while (true)
            {
                // Get the user's encryption password via the password dialog
                if (!encryptionTypeSet && (GlobalSettings.Instance.EncryptionType == EncryptionType.Rijndael || convertingToRsa))
                {
                    PasswordWindow passwordWindow = new PasswordWindow();
                    passwordWindow.ShowDialog();

                    ConnectionFactory.SetEncryptionType(EncryptionType.Rijndael, passwordWindow.Password);
                }

                else
                    ConnectionFactory.SetEncryptionType(GlobalSettings.Instance.EncryptionType.Value);

                // Create the bookmark and history windows which will try to use the password to decrypt sensitive connection details; if it's unable to, an
                // exception will be thrown that wraps a CryptographicException instance
                try
                {
                    await Bookmarks.Init();
                    await History.Init();
                    await ConnectionFactory.GetDefaultProtocol();

                    encryptionTypeSet = true;
                    break;
                }

                catch (Exception e)
                {
                    if ((GlobalSettings.Instance.EncryptionType == EncryptionType.Rijndael || convertingToRsa) && !ContainsCryptographicException(e))
                        throw;

                    // Tell the user that their password is incorrect and, if they click OK, repeat the process
                    DialogResult result = MessageBox.Show(Resources.IncorrectPasswordText, Resources.ErrorTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                    if (result != DialogResult.OK)
                    {
                        return false;
                    }
                }
            }

            // If we're converting over to RSA, we've already loaded and decrypted the sensitive data using 
            if (convertingToRsa)
            {
                ConnectionFactory.SetEncryptionType(GlobalSettings.Instance.EncryptionType.Value, null);

                await Bookmarks.Instance.Save();
                await History.Instance.Save();

                await ConnectionFactory.SetDefaults(await ConnectionFactory.GetDefaults(await ConnectionFactory.GetDefaultProtocol()));
            }

            return true;
        }

        /// <summary>
		/// Recursive method that checks to see if <paramref name="exception"/> or any of its <see cref="Exception.InnerException"/>s wrap a 
		/// <see cref="CryptographicException"/> instance.
		/// </summary>
		/// <param name="exception">Exception that we're currently examining.</param>
		/// <returns>True if <paramref name="exception"/> or any of its <see cref="Exception.InnerException"/>s wrap a <see cref="CryptographicException"/> 
		/// instance, false otherwise.</returns>
		private static bool ContainsCryptographicException(Exception exception)
        {
            if (exception is CryptographicException)
                return true;

            return exception.InnerException != null && ContainsCryptographicException(exception.InnerException);
        }
    }
}