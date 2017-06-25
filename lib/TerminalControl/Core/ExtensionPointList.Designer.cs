namespace Poderosa.Forms {
    partial class ExtensionPointList {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this._list = new System.Windows.Forms.ListView();
            this._idHeader = new System.Windows.Forms.ColumnHeader();
            this._ownerHeader = new System.Windows.Forms.ColumnHeader();
            this._countHeader = new System.Windows.Forms.ColumnHeader();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _list
            // 
            this._list.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._idHeader,
            this._ownerHeader,
            this._countHeader});
            this._list.FullRowSelect = true;
            this._list.GridLines = true;
            this._list.Location = new System.Drawing.Point(-1, -1);
            this._list.MultiSelect = false;
            this._list.Name = "_list";
            this._list.Size = new System.Drawing.Size(558, 209);
            this._list.TabIndex = 0;
            this._list.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this._list.UseCompatibleStateImageBehavior = false;
            this._list.View = System.Windows.Forms.View.Details;
            // 
            // _idHeader
            // 
            this._idHeader.Width = 200;
            // 
            // _versionHeader
            // 
            this._ownerHeader.Width = 200;
            // 
            // _countHeader
            // 
            this._countHeader.Width = 100;
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(389, 214);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 1;
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(470, 214);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // PluginList
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(557, 243);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._list);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExtensionPointList";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ExtensionPointList";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView _list;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.ColumnHeader _idHeader;
        private System.Windows.Forms.ColumnHeader _ownerHeader;
        private System.Windows.Forms.ColumnHeader _countHeader;
    }
}