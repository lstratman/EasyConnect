using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Security;
using System.Linq;
using WeifenLuo.WinFormsUI.Docking;

namespace UltraRDC
{
    public partial class HistoryWindow : DockContent
    {
        Dictionary<TreeNode, HistoricalConnection> _connections = new Dictionary<TreeNode, HistoricalConnection>();
        MainForm.ConnectionDelegate _connectionDelegate = null;
        FavoritesWindow _favoritesWindow = null;
        protected string _historyFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\UltraRDC\\History.xml";
        protected SecureString _password = null;

        public HistoryWindow(MainForm.ConnectionDelegate connectionDelegate, FavoritesWindow favoritesWindow, SecureString password)
        {
            _connectionDelegate = connectionDelegate;
            _favoritesWindow = favoritesWindow;
            _password = password;

            InitializeComponent();

            if (File.Exists(_historyFileName))
            {
                XmlDocument history = new XmlDocument();
                history.Load(_historyFileName);

                foreach (XmlNode node in history.SelectNodes("/history/connection"))
                {
                    HistoricalConnection historyEntry = new HistoricalConnection(node, _password);
                    TreeNode newTreeNode = new TreeNode((String.IsNullOrEmpty(historyEntry.Name) ? historyEntry.Host : historyEntry.Name), 2, 2);

                    if (historyEntry.LastConnection.DayOfYear == DateTime.Now.DayOfYear && historyEntry.LastConnection.Year == DateTime.Now.Year)
                        AddTreeNode(historyTreeView.Nodes[0].Nodes[0].Nodes, newTreeNode);

                    else if (historyEntry.LastConnection.DayOfYear == DateTime.Now.DayOfYear - 1 && historyEntry.LastConnection.Year == DateTime.Now.Year)
                        AddTreeNode(historyTreeView.Nodes[0].Nodes[1].Nodes, newTreeNode);

                    else if (historyEntry.LastConnection.DayOfYear >= DateTime.Now.DayOfYear - (int)DateTime.Now.DayOfWeek && historyEntry.LastConnection.Year == DateTime.Now.Year)
                        AddTreeNode(historyTreeView.Nodes[0].Nodes[2].Nodes, newTreeNode);

                    else if (historyEntry.LastConnection.Month == DateTime.Now.Month && historyEntry.LastConnection.Year == DateTime.Now.Year)
                        AddTreeNode(historyTreeView.Nodes[0].Nodes[3].Nodes, newTreeNode);

                    else if (historyEntry.LastConnection.Year == DateTime.Now.Year)
                        AddTreeNode(historyTreeView.Nodes[0].Nodes[4].Nodes, newTreeNode);

                    else
                        continue;

                    _connections[newTreeNode] = historyEntry;
                }
            }
        }

        public SecureString Password
        {
            set
            {
                _password = value;

                if (_password != null)
                    _password.MakeReadOnly();
            }
        }

        public List<HistoricalConnection> Connections
        {
            get
            {
                return _connections.Values.ToList();
            }
        }

        public RDCConnection FindInHistory(Guid historyGuid)
        {
            return (RDCConnection)_connections.Values.FirstOrDefault((HistoricalConnection c) => c.Guid == historyGuid);
        }

        public void AddToHistory(RDCConnection connection)
        {
            TreeNode connectionNode = FindConnectionNode(historyTreeView.Nodes, connection);
            HistoricalConnection historyEntry = new HistoricalConnection(connection, _password);

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

                TreeNode newTreeNode = new TreeNode((String.IsNullOrEmpty(historyEntry.Name) ? historyEntry.Host : historyEntry.Name), 2, 2);

                AddTreeNode(historyTreeView.Nodes[0].Nodes[0].Nodes, newTreeNode);
                _connections[newTreeNode] = historyEntry;
            }

            else if (connectionNode != null)
            {
                TreeNode newTreeNode = new TreeNode((String.IsNullOrEmpty(historyEntry.Name) ? historyEntry.Host : historyEntry.Name), 2, 2);

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

        protected TreeNode FindConnectionNode(TreeNodeCollection searchNodes, RDCConnection connection)
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

        public class HistoricalConnection : RDCConnection
        {
            public HistoricalConnection(XmlNode node, SecureString encryptionPassword)
                : base(node, encryptionPassword)
            {
                LastConnection = DateTime.Parse(node.Attributes["lastAccess"].Value);
            }

            public HistoricalConnection(RDCConnection connection, SecureString encryptionPassword)
            {
                Host = connection.Host;
                Username = connection.Username;
                DesktopWidth = connection.DesktopWidth;
                DesktopHeight = connection.DesktopHeight;
                ColorDepth = connection.ColorDepth;
                Name = connection.Name;
                IsFavorite = connection.IsFavorite;
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

            public DateTime LastConnection
            {
                get;
                set;
            }

            public override void ToXmlNode(XmlNode node)
            {
                base.ToXmlNode(node);
                
                node.Attributes.Append(node.OwnerDocument.CreateAttribute("lastAccess"));
                node.Attributes["lastAccess"].Value = LastConnection.ToString();
            }
        }

        private void historyTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.ImageIndex == 2)
                _connectionDelegate(_connections[e.Node]);
        }

        public void Save()
        {
            XmlDocument historyFile = new XmlDocument();
            XmlNode rootNode = historyFile.CreateNode(XmlNodeType.Element, "history", null);

            historyFile.AppendChild(rootNode);

            foreach (HistoricalConnection connection in _connections.Values)
            {
                XmlNode connectionNode = historyFile.CreateNode(XmlNodeType.Element, "connection", null);
                rootNode.AppendChild(connectionNode);

                connection.ToXmlNode(connectionNode);
            }

            FileInfo destinationFile = new FileInfo(_historyFileName);

            Directory.CreateDirectory(destinationFile.DirectoryName);
            historyFile.Save(_historyFileName);
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
            ConnectionWindow connectionWindow = new ConnectionWindow(_favoritesWindow, _connections[historyTreeView.SelectedNode], _connectionDelegate, _password);
            connectionWindow.ShowDialog();
        }

        private void connectMenuItem_Click(object sender, EventArgs e)
        {
            _connectionDelegate(_connections[historyTreeView.SelectedNode]);
        }
    }
}
