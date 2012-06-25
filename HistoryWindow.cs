using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Xml;
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

        protected string _historyFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                            "\\EasyConnect\\History.xml";

        protected MainForm _applicationForm = null;

        public HistoryWindow(MainForm applicationForm)
        {
            _applicationForm = applicationForm;
            
            InitializeComponent();

            _historyListView.ListViewItemSorter = new HistoryComparer(_connections);

            if (File.Exists(_historyFileName))
            {
                XmlSerializer historySerializer = new XmlSerializer(typeof(List<HistoricalConnection>));
                List<HistoricalConnection> historicalConnections = null;

                using (XmlReader historyReader = new XmlTextReader(_historyFileName))
                {
                    historicalConnections = (List<HistoricalConnection>)historySerializer.Deserialize(historyReader);
                }

                foreach (HistoricalConnection historyEntry in historicalConnections)
                {
                    historyEntry.EncryptionPassword = _applicationForm.Password;
                    AddToHistory(historyEntry);
                }
            }
        }

        public List<HistoricalConnection> Connections
        {
            get
            {
                return _connections.Values.ToList();
            }
        }

        public RdpConnection FindInHistory(Guid historyGuid)
        {
            return _connections.Values.FirstOrDefault((HistoricalConnection c) => c.Guid == historyGuid);
        }

        public void AddToHistory(RdpConnection connection)
        {
            HistoricalConnection historyEntry = new HistoricalConnection(connection, _applicationForm.Password)
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

            ListViewItem listViewItem = new ListViewItem(historyEntry.LastConnection.ToLongTimeString(), 0);

            listViewItem.SubItems.Add(historyEntry.DisplayName);
            listViewItem.SubItems.Add(historyEntry.Host);

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
            Form optionsWindow = ConnectionFactory.CreateOptionsForm(_connections[_historyListView.SelectedItems[0]]);
            TitleBarTab optionsTab = new TitleBarTab(_applicationForm)
            {
                Content = optionsWindow
            };

            _applicationForm.Tabs.Add(optionsTab);
            _applicationForm.SelectedTab = optionsTab;
        }

        private void connectMenuItem_Click(object sender, EventArgs e)
        {
            _applicationForm.Connect(_connections[_historyListView.SelectedItems[0]]);
        }

        public class HistoricalConnection : RdpConnection
        {
            public HistoricalConnection()
            {
            }

            public HistoricalConnection(SerializationInfo info, StreamingContext context) 
                : base(info, context)
            {
                LastConnection = info.GetDateTime("LastConnection");
            }

            public HistoricalConnection(RdpConnection connection, SecureString encryptionPassword)
            {
                Host = connection.Host;
                Username = connection.Username;
                DesktopWidth = connection.DesktopWidth;
                DesktopHeight = connection.DesktopHeight;
                ColorDepth = connection.ColorDepth;
                Name = connection.Name;
                IsBookmark = connection.IsBookmark;
                AudioMode = connection.AudioMode;
                KeyboardMode = connection.KeyboardMode;
                ConnectPrinters = connection.ConnectPrinters;
                ConnectClipboard = connection.ConnectClipboard;
                ConnectDrives = connection.ConnectDrives;
                DesktopBackground = connection.DesktopBackground;
                FontSmoothing = connection.FontSmoothing;
                DesktopComposition = connection.DesktopComposition;
                WindowContentsWhileDragging = connection.WindowContentsWhileDragging;
                Animations = connection.Animations;
                VisualStyles = connection.VisualStyles;
                PersistentBitmapCaching = connection.PersistentBitmapCaching;
                Password = connection.Password;
                Guid = connection.Guid;
                EncryptionPassword = encryptionPassword;
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);

                info.AddValue("LastConnection", LastConnection);
            }

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
                _applicationForm.Connect(_connections[_historyListView.SelectedItems[0]]);
        }
    }
}