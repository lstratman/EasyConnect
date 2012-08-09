/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: LineFeedStyleDialog.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Poderosa.ConnectionParam;
using Poderosa.Communication;
using EnumDescAttributeT = Poderosa.Toolkit.EnumDescAttribute;

namespace Poderosa.Forms
{
	/// <summary>
	/// LineFeedStyleDialog の概要の説明です。
	/// </summary>
	internal class LineFeedStyleDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label _lineFeedLabel;
		private System.Windows.Forms.ComboBox _lineFeedBox;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public LineFeedStyleDialog(TerminalConnection con)
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this._okButton.Text = GApp.Strings.GetString("Common.OK");
			this._lineFeedLabel.Text = GApp.Strings.GetString("Form.LineFeedStyleDialog._lineFeedLabel");
			this.Text = GApp.Strings.GetString("Form.LineFeedStyleDialog.Text");
			this._lineFeedBox.Items.AddRange(EnumDescAttributeT.For(typeof(LineFeedRule)).DescriptionCollection());
			this._lineFeedBox.SelectedIndex = (int)con.Param.LineFeedRule;
		}

		public LineFeedRule LineFeedRule {
			get {
				return (LineFeedRule)_lineFeedBox.SelectedIndex;
			}
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

		#region Windows フォーム デザイナで生成されたコード 
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this._lineFeedLabel = new System.Windows.Forms.Label();
			this._lineFeedBox = new ComboBox();
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _lineFeedLabel
			// 
			this._lineFeedLabel.Location = new System.Drawing.Point(8, 16);
			this._lineFeedLabel.Name = "_lineFeedLabel";
			this._lineFeedLabel.TabIndex = 0;
			this._lineFeedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _lineFeedBox
			// 
			this._lineFeedBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._lineFeedBox.Location = new System.Drawing.Point(96, 16);
			this._lineFeedBox.Name = "_lineFeedBox";
			this._lineFeedBox.Size = new System.Drawing.Size(152, 20);
			this._lineFeedBox.TabIndex = 1;
			this._lineFeedBox.Text = "comboBox1";
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._okButton.Location = new System.Drawing.Point(80, 48);
			this._okButton.Name = "_okButton";
			this._okButton.TabIndex = 2;
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._cancelButton.Location = new System.Drawing.Point(168, 48);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.TabIndex = 3;
			// 
			// LineFeedStyleDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.ClientSize = new System.Drawing.Size(256, 78);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._lineFeedBox);
			this.Controls.Add(this._lineFeedLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LineFeedStyleDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.AcceptButton = _okButton;
			this.CancelButton = _cancelButton;
			this.ResumeLayout(false);

		}
		#endregion
	}
}
