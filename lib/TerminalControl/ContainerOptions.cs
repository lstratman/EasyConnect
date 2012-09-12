/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ContainerOptions.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;

using Poderosa.Toolkit;
using Poderosa.Communication;
using Poderosa.Terminal;

namespace Poderosa.Config {
	internal class ContainerOptions : CommonOptions {

		protected Language _envLanguage; //Windowsの環境に基づく言語 保存はされない

		protected Commands _commands;
		protected Rectangle _framePosition;

		[ConfigEnumElement(typeof(OptionPreservePlace), InitialAsInt=(int)OptionPreservePlace.InstalledDir)]
		                                   protected OptionPreservePlace _optionPreservePlace;

		[ConfigStringElement(Initial="")]  protected string _defaultKeyDir;
		[ConfigStringElement(Initial="")]  protected string _defaultFileDir;
		[ConfigEnumElement(typeof(FormWindowState), InitialAsInt=(int)FormWindowState.Normal)]
		                                   protected FormWindowState _frameState;
		[ConfigBoolElement(Initial=false)] protected bool _guevaraMode;
		[ConfigBoolElement(Initial=true)]  protected bool _showWelcomeDialog;
		[ConfigBoolElement(Initial=false)] protected bool _hideDialogForSP1Issue;
		
		[ConfigEnumElement(typeof(GFrameStyle), InitialAsInt=(int)GFrameStyle.Single)]
		                                   protected GFrameStyle _frameStyle;
		[ConfigEnumElement(typeof(TabBarStyle), InitialAsInt=(int)TabBarStyle.MultiRow)] 
		                                   protected TabBarStyle _tabBarStyle;
		[ConfigBoolElement(Initial=false)] protected bool _splitterPreservesRatio; //フレームをリサイズしたときスプリッタが連動するかどうか

		[ConfigBoolElement(Initial=true)]  protected bool _showToolBar;
		[ConfigBoolElement(Initial=true)]  protected bool _showTabBar;
		[ConfigBoolElement(Initial=true)]  protected bool _showStatusBar;
		[ConfigIntElement(Initial=8)]      protected int _MRUSize;
		[ConfigIntElement(Initial=4)]      protected int _serialCount;
		
		[ConfigEnumElement(typeof(CID), InitialAsInt=(int)CID.NOP)]  protected CID _actionOnLaunch;

		private static ArrayList _configAttributes;

		public Commands Commands {
			get {
				return _commands;
			}
			set {
				_commands = value;
				GApp.Frame.ApplyHotKeys(value);
			}
		}
		public bool GuevaraMode {
			get {
				return _guevaraMode;
			}
			set {
				_guevaraMode = value;
			}
		}
		public bool HideDialogForSP1Issue {
			get {
				return _hideDialogForSP1Issue;
			}
			set {
				_hideDialogForSP1Issue = value;
			}
		}
		public GFrameStyle FrameStyle {
			get {
				return _frameStyle;
			}
			set {
				_frameStyle = value;
			}
		}
		public Rectangle FramePosition {
			get {
				return _framePosition;
			}
			set {
				_framePosition = value;
			}
		}
		public string DefaultKeyDir {
			get {
				return _defaultKeyDir;
			}
			set {
				_defaultKeyDir = value;
			}
		}
		public string DefaultFileDir {
			get {
				return _defaultFileDir;
			}
			set {
				_defaultFileDir = value;
			}
		}
		public FormWindowState FrameState {
			get {
				return _frameState;
			}
			set {
				_frameState = value;
			}
		}
		public OptionPreservePlace OptionPreservePlace {
			get {
				return _optionPreservePlace;
			}
			set {
				_optionPreservePlace = value;
			}
		}
		public int MRUSize {
			get {
				return _MRUSize;
			}
			set {
				if(value<0 || value>20)
					throw new InvalidOptionException(GApp.Strings.GetString("Message.MRULimit"));
				_MRUSize = value;
			}
		}
		public int SerialCount {
			get {
				return _serialCount;
			}
			set {
				if(value<1 || value>99)
					throw new InvalidOptionException(GApp.Strings.GetString("Message.SerialLimit"));
				_serialCount = value;
			}
		}
		public CID ActionOnLaunch {
			get {
				return _actionOnLaunch;
			}
			set {
				_actionOnLaunch = value;
			}
		}
		public bool SplitterPreservesRatio {
			get {
				return _splitterPreservesRatio;
			}
			set {
				_splitterPreservesRatio = value;
			}
		}
		public bool ShowToolBar {
			get {
				return _showToolBar;
			}
			set {
				_showToolBar = value;
			}
		}
		public bool ShowTabBar {
			get {
				return _showTabBar;
			}
			set {
				_showTabBar = value;
			}
		}
		public bool ShowStatusBar {
			get {
				return _showStatusBar;
			}
			set {
				_showStatusBar = value;
			}
		}
		public TabBarStyle TabBarStyle {
			get {
				return _tabBarStyle;
			}
			set {
				_tabBarStyle = value;
			}
		}
		public Language EnvLanguage {
			get {
				return _envLanguage;
			}
			set {
				_envLanguage = value;
			}
		}
		public bool ShowWelcomeDialog {
			get {
				return _showWelcomeDialog;
			}
			set {
				_showWelcomeDialog = value;
			}
		}

		public ContainerOptions() {
			_envLanguage = GUtil.CurrentLanguage;
			if(_configAttributes==null) InitConfigAttributes();
		}

		public override object Clone() {
			ContainerOptions o = new ContainerOptions();
			CopyTo(o);
			return o;
		}
		public void CopyTo(ContainerOptions o) {
			base.CopyTo(o);
			o._defaultKeyDir = _defaultKeyDir;
			o._defaultFileDir = _defaultFileDir;
			o._framePosition = _framePosition;
			o._frameState = _frameState;
			o._optionPreservePlace = _optionPreservePlace;
			o._frameStyle = _frameStyle;
			o._splitterPreservesRatio = _splitterPreservesRatio;
			o._showToolBar = _showToolBar;
			o._showTabBar = _showTabBar;
			o._showStatusBar = _showStatusBar;
			o._tabBarStyle = _tabBarStyle;
			o._actionOnLaunch = _actionOnLaunch;
			o._MRUSize = _MRUSize;
			o._serialCount = _serialCount;
			o._language = _language;
			o._envLanguage = _envLanguage;
			o._guevaraMode = _guevaraMode;
			o._commands = (Commands)_commands.Clone();
			o._showWelcomeDialog = _showWelcomeDialog;
		}

		private static void InitConfigAttributes() {
			FieldInfo[] fields = typeof(ContainerOptions).GetFields(BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly);
			_configAttributes = new ArrayList(fields.Length);
			foreach(FieldInfo field in fields) {
				object[] attrs = field.GetCustomAttributes(typeof(ConfigElementAttribute), true);
				if(attrs.Length!=0) {
					ConfigElementAttribute attr = (ConfigElementAttribute)attrs[0];
					attr.FieldInfo = field;
					_configAttributes.Add(attr);
				}
			}
		}

		public override sealed void Save(ConfigNode parent) {
			ConfigNode node = new ConfigNode("poderosa-container");
			foreach(ConfigElementAttribute attr in _configAttributes) {
				attr.ExportTo(this, node);
			}
			node["framePosition"] = String.Format("{0},{1},{2},{3}", _framePosition.X, _framePosition.Y, _framePosition.Width, _framePosition.Height);
			parent.AddChild(node);

			_commands.Save(parent);

			base.Save(parent);
		}
		public override sealed void Load(ConfigNode parent) {
			ConfigNode node = parent.FindChildConfigNode("poderosa-container");
			if(node!=null) {
				//基本のアトリビュート
				foreach(ConfigElementAttribute attr in _configAttributes) {
					attr.ImportFrom(this, node);
				}
			}

			//frame stateは別の扱い
			string frame_pos = node==null? null : node.GetValue("framePosition", null);
			bool frame_filled = false;
			if(_frameState==FormWindowState.Normal && frame_pos!=null) {
				string[] t = frame_pos.Split(',');
				if(t.Length==4) {
					_framePosition.X = GUtil.ParseInt(t[0], 0);
					_framePosition.Y = GUtil.ParseInt(t[1], 0);
					_framePosition.Width = GUtil.ParseInt(t[2], 640);
					_framePosition.Height = GUtil.ParseInt(t[3], 480);

					frame_filled = true;
				}
			}
			
			if(!frame_filled) {
				if(_frameState==FormWindowState.Minimized) _frameState = FormWindowState.Normal; //最小化で起動しても仕方ないのでノーマルにする
				Rectangle r = Screen.PrimaryScreen.Bounds;
				_framePosition.X = r.Width / 6;
				_framePosition.Y = r.Height / 6;
				_framePosition.Width = r.Width*2 / 3;
				_framePosition.Height = r.Height*2 / 3;
			}

			_commands = new Commands();
			_commands.Load(parent);

			base.Load(parent);
		}

		public override void Init() {
			foreach(ConfigElementAttribute attr in _configAttributes) {
				attr.Reset(this);
			}
			
			Rectangle r = Screen.PrimaryScreen.Bounds;
			_framePosition.X = r.Width / 6;
			_framePosition.Y = r.Height / 6;
			_framePosition.Width = r.Width*2 / 3;
			_framePosition.Height = r.Height*2 / 3;

			_commands = new Commands();
			_commands.Init();
			base.Init();
		}

	}

	/// <summary>
	/// フレームのスタイル
	/// </summary>
	/// <remarks>
	/// GFrameStyleという名前になっているのは、System.Windows.FormsにFrameStyleという名前の列挙体がすでにあるからです。
	/// </remarks>
	[EnumDesc(typeof(GFrameStyle))]
	public enum GFrameStyle {
		/// <summary>
		/// シングル
		/// </summary>
		[EnumValue(Description="Enum.GFrameStyle.Single")]   Single,             //１度に一つのみ表示
		/// <summary>
		/// 左右に２分割
		/// </summary>
		[EnumValue(Description="Enum.GFrameStyle.DivVertical")] DivVertical,        //左右分割
		/// <summary>
		/// 上下に２分割
		/// </summary>
		[EnumValue(Description="Enum.GFrameStyle.DivHorizontal")]  DivHorizontal,      //上下分割

		[EnumValue(Description="Enum.GFrameStyle.DivVertical3")]   DivVertical3,       //左右３分割
		[EnumValue(Description="Enum.GFrameStyle.DivHorizontal3")] DivHorizontal3,     //上下３分割
		[EnumValue(Description="Enum.GFrameStyle.DivQuad")]        DivQuad             //４分割
}
	[EnumDesc(typeof(TabBarStyle))]
	public enum TabBarStyle {
		[EnumValue(Description="Enum.TabBarStyle.MultiRow")] MultiRow,
		[EnumValue(Description="Enum.TabBarStyle.ScrollButton")] ScrollButton
	}


	[EnumDesc(typeof(OptionPreservePlace))]
	public enum OptionPreservePlace {
		[EnumValue(Description="Enum.OptionPreservePlace.InstalledDir")] InstalledDir,
		[EnumValue(Description="Enum.OptionPreservePlace.AppData")] AppData
	}

}
