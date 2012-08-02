/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ConnectionOptionPanel.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Communication;
using Poderosa.Toolkit;
using Poderosa.Config;
using Poderosa.UI;

namespace Poderosa.Forms {
	internal class ConnectionOptionPanel : OptionDialog.CategoryPanel
	{
		private System.Windows.Forms.GroupBox _socksGroup;
		private CheckBox _useSocks;
		private System.Windows.Forms.Label _socksServerLabel;
		private TextBox _socksServerBox;
		private System.Windows.Forms.Label _socksPortLabel;
		private TextBox _socksPortBox;
		private System.Windows.Forms.Label _socksAccountLabel;
		private TextBox _socksAccountBox;
		private System.Windows.Forms.Label _socksPasswordLabel;
		private TextBox _socksPasswordBox;
		private System.Windows.Forms.Label _socksNANetworksLabel;
		private TextBox _socksNANetworksBox;

		public ConnectionOptionPanel()
		{
			InitializeComponent();
			FillText();
		}
		private void InitializeComponent() {
			this._socksGroup = new System.Windows.Forms.GroupBox();
			this._useSocks = new CheckBox();
			this._socksServerLabel = new System.Windows.Forms.Label();
			this._socksServerBox = new TextBox();
			this._socksPortLabel = new System.Windows.Forms.Label();
			this._socksPortBox = new TextBox();
			this._socksAccountLabel = new System.Windows.Forms.Label();
			this._socksAccountBox = new TextBox();
			this._socksPasswordLabel = new System.Windows.Forms.Label();
			this._socksPasswordBox = new TextBox();
			this._socksNANetworksLabel = new System.Windows.Forms.Label();
			this._socksNANetworksBox = new TextBox();

			this._socksGroup.SuspendLayout();
		
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																						  this._useSocks,
																						  this._socksGroup});
			//
			//_useSocks
			//
			this._useSocks.Location = new System.Drawing.Point(16, 3);
			this._useSocks.Name = "_useSocksAuthentication";
			this._useSocks.FlatStyle = FlatStyle.System;
			this._useSocks.Size = new System.Drawing.Size(160, 23);
			this._useSocks.TabIndex = 1;
			this._useSocks.CheckedChanged += new EventHandler(OnUseSocksOptionChanged);
			//
			//_socksGroup
			//
			this._socksGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this._socksServerLabel,
																					  this._socksServerBox,
																					  this._socksPortLabel,
																					  this._socksPortBox,
																					  this._socksAccountLabel,
																					  this._socksAccountBox,
																					  this._socksPasswordLabel,
																					  this._socksPasswordBox,
																					  this._socksNANetworksLabel,
																					  this._socksNANetworksBox});
			this._socksGroup.Location = new System.Drawing.Point(8, 8);
			this._socksGroup.Name = "_socksGroup";
			this._socksGroup.FlatStyle = FlatStyle.System;
			this._socksGroup.Size = new System.Drawing.Size(416, 128);
			this._socksGroup.TabIndex = 2;
			this._socksGroup.TabStop = false;
			this._socksGroup.Text = "";
			//
			//_socksServerLabel
			//
			this._socksServerLabel.Location = new System.Drawing.Point(8, 18);
			this._socksServerLabel.Name = "_socksServerLabel";
			this._socksServerLabel.Size = new System.Drawing.Size(80, 23);
			this._socksServerLabel.TabIndex = 0;
			this._socksServerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			//_socksServerBox
			//
			this._socksServerBox.Location = new System.Drawing.Point(96, 18);
			this._socksServerBox.Name = "_socksServerBox";
			this._socksServerBox.Size = new System.Drawing.Size(104, 19);
			this._socksServerBox.Enabled = false;
			this._socksServerBox.TabIndex = 1;
			//
			//_socksPortLabel
			//
			this._socksPortLabel.Location = new System.Drawing.Point(216, 18);
			this._socksPortLabel.Name = "_socksPortLabel";
			this._socksPortLabel.Size = new System.Drawing.Size(80, 23);
			this._socksPortLabel.TabIndex = 2;
			this._socksPortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			//_socksPortBox
			//
			this._socksPortBox.Location = new System.Drawing.Point(304, 18);
			this._socksPortBox.Name = "_socksPortBox";
			this._socksPortBox.Size = new System.Drawing.Size(104, 19);
			this._socksPortBox.Enabled = false;
			this._socksPortBox.TabIndex = 3;
			this._socksPortBox.MaxLength = 5;
			//
			//_socksAccountLabel
			//
			this._socksAccountLabel.Location = new System.Drawing.Point(8, 40);
			this._socksAccountLabel.Name = "_socksAccountLabel";
			this._socksAccountLabel.Size = new System.Drawing.Size(80, 23);
			this._socksAccountLabel.TabIndex = 4;
			this._socksAccountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			//_socksAccountBox
			//
			this._socksAccountBox.Location = new System.Drawing.Point(96, 40);
			this._socksAccountBox.Name = "_socksAccountBox";
			this._socksAccountBox.Size = new System.Drawing.Size(104, 19);
			this._socksAccountBox.Enabled = false;
			this._socksAccountBox.TabIndex = 5;
			//
			//_socksPasswordLabel
			//
			this._socksPasswordLabel.Location = new System.Drawing.Point(216, 40);
			this._socksPasswordLabel.Name = "_socksPasswordLabel";
			this._socksPasswordLabel.Size = new System.Drawing.Size(80, 23);
			this._socksPasswordLabel.TabIndex = 6;
			this._socksPasswordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			//_socksPasswordBox
			//
			this._socksPasswordBox.Location = new System.Drawing.Point(304, 40);
			this._socksPasswordBox.Name = "_socksPasswordBox";
			this._socksPasswordBox.PasswordChar = '*';
			this._socksPasswordBox.Enabled = false;
			this._socksPasswordBox.Size = new System.Drawing.Size(104, 19);
			this._socksPasswordBox.TabIndex = 7;
			//
			//_socksNANetworksLabel
			//
			this._socksNANetworksLabel.Location = new System.Drawing.Point(8, 68);
			this._socksNANetworksLabel.Name = "_socksNANetworksLabel";
			this._socksNANetworksLabel.Size = new System.Drawing.Size(400, 28);
			this._socksNANetworksLabel.TabIndex = 8;
			this._socksNANetworksLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			//_socksNANetworksBox
			//
			this._socksNANetworksBox.Location = new System.Drawing.Point(8, 98);
			this._socksNANetworksBox.Name = "_socksNANetworksBox";
			this._socksNANetworksBox.Enabled = false;
			this._socksNANetworksBox.Size = new System.Drawing.Size(400, 19);
			this._socksNANetworksBox.TabIndex = 9;

			this.BackColor = ThemeUtil.TabPaneBackColor;
			this._socksGroup.ResumeLayout();
		}
		private void FillText() {
			this._useSocks.Text = GApp.Strings.GetString("Form.OptionDialog._useSocks");
			this._socksServerLabel.Text = GApp.Strings.GetString("Form.OptionDialog._socksServerLabel");
			this._socksPortLabel.Text = GApp.Strings.GetString("Form.OptionDialog._socksPortLabel");
			this._socksAccountLabel.Text = GApp.Strings.GetString("Form.OptionDialog._socksAccountLabel");
			this._socksPasswordLabel.Text = GApp.Strings.GetString("Form.OptionDialog._socksPasswordLabel");
			this._socksNANetworksLabel.Text = GApp.Strings.GetString("Form.OptionDialog._socksNANetworksLabel");
		}
		public override void InitUI(ContainerOptions options) {
			_useSocks.Checked = options.UseSocks;
			_socksServerBox.Text = options.SocksServer;
			_socksPortBox.Text = options.SocksPort.ToString();
			_socksAccountBox.Text = options.SocksAccount;
			_socksPasswordBox.Text = options.SocksPassword;
			_socksNANetworksBox.Text = options.SocksNANetworks;
		}
		public override bool Commit(ContainerOptions options) {
			string itemname = "";
			try {
				options.UseSocks = _useSocks.Checked;
				if(options.UseSocks && _socksServerBox.Text.Length==0)
					throw new Exception(GApp.Strings.GetString("Message.OptionDialog.EmptySocksServer"));
				options.SocksServer = _socksServerBox.Text;
				itemname = GApp.Strings.GetString("Caption.OptionDialog.SOCKSPortNumber");
				options.SocksPort = Int32.Parse(_socksPortBox.Text);
				options.SocksAccount = _socksAccountBox.Text;
				options.SocksPassword = _socksPasswordBox.Text;
				itemname = GApp.Strings.GetString("Caption.OptionDialog.NetworkAddress");
				foreach(string c in _socksNANetworksBox.Text.Split(';')) {
					if(!NetUtil.IsNetworkAddress(c)) throw new FormatException();
				}
				options.SocksNANetworks = _socksNANetworksBox.Text;

				return true;
			}
			catch(FormatException) {
				GUtil.Warning(this, String.Format(GApp.Strings.GetString("Message.OptionDialog.InvalidItem"), itemname));
				return false;
			}
			catch(Exception ex) {
				GUtil.Warning(this, ex.Message);
				return false;
			}
		}

		private void OnUseSocksOptionChanged(object sender, EventArgs args) {
			bool e = _useSocks.Checked;
			_socksServerBox.Enabled = e;
			_socksPortBox.Enabled = e;
			_socksAccountBox.Enabled = e;
			_socksPasswordBox.Enabled = e;
			_socksNANetworksBox.Enabled = e;
		}
	}
}
