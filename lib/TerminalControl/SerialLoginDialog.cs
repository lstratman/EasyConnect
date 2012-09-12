/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: SerialLoginDialog.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;

using Poderosa.Toolkit;
using EnumDescAttributeT = Poderosa.Toolkit.EnumDescAttribute;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Communication;
using Poderosa.Terminal;

namespace Poderosa.Forms {
	/// <summary>
	/// SerialLoginDialog の概要の説明です。
	/// </summary>
	internal class SerialLoginDialog : System.Windows.Forms.Form {
		
		private ConnectionTag _result;

		private System.Windows.Forms.Button _loginButton;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.GroupBox _terminalGroup;
		private ComboBox _logTypeBox;
		private System.Windows.Forms.Label _logTypeLabel;
		private ComboBox _newLineBox;
		private ComboBox _localEchoBox;
		private System.Windows.Forms.Label _localEchoLabel;
		private System.Windows.Forms.Label _newLineLabel;
		private ComboBox _logFileBox;
		private System.Windows.Forms.Label _logFileLabel;
		private ComboBox _encodingBox;
		private System.Windows.Forms.Label _encodingLabel;
		private Button _selectLogButton;
		private System.Windows.Forms.GroupBox _serialGroup;
		private ComboBox _flowControlBox;
		private System.Windows.Forms.Label _flowControlLabel;
		private ComboBox _stopBitsBox;
		private System.Windows.Forms.Label _stopBitsLabel;
		private ComboBox _parityBox;
		private System.Windows.Forms.Label _parityLabel;
		private ComboBox _dataBitsBox;
		private System.Windows.Forms.Label _dataBitsLabel;
		private ComboBox _baudRateBox;
		private System.Windows.Forms.Label _baudRateLabel;
		private ComboBox _portBox;
		private System.Windows.Forms.Label _portLabel;
		private Label _transmitDelayPerCharLabel;
		private TextBox _transmitDelayPerCharBox;
		private Label _transmitDelayPerLineLabel;
		private TextBox _transmitDelayPerLineBox;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SerialLoginDialog() {
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			this._serialGroup.Text = GApp.Strings.GetString("Form.SerialLoginDialog._serialGroup");
			//以下、SerialConfigとテキストを共用
			this._portLabel.Text = GApp.Strings.GetString("Form.SerialConfig._portLabel");
			this._baudRateLabel.Text = GApp.Strings.GetString("Form.SerialConfig._baudRateLabel");
			this._dataBitsLabel.Text = GApp.Strings.GetString("Form.SerialConfig._dataBitsLabel");
			this._parityLabel.Text = GApp.Strings.GetString("Form.SerialConfig._parityLabel");
			this._stopBitsLabel.Text = GApp.Strings.GetString("Form.SerialConfig._stopBitsLabel");
			this._flowControlLabel.Text = GApp.Strings.GetString("Form.SerialConfig._flowControlLabel");
			this._transmitDelayPerLineLabel.Text = "Transmit Delay(msec/line)";
			this._transmitDelayPerCharLabel.Text = "Transmit Delay(msec/char)";
			string bits = GApp.Strings.GetString("Caption.SerialConfig.Bits");
			this._dataBitsBox.Items.AddRange(new object[] {
															  String.Format("{0}{1}", 7, bits),
															  String.Format("{0}{1}", 8, bits)});

			this._terminalGroup.Text = GApp.Strings.GetString("Form.SerialLoginDialog._terminalGroup");
			
			//以下、LoginDialogとテキスト共用
			this._localEchoLabel.Text = GApp.Strings.GetString("Form.LoginDialog._localEchoLabel");
			this._newLineLabel.Text = GApp.Strings.GetString("Form.LoginDialog._newLineLabel");
			this._logFileLabel.Text = GApp.Strings.GetString("Form.LoginDialog._logFileLabel");
			this._encodingLabel.Text = GApp.Strings.GetString("Form.LoginDialog._encodingLabel");
			this._logTypeLabel.Text = GApp.Strings.GetString("Form.LoginDialog._logTypeLabel");
			this._localEchoBox.Items.AddRange(new object[] {
															   GApp.Strings.GetString("Common.DoNot"),
															   GApp.Strings.GetString("Common.Do")});
			this._loginButton.Text = GApp.Strings.GetString("Common.OK");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this.Text = GApp.Strings.GetString("Form.SerialLoginDialog.Text");

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			InitUI();
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// this._logTypeBox.Items.AddRange(EnumDescAttributeT.For(typeof(LogType)).DescriptionCollection());
		/// this._newLineBox.Items.AddRange(EnumDescAttributeT.For(typeof(NewLine)).DescriptionCollection());
		/// this._encodingBox.Items.AddRange(GUtil.EncodingDescription(GApp.Options.Encodings));
		/// this._stopBitsBox.Items.AddRange(EnumDescAttributeT.For(typeof(StopBits)).DescriptionCollection());
		/// this._parityBox.Items.AddRange(EnumDescAttributeT.For(typeof(Parity)).DescriptionCollection());
		///this._flowControlBox.Items.AddRange(EnumDescAttributeTs.For(typeof(FlowControl)).DescriptionCollection());
		///this._baudRateBox.Items.AddRange(TerminalUtil.BaudRates);
		/// </summary>
		private void InitializeComponent() {
			this._serialGroup = new System.Windows.Forms.GroupBox();
			this._portLabel = new System.Windows.Forms.Label();
			this._portBox = new ComboBox();
			this._baudRateLabel = new System.Windows.Forms.Label();
			this._baudRateBox = new ComboBox();
			this._dataBitsLabel = new System.Windows.Forms.Label();
			this._dataBitsBox = new ComboBox();
			this._parityLabel = new System.Windows.Forms.Label();
			this._parityBox = new ComboBox();
			this._stopBitsLabel = new System.Windows.Forms.Label();
			this._stopBitsBox = new ComboBox();
			this._flowControlLabel = new System.Windows.Forms.Label();
			this._flowControlBox = new ComboBox();
			this._transmitDelayPerCharLabel = new Label();
			this._transmitDelayPerCharBox = new TextBox();
			this._transmitDelayPerLineLabel = new Label();
			this._transmitDelayPerLineBox = new TextBox();
			
			this._terminalGroup = new System.Windows.Forms.GroupBox();
			this._logTypeBox = new ComboBox();
			this._logTypeLabel = new System.Windows.Forms.Label();
			this._newLineBox = new ComboBox();
			this._localEchoBox = new ComboBox();
			this._localEchoLabel = new System.Windows.Forms.Label();
			this._newLineLabel = new System.Windows.Forms.Label();
			this._logFileBox = new ComboBox();
			this._logFileLabel = new System.Windows.Forms.Label();
			this._encodingBox = new ComboBox();
			this._encodingLabel = new System.Windows.Forms.Label();
			this._selectLogButton = new Button();

			this._terminalGroup.SuspendLayout();
			this._serialGroup.SuspendLayout();
			this._loginButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _serialGroup
			// 
			this._serialGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this._transmitDelayPerCharBox,
																					   this._transmitDelayPerCharLabel,
																					   this._transmitDelayPerLineBox,
																					   this._transmitDelayPerLineLabel,
																					   this._flowControlBox,
																					   this._flowControlLabel,
																					   this._stopBitsBox,
																					   this._stopBitsLabel,
																					   this._parityBox,
																					   this._parityLabel,
																					   this._dataBitsBox,
																					   this._dataBitsLabel,
																					   this._baudRateBox,
																					   this._baudRateLabel,
																					   this._portBox,
																					   this._portLabel});
			this._serialGroup.Location = new System.Drawing.Point(8, 8);
			this._serialGroup.Name = "_serialGroup";
			this._serialGroup.FlatStyle = FlatStyle.System;
			this._serialGroup.Size = new System.Drawing.Size(296, 224);
			this._serialGroup.TabIndex = 0;
			this._serialGroup.TabStop = false;
			// 
			// _portLabel
			// 
			this._portLabel.Location = new System.Drawing.Point(8, 16);
			this._portLabel.Name = "_portLabel";
			this._portLabel.Size = new System.Drawing.Size(88, 23);
			this._portLabel.TabIndex = 1;
			this._portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _portBox
			// 
			this._portBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._portBox.Location = new System.Drawing.Point(112, 16);
			this._portBox.Name = "_portBox";
			this._portBox.Size = new System.Drawing.Size(120, 20);
			this._portBox.TabIndex = 2;
			// 
			// _baudRateLabel
			// 
			this._baudRateLabel.Location = new System.Drawing.Point(8, 40);
			this._baudRateLabel.Name = "_baudRateLabel";
			this._baudRateLabel.Size = new System.Drawing.Size(88, 23);
			this._baudRateLabel.TabIndex = 3;
			this._baudRateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _baudRateBox
			// 
			this._baudRateBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._baudRateBox.Items.AddRange(TerminalUtil.BaudRates);
			this._baudRateBox.Location = new System.Drawing.Point(112, 40);
			this._baudRateBox.Name = "_baudRateBox";
			this._baudRateBox.Size = new System.Drawing.Size(120, 20);
			this._baudRateBox.TabIndex = 4;
			// 
			// _dataBitsLabel
			// 
			this._dataBitsLabel.Location = new System.Drawing.Point(8, 64);
			this._dataBitsLabel.Name = "_dataBitsLabel";
			this._dataBitsLabel.Size = new System.Drawing.Size(88, 23);
			this._dataBitsLabel.TabIndex = 5;
			this._dataBitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _dataBitsBox
			// 
			this._dataBitsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._dataBitsBox.Location = new System.Drawing.Point(112, 64);
			this._dataBitsBox.Name = "_dataBitsBox";
			this._dataBitsBox.Size = new System.Drawing.Size(120, 20);
			this._dataBitsBox.TabIndex = 6;
			// 
			// _parityLabel
			// 
			this._parityLabel.Location = new System.Drawing.Point(8, 88);
			this._parityLabel.Name = "_parityLabel";
			this._parityLabel.Size = new System.Drawing.Size(88, 23);
			this._parityLabel.TabIndex = 7;
			this._parityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _parityBox
			// 
			this._parityBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._parityBox.Items.AddRange(EnumDescAttributeT.For(typeof(Parity)).DescriptionCollection());
			this._parityBox.Location = new System.Drawing.Point(112, 88);
			this._parityBox.Name = "_parityBox";
			this._parityBox.Size = new System.Drawing.Size(120, 20);
			this._parityBox.TabIndex = 8;
			// 
			// _stopBitsLabel
			// 
			this._stopBitsLabel.Location = new System.Drawing.Point(8, 112);
			this._stopBitsLabel.Name = "_stopBitsLabel";
			this._stopBitsLabel.Size = new System.Drawing.Size(88, 23);
			this._stopBitsLabel.TabIndex = 9;
			this._stopBitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _stopBitsBox
			// 
			this._stopBitsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._stopBitsBox.Items.AddRange(EnumDescAttributeT.For(typeof(StopBits)).DescriptionCollection());
			this._stopBitsBox.Location = new System.Drawing.Point(112, 112);
			this._stopBitsBox.Name = "_stopBitsBox";
			this._stopBitsBox.Size = new System.Drawing.Size(120, 20);
			this._stopBitsBox.TabIndex = 10;
			// 
			// _flowControlLabel
			// 
			this._flowControlLabel.Location = new System.Drawing.Point(8, 136);
			this._flowControlLabel.Name = "_flowControlLabel";
			this._flowControlLabel.Size = new System.Drawing.Size(88, 23);
			this._flowControlLabel.TabIndex = 11;
			this._flowControlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _flowControlBox
			// 
			this._flowControlBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._flowControlBox.Location = new System.Drawing.Point(112, 136);
			this._flowControlBox.Name = "_flowControlBox";
			this._flowControlBox.Size = new System.Drawing.Size(120, 20);
			this._flowControlBox.Items.AddRange(EnumDescAttributeT.For(typeof(FlowControl)).DescriptionCollection());
			this._flowControlBox.TabIndex = 12;
			// 
			// _transmitDelayPerCharLabel
			// 
			this._transmitDelayPerCharLabel.Location = new System.Drawing.Point(8, 160);
			this._transmitDelayPerCharLabel.Name = "_transmitDelayPerCharLabel";
			this._transmitDelayPerCharLabel.Size = new System.Drawing.Size(88, 23);
			this._transmitDelayPerCharLabel.TabIndex = 13;
			this._transmitDelayPerCharLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _transmitDelayPerCharBox
			// 
			this._transmitDelayPerCharBox.Location = new System.Drawing.Point(112, 160);
			this._transmitDelayPerCharBox.Name = "_transmitDelayPerCharBox";
			this._transmitDelayPerCharBox.Size = new System.Drawing.Size(120, 20);
			this._transmitDelayPerCharBox.TabIndex = 14;
			this._transmitDelayPerCharBox.MaxLength = 3;
			// 
			// _transmitDelayPerLineLabel
			// 
			this._transmitDelayPerLineLabel.Location = new System.Drawing.Point(8, 184);
			this._transmitDelayPerLineLabel.Name = "_transmitDelayPerLineLabel";
			this._transmitDelayPerLineLabel.Size = new System.Drawing.Size(88, 23);
			this._transmitDelayPerLineLabel.TabIndex = 15;
			this._transmitDelayPerLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _transmitDelayPerLineBox
			// 
			this._transmitDelayPerLineBox.Location = new System.Drawing.Point(112, 184);
			this._transmitDelayPerLineBox.Name = "_transmitDelayPerLineBox";
			this._transmitDelayPerLineBox.Size = new System.Drawing.Size(120, 20);
			this._transmitDelayPerLineBox.TabIndex = 16;
			this._transmitDelayPerLineBox.MaxLength = 3;
			// 
			// _terminalGroup
			// 
			this._terminalGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this._logTypeBox,
																						 this._logTypeLabel,
																						 this._newLineBox,
																						 this._localEchoBox,
																						 this._localEchoLabel,
																						 this._newLineLabel,
																						 this._logFileBox,
																						 this._logFileLabel,
																						 this._encodingBox,
																						 this._encodingLabel,
																						 this._selectLogButton});
			this._terminalGroup.Location = new System.Drawing.Point(8, 240);
			this._terminalGroup.Name = "_terminalGroup";
			this._terminalGroup.FlatStyle = FlatStyle.System;
			this._terminalGroup.Size = new System.Drawing.Size(296, 144);
			this._terminalGroup.TabIndex = 17;
			this._terminalGroup.TabStop = false;
			// 
			// _logTypeLabel
			// 
			this._logTypeLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._logTypeLabel.Location = new System.Drawing.Point(8, 16);
			this._logTypeLabel.Name = "_logTypeLabel";
			this._logTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._logTypeLabel.Size = new System.Drawing.Size(120, 16);
			this._logTypeLabel.TabIndex = 18;
			this._logTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _logTypeBox
			// 
			this._logTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._logTypeBox.Items.AddRange(EnumDescAttributeT.For(typeof(LogType)).DescriptionCollection());
			this._logTypeBox.Location = new System.Drawing.Point(112, 16);
			this._logTypeBox.Name = "_logTypeBox";
			this._logTypeBox.Size = new System.Drawing.Size(120, 20);
			this._logTypeBox.TabIndex = 19;
			this._logTypeBox.SelectedIndexChanged += new System.EventHandler(this.OnLogTypeChanged);
			// 
			// _logFileLabel
			// 
			this._logFileLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._logFileLabel.Location = new System.Drawing.Point(8, 40);
			this._logFileLabel.Name = "_logFileLabel";
			this._logFileLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._logFileLabel.Size = new System.Drawing.Size(88, 16);
			this._logFileLabel.TabIndex = 20;
			this._logFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _logFileBox
			// 
			this._logFileBox.Location = new System.Drawing.Point(112, 40);
			this._logFileBox.Name = "_logFileBox";
			this._logFileBox.Size = new System.Drawing.Size(144, 20);
			this._logFileBox.TabIndex = 21;
			// 
			// _selectLogButton
			// 
			this._selectLogButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._selectLogButton.ImageIndex = 0;
			this._selectLogButton.FlatStyle = FlatStyle.System;
			this._selectLogButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._selectLogButton.Location = new System.Drawing.Point(256, 40);
			this._selectLogButton.Name = "_selectLogButton";
			this._selectLogButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._selectLogButton.Size = new System.Drawing.Size(19, 19);
			this._selectLogButton.TabIndex = 22;
			this._selectLogButton.Text = "...";
			this._selectLogButton.Click += new System.EventHandler(this.SelectLog);
			// 
			// _encodingLabel
			// 
			this._encodingLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._encodingLabel.Location = new System.Drawing.Point(8, 64);
			this._encodingLabel.Name = "_encodingLabel";
			this._encodingLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._encodingLabel.Size = new System.Drawing.Size(96, 16);
			this._encodingLabel.TabIndex = 23;
			this._encodingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _encodingBox
			// 
			this._encodingBox.Items.AddRange(EnumDescAttributeT.For(typeof(EncodingType)).DescriptionCollection());
			this._encodingBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._encodingBox.Location = new System.Drawing.Point(112, 64);
			this._encodingBox.Name = "_encodingBox";
			this._encodingBox.Size = new System.Drawing.Size(120, 20);
			this._encodingBox.TabIndex = 24;
			// 
			// _localEchoLabel
			// 
			this._localEchoLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._localEchoLabel.Location = new System.Drawing.Point(8, 88);
			this._localEchoLabel.Name = "_localEchoLabel";
			this._localEchoLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._localEchoLabel.Size = new System.Drawing.Size(96, 16);
			this._localEchoLabel.TabIndex = 25;
			this._localEchoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _localEchoBox
			// 
			this._localEchoBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._localEchoBox.Location = new System.Drawing.Point(112, 88);
			this._localEchoBox.Name = "_localEchoBox";
			this._localEchoBox.Size = new System.Drawing.Size(120, 20);
			this._localEchoBox.TabIndex = 26;
			// 
			// _newLineLabel
			// 
			this._newLineLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._newLineLabel.Location = new System.Drawing.Point(8, 112);
			this._newLineLabel.Name = "_newLineLabel";
			this._newLineLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._newLineLabel.Size = new System.Drawing.Size(96, 16);
			this._newLineLabel.TabIndex = 27;
			this._newLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _newLineBox
			// 
			this._newLineBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._newLineBox.Items.AddRange(EnumDescAttributeT.For(typeof(NewLine)).DescriptionCollection());
			this._newLineBox.Location = new System.Drawing.Point(112, 112);
			this._newLineBox.Name = "_newLineBox";
			this._newLineBox.Size = new System.Drawing.Size(120, 20);
			this._newLineBox.TabIndex = 28;
			// 
			// _loginButton
			// 
			this._loginButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._loginButton.Location = new System.Drawing.Point(136, 392);
			this._loginButton.Name = "_loginButton";
			this._loginButton.FlatStyle = FlatStyle.System;
			this._loginButton.TabIndex = 29;
			this._loginButton.Click += new System.EventHandler(this.OnOK);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(224, 392);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.TabIndex = 30;
			// 
			// SerialLoginDialog
			// 
			this.AcceptButton = this._loginButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(314, 423);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._serialGroup,
																		  this._terminalGroup,
																		  this._cancelButton,
																		  this._loginButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SerialLoginDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this._terminalGroup.ResumeLayout(false);
			this._serialGroup.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public ConnectionTag Result {
			get {
				return _result;
			}
		}

		private void InitUI() {
			for(int i=1; i<=GApp.Options.SerialCount; i++)
				_portBox.Items.Add(String.Format("COM{0}", i));
			
			StringCollection c = GApp.ConnectionHistory.LogPaths;
			foreach(string p in c) _logFileBox.Items.Add(p);

			if(GApp.Options.DefaultLogType!=LogType.None) {
				_logTypeBox.SelectedIndex = (int)GApp.Options.DefaultLogType;
				string t = GUtil.CreateLogFileName(null);
				_logFileBox.Items.Add(t);
				_logFileBox.Text = t;
			}
			else
				_logTypeBox.SelectedIndex = 0;

			AdjustUI();
		}
		private void AdjustUI() {
			bool e = _logTypeBox.SelectedIndex!=(int)LogType.None;
			_logFileBox.Enabled = e;
			_selectLogButton.Enabled = e;
		}

		public void ApplyParam(SerialTerminalParam param) {
			_portBox.SelectedIndex = param.Port-1;
			//これらのSelectedIndexの設定はコンボボックスに設定した項目順に依存しているので注意深くすること
			_baudRateBox.SelectedIndex = _baudRateBox.FindStringExact(param.BaudRate.ToString());
			_dataBitsBox.SelectedIndex = param.ByteSize==7? 0 : 1;
			_parityBox.SelectedIndex = (int)param.Parity;
			_stopBitsBox.SelectedIndex = (int)param.StopBits;
			_flowControlBox.SelectedIndex = (int)param.FlowControl;

			_encodingBox.SelectedIndex = (int)param.EncodingProfile.Type;
			_newLineBox.SelectedIndex = _newLineBox.FindStringExact(param.TransmitNL.ToString());
			_localEchoBox.SelectedIndex = param.LocalEcho? 1 : 0;
			
			_transmitDelayPerCharBox.Text = param.TransmitDelayPerChar.ToString();
			_transmitDelayPerLineBox.Text = param.TransmitDelayPerLine.ToString();

		}

		private void OnOK(object sender, EventArgs args) {
			_result = null;
			this.DialogResult = DialogResult.None;

			SerialTerminalParam param = ValidateParam();
			if(param==null) return;

			try {
				_result = CommunicationUtil.CreateNewSerialConnection(this, param);
				if(_result!=null)
					this.DialogResult = DialogResult.OK;
			}
			catch(Exception ex) {
				GUtil.Warning(this, ex.Message);
			}

		}


		private SerialTerminalParam ValidateParam() {
			SerialTerminalParam p = new SerialTerminalParam();
			try {
				p.LogType = (LogType)EnumDescAttributeT.For(typeof(LogType)).FromDescription(_logTypeBox.Text, LogType.None);
				if(p.LogType!=LogType.None) {
					p.LogPath = _logFileBox.Text;
					if(p.LogPath==GUtil.CreateLogFileName(null)) p.LogPath = GUtil.CreateLogFileName(String.Format("com{0}", _portBox.SelectedIndex+1));
					LogFileCheckResult r = GCUtil.CheckLogFileName(p.LogPath, this);
					if(r==LogFileCheckResult.Cancel || r==LogFileCheckResult.Error) return null;
					p.LogAppend = (r==LogFileCheckResult.Append);
				}

				p.Port = _portBox.SelectedIndex+1;
				p.BaudRate = Int32.Parse(_baudRateBox.Text);
				p.ByteSize = (byte)(_dataBitsBox.SelectedIndex==0? 7 : 8);
				p.StopBits = (StopBits)_stopBitsBox.SelectedIndex;
				p.Parity = (Parity)_parityBox.SelectedIndex;
				p.FlowControl = (FlowControl)_flowControlBox.SelectedIndex;

				p.EncodingProfile = EncodingProfile.Get((EncodingType)_encodingBox.SelectedIndex);

				p.LocalEcho = _localEchoBox.SelectedIndex==1;
				p.TransmitNL = (NewLine)EnumDescAttributeT.For(typeof(NewLine)).FromDescription(_newLineBox.Text, LogType.None);

				p.TransmitDelayPerChar = Int32.Parse(_transmitDelayPerCharBox.Text);
				p.TransmitDelayPerLine = Int32.Parse(_transmitDelayPerLineBox.Text);
				return p;
			}
			catch(Exception ex) {
				GUtil.Warning(this, ex.Message);
				return null;
			}

		}
		private void SelectLog(object sender, System.EventArgs e) {
			string fn = GCUtil.SelectLogFileByDialog(this);
			if(fn!=null) _logFileBox.Text = fn;
		}
		private void OnLogTypeChanged(object sender, System.EventArgs args) {
			AdjustUI();
		}
	}

}
