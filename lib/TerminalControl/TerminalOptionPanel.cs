/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TerminalOptionPanel.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;

using Poderosa.Toolkit;
using Poderosa.Config;
using Poderosa.ConnectionParam;
using Poderosa.UI;

namespace Poderosa.Forms
{
	internal class TerminalOptionPanel : OptionDialog.CategoryPanel {
		private System.Windows.Forms.Label _badCharLabel;
		private ComboBox _badCharBox;
		private System.Windows.Forms.Label _bufferSizeLabel;
		private TextBox _bufferSize;
		private Label _disconnectNotificationLabel;
		private ComboBox _disconnectNotification;
		private CheckBox _closeOnDisconnect;
		private CheckBox _beepOnBellChar;
		private CheckBox _adjustsTabTitleToWindowTitle;
		private CheckBox _allowsScrollInAppMode;
		private CheckBox _keepAliveCheck;
		private TextBox  _keepAliveIntervalBox;
		private Label    _keepAliveLabel;
		private System.Windows.Forms.GroupBox _defaultLogGroup;
		private CheckBox _autoLogCheckBox;
		private System.Windows.Forms.Label _defaultLogTypeLabel;
		private ComboBox _defaultLogTypeBox;
		private System.Windows.Forms.Label _defaultLogDirectoryLabel;
		private TextBox _defaultLogDirectory;
		private System.Windows.Forms.Button _dirSelect;

		public TerminalOptionPanel() {
			InitializeComponent();
			FillText();
		}
		private void InitializeComponent() {
			this._badCharLabel = new System.Windows.Forms.Label();
			this._badCharBox = new ComboBox();
			this._bufferSizeLabel = new System.Windows.Forms.Label();
			this._bufferSize = new TextBox();
			this._disconnectNotificationLabel = new System.Windows.Forms.Label();
			this._disconnectNotification = new ComboBox();
			this._closeOnDisconnect = new System.Windows.Forms.CheckBox();
			this._beepOnBellChar = new System.Windows.Forms.CheckBox();
			this._adjustsTabTitleToWindowTitle = new CheckBox();
			this._allowsScrollInAppMode = new CheckBox();
			this._keepAliveCheck = new CheckBox();
			this._keepAliveIntervalBox = new TextBox();
			this._keepAliveLabel = new Label();
			this._defaultLogGroup = new System.Windows.Forms.GroupBox();
			this._defaultLogTypeLabel = new System.Windows.Forms.Label();
			this._defaultLogTypeBox = new ComboBox();
			this._defaultLogDirectoryLabel = new System.Windows.Forms.Label();
			this._defaultLogDirectory = new TextBox();
			this._dirSelect = new System.Windows.Forms.Button();
			this._autoLogCheckBox = new System.Windows.Forms.CheckBox();

			this._defaultLogGroup.SuspendLayout();

			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._badCharLabel,
																		  this._badCharBox,
																		  this._bufferSizeLabel,
																		  this._bufferSize,
																		  this._disconnectNotificationLabel,
																		  this._disconnectNotification,
																		  this._closeOnDisconnect,
																		  this._beepOnBellChar,
																		  this._adjustsTabTitleToWindowTitle,
																		  this._allowsScrollInAppMode,
																		  this._autoLogCheckBox,
																		  this._keepAliveCheck,
																		  this._keepAliveIntervalBox,
																		  this._keepAliveLabel,
																		  this._defaultLogGroup});
			// 
			// _badCharLabel
			// 
			this._badCharLabel.Location = new System.Drawing.Point(24, 8);
			this._badCharLabel.Name = "_badCharLabel";
			this._badCharLabel.Size = new System.Drawing.Size(160, 23);
			this._badCharLabel.TabIndex = 0;
			this._badCharLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _badCharBox
			// 
			this._badCharBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._badCharBox.Location = new System.Drawing.Point(200, 8);
			this._badCharBox.Name = "_badCharBox";
			this._badCharBox.Size = new System.Drawing.Size(152, 20);
			this._badCharBox.TabIndex = 1;
			// 
			// _bufferSizeLabel
			// 
			this._bufferSizeLabel.Location = new System.Drawing.Point(24, 32);
			this._bufferSizeLabel.Name = "_bufferSizeLabel";
			this._bufferSizeLabel.Size = new System.Drawing.Size(96, 23);
			this._bufferSizeLabel.TabIndex = 2;
			this._bufferSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _bufferSize
			// 
			this._bufferSize.Location = new System.Drawing.Point(200, 32);
			this._bufferSize.Name = "_bufferSize";
			this._bufferSize.Size = new System.Drawing.Size(72, 19);
			this._bufferSize.TabIndex = 3;
			this._bufferSize.TextAlign = HorizontalAlignment.Left;
			this._bufferSize.MaxLength = 4;
			this._bufferSize.Text = "";
			// 
			// _disconnectNotificationLabel
			// 
			this._disconnectNotificationLabel.Location = new System.Drawing.Point(24, 56);
			this._disconnectNotificationLabel.Name = "_disconnectNotificationLabel";
			this._disconnectNotificationLabel.Size = new System.Drawing.Size(160, 23);
			this._disconnectNotificationLabel.TabIndex = 4;
			this._disconnectNotificationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _disconnectNotification
			// 
			this._disconnectNotification.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._disconnectNotification.Location = new System.Drawing.Point(200, 56);
			this._disconnectNotification.Name = "_disconnectNotification";
			this._disconnectNotification.Size = new System.Drawing.Size(152, 19);
			this._disconnectNotification.TabIndex = 5;
			// 
			// _closeOnDisconnect
			// 
			this._closeOnDisconnect.Location = new System.Drawing.Point(24, 80);
			this._closeOnDisconnect.Name = "_closeOnDisconnect";
			this._closeOnDisconnect.FlatStyle = FlatStyle.System;
			this._closeOnDisconnect.Size = new System.Drawing.Size(192, 20);
			this._closeOnDisconnect.TabIndex = 6;
			// 
			// _beepOnBellChar
			// 
			this._beepOnBellChar.Location = new System.Drawing.Point(24, 102);
			this._beepOnBellChar.Name = "_beepOnBellChar";
			this._beepOnBellChar.FlatStyle = FlatStyle.System;
			this._beepOnBellChar.Size = new System.Drawing.Size(288, 20);
			this._beepOnBellChar.TabIndex = 7;
			// 
			// _adjustsTabTitleToWindowTitle
			// 
			this._adjustsTabTitleToWindowTitle.Location = new System.Drawing.Point(24, 126);
			this._adjustsTabTitleToWindowTitle.Name = "_adjustsTabTitleToWindowTitle";
			this._adjustsTabTitleToWindowTitle.FlatStyle = FlatStyle.System;
			this._adjustsTabTitleToWindowTitle.Size = new System.Drawing.Size(336, 20);
			this._adjustsTabTitleToWindowTitle.TabIndex = 8;
			// 
			// _allowsScrollInAppMode
			// 
			this._allowsScrollInAppMode.Location = new System.Drawing.Point(24, 150);
			this._allowsScrollInAppMode.Name = "_allowsScrollInAppMode";
			this._allowsScrollInAppMode.FlatStyle = FlatStyle.System;
			this._allowsScrollInAppMode.Size = new System.Drawing.Size(288, 20);
			this._allowsScrollInAppMode.TabIndex = 9;
			// 
			// _keepAliveCheck
			// 
			this._keepAliveCheck.Location = new System.Drawing.Point(24, 176);
			this._keepAliveCheck.Name = "_keepAliveCheck";
			this._keepAliveCheck.FlatStyle = FlatStyle.System;
			this._keepAliveCheck.Size = new System.Drawing.Size(244, 20);
			this._keepAliveCheck.TabIndex = 10;
			this._keepAliveCheck.CheckedChanged += new EventHandler(OnKeepAliveCheckChanged);
			// 
			// _keepAliveIntervalBox
			// 
			this._keepAliveIntervalBox.Location = new System.Drawing.Point(276, 176);
			this._keepAliveIntervalBox.Name = "_keepAliveIntervalBox";
			this._keepAliveIntervalBox.Size = new System.Drawing.Size(40, 20);
			this._keepAliveIntervalBox.TabIndex = 11;
			this._keepAliveIntervalBox.MaxLength = 2;
			this._keepAliveIntervalBox.TextAlign = HorizontalAlignment.Right;
			// 
			// _keepAliveLabel
			// 
			this._keepAliveLabel.Location = new System.Drawing.Point(316, 176);
			this._keepAliveLabel.Name = "_keepAliveLabel";
			this._keepAliveLabel.Size = new System.Drawing.Size(50, 20);
			this._keepAliveLabel.TabIndex = 12;
			this._keepAliveLabel.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// _defaultLogGroup
			// 
			this._defaultLogGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this._defaultLogTypeLabel,
																						   this._defaultLogTypeBox,
																						   this._defaultLogDirectoryLabel,
																						   this._defaultLogDirectory,
																						   this._dirSelect});
			this._defaultLogGroup.Location = new System.Drawing.Point(16, 204);
			this._defaultLogGroup.Name = "_defaultLogGroup";
			this._defaultLogGroup.FlatStyle = FlatStyle.System;
			this._defaultLogGroup.Size = new System.Drawing.Size(392, 76);
			this._defaultLogGroup.TabIndex = 14;
			this._defaultLogGroup.TabStop = false;
			// 
			// _defaultLogTypeLabel
			// 
			this._defaultLogTypeLabel.Location = new System.Drawing.Point(8, 20);
			this._defaultLogTypeLabel.Name = "_defaultLogTypeLabel";
			this._defaultLogTypeLabel.Size = new System.Drawing.Size(96, 23);
			this._defaultLogTypeLabel.TabIndex = 15;
			this._defaultLogTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _defaultLogTypeBox
			// 
			this._defaultLogTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._defaultLogTypeBox.Location = new System.Drawing.Point(128, 20);
			this._defaultLogTypeBox.Name = "_defaultLogTypeBox";
			this._defaultLogTypeBox.Size = new System.Drawing.Size(104, 20);
			this._defaultLogTypeBox.TabIndex = 16;
			// 
			// _defaultLogDirectoryLabel
			// 
			this._defaultLogDirectoryLabel.Location = new System.Drawing.Point(8, 48);
			this._defaultLogDirectoryLabel.Name = "_defaultLogDirectoryLabel";
			this._defaultLogDirectoryLabel.Size = new System.Drawing.Size(112, 23);
			this._defaultLogDirectoryLabel.TabIndex = 17;
			this._defaultLogDirectoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _defaultLogDirectory
			// 
			this._defaultLogDirectory.Location = new System.Drawing.Point(128, 48);
			this._defaultLogDirectory.Name = "_defaultLogDirectory";
			this._defaultLogDirectory.Size = new System.Drawing.Size(176, 19);
			this._defaultLogDirectory.TabIndex = 18;
			this._defaultLogDirectory.Text = "";
			// 
			// _dirSelect
			// 
			this._dirSelect.Location = new System.Drawing.Point(312, 48);
			this._dirSelect.Name = "_dirSelect";
			this._dirSelect.FlatStyle = FlatStyle.System;
			this._dirSelect.Size = new System.Drawing.Size(19, 19);
			this._dirSelect.TabIndex = 19;
			this._dirSelect.Text = "...";
			this._dirSelect.Click += new System.EventHandler(this.OnSelectLogDirectory);
			// 
			// _autoLogCheckBox
			// 
			this._autoLogCheckBox.Location = new System.Drawing.Point(24, 200);
			this._autoLogCheckBox.Name = "_autoLogCheckBox";
			this._autoLogCheckBox.FlatStyle = FlatStyle.System;
			this._autoLogCheckBox.Size = new System.Drawing.Size(200, 24);
			this._autoLogCheckBox.TabIndex = 13;
			this._autoLogCheckBox.Checked = true;
			this._autoLogCheckBox.CheckedChanged += new System.EventHandler(this.OnAutoLogCheckBoxClick);

			this.BackColor = ThemeUtil.TabPaneBackColor;
			this._defaultLogGroup.ResumeLayout();

		}
		private void FillText() {
			this._badCharLabel.Text = GApp.Strings.GetString("Form.OptionDialog._badCharLabel");
			this._bufferSizeLabel.Text = GApp.Strings.GetString("Form.OptionDialog._bufferSizeLabel");
			this._disconnectNotificationLabel.Text = GApp.Strings.GetString("Form.OptionDialog._disconnectNotificationLabel");
			this._closeOnDisconnect.Text = GApp.Strings.GetString("Form.OptionDialog._closeOnDisconnect");
			this._beepOnBellChar.Text = GApp.Strings.GetString("Form.OptionDialog._beepOnBellChar");
			this._adjustsTabTitleToWindowTitle.Text = GApp.Strings.GetString("Form.OptionDialog._adjustsTabTitleToWindowTitle");
			this._allowsScrollInAppMode.Text = GApp.Strings.GetString("Form.OptionDialog._allowsScrollInAppMode");
			this._keepAliveCheck.Text = GApp.Strings.GetString("Form.OptionDialog._keepAliveCheck");
			this._keepAliveLabel.Text = GApp.Strings.GetString("Form.OptionDialog._keepAliveLabel");
			this._defaultLogTypeLabel.Text = GApp.Strings.GetString("Form.OptionDialog._defaultLogTypeLabel");
			this._defaultLogDirectoryLabel.Text = GApp.Strings.GetString("Form.OptionDialog._defaultLogDirectoryLabel");
			this._autoLogCheckBox.Text = GApp.Strings.GetString("Form.OptionDialog._autoLogCheckBox");

			_badCharBox.Items.AddRange(EnumDescAttribute.For(typeof(WarningOption)).DescriptionCollection());
			_disconnectNotification.Items.AddRange(EnumDescAttribute.For(typeof(DisconnectNotification)).DescriptionCollection());
			_defaultLogTypeBox.Items.AddRange(new object[] {
															   EnumDescAttribute.For(typeof(LogType)).GetDescription(LogType.Default),
															   EnumDescAttribute.For(typeof(LogType)).GetDescription(LogType.Binary),
															   EnumDescAttribute.For(typeof(LogType)).GetDescription(LogType.Xml)});
		}
		public override void InitUI(ContainerOptions options) {
			_bufferSize.Text = options.TerminalBufferSize.ToString();
			_closeOnDisconnect.Checked = options.CloseOnDisconnect;
			_disconnectNotification.SelectedIndex = (int)options.DisconnectNotification;
			_beepOnBellChar.Checked = options.BeepOnBellChar;
			_badCharBox.SelectedIndex = (int)options.WarningOption;
			_adjustsTabTitleToWindowTitle.Checked = options.AdjustsTabTitleToWindowTitle;
			_allowsScrollInAppMode.Checked = options.AllowsScrollInAppMode;
			_keepAliveCheck.Checked = options.KeepAliveInterval!=0;
			_keepAliveIntervalBox.Text = _keepAliveCheck.Checked? (options.KeepAliveInterval/60000).ToString() : "5";
			_autoLogCheckBox.Checked = options.DefaultLogType!=LogType.None;
			_defaultLogTypeBox.SelectedIndex = (int)options.DefaultLogType-1;
			_defaultLogDirectory.Text = options.DefaultLogDirectory;
		}
		public override bool Commit(ContainerOptions options) {
			bool successful = false;
			string itemname = null;
			try {
				options.CloseOnDisconnect = _closeOnDisconnect.Checked;
				options.BeepOnBellChar = _beepOnBellChar.Checked;
				options.AdjustsTabTitleToWindowTitle = _adjustsTabTitleToWindowTitle.Checked;
				options.AllowsScrollInAppMode = _allowsScrollInAppMode.Checked;
				itemname = GApp.Strings.GetString("Caption.OptionDialog.BufferLineCount");
				options.TerminalBufferSize = Int32.Parse(_bufferSize.Text);
				itemname = GApp.Strings.GetString("Caption.OptionDialog.MRUCount");
				options.WarningOption = (WarningOption)_badCharBox.SelectedIndex;
				options.DisconnectNotification = (DisconnectNotification)_disconnectNotification.SelectedIndex;
				if(_keepAliveCheck.Checked) {
					itemname = GApp.Strings.GetString("Caption.OptionDialog.KeepAliveInterval");
					options.KeepAliveInterval = Int32.Parse(_keepAliveIntervalBox.Text) * 60000;
					if(options.KeepAliveInterval<=0) throw new FormatException();
				}
				else
					options.KeepAliveInterval = 0;

				if(_autoLogCheckBox.Checked) {
					if(_defaultLogDirectory.Text.Length==0) {
						GUtil.Warning(this, GApp.Strings.GetString("Message.OptionDialog.EmptyLogDirectory"));
						return false;
					}
					options.DefaultLogType = (LogType)EnumDescAttribute.For(typeof(LogType)).FromDescription(_defaultLogTypeBox.Text, LogType.None);
					if(!System.IO.Directory.Exists(_defaultLogDirectory.Text)) {
						if(GUtil.AskUserYesNo(this, String.Format(GApp.Strings.GetString("Message.OptionDialog.AskCreateDirectory"), _defaultLogDirectory.Text))==DialogResult.Yes) 
							System.IO.Directory.CreateDirectory(_defaultLogDirectory.Text);
						else
							return false;
					}
					options.DefaultLogDirectory = _defaultLogDirectory.Text;
				}
				else
					options.DefaultLogType = LogType.None;
						
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

		private void OnSelectLogDirectory(object sender, EventArgs e) {
			/*
			 * よくみるとフォルダ選択UIは.NET1.1で追加されたようだ。CPはお勤めご苦労。
			CP.Windows.Forms.ShellFolderBrowser br = new CP.Windows.Forms.ShellFolderBrowser();
			br.Title = GApp.Strings.GetString("Caption.OptionDialog.DefaultLogDirectory");
			if(br.ShowDialog(this)) {
				_defaultLogDirectory.Text = br.FolderPath;
			}
			*/
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.ShowNewFolderButton = true;
			dlg.Description = GApp.Strings.GetString("Caption.OptionDialog.DefaultLogDirectory");
			if(_defaultLogDirectory.Text.Length>0 && Directory.Exists(_defaultLogDirectory.Text))
				dlg.SelectedPath = _defaultLogDirectory.Text;
			if(GCUtil.ShowModalDialog(FindForm(), dlg)==DialogResult.OK)
				_defaultLogDirectory.Text = dlg.SelectedPath;
		}
		private void OnAutoLogCheckBoxClick(object sender, EventArgs args) {
			bool e = _autoLogCheckBox.Checked;
			_defaultLogTypeBox.Enabled = e;
			if(_defaultLogTypeBox.SelectedIndex==-1) _defaultLogTypeBox.SelectedIndex = 0;
			_defaultLogDirectory.Enabled = e;
			_dirSelect.Enabled = e;
		}
		private void OnKeepAliveCheckChanged(object sender, EventArgs args) {
			_keepAliveIntervalBox.Enabled = _keepAliveCheck.Checked;
		}
	}
}
