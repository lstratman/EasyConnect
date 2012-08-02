/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: SSHOptionPanel.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Windows.Forms;
using System.Diagnostics;
using Poderosa.Toolkit;
using Poderosa.Config;
using Poderosa.SSH;
using Poderosa.UI;

using Granados.SSHC;
using Granados.PKI;

namespace Poderosa.Forms
{
	/// <summary>
	/// SSHOptionPanel の概要の説明です。
	/// </summary>
	internal class SSHOptionPanel : OptionDialog.CategoryPanel
	{
		private string[] _cipherAlgorithmOrder;

		private System.Windows.Forms.GroupBox _cipherOrderGroup;
		private System.Windows.Forms.ListBox _cipherOrderList;
		private System.Windows.Forms.Button _algorithmOrderUp;
		private System.Windows.Forms.Button _algorithmOrderDown;
		private System.Windows.Forms.GroupBox _ssh2OptionGroup;
		private System.Windows.Forms.Label _hostKeyLabel;
		private ComboBox _hostKeyBox;
		private System.Windows.Forms.Label _windowSizeLabel;
		private TextBox _windowSizeBox;
		private System.Windows.Forms.GroupBox _sshMiscGroup;
		private CheckBox _retainsPassphrase;
		private CheckBox _sshCheckMAC;

		public SSHOptionPanel()
		{
			InitializeComponent();
			FillText();
		}
		private void InitializeComponent() {
			this._cipherOrderGroup = new System.Windows.Forms.GroupBox();
			this._cipherOrderList = new System.Windows.Forms.ListBox();
			this._algorithmOrderUp = new System.Windows.Forms.Button();
			this._algorithmOrderDown = new System.Windows.Forms.Button();
			this._ssh2OptionGroup = new System.Windows.Forms.GroupBox();
			this._hostKeyLabel = new System.Windows.Forms.Label();
			this._hostKeyBox = new ComboBox();
			this._windowSizeLabel = new System.Windows.Forms.Label();
			this._windowSizeBox = new TextBox();
			this._sshMiscGroup = new System.Windows.Forms.GroupBox();
			this._retainsPassphrase = new System.Windows.Forms.CheckBox();
			this._sshCheckMAC = new System.Windows.Forms.CheckBox();

			this._cipherOrderGroup.SuspendLayout();
			this._ssh2OptionGroup.SuspendLayout();
			this._sshMiscGroup.SuspendLayout();
		
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this._cipherOrderGroup,
																				   this._ssh2OptionGroup,
																				   this._sshMiscGroup});
			// 
			// _cipherOrderGroup
			// 
			this._cipherOrderGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																							this._cipherOrderList,
																							this._algorithmOrderUp,
																							this._algorithmOrderDown});
			this._cipherOrderGroup.Location = new System.Drawing.Point(8, 8);
			this._cipherOrderGroup.Name = "_cipherOrderGroup";
			this._cipherOrderGroup.FlatStyle = FlatStyle.System;
			this._cipherOrderGroup.Size = new System.Drawing.Size(416, 80);
			this._cipherOrderGroup.TabIndex = 0;
			this._cipherOrderGroup.TabStop = false;
			// 
			// _cipherOrderList
			// 
			this._cipherOrderList.ItemHeight = 12;
			this._cipherOrderList.Location = new System.Drawing.Point(8, 16);
			this._cipherOrderList.Name = "_cipherOrderList";
			this._cipherOrderList.Size = new System.Drawing.Size(208, 56);
			this._cipherOrderList.TabIndex = 1;
			// 
			// _algorithmOrderUp
			// 
			this._algorithmOrderUp.Location = new System.Drawing.Point(232, 16);
			this._algorithmOrderUp.Name = "_algorithmOrderUp";
			this._algorithmOrderUp.FlatStyle = FlatStyle.System;
			this._algorithmOrderUp.TabIndex = 2;
			this._algorithmOrderUp.Click += new System.EventHandler(this.OnCipherAlgorithmOrderUp);
			// 
			// _algorithmOrderDown
			// 
			this._algorithmOrderDown.Location = new System.Drawing.Point(232, 48);
			this._algorithmOrderDown.Name = "_algorithmOrderDown";
			this._algorithmOrderDown.FlatStyle = FlatStyle.System;
			this._algorithmOrderDown.TabIndex = 3;
			this._algorithmOrderDown.Click += new System.EventHandler(this.OnCipherAlgorithmOrderDown);
			// 
			// _ssh2OptionGroup
			// 
			this._ssh2OptionGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this._hostKeyLabel,
																						   this._hostKeyBox,
																						   this._windowSizeLabel,
																						   this._windowSizeBox});
			this._ssh2OptionGroup.Location = new System.Drawing.Point(8, 96);
			this._ssh2OptionGroup.Name = "_ssh2OptionGroup";
			this._ssh2OptionGroup.FlatStyle = FlatStyle.System;
			this._ssh2OptionGroup.Size = new System.Drawing.Size(416, 80);
			this._ssh2OptionGroup.TabIndex = 4;
			this._ssh2OptionGroup.TabStop = false;
			// 
			// _hostKeyLabel
			// 
			this._hostKeyLabel.Location = new System.Drawing.Point(8, 16);
			this._hostKeyLabel.Name = "_hostKeyLabel";
			this._hostKeyLabel.Size = new System.Drawing.Size(200, 23);
			this._hostKeyLabel.TabIndex = 5;
			this._hostKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _hostKeyBox
			// 
			this._hostKeyBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._hostKeyBox.Items.AddRange(new object[] {
															 "DSA",
															 "RSA"});
			this._hostKeyBox.Location = new System.Drawing.Point(224, 16);
			this._hostKeyBox.Name = "_hostKeyBox";
			this._hostKeyBox.Size = new System.Drawing.Size(121, 20);
			this._hostKeyBox.TabIndex = 6;
			// 
			// _windowSizeLabel
			// 
			this._windowSizeLabel.Location = new System.Drawing.Point(8, 48);
			this._windowSizeLabel.Name = "_windowSizeLabel";
			this._windowSizeLabel.Size = new System.Drawing.Size(192, 23);
			this._windowSizeLabel.TabIndex = 7;
			this._windowSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _windowSizeBox
			// 
			this._windowSizeBox.Location = new System.Drawing.Point(224, 48);
			this._windowSizeBox.MaxLength = 5;
			this._windowSizeBox.Name = "_windowSizeBox";
			this._windowSizeBox.Size = new System.Drawing.Size(120, 19);
			this._windowSizeBox.TabIndex = 8;
			this._windowSizeBox.Text = "0";
			// 
			// _sshMiscGroup
			// 
			this._sshMiscGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this._sshCheckMAC,
																						this._retainsPassphrase});
			this._sshMiscGroup.Location = new System.Drawing.Point(8, 180);
			this._sshMiscGroup.Name = "_sshMiscGroup";
			this._sshMiscGroup.FlatStyle = FlatStyle.System;
			this._sshMiscGroup.Size = new System.Drawing.Size(416, 80);
			this._sshMiscGroup.TabIndex = 9;
			this._sshMiscGroup.TabStop = false;
			// 
			// _retainsPassphrase
			// 
			this._retainsPassphrase.Location = new System.Drawing.Point(8, 14);
			this._retainsPassphrase.Name = "_retainsPassphrase";
			this._retainsPassphrase.FlatStyle = FlatStyle.System;
			this._retainsPassphrase.Size = new System.Drawing.Size(400, 23);
			this._retainsPassphrase.TabIndex = 10;
			// 
			// _sshCheckMAC
			// 
			this._sshCheckMAC.Location = new System.Drawing.Point(8, 33);
			this._sshCheckMAC.Name = "_sshCheckMAC";
			this._sshCheckMAC.FlatStyle = FlatStyle.System;
			this._sshCheckMAC.Size = new System.Drawing.Size(400, 37);
			this._sshCheckMAC.TabIndex = 11;

			this.BackColor = ThemeUtil.TabPaneBackColor;
			this._cipherOrderGroup.ResumeLayout();
			this._ssh2OptionGroup.ResumeLayout();
			this._sshMiscGroup.ResumeLayout();
		}
		private void FillText() {
			this._cipherOrderGroup.Text = GApp.Strings.GetString("Form.OptionDialog._cipherOrderGroup");
			this._algorithmOrderUp.Text = GApp.Strings.GetString("Form.OptionDialog._algorithmOrderUp");
			this._algorithmOrderDown.Text = GApp.Strings.GetString("Form.OptionDialog._algorithmOrderDown");
			this._ssh2OptionGroup.Text = GApp.Strings.GetString("Form.OptionDialog._ssh2OptionGroup");
			this._hostKeyLabel.Text = GApp.Strings.GetString("Form.OptionDialog._hostKeyLabel");
			this._windowSizeLabel.Text = GApp.Strings.GetString("Form.OptionDialog._windowSizeLabel");
			this._sshMiscGroup.Text = GApp.Strings.GetString("Form.OptionDialog._sshMiscGroup");
			this._retainsPassphrase.Text = GApp.Strings.GetString("Form.OptionDialog._retainsPassphrase");
			this._sshCheckMAC.Text = GApp.Strings.GetString("Form.OptionDialog._sshCheckMAC");
		}
		public override void InitUI(ContainerOptions options) {
			_cipherOrderList.Items.Clear();
			string[] co = options.CipherAlgorithmOrder;
			foreach(string c in co)
				_cipherOrderList.Items.Add(c);
			_hostKeyBox.SelectedIndex = LocalSSHUtil.ParsePublicKeyAlgorithm(options.HostKeyAlgorithmOrder[0])==PublicKeyAlgorithm.DSA? 0 : 1; //これはDSA/RSAのどちらかしかない
			_windowSizeBox.Text = options.SSHWindowSize.ToString();
			_retainsPassphrase.Checked = options.RetainsPassphrase;
			_sshCheckMAC.Checked = options.SSHCheckMAC;
			_cipherAlgorithmOrder = options.CipherAlgorithmOrder;
		}
		public override bool Commit(ContainerOptions options) {
			//暗号アルゴリズム順序はoptionsを直接いじっているのでここでは何もしなくてよい
			try {
				PublicKeyAlgorithm[] pa = new PublicKeyAlgorithm[2];
				if(_hostKeyBox.SelectedIndex==0) {
					pa[0] = PublicKeyAlgorithm.DSA;
					pa[1] = PublicKeyAlgorithm.RSA;
				}
				else {
					pa[0] = PublicKeyAlgorithm.RSA;
					pa[1] = PublicKeyAlgorithm.DSA;
				}
				options.HostKeyAlgorithmOrder = LocalSSHUtil.FormatPublicKeyAlgorithmList(pa);

				try {
					options.SSHWindowSize = Int32.Parse(_windowSizeBox.Text);
				}
				catch(FormatException) {
					GUtil.Warning(this, GApp.Strings.GetString("Message.OptionDialog.InvalidWindowSize"));
					return false;
				}

				options.RetainsPassphrase = _retainsPassphrase.Checked;
				options.SSHCheckMAC = _sshCheckMAC.Checked;
				options.CipherAlgorithmOrder = _cipherAlgorithmOrder;

				return true;
			}
			catch(Exception ex) {
				GUtil.Warning(this, ex.Message);
				return false;
			}
		}

		//SSHオプション関係
		private void OnCipherAlgorithmOrderUp(object sender, EventArgs args) {
			int i = _cipherOrderList.SelectedIndex;
			if(i==-1 || i==0) return; //選択されていないか既にトップなら何もしない

			string temp1 = _cipherAlgorithmOrder[i];
			_cipherAlgorithmOrder[i] = _cipherAlgorithmOrder[i-1];
			_cipherAlgorithmOrder[i-1] = temp1;

			object temp2 = _cipherOrderList.SelectedItem;
			_cipherOrderList.Items.RemoveAt(i);
			_cipherOrderList.Items.Insert(i-1, temp2);

			_cipherOrderList.SelectedIndex = i-1;
		}
		private void OnCipherAlgorithmOrderDown(object sender, EventArgs args) {
			int i = _cipherOrderList.SelectedIndex;
			if(i==-1 || i==_cipherOrderList.Items.Count-1) return; //選択されていなければ何もしない

			string temp1 = _cipherAlgorithmOrder[i];
			_cipherAlgorithmOrder[i] = _cipherAlgorithmOrder[i+1];
			_cipherAlgorithmOrder[i+1] = temp1;

			object temp2 = _cipherOrderList.SelectedItem;
			_cipherOrderList.Items.RemoveAt(i);
			_cipherOrderList.Items.Insert(i+1, temp2);

			_cipherOrderList.SelectedIndex = i+1;
		}
		//アルゴリズム名
		private static string CipherAlgorithmName(CipherAlgorithm a) {
			switch(a) {
				case CipherAlgorithm.AES128:
					return "AES(Rijndael) (SSH2 only)";
				case CipherAlgorithm.Blowfish:
					return "Blowfish";
				case CipherAlgorithm.TripleDES:
					return "TripleDES";
				default:
					throw new Exception("Unexpected Algorithm "+a);
			}
		}
	}
}
