using System;
using System.Drawing;
using System.Windows.Forms;
using EasyConnect.Properties;

namespace EasyConnect
{
    public partial class SaveConnectionWindow : Form
    {
        public SaveConnectionWindow(TreeNode bookmarks)
        {
            InitializeComponent();
            bookmarksTreeView.Nodes.Add(bookmarks);
            bookmarksTreeView.SelectedNode = bookmarksTreeView.Nodes[0];
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