using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace UltraRDC
{
    public partial class SaveConnectionWindow : Form
    {
        public SaveConnectionWindow(TreeNode favorites)
        {
            InitializeComponent();
            favoritesTreeView.Nodes.Add(favorites);
            favoritesTreeView.SelectedNode = favoritesTreeView.Nodes[0];
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
                TreeNode currentNode = favoritesTreeView.SelectedNode;
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
                MessageBox.Show("Please enter a name for this connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (favoritesTreeView.SelectedNode.ImageIndex == 1)
                favoritesTreeView.SelectedNode = favoritesTreeView.SelectedNode.Parent;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void favoritesTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.ImageIndex == 1)
            {
                favoritesTreeView.SelectedNode = e.Node;
                nameTextBox.Text = e.Node.Text;

                okButton_Click(null, null);
            }
        }

        private void favoritesTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.ImageIndex == 1)
                nameTextBox.Text = e.Node.Text;
        }

        private void favoritesTreeView_Leave(object sender, EventArgs e)
        {
            if (favoritesTreeView.SelectedNode != null)
            {
                favoritesTreeView.SelectedNode.BackColor = SystemColors.Highlight;
                favoritesTreeView.SelectedNode.ForeColor = SystemColors.HighlightText;
            }
        }

        private void favoritesTreeView_Enter(object sender, EventArgs e)
        {
            if (favoritesTreeView.SelectedNode != null)
            {
                favoritesTreeView.SelectedNode.BackColor = SystemColors.Window;
                favoritesTreeView.SelectedNode.ForeColor = SystemColors.WindowText;
            }
        }
    }
}
