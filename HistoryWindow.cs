using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace EasyConnect
{
    public partial class HistoryWindow : Form
    {
        private readonly Dictionary<TreeNode, HistoricalConnection> _connections =
            new Dictionary<TreeNode, HistoricalConnection>();

        protected string _historyFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                            "\\EasyConnect\\History.xml";

        protected MainForm _applicationForm = null;

        public HistoryWindow(MainForm applicationForm)
        {
            _applicationForm = applicationForm;

            InitializeComponent();

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
                    TreeNode newTreeNode = new TreeNode(historyEntry.DisplayName, 2, 2);

                    if (historyEntry.LastConnection.DayOfYear == DateTime.Now.DayOfYear &&
                        historyEntry.LastConnection.Year == DateTime.Now.Year)
                        AddTreeNode(historyTreeView.Nodes[0].Nodes[0].Nodes, newTreeNode);

                    else if (historyEntry.LastConnection.DayOfYear == DateTime.Now.DayOfYear - 1 &&
                             historyEntry.LastConnection.Year == DateTime.Now.Year)
                        AddTreeNode(historyTreeView.Nodes[0].Nodes[1].Nodes, newTreeNode);

                    else if (historyEntry.LastConnection.DayOfYear >=
                             DateTime.Now.DayOfYear - (int) DateTime.Now.DayOfWeek &&
                             historyEntry.LastConnection.Year == DateTime.Now.Year)
                        AddTreeNode(historyTreeView.Nodes[0].Nodes[2].Nodes, newTreeNode);

                    else if (historyEntry.LastConnection.Month == DateTime.Now.Month &&
                             historyEntry.LastConnection.Year == DateTime.Now.Year)
                        AddTreeNode(historyTreeView.Nodes[0].Nodes[3].Nodes, newTreeNode);

                    else if (historyEntry.LastConnection.Year == DateTime.Now.Year)
                        AddTreeNode(historyTreeView.Nodes[0].Nodes[4].Nodes, newTreeNode);

                    else
                        continue;

                    _connections[newTreeNode] = historyEntry;
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
            TreeNode connectionNode = FindConnectionNode(historyTreeView.Nodes, connection);
            HistoricalConnection historyEntry = new HistoricalConnection(connection, _applicationForm.Password);

            if (connectionNode != null)
            {
                if (!String.IsNullOrEmpty(_connections[connectionNode].Name) && String.IsNullOrEmpty(connection.Name))
                    historyEntry.Name = _connections[connectionNode].Name;
            }

            historyEntry.LastConnection = DateTime.Now;

            if (FindConnectionNode(historyTreeView.Nodes[0].Nodes[0].Nodes, connection) == null)
            {
                if (connectionNode != null)
                {
                    _connections.Remove(connectionNode);
                    connectionNode.Remove();
                }

                TreeNode newTreeNode = new TreeNode(historyEntry.DisplayName, 2, 2);

                AddTreeNode(historyTreeView.Nodes[0].Nodes[0].Nodes, newTreeNode);
                _connections[newTreeNode] = historyEntry;
            }

            else if (connectionNode != null)
            {
                TreeNode newTreeNode = new TreeNode(historyEntry.DisplayName, 2, 2);

                _connections.Remove(connectionNode);
                connectionNode.Remove();
                AddTreeNode(historyTreeView.Nodes[0].Nodes[0].Nodes, newTreeNode);
                _connections[newTreeNode] = historyEntry;
            }

            Save();
        }

        protected void AddTreeNode(TreeNodeCollection collection, TreeNode child)
        {
            int insertPoint = 0;

            for (int i = 0; i < collection.Count; i++)
            {
                if (child.ImageIndex != 1 && collection[i].ImageIndex == 1)
                    break;

                if (child.ImageIndex == collection[i].ImageIndex && String.Compare(collection[i].Text, child.Text) > 0)
                    break;

                insertPoint++;
            }

            collection.Insert(insertPoint, child);
        }

        protected TreeNode FindConnectionNode(TreeNodeCollection searchNodes, RdpConnection connection)
        {
            foreach (TreeNode node in searchNodes)
            {
                if (node.ImageIndex != 2)
                {
                    TreeNode foundNode = FindConnectionNode(node.Nodes, connection);

                    if (foundNode != null)
                        return foundNode;
                }

                else
                {
                    if (connection.Host == _connections[node].Host)
                        return node;
                }
            }

            return null;
        }

        private void historyTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.ImageIndex == 2)
                _applicationForm.Connect(_connections[e.Node]);
        }

        public void Save()
        {
            FileInfo destinationFile = new FileInfo(_historyFileName);
            XmlSerializer historySerializer = new XmlSerializer(typeof(List<HistoricalConnection>));

            Directory.CreateDirectory(destinationFile.DirectoryName);

            using (XmlWriter historyWriter = new XmlTextWriter(_historyFileName, new UnicodeEncoding()))
            {
                historySerializer.Serialize(historyWriter, _connections.Values.ToList());
                historyWriter.Flush();
            }
        }

        private void historyTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Node.ImageIndex == 2)
            {
                historyTreeView.SelectedNode = e.Node;
                historyContextMenu.Show(Cursor.Position);
            }
        }

        private void propertiesMenuItem_Click(object sender, EventArgs e)
        {
            RdpConnectionPropertiesWindow connectionWindow = new RdpConnectionPropertiesWindow(_applicationForm,
                                                                     _connections[historyTreeView.SelectedNode]);
            connectionWindow.ShowDialog();
        }

        private void connectMenuItem_Click(object sender, EventArgs e)
        {
            _applicationForm.Connect(_connections[historyTreeView.SelectedNode]);
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
    }
}