using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Security;

namespace EasyConnect
{
    public partial class FavoritesWindow : ToolWindow
    {
        protected TreeNode draggingNode = null;
        protected TreeNode tempDropNode = null;
        protected Dictionary<TreeNode, RDCConnection> _connections = new Dictionary<TreeNode, RDCConnection>();
        protected string _favoritesFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Favorites.xml";
        protected MainForm.ConnectionDelegate _connectionDelegate = null;
        protected SecureString _password = null;

        public FavoritesWindow(MainForm.ConnectionDelegate connectionDelegate, SecureString password)
        {
            InitializeComponent();

            _connectionDelegate = connectionDelegate;
            _password = password;

            if (File.Exists(_favoritesFileName))
            {
                XmlDocument favorites = new XmlDocument();
                favorites.Load(_favoritesFileName);

                favoritesTreeView.Nodes.Clear();
                InitializeTreeView(favorites.SelectNodes("/folders/folder"), favoritesTreeView.Nodes);
            }
        }

        protected void InitializeTreeView(XmlNodeList favoritesNodes, TreeNodeCollection treeNodes)
        {
            foreach (XmlNode node in favoritesNodes)
            {
                TreeNode newFolder = new TreeNode(node.SelectSingleNode("@name").Value, 0, 0);
                AddTreeNode(treeNodes, newFolder);

                foreach (XmlNode connection in node.SelectNodes("connection"))
                {
                    RDCConnection newConnection = new RDCConnection(connection, _password);
                    TreeNode newConnectionNode = new TreeNode(newConnection.Name, 1, 1);

                    AddTreeNode(newFolder.Nodes, newConnectionNode);

                    _connections[newConnectionNode] = newConnection;
                }

                InitializeTreeView(node.SelectNodes("folder"), newFolder.Nodes);
            }
        }

        public SecureString Password
        {
            set
            {
                _password = value;
            }
        }

        public TreeNode TreeRoot
        {
            get
            {
                return favoritesTreeView.Nodes[0];
            }
        }

        public Dictionary<TreeNode, RDCConnection> Connections
        {
            get
            {
                return _connections;
            }
        }

        private void favoritesTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            draggingNode = (TreeNode)e.Item;

            favoritesTreeView.SelectedNode = draggingNode;
            dragImageList.Images.Clear();
            dragImageList.ImageSize = new Size(draggingNode.Bounds.Size.Width + favoritesTreeView.Indent, draggingNode.Bounds.Height);

            // Create new bitmap

            // This bitmap will contain the tree node image to be dragged

            Bitmap bitmap = new Bitmap(draggingNode.Bounds.Width + favoritesTreeView.Indent, draggingNode.Bounds.Height);

            // Get graphics from bitmap

            Graphics graphics = Graphics.FromImage(bitmap);

            // Draw node icon into the bitmap

            graphics.DrawImage(favoritesImageList.Images[draggingNode.ImageIndex], 0, 0);

            // Draw node label into bitmap

            graphics.DrawString(draggingNode.Text, favoritesTreeView.Font, new SolidBrush(favoritesTreeView.ForeColor), (float)favoritesTreeView.Indent, 1.0f);

            // Add bitmap to imagelist

            dragImageList.Images.Add(bitmap);

            // Get mouse position in client coordinates

            Point point = favoritesTreeView.PointToClient(Control.MousePosition);

            // Compute delta between mouse position and node bounds

            int deltaX = point.X + favoritesTreeView.Indent - draggingNode.Bounds.Left;
            int deltaY = point.Y - draggingNode.Bounds.Top;

            if (DragHelper.ImageList_BeginDrag(dragImageList.Handle, 0, deltaX, deltaY))
            {
                // Begin dragging

                favoritesTreeView.DoDragDrop(bitmap, DragDropEffects.Move);
                // End dragging image

                DragHelper.ImageList_EndDrag();
            }

        }

        private void favoritesTreeView_DragDrop(object sender, DragEventArgs e)
        {
            // Unlock updates
            DragHelper.ImageList_DragLeave(favoritesTreeView.Handle);

            // Get drop node
            TreeNode dropNode = favoritesTreeView.GetNodeAt(favoritesTreeView.PointToClient(new Point(e.X, e.Y)));

            // If drop node isn't equal to drag node, add drag node as child of drop node
            if (draggingNode != dropNode && dropNode.ImageIndex != 1)
            {
                // Remove drag node from parent
                if (draggingNode.Parent == null)
                    favoritesTreeView.Nodes.Remove(draggingNode);

                else
                    draggingNode.Parent.Nodes.Remove(draggingNode);

                // Add drag node to drop node
                AddTreeNode(dropNode.Nodes, draggingNode);
                dropNode.Expand();

                // Set drag node to null
                draggingNode = null;
            }

            Save();
        }

        private void favoritesTreeView_DragOver(object sender, DragEventArgs e)
        {
            // Compute drag position and move image
            Point formP = this.PointToClient(new Point(e.X, e.Y));
            DragHelper.ImageList_DragMove(formP.X - favoritesTreeView.Left, formP.Y - favoritesTreeView.Top);

            // Get actual drop node
            TreeNode dropNode = favoritesTreeView.GetNodeAt(favoritesTreeView.PointToClient(new Point(e.X, e.Y)));
            if (dropNode == null || dropNode.ImageIndex == 1)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Move;

            // if mouse is on a new node select it
            if (this.tempDropNode != dropNode)
            {
                DragHelper.ImageList_DragShowNolock(false);
                favoritesTreeView.SelectedNode = dropNode;
                DragHelper.ImageList_DragShowNolock(true);
                tempDropNode = dropNode;
            }

            // Avoid that drop node is child of drag node 
            TreeNode tmpNode = dropNode;
            while (tmpNode.Parent != null)
            {
                if (tmpNode.Parent == draggingNode) e.Effect = DragDropEffects.None;
                tmpNode = tmpNode.Parent;
            }
        }

        private void favoritesTreeView_DragEnter(object sender, DragEventArgs e)
        {
            DragHelper.ImageList_DragEnter(favoritesTreeView.Handle, e.X - favoritesTreeView.Left, e.Y - favoritesTreeView.Top);
        }

        private void favoritesTreeView_DragLeave(object sender, EventArgs e)
        {
            DragHelper.ImageList_DragLeave(favoritesTreeView.Handle);
        }

        private void favoritesTreeView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effect == DragDropEffects.Move)
            {
                // Show pointer cursor while dragging
                e.UseDefaultCursors = false;
                favoritesTreeView.Cursor = Cursors.Default;
            }

            else 
                e.UseDefaultCursors = true;
        }

        private void favoritesTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            deleteButton.Enabled = (e.Node.Level > 0);

            if (e.Node.ImageIndex != 1)
            {
                newFolderButton.Enabled = true;
                propertiesButton.Enabled = false;
            }

            else
            {
                newFolderButton.Enabled = false;
                propertiesButton.Enabled = true;
            }

            if (e.Button == MouseButtons.Right)
            {
                e.Node.TreeView.SelectedNode = e.Node;

                if (e.Node.ImageIndex != 1)
                {
                    favoritesContextMenu.Items[0].Available = false;
                    favoritesContextMenu.Items[1].Available = true;
                    favoritesContextMenu.Items[2].Available = true;
                    favoritesContextMenu.Items[3].Available = true;
                    favoritesContextMenu.Items[4].Available = (e.Node.Level > 0);
                    favoritesContextMenu.Items[5].Available = false;
                }

                else
                {
                    favoritesContextMenu.Items[0].Available = true;
                    favoritesContextMenu.Items[1].Available = false;
                    favoritesContextMenu.Items[2].Available = false;
                    favoritesContextMenu.Items[3].Available = false;
                    favoritesContextMenu.Items[4].Available = true;
                    favoritesContextMenu.Items[5].Available = true;
                }

                favoritesContextMenu.Show(Cursor.Position);
            }
        }

        private void newFolderButton_Click(object sender, EventArgs e)
        {
            RenameWindow folderNameWindow = new RenameWindow();

            folderNameWindow.ValueText = "New Folder";
            folderNameWindow.Title = "Folder Name";

            DialogResult result = folderNameWindow.ShowDialog(this);

            if (result != DialogResult.OK)
                return;

            TreeNode newNode = new TreeNode(folderNameWindow.ValueText, 0, 0);

            favoritesTreeView.SelectedNode.Expand();
            AddTreeNode(favoritesTreeView.SelectedNode.Nodes, newNode);
            favoritesTreeView.SelectedNode = newNode;

            Save();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            favoritesTreeView.SelectedNode.Remove();
            Save();
        }

        public void AddTreeNode(TreeNodeCollection collection, TreeNode child)
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

        private void propertiesButton_Click(object sender, EventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(this, _connections[favoritesTreeView.SelectedNode], _connectionDelegate, _password);
            connectionWindow.ShowDialog();
        }

        public void Save()
        {
            XmlDocument favoritesFile = new XmlDocument();
            XmlNode rootNode = favoritesFile.CreateNode(XmlNodeType.Element, "folders", null);

            favoritesFile.AppendChild(rootNode);
            SaveTreeView(favoritesTreeView.Nodes, rootNode);

            FileInfo destinationFile = new FileInfo(_favoritesFileName);

            Directory.CreateDirectory(destinationFile.DirectoryName);
            favoritesFile.Save(_favoritesFileName);
        }

        protected void SaveTreeView(TreeNodeCollection treeNodes, XmlNode parentNode)
        {
            foreach (TreeNode node in treeNodes)
            {
                if (node.ImageIndex != 1)
                {
                    XmlNode folderNode = parentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "folder", null);

                    folderNode.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("name"));
                    folderNode.Attributes["name"].Value = node.Text;

                    parentNode.AppendChild(folderNode);

                    SaveTreeView(node.Nodes, folderNode);
                }

                else
                {
                    XmlNode connectionNode = parentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "connection", null);
                    parentNode.AppendChild(connectionNode);

                    _connections[node].ToXmlNode(connectionNode);
                }
            }
        }

        private void favoritesTreeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode currentNode = favoritesTreeView.GetNodeAt(new Point(e.X, e.Y));

            if (currentNode == null || currentNode.ImageIndex != 1)
                return;

            _connectionDelegate(_connections[currentNode]);
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            propertiesButton_Click(null, null);
        }

        private void createFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newFolderButton_Click(null, null);
        }

        private void newConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(this, _connectionDelegate, _password);
            connectionWindow.ShowDialog();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameWindow folderNameWindow = new RenameWindow();
            TreeNode node = favoritesTreeView.SelectedNode;
            TreeNode parent = node.Parent;

            folderNameWindow.ValueText = node.Text;
            folderNameWindow.Title = "Folder Name";

            DialogResult result = folderNameWindow.ShowDialog(this);

            if (result != DialogResult.OK)
                return;

            node.Remove();
            node.Text = folderNameWindow.ValueText;

            AddTreeNode(parent.Nodes, node);

            Save();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteButton_Click(null, null);
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectionDelegate(_connections[favoritesTreeView.SelectedNode]);
        }
    }
}
