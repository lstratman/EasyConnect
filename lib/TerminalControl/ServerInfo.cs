/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ServerInfo.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Text;

using Poderosa;
using Poderosa.Toolkit;
using EnumDescAttributeT = Poderosa.Toolkit.EnumDescAttribute;
using Poderosa.Communication;
using Poderosa.ConnectionParam;
using Poderosa.Terminal;

namespace Poderosa.Forms
{
	/// <summary>
	/// ServerInfo の概要の説明です。
	/// </summary>
	internal class ServerInfo : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Label _serverNamesLabel;
		private System.Windows.Forms.TextBox _serverNamesBox;
		private System.Windows.Forms.Label _IPAddressLabel;
		private System.Windows.Forms.TextBox _IPAddressBox;
		private System.Windows.Forms.TextBox _protocolBox;
		private System.Windows.Forms.Label _protocolLabel;
		private System.Windows.Forms.TextBox _terminalTypeBox;
		private System.Windows.Forms.Label _terminalTypeLabel;
		private System.Windows.Forms.Label _parameterLabel;
		private System.Windows.Forms.TextBox _parameterBox;
		private System.Windows.Forms.Label _statsLabel;
		private System.Windows.Forms.Label _transmitBytes;
		private System.Windows.Forms.Label _receiveBytes;
		private System.Windows.Forms.Label _logLabel;
		private System.Windows.Forms.TextBox _logBox;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ServerInfo(TerminalConnection con)
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			_serverNamesBox.Text = con.ServerName;
			_IPAddressBox.Text = con.ServerAddress==null? "" : con.ServerAddress.ToString();
			_protocolBox.Text = con.ProtocolDescription;
			_parameterBox.Lines = con.ConnectionParameter;
			_terminalTypeBox.Text = EnumDescAttributeT.For(typeof(TerminalType)).GetDescription(con.Param.TerminalType);
			string li = EnumDescAttributeT.For(typeof(LogType)).GetDescription(con.LogType);
			if(con.LogType!=LogType.None) li += "(" + con.LogPath + ")";
			_logBox.Text = li;
			_receiveBytes.Text = String.Format("{0,10}{1}", con.ReceivedDataSize, GApp.Strings.GetString("Caption.ServerInfo.BytesReceived"));
			_transmitBytes.Text = String.Format("{0,10}{1}", con.SentDataSize, GApp.Strings.GetString("Caption.ServerInfo.BytesSent"));

			this._okButton.Text = GApp.Strings.GetString("Common.OK");
			this._serverNamesLabel.Text = GApp.Strings.GetString("Form.ServerInfo._serverNamesLabel");
			this._IPAddressLabel.Text = GApp.Strings.GetString("Form.ServerInfo._IPAddressLabel");
			this._protocolLabel.Text = GApp.Strings.GetString("Form.ServerInfo._protocolLabel");
			this._terminalTypeLabel.Text = GApp.Strings.GetString("Form.ServerInfo._terminalTypeLabel");
			this._parameterLabel.Text = GApp.Strings.GetString("Form.ServerInfo._parameterLabel");
			this._statsLabel.Text = GApp.Strings.GetString("Form.ServerInfo._statsLabel");
			this._logLabel.Text = GApp.Strings.GetString("Form.ServerInfo._logLabel");
			this.Text = GApp.Strings.GetString("Form.ServerInfo.Text");
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
			this._okButton = new System.Windows.Forms.Button();
			this._serverNamesLabel = new System.Windows.Forms.Label();
			this._serverNamesBox = new System.Windows.Forms.TextBox();
			this._IPAddressLabel = new System.Windows.Forms.Label();
			this._IPAddressBox = new System.Windows.Forms.TextBox();
			this._protocolBox = new System.Windows.Forms.TextBox();
			this._protocolLabel = new System.Windows.Forms.Label();
			this._terminalTypeBox = new System.Windows.Forms.TextBox();
			this._terminalTypeLabel = new System.Windows.Forms.Label();
			this._parameterLabel = new System.Windows.Forms.Label();
			this._parameterBox = new System.Windows.Forms.TextBox();
			this._statsLabel = new System.Windows.Forms.Label();
			this._transmitBytes = new System.Windows.Forms.Label();
			this._receiveBytes = new System.Windows.Forms.Label();
			this._logLabel = new System.Windows.Forms.Label();
			this._logBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.Location = new System.Drawing.Point(112, 256);
			this._okButton.Name = "_okButton";
			this._okButton.FlatStyle = FlatStyle.System;
			this._okButton.TabIndex = 0;
			// 
			// _serverNamesLabel
			// 
			this._serverNamesLabel.Location = new System.Drawing.Point(8, 8);
			this._serverNamesLabel.Name = "_serverNamesLabel";
			this._serverNamesLabel.Size = new System.Drawing.Size(80, 16);
			this._serverNamesLabel.TabIndex = 1;
			this._serverNamesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _serverNamesBox
			// 
			this._serverNamesBox.Location = new System.Drawing.Point(88, 8);
			this._serverNamesBox.Name = "_serverNamesBox";
			this._serverNamesBox.ReadOnly = true;
			this._serverNamesBox.Size = new System.Drawing.Size(224, 19);
			this._serverNamesBox.TabIndex = 2;
			this._serverNamesBox.Text = "";
			// 
			// _IPAddressLabel
			// 
			this._IPAddressLabel.Location = new System.Drawing.Point(8, 32);
			this._IPAddressLabel.Name = "_IPAddressLabel";
			this._IPAddressLabel.Size = new System.Drawing.Size(80, 16);
			this._IPAddressLabel.TabIndex = 3;
			this._IPAddressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _IPAddressBox
			// 
			this._IPAddressBox.Location = new System.Drawing.Point(88, 32);
			this._IPAddressBox.Name = "_IPAddressBox";
			this._IPAddressBox.ReadOnly = true;
			this._IPAddressBox.Size = new System.Drawing.Size(224, 19);
			this._IPAddressBox.TabIndex = 4;
			this._IPAddressBox.Text = "";
			// 
			// _protocolBox
			// 
			this._protocolBox.Location = new System.Drawing.Point(88, 56);
			this._protocolBox.Name = "_protocolBox";
			this._protocolBox.ReadOnly = true;
			this._protocolBox.Size = new System.Drawing.Size(224, 19);
			this._protocolBox.TabIndex = 5;
			this._protocolBox.Text = "";
			// 
			// _protocolLabel
			// 
			this._protocolLabel.Location = new System.Drawing.Point(8, 56);
			this._protocolLabel.Name = "_protocolLabel";
			this._protocolLabel.Size = new System.Drawing.Size(80, 16);
			this._protocolLabel.TabIndex = 6;
			this._protocolLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _terminalTypeBox
			// 
			this._terminalTypeBox.Location = new System.Drawing.Point(88, 80);
			this._terminalTypeBox.Name = "_terminalTypeBox";
			this._terminalTypeBox.ReadOnly = true;
			this._terminalTypeBox.Size = new System.Drawing.Size(224, 19);
			this._terminalTypeBox.TabIndex = 5;
			this._terminalTypeBox.Text = "";
			// 
			// _terminalTypeLabel
			// 
			this._terminalTypeLabel.Location = new System.Drawing.Point(8, 80);
			this._terminalTypeLabel.Name = "_terminalTypeLabel";
			this._terminalTypeLabel.Size = new System.Drawing.Size(80, 16);
			this._terminalTypeLabel.TabIndex = 6;
			this._terminalTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _parameterLabel
			// 
			this._parameterLabel.Location = new System.Drawing.Point(8, 104);
			this._parameterLabel.Name = "_parameterLabel";
			this._parameterLabel.Size = new System.Drawing.Size(80, 16);
			this._parameterLabel.TabIndex = 7;
			this._parameterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _parameterBox
			// 
			this._parameterBox.Location = new System.Drawing.Point(88, 104);
			this._parameterBox.Multiline = true;
			this._parameterBox.Name = "_parameterBox";
			this._parameterBox.ReadOnly = true;
			this._parameterBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this._parameterBox.Size = new System.Drawing.Size(224, 80);
			this._parameterBox.TabIndex = 8;
			this._parameterBox.Text = "";
			// 
			// _statsLabel
			// 
			this._statsLabel.Location = new System.Drawing.Point(8, 224);
			this._statsLabel.Name = "_statsLabel";
			this._statsLabel.Size = new System.Drawing.Size(104, 16);
			this._statsLabel.TabIndex = 9;
			this._statsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _transmitBytes
			// 
			this._transmitBytes.Location = new System.Drawing.Point(120, 224);
			this._transmitBytes.Name = "_transmitBytes";
			this._transmitBytes.Size = new System.Drawing.Size(184, 16);
			this._transmitBytes.TabIndex = 10;
			this._transmitBytes.Text = "0";
			this._transmitBytes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _receiveBytes
			// 
			this._receiveBytes.Location = new System.Drawing.Point(120, 240);
			this._receiveBytes.Name = "_receiveBytes";
			this._receiveBytes.Size = new System.Drawing.Size(184, 16);
			this._receiveBytes.TabIndex = 11;
			this._receiveBytes.Text = "0";
			this._receiveBytes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _logLabel
			// 
			this._logLabel.Location = new System.Drawing.Point(8, 192);
			this._logLabel.Name = "_logLabel";
			this._logLabel.Size = new System.Drawing.Size(80, 16);
			this._logLabel.TabIndex = 13;
			this._logLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _logBox
			// 
			this._logBox.Location = new System.Drawing.Point(88, 192);
			this._logBox.Name = "_logBox";
			this._logBox.ReadOnly = true;
			this._logBox.Size = new System.Drawing.Size(224, 19);
			this._logBox.TabIndex = 12;
			this._logBox.Text = "";
			// 
			// ServerInfo
			// 
			this.AcceptButton = this._okButton;
			this.CancelButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.ClientSize = new System.Drawing.Size(314, 287);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._logLabel,
																		  this._logBox,
																		  this._receiveBytes,
																		  this._transmitBytes,
																		  this._statsLabel,
																		  this._parameterBox,
																		  this._parameterLabel,
																		  this._protocolLabel,
																		  this._protocolBox,
																		  this._terminalTypeLabel,
																		  this._terminalTypeBox,
																		  this._IPAddressBox,
																		  this._IPAddressLabel,
																		  this._serverNamesBox,
																		  this._serverNamesLabel,
																		  this._okButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ServerInfo";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion

		private static string FormatStrings(string[] v) {
			StringBuilder b = new StringBuilder();
			if(v!=null) {
				foreach(string t in v) {
					if(b.Length>0) b.Append(", ");
					b.Append(t);
				}
			}
			else
				b.Append("-");

			return b.ToString();
		}
		private static string FormatIPs(IPAddress[] v) {
			StringBuilder b = new StringBuilder();
			if(v!=null) {
				foreach(IPAddress t in v) {
					if(b.Length>0) b.Append(", ");
					b.Append(t.ToString());
				}
			}
			else
				b.Append("-");

			return b.ToString();
		}

	}
}
