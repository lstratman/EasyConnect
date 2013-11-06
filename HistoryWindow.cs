using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using EasyConnect.Protocols;
using EasyConnect.Protocols.Rdp;
using Stratman.Windows.Forms.TitleBarTabs;

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

		/// <summary>
		/// Filename where history data is loaded from/saved to.
		/// </summary>
		protected string _historyFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\History.xml";

		/// <summary>
		/// Constructor; loads the history data and gets the icons for each protocol.
		/// </summary>
		/// <param name="applicationForm">Main application for for this window.</param>
		public HistoryWindow(MainForm applicationForm)
		{
			_applicationForm = applicationForm;

			InitializeComponent();

			_historyListView.ListViewItemSorter = new HistoryComparer(_connections);

			// Get the icon for each protocol
			foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
			{
				Icon icon = new Icon(protocol.ProtocolIcon, 16, 16);

				historyImageList.Images.Add(icon);
				_connectionTypeIcons[protocol.ConnectionType] = historyImageList.Images.Count - 1;
			}

			// Load the history data
			if (File.Exists(_historyFileName))
			{
				XmlSerializer historySerializer = new XmlSerializer(typeof (List<HistoricalConnection>));
				List<HistoricalConnection> historicalConnections = null;

				using (XmlReader historyReader = new XmlTextReader(_historyFileName))
					historicalConnections = (List<HistoricalConnection>) historySerializer.Deserialize(historyReader);

				foreach (HistoricalConnection historyEntry in historicalConnections)
					AddToHistory(historyEntry);
			}
		}

		/// <summary>
		/// List of all connections that the user has made in the last two weeks.
		/// </summary>
		public List<HistoricalConnection> Connections
		{
			get
			{
				return _connections.Values.ToList();
			}
		}

		/// <summary>
		/// Returns the connection, if any, in <see cref="Connections"/> whose <see cref="IConnection.Guid"/> matches <paramref name="historyGuid"/>.
		/// </summary>
		/// <param name="historyGuid">GUID we should use when matching against <see cref="IConnection.Guid"/> for each history entry.</param>
		/// <returns>The connection, if any, in <see cref="Connections"/> whose <see cref="IConnection.Guid"/> matches 
		/// <paramref name="historyGuid"/>.</returns>
		public IConnection FindInHistory(Guid historyGuid)
		{
			return _connections.Values.FirstOrDefault((HistoricalConnection c) => c.Connection.Guid == historyGuid) == null
				       ? null
				       : _connections.Values.First((HistoricalConnection c) => c.Connection.Guid == historyGuid).Connection;
		}

		/// <summary>
		/// Adds a connection that the user has made to the list of historical connections.
		/// </summary>
		/// <param name="connection">Connection that was made that should be added to the history.</param>
		public void AddToHistory(IConnection connection)
		{
			HistoricalConnection historyEntry = new HistoricalConnection(connection)
				                                    {
					                                    LastConnection = DateTime.Now
				                                    };

			AddToHistory(historyEntry);
			Save();
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
		/// Adds <paramref name="historyEntry"/> to <see cref="_historyListView"/> under the proper group.
		/// </summary>
		/// <param name="historyEntry">Connection that we're adding to the history.</param>
		protected void AddToHistory(HistoricalConnection historyEntry)
		{
			if (_historyListView.Groups[historyEntry.LastConnection.ToString("yyyy-MM-dd")] == null)
			{
				int insertIndex = 0;
				string groupName = historyEntry.LastConnection.ToString("yyyy-MM-dd");

				for (insertIndex = 0; insertIndex < _historyListView.Groups.Count; insertIndex++)
				{
					if (String.Compare(_historyListView.Groups[insertIndex].Name, groupName, StringComparison.Ordinal) > 0)
						break;
				}

				_historyListView.Groups.Insert(insertIndex, new ListViewGroup(groupName, historyEntry.LastConnection.ToLongDateString()));
			}

			// To account for legacy versions of the application where everything was an RDP connection, if a history entry was LegacyHistoricalConnection,
			// we use the RDP icon
			ListViewItem listViewItem = new ListViewItem(
				historyEntry.LastConnection.ToLongTimeString(), _connectionTypeIcons[historyEntry.Connection.GetType() == typeof (LegacyHistoricalConnection)
					                                                                     ? typeof (RdpConnection)
					                                                                     : historyEntry.Connection.GetType()]);

			listViewItem.SubItems.Add(historyEntry.Connection.DisplayName);
			listViewItem.SubItems.Add(historyEntry.Connection.Host);

			_connections[listViewItem] = historyEntry;

			_historyListView.Items.Add(listViewItem);
			_historyListView.Groups[historyEntry.LastConnection.ToString("yyyy-MM-dd")].Items.Add(listViewItem);
			_historyListView.Columns[0].Width = 119;
			_historyListView.Columns[1].Width = 143;
			_historyListView.Columns[2].Width = 419;

			if (IsHandleCreated)
				_historyListView.BeginInvoke(new Action(_historyListView.Sort));
		}

		/// <summary>
		/// Writes the list of history entries to disk.
		/// </summary>
		public void Save()
		{
			FileInfo destinationFile = new FileInfo(_historyFileName);
			XmlSerializer historySerializer = new XmlSerializer(typeof (List<HistoricalConnection>));

			// ReSharper disable AssignNullToNotNullAttribute
			Directory.CreateDirectory(destinationFile.DirectoryName);
			// ReSharper restore AssignNullToNotNullAttribute

			// Remove all connections older than two weeks
			foreach (
				KeyValuePair<ListViewItem, HistoricalConnection> connection in
					_connections.Where(kvp => kvp.Value.LastConnection < DateTime.Now.AddDays(-14)).ToList())
			{
				connection.Key.Remove();
				_connections.Remove(connection.Key);
			}

			using (XmlWriter historyWriter = new XmlTextWriter(_historyFileName, new UnicodeEncoding()))
			{
				historySerializer.Serialize(historyWriter, _connections.Values.ToList());
				historyWriter.Flush();
			}
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
		private void connectMenuItem_Click(object sender, EventArgs e)
		{
			_applicationForm.Connect(_connections[_historyListView.SelectedItems[0]].Connection);
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
		private void _historyListView_DoubleClick(object sender, EventArgs e)
		{
			if (_historyListView.SelectedItems.Count > 0)
				_applicationForm.Connect(_connections[_historyListView.SelectedItems[0]].Connection);
		}

		/// <summary>
		/// Wraps a connection that the user made and the time that they made it.
		/// </summary>
		public class HistoricalConnection : ISerializable, IXmlSerializable
		{
			/// <summary>
			/// Default constructor.
			/// </summary>
			public HistoricalConnection()
			{
			}

			/// <summary>
			/// Constructor for <see cref="ISerializable"/>.
			/// </summary>
			/// <param name="info">Container for serialized data.</param>
			/// <param name="context">Serialization context.</param>
			// ReSharper disable UnusedParameter.Local
			public HistoricalConnection(SerializationInfo info, StreamingContext context)
				// ReSharper restore UnusedParameter.Local
			{
				LastConnection = info.GetDateTime("LastConnection");
				Connection = (IConnection) info.GetValue("Connection", typeof (IConnection));
			}

			/// <summary>
			/// Constructor that initializes <see cref="Connection"/>.
			/// </summary>
			/// <param name="connection">Connection that the user made.</param>
			public HistoricalConnection(IConnection connection)
			{
				Connection = connection;
			}

			/// <summary>
			/// Connection that the user made.
			/// </summary>
			public IConnection Connection
			{
				get;
				set;
			}

			/// <summary>
			/// Date and time that the user made the connection.
			/// </summary>
			public DateTime LastConnection
			{
				get;
				set;
			}

			/// <summary>
			/// Serialization method for <see cref="ISerializable"/>.
			/// </summary>
			/// <param name="info">Container for serialized data.</param>
			/// <param name="context">Serialization context.</param>
			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("LastConnection", LastConnection);
				info.AddValue("Connection", Connection);
			}

			/// <summary>
			/// Schema method for <see cref="IXmlSerializable"/>.
			/// </summary>
			/// <returns>Null, always.</returns>
			public XmlSchema GetSchema()
			{
				return null;
			}

			/// <summary>
			/// Deserializes the historical connection data from an XML source.
			/// </summary>
			/// <param name="reader">XML source that we are deserializing from.</param>
			public void ReadXml(XmlReader reader)
			{
				// If we are dealing with a legacy historical connection, deserialize it into LegacyHistoricalConnection
				if (reader.GetAttribute("isLegacy") != "false")
				{
					XmlSerializer serializer = new XmlSerializer(typeof (LegacyHistoricalConnection));
					LegacyHistoricalConnection legacyHistoricalConnection = (LegacyHistoricalConnection) serializer.Deserialize(reader);

					LastConnection = legacyHistoricalConnection.LastConnection;
					Connection = legacyHistoricalConnection;
				}

					// Otherwise, get the type of connection from the node name under Connection and deserialize it using that
				else
				{
					reader.Read();

					while (reader.MoveToContent() == XmlNodeType.Element)
					{
						switch (reader.LocalName)
						{
							case "LastConnection":
								LastConnection = DateTime.Parse(reader.ReadElementContentAsString());
								break;

							case "Connection":
								reader.Read();

								XmlSerializer serializer =
									new XmlSerializer(
										reader.LocalName == "HistoricalConnection"
											? typeof (LegacyHistoricalConnection)
											: ConnectionFactory.GetProtocols().First(p => p.ConnectionType.Name == reader.LocalName).ConnectionType);
								Connection = (IConnection) serializer.Deserialize(reader);

								reader.Read();

								break;
						}
					}

					reader.Read();
				}
			}

			/// <summary>
			/// Serializes the connection data to XML.
			/// </summary>
			/// <param name="writer">XML destination that we're serializing to.</param>
			public void WriteXml(XmlWriter writer)
			{
				// Write an explicit isLegacy="false" attribute so that we don't try to deserialize this using LegacyHistoricalConnection.
				writer.WriteAttributeString("isLegacy", "false");
				writer.WriteElementString("LastConnection", LastConnection.ToString());

				writer.WriteStartElement("Connection");

				XmlSerializer serializer = new XmlSerializer(Connection.GetType());

				serializer.Serialize(writer, Connection);
				writer.WriteEndElement();
			}
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

		/// <summary>
		/// Legacy class designed to accommodate when derived the history connections from <see cref="RdpConnection"/>.
		/// </summary>
		[Serializable]
		[XmlRoot("HistoricalConnection")]
		public class LegacyHistoricalConnection : RdpConnection
		{
			/// <summary>
			/// Time the user made the connection.
			/// </summary>
			public DateTime LastConnection
			{
				get;
				set;
			}
		}
	}
}