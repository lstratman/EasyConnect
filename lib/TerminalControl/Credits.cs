/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: Credits.cs,v 1.3 2005/04/27 23:27:18 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Poderosa.UI;
using Poderosa.Config;

namespace Poderosa.Forms
{
	/// <summary>
	/// Credits の概要の説明です。
	/// </summary>
	internal class Credits : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox _pictureBox;
		private System.Windows.Forms.Label _subtitle;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Label _mainPanel;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Credits()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();
			_okButton.Text = GApp.Strings.GetString("Common.OK");

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			//AboutBoxから借りる
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AboutBox));
			this._pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("_pictureBox.Image")));
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
				_timer.Dispose();
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
			this._pictureBox = new System.Windows.Forms.PictureBox();
			this._subtitle = new System.Windows.Forms.Label();
			this._okButton = new System.Windows.Forms.Button();
			this._mainPanel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _pictureBox
			// 
			this._pictureBox.BackColor = System.Drawing.SystemColors.Window;
			this._pictureBox.Location = new System.Drawing.Point(0, 0);
			this._pictureBox.Name = "_pictureBox";
			this._pictureBox.Size = new System.Drawing.Size(288, 80);
			this._pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this._pictureBox.TabIndex = 0;
			this._pictureBox.TabStop = false;
			// 
			// _subtitle
			// 
			this._subtitle.BackColor = System.Drawing.SystemColors.Window;
			this._subtitle.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this._subtitle.Location = new System.Drawing.Point(0, 80);
			this._subtitle.Name = "_subtitle";
			this._subtitle.Size = new System.Drawing.Size(288, 24);
			this._subtitle.TabIndex = 1;
			this._subtitle.Text = "The Terminal, Reloaded";
			this._subtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._okButton.Location = new System.Drawing.Point(104, 240);
			this._okButton.Name = "_okButton";
			this._okButton.TabIndex = 2;
			// 
			// _mainPanel
			// 
			this._mainPanel.BackColor = System.Drawing.SystemColors.Window;
			this._mainPanel.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(128)));
			this._mainPanel.Location = new System.Drawing.Point(0, 104);
			this._mainPanel.Name = "_mainPanel";
			this._mainPanel.Size = new System.Drawing.Size(288, 128);
			this._mainPanel.TabIndex = 3;
			this._mainPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintCredit);
			// 
			// Credits
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(290, 266);
			this.ControlBox = false;
			this.Controls.Add(this._mainPanel);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._subtitle);
			this.Controls.Add(this._pictureBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "Credits";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Poderosa";
			this.ResumeLayout(false);

		}
		#endregion

		private class Entry {
			public string _en_name;
			public string _ja_name;
			public Entry(string en, string ja) {
				_en_name = en;
				_ja_name = ja;
			}
		}
		private class CreditGroup {
			public string _name;
			public Entry[] _credits;
			
			public CreditGroup(string name, params Entry[] credits) {
				_name = name;
				_credits = credits;
			}
		}

		private ArrayList _creditGroups;
		
		private void CreateCreditData() {
			_creditGroups = new ArrayList();
			_creditGroups.Add(new CreditGroup(
				"",
				new Entry[0]));
			_creditGroups.Add(new CreditGroup(
                "Project Leader & Chief Developer",
				new Entry("Daisuke OKAJIMA", "岡嶋 大介")));
			_creditGroups.Add(new CreditGroup(
                "Website Manager",
				new Entry("Hiroshi Taketazu", "Hiroshi Taketazu"),
				new Entry("Tadashi \"ELF\" Jokagi", "Tadashi \"ELF\" Jokagi")));
			_creditGroups.Add(new CreditGroup(
                "Server Administrator",
				new Entry("yuk@lavans", "yuk@lavans")));
			_creditGroups.Add(new CreditGroup(
				"Developer",
				new Entry("Shintaro UNO", "宇野 信太郎")));
			_creditGroups.Add(new CreditGroup(
				"Special Thanks To",
				new Entry("Routrek Networks, Inc.", "(株)ルートレック・ネットワークス")));
			_creditGroups.Add(new CreditGroup(
				"Poderosa Project",
				new Entry("http://www.poderosa.org/", "http://www.poderosa.org/")));
		}
		
		protected override void OnLoad(EventArgs e) {
			base.OnLoad (e);
			if(!this.DesignMode) {
				CreateCreditData();
				_creditIndex = 0;
				_creditStep = 0;
				_boldFont = new Font(_mainPanel.Font, FontStyle.Bold);

				_timer = new Timer();
				_timer.Tick += new EventHandler(OnTimer);
				_timer.Interval = 50;
				_timer.Start();
			}
		}

		private Font _boldFont;
		private int _creditIndex;
		private int _creditStep;
		private const int STEPS_PER_GROUP = 60;
		private Timer _timer;

		private void OnTimer(object sender, EventArgs args) {
			if(_creditIndex==0) {
				//最初の表示を出すまでにやや間を空ける
				if(++_creditStep==30) {
					_creditIndex++;
					_creditStep = 0;
				}	
			}
			else if(_creditIndex==_creditGroups.Count-1) {
				if(++_creditStep==20) {
					_timer.Stop();
				}
			}
			else if(++_creditStep==STEPS_PER_GROUP) {
				_creditIndex++;
				_creditStep = 0;
			}
			_mainPanel.Invalidate(true);
		}
		private void OnPaintCredit(object sender, PaintEventArgs args) {
			if(_creditIndex==_creditGroups.Count) return;

			CreditGroup grp = (CreditGroup)_creditGroups[_creditIndex];

			Color col;
			if(_creditStep < 10)
				col = ColorUtil.CalculateColor(SystemColors.WindowText, SystemColors.Window, _creditStep*(255/10));
			else if(_creditStep < 40)
				col = SystemColors.WindowText;
			else if(_creditStep < 50)
				col = ColorUtil.CalculateColor(SystemColors.WindowText, SystemColors.Window, (50-_creditStep)*(255/10));
			else
				return; //no draw

			Graphics g = args.Graphics;
			SizeF name_size = g.MeasureString(grp._name, _boldFont);
			Brush br = new SolidBrush(col);
			float y = (_mainPanel.Height - (name_size.Height*(1+grp._credits.Length))) / 2;
			float width = this.Width;
			DrawString(g, grp._name, _boldFont, br, y);
			y += name_size.Height;
			foreach(Entry e in grp._credits) {
				DrawString(g, GEnv.Options.Language==Language.English? e._en_name : e._ja_name, _mainPanel.Font, br, y);
				y += name_size.Height;
			}
			br.Dispose();
		}

		private void DrawString(Graphics g, string text, Font font, Brush br, float y) {
			SizeF sz = g.MeasureString(text, font);
			g.DrawString(text, font, br, (_mainPanel.Width-sz.Width)/2, y);
		}
	}
}
