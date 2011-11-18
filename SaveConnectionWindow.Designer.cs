namespace EasyConnect
{
    partial class SaveConnectionWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveConnectionWindow));
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.bookmarksTreeView = new System.Windows.Forms.TreeView();
            this.bookmarkImageList = new System.Windows.Forms.ImageList(this.components);
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.destinationLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(10, 13);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(38, 13);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name:";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(57, 10);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(207, 20);
            this.nameTextBox.TabIndex = 1;
            // 
            // bookmarksTreeView
            // 
            this.bookmarksTreeView.ImageIndex = 0;
            this.bookmarksTreeView.ImageList = this.bookmarkImageList;
            this.bookmarksTreeView.Location = new System.Drawing.Point(13, 58);
            this.bookmarksTreeView.Name = "bookmarksTreeView";
            this.bookmarksTreeView.SelectedImageIndex = 0;
            this.bookmarksTreeView.Size = new System.Drawing.Size(251, 192);
            this.bookmarksTreeView.TabIndex = 2;
            this.bookmarksTreeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.bookmarksTreeView_NodeMouseDoubleClick);
            this.bookmarksTreeView.Enter += new System.EventHandler(this.bookmarksTreeView_Enter);
            this.bookmarksTreeView.Leave += new System.EventHandler(this.bookmarksTreeView_Leave);
            this.bookmarksTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.bookmarksTreeView_NodeMouseClick);
            // 
            // bookmarksImageList
            // 
            this.bookmarkImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("bookmarksImageList.ImageStream")));
            this.bookmarkImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.bookmarkImageList.Images.SetKeyName(0, "Folder.png");
            this.bookmarkImageList.Images.SetKeyName(1, "RDCSmall.ico");
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(190, 257);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(107, 257);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // destinationLabel
            // 
            this.destinationLabel.AutoSize = true;
            this.destinationLabel.Location = new System.Drawing.Point(10, 42);
            this.destinationLabel.Name = "destinationLabel";
            this.destinationLabel.Size = new System.Drawing.Size(89, 13);
            this.destinationLabel.TabIndex = 5;
            this.destinationLabel.Text = "Destination folder";
            // 
            // SaveConnectionWindow
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(276, 294);
            this.ControlBox = false;
            this.Controls.Add(this.destinationLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.bookmarksTreeView);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.nameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SaveConnectionWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Save";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.TreeView bookmarksTreeView;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ImageList bookmarkImageList;
        private System.Windows.Forms.Label destinationLabel;
    }
}