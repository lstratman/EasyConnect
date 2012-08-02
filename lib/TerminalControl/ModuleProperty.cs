/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ModuleProperty.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

using Poderosa.MacroEnv;

namespace Poderosa.Forms
{
	/// <summary>
	/// ModuleProperty の概要の説明です。
	/// </summary>
	internal class ModuleProperty : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label _titleLabel;
		private TextBox _title;
		private System.Windows.Forms.Label _pathLabel;
		private TextBox _path;
		private System.Windows.Forms.Button _selectFileButton;
		private System.Windows.Forms.Label _additionalAssemblyLabel;
		private TextBox _additionalAssembly;
		private System.Windows.Forms.Label _shortcutLabel;
		private HotKey _shortcut;
		private System.Windows.Forms.CheckBox _debugOption;

		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		
		//編集対象のMacroModule 新規作成時はnull
		private MacroList _parent;
		private MacroModule _module;
		private Keys _prevShortCut;
		
		public MacroModule Module {
			get {
				return _module;
			}
		}
		public Keys ShortCut {
			get {
				return _shortcut.Key;
			}
		}

		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ModuleProperty(MacroList p, MacroModule mod, Keys shortcut) {
			_parent = p;
			_prevShortCut = shortcut;
			_module = mod==null? new MacroModule(0) : (MacroModule)mod.Clone();
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			this._titleLabel.Text = GApp.Strings.GetString("Form.ModuleProperty._titleLabel");
			this._pathLabel.Text = GApp.Strings.GetString("Form.ModuleProperty._pathLabel");
			this._additionalAssemblyLabel.Text = GApp.Strings.GetString("Form.ModuleProperty._additionalAssemblyLabel");
			this._shortcutLabel.Text = GApp.Strings.GetString("Form.ModuleProperty._shortcutLabel");
			this._debugOption.Text = GApp.Strings.GetString("Form.ModuleProperty._debugOption");
			this._okButton.Text = GApp.Strings.GetString("Common.OK");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this.Text = GApp.Strings.GetString("Form.ModuleProperty.Text");

			if(mod!=null) {
				_title.Text = _module.Title;
				_path.Text = _module.Path;
				_additionalAssembly.Text = Concat(_module.AdditionalAssemblies);
				_debugOption.Checked = _module.DebugMode;
				_shortcut.Key = shortcut;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this._titleLabel = new System.Windows.Forms.Label();
			this._title = new TextBox();
			this._pathLabel = new System.Windows.Forms.Label();
			this._path = new TextBox();
			this._selectFileButton = new System.Windows.Forms.Button();
			this._additionalAssemblyLabel = new System.Windows.Forms.Label();
			this._additionalAssembly = new TextBox();
			this._shortcutLabel = new System.Windows.Forms.Label();
			this._shortcut = new Poderosa.Forms.HotKey();
			this._debugOption = new System.Windows.Forms.CheckBox();
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _titleLabel
			// 
			this._titleLabel.Location = new System.Drawing.Point(8, 8);
			this._titleLabel.Name = "_titleLabel";
			this._titleLabel.TabIndex = 0;
			this._titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _title
			// 
			this._title.Location = new System.Drawing.Point(120, 8);
			this._title.Name = "_title";
			this._title.Size = new System.Drawing.Size(200, 19);
			this._title.TabIndex = 1;
			this._title.Text = "";
			// 
			// _pathLabel
			// 
			this._pathLabel.Location = new System.Drawing.Point(8, 32);
			this._pathLabel.Name = "_pathLabel";
			this._pathLabel.TabIndex = 2;
			this._pathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _path
			// 
			this._path.Location = new System.Drawing.Point(120, 32);
			this._path.Name = "_path";
			this._path.Size = new System.Drawing.Size(181, 19);
			this._path.TabIndex = 3;
			this._path.Text = "";
			// 
			// _selectFileButton
			// 
			this._selectFileButton.Location = new System.Drawing.Point(301, 32);
			this._selectFileButton.Name = "_selectFileButton";
			this._selectFileButton.FlatStyle = FlatStyle.System;
			this._selectFileButton.Size = new System.Drawing.Size(19, 19);
			this._selectFileButton.TabIndex = 4;
			this._selectFileButton.Text = "...";
			this._selectFileButton.Click += new System.EventHandler(this.OnSelectFile);
			// 
			// _additionalAssemblyLabel
			// 
			this._additionalAssemblyLabel.Location = new System.Drawing.Point(8, 56);
			this._additionalAssemblyLabel.Name = "_additionalAssemblyLabel";
			this._additionalAssemblyLabel.Size = new System.Drawing.Size(100, 32);
			this._additionalAssemblyLabel.TabIndex = 5;
			this._additionalAssemblyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _additionalAssembly
			// 
			this._additionalAssembly.Location = new System.Drawing.Point(120, 64);
			this._additionalAssembly.Name = "_additionalAssembly";
			this._additionalAssembly.Size = new System.Drawing.Size(200, 19);
			this._additionalAssembly.TabIndex = 6;
			this._additionalAssembly.Text = "";
			// 
			// _shortcutLabel
			// 
			this._shortcutLabel.Location = new System.Drawing.Point(8, 88);
			this._shortcutLabel.Name = "_shortcutLabel";
			this._shortcutLabel.TabIndex = 7;
			this._shortcutLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _shortcut
			// 
			this._shortcut.DebugTextBox = null;
			this._shortcut.ImeMode = System.Windows.Forms.ImeMode.Disable;
			this._shortcut.Key = System.Windows.Forms.Keys.None;
			this._shortcut.Location = new System.Drawing.Point(120, 88);
			this._shortcut.Name = "_shortcut";
			this._shortcut.Size = new System.Drawing.Size(80, 19);
			this._shortcut.TabIndex = 8;
			this._shortcut.Text = "";
			// 
			// _debugOption
			// 
			this._debugOption.Location = new System.Drawing.Point(8, 112);
			this._debugOption.Name = "_debugOption";
			this._debugOption.FlatStyle = FlatStyle.System;
			this._debugOption.Size = new System.Drawing.Size(296, 24);
			this._debugOption.TabIndex = 9;
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.Location = new System.Drawing.Point(168, 136);
			this._okButton.Name = "_okButton";
			this._okButton.FlatStyle = FlatStyle.System;
			this._okButton.TabIndex = 10;
			this._okButton.Click += new System.EventHandler(this.OnOK);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(248, 136);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.TabIndex = 11;
			// 
			// ModuleProperty
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(330, 167);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._titleLabel,
																		  this._title,
																		  this._pathLabel,
																		  this._path,
																		  this._selectFileButton,
																		  this._additionalAssemblyLabel,
																		  this._additionalAssembly,
																		  this._shortcutLabel,
																		  this._shortcut,
																		  this._debugOption,
																		  this._cancelButton,
																		  this._okButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ModuleProperty";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		#endregion

		private void OnSelectFile(object sender, EventArgs args) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.CheckFileExists = true;
			dlg.Multiselect = false;
			dlg.InitialDirectory = GApp.Options.DefaultFileDir;
			dlg.Title = GApp.Strings.GetString("Caption.ModuleProperty.SelectMacroFile");
			dlg.Filter = "JScript.NET Files(*.js)|*.js|.NET Executables(*.exe;*.dll)|*.exe;*.dll";
			if(GCUtil.ShowModalDialog(this, dlg)==DialogResult.OK) {
				GApp.Options.DefaultFileDir = GUtil.FileToDir(dlg.FileName);
				_path.Text = dlg.FileName;
				if(_title.Text.Length==0)
					_title.Text = System.IO.Path.GetFileName(dlg.FileName); //ファイル名本体をデフォルトのタイトルにする
			}
		}
		private void OnOK(object sender, EventArgs args) {
			this.DialogResult = DialogResult.None;
			if(!File.Exists(_path.Text)) {
				GUtil.Warning(this, String.Format(GApp.Strings.GetString("Message.ModuleProperty.FileNotExist"), _path.Text));
			}
			else if(_title.Text.Length>30)
				GUtil.Warning(this, GApp.Strings.GetString("Message.ModuleProperty.TooLongTitle"));
			else {
				if(_shortcut.Key!=_prevShortCut) {
					string n = _parent.FindCommandDescription(_shortcut.Key);
					if(n!=null) {
						GUtil.Warning(this, String.Format(GApp.Strings.GetString("Message.ModuleProperty.DuplicatedKey"), n));
						return;
					}
				}

				_module.Title = _title.Text;
				_module.Path = _path.Text;
				_module.DebugMode = _debugOption.Checked;
				_module.AdditionalAssemblies = ParseAdditionalAssemblies(_additionalAssembly.Text);
				this.DialogResult = DialogResult.OK;
			}
		}

		private string Concat(string[] v) {
			if(v==null) return "";
			StringBuilder b = new StringBuilder();
			foreach(string t in v) {
				if(b.Length>0) b.Append(';');
				b.Append(t);
			}
			return b.ToString();
		}

		private string[] ParseAdditionalAssemblies(string t) {
			string[] l = t.Split(new char[] { ';',',' });
			return l;
		}
	}
}
