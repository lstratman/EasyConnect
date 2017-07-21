/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalEmulatorPlugin.cs,v 1.3 2011/10/27 23:21:58 kzmi Exp $
 */
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

using Poderosa.Plugins;
using Poderosa.Sessions;
using Poderosa.Preferences;
using Poderosa.Forms;
using Poderosa.Commands;

[assembly: PluginDeclaration(typeof(Poderosa.Terminal.TerminalEmulatorPlugin))]

namespace Poderosa.Terminal {
    /// <summary>
    /// <ja>
    /// TerminalEmulatorPluginプラグインが提供する拡張ポイントです。
    /// </ja>
    /// <en>
    /// Extension point that TerminalEmulatorPlugin plug-in offers.
    /// </en>
    /// </summary>
    /// <exclude/>
    public static class TerminalEmulatorConstants {
        public const string TERMINAL_CONTEXT_MENU_EXTENSIONPOINT = "org.poderosa.terminalemulator.contextMenu";
        public const string DOCUMENT_CONTEXT_MENU_EXTENSIONPOINT = "org.poderosa.terminalemulator.documentContextMenu";
        public const string TERMINALSPECIAL_EXTENSIONPOINT = "org.poderosa.terminalemulator.specialCommand";
        public const string INTELLISENSE_CANDIDATE_EXTENSIONPOINT = "org.poderosa.terminalemulator.intellisense";
        public const string LOG_FILENAME_FORMATTER_EXTENSIONPOINT = "org.poderosa.terminalemulator.logFileNameFormatter";
        public const string DYNAMIC_CAPTION_FORMATTER_EXTENSIONPOINT = "org.poderosa.terminalemulator.dynamicCaptionFormatter";
    }

    [PluginInfo(ID = TerminalEmulatorPlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.core.sessions;org.poderosa.core.commands")]
    internal class TerminalEmulatorPlugin : PluginBase, ITerminalEmulatorService {
        public const string PLUGIN_ID = "org.poderosa.terminalemulator";

        private ICoreServices _coreServices;
        private ICommandManager _commandManager;
        private IExtensionPoint _contextMenu;
        private IExtensionPoint _documentContextMenu;
        private IExtensionPoint _intelliSenseExtension;
        private IExtensionPoint _autoLogFileFormatter;
        private IExtensionPoint _dynamicCaptionFormatter;
        private TerminalOptionsSupplier _optionSupplier;
        private KeepAlive _keepAlive;
        private CustomKeySettings _customKeySettings;
        private ShellSchemeCollection _shellSchemeCollection;
        private PromptCheckerWithTimer _promptCheckerWithTimer;

        private bool _laterInitialized; //遅延初期化用フラグ

        private static TerminalEmulatorPlugin _instance;
        public static TerminalEmulatorPlugin Instance {
            get {
                return _instance;
            }
        }

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;
            _optionSupplier = new TerminalOptionsSupplier();
            _keepAlive = new KeepAlive();
            _customKeySettings = new CustomKeySettings();
            _shellSchemeCollection = new ShellSchemeCollection();

            GEnv.Init();
            IPluginManager pm = poderosa.PluginManager;
            ICoreServices cs = (ICoreServices)poderosa.GetAdapter(typeof(ICoreServices));
            Debug.Assert(cs != null);
            cs.PreferenceExtensionPoint.RegisterExtension(_optionSupplier);
            cs.PreferenceExtensionPoint.RegisterExtension(_shellSchemeCollection);
            _coreServices = cs;

            //Serialize Service
            cs.SerializerExtensionPoint.RegisterExtension(new TerminalSettingsSerializer(pm));

            _commandManager = cs.CommandManager;
            TerminalCommand.Register(_commandManager);
            TerminalSettingMenuGroup.Initialize();

            //PromptChecker
            _promptCheckerWithTimer = new PromptCheckerWithTimer();

            //Edit Menuに追加
            IExtensionPoint editmenu = pm.FindExtensionPoint("org.poderosa.menu.edit");
            editmenu.RegisterExtension(new AdvancedCopyPasteMenuGroup());
            editmenu.RegisterExtension(new TerminalBufferMenuGroup());
            editmenu.RegisterExtension(new SelectionMenuGroup());

            //Console Menu : これは処置に困るところだが！
            IExtensionPoint consolemenu = pm.FindExtensionPoint("org.poderosa.menu.console");
            consolemenu.RegisterExtension(new TerminalSettingMenuGroup());
            consolemenu.RegisterExtension(new IntelliSenseMenuGroup());

            //Context Menu
            _contextMenu = pm.CreateExtensionPoint(TerminalEmulatorConstants.TERMINAL_CONTEXT_MENU_EXTENSIONPOINT, typeof(IPoderosaMenuGroup), this);
            _contextMenu.RegisterExtension(new BasicCopyPasteMenuGroup());
            _contextMenu.RegisterExtension(new TerminalSettingMenuGroup());
            _contextMenu.RegisterExtension(new IntelliSenseMenuGroup());

            //タブのコンテキストメニュー
            _documentContextMenu = pm.CreateExtensionPoint(TerminalEmulatorConstants.DOCUMENT_CONTEXT_MENU_EXTENSIONPOINT, typeof(IPoderosaMenuGroup), this);
            _documentContextMenu.RegisterExtension(new PoderosaMenuGroupImpl(new PoderosaMenuItemImpl(
                cs.CommandManager.Find("org.poderosa.core.session.closedocument"), GEnv.Strings, "Menu.DocumentClose")));

            //ToolBar
            IExtensionPoint toolbar = pm.FindExtensionPoint("org.poderosa.core.window.toolbar");
            TerminalToolBar terminaltoolbar = new TerminalToolBar();
            toolbar.RegisterExtension(terminaltoolbar);
            GetSessionManager().AddActiveDocumentChangeListener(terminaltoolbar);

            //その他 Extension
            _intelliSenseExtension = pm.CreateExtensionPoint(TerminalEmulatorConstants.INTELLISENSE_CANDIDATE_EXTENSIONPOINT, typeof(IIntelliSenseCandidateExtension), this);
            _autoLogFileFormatter = pm.CreateExtensionPoint(TerminalEmulatorConstants.LOG_FILENAME_FORMATTER_EXTENSIONPOINT, typeof(IAutoLogFileFormatter), this);
            _dynamicCaptionFormatter = pm.CreateExtensionPoint(TerminalEmulatorConstants.DYNAMIC_CAPTION_FORMATTER_EXTENSIONPOINT, typeof(IDynamicCaptionFormatter), this);

            //Command Popup
            CommandResultSession.Init(poderosa);
            PopupStyleCommandResultRecognizer.CreateExtensionPointAndDefaultCommands(pm);

            // Preferences for PromptRecognizer
            cs.PreferenceExtensionPoint.RegisterExtension(PromptRecognizerPreferences.Instance);

            // Preferences for XTerm
            cs.PreferenceExtensionPoint.RegisterExtension(XTermPreferences.Instance);
        }

        #region ITerminalEmulatorPlugin
        public ITerminalEmulatorOptions TerminalEmulatorOptions {
            get {
                return GEnv.Options;
            }
        }
        public ITerminalSettings CreateDefaultTerminalSettings(string caption, Image icon) {
            TerminalSettings t = new TerminalSettings();
            t.BeginUpdate();
            t.Icon = icon;
            t.Caption = caption;
            t.EnabledCharTriggerIntelliSense = GEnv.Options.EnableComplementForNewConnections;
            t.EndUpdate();
            return t;
        }
        public ISimpleLogSettings CreateDefaultSimpleLogSettings() {
            return new SimpleLogSettings();
        }
        public IPoderosaMenuGroup[] ContextMenu {
            get {
                return (IPoderosaMenuGroup[])_contextMenu.GetExtensions();
            }
        }
        public IPoderosaMenuGroup[] DocumentContextMenu {
            get {
                return (IPoderosaMenuGroup[])_documentContextMenu.GetExtensions();
            }
        }
        public IIntelliSenseCandidateExtension[] IntelliSenseExtensions {
            get {
                return (IIntelliSenseCandidateExtension[])_intelliSenseExtension.GetExtensions();
            }
        }
        public IAutoLogFileFormatter[] AutoLogFileFormatter {
            get {
                return (IAutoLogFileFormatter[])_autoLogFileFormatter.GetExtensions();
            }
        }
        public IDynamicCaptionFormatter[] DynamicCaptionFormatter {
            get {
                return (IDynamicCaptionFormatter[])_dynamicCaptionFormatter.GetExtensions();
            }
        }

        public ICommandCategory TerminalCommandCategory {
            get {
                return TerminalCommand.TerminalCommandCategory;
            }
        }
        public void LaterInitialize() {
            if (!_laterInitialized)
                _shellSchemeCollection.Load();
            _laterInitialized = true;
        }
        public IShellSchemeCollection ShellSchemeCollection {
            get {
                return _shellSchemeCollection;
            }
        }
        #endregion

        public ISessionManager GetSessionManager() {
            return _coreServices.SessionManager;
        }
        public IWinFormsService GetWinFormsService() {
            return (IWinFormsService)_coreServices.WindowManager.GetAdapter(typeof(IWinFormsService));
        }
        public IWindowManager GetWindowManager() {
            return _coreServices.WindowManager;
        }
        public ICommandManager GetCommandManager() {
            return _commandManager;
        }
        public IPoderosaApplication GetPoderosaApplication() {
            return (IPoderosaApplication)_poderosaWorld.GetAdapter(typeof(IPoderosaApplication));
        }
        public TerminalOptionsSupplier OptionSupplier {
            get {
                return _optionSupplier;
            }
        }
        public KeepAlive KeepAlive {
            get {
                return _keepAlive;
            }
        }
        public CustomKeySettings CustomKeySettings {
            get {
                return _customKeySettings;
            }
        }

        public override void TerminatePlugin() {
            base.TerminatePlugin();
            _shellSchemeCollection.PreClose();
            _promptCheckerWithTimer.Close();
        }
    }

    internal class CustomKeySettings {
        private FixedStyleKeyFunction _keyFunction;

        public void Reset(ITerminalEmulatorOptions opt) {
            //TODO ここはPeripheralPanelとかぶっている。なんとかしたい
            StringBuilder bld = new StringBuilder();
            if (opt.Send0x7FByDel)
                bld.Append("Delete=0x7F");
            if (opt.Send0x7FByBack) {
                if (bld.Length > 0)
                    bld.Append(", ");
                bld.Append("Back=0x7F");
            }

            KeyboardStyle ks = opt.Zone0x1F;
            if (ks != KeyboardStyle.None) {
                string s;
                if (ks == KeyboardStyle.Default)
                    s = "Ctrl+D6=0x1E, Ctrl+Minus=0x1F";
                else //Japanese
                    s = "Ctrl+BackSlash=0x1F";
                if (bld.Length > 0)
                    bld.Append(", ");
                bld.Append(s);
            }

            if (opt.CustomKeySettings.Length > 0) {
                if (bld.Length > 0)
                    bld.Append(", ");
                bld.Append(opt.CustomKeySettings);
            }

            //仕上げ。パースエラーがちょっとアレだ
            _keyFunction = KeyFunction.Parse(bld.ToString()).ToFixedStyle();
        }

        public char[] Scan(Keys key) {
            //TODO この実装だと、パースエラーのあるとき、キーを押した時点でエラーになる。これはまずい。PreferenceListenerにロード完了通知が欲しい
            if (_keyFunction == null)
                Reset(GEnv.Options);

            //実行頻度高いので、いちおう、IEnumerator使わない。
            for (int i = 0; i < _keyFunction._keys.Length; i++) {
                if (_keyFunction._keys[i] == key)
                    return _keyFunction._datas[i];
            }
            return null;
        }
    }

    //タイマーでプロンプト認識
    internal class PromptCheckerWithTimer {
        //private ITimerSite _timerSite;
        public PromptCheckerWithTimer() {
            //IntelliSenseに副作用あるので一時停止中
            //_timerSite = TerminalEmulatorPlugin.Instance.GetWinFormsService().CreateTimer(1000, new TimerDelegate(OnTimer));
        }
        public void Close() {
            //_timerSite.Close();
        }

        private void OnTimer() {
            //全ターミナルに一斉処置
            ISessionManager sm = TerminalEmulatorPlugin.Instance.GetSessionManager();
            foreach (ISession s in sm.AllSessions) {
                //ちょっと裏技的だが
                ITerminalControlHost tc = (ITerminalControlHost)s.GetAdapter(typeof(ITerminalControlHost));
                if (tc != null) {
                    tc.Terminal.PromptRecognizer.CheckIfUpdated();
                }
            }
        }
    }

}
