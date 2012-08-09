/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: CommandOptionPanel.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;

using Poderosa.Toolkit;
using Poderosa.Config;
using Poderosa.UI;

namespace Poderosa.Forms
{
	internal class CommandOptionPanel : OptionDialog.CategoryPanel
	{
		private Commands _commands;

		private System.Windows.Forms.ListView _keyConfigList;
		private System.Windows.Forms.ColumnHeader _commandCategoryHeader;
		private System.Windows.Forms.ColumnHeader _commandNameHeader;
		private System.Windows.Forms.ColumnHeader _commandConfigHeader;
		private System.Windows.Forms.Button _resetKeyConfigButton;
		private System.Windows.Forms.Button _clearKeyConfigButton;
		private System.Windows.Forms.GroupBox _commandConfigGroup;
		private System.Windows.Forms.Label _commandNameLabel;
		private System.Windows.Forms.Label _commandName;
		private System.Windows.Forms.Label _currentConfigLabel;
		private System.Windows.Forms.Label _currentCommand;
		private System.Windows.Forms.Label _newAllocationLabel;
		private HotKey _hotKey;
		private System.Windows.Forms.Button _allocateKeyButton;

		public CommandOptionPanel()
		{
			InitializeComponent();
			FillText();
		}
		private void InitializeComponent() {
			this._keyConfigList = new System.Windows.Forms.ListView();
			this._commandCategoryHeader = new System.Windows.Forms.ColumnHeader();
			this._commandNameHeader = new System.Windows.Forms.ColumnHeader();
			this._commandConfigHeader = new System.Windows.Forms.ColumnHeader();
			this._resetKeyConfigButton = new System.Windows.Forms.Button();
			this._clearKeyConfigButton = new System.Windows.Forms.Button();
			this._commandConfigGroup = new System.Windows.Forms.GroupBox();
			this._commandNameLabel = new System.Windows.Forms.Label();
			this._commandName = new System.Windows.Forms.Label();
			this._currentConfigLabel = new System.Windows.Forms.Label();
			this._currentCommand = new System.Windows.Forms.Label();
			this._hotKey = new Poderosa.Forms.HotKey();
			this._newAllocationLabel = new System.Windows.Forms.Label();
			this._allocateKeyButton = new System.Windows.Forms.Button();

			this._commandConfigGroup.SuspendLayout();
		
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this._keyConfigList,
																					   this._resetKeyConfigButton,
																					   this._clearKeyConfigButton,
																					   this._commandConfigGroup});
			// 
			// _keyConfigList
			// 
			this._keyConfigList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							 this._commandCategoryHeader,
																							 this._commandNameHeader,
																							 this._commandConfigHeader});
			this._keyConfigList.FullRowSelect = true;
			this._keyConfigList.GridLines = true;
			this._keyConfigList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this._keyConfigList.MultiSelect = false;
			this._keyConfigList.Name = "_keyConfigList";
			this._keyConfigList.Size = new System.Drawing.Size(432, 172);
			this._keyConfigList.TabIndex = 0;
			this._keyConfigList.View = System.Windows.Forms.View.Details;
			this._keyConfigList.SelectedIndexChanged += new System.EventHandler(this.OnKeyMapItemActivated);
			// 
			// _commandCategoryHeader
			// 
			this._commandCategoryHeader.Width = 80;
			// 
			// _commandNameHeader
			// 
			this._commandNameHeader.Width = 188;
			// 
			// _commandConfigHeader
			// 
			this._commandConfigHeader.Width = 136;
			// 
			// _resetKeyConfigButton
			// 
			this._resetKeyConfigButton.Location = new System.Drawing.Point(216, 172);
			this._resetKeyConfigButton.Name = "_resetKeyConfigButton";
			this._resetKeyConfigButton.FlatStyle = FlatStyle.System;
			this._resetKeyConfigButton.Size = new System.Drawing.Size(104, 23);
			this._resetKeyConfigButton.TabIndex = 1;
			this._resetKeyConfigButton.Click += new System.EventHandler(this.OnResetKeyConfig);
			// 
			// _clearKeyConfigButton
			// 
			this._clearKeyConfigButton.Location = new System.Drawing.Point(336, 172);
			this._clearKeyConfigButton.Name = "_clearKeyConfigButton";
			this._clearKeyConfigButton.FlatStyle = FlatStyle.System;
			this._clearKeyConfigButton.Size = new System.Drawing.Size(88, 23);
			this._clearKeyConfigButton.TabIndex = 2;
			this._clearKeyConfigButton.Click += new System.EventHandler(this.OnClearKeyConfig);
			// 
			// _commandConfigGroup
			// 
			this._commandConfigGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																							  this._commandNameLabel,
																							  this._commandName,
																							  this._currentConfigLabel,
																							  this._currentCommand,
																							  this._hotKey,
																							  this._newAllocationLabel,
																							  this._allocateKeyButton});
			this._commandConfigGroup.Location = new System.Drawing.Point(8, 196);
			this._commandConfigGroup.Name = "_commandConfigGroup";
			this._commandConfigGroup.FlatStyle = FlatStyle.System;
			this._commandConfigGroup.Size = new System.Drawing.Size(416, 96);
			this._commandConfigGroup.TabIndex = 3;
			this._commandConfigGroup.TabStop = false;
			// 
			// _commandNameLabel
			// 
			this._commandNameLabel.Location = new System.Drawing.Point(8, 16);
			this._commandNameLabel.Name = "_commandNameLabel";
			this._commandNameLabel.Size = new System.Drawing.Size(88, 23);
			this._commandNameLabel.TabIndex = 4;
			this._commandNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _commandName
			// 
			this._commandName.Location = new System.Drawing.Point(112, 16);
			this._commandName.Name = "_commandName";
			this._commandName.Size = new System.Drawing.Size(248, 23);
			this._commandName.TabIndex = 5;
			this._commandName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _currentConfigLabel
			// 
			this._currentConfigLabel.Location = new System.Drawing.Point(8, 40);
			this._currentConfigLabel.Name = "_currentConfigLabel";
			this._currentConfigLabel.Size = new System.Drawing.Size(88, 23);
			this._currentConfigLabel.TabIndex = 6;
			this._currentConfigLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _currentCommand
			// 
			this._currentCommand.Location = new System.Drawing.Point(112, 40);
			this._currentCommand.Name = "_currentCommand";
			this._currentCommand.Size = new System.Drawing.Size(248, 23);
			this._currentCommand.TabIndex = 7;
			this._currentCommand.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _hotKey
			// 
			this._hotKey.DebugTextBox = null;
			this._hotKey.ImeMode = System.Windows.Forms.ImeMode.Disable;
			this._hotKey.Key = System.Windows.Forms.Keys.None;
			this._hotKey.Location = new System.Drawing.Point(112, 64);
			this._hotKey.Name = "_hotKey";
			this._hotKey.Size = new System.Drawing.Size(168, 19);
			this._hotKey.TabIndex = 8;
			this._hotKey.Text = "";
			// 
			// _newAllocationLabel
			// 
			this._newAllocationLabel.Location = new System.Drawing.Point(8, 64);
			this._newAllocationLabel.Name = "_newAllocationLabel";
			this._newAllocationLabel.Size = new System.Drawing.Size(88, 23);
			this._newAllocationLabel.TabIndex = 9;
			this._newAllocationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _allocateKeyButton
			// 
			this._allocateKeyButton.Enabled = false;
			this._allocateKeyButton.Location = new System.Drawing.Point(288, 64);
			this._allocateKeyButton.Name = "_allocateKeyButton";
			this._allocateKeyButton.FlatStyle = FlatStyle.System;
			this._allocateKeyButton.Size = new System.Drawing.Size(75, 24);
			this._allocateKeyButton.TabIndex = 10;
			this._allocateKeyButton.Click += new System.EventHandler(this.OnAllocateKey);

			this.BackColor = ThemeUtil.TabPaneBackColor;
			this._commandConfigGroup.ResumeLayout();
		}
		private void FillText() {
			this._commandCategoryHeader.Text = GApp.Strings.GetString("Form.OptionDialog._commandCategoryHeader");
			this._commandNameHeader.Text = GApp.Strings.GetString("Form.OptionDialog._commandNameHeader");
			this._commandConfigHeader.Text = GApp.Strings.GetString("Form.OptionDialog._commandConfigHeader");
			this._resetKeyConfigButton.Text = GApp.Strings.GetString("Form.OptionDialog._resetKeyConfigButton");
			this._clearKeyConfigButton.Text = GApp.Strings.GetString("Form.OptionDialog._clearKeyConfigButton");
			this._commandConfigGroup.Text = GApp.Strings.GetString("Form.OptionDialog._commandConfigGroup");
			this._commandNameLabel.Text = GApp.Strings.GetString("Form.OptionDialog._commandNameLabel");
			this._currentConfigLabel.Text = GApp.Strings.GetString("Form.OptionDialog._currentConfigLabel");
			this._newAllocationLabel.Text = GApp.Strings.GetString("Form.OptionDialog._newAllocationLabel");
			this._allocateKeyButton.Text = GApp.Strings.GetString("Form.OptionDialog._allocateKeyButton");
		}
		public override void InitUI(ContainerOptions options) {
			_commands = (Commands)options.Commands.Clone();
			InitKeyConfigUI();
		}
		public override bool Commit(ContainerOptions options) {
			options.Commands = _commands;
			return true;
		}

		private void InitKeyConfigUI() {
			_keyConfigList.Items.Clear();
			IEnumerator ie = _commands.EnumEntries();
			while(ie.MoveNext()) {
				Commands.Entry e = (Commands.Entry)ie.Current;
				if(e.Category==Commands.Category.Fixed) continue;
				ListViewItem li = new ListViewItem(EnumDescAttribute.For(typeof(Commands.Category)).GetDescription(e.Category));
				li = _keyConfigList.Items.Add(li);
				li.SubItems.Add(e.Description);
				li.SubItems.Add(e.KeyDisplayString);
				li.Tag = e.CID;
			}
		}
		private void OnKeyMapItemActivated(object sender, EventArgs args) {
			if(_keyConfigList.SelectedItems.Count==0) return;

			CID id = (CID)_keyConfigList.SelectedItems[0].Tag;
			Commands.Entry e = _commands.FindEntry(id);
			Debug.Assert(e!=null);
			_hotKey.Key = e.Modifiers|e.Key;

			_commandName.Text = String.Format("{0} - {1}", EnumDescAttribute.For(typeof(Commands.Category)).GetDescription(e.Category), e.Description);
			_currentCommand.Text = e.KeyDisplayString;
			_allocateKeyButton.Enabled = true;
		}
		private void OnAllocateKey(object sender, EventArgs args) {
			if(_keyConfigList.SelectedItems.Count==0) return;

			CID id = (CID)_keyConfigList.SelectedItems[0].Tag;
			Keys key = _hotKey.Key;
			int code = GUtil.KeyToControlCode(key);
			if(code!=-1) {
				if(GUtil.AskUserYesNo(this, String.Format(GApp.Strings.GetString("Message.OptionDialog.AskOverwriteToASCIIInput"), _hotKey.Text, code))==DialogResult.No)
					return;
			}

			Commands.Entry existing = _commands.FindEntry(key);
			if(existing!=null && existing.CID!=id) {
				if(GUtil.AskUserYesNo(this, String.Format(GApp.Strings.GetString("Message.OptionDialog.AskOverwriteCommand"), existing.Description))==DialogResult.No)
					return;

				existing.Key = Keys.None;
				existing.Modifiers = Keys.None;
				FindListViewItem(existing.CID).SubItems[2].Text = "";
			}

			//ê›íËÇèëÇ´ä∑Ç¶
			Commands.Entry e = _commands.FindEntry(id);
			Debug.Assert(e!=null);
			_commands.ModifyKey(e.CID, key & Keys.Modifiers, key & Keys.KeyCode);
			_keyConfigList.SelectedItems[0].SubItems[2].Text = e.KeyDisplayString;
		}

		private void OnResetKeyConfig(object sender, EventArgs args) {
			_commands.Init();
			InitKeyConfigUI();
		}
		private void OnClearKeyConfig(object sender, EventArgs args) {
			_commands.ClearKeyBinds();
			InitKeyConfigUI();
		}
		private ListViewItem FindListViewItem(CID id) {
			foreach(ListViewItem li in _keyConfigList.Items) {
				if(li.Tag.Equals(id)) return li;
			}
			return null;
		}
	}
}
