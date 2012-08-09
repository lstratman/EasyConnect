/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: EditEnvVariable.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Poderosa.Forms
{
	/// <summary>
	/// EditEnvVariable の概要の説明です。
	/// </summary>
	internal class EditEnvVariable : System.Windows.Forms.Form
	{
		private bool _isNew;
		private EnvVariableList _parent;

		private System.Windows.Forms.Label _nameLabel;
		private TextBox _nameBox;
		private System.Windows.Forms.Label _valueLabel;
		private TextBox _valueBox;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EditEnvVariable(EnvVariableList p)
		{
			_parent = p;
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			this._nameLabel.Text = GApp.Strings.GetString("Form.EditEnvVariable._nameLabel");
			this._valueLabel.Text = GApp.Strings.GetString("Form.EditEnvVariable._valueLabel");
			this._okButton.Text = GApp.Strings.GetString("Common.OK");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this.Text = GApp.Strings.GetString("Form.EditEnvVariable.Text");
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
			this._nameLabel = new System.Windows.Forms.Label();
			this._nameBox = new TextBox();
			this._valueLabel = new System.Windows.Forms.Label();
			this._valueBox = new TextBox();
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _nameLabel
			// 
			this._nameLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._nameLabel.Location = new System.Drawing.Point(8, 8);
			this._nameLabel.Name = "_nameLabel";
			this._nameLabel.Size = new System.Drawing.Size(56, 16);
			this._nameLabel.TabIndex = 0;
			// 
			// _nameBox
			// 
			this._nameBox.Location = new System.Drawing.Point(72, 8);
			this._nameBox.Name = "_nameBox";
			this._nameBox.Size = new System.Drawing.Size(216, 19);
			this._nameBox.TabIndex = 1;
			this._nameBox.Text = "";
			// 
			// _valueLabel
			// 
			this._valueLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._valueLabel.Location = new System.Drawing.Point(8, 32);
			this._valueLabel.Name = "_valueLabel";
			this._valueLabel.Size = new System.Drawing.Size(56, 16);
			this._valueLabel.TabIndex = 2;
			// 
			// _valueBox
			// 
			this._valueBox.Location = new System.Drawing.Point(72, 32);
			this._valueBox.Name = "_valueBox";
			this._valueBox.Size = new System.Drawing.Size(216, 19);
			this._valueBox.TabIndex = 3;
			this._valueBox.Text = "";
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.Location = new System.Drawing.Point(136, 64);
			this._okButton.Name = "_okButton";
			this._okButton.FlatStyle = FlatStyle.System;
			this._okButton.TabIndex = 4;
			this._okButton.Click += new EventHandler(OnOK);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(216, 64);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.TabIndex = 5;
			// 
			// EditEnvVariable
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(292, 93);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._cancelButton,
																		  this._okButton,
																		  this._valueBox,
																		  this._valueLabel,
																		  this._nameBox,
																		  this._nameLabel});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditEnvVariable";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion

		public string VarName {
			get {
				return _nameBox.Text;
			}
			set {
				_nameBox.Text = value;
			}
		}
		public string VarValue {
			get {
				return _valueBox.Text;
			}
			set {
				_valueBox.Text = value;
			}
		}
		public bool IsNewVariable {
			get {
				return _isNew;
			}
			set {
				_isNew = value;
				_nameBox.Enabled = _isNew;
			}
		}

		private void OnOK(object sender, EventArgs args) {
			this.DialogResult = DialogResult.None;
			string n = _nameBox.Text;
			if(n.Length==0)
				GUtil.Warning(this, GApp.Strings.GetString("Message.EditEnvVariable.EmptyName"));
			else if(n.IndexOf('=')!=-1 || n.IndexOf(' ')!=-1)
				GUtil.Warning(this, GApp.Strings.GetString("Message.EditEnvVariable.InvalidChars"));
			else if(_isNew && _parent.HasVariable(n))
				GUtil.Warning(this, GApp.Strings.GetString("Message.EditEnvVariable.DuplicatedName"));
			else //success
				this.DialogResult = DialogResult.OK;
		}
	}
}
