using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Windows.Input;
#if !APPX
using System.Configuration;
using AppLimit.NetSparkle;
#endif
using EasyConnect.Properties;
using EasyConnect.Protocols;
using EasyTabs;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;
using Win32Interop.Enums;
using Win32Interop.Methods;
using System.Threading.Tasks;

namespace EasyConnect
{
	/// <summary>
	/// Main application form for EasyConnect that hosts the tabs in which the various windows are displayed.
	/// </summary>
	public partial class MainForm : TitleBarTabs
	{
		/// <summary>
		/// Delegate to open the <see cref="HistoricalConnection"/> whose <see cref="IConnection.Guid"/> matches <paramref name="historyGuid"/>.
		/// </summary>
		/// <param name="historyGuid">Value to use to match against <see cref="IConnection.Guid"/> when searching for a 
		/// <see cref="HistoricalConnection"/></param>
		/// <returns>The newly created tab for the connection to the <see cref="HistoricalConnection"/> whose <see cref="IConnection.Guid"/>
		/// matches <paramref name="historyGuid"/>.</returns>
		public delegate Task<TitleBarTab> ConnectToHistoryDelegate(Guid historyGuid);

		/// <summary>
		/// Instance of the first created MainForm, used for remoting purposes to open history entries via the jump list.
		/// </summary>
		public static MainForm ActiveInstance = null;

		/// <summary>
		/// Remoting method to use to open history entries via the jump list.
		/// </summary>
		public static ConnectToHistoryDelegate ConnectToHistoryMethod = null;

		/// <summary>
		/// When the application is invoked via the jump list to open a historical connection, we use this IPC channel to communicate with an already-open 
		/// process (if it exists) to tell it to open the given historical connection.
		/// </summary>
		protected static IpcServerChannel _ipcChannel = null;

		/// <summary>
		/// Contains the UI for using bookmarks but also the data concerning all of the bookmarks and folders that the user has.
		/// </summary>
		protected BookmarksWindow _bookmarksWindow = null;

		/// <summary>
		/// Flag indicating whether or not the user is currently pressing the Ctrl key.
		/// </summary>
		protected bool _ctrlDown = false;

		/// <summary>
		/// Contains the UI for looking at a user's connection history but also the data concerning those historical connections.
		/// </summary>
		protected HistoryWindow _historyWindow = null;

		/// <summary>
		/// Pointer to the low-level mouse hook callback (<see cref="KeyboardHookCallback"/>).
		/// </summary>
		protected IntPtr _hookId;

		/// <summary>
		/// Delegate of <see cref="KeyboardHookCallback"/>; declared as a member variable to keep it from being garbage collected.
		/// </summary>
		protected HOOKPROC _hookproc = null;

		/// <summary>
		/// Jump list for this application.
		/// </summary>
		protected JumpList _jumpList = null;

		/// <summary>
		/// Application-level (not connection protocol defaults) that the user has set.
		/// </summary>
		protected GlobalSettings _settings;

		/// <summary>
		/// Encryption type that was previously selected by the user.
		/// </summary>
		protected EncryptionType _previousEncryptionType;

		/// <summary>
		/// Tab that the user had previously focused on.
		/// </summary>
		protected TitleBarTab _previouslyClickedTab;

		/// <summary>
		/// When populating the application's jump list with the recent connections that the user has made, this is the category that the items go under.
		/// </summary>
		protected JumpListCustomCategory _recentCategory = new JumpListCustomCategory("Recent");

		/// <summary>
		/// Queue of the ten most recent connections, indicating which ones are going to show up in the jump list.
		/// </summary>
		protected Queue<HistoricalConnection> _recentConnections = new Queue<HistoricalConnection>();

		/// <summary>
		/// Flag indicating whether the user is pressing the Shift button.
		/// </summary>
		protected bool _shiftDown = false;

#if !APPX
        protected static Sparkle _sparkle;
#endif

		/// <summary>
		/// Default constructor.
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
			Init();

#if !APPX
            if (_sparkle == null && ConfigurationManager.AppSettings["checkForUpdates"] != "false")
			{
				_sparkle = new Sparkle(
					String.IsNullOrEmpty(ConfigurationManager.AppSettings["appCastUrl"])
						? "http://lstratman.github.io/EasyConnect/updates/EasyConnect.xml"
						: ConfigurationManager.AppSettings["appCastUrl"]);
				_sparkle.ApplicationWindowIcon = Icon;
				_sparkle.ApplicationIcon = Icon.ToBitmap();

				_sparkle.StartLoop(true, true);
			}
#endif
		}

		/// <summary>
		/// Constructor; initializes the UI.
		/// </summary>
		/// <param name="openToBookmarks">Bookmarks, if any, that we should open when initially creating the UI.</param>
		public MainForm(IEnumerable<Guid> openToBookmarks)
			: this()
		{
			if (openToBookmarks != null)
			{
				IEnumerable<Guid> toBookmarks = openToBookmarks.ToList();

				if (toBookmarks.Any())
				{
					OpenToBookmarks = (from Guid bookmarkGuid in toBookmarks
					                   select Bookmarks.Instance.FindBookmark(bookmarkGuid)).ToList();
				}
			}

			// Show the bookmarks manager window initially if we're not opening bookmarks or history entries
			if (OpenToBookmarks == null && OpenToHistory == Guid.Empty)
				OpenBookmarkManager();
		}

		/// <summary>
		/// Constructor; initializes the UI.
		/// </summary>
		/// <param name="openToBookmarks">Bookmarks, if any, that we should open when initially creating the UI.</param>
		public MainForm(List<IConnection> openToBookmarks)
			: this()
		{
			OpenToBookmarks = openToBookmarks;

			// Show the bookmarks manager window initially if we're not opening bookmarks or history entries
			if (OpenToBookmarks == null && OpenToHistory == Guid.Empty)
				OpenBookmarkManager();
		}

		/// <summary>
		/// Contains the UI for using bookmarks but also the data concerning all of the bookmarks and folders that the user has.
		/// </summary>
		public BookmarksWindow BookmarksWindow
		{
			get
			{
				if (_bookmarksWindow == null && ConnectionFactory.ReadyForCrypto)
					_bookmarksWindow = new BookmarksWindow(this);

				return _bookmarksWindow;
			}
		}

		/// <summary>
		/// Contains the UI for looking at a user's connection history but also the data concerning those historical connections.
		/// </summary>
		protected HistoryWindow HistoryWindow
		{
			get
			{
				if (_historyWindow == null && ConnectionFactory.ReadyForCrypto)
					_historyWindow = new HistoryWindow(this);

				return _historyWindow;
			}
		}

		/// <summary>
		/// <see cref="IConnection.Guid"/> of the <see cref="HistoricalConnection"/> that we should connect to initially.
		/// </summary>
		public Guid OpenToHistory
		{
			get;
			set;
		}

		/// <summary>
		/// Bookmarks, if any, that we should open when initially creating the UI.
		/// </summary>
		public List<IConnection> OpenToBookmarks
		{
			get;
			set;
		}

		/// <summary>
		/// Forces a complete redraw of the tabs overlay.
		/// </summary>
		public new void RedrawTabs()
		{
			if (_overlay != null)
				_overlay.Render(true);
		}

		/// <summary>
		/// Initializes the UI, loads the bookmark and history data, and sets up the IPC remoting channel and low-level keyboard hook.
		/// </summary>
		protected void Init()
		{
			AeroPeekEnabled = false;

			// Create a remoting channel used to tell this window to open historical connections when entries in the jump list are clicked
			if (_ipcChannel == null)
			{
				_ipcChannel = new IpcServerChannel("EasyConnect");
				ChannelServices.RegisterChannel(_ipcChannel, false);
				RemotingConfiguration.RegisterWellKnownServiceType(typeof (HistoryMethods), "HistoryMethods", WellKnownObjectMode.SingleCall);
			}

			// Wire up the tab event handlers
			TabClicked += MainForm_TabClicked;

			ActiveInstance = this;
			ConnectToHistoryMethod = ConnectToHistory;

			TabRenderer = new ChromeTabRenderer(this);

			// Get the low-level keyboard hook that will be used to process shortcut keys
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
			{
				_hookproc = KeyboardHookCallback;
				_hookId = User32.SetWindowsHookEx(WH.WH_KEYBOARD_LL, _hookproc, Kernel32.GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		/// <summary>
		/// Sets the encryption type to use to protect the application settings.  Calls 
		/// <see cref="ConnectionFactory.SetEncryptionType(EncryptionType, System.Security.SecureString)"/> and then saves the bookmarks, history, and main 
		/// application settings.
		/// </summary>
		/// <param name="encryptionType">Encryption type to use.</param>
		/// <param name="encryptionPassword">Encryption password, if any, to use.</param>
		private async Task SetEncryptionType(EncryptionType encryptionType, SecureString encryptionPassword)
		{
			ConnectionFactory.SetEncryptionType(encryptionType, encryptionPassword);

			await Bookmarks.Instance.Save();
			await History.Instance.Save();

			await ConnectionFactory.SetDefaults(await ConnectionFactory.GetDefaults(await ConnectionFactory.GetDefaultProtocol()));
		}

		/// <summary>
		/// Processes shortcut keys.
		/// </summary>
		/// <param name="nCode">Code indicating if we should process this message.</param>
		/// <param name="wParam"><see cref="WM"/> enumeration value which is the message that's being passed to us.</param>
		/// <param name="lParam">Virtual key code of the key being pressed.</param>
		/// <returns>The result of the next hook in the queue (<see cref="User32.CallNextHookEx"/>).</returns>
		protected IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			// Only process the key press if our window is active
			if (nCode >= 0 && User32.GetActiveWindow() == Handle)
			{
				// Get the key that was pressed
				Key key = KeyInterop.KeyFromVirtualKey(Marshal.ReadInt32(lParam));

				switch ((WM) wParam.ToInt32())
				{
					case WM.WM_KEYDOWN:
					case WM.WM_SYSKEYDOWN:
						switch (key)
						{
							case Key.RightCtrl:
							case Key.LeftCtrl:
								_ctrlDown = true;
								break;

							case Key.RightShift:
							case Key.LeftShift:
								_shiftDown = true;
								break;

								// Ctrl+T creates a new tab
							case Key.T:
								if (_ctrlDown)
									AddNewTab();

								break;

								// Ctrl+Tab cycles forward in the tab list, while Ctrl+Shift+Tab cycles backward
							case Key.Tab:
								if (Tabs.Count > 1)
								{
									if (_ctrlDown && _shiftDown)
									{
										if (SelectedTabIndex == 0)
											SelectedTabIndex = Tabs.Count - 1;

										else
											SelectedTabIndex--;
									}

									else if (_ctrlDown)
									{
										if (SelectedTabIndex == Tabs.Count - 1)
											SelectedTabIndex = 0;

										else
											SelectedTabIndex++;
									}
								}

								break;
						}

						break;

					case WM.WM_KEYUP:
					case WM.WM_SYSKEYUP:
						switch (key)
						{
							case Key.RightCtrl:
							case Key.LeftCtrl:
								_ctrlDown = false;
								break;

							case Key.RightShift:
							case Key.LeftShift:
								_shiftDown = false;
								break;
						}

						break;
				}
			}

			// Call the next hook in the queue
			return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
		}

		/// <summary>
		/// Handler method that's called when a tab is clicked on.  This is different from the <see cref="TitleBarTabs.TabSelected"/> event handler in that 
		/// this is called even if the tab is currently active.  This is used to show the toolbar for <see cref="ConnectionWindow"/> instances that 
		/// automatically hide their toolbars when the connection's UI is focused on.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void MainForm_TabClicked(object sender, TitleBarTabEventArgs e)
		{
			// Only show the toolbar if the user clicked on an already-selected tab
			if (e.Tab.Content is ConnectionWindow && e.Tab == _previouslyClickedTab && !e.WasDragging)
				(e.Tab.Content as ConnectionWindow).ShowToolbar();

			_previouslyClickedTab = e.Tab;
		}

        /// <summary>
        /// Handler method that's called when the user closes the <see cref="global::EasyConnect.BookmarksWindow"/> tab.  Sets <see cref="_bookmarksWindow"/> to null so that we know we
        /// need to create a new instance the next time the user tries to open it.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        protected void Bookmarks_FormClosed(object sender, FormClosedEventArgs e)
		{
			_bookmarksWindow = null;
		}

		/// <summary>
		/// Opens an <see cref="SettingsWindow"/> instance when the user clicks on the "Settings" menu item from a <see cref="ConnectionWindow"/>.
		/// </summary>
		public async Task OpenSettings()
		{
			TitleBarTab tab = Tabs.FirstOrDefault(t => t.Content is SettingsWindow);

			// Focus on the settings tab if a window is already open
			if (tab != null)
			{
				SelectedTab = tab;
				return;
			}

			_previousEncryptionType = GlobalSettings.Instance.EncryptionType ?? EncryptionType.Rijndael;

			// Create the settings window and then add entries for each protocol type to the window
			SettingsWindow settingsWindow = new SettingsWindow(this);
			GlobalSettingsWindow globalSettingsWindow = new GlobalSettingsWindow();

			globalSettingsWindow.Closed += globalSettingsWindow_Closed;
			settingsWindow.SettingsForms.Add(globalSettingsWindow);

			foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
			{
				Form settingsForm = await protocol.GetSettingsFormInDefaultsMode();

			    settingsForm.Closed += settingsForm_Closed;
				settingsWindow.SettingsForms.Add(settingsForm);
			}

			ShowInEmptyTab(settingsWindow);
		}

	    private async void settingsForm_Closed(object sender, EventArgs e)
	    {
	        await ConnectionFactory.SetDefaults(((ISettingsForm)sender).Connection);
        }

		/// <summary>
		/// Handler method that's closed when the user closes the global settings window.  Sets the encryption type and the password that the user selected.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void globalSettingsWindow_Closed(object sender, EventArgs e)
		{
			if (_previousEncryptionType != GlobalSettings.Instance.EncryptionType)
				// ReSharper disable PossibleInvalidOperationException
				await SetEncryptionType(GlobalSettings.Instance.EncryptionType.Value, (sender as GlobalSettingsWindow).EncryptionPassword);
			// ReSharper restore PossibleInvalidOperationException
		}

		/// <summary>
		/// Creates a new tab to hold <paramref name="form"/>.
		/// </summary>
		/// <param name="form">Form instance that we are to open in a new tab.</param>
		protected void ShowInEmptyTab(Form form)
		{
			// If we're opening the form from an unconnected ConnectionWindow, just replace its content with the new form
			if (SelectedTab != null && SelectedTab.Content is ConnectionWindow && !(SelectedTab.Content as ConnectionWindow).IsConnected)
			{
				Form oldWindow = SelectedTab.Content;

				SelectedTab.Content = form;
				ResizeTabContents();

				oldWindow.Close();
			}

				// Otherwise, create a new tab associated with the form
			else
			{
				TitleBarTab newTab = new TitleBarTab(this)
					                     {
						                     Content = form
					                     };

				Tabs.Add(newTab);
				ResizeTabContents(newTab);
				SelectedTabIndex = _tabs.Count - 1;
			}

			form.Show();

			if (_overlay != null)
				_overlay.Render(true);
		}

        /// <summary>
        /// Opens a <see cref="global::EasyConnect.HistoryWindow"/> instance when the user clicks on the "History" menu item in the tools menu from a 
        /// <see cref="ConnectionWindow"/> instance.
        /// </summary>
        public void OpenHistory()
		{
			TitleBarTab tab = Tabs.FirstOrDefault(t => t.Content is HistoryWindow);

			// Focus on the existing history window tab if it exists
			if (tab != null)
			{
				SelectedTab = tab;
				return;
			}

			HistoryWindow.FormClosed += History_FormClosed;
			ShowInEmptyTab(HistoryWindow);
		}

        /// <summary>
        /// Handler method that's called when the user closes the <see cref="global::EasyConnect.HistoryWindow"/> tab.  Sets <see cref="_historyWindow"/> to null so that we know we
        /// need to create a new instance the next time the user tries to open it.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void History_FormClosed(object sender, FormClosedEventArgs e)
		{
			_historyWindow = null;
		}

        /// <summary>
        /// Opens a <see cref="global::EasyConnect.BookmarksWindow"/> instance when the user clicks on the "Bookmarks" menu item in the tools menu from a 
        /// <see cref="ConnectionWindow"/> instance.
        /// </summary>
        public void OpenBookmarkManager()
		{
			TitleBarTab tab = Tabs.FirstOrDefault(t => t.Content is BookmarksWindow);

			// Focus on the existing bookmarks manager tab if it exists
			if (tab != null)
			{
				SelectedTab = tab;
				return;
			}

			BookmarksWindow.FormClosed += Bookmarks_FormClosed;
			ShowInEmptyTab(BookmarksWindow);
		}

		/// <summary>
		/// Opens the <see cref="HistoricalConnection"/> whose <see cref="IConnection.Guid"/> property matches <paramref name="historyGuid"/>.
		/// </summary>
		/// <param name="historyGuid">GUID used to identify the <see cref="HistoricalConnection"/> to open.</param>
		/// <returns><see cref="ConnectionWindow"/> tab for the <see cref="HistoricalConnection"/> whose <see cref="IConnection.Guid"/> property 
		/// matches <paramref name="historyGuid"/>.</returns>
		public async Task<TitleBarTab> ConnectToHistory(Guid historyGuid)
		{
			IConnection connection = History.Instance.FindInHistory(historyGuid);

			if (connection != null)
				return await Connect(connection);

			return null;
		}

		/// <summary>
		/// Opens the <see cref="IConnection"/>s in <paramref name="bookmarks"/>.
		/// </summary>
		/// <param name="bookmarks">Bookmarks to open.</param>
		public async Task ConnectToBookmarks(List<IConnection> bookmarks)
		{
			foreach (IConnection bookmark in bookmarks)
				await Connect(bookmark);

			SelectedTabIndex = Tabs.Count - 1;
		}

		/// <summary>
		/// Opens a new tab for <paramref name="connection"/>.
		/// </summary>
		/// <param name="connection">Connection that we are to open a new tab for.</param>
		/// <returns>Tab that was created for <paramref name="connection"/>.</returns>
		public async Task<TitleBarTab> Connect(IConnection connection)
		{
			return await Connect(connection, false);
		}

		/// <summary>
		/// Opens a new tab for <paramref name="connection"/>.
		/// </summary>
		/// <param name="connection">Connection that we are to open a new tab for.</param>
		/// <param name="focusNewTab">Flag indicating whether we should focus on the new tab.</param>
		/// <returns>Tab that was created for <paramref name="connection"/>.</returns>
		public async Task<TitleBarTab> Connect(IConnection connection, bool focusNewTab)
		{
			ConnectionWindow connectionWindow = new ConnectionWindow(connection);

			TitleBarTab newTab = new TitleBarTab(this)
				                     {
					                     Content = connectionWindow
				                     };
			Tabs.Insert(SelectedTabIndex + 1, newTab);
			ResizeTabContents(newTab);

			if (focusNewTab)
			{
				SelectedTab = newTab;
				_previouslyClickedTab = newTab;
			}

			await connectionWindow.Connect();

			return newTab;
		}

		/// <summary>
		/// Called from <see cref="ConnectionWindow"/> instances when a connection is established.  Creates a thumbnail preview for the tab (if one doesn't
		/// exist already) for Aero Peek and adds the connection to the list of entries on the application's jump list.
		/// </summary>
		/// <param name="connectionWindow"></param>
		/// <param name="connection"></param>
		public async Task RegisterConnection(ConnectionWindow connectionWindow, IConnection connection)
		{
			await History.Instance.AddToHistory(connection);

			// Add the connection to the jump list
			if (_recentConnections.FirstOrDefault((HistoricalConnection c) => c.Connection.Guid == connection.Guid) == null)
			{
				_recentCategory.AddJumpListItems(
					new JumpListLink(Application.ExecutablePath, connectionWindow.Text)
						{
							Arguments = "/openHistory:" + connection.Guid.ToString(),
							IconReference =
								new IconReference(Application.ExecutablePath, 0)
						});
				_jumpList.Refresh();

				_recentConnections.Enqueue(History.Instance.Connections.First((HistoricalConnection c) => c.Connection.Guid == connection.Guid));

				if (_recentConnections.Count > _jumpList.MaxSlotsInList)
					_recentConnections.Dequeue();
			}
		}

		/// <summary>
		/// Handler method that's called when the form is closing; saves the bookmarks.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
            await Bookmarks.Instance.Save();

			foreach (TitleBarTab tab in Tabs.ToArray())
				tab.Content.Close();
		}

		/// <summary>
		/// Handler method that's called when the form is shown.  Creates and initializes the jump list if necessary and, if they are specified, opens the
		/// bookmarks specified by <see cref="OpenToBookmarks"/> or the history entries pointed to by <see cref="OpenToHistory"/>.
		/// </summary>
		/// <param name="e">Arguments associated with this event.</param>
		protected override async void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (_jumpList == null)
			{
				_jumpList = JumpList.CreateJumpList();
				_jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Neither;
				_jumpList.AddCustomCategories(_recentCategory);

				// Get all of the historical connections and order them by their last connection times
				List<HistoricalConnection> historicalConnections =
                    History.Instance.Connections.OrderBy((HistoricalConnection c) => c.LastConnection).ToList();
				historicalConnections = historicalConnections.GetRange(0, Math.Min(historicalConnections.Count, Convert.ToInt32(_jumpList.MaxSlotsInList)));

				// Add each history entry to the jump list
				foreach (HistoricalConnection historicalConnection in historicalConnections)
				{
					_recentCategory.AddJumpListItems(
						new JumpListLink(Application.ExecutablePath, historicalConnection.Connection.DisplayName)
							{
								Arguments = "/openHistory:" + historicalConnection.Connection.Guid.ToString(),
								IconReference = new IconReference(Application.ExecutablePath, 0)
							});
					_recentConnections.Enqueue(historicalConnection);
				}

				_jumpList.Refresh();

				if (OpenToHistory != Guid.Empty)
					SelectedTab = await Connect(History.Instance.FindInHistory(OpenToHistory));
			}

			if (OpenToHistory == Guid.Empty && OpenToBookmarks != null)
				await ConnectToBookmarks(OpenToBookmarks);
		}

		/// <summary>
		/// Method to create a new tab when the add button in the title bar is clicked; creates a new <see cref="ConnectionWindow"/>.
		/// </summary>
		/// <returns>Tab for a new <see cref="ConnectionWindow"/> instance.</returns>
		public override TitleBarTab CreateTab()
		{
			_previouslyClickedTab = new TitleBarTab(this)
				                        {
					                        Content = new ConnectionWindow()
				                        };

			return _previouslyClickedTab;
		}

		/// <summary>
		/// Custom message pump method for the window.  Processes the <see cref="WM.WM_MOUSEACTIVATE"/> message.
		/// </summary>
		/// <param name="m">Message that we are to process.</param>
		protected override void WndProc(ref Message m)
		{
			switch ((WM) m.Msg)
			{
					// If the selected tab is a connection window and the cursor is over the content area of the window, focus on that content
				case WM.WM_MOUSEACTIVATE:
					if (SelectedTab != null && SelectedTab.Content is ConnectionWindow)
					{
						if ((SelectedTab.Content as ConnectionWindow).IsCursorOverContent)
							(SelectedTab.Content as ConnectionWindow).FocusContent();
					}

					base.WndProc(ref m);
					break;

				default:
					base.WndProc(ref m);
					break;
			}
		}

		/// <summary>
		/// Initiates an update check for the application.
		/// </summary>
		/// <returns>True if the update process was started successfully, false otherwise.</returns>
		public void CheckForUpdate()
		{
#if !APPX
            if (_sparkle == null)
            {
                return;
            }

			_sparkle.StopLoop();

			_sparkle = new Sparkle(
				String.IsNullOrEmpty(ConfigurationManager.AppSettings["appCastUrl"])
					? "http://lstratman.github.io/EasyConnect/updates/EasyConnect.xml"
					: ConfigurationManager.AppSettings["appCastUrl"]);
			_sparkle.ApplicationWindowIcon = Icon;
			_sparkle.ApplicationIcon = Icon.ToBitmap();
			_sparkle.checkLoopFinished += _sparkle_checkLoopFinished;

			_sparkle.StartLoop(true, true);
#endif
		}

#if !APPX
        void _sparkle_checkLoopFinished(object sender, bool UpdateRequired)
		{
			_sparkle.checkLoopFinished -= _sparkle_checkLoopFinished;

			if (!UpdateRequired)
				MessageBox.Show(this, "No updates are available.", "Software Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
#endif
	}
}