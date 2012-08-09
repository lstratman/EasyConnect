/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: MultiPaneControl.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Poderosa.Connection;
using Poderosa.Communication;
using Poderosa.Terminal;
using Poderosa.Config;
using Poderosa.Text;

namespace Poderosa.Forms
{
	/// <summary>
	/// 複数個のTerminalPaneをホストするコントロール
	/// </summary>
	internal class MultiPaneControl : UserControl
	{
		private Splitter[] _splitters;
		public TerminalPane[] _panes;
		private double[][] _splitterRatio; //[２分割用/３分割用][index]で指定

		private bool _ignoreResize;

		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MultiPaneControl() {
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.UserPaint|ControlStyles.DoubleBuffer, true);

			_ignoreResize = false;
			_splitters = new Splitter[2];
			_splitterRatio = new double[2][];
			_splitterRatio[0] = new double[] { 0.5 };
			_splitterRatio[1] = new double[] { 0.33, 0.66 };
			_panes = new TerminalPane[3];
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
			// 
			// MultiPaneControl
			// 
			this.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.Name = "MultiPaneControl";
		
		}
		#endregion


		public void InitUI(ContainerOptions prev, ContainerOptions opt) {
			ConnectionTag[] cons = new ConnectionTag[_panes.Length];
			for(int i=0; i<_panes.Length; i++) {
				cons[i] = (_panes[i]==null || !_panes[i].FakeVisible)? null : _panes[i].ConnectionTag;
				if(_panes[i]!=null) _panes[i].Detach();
				_panes[i] = null;
			}

			Controls.Clear();

			GFrameStyle style = opt.FrameStyle;
			int pane_count = StyleToPaneCount(style);
			int prev_pane_count = prev==null? 1 : StyleToPaneCount(prev.FrameStyle);
			bool is_vertical = style==GFrameStyle.DivVertical || style==GFrameStyle.DivVertical3;

			//Controlの初期化
			this.SuspendLayout();
			for(int i = pane_count-1; i>=0; i--) {
				TerminalPane p = new TerminalPane();
				_panes[i] = p;
				p.Visible = true;
				p.Dock = i==pane_count-1? DockStyle.Fill : is_vertical? DockStyle.Left : DockStyle.Top;
				if(i<pane_count-1) {
					int a = (int)((is_vertical? this.Width : this.Height) * (i==0? 0 : _splitterRatio[pane_count-2][i-1]));
					int b = (int)((is_vertical? this.Width : this.Height) * (_splitterRatio[pane_count-2][i] - (i==0? 0 : _splitterRatio[pane_count-2][i-1])));
					if(is_vertical) {
						p.Left = a;
						p.Width = b;
					}
					else {
						p.Top = a;
						p.Height = b;
					}
				}
				this.Controls.Add(p);

				if(i>0) {
					Splitter s = new Splitter();
					_splitters[i-1] = s;
					s.SplitterMoving += new SplitterEventHandler(this.OnSplitterMoving);
					s.SplitterMoved  += new SplitterEventHandler(this.OnSplitterMoved);
					s.Dock = is_vertical? DockStyle.Left : DockStyle.Top;
					s.BorderStyle = BorderStyle.Fixed3D;
					s.MinSize = 8;
					s.SplitPosition = (int)((is_vertical? this.Width : this.Height) * _splitterRatio[pane_count-2][i-1]);
					this.Controls.Add(s);
				}
			}
			this.ResumeLayout(true);


			//必要なものをAttach
			foreach(ConnectionTag ct in GEnv.Connections.OrderedConnections) {
				int pos = ct.PositionIndex;
				if(prev_pane_count<pane_count && ct.PreservedPositionIndex>=prev_pane_count) { //増えたペインへの強制割り当て
					pos = ct.PreservedPositionIndex;
					if(pos >= pane_count) pos = pane_count-1;
					ct.PositionIndex = pos; 
					if(_panes[pos].ConnectionTag==null) {
						_panes[pos].Attach(ct);
						_panes[pos].FakeVisible = true;
						GEnv.Frame.RefreshConnection(ct);
					}
				}
				else if(pos < pane_count) { //平和な場合
					if(_panes[pos].ConnectionTag==null) {
						_panes[pos].Attach(ct);
						_panes[pos].FakeVisible = true;
						GEnv.Frame.RefreshConnection(ct);
					}
				}
				else { //隠れる場合
					ct.PositionIndex = pane_count-1;
					if(ct!=null && _panes[pane_count-1].ConnectionTag==null) {
						_panes[pane_count-1].Attach(ct);
						_panes[pane_count-1].FakeVisible = true;
						GEnv.Frame.RefreshConnection(ct);
					}
				}
			}
		}

		public void RemoveAllConnections() {
			for(int i=0; i<_panes.Length; i++) {
				if(_panes[i]!=null) {
					_panes[i].Detach();
					_panes[i].FakeVisible = false;
				}
			}
		}

		//
		public void ActivateConnection(ConnectionTag ct) {
			//ct.PaneType = CalcPaneType(ct);
			TerminalPane pane = GetPane(ct.PositionIndex);
			//既にコネクションがあればそれのUIをリフレッシュ
			if(pane.Connection!=null) {
				ConnectionTag k = GEnv.Connections.FindTag(pane.Connection);
				if(k!=null) { //ここで既に閉じた接続が得られてしまうことがある。本当は接続を閉じるときに参照を解消すべきだが手抜き
					pane.Detach();
					GEnv.Frame.RefreshConnection(k);
				}
			}

			pane.FakeVisible = true;
			pane.Attach(ct);
			if(!pane.AsControl().Focused)
				pane.AsControl().Focus();
			GEnv.Frame.RefreshConnection(ct);
		}


		public bool MoveActivePane(Keys direction) {
			ConnectionTag ct = GEnv.Connections.ActiveTag;
			if(ct==null || ct.AttachedPane==null) return false; //!!本来この条件になることはないはずだが、激しくタブを移動させているとこうなることがあった。
			Debug.Assert(ct.AttachedPane.FakeVisible);

			GFrameStyle style = GApp.Options.FrameStyle;
			if(style==GFrameStyle.Single) return false;

			int  pane_count  = style==GFrameStyle.DivVertical3 || style==GFrameStyle.DivHorizontal3? 3 : 2;
			bool is_vertical = style==GFrameStyle.DivVertical3 || style==GFrameStyle.DivVertical;
			int destinationIndex = ct.PositionIndex;
			switch(direction) {
				case Keys.Up:
					if(!is_vertical) destinationIndex--;
					else return false;
					break;
				case Keys.Down:
					if(!is_vertical) destinationIndex++;
					else return false;
					break;
				case Keys.Left:
					if(is_vertical) destinationIndex--;
					else return false;
					break;
				case Keys.Right:
					if(is_vertical) destinationIndex++;
					else return false;
					break;
			}

			if(destinationIndex<0) return false;
			if(destinationIndex>=pane_count) return false;

			MovePane(ct, destinationIndex);
			return true;
		}
		private void MovePane(ConnectionTag ct, int destinationIndex) {
			//位置の変更
			//移動先に表示
			//Debug.WriteLine("--------");
			//GEnv.Connections.Dump();
			int originalPos = ct.PositionIndex;
			Debug.Assert(originalPos!=destinationIndex);
			ct.PositionIndex = destinationIndex;
			TerminalPane pane = GetPane(destinationIndex);
			if(ct.AttachedPane!=null) ct.AttachedPane.Detach();
			if(pane.FakeVisible) pane.Detach();
			pane.FakeVisible = true;
			pane.Attach(ct);
			pane.Focus();
			GEnv.Frame.RefreshConnection(ct);
			//GEnv.Connections.Dump();

			//ここでPreservedPositionIndexを設定
			ct.PreservedPositionIndex = destinationIndex;

			//移動元に別の候補がいればそれを表示
			ConnectionTag orig = GEnv.Connections.GetCandidateOfActivation(originalPos, ct);
			if(orig!=null) {
				//orig.PaneType = CalcPaneType(orig);
				GetPane(originalPos).Attach(orig);
				GEnv.Frame.RefreshConnection(orig);
			}
			else {
				GetPane(originalPos).FakeVisible = false;
			}
		}
		public void SetConnectionLocation(ConnectionTag ct, IPoderosaTerminalPane pane) {
			if(ct.AttachedPane==null) { //非表示のとき
				ct.PositionIndex = GetPaneIndex(pane);
				ct.PreservedPositionIndex = ct.PositionIndex; //手動で設定されたときはここへも記録
				ActivateConnection(ct);
			}
			else {
				ActivateConnection(ct);
				MovePane(ct, GetPaneIndex(pane));
			}
		}


		//次にターミナルを開くとどのサイズになるかを返す
		public Size TerminalSizeForNextConnection {
			get {
				return GetPane(this.PositionForNextConnection).TerminalSize;
			}
		}
		public int PositionForNextConnection {
			get {
				if(GApp.Options.FrameStyle==GFrameStyle.Single)
					return 0;
				else {
					int i;
					for(i=0; i<_panes.Length; i++) {
						if(_panes[i]==null) break;
						if(!_panes[i].FakeVisible) return i;
					}

					return GEnv.Connections.Count % i; //iが有効な個数になっている
				}
			}
		}


		private TerminalPane GetPane(int positionIndex) {
			return _panes[positionIndex];
		}
		private int GetPaneIndex(IPoderosaTerminalPane pane) {
			int i;
			for(i=0; i<_panes.Length; i++) {
				if(_panes[i]==pane) return i;
			}
			return -1;
		}

		public void ApplyOptions(CommonOptions opt) {
			int i;
			for(i=0; i<_panes.Length; i++) {
				if(_panes[i]!=null) _panes[i].ApplyOptions(opt);
			}
		}

		private void OnSplitterMoving(object sender, SplitterEventArgs args) {
			GFrameStyle s = GApp.Options.FrameStyle;
			bool is_vertical = s==GFrameStyle.DivVertical || s==GFrameStyle.DivVertical3;
			int[] ws = new int[_panes.Length];
			for(int i=0; i<_panes.Length; i++) {
				ws[i] = _panes[i]==null? 0 : is_vertical? _panes[i].Width : _panes[i].Height;
			}

			//スプリッタのインデクス
			int splitter_index = 0;
			for(int i=0; i<_panes.Length-1; i++) {
				if(_splitters[i]==sender) {
					splitter_index = i;
					break;
				}
			}

			int diff = (is_vertical? args.SplitX-_panes[splitter_index].Right : args.SplitY-_panes[splitter_index].Bottom);
			ws[splitter_index]   += diff;
			ws[splitter_index+1] -= diff;

			for(int i=0; i<_panes.Length; i++) {
				if(_panes[i]==null) break;
				if(is_vertical)
					_panes[i].SplitterDragging(ws[i], this.Height);
				else
					_panes[i].SplitterDragging(this.Width, ws[i]);
			}
		}

		private bool _ignoreSplitterMoveFlag; //OnResizeの中でSplitPositionをセットするとOnSplitterMovedも呼ばれてしまうのでこれを防止するためにフラグをセット
		private void OnSplitterMoved(object sender, SplitterEventArgs args) {
			if(_ignoreSplitterMoveFlag) return;

			GFrameStyle s = GApp.Options.FrameStyle;
			bool is_vertical = (s==GFrameStyle.DivVertical || s==GFrameStyle.DivVertical3);
			int  pane_count  = StyleToPaneCount(s);
			for(int i=0; i<pane_count; i++) {
				_panes[i].SplitterDragging(_panes[i].Width, _panes[i].Height);
			}

			//スプリッタのインデクス
			int splitter_index = 0;
			int total = 0;
			for(int i=0; i<pane_count-1; i++) {
				total += _splitters[i].SplitPosition;
				if(_splitters[i]==sender) {
					splitter_index = i;
					break;
				}
			}

			double r = (double)(total) / (is_vertical? this.Width : this.Height);
			//Debug.WriteLine("Ratio="+r);
			_splitterRatio[pane_count-2][splitter_index] = r;
		}
		protected override void OnResize(EventArgs args) {
			base.OnResize(args);
			if(_splitters==null || _ignoreResize || !GApp.Options.SplitterPreservesRatio) return;
			GFrameStyle s = GApp.Options.FrameStyle;
			if(s==GFrameStyle.Single) return;
			if(_splitters[0]==null) return; //未初期化時はスキップ
			
			AdjustSplitters();
		}
		private void AdjustSplitters() {
			if(_ignoreSplitterMoveFlag) return;

			_ignoreSplitterMoveFlag = true;
			GFrameStyle s = GApp.Options.FrameStyle;

			bool is_vertical = IsVerticalFrameStyle(s);
			int  pane_count  = StyleToPaneCount(s);
			double offset = 0;
			for(int i=0; i<pane_count-1; i++) {
				double next = _splitterRatio[pane_count-2][i];	
				if(is_vertical) {
					//Debug.WriteLine(String.Format("{0} {1}", i, (int)(this.Width * next)));
					_splitters[i].SplitPosition = (int)(this.Width * (next-offset));
					_panes[i].SplitterDragging((int)(this.Width * (next-offset)), this.Height);
				}
				else {
					_splitters[i].SplitPosition = (int)(this.Height * (next-offset));
					_panes[i].SplitterDragging(this.Width, (int)(this.Height * (next-offset)));
				}
				offset = next;
			}
			//ラスト
			if(is_vertical) {
				_panes[pane_count-1].SplitterDragging((int)(this.Width * (1-offset)), this.Height);
			}
			else {
				_panes[pane_count-1].SplitterDragging(this.Width, (int)(this.Height * (1-offset)));
			}
			_ignoreSplitterMoveFlag = false;
		}

		//フォーカスのあるペインを拡大または縮小する
		public void ResizeSplitterByFocusedPane(bool expand) {
			ConnectionTag ct = GEnv.Connections.ActiveTag;
			RenderProfile prof = ct==null? GEnv.DefaultRenderProfile : ct.RenderProfile;
			if(prof==null) prof = GEnv.DefaultRenderProfile;
			double diff = IsVerticalFrameStyle(GApp.Options.FrameStyle)? prof.Pitch.Width / this.Width : prof.Pitch.Height / this.Height;
			

			int active_index = 0; //アクティブなのがなくてもnull
			if(ct!=null) {
				for(int i=0; i<_panes.Length; i++) {
					if(ct.AttachedPane==_panes[i]) {
						active_index = i;
						break;
					}
				}
			}

			int pane_count = StyleToPaneCount(GApp.Options.FrameStyle);
			double[] ratio = _splitterRatio[pane_count-2];
			if(active_index<pane_count-1) {
				double n0 = ratio[active_index] + (expand? diff : -diff);
				if(n0 < diff*2 || n0 > (active_index+1>=ratio.Length? 1 : ratio[active_index+1])-diff*2) return; //小さすぎてしまうときは拒否
				ratio[active_index] = n0;
			}
			else {
				double n0 = ratio[active_index-1] + (expand? -diff : diff);
				if(n0 < diff*2 || n0 > 1-diff*2) return;
				ratio[active_index-1] = n0;
			}
			AdjustSplitters();
		}

		//文字数を指定してのリサイズ　これはマクロから呼ばれるのみ
		//幅、高さが-1のときはサイズの変更をしないことを意味する
		public void ResizeByChar(int width1, int height1, int width2, int height2) {
			_ignoreSplitterMoveFlag = true;
			_ignoreResize = true;
			this.SuspendLayout();

			/*
			TerminalPane p1 = GetPane(0);
			TerminalPane p2 = GetPane(1);
			Size current_size = GApp.Frame.Size;
			GFrameStyle s = GApp.Options.FrameStyle;
			SizeF pitch = GEnv.DefaultRenderProfile.Pitch;
			if(s==GFrameStyle.Single) {
				Size sz = p1.TerminalSize;
				if(width1==-1) width1 = sz.Width;
				if(height1==-1) height1 = sz.Height;
				Size new_size = new Size(current_size.Width + (int)((width1 - sz.Width)*pitch.Width), current_size.Height + (int)((height1 - sz.Height)*pitch.Height));
				GApp.Frame.Size = new_size;
			}
			else if(s==GFrameStyle.DivHorizontal) {
				Size sz1 = p1.TerminalSize;
				Size sz2 = p2.TerminalSize;
				if(width1==-1) width1 = sz1.Width;
				if(height1==-1) height1 = sz1.Height;
				if(height2==-1) height2 = sz2.Height;
				Size new_size = new Size(current_size.Width + (int)((width1 - sz1.Width)*pitch.Width), current_size.Height + (int)((height1-sz1.Height + height2-sz2.Height)*pitch.Height));
				GApp.Frame.Size = new_size;

				int newpos = _splitter.SplitPosition + (int)((height1-sz1.Height) * pitch.Height);
				_splitter.SplitPosition = newpos;
				p1.SplitterDragging(this.Width, newpos);
				p2.SplitterDragging(this.Width, this.Height - _splitter.Height - newpos);
				_splitterRatio =  (double)newpos / (this.Height - _splitter.Height);
			}
			else if(s==GFrameStyle.DivVertical) {
				Size sz1 = p1.TerminalSize;
				Size sz2 = p2.TerminalSize;
				if(width1==-1) width1 = sz1.Width;
				if(width2==-1) width2 = sz2.Width;
				if(height1==-1) height1 = sz1.Height;
				Size new_size = new Size(current_size.Width + (int)((width1-sz1.Width + width2-sz2.Width)*pitch.Width), current_size.Height + (int)((height1-sz1.Height)*pitch.Height));
				GApp.Frame.Size = new_size;

				int newpos = _splitter.SplitPosition + (int)((width1-sz1.Width) * pitch.Width);
				_splitter.SplitPosition = newpos;
				p1.SplitterDragging(newpos, this.Height);
				p2.SplitterDragging(this.Width - _splitter.Width - newpos, this.Height);
				_splitterRatio =  (double)newpos / (this.Width - _splitter.Width);
			}
			*/

			_ignoreSplitterMoveFlag = false;
			_ignoreResize = false;
			this.ResumeLayout(true);
		}

		protected override bool IsInputKey(Keys key) {
			//Debug.WriteLine("MultiPane IsInputKey " + key);
			return false;
		}
		protected override bool ProcessDialogKey(Keys keyData) {
			//Debug.WriteLine("MultiPane ProcessDialogKey " + keyData);
			//おそらくContainerControlが、Shift+カーソルキーを処理してフォーカスを移動させるようだ
			if((keyData & Keys.Modifiers)==Keys.Shift && GUtil.IsCursorKey(keyData & Keys.KeyCode))
				return true;
			else
				return base.ProcessDialogKey(keyData);
		}

		private static int StyleToPaneCount(GFrameStyle style) {
			switch(style) {
				case GFrameStyle.Single:
					return 1;
				case GFrameStyle.DivVertical:
				case GFrameStyle.DivHorizontal:
					return 2;
				case GFrameStyle.DivVertical3:
				case GFrameStyle.DivHorizontal3:
					return 3;
				default:
					Debug.Assert(false);
					return 0;
			}
		}
		private static bool IsVerticalFrameStyle(GFrameStyle style) {
			return style==GFrameStyle.DivVertical || style==GFrameStyle.DivVertical3;
		}


		//PaneTagと表示位置が等しく、Visibleの値が等しいことを判定する
		private class PosCheck {
			private int _positionIndex;
			private TerminalConnection _ignore;
			public PosCheck(int pi) {
				_positionIndex = pi;
				_ignore = null;
			}
			//_ignoreに等しい接続はマッチしないと判定
			public PosCheck(int pi, TerminalConnection ig) {
				_positionIndex = pi;
				_ignore = ig;
			}
			public bool Check(ConnectionTag ct) {
				return ct.PositionIndex==_positionIndex && _ignore!=ct.Connection;
			}
		}
	}


}
