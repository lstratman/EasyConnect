using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

using Poderosa.Config;
using Poderosa.ConnectionParam;

namespace Poderosa.Forms
{
	/// <summary>
	/// ConfigConverter の概要の説明です。
	/// </summary>
	internal class WelcomeDialog : Form
	{
		private CID _cid;

		private System.Windows.Forms.Label _welcomeMessage;
		private System.Windows.Forms.RadioButton _optNewConnection;
		private System.Windows.Forms.RadioButton _optCygwin;
		private System.Windows.Forms.RadioButton _optConvert;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.CheckBox _checkNext;

		private void InitializeComponent() {
			this._welcomeMessage = new System.Windows.Forms.Label();
			this._optNewConnection = new System.Windows.Forms.RadioButton();
			this._optCygwin = new System.Windows.Forms.RadioButton();
			this._optConvert = new System.Windows.Forms.RadioButton();
			this._cancelButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this._checkNext = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// _welcomeMessage
			// 
			this._welcomeMessage.Location = new System.Drawing.Point(8, 8);
			this._welcomeMessage.Name = "_welcomeMessage";
			this._welcomeMessage.Size = new System.Drawing.Size(336, 56);
			this._welcomeMessage.TabIndex = 0;
			// 
			// _optNewConnection
			// 
			this._optNewConnection.Checked = true;
			this._optNewConnection.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._optNewConnection.Location = new System.Drawing.Point(16, 72);
			this._optNewConnection.Name = "_optNewConnection";
			this._optNewConnection.Size = new System.Drawing.Size(320, 24);
			this._optNewConnection.TabIndex = 1;
			this._optNewConnection.TabStop = true;
			// 
			// _optCygwin
			// 
			this._optCygwin.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._optCygwin.Location = new System.Drawing.Point(16, 96);
			this._optCygwin.Name = "_optCygwin";
			this._optCygwin.Size = new System.Drawing.Size(320, 24);
			this._optCygwin.TabIndex = 2;
			// 
			// _optConvert
			// 
			this._optConvert.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._optConvert.Location = new System.Drawing.Point(16, 120);
			this._optConvert.Name = "_optConvert";
			this._optConvert.Size = new System.Drawing.Size(320, 24);
			this._optConvert.TabIndex = 3;
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._cancelButton.Location = new System.Drawing.Point(264, 184);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.TabIndex = 4;
			// 
			// _okButton
			// 
			this._okButton.DialogResult = DialogResult.OK;
			this._okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._okButton.Location = new System.Drawing.Point(168, 184);
			this._okButton.Name = "_okButton";
			this._okButton.TabIndex = 5;
			this._okButton.Click += new System.EventHandler(this.OnOK);
			// 
			// _checkNext
			// 
			this._checkNext.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._checkNext.Location = new System.Drawing.Point(144, 160);
			this._checkNext.Name = "_checkNext";
			this._checkNext.Size = new System.Drawing.Size(200, 16);
			this._checkNext.TabIndex = 6;
			// 
			// WelcomeDialog
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(354, 216);
			this.Controls.Add(this._checkNext);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._optConvert);
			this.Controls.Add(this._optCygwin);
			this.Controls.Add(this._optNewConnection);
			this.Controls.Add(this._welcomeMessage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WelcomeDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);

		}
		public CID CID {
			get {
				return _cid;
			}
		}
	
		public WelcomeDialog()
		{
			_cid = CID.NOP;
			InitializeComponent();
			InitializeText();
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			_checkNext.Checked = GApp.Options.ShowWelcomeDialog;
		}
		private void InitializeText() {
			this.Text = GApp.Strings.GetString("Form.WelcomeDialog.Text");
			_welcomeMessage.Text = GApp.Strings.GetString("Form.WelcomeDialog._welcomeMessage");
			_optNewConnection.Text = GApp.Strings.GetString("Form.WelcomeDialog._optNewConnection");
			_optCygwin.Text = GApp.Strings.GetString("Form.WelcomeDialog._optCygwin");
			_optConvert.Text = GApp.Strings.GetString("Form.WelcomeDialog._optConvert");
			_cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			_okButton.Text = GApp.Strings.GetString("Common.OK");
			_checkNext.Text = GApp.Strings.GetString("Form.WelcomeDialog._checkNext");
		}

		private void OnOK(object sender, EventArgs args) {
			if(_optNewConnection.Checked)
				_cid = CID.NewConnection;
			else if(_optCygwin.Checked)
				_cid = CID.NewCygwinConnection;
			else if(_optConvert.Checked)
				StartConvert();
		}
		protected override void OnClosed(EventArgs e) {
			base.OnClosed (e);
			GApp.Options.ShowWelcomeDialog = _checkNext.Checked;
		}


		private void StartConvert() {
			string dir = SelectDirectory();
			if(dir==null) {
				this.DialogResult = DialogResult.None;
				return;
			}

			try {
				ImportConfig(dir + "\\options.conf");
			}      
			catch(Exception ex) {
				Debug.WriteLine(ex.StackTrace);
				GUtil.Warning(this, ex.Message);
				this.DialogResult = DialogResult.None;
				return;
			}
		}
		public string SelectDirectory() {
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.ShowNewFolderButton = false;
			dlg.Description = GApp.Strings.GetString("Caption.WelcomeDialog.SelectConfigDirectory");
			string initial_dir = GuessVaraTermDir();
			if(initial_dir!=null) dlg.SelectedPath = initial_dir;
			if(GCUtil.ShowModalDialog(this, dlg)==DialogResult.OK)
				return dlg.SelectedPath;
			else
				return null;
		}

		private void ImportConfig(string filename) {
			TextReader reader = null;
			try {
				GApp.ConnectionHistory.Clear();
				reader = new StreamReader(filename, Encoding.Default);
				reader.ReadLine(); //skip header
				string line = reader.ReadLine();
				int mru_count = 0;
				while(line != null) {
					//全設定をインポートするわけではない
					if(line.EndsWith("section terminal {"))
						ImportTerminalSettings(ReadStringPair(reader));
					else if(line.EndsWith("section key-definition {"))
						ImportKeySettings(ReadStringPair(reader));
					else if(line.EndsWith("section connection {")) {
						Hashtable t = ReadStringPair(reader);
						if(t.Contains("type")) { //socks設定と混同しないように
							ImportConnectionSettings(t);
							mru_count++;
						}
					}
					line = reader.ReadLine();
				}
				GApp.Options.MRUSize = Math.Max(mru_count, 4);
				GApp.Frame.AdjustMRUMenu();
				GApp.Frame.ApplyHotKeys();
			}
			finally {
				if(reader!=null) reader.Close();
			}
		}
		private void ImportTerminalSettings(Hashtable values) {
			ContainerOptions opt = GApp.Options;
			opt.LeftAltKey  = (AltKeyAction)GUtil.ParseEnum(typeof(AltKeyAction), (string)values["left-alt"], opt.LeftAltKey);
			opt.RightAltKey = (AltKeyAction)GUtil.ParseEnum(typeof(AltKeyAction), (string)values["right-alt"], opt.RightAltKey);
			opt.AutoCopyByLeftButton = GUtil.ParseBool((string)values["auto-copy-by-left-button"], opt.AutoCopyByLeftButton);
			opt.RightButtonAction = (RightButtonAction)GUtil.ParseEnum(typeof(RightButtonAction), (string)values["right-button"], opt.RightButtonAction);
			opt.TerminalBufferSize = GUtil.ParseInt((string)values["buffer-size"], opt.TerminalBufferSize);
			string fontname = (string)values["font-family"];
			string ja_fontname = (string)values["japanese-font-family"];
			float size = GUtil.ParseFloat((string)values["font-size"], opt.FontSize);
			opt.Font = new Font(fontname, size);
			opt.JapaneseFont = new Font(ja_fontname, size);
			opt.UseClearType = GUtil.ParseBool((string)values["cleartype"], opt.UseClearType);
			opt.BGColor = GUtil.ParseColor((string)values["bg-color"], opt.BGColor);
			opt.TextColor = GUtil.ParseColor((string)values["text-color"], opt.TextColor);
			opt.ESColorSet.Load((string)values["escapesequence-color"]);
			opt.BackgroundImageFileName = (string)values["bg-image"];
			opt.ImageStyle = (ImageStyle)GUtil.ParseEnum(typeof(ImageStyle), (string)values["bg-style"], opt.ImageStyle);
			opt.DefaultLogType = (LogType)GUtil.ParseEnum(typeof(LogType), (string)values["default-log-type"], opt.DefaultLogType);
			opt.DefaultLogDirectory = (string)values["default-log-directory"];
		}
		private void ImportKeySettings(Hashtable values) {
			IDictionaryEnumerator ie = values.GetEnumerator();
			while(ie.MoveNext()) {
				string name = (string)ie.Key;
				CID cid = (CID)GUtil.ParseEnum(typeof(CID), name, CID.NOP);
				if(cid!=CID.NOP) {
					Keys k = GUtil.ParseKey(((string)ie.Value).Split(','));
					GApp.Options.Commands.ModifyKey(cid, k & Keys.Modifiers, k & Keys.KeyCode);
				}
			}
		}
		private void ImportConnectionSettings(Hashtable values) {
			ConfigNode cn = ConfigNode.CreateIndirect("", values);
			GApp.ConnectionHistory.Append(TerminalParam.CreateFromConfigNode(cn));
		}

		private Hashtable ReadStringPair(TextReader reader) {
			Hashtable r = new Hashtable();
			string line = reader.ReadLine();
			while(!line.EndsWith("}")) {
				int start = 0;
				while(line[start]=='\t') start++;
				int eq = line.IndexOf('=', start);
				if(eq!=-1) {
					string name = line.Substring(start, eq-start);
					string value = line.Substring(eq+1);
					r[name] = value;
				}
				line = reader.ReadLine();
			}
			return r;
		}

		private string GuessVaraTermDir() {
			string candidate1 = Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + "\\Terminal Emulator VaraTerm";
			string candidate2 = candidate1 + "\\" + Environment.UserName;
			if(Directory.Exists(candidate2))
				return candidate2;
			else if(Directory.Exists(candidate1))
				return candidate1;
			else
				return null;
			
		}

	}
}
