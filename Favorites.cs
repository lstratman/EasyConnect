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
    public class Favorites
    {
        protected string _favoritesFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Favorites.xml";
        protected MainForm.ConnectionDelegate _connectionDelegate = null;
        protected SecureString _password = null;
        protected FavoritesFolder _rootFolder = new FavoritesFolder();

        public FavoritesFolder RootFolder
        {
            get
            {
                return _rootFolder;
            }
        }

        public Favorites(MainForm.ConnectionDelegate connectionDelegate, SecureString password)
        {
            _connectionDelegate = connectionDelegate;
            _password = password;

            if (File.Exists(_favoritesFileName))
            {
                XmlDocument favorites = new XmlDocument();
                favorites.Load(_favoritesFileName);

                _rootFolder.Favorites.Clear();
                _rootFolder.ChildFolders.Clear();

                InitializeTreeView(favorites.SelectSingleNode("/favorites"), _rootFolder);
            }
        }

        protected void InitializeTreeView(XmlNode favoritesFolder, FavoritesFolder currentFolder)
        {
            if (favoritesFolder == null)
                return;

            foreach (XmlNode favorite in favoritesFolder.SelectNodes("favorite"))
                currentFolder.Favorites.Add(new RDCConnection(favorite, _password));

            foreach (XmlNode folder in favoritesFolder.SelectNodes("folder"))
            {
                FavoritesFolder newFolder = new FavoritesFolder
                                                {
                                                    Name = folder.SelectSingleNode("@name").Value
                                                };

                currentFolder.ChildFolders.Add(newFolder);
                InitializeTreeView(folder, newFolder);
            }
        }

        public SecureString Password
        {
            set
            {
                _password = value;
            }
        }

        public void Save()
        {
            XmlDocument favoritesFile = new XmlDocument();
            XmlNode rootNode = favoritesFile.CreateNode(XmlNodeType.Element, "favorites", null);

            favoritesFile.AppendChild(rootNode);
            SaveTreeView(_rootFolder, rootNode);

            FileInfo destinationFile = new FileInfo(_favoritesFileName);

            Directory.CreateDirectory(destinationFile.DirectoryName);
            favoritesFile.Save(_favoritesFileName);
        }

        protected void SaveTreeView(FavoritesFolder currentFolder, XmlNode parentNode)
        {
            foreach (RDCConnection favorite in currentFolder.Favorites)
            {
                XmlNode connectionNode = parentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "favorite", null);
                parentNode.AppendChild(connectionNode);

                favorite.ToXmlNode(connectionNode);
            }

            foreach (FavoritesFolder folder in currentFolder.ChildFolders)
            {
                XmlNode folderNode = parentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "folder", null);

                folderNode.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("name"));
                folderNode.Attributes["name"].Value = folder.Name;

                parentNode.AppendChild(folderNode);
                SaveTreeView(folder, folderNode);
            }
        }

        /*private void newFolderButton_Click(object sender, EventArgs e)
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

        private void propertiesButton_Click(object sender, EventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(this, _connections[favoritesTreeView.SelectedNode], _connectionDelegate, _password);
            connectionWindow.ShowDialog();
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
        }*/
    }
}
