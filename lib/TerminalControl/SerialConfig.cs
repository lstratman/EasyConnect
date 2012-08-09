/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: SerialConfig.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Poderosa.Toolkit;
using EnumDescAttributeT = Poderosa.Toolkit.EnumDescAttribute;
using Poderosa.Communication;
using Poderosa.Terminal;
using Poderosa.ConnectionParam;

namespace Poderosa.Forms
{
	/// <summary>
	/// 接続後にシリアルのパラメータを変更するためのUI
	/// ログインダイアログとかぶっている処理は多いので何とかしたいところではあるが...
	/// </summary>
	internal class SerialConfigForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		private System.Windows.Forms.Button _loginButton;
		private System.Windows.Forms.Button _cancelButton;
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
		private Label _portBox; //ポートは変更できない
		private System.Windows.Forms.Label _portLabel;
		private Label _transmitDelayPerCharLabel;
		private TextBox _transmitDelayPerCharBox;
		private Label _transmitDelayPerLineLabel;
		private TextBox _transmitDelayPerLineBox;


		public SerialConfigForm()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			this._portLabel.Text = GApp.Strings.GetString("Form.SerialConfig._portLabel");
			this._baudRateLabel.Text = GApp.Strings.GetString("Form.SerialConfig._baudRateLabel");
			this._dataBitsLabel.Text = GApp.Strings.GetString("Form.SerialConfig._dataBitsLabel");
			this._parityLabel.Text = GApp.Strings.GetString("Form.SerialConfig._parityLabel");
			this._stopBitsLabel.Text = GApp.Strings.GetString("Form.SerialConfig._stopBitsLabel");
			this._flowControlLabel.Text = GApp.Strings.GetString("Form.SerialConfig._flowControlLabel");
			string bits = GApp.Strings.GetString("Caption.SerialConfig.Bits");
			this._dataBitsBox.Items.AddRange(new object[] {
															  String.Format("{0}{1}", 7, bits),
															  String.Format("{0}{1}", 8, bits)});
			this.Text = GApp.Strings.GetString("Form.SerialConfig.Text");
			this._loginButton.Text = GApp.Strings.GetString("Common.OK");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");

			this._transmitDelayPerLineLabel.Text = "Transmit Delay(msec/line)";
			this._transmitDelayPerCharLabel.Text = "Transmit Delay(msec/char)";
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


		//			this._flowControlBox.Items.AddRange(EnumDescAttributeT.For(typeof(FlowControl)).DescriptionCollection());
		//			this._stopBitsBox.Items.AddRange(EnumDescAttributeT.For(typeof(StopBits)).DescriptionCollection());
		//			this._parityBox.Items.AddRange(EnumDescAttributeT.For(typeof(Parity)).DescriptionCollection());
		//			this._baudRateBox.Items.AddRange(TerminalUtil.BaudRates);


		#region Windows Form Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this._portLabel = new System.Windows.Forms.Label();
			this._portBox = new System.Windows.Forms.Label();
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
			this._loginButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _portLabel
			// 
			this._portLabel.Location = new System.Drawing.Point(8, 16);
			this._portLabel.Name = "_portLabel";
			this._portLabel.Size = new System.Drawing.Size(88, 23);
			this._portLabel.TabIndex = 0;
			this._portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _portBox
			// 
			this._portBox.Location = new System.Drawing.Point(112, 20);
			this._portBox.Name = "_portBox";
			this._portBox.Size = new System.Drawing.Size(96, 20);
			this._portBox.TabIndex = 1;
			// 
			// _baudRateLabel
			// 
			this._baudRateLabel.Location = new System.Drawing.Point(8, 40);
			this._baudRateLabel.Name = "_baudRateLabel";
			this._baudRateLabel.Size = new System.Drawing.Size(88, 23);
			this._baudRateLabel.TabIndex = 2;
			this._baudRateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _baudRateBox
			// 
			this._baudRateBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._baudRateBox.Items.AddRange(TerminalUtil.BaudRates);
			this._baudRateBox.Location = new System.Drawing.Point(112, 40);
			this._baudRateBox.Name = "_baudRateBox";
			this._baudRateBox.Size = new System.Drawing.Size(96, 20);
			this._baudRateBox.TabIndex = 3;
			// 
			// _dataBitsLabel
			// 
			this._dataBitsLabel.Location = new System.Drawing.Point(8, 64);
			this._dataBitsLabel.Name = "_dataBitsLabel";
			this._dataBitsLabel.Size = new System.Drawing.Size(88, 23);
			this._dataBitsLabel.TabIndex = 4;
			this._dataBitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _dataBitsBox
			// 
			this._dataBitsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._dataBitsBox.Location = new System.Drawing.Point(112, 64);
			this._dataBitsBox.Name = "_dataBitsBox";
			this._dataBitsBox.Size = new System.Drawing.Size(96, 20);
			this._dataBitsBox.TabIndex = 5;
			// 
			// _parityLabel
			// 
			this._parityLabel.Location = new System.Drawing.Point(8, 88);
			this._parityLabel.Name = "_parityLabel";
			this._parityLabel.Size = new System.Drawing.Size(88, 23);
			this._parityLabel.TabIndex = 6;
			this._parityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _parityBox
			// 
			this._parityBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._parityBox.Items.AddRange(EnumDescAttribute.For(typeof(Parity)).DescriptionCollection());
			this._parityBox.Location = new System.Drawing.Point(112, 88);
			this._parityBox.Name = "_parityBox";
			this._parityBox.Size = new System.Drawing.Size(96, 20);
			this._parityBox.TabIndex = 7;
			// 
			// _stopBitsLabel
			// 
			this._stopBitsLabel.Location = new System.Drawing.Point(8, 112);
			this._stopBitsLabel.Name = "_stopBitsLabel";
			this._stopBitsLabel.Size = new System.Drawing.Size(88, 23);
			this._stopBitsLabel.TabIndex = 8;
			this._stopBitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _stopBitsBox
			// 
			this._stopBitsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._stopBitsBox.Items.AddRange(EnumDescAttribute.For(typeof(StopBits)).DescriptionCollection());
			this._stopBitsBox.Location = new System.Drawing.Point(112, 112);
			this._stopBitsBox.Name = "_stopBitsBox";
			this._stopBitsBox.Size = new System.Drawing.Size(96, 20);
			this._stopBitsBox.TabIndex = 9;
			// 
			// _flowControlLabel
			// 
			this._flowControlLabel.Location = new System.Drawing.Point(8, 136);
			this._flowControlLabel.Name = "_flowControlLabel";
			this._flowControlLabel.Size = new System.Drawing.Size(88, 23);
			this._flowControlLabel.TabIndex = 10;
			this._flowControlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _flowControlBox
			// 
			this._flowControlBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._flowControlBox.Items.AddRange(EnumDescAttribute.For(typeof(FlowControl)).DescriptionCollection());
			this._flowControlBox.Location = new System.Drawing.Point(112, 136);
			this._flowControlBox.Name = "_flowControlBox";
			this._flowControlBox.Size = new System.Drawing.Size(96, 20);
			this._flowControlBox.TabIndex = 11;
			// 
			// _transmitDelayPerCharLabel
			// 
			this._transmitDelayPerCharLabel.Location = new System.Drawing.Point(8, 160);
			this._transmitDelayPerCharLabel.Name = "_transmitDelayPerCharLabel";
			this._transmitDelayPerCharLabel.Size = new System.Drawing.Size(88, 23);
			this._transmitDelayPerCharLabel.TabIndex = 12;
			this._transmitDelayPerCharLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _transmitDelayPerCharBox
			// 
			this._transmitDelayPerCharBox.Location = new System.Drawing.Point(112, 160);
			this._transmitDelayPerCharBox.Name = "_transmitDelayPerCharBox";
			this._transmitDelayPerCharBox.Size = new System.Drawing.Size(96, 20);
			this._transmitDelayPerCharBox.TabIndex = 13;
			this._transmitDelayPerCharBox.MaxLength = 3;
			// 
			// _transmitDelayPerLineLabel
			// 
			this._transmitDelayPerLineLabel.Location = new System.Drawing.Point(8, 184);
			this._transmitDelayPerLineLabel.Name = "_transmitDelayPerLineLabel";
			this._transmitDelayPerLineLabel.Size = new System.Drawing.Size(88, 23);
			this._transmitDelayPerLineLabel.TabIndex = 14;
			this._transmitDelayPerLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _transmitDelayPerLineBox
			// 
			this._transmitDelayPerLineBox.Location = new System.Drawing.Point(112, 184);
			this._transmitDelayPerLineBox.Name = "_transmitDelayPerLineBox";
			this._transmitDelayPerLineBox.Size = new System.Drawing.Size(96, 20);
			this._transmitDelayPerLineBox.TabIndex = 15;
			this._transmitDelayPerLineBox.MaxLength = 3;
			// 
			// _loginButton
			// 
			this._loginButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._loginButton.Location = new System.Drawing.Point(48, 216);
			this._loginButton.Name = "_loginButton";
			this._loginButton.FlatStyle = FlatStyle.System;
			this._loginButton.TabIndex = 16;
			this._loginButton.Click += new System.EventHandler(this.OnOK);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(136, 216);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.TabIndex = 17;
			// 
			// SerialConfig
			// 
			this.AcceptButton = this._loginButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(218, 247);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																			this._transmitDelayPerCharLabel,
																			this._transmitDelayPerCharBox,
																			this._transmitDelayPerLineLabel,
																			this._transmitDelayPerLineBox,
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
																		  this._portLabel,
																		  this._loginButton,
																		  this._cancelButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SerialConfig";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion

		private TerminalConnection _con;
		public void ApplyParam(TerminalConnection con) {
			_con = con;
			SerialTerminalParam param = (SerialTerminalParam)con.Param;
			_portBox.Text = "COM"+param.Port;
			_baudRateBox.SelectedIndex = _baudRateBox.FindStringExact(param.BaudRate.ToString());
			_dataBitsBox.SelectedIndex = param.ByteSize==7? 0 : 1;
			_parityBox.SelectedIndex = (int)param.Parity;
			_stopBitsBox.SelectedIndex = (int)param.StopBits;
			_flowControlBox.SelectedIndex = (int)param.FlowControl;
			_transmitDelayPerCharBox.Text = param.TransmitDelayPerChar.ToString();
			_transmitDelayPerLineBox.Text = param.TransmitDelayPerLine.ToString();
		}
		private void OnOK(object sender, EventArgs args) {
			SerialTerminalParam p = (SerialTerminalParam)_con.Param.Clone();
			p.BaudRate = Int32.Parse(_baudRateBox.Text);
			p.ByteSize = (byte)(_dataBitsBox.SelectedIndex==0? 7 : 8);
			p.StopBits = (StopBits)_stopBitsBox.SelectedIndex;
			p.Parity = (Parity)_parityBox.SelectedIndex;
			p.FlowControl = (FlowControl)_flowControlBox.SelectedIndex;
			try {
				p.TransmitDelayPerChar = Int32.Parse(_transmitDelayPerCharBox.Text);
				p.TransmitDelayPerLine = Int32.Parse(_transmitDelayPerLineBox.Text);
			}
			catch(Exception ex) {
				GUtil.Warning(this, ex.Message);
				this.DialogResult = DialogResult.None;
				return;
			}

			try {
				((SerialTerminalConnection)_con).ApplySerialParam(p);
				this.DialogResult = DialogResult.OK;
			}
			catch(Exception ex) {
				GUtil.Warning(this, ex.Message);
				this.DialogResult = DialogResult.None;
			}
		}

	}
}
