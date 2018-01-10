using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using EasyConnect.Protocols;
using EasyConnect.Protocols.Rdp;
using EasyTabs;
using EasyConnect.Properties;
using System.Configuration;

namespace EasyConnect
{
    /// <summary>
    /// Displays a history of the connections that the user has made over the past two weeks, separated by each day.
    /// </summary>
    public partial class HistoryWindow : Form
	{
		/// <summary>
		/// Lookup that associates each list view item with its historical connection.
		/// </summary>
		private readonly Dictionary<ListViewItem, HistoricalConnection> _connections = new Dictionary<ListViewItem, HistoricalConnection>();

		/// <summary>
		/// Main application form that this window is associated with.
		/// </summary>
		protected MainForm _applicationForm = null;

		/// <summary>
		/// Lookup that associates each connection protocol with its icon in <see cref="historyImageList"/>.
		/// </summary>
		protected Dictionary<Type, int> _connectionTypeIcons = new Dictionary<Type, int>();

        protected HtmlPanel _urlPanel;

		/// <summary>
		/// Constructor; loads the history data and gets the icons for each protocol.
		/// </summary>
		/// <param name="applicationForm">Main application for for this window.</param>
		public HistoryWindow(MainForm applicationForm)
		{
			_applicationForm = applicationForm;

			InitializeComponent();

		    historyContextMenu.Renderer = new EasyConnectToolStripRender();
            _historyListView.ListViewItemSorter = new HistoryComparer(_connections);

			// Get the icon for each protocol
			foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
			{
				Icon icon = new Icon(protocol.ProtocolIcon, 16, 16);

				historyImageList.Images.Add(icon);
				_connectionTypeIcons[protocol.ConnectionType] = historyImageList.Images.Count - 1;
			}

            Connections_CollectionModified(History.Instance.Connections, new ListModificationEventArgs(ListModification.RangeAdded, 0, History.Instance.Connections.Count));
            History.Instance.Connections.CollectionModified += Connections_CollectionModified;

            _toolsMenu.Renderer = new EasyConnectToolStripRender();
            _iconPictureBox.Image = new Icon(Icon, 16, 16).ToBitmap();

            _urlPanel = new HtmlPanel
            {
                AutoScroll = false,
                Width = _urlPanelContainer.Width,
                Height = _urlPanelContainer.Height,
                Left = 0,
                Top = 1,
                Font = urlTextBox.Font,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };

            _urlPanelContainer.Controls.Add(_urlPanel);
            _urlPanel.Text = String.Format(
                    @"<span style=""background-color: #FFFFFF; font-family: {2}; font-size: {1}pt; height: {0}px; color: #9999BF"">easyconnect://<font color=""black"">history</font></span>",
                    _urlPanel.Height, urlTextBox.Font.SizeInPoints, urlTextBox.Font.FontFamily.GetName(0));

#if APPX
            _updatesMenuItem.Visible = false;
            _toolsMenuSeparator2.Visible = false;
#else
            _updatesMenuItem.Visible = ConfigurationManager.AppSettings["checkForUpdates"] != "false";
            _toolsMenuSeparator2.Visible = ConfigurationManager.AppSettings["checkForUpdates"] != "false";
#endif
        }

        private void Connections_CollectionModified(object sender, ListModificationEventArgs e)
        {
            ListWithEvents<HistoricalConnection> history = sender as ListWithEvents<HistoricalConnection>;

            if (e.Modification == ListModification.ItemModified || e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded)
            {
                for (int i = e.StartIndex; i < e.StartIndex + e.Count; i++)
                {
                    HistoricalConnection historyEntry = history[i];

                    if (_historyListView.Groups[historyEntry.LastConnection.ToString("yyyy-MM-dd")] == null)
                    {
                        int insertIndex = 0;
                        string groupName = historyEntry.LastConnection.ToString("yyyy-MM-dd");

                        for (insertIndex = 0; insertIndex < _historyListView.Groups.Count; insertIndex++)
                        {
                            if (String.Compare(_historyListView.Groups[insertIndex].Name, groupName, StringComparison.Ordinal) < 0)
                                break;
                        }

                        _historyListView.Groups.Insert(insertIndex, new ListViewGroup(groupName, historyEntry.LastConnection.ToLongDateString()));
                    }

                    // To account for legacy versions of the application where everything was an RDP connection, if a history entry was LegacyHistoricalConnection,
                    // we use the RDP icon
                    ListViewItem listViewItem = new ListViewItem(
                        historyEntry.LastConnection.ToLongTimeString(), _connectionTypeIcons[historyEntry.Connection.GetType() == typeof(LegacyHistoricalConnection)
                                                                                                 ? typeof(RdpConnection)
                                                                                                 : historyEntry.Connection.GetType()]);

                    listViewItem.SubItems.Add(historyEntry.Connection.DisplayName);
                    listViewItem.SubItems.Add(historyEntry.Connection.Host);

                    _connections[listViewItem] = historyEntry;

                    _historyListView.Items.Add(listViewItem);
                    _historyListView.Groups[historyEntry.LastConnection.ToString("yyyy-MM-dd")].Items.Add(listViewItem);
                    _historyListView.Columns[0].Width = 119;
                    _historyListView.Columns[1].Width = 143;
                    _historyListView.Columns[2].Width = 419;
                }

                if (IsHandleCreated)
                    _historyListView.BeginInvoke(new Action(_historyListView.Sort));
            }
        }

        /// <summary>
        /// Handler method that's called when the window is shown; sorts <see cref="_historyListView"/>.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			_historyListView.BeginInvoke(new Action(_historyListView.Sort));
		}

		/// <summary>
		/// Handler method that's called when the user clicks on the "Properties..." menu item in the context menu that appears when the user right-clicks on
		/// a history entry; opens up the options for the connection in a new tab.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Argument associated with this event.</param>
		private void propertiesMenuItem_Click(object sender, EventArgs e)
		{
			// Get the options form for the connection type and open it in a new tab
			Form optionsWindow = ConnectionFactory.CreateOptionsForm(_connections[_historyListView.SelectedItems[0]].Connection);
			TitleBarTab optionsTab = new TitleBarTab(_applicationForm)
				                         {
					                         Content = optionsWindow
				                         };

			_applicationForm.Tabs.Add(optionsTab);
			_applicationForm.SelectedTab = optionsTab;
		}

		/// <summary>
		/// Handler method that's called when the user clicks on the "Connect" menu item in the context menu that appears when the user right-clicks on
		/// a history entry; opens up a connection window in a new tab.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Argument associated with this event.</param>
		private async void connectMenuItem_Click(object sender, EventArgs e)
		{
			await _applicationForm.Connect(_connections[_historyListView.SelectedItems[0]].Connection);
		}

		/// <summary>
		/// Handler method that's called when the user clicks on an item in <see cref="_historyListView"/>; if it's a right-click, we open
		/// <see cref="historyContextMenu"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_historyListView"/>.</param>
		/// <param name="e">Argument associated with this event.</param>
		private void _historyListView_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right && _historyListView.SelectedItems.Count > 0)
				historyContextMenu.Show(Cursor.Position);
		}

		/// <summary>
		/// Handler method that's called when the user double-clicks on an item in <see cref="_historyListView"/>; opens up a connection window in a new tab.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Argument associated with this event.</param>
		private async void _historyListView_DoubleClick(object sender, EventArgs e)
		{
			if (_historyListView.SelectedItems.Count > 0)
				await _applicationForm.Connect(_connections[_historyListView.SelectedItems[0]].Connection);
		}

        /// <summary>
		/// Main application instance that this window is associated with, which is used to call back into application functionality.
		/// </summary>
		protected MainForm ParentTabs
        {
            get
            {
                return (MainForm)Parent;
            }
        }

        /// <summary>
		/// Handler method that's called when the user's cursor goes over <see cref="_toolsButton"/>.  Sets the button's background to the standard
		/// "hover" image.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_toolsButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _toolsButton_MouseEnter(object sender, EventArgs e)
        {
            _toolsButton.BackgroundImage = Resources.ButtonHoverBackground;
        }

        /// <summary>
        /// Handler method that's called when the user's cursor leaves <see cref="_toolsButton"/>.  Sets the button's background to nothing.
        /// </summary>
        /// <param name="sender">Object from which this event originated, <see cref="_toolsButton"/> in this case.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void _toolsButton_MouseLeave(object sender, EventArgs e)
        {
            if (!_toolsMenu.Visible)
                _toolsButton.BackgroundImage = null;
        }

        /// <summary>
        /// Handler method that's called when the user clicks the "Exit" menu item in the tools menu.  Exits the entire application.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void _exitMenuItem_Click(object sender, EventArgs e)
        {
            ((Form)Parent).Close();
        }

        /// <summary>
        /// Handler method that's called when the user clicks the "Tools" icon in the toolbar.  Opens up <see cref="_toolsMenu"/>.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void _toolsButton_Click(object sender, EventArgs e)
        {
            _toolsButton.BackgroundImage = Resources.ButtonPressedBackground;
            _toolsMenu.DefaultDropDownDirection = ToolStripDropDownDirection.Left;
            _toolsMenu.Show(_toolsButton, -1 * _toolsMenu.Width + _toolsButton.Width, _toolsButton.Height);
        }

        private void _aboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog(ParentTabs);
        }

        /// <summary>
		/// Handler method that's called when the user clicks on the "Check for updates" menu item under the tools menu.  Starts the update check process by
		/// calling <see cref="MainForm.CheckForUpdate"/> on <see cref="ParentTabs"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _updatesMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.CheckForUpdate();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the "History" menu item under the tools menu.  Creates the history tab if one doesn't exist 
		/// already and then switches to it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.OpenHistory();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the <see cref="_newWindowMenuItem"/> in the tools menu.  Creates a new <see cref="MainForm"/>
		/// instance and opens it.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_newWindowMenuItem"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _newWindowMenuItem_Click(object sender, EventArgs e)
        {
            MainForm newWindow = new MainForm(new List<Guid>());
            ParentTabs.ApplicationContext.OpenWindow(newWindow);

            newWindow.Show();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the "New tab" menu item under the tools menu.  Creates a new tab and then switches to it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _newTabMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.AddNewTab();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the "Options" menu item under the tools menu.  Creates the options tab if one doesn't exist 
		/// already and then switches to it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void _optionsMenuItem_Click(object sender, EventArgs e)
        {
            await ParentTabs.OpenOptions();
        }

        private void _toolsMenu_VisibleChanged(object sender, EventArgs e)
        {
            if (!_toolsMenu.Visible)
                _toolsButton.BackgroundImage = null;
        }

		/// <summary>
		/// Custom sorting class that sorts <see cref="ListViewItem"/>s by getting their corresponding <see cref="HistoricalConnection"/> instances and
		/// comparing their <see cref="HistoricalConnection.LastConnection"/> properties.
		/// </summary>
		protected class HistoryComparer : IComparer
		{
			/// <summary>
			/// List of connections correlated with <see cref="ListViewItem"/>s.
			/// </summary>
			private readonly Dictionary<ListViewItem, HistoricalConnection> _connections = null;

			/// <summary>
			/// Constructor that initializes <see cref="_connections"/>.
			/// </summary>
			/// <param name="connections">Connections to correlate each <see cref="ListViewItem"/> with.</param>
			public HistoryComparer(Dictionary<ListViewItem, HistoricalConnection> connections)
			{
				_connections = connections;
			}

			/// <summary>
			/// Compares the <see cref="HistoricalConnection.LastConnection"/> properties of the <see cref="HistoricalConnection"/> instances for each
			/// <see cref="ListViewItem"/>.
			/// </summary>
			/// <param name="x">First <see cref="ListViewItem"/> that we should compare.</param>
			/// <param name="y">Second <see cref="ListViewItem"/> that we should compare.</param>
			/// <returns>The result of <see cref="DateTime.CompareTo(DateTime)"/> of <paramref name="x"/> when provided with 
			/// <see cref="HistoricalConnection.LastConnection"/> for <paramref name="y"/>.</returns>
			public int Compare(object x, object y)
			{
				HistoricalConnection connectionX = _connections[(ListViewItem) x];
				HistoricalConnection connectionY = _connections[(ListViewItem) y];

				return connectionY.LastConnection.CompareTo(connectionX.LastConnection);
			}
		}

        private void urlBackground_Resize(object sender, EventArgs e)
        {
            _urlPanel.AutoScroll = false;
        }

        private void HistoryWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            History.Instance.Connections.CollectionModified -= Connections_CollectionModified;
        }
    }
}