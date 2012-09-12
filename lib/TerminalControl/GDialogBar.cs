/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: GDialogBar.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using EnumDescAttributeT = Poderosa.Toolkit.EnumDescAttribute;

using Poderosa.UI;
using Poderosa.Toolkit;
using Poderosa.Terminal;
using Poderosa.Communication;
using Poderosa.ConnectionParam;
using Poderosa.Config;

namespace Poderosa.Forms
{
	/// <summary>
	/// GDialogBar の概要の説明です。
	/// 
	/// ツールバーは、標準的なアイコンは24pxピッチ、アイコン自体は16px。縦棒のエリアは8px
	/// </summary>
	internal class GDialogBar : System.Windows.Forms.UserControl
	{
		private GButton _newConnection;
		private GButton _newSerialConnection;
		private GButton _newCygwinConnection;
		private GButton _newSFUConnection;
		private GButton _openShortcut;
		private GButton _saveShortcut;
		private ToggleButton _singleStyle;
		private ToggleButton _divHorizontalStyle;
		private ToggleButton _divVerticalStyle;
		private ToggleButton _divHorizontal3Style;
		private ToggleButton _divVertical3Style;
		private System.Windows.Forms.Label _newLineLabel;
		private ComboBox _newLineOption;
		private ToggleButton _localEcho;
		private GButton _lineFeedRule;
		private ToggleButton _logSuspend;
		private GButton _commentLog;
		private GButton _serverInfo;
		private System.Windows.Forms.Label _encodingLabel;
		private ComboBox _encodingBox;
		private System.ComponentModel.IContainer components = null;
		private bool _toolTipInitialized;
		private ToolTip _toolTip;
		private bool _blockEventHandler;

		public GDialogBar()
		{
			// この呼び出しは、Windows.Forms フォーム デザイナで必要です。
			InitializeComponent();
			_newLineOption.BringToFront(); //日本語・英語で若干配置を変える都合で、これをトップにもってくる
			ReloadLanguage(GEnv.Options.Language);

			_toolTipInitialized = false;
			// TODO: InitForm を呼び出しの後に初期化処理を追加します。
			LoadImages();
		}
		public void ReloadLanguage(Language l) {
			this._encodingLabel.Text = GApp.Strings.GetString("Form.GDialogBar._encodingLabel");
			this._newLineLabel.Text = GApp.Strings.GetString("Form.GDialogBar._newLineLabel");
			this._newLineOption.Left = l==Language.Japanese? 340 : 356; //配置の都合
			InitToolTip();
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

		#region Component Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// this._newLineOption.Items.AddRange(EnumDescAttributeT.For(typeof(NewLine)).DescriptionCollection());
		/// 
		/// this._encodingBox.Items.AddRange(EnumDescAttributeT.For(typeof(EncodingType)).DescriptionCollection());
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GDialogBar));
			this._openShortcut = new GButton();
			this._newConnection = new GButton();
			this._newSerialConnection = new GButton();
			this._newCygwinConnection = new GButton();
			this._newSFUConnection = new GButton();
			this._saveShortcut = new GButton();
			this._singleStyle = new ToggleButton();
			this._divHorizontalStyle = new ToggleButton();
			this._divVerticalStyle = new ToggleButton();
			this._divHorizontal3Style = new ToggleButton();
			this._divVertical3Style = new ToggleButton();
			this._newLineLabel = new System.Windows.Forms.Label();
			this._newLineOption = new ComboBox();
			this._logSuspend = new ToggleButton();
			this._lineFeedRule = new GButton();
			this._localEcho = new ToggleButton();
			this._serverInfo = new GButton();
			this._commentLog = new GButton();
			this._encodingLabel = new System.Windows.Forms.Label();
			this._encodingBox = new ComboBox();
			this.SuspendLayout();
			// 
			// _newConnection
			// 
			this._newConnection.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._newConnection.ForeColor = System.Drawing.SystemColors.ControlText;
			this._newConnection.Location = new System.Drawing.Point(8, 2);
			this._newConnection.Name = "_newConnection";
			this._newConnection.Size = new System.Drawing.Size(24, 23);
			this._newConnection.TabIndex = 0;
			this._newConnection.TabStop = false;
			this._newConnection.Click += new System.EventHandler(this.OpenNewConnection);
			this._newConnection.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._newConnection.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._newConnection.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _newSerialConnection
			// 
			this._newSerialConnection.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._newSerialConnection.ForeColor = System.Drawing.SystemColors.ControlText;
			this._newSerialConnection.Location = new System.Drawing.Point(32, 2);
			this._newSerialConnection.Name = "_newSerialConnection";
			this._newSerialConnection.Size = new System.Drawing.Size(24, 23);
			this._newSerialConnection.TabIndex = 0;
			this._newSerialConnection.TabStop = false;
			this._newSerialConnection.Click += new System.EventHandler(this.OpenNewSerialConnection);
			this._newSerialConnection.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._newSerialConnection.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._newSerialConnection.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _newCygwinConnection
			// 
			this._newCygwinConnection.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._newCygwinConnection.ForeColor = System.Drawing.SystemColors.ControlText;
			this._newCygwinConnection.Location = new System.Drawing.Point(56, 2);
			this._newCygwinConnection.Name = "_newCygwinConnection";
			this._newCygwinConnection.Size = new System.Drawing.Size(24, 23);
			this._newCygwinConnection.TabIndex = 0;
			this._newCygwinConnection.TabStop = false;
			this._newCygwinConnection.Click += new System.EventHandler(this.OpenNewCygwinConnection);
			this._newCygwinConnection.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._newCygwinConnection.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._newCygwinConnection.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _newSFUConnection
			// 
			this._newSFUConnection.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._newSFUConnection.ForeColor = System.Drawing.SystemColors.ControlText;
			this._newSFUConnection.Location = new System.Drawing.Point(80, 2);
			this._newSFUConnection.Name = "_newSFUConnection";
			this._newSFUConnection.Size = new System.Drawing.Size(24, 23);
			this._newSFUConnection.TabIndex = 0;
			this._newSFUConnection.TabStop = false;
			this._newSFUConnection.Click += new System.EventHandler(this.OpenNewSFUConnection);
			this._newSFUConnection.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._newSFUConnection.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._newSFUConnection.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _openShortcut
			// 
			this._openShortcut.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._openShortcut.ForeColor = System.Drawing.SystemColors.ControlText;
			this._openShortcut.Location = new System.Drawing.Point(112, 2);
			this._openShortcut.Name = "_openShortcut";
			this._openShortcut.Size = new System.Drawing.Size(24, 23);
			this._openShortcut.TabIndex = 0;
			this._openShortcut.TabStop = false;
			this._openShortcut.Click += new System.EventHandler(this.OpenShortCut);
			this._openShortcut.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._openShortcut.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._openShortcut.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _saveShortcut
			// 
			this._saveShortcut.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._saveShortcut.Enabled = false;
			this._saveShortcut.Location = new System.Drawing.Point(136, 2);
			this._saveShortcut.Name = "_saveShortcut";
			this._saveShortcut.Size = new System.Drawing.Size(24, 23);
			this._saveShortcut.TabIndex = 0;
			this._saveShortcut.TabStop = false;
			this._saveShortcut.Click += new System.EventHandler(this.SaveShortCut);
			this._saveShortcut.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._saveShortcut.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._saveShortcut.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _singleStyle
			// 
			this._singleStyle.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._singleStyle.Checked = false;
			this._singleStyle.TabStop = false;
			this._singleStyle.AutoToggle = false;
			this._singleStyle.Location = new System.Drawing.Point(168, 2);
			this._singleStyle.Name = "_singleStyle";
			this._singleStyle.Size = new System.Drawing.Size(24, 23);
			this._singleStyle.TabIndex = 1;
			this._singleStyle.Click += new System.EventHandler(this.ToggleSingleStyle);
			this._singleStyle.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._singleStyle.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._singleStyle.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _divHorizontalStyle
			// 
			this._divHorizontalStyle.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._divHorizontalStyle.Checked = false;
			this._divHorizontalStyle.AutoToggle = false;
			this._divHorizontalStyle.Location = new System.Drawing.Point(192, 2);
			this._divHorizontalStyle.Name = "_divHorizontalStyle";
			this._divHorizontalStyle.Size = new System.Drawing.Size(24, 23);
			this._divHorizontalStyle.TabStop = false;
			this._divHorizontalStyle.TabIndex = 2;
			this._divHorizontalStyle.Click += new System.EventHandler(this.ToggleDivHorizontalStyle);
			this._divHorizontalStyle.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._divHorizontalStyle.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._divHorizontalStyle.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _divVerticalStyle
			// 
			this._divVerticalStyle.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._divVerticalStyle.Checked = false;
			this._divVerticalStyle.AutoToggle = false;
			this._divVerticalStyle.Location = new System.Drawing.Point(216, 2);
			this._divVerticalStyle.Name = "_divVerticalStyle";
			this._divVerticalStyle.Size = new System.Drawing.Size(24, 23);
			this._divVerticalStyle.TabStop = false;
			this._divVerticalStyle.Click += new System.EventHandler(this.ToggleDivVerticalStyle);
			this._divVerticalStyle.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._divVerticalStyle.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._divVerticalStyle.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _divHorizontal3Style
			// 
			this._divHorizontal3Style.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._divHorizontal3Style.Checked = false;
			this._divHorizontal3Style.AutoToggle = false;
			this._divHorizontal3Style.Location = new System.Drawing.Point(240, 2);
			this._divHorizontal3Style.Name = "_divHorizontal3Style";
			this._divHorizontal3Style.Size = new System.Drawing.Size(24, 23);
			this._divHorizontal3Style.TabStop = false;
			this._divHorizontal3Style.Click += new System.EventHandler(this.ToggleDivHorizontal3Style);
			this._divHorizontal3Style.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._divHorizontal3Style.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._divHorizontal3Style.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _divVertical3Style
			// 
			this._divVertical3Style.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._divVertical3Style.Checked = false;
			this._divVertical3Style.AutoToggle = false;
			this._divVertical3Style.Location = new System.Drawing.Point(264, 2);
			this._divVertical3Style.Name = "_divVertical3Style";
			this._divVertical3Style.Size = new System.Drawing.Size(24, 23);
			this._divVertical3Style.TabStop = false;
			this._divVertical3Style.Click += new System.EventHandler(this.ToggleDivVertical3Style);
			this._divVertical3Style.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._divVertical3Style.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._divVertical3Style.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _newLineLabel
			// 
			this._newLineLabel.Location = new System.Drawing.Point(296, 7);
			this._newLineLabel.Name = "_newLineLabel";
			this._newLineLabel.Size = new System.Drawing.Size(60, 15);
			this._newLineLabel.TabIndex = 0;
			this._newLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _newLineOption
			// 
			this._newLineOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._newLineOption.Enabled = false;
			this._newLineOption.Items.AddRange(EnumDescAttributeT.For(typeof(NewLine)).DescriptionCollection());
			this._newLineOption.Location = new System.Drawing.Point(340, 4);
			this._newLineOption.Name = "_newLineOption";
			this._newLineOption.Size = new System.Drawing.Size(72, 20);
			this._newLineOption.TabIndex = 0;
			this._newLineOption.TabStop = false;
			this._newLineOption.SelectedIndexChanged += new System.EventHandler(this.ChangeNewLine);
			// 
			// _encodingLabel
			// 
			this._encodingLabel.Location = new System.Drawing.Point(416, 7);
			this._encodingLabel.Name = "_encodingLabel";
			this._encodingLabel.Size = new System.Drawing.Size(80, 15);
			this._encodingLabel.TabIndex = 0;
			this._encodingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _encodingBox
			// 
			this._encodingBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._encodingBox.Enabled = false;
			this._encodingBox.Location = new System.Drawing.Point(496, 4);
			this._encodingBox.Name = "_encodingBox";
			this._encodingBox.Size = new System.Drawing.Size(96, 20);
			this._encodingBox.TabIndex = 0;
			this._encodingBox.TabStop = false;
			this._encodingBox.SelectedIndexChanged += new System.EventHandler(this.ChangeEncoding);
			this._encodingBox.Items.AddRange(EnumDescAttributeT.For(typeof(EncodingType)).DescriptionCollection());
			// 
			// _localEcho
			// 
			this._localEcho.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._localEcho.Checked = false;
			this._localEcho.Enabled = false;
			this._localEcho.Location = new System.Drawing.Point(600, 2);
			this._localEcho.Name = "_localEcho";
			this._localEcho.Size = new System.Drawing.Size(24, 23);
			this._localEcho.TabIndex = 0;
			this._localEcho.TabStop = false;
			this._localEcho.Click += new System.EventHandler(this.ToggleLocalEcho);
			this._localEcho.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._localEcho.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._localEcho.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _lineFeedRule
			// 
			this._lineFeedRule.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._lineFeedRule.Enabled = false;
			this._lineFeedRule.Location = new System.Drawing.Point(624, 2);
			this._lineFeedRule.Name = "_lineFeedRule";
			this._lineFeedRule.Size = new System.Drawing.Size(24, 23);
			this._lineFeedRule.TabIndex = 0;
			this._lineFeedRule.TabStop = false;
			this._lineFeedRule.Click += new System.EventHandler(this.LineFeedRule);
			this._lineFeedRule.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._lineFeedRule.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._lineFeedRule.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _logSuspend
			// 
			this._logSuspend.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._logSuspend.Checked = false;
			this._logSuspend.Enabled = false;
			this._logSuspend.Location = new System.Drawing.Point(650, 2);
			this._logSuspend.Name = "_logSuspend";
			this._logSuspend.Size = new System.Drawing.Size(24, 23);
			this._logSuspend.TabIndex = 0;
			this._logSuspend.TabStop = false;
			this._logSuspend.Click += new System.EventHandler(this.ToggleLogSwitch);
			this._logSuspend.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._logSuspend.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._logSuspend.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _commentLog
			// 
			this._commentLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._commentLog.Enabled = false;
			this._commentLog.Location = new System.Drawing.Point(672, 2);
			this._commentLog.Name = "_commentLog";
			this._commentLog.Size = new System.Drawing.Size(24, 23);
			this._commentLog.TabIndex = 0;
			this._commentLog.TabStop = false;
			this._commentLog.Click += new System.EventHandler(this.CommentLog);
			this._commentLog.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._commentLog.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._commentLog.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// _serverInfo
			// 
			this._serverInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._serverInfo.Enabled = false;
			this._serverInfo.Location = new System.Drawing.Point(704, 2);
			this._serverInfo.Name = "_serverInfo";
			this._serverInfo.Size = new System.Drawing.Size(24, 23);
			this._serverInfo.TabIndex = 0;
			this._serverInfo.TabStop = false;
			this._serverInfo.Click += new System.EventHandler(this.ShowServerInfo);
			this._serverInfo.MouseEnter += new System.EventHandler(this.OnMouseEnterToButton);
			this._serverInfo.MouseHover += new System.EventHandler(this.OnMouseHoverOnButton);
			this._serverInfo.MouseLeave += new System.EventHandler(this.OnMouseLeaveFromButton);
			// 
			// GDialogBar
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._newConnection,
																		  this._newSerialConnection,
																		  this._newCygwinConnection,
																		  this._newSFUConnection,
																		  this._openShortcut,
																		  this._saveShortcut,
																		  this._singleStyle,
																		  this._divHorizontalStyle,
																		  this._divVerticalStyle,
																		  this._divHorizontal3Style,
																		  this._divVertical3Style,
																		  this._newLineLabel,
																		  this._newLineOption,
																		  this._encodingLabel,
																		  this._encodingBox,
																		  this._lineFeedRule,
																		  this._commentLog,
																		  this._logSuspend,
																		  this._localEcho,
																		  this._serverInfo});
			this.Name = "GDialogBar";
			this.Size = new System.Drawing.Size(664, 24);
			this.TabStop = false;
			this.ResumeLayout(false);

		}
		#endregion

		private void LoadImages() {
			this._openShortcut.Image = IconList.LoadIcon(IconList.ICON_OPEN);
			this._newConnection.Image = IconList.LoadIcon(IconList.ICON_NEWCONNECTION);
			this._newSerialConnection.Image = IconList.LoadIcon(IconList.ICON_SERIAL);
			this._newCygwinConnection.Image = IconList.LoadIcon(IconList.ICON_CYGWIN);
			this._newSFUConnection.Image = IconList.LoadIcon(IconList.ICON_SFU);
			this._saveShortcut.Image = IconList.LoadIcon(IconList.ICON_SAVE);
			this._singleStyle.Image = IconList.LoadIcon(IconList.ICON_SINGLE);
			this._divHorizontalStyle.Image = IconList.LoadIcon(IconList.ICON_DIVHORIZONTAL);
			this._divVerticalStyle.Image = IconList.LoadIcon(IconList.ICON_DIVVERTICAL);
			this._divHorizontal3Style.Image = IconList.LoadIcon(IconList.ICON_DIVHORIZONTAL3);
			this._divVertical3Style.Image = IconList.LoadIcon(IconList.ICON_DIVVERTICAL3);
			this._localEcho.Image = IconList.LoadIcon(IconList.ICON_LOCALECHO);
			this._lineFeedRule.Image = IconList.LoadIcon(IconList.ICON_LINEFEED);
			this._logSuspend.Image = IconList.LoadIcon(IconList.ICON_SUSPENDLOG);
			this._commentLog.Image = IconList.LoadIcon(IconList.ICON_COMMENTLOG);
			this._serverInfo.Image = IconList.LoadIcon(IconList.ICON_INFO);
		}

		public ToggleButton SuspendLogButton {
			get {
				return _logSuspend;
			}
		}
		public ToggleButton LocalEchoButton {
			get {
				return _localEcho;
			}
		}
		public GButton LineFeedRuleButton {
			get {
				return _lineFeedRule;
			}
		}

		public ComboBox NewLineBox {
			get {
				return _newLineOption;
			}
		}

		protected override void OnGotFocus(EventArgs args) {
			base.OnGotFocus(args);
			//GApp.GlobalCommandTarget.SetFocusToActiveConnection();
			//Debug.WriteLine("DialogBar gotfocus");
		}

		protected override void OnPaint(PaintEventArgs arg) {
			base.OnPaint(arg);
			//上に区切り線を引く
			Graphics g = arg.Graphics;
			Pen p = new Pen(Color.FromKnownColor(KnownColor.WindowFrame));
			g.DrawLine(p, 0, 0, Width, 0);
			p = new Pen(Color.FromKnownColor(KnownColor.Window));
			g.DrawLine(p, 0, 1, Width, 1);

			//ツールバーの区切り目
			const int margin = 3;
			p = new Pen(Color.FromKnownColor(KnownColor.ControlDark));
			g.DrawLine(p, 108, margin, 108, this.Height-margin);
			g.DrawLine(p, 162, margin, 162, this.Height-margin);
			g.DrawLine(p, 292, margin, 292, this.Height-margin);
			g.DrawLine(p, 696, margin, 696, this.Height-margin);
		}

		private void OpenNewConnection(object sender, System.EventArgs e) {
			GApp.GlobalCommandTarget.NewConnectionWithDialog(null);
		}
		private void OpenNewSerialConnection(object sender, System.EventArgs e) {
			GApp.GlobalCommandTarget.NewSerialConnectionWithDialog(null);
		}
		private void OpenNewCygwinConnection(object sender, System.EventArgs e) {
			GApp.GlobalCommandTarget.NewCygwinConnectionWithDialog(null);
		}
		private void OpenNewSFUConnection(object sender, System.EventArgs e) {
			GApp.GlobalCommandTarget.NewSFUConnectionWithDialog(null);
		}

		private void OpenShortCut(object sender, System.EventArgs e) {
			GApp.GlobalCommandTarget.OpenShortCutWithDialog();
		}
		private void SaveShortCut(object sender, System.EventArgs e) {
			ContainerConnectionCommandTarget t = GApp.GetConnectionCommandTarget();
			t.SaveShortCut();
			t.Focus();
		}
		private void ChangeNewLine(object sender, System.EventArgs e) {
			if(_blockEventHandler) return;
			NewLine nl = (NewLine)_newLineOption.SelectedIndex;
			ContainerConnectionCommandTarget t = GApp.GetConnectionCommandTarget();
			t.SetTransmitNewLine(nl);
			t.Focus();
		}
		private void ChangeEncoding(object sender, System.EventArgs e) {
			if(_blockEventHandler) return;
			EncodingProfile enc = EncodingProfile.Get((EncodingType)_encodingBox.SelectedIndex);
			ContainerConnectionCommandTarget t = GApp.GetConnectionCommandTarget();
			t.SetEncoding(enc);
			t.Focus();
		}
		private void ToggleLocalEcho(object sender, System.EventArgs e) {
			if(_blockEventHandler) return;
			ContainerConnectionCommandTarget t = GApp.GetConnectionCommandTarget();
			t.SetLocalEcho(!t.Connection.Param.LocalEcho);
			t.Focus();
		}
		private void LineFeedRule(object sender, System.EventArgs e) {
			if(_blockEventHandler) return;
			ContainerConnectionCommandTarget t = GApp.GetConnectionCommandTarget();
			t.ShowLineFeedRuleDialog();
			t.Focus();
		}
		private void ToggleLogSwitch(object sender, System.EventArgs e) {
			if(_blockEventHandler) return;
			ContainerConnectionCommandTarget t = GApp.GetConnectionCommandTarget();
			t.SetLogSuspended(!t.Connection.LogSuspended);
			t.Focus();
		}
		private void ToggleSingleStyle(object sender, System.EventArgs e) {
			if(_blockEventHandler) return;
			if(GApp.GlobalCommandTarget.SetFrameStyle(GFrameStyle.Single)==CommandResult.Ignored)
				_singleStyle.Checked = true;
		}
		private void ToggleDivHorizontalStyle(object sender, System.EventArgs e) {
			if(_blockEventHandler) return;
			if(GApp.GlobalCommandTarget.SetFrameStyle(GFrameStyle.DivHorizontal)==CommandResult.Ignored)
				_divHorizontalStyle.Checked = true;
		}
		private void ToggleDivVerticalStyle(object sender, System.EventArgs e) {
			if(_blockEventHandler) return;
			if(GApp.GlobalCommandTarget.SetFrameStyle(GFrameStyle.DivVertical)==CommandResult.Ignored)
				_divVerticalStyle.Checked = true;
		}
		private void ToggleDivHorizontal3Style(object sender, System.EventArgs e) {
			if(_blockEventHandler) return;
			if(GApp.GlobalCommandTarget.SetFrameStyle(GFrameStyle.DivHorizontal3)==CommandResult.Ignored)
				_divHorizontal3Style.Checked = true;
		}
		private void ToggleDivVertical3Style(object sender, System.EventArgs e) {
			if(_blockEventHandler) return;
			if(GApp.GlobalCommandTarget.SetFrameStyle(GFrameStyle.DivVertical3)==CommandResult.Ignored)
				_divVertical3Style.Checked = true;
		}


		public void EnableTerminalUI(bool enabled, TerminalConnection con) {
			_blockEventHandler = true;
			_saveShortcut.Enabled = enabled;
			_newLineOption.Enabled = enabled && !con.IsClosed;
			_logSuspend.Enabled = enabled && !con.IsClosed && con.TextLogger.IsActive;
			_localEcho.Enabled = enabled && !con.IsClosed;
			_lineFeedRule.Enabled = enabled && !con.IsClosed;
			_encodingBox.Enabled = enabled && !con.IsClosed;
			_serverInfo.Enabled = enabled;
			_commentLog.Enabled = enabled && !con.IsClosed && con.TextLogger.IsActive;
			if(enabled) {
				_newLineOption.SelectedIndex = (int)con.Param.TransmitNL;
				_encodingBox.SelectedIndex = (int)con.Param.EncodingProfile.Type;
				_logSuspend.Checked = con.LogSuspended;
				_localEcho.Checked = con.Param.LocalEcho;
			}
			_blockEventHandler = false;
			Invalidate(true);
		}
		public void ApplyOptions(ContainerOptions opt) {
			GFrameStyle f = opt.FrameStyle;
			_singleStyle.Checked         = f==GFrameStyle.Single;
			_divHorizontalStyle.Checked  = f==GFrameStyle.DivHorizontal;
			_divVerticalStyle.Checked    = f==GFrameStyle.DivVertical;
			_divHorizontal3Style.Checked = f==GFrameStyle.DivHorizontal3;
			_divVertical3Style.Checked   = f==GFrameStyle.DivVertical3;
			Invalidate(true);
		}


		private void OnMouseEnterToButton(object sender, EventArgs args) {
			if(!_toolTipInitialized) InitToolTip();

			GStatusBar sb = GApp.Frame.StatusBar;
			if(sender==_openShortcut)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._openShortcut"));
			else if(sender==_newConnection)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._newConnection"));
			else if(sender==_newSerialConnection)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._newSerialConnection"));
			else if(sender==_newCygwinConnection)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._newCygwinConnection"));
			else if(sender==_newSFUConnection)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._newSFUConnection"));
			else if(sender==_saveShortcut)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._saveShortcut"));
			else if(sender==_singleStyle)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._singleStyle"));
			else if(sender==_divHorizontalStyle)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._divHorizontalStyle"));
			else if(sender==_divVerticalStyle)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._divVerticalStyle"));
			else if(sender==_divHorizontal3Style)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._divHorizontal3Style"));
			else if(sender==_divVertical3Style)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._divVertical3Style"));
			else if(sender==_newLineOption)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._newLineOption"));
			else if(sender==_lineFeedRule)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._lineFeedRule"));
			else if(sender==_encodingBox)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._encodingBox"));
			else if(sender==_logSuspend)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._logSuspend"));
			else if(sender==_commentLog)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._commentLog"));
			else if(sender==_localEcho)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._localEcho"));
			else if(sender==_serverInfo)
				sb.SetStatusBarText(GApp.Strings.GetString("Caption.ToolBar._serverInfo"));
			else
				Debug.WriteLine("Unexpected toolbar object");
		}

		private void InitToolTip() {
			if(_toolTip!=null) _toolTip.RemoveAll();
			ToolTip tt = new ToolTip();
			tt.SetToolTip(_openShortcut, GApp.Strings.GetString("ToolTip.ToolBar._openShortcut"));
			tt.SetToolTip(_saveShortcut, GApp.Strings.GetString("ToolTip.ToolBar._saveShortcut"));
			tt.SetToolTip(_singleStyle, GApp.Strings.GetString("ToolTip.ToolBar._singleStyle"));
			tt.SetToolTip(_divHorizontalStyle, GApp.Strings.GetString("ToolTip.ToolBar._divHorizontalStyle"));
			tt.SetToolTip(_divVerticalStyle, GApp.Strings.GetString("ToolTip.ToolBar._divVerticalStyle"));
			tt.SetToolTip(_divHorizontal3Style, GApp.Strings.GetString("ToolTip.ToolBar._divHorizontal3Style"));
			tt.SetToolTip(_divVertical3Style, GApp.Strings.GetString("ToolTip.ToolBar._divVertical3Style"));
			tt.SetToolTip(_newConnection, GApp.Strings.GetString("ToolTip.ToolBar._newConnection"));
			tt.SetToolTip(_newSerialConnection, GApp.Strings.GetString("ToolTip.ToolBar._newSerialConnection"));
			tt.SetToolTip(_newCygwinConnection, GApp.Strings.GetString("ToolTip.ToolBar._newCygwinConnection"));
			tt.SetToolTip(_newSFUConnection, GApp.Strings.GetString("ToolTip.ToolBar._newSFUConnection"));
			tt.SetToolTip(_newLineLabel, GApp.Strings.GetString("ToolTip.ToolBar._newLineLabel"));
			tt.SetToolTip(_newLineOption, GApp.Strings.GetString("ToolTip.ToolBar._newLineLabel"));
			tt.SetToolTip(_encodingLabel, GApp.Strings.GetString("ToolTip.ToolBar._encodingLabel"));
			tt.SetToolTip(_encodingBox, GApp.Strings.GetString("ToolTip.ToolBar._encodingBox"));
			tt.SetToolTip(_localEcho, GApp.Strings.GetString("ToolTip.ToolBar._localEcho"));
			tt.SetToolTip(_lineFeedRule, GApp.Strings.GetString("ToolTip.ToolBar._lineFeedRule"));
			tt.SetToolTip(_logSuspend, GApp.Strings.GetString("ToolTip.ToolBar._logSuspend"));
			tt.SetToolTip(_commentLog, GApp.Strings.GetString("ToolTip.ToolBar._commentLog"));
			tt.SetToolTip(_serverInfo, GApp.Strings.GetString("ToolTip.ToolBar._serverInfo"));

			_toolTip = tt;
			_toolTipInitialized = true;
		}

		private void OnMouseLeaveFromButton(object sender, EventArgs args) {
			GApp.Frame.StatusBar.ClearStatusBarText();
		}
		private void OnMouseHoverOnButton(object sender, EventArgs args) {
			
		}

		private void CommentLog(object sender, EventArgs e) {
			GApp.GetConnectionCommandTarget().CommentLog();
		}
		private void ShowServerInfo(object sender, EventArgs e) {
			GApp.GetConnectionCommandTarget().ShowServerInfo();
		}
	}
}
