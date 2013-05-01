/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TabBar.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Communication;
using Poderosa.Terminal;
using Poderosa.Config;
using Poderosa.UI;

namespace Poderosa.Forms {
	/// <summary>
	/// TabBar の概要の説明です。
	/// </summary>
	internal class TabBar : UserControl {
		private ToolTip _tabToolTip;
		private const int UNITHEIGHT = 25; //!!これはフォントからちゃんと計算しないといけないだろう
		private const int SCROLLBUTTON_SIZE = 18;
		private const int BUTTON_MARGIN = 4;

		private System.ComponentModel.IContainer components;

		private int _scrollStep; //スクロールボタンを押したときのアニメーションに使うカウント
		private const int ANIMATION_COUNT = 15;

		//各Tabを格納するコレクション

		//スクロールスタイルであっても、必要なボタンは常に存在する。位置とVisibleの調整だけである。

		private Color _activeTabColor;
		private Font  _basicFont;
		private Font  _activeTabFont;
		private Bitmap _tabIconDefault;
		private Bitmap _tabIconSerial;
		private Bitmap _tabIconCygwin;
		private Bitmap _tabIconSFU;

		private TabBarScrollButton _leftScrollButton;
		private TabBarScrollButton _rightScrollButton;
		private int _scrollButtonOffset;

		public TabBar() {
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			Color c = SystemColors.Control;
			_activeTabColor = Color.FromArgb((c.R+255)/2, (c.G+255)/2, (c.B+255)/2); //白との中間をとる
			_basicFont = (Font)this.Font.Clone();
			_activeTabFont = new Font(_basicFont, _basicFont.Style|FontStyle.Bold);
			_tabToolTip = new ToolTip();

			if(GApp.Options.TabBarStyle==TabBarStyle.ScrollButton)
				InitScrollButtons();
		}
		private Bitmap GetTabIcon(ConnectionTag tag) {
			if(_tabIconDefault==null) {
				_tabIconDefault = (Bitmap)IconList.LoadIcon(IconList.ICON_NEWCONNECTION);
				_tabIconSerial  = (Bitmap)IconList.LoadIcon(IconList.ICON_SERIAL);
				_tabIconCygwin  = (Bitmap)IconList.LoadIcon(IconList.ICON_CYGWIN);
				_tabIconSFU     = (Bitmap)IconList.LoadIcon(IconList.ICON_SFU);
			}
			if(tag.Connection.Param is SFUTerminalParam)
				return _tabIconSFU;
			else if(tag.Connection.Param is CygwinTerminalParam)
				return _tabIconCygwin;
			else if(tag.Connection.Param is SerialTerminalParam)
				return _tabIconSerial;
			else
				return _tabIconDefault;
		}
		private void InitScrollButtons() {
			_leftScrollButton = new TabBarScrollButton();
			_leftScrollButton._isRight = false;
			_leftScrollButton.Enabled = false;
			_leftScrollButton.Width = SCROLLBUTTON_SIZE;
			_leftScrollButton.Height = SCROLLBUTTON_SIZE;
			_leftScrollButton.BringToFront();
			_leftScrollButton.Click += new EventHandler(OnLeftScrollButtonClicked);
			this.Controls.Add(_leftScrollButton);
			_rightScrollButton = new TabBarScrollButton();
			_rightScrollButton._isRight = true;
			_rightScrollButton.Enabled = false;
			_rightScrollButton.Width = SCROLLBUTTON_SIZE;
			_rightScrollButton.Height = SCROLLBUTTON_SIZE;
			_rightScrollButton.BringToFront();
			_rightScrollButton.Click += new EventHandler(OnRightScrollButtonClicked);
			this.Controls.Add(_rightScrollButton);
		}

		public void AddTab(ConnectionTag ct) {
			Control b = CreateNewButton(ct);
			Controls.Add(b);
			if(GApp.Options.TabBarStyle==TabBarStyle.ScrollButton) {
				int width  = GetTabAreaWidth();
				int index = GEnv.Connections.Count;
				while(width > 0 && index>0) {
					index--;
					width -= GetNecessaryButtonWidth(GEnv.Connections.TagAt(index));
				}
				index++;
				if(index>=GEnv.Connections.Count) index = GEnv.Connections.Count-1;
				if(index<0) index=0;
				_scrollButtonOffset = index;
			}
			ArrangeButtons();
		}

		public void RemoveTab(ConnectionTag ct) {
			if(ct!=null) {
				int n = GEnv.Connections.IndexOf(ct);
				if(n==_scrollButtonOffset) _scrollButtonOffset--;
				if(_scrollButtonOffset<0) _scrollButtonOffset=0;

				if(ct.Button!=null) Controls.Remove(ct.Button);
				ArrangeButtons();
			}
		}

		public void Clear() {
			Controls.Clear();
			if(GApp.Options.TabBarStyle==TabBarStyle.ScrollButton) {
				InitScrollButtons();
				ArrangeButtons();
			}
		}

		public void RefreshConnection(ConnectionTag tag) {
			if(tag.Button==null) return;

			SetButtonText(tag.Button as TabBarButton, GEnv.Connections.IndexOf(tag), tag);
			((TabBarButton)tag.Button).Image = GetTabIcon(tag);
			
			_tabToolTip.SetToolTip(tag.Button, tag.FormatFrameText());
		}

				
		private Control CreateNewButton(ConnectionTag ct) {
			TabBarButton b = new TabBarButton();
			ct.Button = b;
			SetButtonText(b, Controls.Count-1, ct);
			_tabToolTip.SetToolTip(b, ct.FormatFrameText());
			//b.Image = _imageList.Images[(int)e.item.Type];

			b.Tag = ct;
			b.Font = _basicFont;
			b.Width = GetNecessaryButtonWidth(b); 
			b.Visible = true;
			b.TabStop = false;
			b.Click += new EventHandler(this.OnButtonClick);
			b.MouseDown += new MouseEventHandler(OnMouseDown);
			b.MouseUp += new MouseEventHandler(OnMouseUp);
			b.MouseMove += new MouseEventHandler(OnMouseMove);
			return b;
		}
		private int GetNecessaryButtonWidth(Control b) {
			return (int)b.CreateGraphics().MeasureString(b.Text, _activeTabFont).Width + 37;//37はアイコン、インデクス、左右マージンの合計
		}
		private int GetNecessaryButtonWidth(ConnectionTag ct) {
			return (int)ct.Button.CreateGraphics().MeasureString((GEnv.Connections.IndexOf(ct)+1).ToString()+ct.FormatTabText(), _activeTabFont).Width + 37;//37はアイコン、インデクス、左右マージンの合計
		}
		private int GetTabAreaWidth() {
			int b = this.Width-4;
			if(GApp.Options.TabBarStyle==TabBarStyle.ScrollButton)
				b -= SCROLLBUTTON_SIZE*2;
			return b;
		}

		private void OnButtonClick(object sender, EventArgs args) {
			TabBarButton b = (TabBarButton)sender;
			GApp.GlobalCommandTarget.ActivateConnection(((ConnectionTag)b.Tag).Connection);
		}
		private void OnMouseUp(object sender, MouseEventArgs args) {
			if(args.Button!=MouseButtons.Right) return;

			TabBarButton b = (TabBarButton)sender;
			int tx = this.Left + b.Left;
			int ty = this.Top + b.Top;
			ConnectionTag t = (ConnectionTag)b.Tag;
			Debug.Assert(t!=null);
			GApp.Frame.CommandTargetConnection = t.Connection;
			//メニューのUI調整
			GApp.Frame.AdjustContextMenu(true, t.Connection);
			GApp.Frame.ContextMenu.Show(GApp.Frame, new Point(tx+args.X, ty+args.Y)); //ボタンやタブバーをコンテナにするとキーボードで選択できなくなる
		}

		//ただクリックしただけでMouseMoveが発生してしまうので、正しくドラッグ開始を判定できない
		private int _dragStartPosX;
		private int _dragStartPosY;
		private void OnMouseDown(object sender, MouseEventArgs args) {
			if(args.Button!=MouseButtons.Left) return;
			_dragStartPosX = args.X;
			_dragStartPosY = args.Y;
		}
		private void OnMouseMove(object sender, MouseEventArgs args) {
			if(args.Button!=MouseButtons.Left) return;

			if(Math.Abs(_dragStartPosX-args.X) + Math.Abs(_dragStartPosY-args.Y) >= 3) {
				object tag = ((TabBarButton)sender).Tag;
				this.DoDragDrop(tag, DragDropEffects.Move);
				TabBarButton btn = sender as TabBarButton;
				if(btn!=null) btn.Reset();
			}
		}
		private void OnLeftScrollButtonClicked(object sender, EventArgs args) {
			_scrollButtonOffset--;
			_scrollStep = ANIMATION_COUNT-1;
			_leftScrollButton.Enabled = false;
			_rightScrollButton.Enabled = false;

			int w = GEnv.Connections.TagAt(_scrollButtonOffset).Button.Width;
			ArrangeButtonsForScrollStyle(true, -(w * _scrollStep / ANIMATION_COUNT));
			
			Timer t = new Timer();
			t.Interval = 20;
			t.Tick += new EventHandler(OnLeftScrollAnimation);
			t.Start();
		}
		private void OnRightScrollButtonClicked(object sender, EventArgs args) {
			_scrollStep = ANIMATION_COUNT-1;
			_leftScrollButton.Enabled = false;
			_rightScrollButton.Enabled = false;

			int w = GEnv.Connections.TagAt(_scrollButtonOffset).Button.Width;
			ArrangeButtonsForScrollStyle(true, -(w * (ANIMATION_COUNT-_scrollStep) / ANIMATION_COUNT));
			
			Timer t = new Timer();
			t.Interval = 20;
			t.Tick += new EventHandler(OnRightScrollAnimation);
			t.Start();

			//_scrollButtonOffset++;
			//ArrangeButtons();
		}
		private void OnLeftScrollAnimation(object sender, EventArgs args) {
			_scrollStep--;
			if(_scrollStep==0) {
				((Timer)sender).Stop();
				ArrangeButtonsForScrollStyle(false, 0);
			}
			else {
				int w = GEnv.Connections.TagAt(_scrollButtonOffset).Button.Width;
				ArrangeButtonsForScrollStyle(_scrollStep>0, -(w * _scrollStep / ANIMATION_COUNT));
			}
		}
		private void OnRightScrollAnimation(object sender, EventArgs args) {
			_scrollStep--;
			if(_scrollStep==0) {
				_scrollButtonOffset++;
				((Timer)sender).Stop();
				ArrangeButtonsForScrollStyle(false, 0);
			}
			else {
				int w = GEnv.Connections.TagAt(_scrollButtonOffset).Button.Width;
				ArrangeButtonsForScrollStyle(true, -(w * (ANIMATION_COUNT-_scrollStep) / ANIMATION_COUNT));
			}
		}

		public void SetActiveTab(ConnectionTag active) {
			foreach(ConnectionTag ct in GEnv.Connections) {
				TabBarButton b = ct.Button as TabBarButton;

				if(b == null) {
					continue;
				}

				if(active==ct) {
					b.BackColor = _activeTabColor;
					b.Font = _activeTabFont;
					b.Selected = true;
					if(GApp.Options.TabBarStyle==TabBarStyle.ScrollButton) {
						if(!b.Visible) {
							int index = GEnv.Connections.IndexOf(ct);
							if(index!=-1) EnsureButtonVisible(index);
						}
					}
				}
				else {
					b.BackColor = SystemColors.Control;
					b.Font = _basicFont;
					b.Selected = false;
				}
				b.Invalidate();
			}
		}

		//スクロールスタイルにおいて、見えていないボタンが見える位置までもっていく
		private void EnsureButtonVisible(int index) {
			if(index < _scrollButtonOffset) {
				_scrollButtonOffset = index;
			}
			else {
				int width  = GetTabAreaWidth() - 30;
				while(width > 0 && index>=0) {
					width -= GetNecessaryButtonWidth(GEnv.Connections.TagAt(index--));
				}

				index++;
				if(index>=GEnv.Connections.Count) index = GEnv.Connections.Count-1;
				_scrollButtonOffset = index;
			}
			ArrangeButtons();
		}
		//順番の入れ替え
		public void ReorderButton(int index, int new_index, ConnectionTag active_tag) {
			this.Controls.SetChildIndex(this.Controls[index], new_index);
			ArrangeButtons();
			SetActiveTab(active_tag);
		}

		//ボタンの再配置
		public void ArrangeButtons() {
			if(GApp.Options.TabBarStyle==TabBarStyle.ScrollButton)
				ArrangeButtonsForScrollStyle(false, 0);
			else
				ArrangeButtonsForMultiRowStyle();
		}
		public void ApplyOptions(ContainerOptions opt) {
			if(opt.TabBarStyle==TabBarStyle.ScrollButton)
				ArrangeButtonsForScrollStyle(false, 0);
			else
				ArrangeButtonsForMultiRowStyle();
		}

		private void ArrangeButtonsForMultiRowStyle() {
			if(_leftScrollButton!=null) {
				this.Controls.Remove(_leftScrollButton);
				this.Controls.Remove(_rightScrollButton);
				_leftScrollButton = _rightScrollButton = null;
			}

			int x = 2;
			int y = 3;
			int i = 0;
			foreach(Control c in Controls) {
				TabBarButton button = c as TabBarButton;
				if(button==null) continue;

				SetButtonText(button, i, (ConnectionTag)button.Tag);
				if(x + button.Width >= this.Width) {
					x = 2;
					y += UNITHEIGHT;
				}
				button.Left = x;
				button.Width = GetNecessaryButtonWidth((ConnectionTag)button.Tag);
				x += button.Width + BUTTON_MARGIN;
				button.Top = y;
				button.Visible = true;
				button.Height = UNITHEIGHT - 4;
				button.Invalidate();
				i++;
			}
			this.Height = y + UNITHEIGHT;
		}
		private void ArrangeButtonsForScrollStyle(bool animation, int animation_offset) {
			if(_leftScrollButton==null) InitScrollButtons();

			for(int i=0; i<_scrollButtonOffset; i++) GEnv.Connections.TagAt(i).Button.Visible = false;
			int x = 2;
			int y = 3;
			int limit = x + GetTabAreaWidth();
			x += animation_offset;
			int offset = _scrollButtonOffset;
			int index = offset;
			while(offset < GEnv.Connections.Count) {
				ConnectionTag ct = GEnv.Connections.TagAt(offset);
				if(!ct.IsTerminated) {
					TabBarButton button = (TabBarButton)ct.Button;
					button.Left = x;
					button.Width = GetNecessaryButtonWidth(ct);
					SetButtonText(button, index, ct);
					if(x > limit && offset>_scrollButtonOffset) break; //少なくとも一つはボタンを表示する
					button.Top = y;
					button.Visible = true;
					button.Height = UNITHEIGHT - 4;
					x += button.Width + BUTTON_MARGIN;
					index++;
				}
				offset++;
			}
			
			for(int i=offset; i<GEnv.Connections.Count; i++) GEnv.Connections.TagAt(i).Button.Visible = false;
			
			_leftScrollButton.Left = this.Width-SCROLLBUTTON_SIZE*2;
			_leftScrollButton.Top  = y+2;
			_leftScrollButton.BringToFront();
			_leftScrollButton.Enabled = !animation && _scrollButtonOffset>0;
			_rightScrollButton.Left = this.Width-SCROLLBUTTON_SIZE;
			_rightScrollButton.Top  = y+2;
			_rightScrollButton.BringToFront();
			_rightScrollButton.Enabled = !animation && offset<GEnv.Connections.Count;

			//幅をふやしていったときなど、スペースに余裕があるなら表示を拡大
			if(!animation && _scrollButtonOffset>0 && GEnv.Connections.TagAt(_scrollButtonOffset-1).Button.Width+BUTTON_MARGIN<limit-x) {
				_scrollButtonOffset--;
				ArrangeButtonsForScrollStyle(false, 0);
			}
			this.Height = y + UNITHEIGHT;
		}

		private void SetButtonText(TabBarButton btn, int index, ConnectionTag tag) {
			btn.HeadText = (index+1).ToString();
			string t = tag.FormatTabText();
			btn.Text = (t==null || t.Length==0)? " " : t; //テキストなしだとHeadTextもなくなってしまうのでやむなく回避
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
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
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			// 
			// TabBar
			// 
			this.Name = "TabBar";
			this.Size = new System.Drawing.Size(292, 248);
			this.TabStop = false;

		}
		#endregion

		protected override void OnGotFocus(EventArgs args) {
			base.OnGotFocus(args);
			//GApp.GlobalCommandTarget.SetFocusToActiveConnection();
		}
		protected override void OnPaint(PaintEventArgs arg) {
			base.OnPaint(arg);
			//上に区切り線を引く
			Graphics g = arg.Graphics;
			Pen p = new Pen(Color.FromKnownColor(KnownColor.WindowFrame));
			g.DrawLine(p, 0, 0, Width, 0);
			p = new Pen(Color.FromKnownColor(KnownColor.Window));
			g.DrawLine(p, 0, 1, Width, 1);

		}
	}

	internal class TabBarScrollButton : Button {

		/* この形
			*
			**
			***
			**
			*
		 */ 
		internal bool _isRight; //右向きのときtrue

		public TabBarScrollButton() {
			this.Image = null;
			this.Text = "";
		}

		protected override void OnPaint(PaintEventArgs args) {
			base.OnPaint(args);

			KnownColor col = this.Enabled? KnownColor.ControlText : KnownColor.ControlDark;
			Pen p = new Pen(Color.FromKnownColor(col));

			Graphics g = args.Graphics;
			int x = this.Width/2 + (_isRight? -1 : 1);
			int y = this.Height/2-4;
			for(int i=0; i<4; i++) { //縦線を描画
				int len = 7-i*2;
				g.DrawLine(p, x, y+i, x, y+i+len);
				x += _isRight? 1 : -1;
			}
		}
	}
}