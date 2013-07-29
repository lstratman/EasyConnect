using System;
using System.Windows.Forms;
using EasyConnect.Properties;

namespace EasyConnect
{
	/// <summary>
	/// Modal window that appears when the user clicks the "Bookmark this server..." menu item in the bookmarks menu in a <see cref="ConnectionWindow"/>
	/// instance; allows the user to specify a name for the bookmark and the folder in which it should be saved.
	/// </summary>
	public partial class SaveConnectionWindow : Form
	{
		/// <summary>
		/// Constructor; initializes the bookmarks tree view by cloning the current folder structure from <see cref="MainForm.Bookmarks"/>.
		/// </summary>
		/// <param name="applicationForm">Main application form associated with this window.</param>
		public SaveConnectionWindow(MainForm applicationForm)
		{
			InitializeComponent();

			bookmarksTreeView.Nodes.Add((TreeNode) applicationForm.Bookmarks.FoldersTreeView.Nodes[0].Clone());
			bookmarksTreeView.SelectedNode = bookmarksTreeView.Nodes[0];
		}

		/// <summary>
		/// Name for the bookmark entered by the user.
		/// </summary>
		public string ConnectionName
		{
			get
			{
				return nameTextBox.Text;
			}
		}

		/// <summary>
		/// Path to the folder selected by the user; comes in the form of the index of each folder selected in its parent collection, separated by a "/".
		/// </summary>
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

		/// <summary>
		/// Handler method that's called when the user clicks on <see cref="okButton"/>.  Makes sure that the user has entered the required information and
		/// closes the form.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="okButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void okButton_Click(object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty(nameTextBox.Text))
			{
				MessageBox.Show(Resources.EnterNameForThisConnection, Resources.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}