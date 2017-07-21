/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: WindowManager.cs,v 1.3 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Globalization;

using Poderosa.Util;
using Poderosa.Plugins;
using Poderosa.Sessions;
using Poderosa.Preferences;
using Poderosa.View;
using Poderosa.Commands;

[assembly: PluginDeclaration(typeof(Poderosa.Forms.WindowManagerPlugin))]

namespace Poderosa.Forms {
    [PluginInfo(ID = WindowManagerPlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.core.preferences;org.poderosa.core.commands")]
    internal class WindowManagerPlugin :
            PluginBase,
            IGUIMessageLoop,
            IWindowManager,
            IWinFormsService,
            ICultureChangeListener,
            IKeyBindChangeListener {
        public const string PLUGIN_ID = "org.poderosa.core.window";

        private List<MainWindow> _windows;
        private List<PopupViewContainer> _popupWindows;
        private MainWindow _activeWindow;
        private PoderosaAppContext _appContext;
        private MainWindowMenu _menu;
        private WindowPreference _preferences;
        private ViewFactoryManager _viewFactoryManager;

        private object _draggingObject;
        private SelectionService _selectionService;

        private bool _executingAllWindowClose;

        private static WindowManagerPlugin _instance;

        public static WindowManagerPlugin Instance {
            get {
                return _instance;
            }
        }


        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;

            //Coreアセンブリ内のプラグインを代表してここでAdapterFactoryをセット
            new CoreServices(poderosa);

            TabBar.Init();

            IPluginManager pm = poderosa.PluginManager;
            pm.FindExtensionPoint("org.poderosa.root").RegisterExtension(this);
            pm.CreateExtensionPoint(WindowManagerConstants.MAINWINDOWCONTENT_ID, typeof(IViewManagerFactory), this);
            pm.CreateExtensionPoint(WindowManagerConstants.VIEW_FACTORY_ID, typeof(IViewFactory), this);
            pm.CreateExtensionPoint(WindowManagerConstants.VIEWFORMATEVENTHANDLER_ID, typeof(IViewFormatEventHandler), this);
            pm.CreateExtensionPoint(WindowManagerConstants.TOOLBARCOMPONENT_ID, typeof(IToolBarComponent), this);
            pm.CreateExtensionPoint(WindowManagerConstants.MAINWINDOWEVENTHANDLER_ID, typeof(IMainWindowEventHandler), this);
            pm.CreateExtensionPoint(WindowManagerConstants.FILEDROPHANDLER_ID, typeof(IFileDropHandler), this);
            AboutBoxUtil.DefineExtensionPoint(pm);

            _preferences = new WindowPreference();
            pm.FindExtensionPoint(PreferencePlugin.EXTENSIONPOINT_NAME)
                .RegisterExtension(_preferences);
            pm.FindExtensionPoint(WindowManagerConstants.MAINWINDOWCONTENT_ID)
                .RegisterExtension(new DefaultViewManagerFactory());

            _windows = new List<MainWindow>();
            _popupWindows = new List<PopupViewContainer>();

            _menu = new MainWindowMenu();
            _appContext = new PoderosaAppContext();
            _selectionService = new SelectionService(this);
            _viewFactoryManager = new ViewFactoryManager();

            CommandManagerPlugin.Instance.AddKeyBindChangeListener(this);
            poderosa.Culture.AddChangeListener(this);
        }

        public void RunExtension() {
            try {
                _poderosaWorld.Culture.SetCulture(CoreServicePreferenceAdapter.LangToCulture(_preferences.OriginalPreference.Language));
                MainWindowArgument[] args = MainWindowArgument.Parse(_preferences);
                foreach (MainWindowArgument arg in args)
                    _windows.Add(CreateMainWindow(arg));

                if (GetStartMode() == StartMode.StandAlone) {
                    Application.Run(_appContext);
                    IPoderosaApplication app = (IPoderosaApplication)_poderosaWorld.GetAdapter(typeof(IPoderosaApplication));
                    app.Shutdown();
                }
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }

        private MainWindow CreateMainWindow(MainWindowArgument arg) {
            MainWindow w = new MainWindow(arg, _menu);
            w.Text = "Poderosa";
            w.FormClosed += new FormClosedEventHandler(WindowClosedHandler);
            w.Activated += delegate(object sender, EventArgs args) {
                _activeWindow = (MainWindow)sender; //最後にアクティブになったものを指定する
            };
            w.Show();
            return w;
        }

        public void CreateNewWindow(MainWindowArgument arg) {
            _windows.Add(CreateMainWindow(arg));
        }

        //アプリ終了時
        public CommandResult CloseAllWindows() {
            try {
                _executingAllWindowClose = true;
                _preferences.WindowArray.Clear();
                //コピーのコレクションに対して実行しないといかん
                List<MainWindow> targets = new List<MainWindow>(_windows);
                foreach (MainWindow window in targets) {
                    CommandResult r = window.CancellableClose();
                    if (r != CommandResult.Succeeded)
                        return r; //キャンセルされた場合はそこで中止
                    _preferences.FormatWindowPreference(window);
                }

                return CommandResult.Succeeded;
            }
            finally {
                _executingAllWindowClose = false;
            }
        }

        private void WindowClosedHandler(object sender, FormClosedEventArgs arg) {
            MainWindow w = (MainWindow)sender;
            if (!_executingAllWindowClose) { //最後のウィンドウが普通に閉じられた場合
                _preferences.WindowArray.Clear();
                _preferences.FormatWindowPreference(w);
            }
            _windows.Remove(w);
            NotifyMainWindowUnloaded(w);
            if (_windows.Count == 0 && GetStartMode() == StartMode.StandAlone) {
                CloseAllPopupWindows();
                _appContext.ExitThread();
            }
        }

        public override void TerminatePlugin() {
            base.TerminatePlugin();
            if (_windows.Count > 0) {
                CloseAllPopupWindows();
                MainWindow[] t = _windows.ToArray(); //クローズイベント内で_windowsの要素が変化するのでローカルコピーが必要
                foreach (MainWindow w in t)
                    w.Close();
            }
        }

        public void InitializeExtension() {
        }


        #region IWindowManager
        public IPoderosaMainWindow[] MainWindows {
            get {
                return _windows.ToArray();
            }
        }
        public IPoderosaMainWindow ActiveWindow {
            get {
                return _activeWindow;
            }
        }
        public ISelectionService SelectionService {
            get {
                return _selectionService;
            }
        }
        public void ReloadMenu() {
            foreach (MainWindow w in _windows)
                w.ReloadMenu(_menu, true);
        }
        /*
        public void ReloadMenu(string extension_point_name) {
            MainMenuItem item = _menu.FindMainMenuItem(extension_point_name);
            if(item==null) throw new ArgumentException("extension point not found");
            foreach(MainWindow w in _windows) w.ReloadMenu(_menu, item);
        }
         */
        public void ReloadPreference(ICoreServicePreference pref) {
            foreach (MainWindow w in _windows)
                w.ReloadPreference(pref);
        }
        public void ReloadPreference() {
            //デフォルトを使う
            ReloadPreference(_preferences.OriginalPreference);
        }

        //Popup作成
        public IPoderosaPopupWindow CreatePopupView(PopupViewCreationParam viewcreation) {
            PopupViewContainer vc = new PopupViewContainer(viewcreation);
            if (viewcreation.OwnedByCommandTargetWindow)
                vc.Owner = this.ActiveWindow.AsForm();
            vc.ShowInTaskbar = viewcreation.ShowInTaskBar;
            _popupWindows.Add(vc);
            vc.FormClosed += delegate(object sender, FormClosedEventArgs args) {
                _popupWindows.Remove((PopupViewContainer)sender);
            };

            return vc;
        }


        #endregion


        #region IKeyBindChangeListener
        public void OnKeyBindChanged(IKeyBinds newvalues) {
            foreach (MainWindow w in _windows)
                w.ReloadMenu(_menu, false);
        }
        #endregion


        #region ICultureChangeListener
        public void OnCultureChanged(CultureInfo newculture) {
            //メニューのリロード含め全部やる
            CoreUtil.Strings.OnCultureChanged(newculture); //先にリソース更新
            ReloadMenu();
        }
        #endregion

        public ITimerSite CreateTimer(int interval, TimerDelegate callback) {
            return new TimerSite(interval, callback);
        }
        public MainWindowMenu MainMenu {
            get {
                return _menu;
            }
        }
        public WindowPreference WindowPreference {
            get {
                return _preferences;
            }
        }
        public ViewFactoryManager ViewFactoryManager {
            get {
                return _viewFactoryManager;
            }
        }
        #region IWinFormsService
        public object GetDraggingObject(IDataObject data, Type required_type) {
            //TODO IDataObject使わなくていいの？
            if (_draggingObject == null)
                return null;
            else {
                //TODO これちょっといい加減だが
                Debug.Assert(required_type == typeof(IPoderosaDocument));
                return ((TabBarManager.InternalTabKey)_draggingObject).PoderosaDocument;
            }
        }
        public void BypassDragEnter(Control target, DragEventArgs args) {
            ICommandTarget ct = CommandTargetUtil.AsCommandTarget(target as IAdaptable);
            if (ct == null)
                return;

            args.Effect = DragDropEffects.None;
            if (args.Data.GetDataPresent("FileDrop")) {
                string[] filenames = (string[])args.Data.GetData("FileDrop", true);
                IFileDropHandler[] hs = (IFileDropHandler[])_poderosaWorld.PluginManager.FindExtensionPoint(WindowManagerConstants.FILEDROPHANDLER_ID).GetExtensions();
                foreach (IFileDropHandler h in hs) {
                    if (h.CanAccept(ct, filenames)) {
                        args.Effect = DragDropEffects.Link;
                        return;
                    }
                }
            }
        }
        public void BypassDragDrop(Control target, DragEventArgs args) {
            ICommandTarget ct = CommandTargetUtil.AsCommandTarget(target as IAdaptable);
            if (ct == null)
                return;

            if (args.Data.GetDataPresent("FileDrop")) {
                string[] filenames = (string[])args.Data.GetData("FileDrop", true);
                IFileDropHandler[] hs = (IFileDropHandler[])_poderosaWorld.PluginManager.FindExtensionPoint(WindowManagerConstants.FILEDROPHANDLER_ID).GetExtensions();
                foreach (IFileDropHandler h in hs) {
                    if (h.CanAccept(ct, filenames)) {
                        h.DoDropAction(ct, filenames);
                        return;
                    }
                }
            }
        }
        //これはインタフェースメンバではない。MainWindowのWndProcがWM_COPYDATAを捕まえて呼ぶ。
        public void TurningOpenFile(ICommandTarget ct, string filename) {
            string[] filenames = new string[] { filename };
            IFileDropHandler[] hs = (IFileDropHandler[])_poderosaWorld.PluginManager.FindExtensionPoint(WindowManagerConstants.FILEDROPHANDLER_ID).GetExtensions();
            foreach (IFileDropHandler h in hs) {
                if (h.CanAccept(ct, filenames)) {
                    h.DoDropAction(ct, filenames);
                    return;
                }
            }
        }
        #endregion

        public void SetDraggingTabBar(TabKey value) {
            _draggingObject = value;
        }

        private StartMode GetStartMode() {
#if UNITTEST
            return StartMode.Slave;
#else
            //NOTE Preferenceから取得するなどすべきか
            return StartMode.StandAlone;
#endif
        }

        private void CloseAllPopupWindows() {
            PopupViewContainer[] ws = _popupWindows.ToArray();
            foreach (PopupViewContainer w in ws)
                w.Close();
        }

        //ウィンドウ開閉イベント通知
        public void NotifyMainWindowLoaded(MainWindow w) {
            IMainWindowEventHandler[] hs = (IMainWindowEventHandler[])_poderosaWorld.PluginManager.FindExtensionPoint(WindowManagerConstants.MAINWINDOWEVENTHANDLER_ID).GetExtensions();
            foreach (IMainWindowEventHandler h in hs) {
                if (_windows.Count == 0)
                    h.OnFirstMainWindowLoaded(w);
                else
                    h.OnMainWindowLoaded(w);
            }
        }
        public void NotifyMainWindowUnloaded(MainWindow w) {
            IMainWindowEventHandler[] hs = (IMainWindowEventHandler[])_poderosaWorld.PluginManager.FindExtensionPoint(WindowManagerConstants.MAINWINDOWEVENTHANDLER_ID).GetExtensions();
            foreach (IMainWindowEventHandler h in hs) {
                if (_windows.Count == 0)
                    h.OnLastMainWindowUnloaded(w);
                else
                    h.OnMainWindowUnloaded(w);
            }
        }

    }

    internal class TimerSite : ITimerSite {
        private TimerDelegate _callback;
        private Timer _timer;

        public TimerSite(int interval, TimerDelegate callback) {
            _callback = callback;
            _timer = new Timer();
            _timer.Interval = interval;
            _timer.Tick += delegate(object sender, EventArgs ars) {
                try {
                    _callback();
                }
                catch (Exception ex) {
                    RuntimeUtil.ReportException(ex);
                }
            };
            _timer.Enabled = true;
        }

        public void Close() {
            _timer.Stop();
            _timer.Dispose();
        }
    }

    internal class MainWindowArgument {
        private Rectangle _location;
        private FormWindowState _windowState;
        private string _splitInfo;
        private string _toolBarInfo;
        private int _tabRowCount;

        public MainWindowArgument(Rectangle location, FormWindowState state, string split, string toolbar, int tabrowcount) {
            _location = location;
            _windowState = state;
            _splitInfo = split;
            _toolBarInfo = toolbar;
            _tabRowCount = tabrowcount;
        }
        public string ToolBarInfo {
            get {
                return _toolBarInfo;
            }
        }
        public int TabRowCount {
            get {
                return _tabRowCount;
            }
        }

        //フォームへの適用は、OnLoadの前と後で分ける
        public void ApplyToUnloadedWindow(MainWindow f) {
        }

        public void ApplyToLoadedWindow(MainWindow f) {
            const int MARGIN = 3;
            Rectangle titlebarRect =
                new Rectangle(_location.X + MARGIN, _location.Y + MARGIN,
                                Math.Max(_location.Width - MARGIN * 2, 1),
                                Math.Max(SystemInformation.CaptionHeight - MARGIN * 2, 1));
            bool visible = false;
            foreach (Screen s in Screen.AllScreens) {
                if (s.WorkingArea.IntersectsWith(titlebarRect))
                    visible = true;
            }

            if (!visible) {
                Screen baseScreen = null;
                foreach (Screen s in Screen.AllScreens) {
                    if (s.Bounds.IntersectsWith(_location)) {
                        baseScreen = s;
                        break;
                    }
                }
                if (baseScreen == null)
                    baseScreen = Screen.PrimaryScreen;

                Rectangle sb = baseScreen.WorkingArea;
                if (_location.Width > sb.Width)
                    _location.Width = sb.Width;
                if (_location.Height > sb.Height)
                    _location.Height = sb.Height;
                _location.X = sb.X + (sb.Width - _location.Width) / 2;
                _location.Y = sb.Y + (sb.Height - _location.Height) / 2;
            }

            //DesktopBoundsの設定はOnLoadの中じゃないといかんらしい
            f.DesktopBounds = _location;
            f.WindowState = _windowState;

            //頑張ればOnLoad以前にSplitInfoを適用できるかも
            if (_splitInfo.Length > 0) {
                ISplittableViewManager vm = (ISplittableViewManager)f.ViewManager.GetAdapter(typeof(ISplittableViewManager));
                if (vm != null)
                    vm.ApplySplitInfo(_splitInfo);
            }

            //ToolBarのコンポーネント位置調整
            f.ToolBarInternal.RestoreLayout();
        }

        //位置情報の保存と復元
        //この正規表現で示される値で。例 (Max,0,0,1024,768) 位置に負の値を許すことに注意。
        public static MainWindowArgument[] Parse(IWindowPreference pref) {
            int count = pref.WindowCount;

            //マッチしないときはデフォルト
            if (count == 0) {
                //初期状態で最小化は許さず
                MainWindowArgument arg = new MainWindowArgument(GetInitialLocation(), FormWindowState.Normal, "", "", 1);
                return new MainWindowArgument[] { arg };
            }
            else {
                //正規表現内のコメント: ソースを表示するフォント次第ではおかしいかも
                //                      (<FormWindowState>, left,      ,     top,         ,    width       ,     height   )  
                Regex re = new Regex("\\((Max,|Min,)?\\s*(-?[\\d]+)\\s*,\\s*(-?[\\d]+)\\s*,\\s*([\\d]+)\\s*,\\s*([\\d]+)\\)");

                MainWindowArgument[] result = new MainWindowArgument[count];
                for (int i = 0; i < count; i++) {
                    string positions = pref.WindowPositionAt(i);

                    Match m = re.Match(positions);
                    GroupCollection gc = m.Groups;
                    Debug.Assert(gc.Count == 6); //自身と子要素５つ
                    //なお、最小化したまま終了しても次回起動時はノーマルサイズで。
                    result[i] = new MainWindowArgument(
                      ParseRectangle(gc[2].Value, gc[3].Value, gc[4].Value, gc[5].Value),
                      gc[1].Value == "Max," ? FormWindowState.Maximized : FormWindowState.Normal, //カンマつきに注意
                      pref.WindowSplitFormatAt(i), pref.ToolBarFormatAt(i), pref.TabRowCountAt(i));
                }
                return result;
            }
        }

        private static Rectangle ParseRectangle(string left, string top, string width, string height) {
            try {
                Rectangle r = new Rectangle(Int32.Parse(left), Int32.Parse(top), Int32.Parse(width), Int32.Parse(height));
                return r;
            }
            catch (FormatException) {
                return GetInitialLocation();
            }
        }

        private static Rectangle GetInitialLocation() {
            //プライマリスクリーンの半分のサイズを中央に
            Rectangle r = Screen.PrimaryScreen.Bounds;
            return new Rectangle(r.X + r.Width / 4, r.Y + r.Height / 4, r.Width / 2, r.Height / 2);
        }

    }

    internal class PoderosaAppContext : ApplicationContext {
        public PoderosaAppContext() {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.VisualStyleState = System.Windows.Forms.VisualStyles.VisualStyleState.ClientAndNonClientAreasEnabled;
        }

    }


    internal enum StartMode {
        StandAlone,
        Slave
    }
}

namespace Poderosa {
    //このアセンブリのStringResourceへのアクセサ WindowManagerに代表させるのはいかんカンジ
    internal static class CoreUtil {
        private static StringResource _strings;
        public static StringResource Strings {
            get {
                if (_strings == null)
                    _strings = new StringResource("Poderosa.Core.strings", typeof(CoreUtil).Assembly, true);
                return _strings;
            }
        }
    }
}
