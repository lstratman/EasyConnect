/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: KeyGenWizard.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

using Granados.PKI;
using Granados.SSHCV2;

namespace Poderosa.Forms
{
	/// <summary>
	/// KeyGenWizard の概要の説明です。
	/// </summary>
	public class KeyGenWizard : System.Windows.Forms.Form
	{
		//現在のページ
		private enum Page {
			Parameter,
			Generation,
			Store
		}
		private Page _page;

		private KeyGenThread _keyGenThread;
		private SSH2UserAuthKey _resultKey;

		private System.Windows.Forms.Panel _parameterPanel;
		private System.Windows.Forms.Button _nextButton;
		public System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Label _promptLabel1;
		private System.Windows.Forms.Label _algorithmLabel;
		private System.Windows.Forms.Label _bitCountLabel;
		private ComboBox _algorithmBox;
		private ComboBox _bitCountBox;
		private System.Windows.Forms.Panel _generationPanel;
		private System.Windows.Forms.Label _keygenLabel;
		private ProgressBar _generationBar;
		private System.Windows.Forms.Panel _storePanel;
		private System.Windows.Forms.Label _completeLabel;
		private System.Windows.Forms.Button _storePrivateKey;
		private System.Windows.Forms.Button _storeSECSHPublicKeyButton;
		private System.Windows.Forms.Button _storeOpenSSHPublicKeyButton;
		private System.Windows.Forms.Label _passphraseLabel;
		private TextBox _passphraseBox;
		private TextBox _confirmBox;
		private System.Windows.Forms.Label _confirmLabel;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public KeyGenWizard()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			if(!this.DesignMode)
				this.Width = PanelPitch;

			this._confirmLabel.Text = GApp.Strings.GetString("Form.KeyGenWizard._confirmLabel");
			this._passphraseLabel.Text = GApp.Strings.GetString("Form.KeyGenWizard._passphraseLabel");
			this._bitCountLabel.Text = GApp.Strings.GetString("Form.KeyGenWizard._bitCountLabel");
			this._algorithmLabel.Text = GApp.Strings.GetString("Form.KeyGenWizard._algorithmLabel");
			this._promptLabel1.Text = GApp.Strings.GetString("Form.KeyGenWizard._promptLabel1");
			this._nextButton.Text = GApp.Strings.GetString("Form.KeyGenWizard._nextButton");
			this._cancelButton.Text = GApp.Strings.GetString("Common.Cancel");
			this._keygenLabel.Text = GApp.Strings.GetString("Form.KeyGenWizard._keygenLabel");
			this._storePrivateKey.Text = GApp.Strings.GetString("Form.KeyGenWizard._storePrivateKey");
			this._storeSECSHPublicKeyButton.Text = GApp.Strings.GetString("Form.KeyGenWizard._storeSECSHPublicKeyButton");
			this._storeOpenSSHPublicKeyButton.Text = GApp.Strings.GetString("Form.KeyGenWizard._storeOpenSSHPublicKeyButton");
			this._completeLabel.Text = GApp.Strings.GetString("Form.KeyGenWizard._completeLabel");
			this.Text = GApp.Strings.GetString("Form.KeyGenWizard.Text");

			_page=Page.Parameter;
		}
		public void SetResultKey(SSH2UserAuthKey key) {
			_resultKey = key;
		}

		private int PanelPitch {
			get {
				return _parameterPanel.Width+8;
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
			this._parameterPanel = new System.Windows.Forms.Panel();
			this._confirmBox = new TextBox();
			this._confirmLabel = new System.Windows.Forms.Label();
			this._passphraseBox = new TextBox();
			this._passphraseLabel = new System.Windows.Forms.Label();
			this._bitCountBox = new ComboBox();
			this._bitCountLabel = new System.Windows.Forms.Label();
			this._algorithmLabel = new System.Windows.Forms.Label();
			this._promptLabel1 = new System.Windows.Forms.Label();
			this._algorithmBox = new ComboBox();
			this._nextButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this._generationPanel = new System.Windows.Forms.Panel();
			this._generationBar = new ProgressBar();
			this._keygenLabel = new System.Windows.Forms.Label();
			this._storePanel = new System.Windows.Forms.Panel();
			this._storeSECSHPublicKeyButton = new System.Windows.Forms.Button();
			this._storeOpenSSHPublicKeyButton = new System.Windows.Forms.Button();
			this._storePrivateKey = new System.Windows.Forms.Button();
			this._completeLabel = new System.Windows.Forms.Label();
			this._parameterPanel.SuspendLayout();
			this._generationPanel.SuspendLayout();
			this._storePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// _parameterPanel
			// 
			this._parameterPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																						  this._confirmBox,
																						  this._confirmLabel,
																						  this._passphraseBox,
																						  this._passphraseLabel,
																						  this._bitCountBox,
																						  this._bitCountLabel,
																						  this._algorithmLabel,
																						  this._promptLabel1,
																						  this._algorithmBox});
			this._parameterPanel.Location = new System.Drawing.Point(0, 8);
			this._parameterPanel.Name = "_parameterPanel";
			this._parameterPanel.Size = new System.Drawing.Size(304, 184);
			this._parameterPanel.TabIndex = 0;
			// 
			// _confirmBox
			// 
			this._confirmBox.Location = new System.Drawing.Point(128, 128);
			this._confirmBox.Name = "_confirmBox";
			this._confirmBox.PasswordChar = '*';
			this._confirmBox.Size = new System.Drawing.Size(152, 19);
			this._confirmBox.TabIndex = 8;
			this._confirmBox.Text = "";
			// 
			// _confirmLabel
			// 
			this._confirmLabel.Location = new System.Drawing.Point(16, 128);
			this._confirmLabel.Name = "_confirmLabel";
			this._confirmLabel.TabIndex = 7;
			this._confirmLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _passphraseBox
			// 
			this._passphraseBox.Location = new System.Drawing.Point(128, 104);
			this._passphraseBox.Name = "_passphraseBox";
			this._passphraseBox.PasswordChar = '*';
			this._passphraseBox.Size = new System.Drawing.Size(152, 19);
			this._passphraseBox.TabIndex = 6;
			this._passphraseBox.Text = "";
			// 
			// _passphraseLabel
			// 
			this._passphraseLabel.Location = new System.Drawing.Point(16, 104);
			this._passphraseLabel.Name = "_passphraseLabel";
			this._passphraseLabel.TabIndex = 5;
			this._passphraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _bitCountBox
			// 
			this._bitCountBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._bitCountBox.Items.AddRange(new object[] {
															  "768",
															  "1024",
															  "2048"});
			this._bitCountBox.Location = new System.Drawing.Point(128, 80);
			this._bitCountBox.SelectedIndex = 0;
			this._bitCountBox.Name = "_bitCountBox";
			this._bitCountBox.Size = new System.Drawing.Size(121, 20);
			this._bitCountBox.TabIndex = 4;
			// 
			// _bitCountLabel
			// 
			this._bitCountLabel.Location = new System.Drawing.Point(16, 80);
			this._bitCountLabel.Name = "_bitCountLabel";
			this._bitCountLabel.TabIndex = 3;
			this._bitCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _algorithmLabel
			// 
			this._algorithmLabel.Location = new System.Drawing.Point(16, 56);
			this._algorithmLabel.Name = "_algorithmLabel";
			this._algorithmLabel.TabIndex = 1;
			this._algorithmLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _promptLabel1
			// 
			this._promptLabel1.Location = new System.Drawing.Point(8, 8);
			this._promptLabel1.Name = "_promptLabel1";
			this._promptLabel1.Size = new System.Drawing.Size(288, 40);
			this._promptLabel1.TabIndex = 0;
			// 
			// _algorithmBox
			// 
			this._algorithmBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._algorithmBox.Items.AddRange(new object[] {
															   "DSA",
															   "RSA"});
			this._algorithmBox.Location = new System.Drawing.Point(128, 56);
			this._algorithmBox.Name = "_algorithmBox";
			this._algorithmBox.SelectedIndex = 0;
			this._algorithmBox.Size = new System.Drawing.Size(121, 20);
			this._algorithmBox.TabIndex = 2;
			// 
			// _nextButton
			// 
			this._nextButton.Location = new System.Drawing.Point(224, 192);
			this._nextButton.Name = "_nextButton";
			this._nextButton.FlatStyle = FlatStyle.System;
			this._nextButton.TabIndex = 1;
			this._nextButton.Click += new System.EventHandler(this.OnNext);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(136, 192);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.FlatStyle = FlatStyle.System;
			this._cancelButton.TabIndex = 2;
			// 
			// _generationPanel
			// 
			this._generationPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this._generationBar,
																						   this._keygenLabel});
			this._generationPanel.Location = new System.Drawing.Point(312, 8);
			this._generationPanel.Name = "_generationPanel";
			this._generationPanel.Size = new System.Drawing.Size(304, 184);
			this._generationPanel.TabIndex = 5;
			this._generationPanel.Visible = false;
			// 
			// _generationBar
			// 
			this._generationBar.Location = new System.Drawing.Point(8, 80);
			this._generationBar.Maximum = 200;
			this._generationBar.Minimum = 0;
			this._generationBar.Name = "_generationBar";
			this._generationBar.Size = new System.Drawing.Size(288, 24);
			this._generationBar.Step = 1;
			this._generationBar.TabIndex = 1;
			this._generationBar.Value = 0;
			// 
			// _keygenLabel
			// 
			this._keygenLabel.Location = new System.Drawing.Point(8, 8);
			this._keygenLabel.Name = "_keygenLabel";
			this._keygenLabel.Size = new System.Drawing.Size(288, 40);
			this._keygenLabel.TabIndex = 0;
			// 
			// _storePanel
			// 
			this._storePanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this._storeSECSHPublicKeyButton,
																					  this._storeOpenSSHPublicKeyButton,
																					  this._storePrivateKey,
																					  this._completeLabel});
			this._storePanel.Location = new System.Drawing.Point(624, 8);
			this._storePanel.Name = "_storePanel";
			this._storePanel.Size = new System.Drawing.Size(304, 184);
			this._storePanel.TabIndex = 6;
			this._storePanel.Visible = false;
			// 
			// _storePrivateKey
			// 
			this._storePrivateKey.Location = new System.Drawing.Point(24, 56);
			this._storePrivateKey.Name = "_storePrivateKey";
			this._storePrivateKey.FlatStyle = FlatStyle.System;
			this._storePrivateKey.Size = new System.Drawing.Size(256, 23);
			this._storePrivateKey.TabIndex = 2;
			this._storePrivateKey.Click += new System.EventHandler(this.OnSavePrivateKey);
			// 
			// _storeSECSHPublicKeyButton
			// 
			this._storeSECSHPublicKeyButton.Location = new System.Drawing.Point(24, 96);
			this._storeSECSHPublicKeyButton.Name = "_storeSECSHPublicKeyButton";
			this._storeSECSHPublicKeyButton.FlatStyle = FlatStyle.System;
			this._storeSECSHPublicKeyButton.Size = new System.Drawing.Size(256, 23);
			this._storeSECSHPublicKeyButton.TabIndex = 3;
			this._storeSECSHPublicKeyButton.Click += new System.EventHandler(this.OnSaveSECSHPublicKey);
			// 
			// _storeOpenSSHPublicKeyButton
			// 
			this._storeOpenSSHPublicKeyButton.Location = new System.Drawing.Point(24, 136);
			this._storeOpenSSHPublicKeyButton.Name = "_storeOpenSSHPublicKeyButton";
			this._storeOpenSSHPublicKeyButton.FlatStyle = FlatStyle.System;
			this._storeOpenSSHPublicKeyButton.Size = new System.Drawing.Size(256, 23);
			this._storeOpenSSHPublicKeyButton.TabIndex = 4;
			this._storeOpenSSHPublicKeyButton.Click += new System.EventHandler(this.OnSaveOpenSSHPublicKey);
			// 
			// _completeLabel
			// 
			this._completeLabel.Location = new System.Drawing.Point(8, 8);
			this._completeLabel.Name = "_completeLabel";
			this._completeLabel.Size = new System.Drawing.Size(288, 40);
			this._completeLabel.TabIndex = 1;
			// 
			// KeyGenWizard
			// 
			this.AcceptButton = this._nextButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(930, 223);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._storePanel,
																		  this._cancelButton,
																		  this._nextButton,
																		  this._parameterPanel,
																		  this._generationPanel});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "KeyGenWizard";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this._parameterPanel.ResumeLayout(false);
			this._generationPanel.ResumeLayout(false);
			this._storePanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public ProgressBar GenerationBar {
			get {
				return _generationBar;
			}
		}
		private PublicKeyAlgorithm KeyAlgorithm {
			get {
				if(_algorithmBox.Text=="RSA")
					return PublicKeyAlgorithm.RSA;
				else
					return PublicKeyAlgorithm.DSA;
			}
		}
		private bool VerifyPassphrase() {
			if(_passphraseBox.Text!=_confirmBox.Text) {
				GUtil.Warning(this, GApp.Strings.GetString("Message.KeyGenWizard.NotMatch"));
				return false;
			}
			else if(_passphraseBox.Text.Length==0) {
				return DialogResult.Yes==GUtil.AskUserYesNo(this, GApp.Strings.GetString("Message.KeyGenWizard.ConfirmEmptyPassphrase"));
			}
			else
				return true;
		}


		private void OnNext(object sender, EventArgs args) {
			switch(_page) {
				case Page.Parameter:
					if(!VerifyPassphrase()) return;

					_parameterPanel.Visible = false;
					_generationPanel.Visible = true;
					_generationPanel.Left -= PanelPitch;
					_keyGenThread = new KeyGenThread(this, KeyAlgorithm, Int32.Parse(_bitCountBox.Text));
					this.MouseMove += new MouseEventHandler(_keyGenThread.OnMouseMove);
					_generationPanel.MouseMove += new MouseEventHandler(_keyGenThread.OnMouseMove);
					_nextButton.Enabled = false;
					_page = Page.Generation;
					_keyGenThread.Start();
					break;
				case Page.Generation:
					_generationPanel.Visible = false;
					_storePanel.Visible = true;
					_storePanel.Left -= PanelPitch*2;
					_page = Page.Store;
					_nextButton.Text = GApp.Strings.GetString("Message.KeyGenWizard.Finish");
					break;
				case Page.Store:
					Close();
					break;
			}
		}

		protected override void WndProc(ref Message msg) {
			base.WndProc(ref msg);
			if(msg.Msg==GConst.WMG_KEYGEN_PROGRESS) {
				_generationBar.Value = msg.LParam.ToInt32();
			}
			else if(msg.Msg==GConst.WMG_KEYGEN_FINISHED) {
				CheckGenerationComplete();
			}
		}
		protected override void OnClosed(EventArgs args) {
			if(_keyGenThread!=null) _keyGenThread.SetAbortFlag();
			base.OnClosed(args);
		}

		public void SetProgressValue(int v) {
			_generationBar.Value = v;
			if(v==_generationBar.Maximum) {
				_keygenLabel.Text = GApp.Strings.GetString("Message.KeyGenWizard.RandomNumberCompleted");
				this.Cursor = Cursors.WaitCursor;
				CheckGenerationComplete();
			}
		}
		private void CheckGenerationComplete() {
			//プログレスバーが終端にいくのと、鍵の生成が終わるのは両方満たさないといけない
			if(_generationBar.Value==_generationBar.Maximum && _resultKey!=null) {
				_nextButton.Enabled = true;
				_keygenLabel.Text = GApp.Strings.GetString("Message.KeyGenWizard.GenerationCompleted");
				this.Cursor = Cursors.Default;
			}
		}


		private void OnSavePrivateKey(object sender, EventArgs args) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.InitialDirectory = GApp.Options.DefaultKeyDir;
			dlg.Title = GApp.Strings.GetString("Caption.KeyGenWizard.SavePrivateKey");
			if(GCUtil.ShowModalDialog(this, dlg)==DialogResult.OK) {
				GApp.Options.DefaultKeyDir = GUtil.FileToDir(dlg.FileName);
				try {
					string pp = _passphraseBox.Text;
					if(pp.Length==0) pp = null; //空パスフレーズはnull指定

					_resultKey.WritePrivatePartInSECSHStyleFile(new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write), "", pp);
				}
				catch(Exception ex) {
					GUtil.Warning(this, String.Format(GApp.Strings.GetString("Message.KeyGenWizard.KeySaveError"), ex.Message));
				}
			}
		}
		private void OnSaveSECSHPublicKey(object sender, EventArgs args) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.InitialDirectory = GApp.Options.DefaultKeyDir;
			dlg.Title = GApp.Strings.GetString("Caption.KeyGenWizard.SavePublicInSECSH");
			dlg.DefaultExt = "pub";
			dlg.Filter = "SSH Public Key(*.pub)|*.pub|All Files(*.*)|*.*";
			if(GCUtil.ShowModalDialog(this, dlg)==DialogResult.OK) {
				GApp.Options.DefaultKeyDir = GUtil.FileToDir(dlg.FileName);
				try {
					_resultKey.WritePublicPartInSECSHStyle(new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write), "");
				}
				catch(Exception ex) {
					GUtil.Warning(this, String.Format(GApp.Strings.GetString("Message.KeyGenWizard.KeySaveError"), ex.Message));
				}
			}
		}
		private void OnSaveOpenSSHPublicKey(object sender, EventArgs args) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.InitialDirectory = GApp.Options.DefaultKeyDir;
			dlg.Title = GApp.Strings.GetString("Caption.KeyGenWizard.SavePublicInOpenSSH");
			dlg.DefaultExt = "pub";
			dlg.Filter = "SSH Public Key(*.pub)|*.pub|All Files(*.*)|*.*";
			if(GCUtil.ShowModalDialog(this, dlg)==DialogResult.OK) {
				GApp.Options.DefaultKeyDir = GUtil.FileToDir(dlg.FileName);
				try {
					_resultKey.WritePublicPartInOpenSSHStyle(new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write));
				}
				catch(Exception ex) {
					GUtil.Warning(this, String.Format(GApp.Strings.GetString("Message.KeyGenWizard.KeySaveError"), ex.Message));
				}
			}
		}
	}

	/*
	 * いくつか試したところ、鍵作成にいくつの乱数が必要かはかなりばらつきがある。そこで次のようにする。
	 * 1. MouseMove100回を必ず受信する。
	 * 2. １回のMouseMoveにつき100個の乱数を計算する。100個消費したら次のMouseMoveが来るまでブロック。
	 * 4. 途中で鍵作成が終了しても、100個のMouseMoveが来るまではUI上は生成をしているふりをする。
	 */

	internal class KeyGenThread {
		private KeyGenWizard _parent;
		private PublicKeyAlgorithm _algorithm;
		private int _bitCount;
		private KeyGenRandomGenerator _rnd;
		private int _mouseMoveCount;

		public KeyGenThread(KeyGenWizard p, PublicKeyAlgorithm a, int b) {
			_parent = p;
			_algorithm = a;
			_bitCount = b;
			_rnd = new KeyGenRandomGenerator();
		}

		public void Start() {
			GUtil.CreateThread(new ThreadStart(EntryPoint)).Start();
		}
		public void SetAbortFlag() {
			_rnd.SetAbortFlag();			
		}

		private void EntryPoint() {
			try {
				_mouseMoveCount = 0;
				KeyPair kp;
				if(_algorithm==PublicKeyAlgorithm.DSA)
					kp = Granados.PKI.DSAKeyPair.GenerateNew(_bitCount, _rnd);
				else
					kp = Granados.PKI.RSAKeyPair.GenerateNew(_bitCount, _rnd);
				_parent.SetResultKey(new SSH2UserAuthKey(kp));
				Win32.PostMessage(_parent.Handle, GConst.WMG_KEYGEN_FINISHED, IntPtr.Zero, IntPtr.Zero);
			}
			catch(Exception ex) {
				Debug.WriteLine(ex.StackTrace);
				
			}
		}

		//これはフォームのスレッドで実行される。注意！
		public void OnMouseMove(object sender, MouseEventArgs args) {
			
			if(_mouseMoveCount==_parent.GenerationBar.Maximum) return;
			
			int n = (int)System.DateTime.Now.Ticks;
			n ^= (args.X << 16);
			n ^= args.Y;
			n ^= (int)0x31031293; //これぐらいやれば十分ばらけるだろう

			if(++_mouseMoveCount==_parent.GenerationBar.Maximum)
				_rnd.RefreshFinal(n);
			else
				_rnd.Refresh(n);

			_parent.SetProgressValue(_mouseMoveCount);

		}

		private class KeyGenRandomGenerator : Random {
			private Random _internal;
			public int _doubleCount;
			private int _internalAvailableCount;
			private bool _abortFlag;
		
			public KeyGenRandomGenerator() {
				_internalAvailableCount = 0;
				_abortFlag = false;
			}

			public override double NextDouble() {

				while(_internalAvailableCount==0) {
					Thread.Sleep(100); //同期オブジェクトを使うまでもないだろう
					if(_abortFlag) throw new Exception("key generation aborted");
				}

				_internalAvailableCount--;
				_doubleCount++;
				return _internal.NextDouble();
			}
			//他はoverrideしない

			public void Refresh(int seed) {
				_internal = new Random(seed);
				_internalAvailableCount = 50;
			}
			public void RefreshFinal(int seed) {
				_internal = new Random(seed);
				_internalAvailableCount = Int32.MaxValue;
			}

			public void SetAbortFlag() {
				_abortFlag = true;
			}
		}
	}
}
