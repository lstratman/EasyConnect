using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using EasyConnect.Properties;

namespace EasyConnect
{
    public partial class SaveConnectionWindow : Form
    {
        public SaveConnectionWindow(MainForm applicationForm, BookmarksFolder currentFolder)
        {
            InitializeComponent();
            bookmarksTreeView.Nodes.Add((TreeNode)applicationForm.Bookmarks.FoldersTreeView.Nodes[0].Clone());

            if (currentFolder == null)
                bookmarksTreeView.SelectedNode = bookmarksTreeView.Nodes[0];

            else
            {
                TreeNode folderNode =
                    applicationForm.Bookmarks.TreeNodeFolders.Single(kvp => kvp.Value == currentFolder).Key;
                string path = "";

                while (folderNode != null)
                {
                    path = "/" + folderNode.Index + path;
                    folderNode = folderNode.Parent;
                }

                folderNode = bookmarksTreeView.Nodes[0];
                int[] pathIndexes = (from index in path.Split('/')
                                     where !String.IsNullOrEmpty(index)
                                     select Convert.ToInt32(index)).ToArray();

                for (int i = 1; i < pathIndexes.Length - 1; i++)
                {
                    folderNode.Nodes[pathIndexes[i]].Expand();
                    folderNode = folderNode.Nodes[pathIndexes[i]];
                }

                if (folderNode.Nodes.Count > 0)
                    bookmarksTreeView.SelectedNode = folderNode.Nodes[pathIndexes.Last()];
            }
        }

        public string ConnectionName
        {
            get
            {
                return nameTextBox.Text;
            }
        }

        public string DestinationFolderPath
        {
            get
            {
                TreeNode currentNode = bookmarksTreeView.SelectedNode;
                string path = "";

                while (currentNode != null)
                {
                    path = "/" + currentNode.Index + path;
                    currentNode = currentNode.Parent;
                }

                return path;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(nameTextBox.Text))
            {
                MessageBox.Show(Resources.EnterNameForThisConnection, Resources.ErrorTitle, MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            if (bookmarksTreeView.SelectedNode.ImageIndex == 1)
                bookmarksTreeView.SelectedNode = bookmarksTreeView.SelectedNode.Parent;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void bookmarksTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.ImageIndex == 1)
            {
                bookmarksTreeView.SelectedNode = e.Node;
                nameTextBox.Text = e.Node.Text;

                okButton_Click(null, null);
            }
        }

        private void bookmarksTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.ImageIndex == 1)
                nameTextBox.Text = e.Node.Text;
        }

        private void bookmarksTreeView_Leave(object sender, EventArgs e)
        {
            if (bookmarksTreeView.SelectedNode != null)
            {
                bookmarksTreeView.SelectedNode.BackColor = SystemColors.Highlight;
                bookmarksTreeView.SelectedNode.ForeColor = SystemColors.HighlightText;
            }
        }

        private void bookmarksTreeView_Enter(object sender, EventArgs e)
        {
            if (bookmarksTreeView.SelectedNode != null)
            {
                bookmarksTreeView.SelectedNode.BackColor = SystemColors.Window;
                bookmarksTreeView.SelectedNode.ForeColor = SystemColors.WindowText;
            }
        }
    }
}