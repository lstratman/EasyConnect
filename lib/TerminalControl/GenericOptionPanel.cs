/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: GenericOptionPanel.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Toolkit;
using Poderosa.Config;
using Poderosa.UI;

namespace Poderosa.Forms
{
	/// <summary>
	/// GenericOptionPanel ÇÃäTóvÇÃê‡ñæÇ≈Ç∑ÅB
	/// </summary>
	internal class GenericOptionPanel : OptionDialog.CategoryPanel
	{
		private System.Windows.Forms.Label _languageLabel;
		private ComboBox _languageBox;
		private System.Windows.Forms.Label _actionOnLaunchLabel;
		private ComboBox _actionOnLaunchBox;
		private System.Windows.Forms.Label _MRUSizeLabel;
		private TextBox _MRUSize;
		private System.Windows.Forms.Label _serialCountLabel;
		private TextBox _serialCount;
		private CheckBox _showToolBar;
		private CheckBox _showStatusBar;
		private CheckBox _showTabBar;
		private System.Windows.Forms.GroupBox _tabBarGroup;
		private System.Windows.Forms.Label _tabStyleLabel;
		private ComboBox _tabStyleBox;
		private CheckBox _splitterRatioBox;
		private CheckBox _askCloseOnExit;
		private CheckBox _quitAppWithLastPane;
		private System.Windows.Forms.Label _optionPreservePlaceLabel;
		private ComboBox _optionPreservePlace;
		private System.Windows.Forms.Label _optionPreservePlacePath;

		public GenericOptionPanel()
		{
			InitializeComponent();
			FillText();
		}
		private void InitializeComponent() {
			this._actionOnLaunchLabel = new System.Windows.Forms.Label();
			this._actionOnLaunchBox = new ComboBox();
			this._MRUSizeLabel = new System.Windows.Forms.Label();
			this._MRUSize = new TextBox();
			this._serialCountLabel = new System.Windows.Forms.Label();
			this._serialCount = new TextBox();
			this._tabBarGroup = new System.Windows.Forms.GroupBox();
			this._tabStyleLabel = new System.Windows.Forms.Label();
			this._tabStyleBox = new ComboBox();
			this._splitterRatioBox = new System.Windows.Forms.CheckBox();
			this._showToolBar = new System.Windows.Forms.CheckBox();
			this._showTabBar = new System.Windows.Forms.CheckBox();
			this._showStatusBar = new System.Windows.Forms.CheckBox();
			this._askCloseOnExit = new System.Windows.Forms.CheckBox();
			this._quitAppWithLastPane = new System.Windows.Forms.CheckBox();
			this._optionPreservePlaceLabel = new System.Windows.Forms.Label();
			this._optionPreservePlace = new ComboBox();
			this._optionPreservePlacePath = new Label();
			this._languageLabel = new Label();
			this._languageBox = new ComboBox();

			this._tabBarGroup.SuspendLayout();
		
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this._actionOnLaunchLabel,
																					   this._actionOnLaunchBox,
																					   this._MRUSizeLabel,
																					   this._MRUSize,
																					   this._serialCountLabel,
																					   this._serialCount,
																					   this._showToolBar,
																					   this._showTabBar,
																					   this._showStatusBar,
																					   this._tabBarGroup,
																					   this._splitterRatioBox,
																					   this._askCloseOnExit,
																					   this._quitAppWithLastPane,
																					   this._optionPreservePlaceLabel,
																					   this._optionPreservePlacePath,
																					   this._optionPreservePlace,
																					   this._languageLabel,
																					   this._languageBox});
			// 
			// _languageLabel
			// 
			this._languageLabel.Location = new System.Drawing.Point(16, 8);
			this._languageLabel.Name = "_languageLabel";
			this._languageLabel.Size = new System.Drawing.Size(168, 24);
			this._languageLabel.TabIndex = 0;
			this._languageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _languageBox
			// 
			this._languageBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._languageBox.Location = new System.Drawing.Point(208, 8);
			this._languageBox.Name = "_languageBox";
			this._languageBox.Size = new System.Drawing.Size(216, 20);
			this._languageBox.TabIndex = 1;
			// 
			// _actionOnLaunchLabel
			// 
			this._actionOnLaunchLabel.Location = new System.Drawing.Point(16, 32);
			this._actionOnLaunchLabel.Name = "_actionOnLaunchLabel";
			this._actionOnLaunchLabel.Size = new System.Drawing.Size(104, 23);
			this._actionOnLaunchLabel.TabIndex = 2;
			this._actionOnLaunchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _actionOnLaunchBox
			// 
			this._actionOnLaunchBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._actionOnLaunchBox.Location = new System.Drawing.Point(208, 32);
			this._actionOnLaunchBox.Name = "_actionOnLaunchBox";
			this._actionOnLaunchBox.Size = new System.Drawing.Size(216, 20);
			this._actionOnLaunchBox.TabIndex = 3;
			// 
			// _MRUSizeLabel
			// 
			this._MRUSizeLabel.Location = new System.Drawing.Point(16, 56);
			this._MRUSizeLabel.Name = "_MRUSizeLabel";
			this._MRUSizeLabel.Size = new System.Drawing.Size(272, 23);
			this._MRUSizeLabel.TabIndex = 4;
			this._MRUSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _MRUSize
			// 
			this._MRUSize.Location = new System.Drawing.Point(304, 56);
			this._MRUSize.MaxLength = 2;
			this._MRUSize.Name = "_MRUSize";
			this._MRUSize.Size = new System.Drawing.Size(120, 19);
			this._MRUSize.TabIndex = 5;
			this._MRUSize.Text = "";
			// 
			// _serialCountLabel
			// 
			this._serialCountLabel.Location = new System.Drawing.Point(16, 80);
			this._serialCountLabel.Name = "_serialCountLabel";
			this._serialCountLabel.Size = new System.Drawing.Size(272, 23);
			this._serialCountLabel.TabIndex = 6;
			this._serialCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _serialCount
			// 
			this._serialCount.Location = new System.Drawing.Point(304, 80);
			this._serialCount.MaxLength = 2;
			this._serialCount.Name = "_serialCount";
			this._serialCount.Size = new System.Drawing.Size(120, 19);
			this._serialCount.TabIndex = 7;
			this._serialCount.Text = "";
			// 
			// _showTabBar
			// 
			this._showTabBar.Location = new System.Drawing.Point(24, 107);
			this._showTabBar.Name = "_showTabBar";
			this._showTabBar.FlatStyle = FlatStyle.System;
			this._showTabBar.Size = new System.Drawing.Size(136, 23);
			this._showTabBar.TabIndex = 8;
			this._showTabBar.CheckedChanged += new EventHandler(OnShowTabBarCheckedChanged);
			// 
			// _tabBarGroup
			// 
			this._tabBarGroup.Controls.AddRange(new System.Windows.Forms.Control[] {																					
																					   this._tabStyleLabel,
																					   this._tabStyleBox});
			this._tabBarGroup.Location = new System.Drawing.Point(16, 112);
			this._tabBarGroup.Name = "_tabBarGroup";
			this._tabBarGroup.FlatStyle = FlatStyle.System;
			this._tabBarGroup.Size = new System.Drawing.Size(408, 40);
			this._tabBarGroup.TabIndex = 9;
			this._tabBarGroup.TabStop = false;
			// 
			// _tabStyleLabel
			// 
			this._tabStyleLabel.Location = new System.Drawing.Point(16, 14);
			this._tabStyleLabel.Name = "_tabStyleLabel";
			this._tabStyleLabel.Size = new System.Drawing.Size(104, 24);
			this._tabStyleLabel.TabIndex = 10;
			this._tabStyleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _tabStyleBox
			// 
			this._tabStyleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._tabStyleBox.Location = new System.Drawing.Point(192, 15);
			this._tabStyleBox.Name = "_tabStyleBox";
			this._tabStyleBox.Size = new System.Drawing.Size(196, 20);
			this._tabStyleBox.TabIndex = 11;
			// 
			// _splitterRatioBox
			// 
			this._splitterRatioBox.Location = new System.Drawing.Point(24, 160);
			this._splitterRatioBox.Name = "_splitterRatioBox";
			this._splitterRatioBox.FlatStyle = FlatStyle.System;
			this._splitterRatioBox.Size = new System.Drawing.Size(320, 20);
			this._splitterRatioBox.TabIndex = 12;
			// 
			// _showToolBar
			// 
			this._showToolBar.Location = new System.Drawing.Point(24, 184);
			this._showToolBar.Name = "_showToolBar";
			this._showToolBar.FlatStyle = FlatStyle.System;
			this._showToolBar.Size = new System.Drawing.Size(168, 23);
			this._showToolBar.TabIndex = 13;
			// 
			// _showStatusBar
			// 
			this._showStatusBar.Location = new System.Drawing.Point(224, 184);
			this._showStatusBar.Name = "_showStatusBar";
			this._showStatusBar.FlatStyle = FlatStyle.System;
			this._showStatusBar.Size = new System.Drawing.Size(168, 23);
			this._showStatusBar.TabIndex = 14;
			// 
			// _askCloseOnExit
			// 
			this._askCloseOnExit.Location = new System.Drawing.Point(24, 220);
			this._askCloseOnExit.Name = "_askCloseOnExit";
			this._askCloseOnExit.FlatStyle = FlatStyle.System;
			this._askCloseOnExit.Size = new System.Drawing.Size(296, 23);
			this._askCloseOnExit.TabIndex = 15;
			// 
			// _quitAppWithLastPane
			// 
			this._quitAppWithLastPane.Location = new System.Drawing.Point(24, 244);
			this._quitAppWithLastPane.Name = "_quitAppWithLastPane";
			this._quitAppWithLastPane.FlatStyle = FlatStyle.System;
			this._quitAppWithLastPane.Size = new System.Drawing.Size(296, 23);
			this._quitAppWithLastPane.TabIndex = 16;
			// 
			// _optionPreservePlaceLabel
			// 
			this._optionPreservePlaceLabel.Location = new System.Drawing.Point(16, 268);
			this._optionPreservePlaceLabel.Name = "_optionPreservePlaceLabel";
			this._optionPreservePlaceLabel.Size = new System.Drawing.Size(208, 24);
			this._optionPreservePlaceLabel.TabIndex = 17;
			this._optionPreservePlaceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _optionPreservePlaceBox
			// 
			this._optionPreservePlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._optionPreservePlace.Location = new System.Drawing.Point(224, 268);
			this._optionPreservePlace.Name = "_optionPreservePlaceBox";
			this._optionPreservePlace.Size = new System.Drawing.Size(200, 20);
			this._optionPreservePlace.TabIndex = 18;
			this._optionPreservePlace.SelectedIndexChanged += new System.EventHandler(this.OnOptionPreservePlaceChanged);
			// 
			// _optionPreservePlacePath
			// 
			this._optionPreservePlacePath.Location = new System.Drawing.Point(16, 292);
			this._optionPreservePlacePath.BorderStyle = BorderStyle.FixedSingle;
			this._optionPreservePlacePath.Name = "_optionPreservePlacePath";
			this._optionPreservePlacePath.Size = new System.Drawing.Size(408, 36);
			this._optionPreservePlacePath.TabIndex = 19;
			this._optionPreservePlacePath.TextAlign = System.Drawing.ContentAlignment.TopLeft;

			this.BackColor = ThemeUtil.TabPaneBackColor;
			this._tabBarGroup.ResumeLayout();
		}
		private void FillText() {
			this._actionOnLaunchLabel.Text = GApp.Strings.GetString("Form.OptionDialog._actionOnLaunchLabel");
			this._MRUSizeLabel.Text = GApp.Strings.GetString("Form.OptionDialog._MRUSizeLabel");
			this._serialCountLabel.Text = GApp.Strings.GetString("Form.OptionDialog._serialCountLabel");
			this._showTabBar.Text = GApp.Strings.GetString("Form.OptionDialog._showTabBar");
			this._tabStyleLabel.Text = GApp.Strings.GetString("Form.OptionDialog._tabStyleLabel");
			this._splitterRatioBox.Text = GApp.Strings.GetString("Form.OptionDialog._splitterRatioBox");
			this._showToolBar.Text = GApp.Strings.GetString("Form.OptionDialog._showToolBar");
			this._showStatusBar.Text = GApp.Strings.GetString("Form.OptionDialog._showStatusBar");
			this._askCloseOnExit.Text = GApp.Strings.GetString("Form.OptionDialog._askCloseOnExit");
			this._quitAppWithLastPane.Text = GApp.Strings.GetString("Form.OptionDialog._quitAppWithLastPane");
			this._optionPreservePlaceLabel.Text = GApp.Strings.GetString("Form.OptionDialog._optionPreservePlaceLabel");
			this._languageLabel.Text = GApp.Strings.GetString("Form.OptionDialog._languageLabel");

			_tabStyleBox.Items.AddRange(EnumDescAttribute.For(typeof(TabBarStyle)).DescriptionCollection());
			_optionPreservePlace.Items.AddRange(EnumDescAttribute.For(typeof(OptionPreservePlace)).DescriptionCollection());
			_languageBox.Items.AddRange(EnumDescAttribute.For(typeof(Language)).DescriptionCollection());
		}
		public override void InitUI(ContainerOptions options) {
			_MRUSize.Text = options.MRUSize.ToString();
			_serialCount.Text = options.SerialCount.ToString();
			_actionOnLaunchBox.Items.Add(GApp.Strings.GetString("Caption.OptionDialog.ActionOnLaunch.Nothing"));
			_actionOnLaunchBox.Items.Add(GApp.Strings.GetString("Caption.OptionDialog.ActionOnLaunch.NewConnection"));
			for(int i=0; i<GApp.MacroManager.ModuleCount; i++)
				_actionOnLaunchBox.Items.Add(GApp.Strings.GetString("Caption.OptionDialog.ActionOnLaunch.Macro")+GApp.MacroManager.GetModule(i).Title);
			_actionOnLaunchBox.SelectedIndex = ToActionOnLaunchIndex(options.ActionOnLaunch);
			_showToolBar.Checked = options.ShowToolBar;
			_showTabBar.Checked = options.ShowTabBar;
			_showStatusBar.Checked = options.ShowStatusBar;
			_splitterRatioBox.Checked = options.SplitterPreservesRatio;
			_tabStyleBox.SelectedIndex = (int)options.TabBarStyle;
			_askCloseOnExit.Checked = options.AskCloseOnExit;
			_quitAppWithLastPane.Checked = options.QuitAppWithLastPane;
			_optionPreservePlace.SelectedIndex = (int)options.OptionPreservePlace;
			_languageBox.SelectedIndex = (int)options.Language;
		}
		public override bool Commit(ContainerOptions options) {
			string itemname = null;
			bool successful = false;
			try {
				options.ActionOnLaunch = ToActionOnLaunchCID(_actionOnLaunchBox.SelectedIndex);
				itemname = GApp.Strings.GetString("Caption.OptionDialog.MRUCount");
				options.MRUSize = Int32.Parse(_MRUSize.Text);
				itemname = GApp.Strings.GetString("Caption.OptionDialog.SerialPortCount");
				options.SerialCount = Int32.Parse(_serialCount.Text);

				options.ShowTabBar = _showTabBar.Checked;
				options.ShowToolBar = _showToolBar.Checked;
				options.SplitterPreservesRatio = _splitterRatioBox.Checked;
				options.TabBarStyle = (TabBarStyle)_tabStyleBox.SelectedIndex;
				options.ShowStatusBar = _showStatusBar.Checked;
				options.AskCloseOnExit = _askCloseOnExit.Checked;
				options.QuitAppWithLastPane = _quitAppWithLastPane.Checked;
				if(GApp.Options.OptionPreservePlace!=(OptionPreservePlace)_optionPreservePlace.SelectedIndex && !GApp.IsRegistryWritable) {
					GUtil.Warning(this, GApp.Strings.GetString("Message.OptionDialog.RegistryWriteAuthWarning"));
					return false;
				}
				options.OptionPreservePlace = (OptionPreservePlace)_optionPreservePlace.SelectedIndex;
				options.Language = (Language)_languageBox.SelectedIndex;
				if(options.Language==Language.Japanese && GApp.Options.EnvLanguage==Language.English) {
					if(GUtil.AskUserYesNo(this, GApp.Strings.GetString("Message.OptionDialog.AskJapaneseFont"))==DialogResult.No)
						return false;
				}

				successful = true;
			}
			catch(FormatException) {
				GUtil.Warning(this, String.Format(GApp.Strings.GetString("Message.OptionDialog.InvalidItem"), itemname));
			}
			catch(InvalidOptionException ex) {
				GUtil.Warning(this, ex.Message);
			}

			return successful;
		}

		private void OnShowTabBarCheckedChanged(object sender, EventArgs args) {
			_tabStyleBox.Enabled = _showTabBar.Checked;
			_splitterRatioBox.Enabled = _showTabBar.Checked;
		}

		private void OnOptionPreservePlaceChanged(object sender, EventArgs e) {
			AdjustOptionFileLocation((OptionPreservePlace)_optionPreservePlace.SelectedIndex);
		}
		private void AdjustOptionFileLocation(OptionPreservePlace p) {
			_optionPreservePlacePath.Text = GApp.GetOptionDirectory(p);
		}

		private static int ToActionOnLaunchIndex(CID action) {
			if(action==CID.NOP)
				return 0;
			else if(action==CID.NewConnection)
				return 1;
			else //CID.ExecMacro
				return 2 + (int)(action - CID.ExecMacro);
		}
		private static CID ToActionOnLaunchCID(int n) {
			if(n==0)
				return CID.NOP;
			else if(n==1)
				return CID.NewConnection;
			else
				return CID.ExecMacro + (n-2);
		}
	}
}
