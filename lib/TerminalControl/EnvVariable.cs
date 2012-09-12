/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: EnvVariable.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Poderosa.Forms
{
	/// <summary>
	/// EnvVariable の概要の説明です。
	/// </summary>
	internal class EnvVariableList : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListView _list;
		private System.Windows.Forms.Button _addButton;
		private System.Windows.Forms.Button _editButton;
		private System.Windows.Forms.Button _deleteButton;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.ColumnHeader _nameHeader;
		private System.Windows.Forms.ColumnHeader _valueHeader;
		private System.Windows.Forms.Button _cancelButton;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EnvVariableList()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			this.Text = GApp.Strings.GetString("Form.EnvVariableList.Text");
			this._nameHeader.Text = GApp.Strings.GetString("Form.EnvVariableList._nameHeader");
			this._valueHeader.Text = GApp.Strings.GetString("Form.EnvVariableList._valueHeader");
			this._addButton.Text = GApp.Strings.GetString("Form.EnvVariableList._addButton");
			this._editButton.Text = GApp.Strings.GetString("Form.EnvVariableList._editButton");
			this._deleteButton.Text = GApp.Strings.GetString("Form.EnvVariableList._deleteButton");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this._okButton.Text = GApp.Strings.GetString("Common.OK");

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			Init();
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this._list = new System.Windows.Forms.ListView();
			this._nameHeader = new System.Windows.Forms.ColumnHeader();
			this._valueHeader = new System.Windows.Forms.ColumnHeader();
			this._addButton = new System.Windows.Forms.Button();
			this._editButton = new System.Windows.Forms.Button();
			this._deleteButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _list
			// 
			this._list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					this._nameHeader,
																					this._valueHeader});
			this._list.FullRowSelect = true;
			this._list.GridLines = true;
			this._list.LabelEdit = true;
			this._list.Name = "_list";
			this._list.Size = new System.Drawing.Size(272, 168);
			this._list.TabIndex = 0;
			this._list.MultiSelect = false;
			this._list.Sorting = SortOrder.Ascending;
			this._list.View = System.Windows.Forms.View.Details;
			this._list.SelectedIndexChanged += new System.EventHandler(this.OnSelectedItemChanged);
			// 
			// _nameHeader
			// 
			this._nameHeader.Width = 80;
			// 
			// _valueHeader
			// 
			this._valueHeader.Width = 180;
			// 
			// _addButton
			// 
			this._addButton.Location = new System.Drawing.Point(280, 8);
			this._addButton.Name = "_addButton";
			this._addButton.FlatStyle = FlatStyle.System;
			this._addButton.TabIndex = 1;
			this._addButton.Click += new System.EventHandler(this.OnAddButtonClicked);
			// 
			// _editButton
			// 
			this._editButton.Location = new System.Drawing.Point(280, 40);
			this._editButton.Name = "_editButton";
			this._editButton.FlatStyle = FlatStyle.System;
			this._editButton.TabIndex = 2;
			this._editButton.Click += new System.EventHandler(this.OnEditButtonClicked);
			// 
			// _deleteButton
			// 
			this._deleteButton.Location = new System.Drawing.Point(280, 72);
			this._deleteButton.Name = "_deleteButton";
			this._deleteButton.FlatStyle = FlatStyle.System;
			this._deleteButton.TabIndex = 3;
			this._deleteButton.Click += new System.EventHandler(this.OnDeleteButtonClicked);
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.Location = new System.Drawing.Point(280, 112);
			this._okButton.Name = "_okButton";
			this._okButton.FlatStyle = FlatStyle.System;
			this._okButton.TabIndex = 4;
			this._okButton.Click += new System.EventHandler(this.OnOK);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(280, 144);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.TabIndex = 5;
			// 
			// EnvVariable
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(360, 175);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._editButton,
																		  this._cancelButton,
																		  this._okButton,
																		  this._deleteButton,
																		  this._addButton,
																		  this._list});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EnvVariableList";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion

		private void Init() {
			IDictionaryEnumerator de = GApp.MacroManager.EnvironmentVariables;
			while(de.MoveNext()) {
				if(!(de.Value is string)) continue;
				ListViewItem li = new ListViewItem((string)de.Key);
				li = _list.Items.Add(li);
				li.SubItems.Add((string)de.Value);
			}
			_editButton.Enabled = false;
			_deleteButton.Enabled = false;
		}
		private void OnSelectedItemChanged(object sender, EventArgs args) {
			_editButton.Enabled = true;
			_deleteButton.Enabled = true;
		}

		private void OnAddButtonClicked(object sender, EventArgs args) {
			EditEnvVariable d = new EditEnvVariable(this);
			d.IsNewVariable = true;
			if(GCUtil.ShowModalDialog(this, d)==DialogResult.OK) {
				ListViewItem li = new ListViewItem(d.VarName);
				li = _list.Items.Add(li);
				li.SubItems.Add(d.VarValue);
				li.Selected = true;
			}
		}
		private void OnEditButtonClicked(object sender, EventArgs args) {
			ListViewItem li = _list.SelectedItems[0];
			EditEnvVariable d = new EditEnvVariable(this);
			d.VarName = li.Text;
			d.VarValue = li.SubItems[1].Text;
			d.IsNewVariable = false;
			if(GCUtil.ShowModalDialog(this, d)==DialogResult.OK) {
				li.Text = d.VarName;
				li.SubItems[1].Text = d.VarValue;
			}
		}
		private void OnDeleteButtonClicked(object sender, EventArgs args) {
			if(_list.SelectedIndices.Count>0)
				_list.Items.RemoveAt(_list.SelectedIndices[0]);
		}

		private void OnOK(object sender, EventArgs args) {
			Hashtable t = new Hashtable();
			foreach(ListViewItem li in _list.Items) {
				t[li.Text] = li.SubItems[1].Text;
			}

			GApp.MacroManager.ResetEnvironmentVariables(t);
		}
		
		public bool HasVariable(string name) {
			foreach(ListViewItem li in _list.Items) {
				if(name==li.Text) return true;
			}
			return false;
		}
	}
}
