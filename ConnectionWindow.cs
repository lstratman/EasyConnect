using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EasyConnect.Properties;
using EasyConnect.Protocols;
using Timer = System.Timers.Timer;
using System.Configuration;

namespace EasyConnect
{
	/// <summary>
	/// Container window for connection protocols.  Implements a toolbar with the URI text box, OmniBar, and bookmarks button and has a container where the UI
	/// for the various connection protocols will be displayed (created from 
	/// <see cref="BaseProtocol{TConnection,TOptionsForm,TConnectionForm}.CreateConnectionForm"/>.
	/// </summary>
	public partial class ConnectionWindow : Form
	{
		/// <summary>
		/// Number of times that <see cref="_animationTimer"/> has fired, which is used to control the showing/hiding animation of the toolbar when 
		/// <see cref="Options.AutoHideToolbar"/> is true.
		/// </summary>
		protected int _animationTicks = 0;

		/// <summary>
		/// Timer that is used to animate the showing/hiding of the toolbar when <see cref="Options.AutoHideToolbar"/> is true.
		/// </summary>
		protected Timer _animationTimer = null;

		/// <summary>
		/// A list of all connections, either bookmarks or history entries, that can be used as auto-complete candidates in the OmniBar.
		/// </summary>
		protected List<IConnection> _autoCompleteEntries = new List<IConnection>();

		/// <summary>
		/// Flag indicating whether the toolbar should be automatically hidden when the user's focus is on the remote desktop UI.
		/// </summary>
		protected bool? _autoHideToolbar = null;

		/// <summary>
		/// Regex used to match the background color in a CSS style list.
		/// </summary>
		protected Regex _backgroundColorRegex = new Regex("background-color: #([A-F0-9]{6});");

		/// <summary>
		/// Connection that's currently active in this window.
		/// </summary>
		protected IConnection _connection = null;

		/// <summary>
		/// Flag indicating whether the size of <see cref="_connectionContainerPanel"/> has been set explicitly.
		/// </summary>
		protected bool _connectionContainerPanelSizeSet = false;

		/// <summary>
		/// UI for the connection created from <see cref="BaseProtocol{TConnection,TOptionsForm,TConnectionForm}.CreateConnectionForm"/>.
		/// </summary>
		protected BaseConnectionForm _connectionForm = null;

		/// <summary>
		/// Regex used to match the font color in a CSS style list.
		/// </summary>
		protected Regex _fontColorRegex = new Regex("; color: #([A-F0-9]{6});");

		/// <summary>
		/// Lookup that associates each bookmark with its corresponding menu item.
		/// </summary>
		protected Dictionary<ToolStripMenuItem, IConnection> _menuItemConnections = new Dictionary<ToolStripMenuItem, IConnection>();

		/// <summary>
		/// Current auto complete item in the OmniBar that the user is focused on.
		/// </summary>
		protected int _omniBarFocusIndex = -1;

		/// <summary>
		/// Flag indicating whether we should suppress the appearance of the OmniBar when we're setting the value of <see cref="urlTextBox"/> manually in code.
		/// </summary>
		protected bool _suppressOmniBar = false;

		/// <summary>
		/// Flag indicating whether or not the toolbar is currently shown.
		/// </summary>
		protected bool _toolbarShown = true;

		/// <summary>
		/// Subset of <see cref="_autoCompleteEntries"/> that match the current text that the user is entering in <see cref="urlTextBox"/>.
		/// </summary>
		protected List<IConnection> _validAutoCompleteEntries;

		protected HtmlPanel _urlPanel;

		/// <summary>
		/// Default constructor; initializes the UI for the OmniBar.
		/// </summary>
		public ConnectionWindow()
		{
			InitializeComponent();

		    _toolsMenu.Renderer = new EasyConnectToolStripRender();
		    _bookmarksMenu.Renderer = new EasyConnectToolStripRender();

			// Create the panels that will contain the display text for each auto complete entry when the user is typing in the URL text box
			for (int i = 0; i < 6; i++)
			{
				HtmlPanel autoCompletePanel = new HtmlPanel
					                              {
						                              AutoScroll = false,
						                              Width = _omniBarPanel.Width,
						                              Height = 30,
						                              Left = 0
					                              };

				autoCompletePanel.Top = i * autoCompletePanel.Height;
				autoCompletePanel.Font = urlTextBox.Font;
				autoCompletePanel.MouseEnter += autoCompletePanel_MouseEnter;
				autoCompletePanel.MouseLeave += autoCompletePanel_MouseLeave;
				autoCompletePanel.Click += autoCompletePanel_Click;
				autoCompletePanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

				PictureBox autoCompletePictureBox = new PictureBox
					                                    {
						                                    Width = 16,
						                                    Height = 16,
						                                    Left = 5,
						                                    Top = autoCompletePanel.Top + 7
					                                    };

				_omniBarPanel.Controls.Add(autoCompletePictureBox);
				_omniBarPanel.Controls.Add(autoCompletePanel);
			}

			urlTextBox.LostFocus += urlTextBox_LostFocus;
			_iconPictureBox.Image = new Icon(Resources.EasyConnect, 16, 16).ToBitmap();

			_urlPanel = new HtmlPanel
				            {
					            AutoScroll = false,
								Width = _urlPanelContainer.Width,
								Height = _urlPanelContainer.Height,
					            Left = 0,
								Top = 0,
					            Font = urlTextBox.Font,
								Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
				            };
			_urlPanel.Click += _urlPanel_Click;
			_urlPanelContainer.Controls.Add(_urlPanel);
			urlTextBox.Visible = false;

            _updatesMenuItem.Visible = ConfigurationManager.AppSettings["checkForUpdates"] != "false";
        }

		void _urlPanel_Click(object sender, EventArgs e)
		{
			_urlPanelContainer.Visible = false;
			
			urlTextBox.Visible = true;
			urlTextBox.Focus();
			urlTextBox.SelectAll();
		}

		/// <summary>
		/// Constructor that allows us to initialize the window with a connection that we're going to connect to.
		/// </summary>
		/// <param name="connection">Connection that we want to use with this window.</param>
		public ConnectionWindow(IConnection connection)
			: this()
		{
			_connection = connection;

			Icon = ConnectionFactory.GetProtocol(connection).ProtocolIcon;
			Text = connection.DisplayName;
			_suppressOmniBar = true;
			urlTextBox.Text = ConnectionFactory.GetProtocol(connection).ProtocolPrefix + "://" + connection.Host;
			_suppressOmniBar = false;
		}

		/// <summary>
		/// Explicitly sealed override of <see cref="Form.Text"/> that gets rid of the "Virtual member call in constructor" warning in 
		/// <see cref="ConnectionWindow(IConnection)"/>.
		/// </summary>
		public override sealed string Text
		{
			get
			{
				return base.Text;
			}

			set
			{
				base.Text = value;
			}
		}

		/// <summary>
		/// Returns true if the user's cursor is over the remote desktop UI, false otherwise.
		/// </summary>
		public bool IsCursorOverContent
		{
			get
			{
				Point relativePosition = _connectionContainerPanel.PointToClient(MousePosition);
				return (relativePosition.X >= 0 && relativePosition.Y >= 0 && relativePosition.X <= _connectionContainerPanel.Width &&
				        relativePosition.Y <= _connectionContainerPanel.Height);
			}
		}

		/// <summary>
		/// Main application form that this window is associated with.
		/// </summary>
		protected MainForm ParentTabs
		{
			get
			{
				return (MainForm) Parent;
			}
		}

		/// <summary>
		/// Returns the value of the <see cref="BaseConnectionForm.IsConnected"/> property for <see cref="_connectionForm"/>.
		/// </summary>
		public bool IsConnected
		{
			get
			{
				if (_connectionForm == null)
					return false;

				return _connectionForm.IsConnected;
			}
		}

		/// <summary>
		/// Returns the value of <see cref="Options.AutoHideToolbar"/>.
		/// </summary>
		protected bool AutoHideToolbar
		{
			get
			{
				if (_autoHideToolbar == null)
					_autoHideToolbar = ParentTabs.Options.AutoHideToolbar;

				return _autoHideToolbar.Value;
			}
		}

		/// <summary>
		/// Handler method that's called when focus leaves <see cref="urlTextBox"/>; if the user has chosen to auto hide the toolbar and the mouse was clicked
		/// outside of the bounds of the toolbar, begin the auto hide process.
		/// </summary>
		/// <param name="sender">Object from which this event originated (<see cref="urlTextBox"/> in this case).</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void urlTextBox_LostFocus(object sender, EventArgs e)
		{
			urlTextBox.Visible = false;
			_urlPanelContainer.Visible = true;

			if (!String.IsNullOrEmpty(urlTextBox.Text))
			{
				Match urlMatch = Regex.Match(urlTextBox.Text, "^((?<protocol>.*)://){0,1}(?<hostName>.*)$");

				_urlPanel.Text = String.Format(
					@"<div style=""background-color: #FFFFFF; font-family: {2}; font-size: {3}pt; height: {4}px; color: #9999BF;"">{0}://<font color=""black"">{1}</font></div>",
					urlMatch.Groups["protocol"].Success
						? urlMatch.Groups["protocol"].Value
						: ConnectionFactory.GetDefaultProtocol().ProtocolPrefix, urlMatch.Groups["hostName"].Value, urlTextBox.Font.FontFamily.GetName(0),
					urlTextBox.Font.SizeInPoints, _urlPanel.Height);

				urlTextBox.Text = (urlMatch.Groups["protocol"].Success
					                   ? urlMatch.Groups["protocol"].Value
					                   : ConnectionFactory.GetDefaultProtocol().ProtocolPrefix) + "://" + urlMatch.Groups["hostName"];
			}

			else
			{
				_urlPanel.Text = "";
			}

			if (AutoHideToolbar && PointToClient(Cursor.Position).Y > toolbarBackground.Height)
			{
				_bookmarksMenu.Hide();
				_toolsMenu.Hide();

				HideToolbar();
			}
		}

		/// <summary>
		/// Handler method that's called when the window is loaded.  If a <see cref="_connection"/> was specified (i.e. we're establishing a connection
		/// directly) and <see cref="AutoHideToolbar"/> is true, we hide the toolbar automatically.
		/// </summary>
		/// <param name="e">Arguments associated with this event.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (_connection != null && AutoHideToolbar)
			{
				toolbarBackground.Height = 5;
				_toolbarShown = false;
			}
		}

		/// <summary>
		/// Handler method that's called when a user clicks on an auto complete entry in <see cref="_omniBarPanel"/>.  Sets <see cref="_connection"/> to the
		/// corresponding entry in <see cref="_validAutoCompleteEntries"/> and calls <see cref="Connect"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originates.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void autoCompletePanel_Click(object sender, EventArgs e)
		{
			_omniBarPanel.Visible = false;
			_omniBarBorder.Visible = false;

			_connection = _validAutoCompleteEntries[_omniBarPanel.Controls.IndexOf((HtmlPanel) sender) / 2];
			Connect();
		}

		/// <summary>
		/// Focuses on <see cref="_connectionForm"/>.
		/// </summary>
		public void FocusContent()
		{
			if (_connectionForm != null)
				_connectionForm.Focus();
		}

		/// <summary>
		/// Handler method that's called when the user presses a key in <see cref="urlTextBox"/>.  Does the following:
		/// 
		/// <list type="bullet">
		/// <item><description>If it was <see cref="Keys.Enter"/>, it will connect to the focused auto complete entry if one was focused on, otherwise it
		/// will connect to the host entered in <see cref="urlTextBox"/>.</description></item>
		/// <item><description>If it was <see cref="Keys.Up"/> and the OmniBar is visible, it focuses on the previous item in the auto complete 
		/// list.</description></item>
		/// <item><description>If it was <see cref="Keys.Down"/> and the OmniBar is visible, it focuses on the next item in the auto complete 
		/// list.</description></item>
		/// <item><description>If it was <see cref="Keys.Escape"/> and the OmniBar is visible, it hides the OmniBar.</description></item>
		/// </list>
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="urlTextBox"/> in this case.</param>
		/// <param name="e">Key(s) that were pressed for this event.</param>
		private void urlTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				if (_omniBarFocusIndex == -1)
				{
					IConnection newConnection = ConnectionFactory.GetConnection(urlTextBox.Text);
					newConnection.Guid = Guid.NewGuid();

					_connection = newConnection;
				}

				else
					_connection = _validAutoCompleteEntries[_omniBarFocusIndex];

				Connect();
			}

			else if (e.KeyCode == Keys.Up && _omniBarPanel.Visible)
			{
				if (_omniBarFocusIndex > 0)
				{
					UnfocusOmniBarItem((HtmlPanel) _omniBarPanel.Controls[_omniBarFocusIndex * 2 + 1]);

					_omniBarFocusIndex--;
					FocusOmniBarItem((HtmlPanel) _omniBarPanel.Controls[_omniBarFocusIndex * 2 + 1]);
				}

				e.Handled = true;
			}

			else if (e.KeyCode == Keys.Down && _omniBarPanel.Visible)
			{
				if (_omniBarFocusIndex / 2 < _omniBarPanel.Controls.Count - 1)
				{
					if (_omniBarFocusIndex > -1)
						UnfocusOmniBarItem((HtmlPanel) _omniBarPanel.Controls[_omniBarFocusIndex * 2 + 1]);

					_omniBarFocusIndex++;
					FocusOmniBarItem((HtmlPanel) _omniBarPanel.Controls[_omniBarFocusIndex * 2 + 1]);
				}

				e.Handled = true;
			}

			else if (e.KeyCode == Keys.Escape && _omniBarPanel.Visible)
			{
				urlTextBox_Leave(null, null);

				e.Handled = true;
			}
		}

		/// <summary>
		/// Simulates focus on an item in the OmniBar's auto complete list by setting its font and background colors appropriately.
		/// </summary>
		/// <param name="omniBarItem">Item that we are to focus on.</param>
		protected void FocusOmniBarItem(HtmlPanel omniBarItem)
		{
			omniBarItem.Text = _fontColorRegex.Replace(_backgroundColorRegex.Replace(omniBarItem.Text, "background-color: #3D9DFD;"), "; color: #9DCDFD;");
			(omniBarItem.Parent.Controls[omniBarItem.Parent.Controls.IndexOf(omniBarItem) - 1] as PictureBox).BackColor = Color.FromArgb(61, 157, 253);
		}

		/// <summary>
		/// Removes focus on an item in the OmniBar's auto complete list by setting its font and background colors appropriately.
		/// </summary>
		/// <param name="omniBarItem">Item that we are to remove focus from.</param>
		protected void UnfocusOmniBarItem(HtmlPanel omniBarItem)
		{
			omniBarItem.Text = _fontColorRegex.Replace(_backgroundColorRegex.Replace(omniBarItem.Text, "background-color: #FFFFFF;"), "; color: #9999BF;");
			(omniBarItem.Parent.Controls[omniBarItem.Parent.Controls.IndexOf(omniBarItem) - 1] as PictureBox).BackColor = Color.White;
		}

		/// <summary>
		/// Opens a connection to <see cref="_connection"/>.
		/// </summary>
		public void Connect()
		{
			// Set the top and height of the connection contain panel appropriately depending on if we're auto-hiding the toolbar
			if (AutoHideToolbar && !_connectionContainerPanelSizeSet)
			{
				_connectionContainerPanel.Top = 5;
				_connectionContainerPanel.Height += 31;
			}

			_urlPanel.Text = String.Format(
				@"<div style=""background-color: #FFFFFF; font-family: {2}; font-size: {3}pt; height: {4}px; color: #9999BF;"">{0}://<font color=""black"">{1}</font></div>",
				ConnectionFactory.GetProtocol(_connection).ProtocolPrefix, _connection.Host, urlTextBox.Font.FontFamily.GetName(0), urlTextBox.Font.SizeInPoints,
				_urlPanel.Height);

			_urlPanelContainer.Visible = true;
			urlTextBox.Visible = false;

			_urlPanel.PerformLayout();

			// Initialize the UI elements
			_connectionContainerPanelSizeSet = true;
			_connectionForm = ConnectionFactory.CreateConnectionForm(_connection, _connectionContainerPanel);
			_connectionForm.ConnectionLost += _connectionForm_ConnectionLost;
			_connectionForm.Connected += _connectionForm_Connected;
			Icon = ConnectionFactory.GetProtocol(_connection).ProtocolIcon;
			_iconPictureBox.Image = new Icon(Icon, 16, 16).ToBitmap();
			Text = _connection.DisplayName;

			_suppressOmniBar = true;
			urlTextBox.Text = ConnectionFactory.GetProtocol(_connection).ProtocolPrefix + "://" + _connection.Host;
			_suppressOmniBar = false;

			try
			{
				_connectionForm.Connect();
			}

			catch (Exception)
			{
				Close();
				return;
			}

			ParentTabs.RegisterConnection(this, _connection);
			HideToolbar();
		}

		private void _connectionForm_Connected(object sender, EventArgs e)
		{
			Icon = ConnectionFactory.GetProtocol(_connection).ProtocolIcon;
			_iconPictureBox.Image = new Icon(Icon, 16, 16).ToBitmap();

			ParentTabs.RedrawTabs();
		}

		private void _connectionForm_ConnectionLost(object sender, EventArgs e)
		{
			_iconPictureBox.Image = new Icon(Resources.EasyConnect, 16, 16).ToBitmap();

			Icon = Resources.Disconnected;
			ParentTabs.RedrawTabs();
		}

		/// <summary>
		/// Handler method that's called when the user's cursor goes over <see cref="_bookmarksButton"/>.  Sets the button's background to the standard
		/// "hover" image.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_bookmarksButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _bookmarksButton_MouseEnter(object sender, EventArgs e)
		{
			_bookmarksButton.BackgroundImage = Resources.ButtonHoverBackground;
		}

		/// <summary>
		/// Handler method that's called when the user's cursor leaves <see cref="_bookmarksButton"/>.  Sets the button's background to nothing.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_bookmarksButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _bookmarksButton_MouseLeave(object sender, EventArgs e)
		{
			if (!_bookmarksMenu.Visible)
				_bookmarksButton.BackgroundImage = null;
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
			((Form) Parent).Close();
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

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
				ConnectionWindow_Shown(null, null);
		}

		/// <summary>
		/// Handler method that's called when the user clicks the "Bookmarks" icon in the toolbar.  Populates <see cref="_bookmarksMenu"/> and opens it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _bookmarksButton_Click(object sender, EventArgs e)
		{
			_bookmarksButton.BackgroundImage = Resources.ButtonPressedBackground;

			// Clear out the bookmarks menu beyond the first two entries, "Bookmarks manager" and "Bookmark this server"
			while (_bookmarksMenu.Items.Count > 2)
				_bookmarksMenu.Items.RemoveAt(2);

			// If the user has any bookmarks defined, add a separator between the first two entries and what will be the top-level bookmarks folders
			if (ParentTabs.Bookmarks.RootFolder.ChildFolders.Count > 0 ||
			    ParentTabs.Bookmarks.RootFolder.Bookmarks.Count > 0)
				_bookmarksMenu.Items.Add(new ToolStripSeparator());

			_menuItemConnections.Clear();

			// Add the bookmarks folder structure and its descendants recursively
			PopulateBookmarks(ParentTabs.Bookmarks.RootFolder, _bookmarksMenu.Items, true);

			_bookmarksMenu.DefaultDropDownDirection = ToolStripDropDownDirection.Left;
			_bookmarksMenu.Show(
				_bookmarksButton, -1 * (_bookmarksMenu.Items.Count > 2
					                        ? _bookmarksMenu.Width
					                        : 259) + _bookmarksButton.Width,
				_bookmarksButton.Height);
		}

		/// <summary>
		/// Recursive method called from <see cref="_bookmarksButton_Click"/> to create the menu items for the bookmarks folder structure.
		/// </summary>
		/// <param name="currentFolder">Folder that we're currently processing.</param>
		/// <param name="menuItems">Menu item that we're supposed to add the immediate children of <paramref name="currentFolder"/> to.</param>
		/// <param name="root">Flag indicating whether or not this is the root.</param>
		/// <returns>The newly-created menu item for <paramref name="currentFolder"/>.</returns>
		private ToolStripItem PopulateBookmarks(BookmarksFolder currentFolder, ToolStripItemCollection menuItems, bool root)
		{
			ToolStripItemCollection addLocation = menuItems;
			ToolStripMenuItem folderMenuItem = null;

			if (!root)
			{
				folderMenuItem = new ToolStripMenuItem(currentFolder.Name, Resources.Folder)
					                 {
						                 DropDownDirection = ToolStripDropDownDirection.Left
					                 };

				addLocation = folderMenuItem.DropDownItems;
			}

			// Get and populate the list of child folders recursively
			List<ToolStripItem> addItems =
				currentFolder.ChildFolders.OrderBy(f => f.Name).Select(childFolder => PopulateBookmarks(childFolder, addLocation, false)).ToList();

			// Add each child bookmark after the child folders
			foreach (IConnection bookmark in currentFolder.Bookmarks.OrderBy(b => b.DisplayName))
			{
				ToolStripMenuItem bookmarkMenuItem = new ToolStripMenuItem(
					bookmark.DisplayName, new Icon(ConnectionFactory.GetProtocol(bookmark).ProtocolIcon, 16, 16).ToBitmap(),
					(object sender, EventArgs e) =>
						{
							if (_connectionForm != null)
								_connectionForm.Close();

							_connection = _menuItemConnections[(ToolStripMenuItem) sender];
							Connect();
						});

				_menuItemConnections[bookmarkMenuItem] = bookmark;
				addItems.Add(bookmarkMenuItem);
			}

			addLocation.AddRange(addItems.ToArray());

			return folderMenuItem;
		}

		/// <summary>
		/// Handler method that's called when the user clicks on the "Bookmark this server" menu item.  Opens a dialog asking where the user wants to create
		/// the bookmark and what they want to call it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _bookmarkMenuItem_Click(object sender, EventArgs e)
		{
			if (_connection == null)
				return;

			SaveConnectionWindow saveWindow = new SaveConnectionWindow(ParentTabs);
			saveWindow.ShowDialog(this);

			if (saveWindow.DialogResult != DialogResult.OK)
				return;

			// Split the folder path and then descend the nodes to find the destination folder that the user wants to save this bookmark to
			string[] pathComponents = saveWindow.DestinationFolderPath.Split('/');
			TreeNode currentNode = ParentTabs.Bookmarks.FoldersTreeView.Nodes[0];

			for (int i = 2; i < pathComponents.Length; i++)
				currentNode = currentNode.Nodes[Convert.ToInt32(pathComponents[i])];

			// If an existing connection matches the one that we're saving, remove it after creating the new bookmark
			IConnection overwriteConnection =
				ParentTabs.Bookmarks.TreeNodeFolders[currentNode].Bookmarks.SingleOrDefault(
					b =>
					(b.Name == saveWindow.ConnectionName && !String.IsNullOrEmpty(b.Name)) ||
					(String.IsNullOrEmpty(b.Name) && b.Host == _connection.Host));

			_connection.Name = saveWindow.ConnectionName;
			_connection.IsBookmark = true;

			Text = _connection.DisplayName;
			ParentTabs.Bookmarks.TreeNodeFolders[currentNode].Bookmarks.Add(_connection);

			if (overwriteConnection != null)
				ParentTabs.Bookmarks.TreeNodeFolders[currentNode].Bookmarks.Remove(overwriteConnection);

			ParentTabs.Bookmarks.Save();
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
		/// Handler method that's called when the user clicks the "Bookmarks manager" menu item under the bookmarks menu.  Creates the bookmarks manager tab if 
		/// one doesn't already exist then switches to it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _bookmarksManagerMenuItem2_Click(object sender, EventArgs e)
		{
			ParentTabs.OpenBookmarkManager();
		}

		/// <summary>
		/// Handler method that's called when the user clicks the "Options" menu item under the tools menu.  Creates the options tab if one doesn't exist 
		/// already and then switches to it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _optionsMenuItem_Click(object sender, EventArgs e)
		{
			ParentTabs.OpenOptions();
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
		/// Handler method that's called when the user focuses on this tab.  Refocuses on the actual connection content (<see cref="_connectionForm"/>) if a
		/// connection has been established, otherwise focuses on <see cref="urlTextBox"/> if it's empty.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void ConnectionWindow_Shown(object sender, EventArgs e)
		{
			if (_connectionForm != null && _connectionForm.IsConnected && !_connectionForm.ContainsFocus)
				_connectionForm.Focus();

			if (string.IsNullOrEmpty(urlTextBox.Text) || _toolbarShown)
				_urlPanel_Click(null, null);
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
		/// Starts an animation that slides the toolbar down to display it in the case when the user clicks on the tab when <see cref="AutoHideToolbar"/>
		/// is true.  Kicks off an incremental animation method via <see cref="_animationTimer"/>.
		/// </summary>
		public void ShowToolbar()
		{
			// Exit early if the toolbar is already show, we're not supposed to autohide it, the window's handle hasn't been created yet, or we're in the 
			// middle of animating it already
			if (_toolbarShown || !AutoHideToolbar || !IsHandleCreated || (_animationTimer != null && _animationTimer.Enabled))
				return;

			_urlPanel_Click(null, null);

			_animationTicks = 0;
			_animationTimer = new Timer(20);
			_animationTimer.Elapsed += (sender, args) =>
				{
					// After six ticks, exit the animation process
					if (_animationTicks >= 6 || !toolbarBackground.IsHandleCreated)
					{
						_animationTimer.Enabled = false;
						return;
					}

					// Update the height of the toolbar incrementally
					toolbarBackground.Invoke(
						new Action(
							() =>
								{
									toolbarBackground.Height += 5;

									if (toolbarBackground.Height == 35)
										toolbarBackground.Height = 36;
								}));
					_animationTicks++;
				};
			_animationTimer.Enabled = true;
			_toolbarShown = true;
		}

		/// <summary>
		/// Starts an animation that slides the toolbar up to display it in the case when the user clicks on the content area of a tab when 
		/// <see cref="AutoHideToolbar"/> is true.  Kicks off an incremental animation method via <see cref="_animationTimer"/>.
		/// </summary>
		public void HideToolbar()
		{
			// Exit early if the toolbar is already hidden, we're not supposed to autohide it, the window's handle hasn't been created yet, or we're in the 
			// middle of animating it already
			if (!_toolbarShown || !AutoHideToolbar || !IsHandleCreated || (_animationTimer != null && _animationTimer.Enabled) || _connectionForm == null)
				return;

			_animationTicks = 0;
			_animationTimer = new Timer(20);
			_animationTimer.Elapsed += (sender, args) =>
				{
					// After six ticks, exit the animation process
					if (_animationTicks >= 6 || !toolbarBackground.IsHandleCreated)
					{
						_animationTimer.Enabled = false;
						return;
					}

					// Update the height of the toolbar incrementally
					toolbarBackground.Invoke(
						new Action(
							() =>
								{
									toolbarBackground.Height -= 5;

									if (toolbarBackground.Height == 6)
										toolbarBackground.Height = 5;
								}));
					_animationTicks++;
				};
			_animationTimer.Enabled = true;
			_toolbarShown = false;
		}

		/// <summary>
		/// Handler method that's called when the user clicks on the toolbar's background.  Displays the toolbar if <see cref="AutoHideToolbar"/> is set to
		/// true.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void toolbarBackground_Click(object sender, EventArgs e)
		{
			ShowToolbar();
		}

		/// <summary>
		/// Handler method that's called when the user focuses on <see cref="urlTextBox"/>.  Clears out the list of auto-complete entries.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void urlTextBox_Enter(object sender, EventArgs e)
		{
			_autoCompleteEntries.Clear();
			_urlPanelContainer.Visible = false;
			urlTextBox.Visible = true;
		}

		/// <summary>
		/// Handler method that's called when the user changes the text in <see cref="urlTextBox"/>.  Updates the list of auto-complete entries and updates
		/// the OmniBar accordingly.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void urlTextBox_TextChanged(object sender, EventArgs e)
		{
			// If the text box is blank or we're deliberately suppressing the OmniBar, hide it
			if (urlTextBox.Text.Length == 0 || _suppressOmniBar)
			{
				_omniBarPanel.Visible = false;
				_omniBarBorder.Visible = false;

				return;
			}

			IConnection currentlyFocusedItem = null;

			if (_omniBarFocusIndex != -1)
				currentlyFocusedItem = _validAutoCompleteEntries[_omniBarFocusIndex];

			// If this is the first text change since the user focused on the text box, then we get the list of all bookmarks and history items that weren't
			// from bookmarks and then use them all as potential auto-complete entries
			if (_autoCompleteEntries.Count == 0)
			{
				List<IConnection> bookmarks = new List<IConnection>();
				GetAllBookmarks(ParentTabs.Bookmarks.RootFolder, bookmarks);
				_autoCompleteEntries.AddRange(bookmarks);

				// Exclude history entries that are from the user clicking a bookmark
				if (ParentTabs.History.Connections != null)
				{
					_autoCompleteEntries.AddRange(
						ParentTabs.History.Connections.OrderByDescending(c => c.LastConnection).Distinct(
							new EqualityComparer<HistoryWindow.HistoricalConnection>(
								(x, y) => x.Connection.Host == y.Connection.Host)).Where(
									c => _autoCompleteEntries.FindIndex(a => a.Host == c.Connection.Host) == -1).Select
							(c => c.Connection));
				}
			}

			// Get a list of valid auto-complete entries by matching on the bookmark's display name or host
			_validAutoCompleteEntries =
				_autoCompleteEntries.Where(
					c =>
					c.DisplayName.IndexOf(urlTextBox.Text, StringComparison.InvariantCultureIgnoreCase) != -1 ||
					c.Host.IndexOf(urlTextBox.Text, StringComparison.InvariantCultureIgnoreCase) != -1).OrderBy(c => c.DisplayName).Take(6).ToList();

			if (_validAutoCompleteEntries.Count > 0)
			{
				_omniBarPanel.SuspendLayout();
				_omniBarBorder.SuspendLayout();

				for (int i = 0; i < _validAutoCompleteEntries.Count; i++)
				{
					IConnection connection = _validAutoCompleteEntries[i];
					HtmlPanel autoCompletePanel = _omniBarPanel.Controls[i * 2 + 1] as HtmlPanel;
					PictureBox autoCompletePictureBox = _omniBarPanel.Controls[i * 2] as PictureBox;

					// Set the text of the auto-complete item to "{Protocol}://{URI} - {DisplayName}" and bold the matching portions of the text
					autoCompletePanel.Text =
						String.Format(
							@"<div style=""background-color: #FFFFFF; padding-left: 29px; padding-top: 5px; padding-bottom: 5px; padding-right: 5px; font-family: {3}; font-size: {4}pt; height: 30px; color: #9999BF;""><font color=""green"">{0}://{1}</font>{2}</div>",
							ConnectionFactory.GetProtocol(connection).ProtocolPrefix,
							Regex.Replace(connection.Host, urlTextBox.Text, "<b>$0</b>", RegexOptions.IgnoreCase), connection.DisplayName == connection.Host
								                                                                                       ? ""
								                                                                                       : " - " +
								                                                                                         Regex.Replace(
									                                                                                         connection.DisplayName,
									                                                                                         urlTextBox.Text, "<b>$0</b>",
									                                                                                         RegexOptions.IgnoreCase),
							urlTextBox.Font.FontFamily.GetName(0), urlTextBox.Font.SizeInPoints);

					autoCompletePictureBox.Image = new Icon(ConnectionFactory.GetProtocol(connection).ProtocolIcon, 16, 16).ToBitmap();
					autoCompletePictureBox.BackColor = Color.White;

					// If the user was focused on this item, highlight it
					if (connection == currentlyFocusedItem)
						FocusOmniBarItem(autoCompletePanel);
				}

				_omniBarPanel.Height = _validAutoCompleteEntries.Count * 30 - 1;
				_omniBarBorder.Height = _omniBarPanel.Height + 2;
				_omniBarPanel.Visible = true;
				_omniBarBorder.Visible = true;
				_omniBarPanel.ResumeLayout();
				_omniBarBorder.ResumeLayout();
				_omniBarPanel.PerformLayout();
				_omniBarBorder.PerformLayout();
			}

				// If we found no matching entries, hide the OmniBar
			else
			{
				_omniBarPanel.Visible = false;
				_omniBarBorder.Visible = false;
			}
		}

		/// <summary>
		/// Handler method that's called when the user's cursor leaves an auto-complete panel.  Reverts the panel's background to white.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void autoCompletePanel_MouseLeave(object sender, EventArgs e)
		{
			if (_omniBarFocusIndex != -1 && (sender as HtmlPanel) == (_omniBarPanel.Controls[_omniBarFocusIndex * 2 + 1] as HtmlPanel))
				return;

			(sender as HtmlPanel).Text = _backgroundColorRegex.Replace((sender as HtmlPanel).Text, "background-color: #FFFFFF;");
		}

		/// <summary>
		/// Handler method that's called when the user's cursor enters an auto-complete panel.  Sets the panel's background to blue.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void autoCompletePanel_MouseEnter(object sender, EventArgs e)
		{
			if (_omniBarFocusIndex != -1 && (sender as HtmlPanel) == (_omniBarPanel.Controls[_omniBarFocusIndex * 2 + 1] as HtmlPanel))
				return;

			(sender as HtmlPanel).Text = _backgroundColorRegex.Replace((sender as HtmlPanel).Text, "background-color: #CDE5FE;");
		}

		/// <summary>
		/// Recursive method called from <see cref="urlTextBox_TextChanged"/> to get all of the bookmarks in the folder structure to use for auto-complete 
		/// items.
		/// </summary>
		/// <param name="currentFolder">Current folder that we're looking in.</param>
		/// <param name="bookmarks">List of bookmarks already assembled.</param>
		protected void GetAllBookmarks(BookmarksFolder currentFolder, List<IConnection> bookmarks)
		{
			bookmarks.AddRange(currentFolder.Bookmarks);

			foreach (BookmarksFolder childFolder in currentFolder.ChildFolders)
				GetAllBookmarks(childFolder, bookmarks);
		}

		/// <summary>
		/// Handler method that's called when the user's focus leaves <see cref="urlTextBox"/>.  Hides the OmniBar.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="urlTextBox"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void urlTextBox_Leave(object sender, EventArgs e)
		{
			_omniBarFocusIndex = -1;
			_omniBarBorder.Visible = false;
			_omniBarPanel.Visible = false;
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
		/// Custom comparer class that takes a generic type parameter and a comparison function so it can be used in sorting and distinct operations.
		/// </summary>
		/// <typeparam name="T">Type of the object that we're going to be comparing.</typeparam>
		protected class EqualityComparer<T> : IEqualityComparer<T>
		{
			/// <summary>
			/// Constructor that initializes <see cref="Comparer"/>.
			/// </summary>
			/// <param name="comparer">Comparer function to use.</param>
			public EqualityComparer(Func<T, T, bool> comparer)
			{
				Comparer = comparer;
			}

			/// <summary>
			/// Comparer function to use.
			/// </summary>
			public Func<T, T, bool> Comparer
			{
				get;
				set;
			}

			/// <summary>
			/// Uses <see cref="Comparer"/> to compare two objects.
			/// </summary>
			/// <param name="x">First object to compare.</param>
			/// <param name="y">Second object to compare.</param>
			/// <returns>True if <paramref name="x"/> and <paramref name="y"/> are equivalent, false otherwise.</returns>
			public bool Equals(T x, T y)
			{
				return Comparer(x, y);
			}

			/// <summary>
			/// Gets the hash code for a particular object.
			/// </summary>
			/// <param name="obj">Object for which we are to get the hash code.</param>
			/// <returns>The hash code for <paramref name="obj"/>.</returns>
			public int GetHashCode(T obj)
			{
				return obj.GetHashCode();
			}
		}

		private void _bookmarksMenu_VisibleChanged(object sender, EventArgs e)
		{
			if (!_bookmarksMenu.Visible)
				_bookmarksButton.BackgroundImage = null;
		}

		private void _toolsMenu_VisibleChanged(object sender, EventArgs e)
		{
			if (!_toolsMenu.Visible)
				_toolsButton.BackgroundImage = null;
		}

		private void _aboutMenuItem_Click(object sender, EventArgs e)
		{
			AboutBox aboutBox = new AboutBox();
			aboutBox.ShowDialog(ParentTabs);
		}
	}
}