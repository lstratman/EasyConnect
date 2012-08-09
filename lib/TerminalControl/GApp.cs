/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: GApp.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Resources;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;

using Poderosa.Toolkit;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Terminal;
using Poderosa.Forms;
using Poderosa.Communication;
using Poderosa.Config;
using Poderosa.MacroEnv;
using Poderosa.Text;
using Poderosa.UI;

namespace Poderosa
{
	internal class GApp
	{
		public static GFrame _frame;
		private static StringResources _strings;
		private static ConnectionHistory _history;
		private static MacroManager _macroManager;
		private static PoderosaContainer _container;
		private static ContainerGlobalCommandTarget _globalCommandTarget;
		private static ContainerInterThreadUIService _interThreadUIService;
		private static SSHKnownHosts _sshKnownHosts;
		private static ContainerOptions _options;
		public static IntPtr _globalMutex; //複数起動時の設定ファイル保護
		private static bool _closingApp; //アプリ終了時に立つフラグ 
		private static bool _closeWithoutSave;

	
		[STAThread]
		static void Main(string[] args) {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());
            
            try {
				if(args.Length>0) {
					if(InterProcessService.SendShortCutFileNameToExistingInstance(args[0])) return;
				}

				GApp.Run(args);
			}
			catch(Exception e) {
#if DEBUG
				Debug.WriteLine(e.Message);
				Debug.WriteLine(e.StackTrace);
#else
				GUtil.ReportCriticalError(e);
#endif
			}
			
		}

		private static IntPtr CheckDuplicatedInstance() {
			IntPtr t = Win32.CreateEvent(IntPtr.Zero, 0, 0, "PoderosaHandle");
			if(Win32.GetLastError()==Win32.ERROR_ALREADY_EXISTS) {
				Win32.CloseHandle(t);
				return IntPtr.Zero;
			}
			else
				return t;
		}
        public static void CreateGFrame(string[] args)
        {
            InitialAction a = new InitialAction();
            _globalMutex = Win32.CreateMutex(IntPtr.Zero, 0, "PoderosaGlobalMutex");
            bool already_exists = (Win32.GetLastError() == Win32.ERROR_ALREADY_EXISTS);
            if (_globalMutex == IntPtr.Zero) throw new Exception("Global mutex could not open");

            LoadEnvironment(a);
            Init(a, args, already_exists);
            //System.Windows.Forms.Application.Run(_frame);
            //_frame.Show();
            
            if (!_closeWithoutSave) SaveEnvironment();
            GEnv.Terminate();

            Win32.CloseHandle(_globalMutex);
        }
		public static void Run(string[] args) {
			InitialAction a = new InitialAction();
			_globalMutex = Win32.CreateMutex(IntPtr.Zero, 0, "PoderosaGlobalMutex");
			bool already_exists = (Win32.GetLastError()==Win32.ERROR_ALREADY_EXISTS);
			if(_globalMutex==IntPtr.Zero) throw new Exception("Global mutex could not open");

			LoadEnvironment(a);
			Init(a, args, already_exists);
			//System.Windows.Forms.Application.Run(_frame);
            //_frame.Show();
            if(!_closeWithoutSave) SaveEnvironment();
			GEnv.Terminate();

			Win32.CloseHandle(_globalMutex);
		}

		public static void Init(InitialAction act, string[] args, bool already_exists) { //GFrameの作成はコンストラクタの後にしないと、GuevaraAppのメソッドをデリゲートの引数にできない。

			if(args.Length>0) {
				act.ShortcutFile = args[0];
			}

			_frame = new GFrame(act);
			_globalCommandTarget.Init(_frame);

			if(already_exists && _options.FrameState==FormWindowState.Normal) {
				Rectangle rect = _options.FramePosition;
				rect.Location += new Size(24,24);
				_options.FramePosition = rect;
			}

			_frame.DesktopBounds = _options.FramePosition;
			_frame.WindowState = _options.FrameState;
			_frame.AdjustMRUMenu();

			//キャッチできなかったエラーの補足
			Application.ThreadException += new ThreadExceptionEventHandler(OnThreadException);
		}

		public static void LoadEnvironment(InitialAction act) {
			ThemeUtil.Init();

			OptionPreservePlace place = GetOptionPreservePlace();
			_options = new ContainerOptions();
			_history = new ConnectionHistory();
			_macroManager = new MacroManager();
			_container = new PoderosaContainer();
			_globalCommandTarget = new ContainerGlobalCommandTarget();
			_interThreadUIService = new ContainerInterThreadUIService();
			_sshKnownHosts = new SSHKnownHosts();


			//この時点ではOSの言語設定に合ったリソースをロードする。起動直前で必要に応じてリロード
			ReloadStringResource();

			GEnv.Init(_container);
			GEnv.Options = _options;
			GEnv.GlobalCommandTarget = _globalCommandTarget;
			GEnv.InterThreadUIService = _interThreadUIService;
			GEnv.SSHKnownHosts = _sshKnownHosts;
			string dir = GetOptionDirectory(place);
			LoadConfigFiles(dir, act);
			_options.OptionPreservePlace = place;

			//ここまできたら言語設定をチェックし、必要なら読み直し
			if(GUtil.CurrentLanguage!=_options.Language) {
				System.Threading.Thread.CurrentThread.CurrentUICulture = _options.Language==Language.Japanese? new CultureInfo("ja") : CultureInfo.InvariantCulture;
				ReloadStringResource();
			}

		}

		private static void LoadConfigFiles(string dir, InitialAction act) {
			if(Win32.WaitForSingleObject(_globalMutex, 10000)!=Win32.WAIT_OBJECT_0) throw new Exception("Global mutex lock error");

			try {
				string optionfile = dir+"options.conf";
				bool config_loaded = false;
				bool macro_loaded = false;

				TextReader reader = null;
				try {
					if(File.Exists(optionfile)) {
						reader = new StreamReader(File.Open(optionfile, FileMode.Open, FileAccess.Read), Encoding.Default);
						if(VerifyConfigHeader(reader)) {
							ConfigNode root = new ConfigNode("root", reader).FindChildConfigNode("poderosa");
							if(root!=null) {
								_options.Load(root);
								config_loaded = true;
								_history.Load(root);
								_macroManager.Load(root);
								macro_loaded = true;
							}
						}
					}
				}
				catch(Exception ex) {
					//_errorOccurredOnBoot = true;
					Debug.WriteLine(ex.StackTrace);
					GUtil.WriteDebugLog(ex.StackTrace);
					act.AddMessage("Failed to read the configuration file.\n" + ex.Message);
				}
				finally {
					if(!config_loaded) _options.Init();
					if(!macro_loaded)  _macroManager.SetDefault();
					if(reader != null) reader.Close();
				}

				GEnv.Options = _options; //これでDefaultRenderProfileが初期化される

				string kh = dir+"ssh_known_hosts";
				if(File.Exists(kh)) {
					try {
						_sshKnownHosts.Load(kh);
					}
					catch(Exception ex) {
						_sshKnownHosts.Clear();
						act.AddMessage("Failed to read the 'ssh_known_hosts' file.\n" + ex.Message);
					}
				}
			}
			finally {
				Win32.ReleaseMutex(_globalMutex);
			}

		}

		private static void ReloadStringResource() {
			_strings = new StringResources("Poderosa.strings", typeof(GApp).Assembly); 
			EnumDescAttribute.AddResourceTable(typeof(GApp).Assembly, _strings);
			GEnv.ReloadStringResource();
		}

		private static void InitConfig() {
			_options.Init();
			_macroManager.SetDefault();
		}

		
		private static void SaveEnvironment() {

			//OptionDialogで、レジストリへの書き込み権限がないとOptionPreservePlaceは変更できないようにしてあるのでWritableなときだけ書いておけばOK
			if(IsRegistryWritable) {
				RegistryKey g = Registry.CurrentUser.CreateSubKey(GCConst.REGISTRY_PATH);
				g.SetValue("option-place", EnumDescAttribute.For(typeof(OptionPreservePlace)).GetName(_options.OptionPreservePlace));
			}

			if(Win32.WaitForSingleObject(_globalMutex, 10000)!=Win32.WAIT_OBJECT_0) throw new Exception("Global mutex lock error");

			try {
				string dir = GetOptionDirectory(_options.OptionPreservePlace);
				TextWriter wr = null;
				try {
					if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);

					_sshKnownHosts.WriteTo(dir+"ssh_known_hosts");
					wr = new StreamWriter(dir+"options.conf", false, Encoding.Default);
				}
				catch(Exception ex) {
					//GUtil.ReportCriticalError(ex);
					GUtil.Warning(Form.ActiveForm, String.Format(GApp.Strings.GetString("Message.GApp.WriteError"), ex.Message, dir));
				}

				if(wr!=null) {
					try {
						ConfigNode node = new ConfigNode("poderosa");
						_options.Save(node);
						_history.Save(node);
						_macroManager.Save(node);
						
						wr.WriteLine(GCConst.CONFIG_HEADER);
						node.WriteTo(wr);
						wr.Close();
					}
					catch(Exception ex) {
						GUtil.ReportCriticalError(ex);
					}
				}
			}
			finally {
				Win32.ReleaseMutex(_globalMutex);
			}

		}
		public static string GetOptionDirectory(OptionPreservePlace p) {
			if(p==OptionPreservePlace.InstalledDir) {
				string t = AppDomain.CurrentDomain.BaseDirectory;
				if(Environment.UserName.Length>0) t += Environment.UserName + "\\";
				return t;
			}
			else
				return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\poderosa\\";
		}
		public static string GetCommonLogDirectory() {
			if(GetOptionPreservePlace()==OptionPreservePlace.InstalledDir)
				return AppDomain.CurrentDomain.BaseDirectory + "\\";
			else
				return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\";
		}
		public static string BaseDirectory {
			get {
				return AppDomain.CurrentDomain.BaseDirectory;
			}
		}


		public static ConnectionHistory ConnectionHistory {
			get {
				return _history;
			}
		}
		public static MacroManager MacroManager {
			get {
				return _macroManager;
			}
		}
		public static void UpdateOptions(ContainerOptions opt) {
			GEnv.Options = opt;
			_frame.ApplyOptions(_options, opt); 
			_history.LimitCount(opt.MRUSize);
			_frame.AdjustMRUMenu();

			if(_options.Language!=opt.Language) { //言語のリロードが必要なとき
				System.Threading.Thread.CurrentThread.CurrentUICulture = opt.Language==Language.Japanese? new CultureInfo("ja") : CultureInfo.InvariantCulture;
				GApp.ReloadStringResource();
				Granados.SSHC.Strings.Reload();
				GApp.MacroManager.ReloadLanguage();
				_frame.ReloadLanguage(opt.Language);
			}

			//デフォルトのままであった場合には更新をかける
			RenderProfile newprof = new RenderProfile(opt);
			foreach(ConnectionTag ct in GEnv.Connections) {
				if(ct.RenderProfile==null && ct.AttachedPane!=null) ct.AttachedPane.ApplyRenderProfile(newprof);
			}
			GEnv.DefaultRenderProfile = newprof;
			_options = opt;
		}
		internal static StringResources Strings {
			get {
				return _strings;
			}
		}
		internal static ContainerInterThreadUIService InterThreadUIService {
			get {
				return _interThreadUIService;
			}
		}

		internal static GFrame Frame {
			get {
				return _frame;
			}
		}
		internal static ContainerOptions Options {
			get {
				return _options;
			}
		}
		internal static ContainerGlobalCommandTarget GlobalCommandTarget {
			get {
				return _globalCommandTarget;
			}
		}
		public static bool ClosingApp {
			get {
				return _closingApp;
			}
			set {
				_closingApp = value;
			}
		}
		public static bool CloseWithoutSave {
			get {
				return _closeWithoutSave;
			}
			set {
				_closeWithoutSave = value;
			}
		}


		public static bool IsRegistryWritable {
			get {
				try {
					RegistryKey g = Registry.CurrentUser.CreateSubKey(GCConst.REGISTRY_PATH);
					if(g==null)
						return false;
					else
						return true;
				}
				catch(Exception) {
					return false;
				}
			}
		}

		private static OptionPreservePlace GetOptionPreservePlace() {
			RegistryKey g = Registry.CurrentUser.OpenSubKey(GCConst.REGISTRY_PATH, false);
			if(g==null)
				return OptionPreservePlace.InstalledDir;
			else {
				string v = (string)g.GetValue("option-place");
				if(v==null || v.Length==0)
					return OptionPreservePlace.InstalledDir;
				else
					return (OptionPreservePlace)Enum.Parse(typeof(OptionPreservePlace), v);
			}
		}

		public static ContainerConnectionCommandTarget GetConnectionCommandTarget() {
			TerminalConnection con = GEnv.Connections.ActiveConnection;
			return con==null? null : new ContainerConnectionCommandTarget(con);
		}
		public static ContainerConnectionCommandTarget GetConnectionCommandTarget(TerminalConnection con) {
			return new ContainerConnectionCommandTarget(con); 
		}
		public static ContainerConnectionCommandTarget GetConnectionCommandTarget(ConnectionTag tag) {
			return new ContainerConnectionCommandTarget(tag.Connection); 
		}

		public static void OnThreadException(object sender, ThreadExceptionEventArgs e) {
			GUtil.ReportThreadException(e.Exception);
			/*
			string msg = DateTime.Now.ToString() + " : " + "OnThreadException() sender=" + sender.ToString() + "\r\n  StackTrace=" + new StackTrace(true).ToString();
			Debug.WriteLine(msg);
			GUtil.WriteDebugLog(msg);
			throw e.Exception;
			*/
		}

		private static bool VerifyConfigHeader(TextReader reader) {
			string l = reader.ReadLine();
			return l==GCConst.CONFIG_HEADER;
		}
	}
	internal class GCConst {
		private static string _productWeb;
		public static string PRODUCT_WEB {
			get {
				if(_productWeb==null)
					_productWeb = GApp.Strings.GetString("Util.GApp.ProductWebSite");
				return _productWeb;
			}
		}
		public const string REGISTRY_PATH = "Software\\Poderosa Project\\Poderosa";
		public const string CONFIG_HEADER = "Poderosa Config 3.0";
	}
	internal class PoderosaContainer : IPoderosaContainer {
		public void RemoveConnection(ConnectionTag ct) {
			GApp.Frame.RemoveConnection(ct);
		}

		public void ActivateConnection(ConnectionTag ct) {
			GApp.GlobalCommandTarget.ActivateConnection2(ct);
		}
		public void RefreshConnection(ConnectionTag ct) {
			GApp.Frame.RefreshConnection(ct);
		}

		public void OnDragDrop(DragEventArgs args) {
			GApp.Frame.OnDragDropInternal(args);
		}
		public void OnDragEnter(DragEventArgs args) {
			GApp.Frame.OnDragEnterInternal(args);
		}

		public System.Drawing.Size TerminalSizeForNextConnection {
			get {
				return GApp.Frame.PaneContainer.TerminalSizeForNextConnection;
			}
		}

		public int PositionForNextConnection {
			get {
				return GApp.Frame.PaneContainer.PositionForNextConnection;
			}
		}


		public void IndicateBell() {
			GApp.Frame.StatusBar.IndicateBell();
		}

		public void SetStatusBarText(string text) {
			GApp.Frame.StatusBar.SetStatusBarText(text);
		}

		public void ShowContextMenu(System.Drawing.Point pt, ConnectionTag ct) {
			//GApp.Frame.CommandTargetConnection = ct.Connection;
			//メニューのUI調整
			//GApp.Frame.AdjustContextMenu(true, ct.Connection);
			//GApp.Frame.ContextMenu.Show(GApp.Frame.PaneContainer, pt);
            
            //foreach (TerminalPane p in GApp.Frame._multiPaneControl._panes)
            //{
                //if (p.Visible)
                    //GApp.Frame.ContextMenu.Show(p, pt);
            //}
		}

		public void SetSelectionStatus(SelectionStatus status) {
			if(status==SelectionStatus.Auto)
				GApp.Frame.StatusBar.IndicateAutoSelectionMode();
			else if(status==SelectionStatus.Free)
				GApp.Frame.StatusBar.IndicateFreeSelectionMode();
			else
				GApp.Frame.StatusBar.ClearSelectionMode();
		}

		public bool MacroIsRunning {
			get {
				return GApp.MacroManager.MacroIsRunning;
			}
		}

		public CommandResult ProcessShortcutKey(Keys key) {
			return GApp.Options.Commands.ProcessKey(key, GApp.MacroManager.MacroIsRunning);
		}

		public Form AsForm() {
			return GApp.Frame;
		}


		public System.IntPtr Handle {
			get {
				return GApp.Frame.Handle;
			}
		}

		public bool IgnoreErrors {
			get {
				return GApp.ClosingApp;
			}
		}
	}
	internal class InterProcessService {

		public const int OPEN_SHORTCUT = 7964;
		public const int OPEN_SHORTCUT_OK = 485;

		public static bool SendShortCutFileNameToExistingInstance(string filename) {
			unsafe {
				//find target
				IntPtr hwnd = Win32.FindWindowEx(IntPtr.Zero,IntPtr.Zero,null,null);
				bool success = false;
				char[] name = new char[256];
				char[] mf  = new char[256];
				while(hwnd!=IntPtr.Zero) {
					int len = Win32.GetWindowText(hwnd, name, 256);
					if(new string(name, 0, len).IndexOf("Poderosa")!=-1) { //Window Classを確認するとか何とかすべきかも、だが
						success = TryToSend(hwnd, filename); 
						if(success) break;
					}
					hwnd = Win32.FindWindowEx(IntPtr.Zero,hwnd,null,null);
				}

				
				return success;
			}

		}

		private unsafe static bool TryToSend(IntPtr hwnd, string filename) {
			char[] data = filename.ToCharArray();
			char* b = stackalloc char[data.Length+1];
			for(int i=0; i<data.Length; i++) b[i] = data[i];
			b[data.Length] = '\0';
				
			//string t = ReadFileName(hglobal);
			Win32.COPYDATASTRUCT cddata = new Win32.COPYDATASTRUCT();
			cddata.dwData = OPEN_SHORTCUT;
			cddata.cbData = (uint)(sizeof(char) * (data.Length+1));
			cddata.lpData = b;

			int lresult = Win32.SendMessage(hwnd, Win32.WM_COPYDATA, IntPtr.Zero, new IntPtr(&cddata));
			//Debug.WriteLine("TryToSend "+lresult);
			return lresult==OPEN_SHORTCUT_OK;
		}

	}
	internal class InitialAction {
		private ArrayList _messages;  //message boxを出すべき内容
		private string _shortcutFile; //最初に開くショートカットファイル：不要なときはnull
	
		public InitialAction() {
			_messages = new ArrayList();
		}
		public void AddMessage(string msg) {
			_messages.Add(msg);
		}
		public string ShortcutFile {
			get {
				return _shortcutFile;
			}
			set {
				_shortcutFile = value;
			}
		}
		public IEnumerable Messages {
			get {
				return _messages;
			}
		}
	}
}
