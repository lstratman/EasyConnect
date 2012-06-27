using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
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
    public partial class HistoryWindow : Form
    {
        private readonly Dictionary<ListViewItem, HistoricalConnection> _connections =
            new Dictionary<ListViewItem, HistoricalConnection>();

        protected Dictionary<Type, int> _connectionTypeIcons = new Dictionary<Type, int>();

        protected string _historyFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                            "\\EasyConnect\\History.xml";

        protected MainForm _applicationForm = null;

        public HistoryWindow(MainForm applicationForm)
        {
            _applicationForm = applicationForm;
            
            InitializeComponent();

            _historyListView.ListViewItemSorter = new HistoryComparer(_connections);

            foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
            {
                Icon icon = new Icon(protocol.ProtocolIcon, 16, 16);

                historyImageList.Images.Add(icon);
                _connectionTypeIcons[protocol.ConnectionType] = historyImageList.Images.Count - 1;
            }

            if (File.Exists(_historyFileName))
            {
                XmlSerializer historySerializer = new XmlSerializer(typeof(List<HistoricalConnection>));
                List<HistoricalConnection> historicalConnections = null;

                using (XmlReader historyReader = new XmlTextReader(_historyFileName))
                {
                    historicalConnections = (List<HistoricalConnection>)historySerializer.Deserialize(historyReader);
                }

                foreach (HistoricalConnection historyEntry in historicalConnections)
                    AddToHistory(historyEntry);
            }
        }

        public List<HistoricalConnection> Connections
        {
            get
            {
                return _connections.Values.ToList();
            }
        }

        public IConnection FindInHistory(Guid historyGuid)
        {
            return _connections.Values.FirstOrDefault((HistoricalConnection c) => c.Connection.Guid == historyGuid) == null
                       ? null
                       : _connections.Values.First((HistoricalConnection c) => c.Connection.Guid == historyGuid).Connection;
        }

        public void AddToHistory(IConnection connection)
        {
            HistoricalConnection historyEntry = new HistoricalConnection(connection)
                                                    {
                                                        LastConnection = DateTime.Now
                                                    };
            
            AddToHistory(historyEntry);
            Save();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _historyListView.BeginInvoke(new Action(_historyListView.Sort));
        }

        protected void AddToHistory(HistoricalConnection historyEntry)
        {
            if (_historyListView.Groups[historyEntry.LastConnection.ToString("yyyy-MM-dd")] == null)
                _historyListView.Groups.Add(
                    new ListViewGroup(historyEntry.LastConnection.ToString("yyyy-MM-dd"), historyEntry.LastConnection.ToLongDateString()));

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

        public void Save()
        {
            FileInfo destinationFile = new FileInfo(_historyFileName);
            XmlSerializer historySerializer = new XmlSerializer(typeof(List<HistoricalConnection>));

            Directory.CreateDirectory(destinationFile.DirectoryName);

            foreach (KeyValuePair<ListViewItem, HistoricalConnection> connection in _connections.Where(kvp => kvp.Value.LastConnection < DateTime.Now.AddDays(-14)).ToList())
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

        private void propertiesMenuItem_Click(object sender, EventArgs e)
        {
            Form optionsWindow = ConnectionFactory.CreateOptionsForm(_connections[_historyListView.SelectedItems[0]].Connection);
            TitleBarTab optionsTab = new TitleBarTab(_applicationForm)
            {
                Content = optionsWindow
            };

            _applicationForm.Tabs.Add(optionsTab);
            _applicationForm.SelectedTab = optionsTab;
        }

        private void connectMenuItem_Click(object sender, EventArgs e)
        {
            _applicationForm.Connect(_connections[_historyListView.SelectedItems[0]].Connection);
        }

        public class HistoricalConnection : ISerializable, IXmlSerializable
        {
            public IConnection Connection
            {
                get;
                set;
            }

            public HistoricalConnection()
            {
            }

            public HistoricalConnection(SerializationInfo info, StreamingContext context)
            {
                LastConnection = info.GetDateTime("LastConnection");
                Connection = (IConnection) info.GetValue("Connection", typeof (IConnection));
            }

            public HistoricalConnection(IConnection connection)
            {
                Connection = connection;
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("LastConnection", LastConnection);
                info.AddValue("Connection", Connection);
            }

            public DateTime LastConnection
            {
                get;
                set;
            }

            public XmlSchema GetSchema()
            {
                return null;
            }

            public void ReadXml(XmlReader reader)
            {
                if (reader.GetAttribute("isLegacy") != "false")
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(LegacyHistoricalConnection));
                    LegacyHistoricalConnection legacyHistoricalConnection = (LegacyHistoricalConnection)serializer.Deserialize(reader);

                    LastConnection = legacyHistoricalConnection.LastConnection;
                    Connection = legacyHistoricalConnection;
                }

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
                                Connection = (IConnection)serializer.Deserialize(reader);

                                reader.Read();

                                break;
                        }
                    }

                    reader.Read();
                }
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("isLegacy", "false");
                writer.WriteElementString("LastConnection", LastConnection.ToString());

                writer.WriteStartElement("Connection");

                XmlSerializer serializer = new XmlSerializer(Connection.GetType());

                serializer.Serialize(writer, Connection);
                writer.WriteEndElement();
            }
        }

        [Serializable]
        [XmlRoot("HistoricalConnection")]
        public class LegacyHistoricalConnection : RdpConnection
        {
            public DateTime LastConnection
            {
                get;
                set;
            }
        }

        protected class HistoryComparer : IComparer
        {
            private Dictionary<ListViewItem, HistoricalConnection> _connections = null;

            public HistoryComparer(Dictionary<ListViewItem, HistoricalConnection> connections)
            {
                _connections = connections;
            }

            public int Compare(object x, object y)
            {
                HistoricalConnection connectionX = _connections[(ListViewItem)x];
                HistoricalConnection connectionY = _connections[(ListViewItem)y];

                return connectionY.LastConnection.CompareTo(connectionX.LastConnection);
            }
        }

        private void _historyListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _historyListView.SelectedItems.Count > 0)
                historyContextMenu.Show(Cursor.Position);
        }

        private void _historyListView_DoubleClick(object sender, EventArgs e)
        {
            if (_historyListView.SelectedItems.Count > 0)
                _applicationForm.Connect(_connections[_historyListView.SelectedItems[0]].Connection);
        }
    }
}