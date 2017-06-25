/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSessionPlugin.cs,v 1.3 2011/10/27 23:21:59 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Poderosa.Plugins;
using Poderosa.Util;
using Poderosa.Forms;
using Poderosa.Terminal;
using Poderosa.UI;
using Poderosa.Protocols;
using Poderosa.Commands;
using Poderosa.Preferences;
using Poderosa.Serializing;

[assembly: PluginDeclaration(typeof(Poderosa.Sessions.TerminalSessionsPlugin))]

namespace Poderosa.Sessions {
    [PluginInfo(ID = TerminalSessionsPlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.core.sessions;org.poderosa.terminalemulator;org.poderosa.protocols")]
    internal class TerminalSessionsPlugin : PluginBase, ITerminalSessionsService {
        public const string PLUGIN_ID = "org.poderosa.terminalsessions";

        public const string TERMINAL_CONNECTION_FACTORY_ID = "org.poderosa.termianlsessions.terminalConnectionFactory";

        private static TerminalSessionsPlugin _instance;

        private ICoreServices _coreServices;
        private TerminalSessionOptionsSupplier _terminalSessionsOptionSupplier;
        private IProtocolService _protocolService;
        private ITerminalEmulatorService _terminalEmulatorService;

        private TerminalViewFactory _terminalViewFactory;
        private PaneBridgeAdapter _paneBridgeAdapter;

        private StartCommand _startCommand;
        private ReproduceCommand _reproduceCommand;
        private IPoderosaCommand _pasteCommand;
        private IExtensionPoint _pasteCommandExt;

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;

            IPluginManager pm = poderosa.PluginManager;
            _coreServices = (ICoreServices)poderosa.GetAdapter(typeof(ICoreServices));
            TEnv.ReloadStringResource();
            _terminalViewFactory = new TerminalViewFactory();
            pm.FindExtensionPoint(WindowManagerConstants.VIEW_FACTORY_ID).RegisterExtension(_terminalViewFactory);
            //このViewFactoryはデフォ
            foreach (IViewManagerFactory mf in pm.FindExtensionPoint(WindowManagerConstants.MAINWINDOWCONTENT_ID).GetExtensions())
                mf.DefaultViewFactory = _terminalViewFactory;

            //ログインダイアログのサポート用
            pm.CreateExtensionPoint("org.poderosa.terminalsessions.telnetSSHLoginDialogInitializer", typeof(ITelnetSSHLoginDialogInitializer), this);
            pm.CreateExtensionPoint("org.poderosa.terminalsessions.loginDialogUISupport", typeof(ILoginDialogUISupport), this);
            pm.CreateExtensionPoint("org.poderosa.terminalsessions.terminalParameterStore", typeof(ITerminalSessionParameterStore), this);
            IExtensionPoint factory_point = pm.CreateExtensionPoint(TERMINAL_CONNECTION_FACTORY_ID, typeof(ITerminalConnectionFactory), this);

            _pasteCommandExt = pm.CreateExtensionPoint("org.poderosa.terminalsessions.pasteCommand", typeof(IPoderosaCommand), this);

            _terminalSessionsOptionSupplier = new TerminalSessionOptionsSupplier();
            _coreServices.PreferenceExtensionPoint.RegisterExtension(_terminalSessionsOptionSupplier);


            //Add conversion for TerminalPane
            _paneBridgeAdapter = new PaneBridgeAdapter();
            poderosa.AdapterManager.RegisterFactory(_paneBridgeAdapter);

            _startCommand = new StartCommand(factory_point);
            _reproduceCommand = new ReproduceCommand();
            _coreServices.CommandManager.Register(_reproduceCommand);

            ReproduceMenuGroup rmg = new ReproduceMenuGroup();
            IExtensionPoint consolemenu = pm.FindExtensionPoint("org.poderosa.menu.console");
            consolemenu.RegisterExtension(rmg);

            IExtensionPoint contextmenu = pm.FindExtensionPoint("org.poderosa.terminalemulator.contextMenu");
            contextmenu.RegisterExtension(rmg);

            IExtensionPoint documentContext = pm.FindExtensionPoint("org.poderosa.terminalemulator.documentContextMenu");
            documentContext.RegisterExtension(rmg);
        }

        public static TerminalSessionsPlugin Instance {
            get {
                return _instance;
            }
        }

        public IProtocolService ProtocolService {
            get {
                if (_protocolService == null)
                    _protocolService = (IProtocolService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.protocols", typeof(IProtocolService));
                return _protocolService;
            }
        }

        public ITerminalEmulatorService TerminalEmulatorService {
            get {
                if (_terminalEmulatorService == null)
                    _terminalEmulatorService = (ITerminalEmulatorService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.terminalemulator", typeof(ITerminalEmulatorService));
                return _terminalEmulatorService;
            }
        }
        public ISerializeService SerializeService {
            get {
                return _coreServices.SerializeService;
            }
        }
        public IWindowManager WindowManager {
            get {
                return _coreServices.WindowManager;
            }
        }
        public PaneBridgeAdapter PaneBridgeAdapter {
            get {
                return _paneBridgeAdapter;
            }
        }

        public TerminalViewFactory TerminalViewFactory {
            get {
                return _terminalViewFactory;
            }
        }

        public ITerminalSessionOptions TerminalSessionOptions {
            get {
                return _terminalSessionsOptionSupplier.OriginalOptions;
            }
        }

        public ICommandManager CommandManager {
            get {
                return _coreServices.CommandManager;
            }
        }
        public ISessionManager SessionManager {
            get {
                return _coreServices.SessionManager;
            }
        }

        public ICommandCategory ConnectCommandCategory {
            get {
                return Poderosa.Sessions.ConnectCommandCategory._instance;
            }
        }

        #region ITerminalSessionService
        public ITerminalSessionStartCommand TerminalSessionStartCommand {
            get {
                return _startCommand;
            }
        }
        #endregion

        public ReproduceCommand ReproduceCommand {
            get {
                return _reproduceCommand;
            }
        }
        public ICoreServicePreference CoreServicesPreference {
            get {
                IPreferenceFolder folder = _coreServices.Preferences.FindPreferenceFolder("org.poderosa.core.window");
                return (ICoreServicePreference)folder.QueryAdapter(typeof(ICoreServicePreference));
            }
        }

        /// <summary>
        /// Get a Paste command object
        /// </summary>
        /// <remarks>
        /// If an instance was registered on the extension point, this method returns it.
        /// Otherwise, returns the default implementation.
        /// </remarks>
        /// <returns></returns>
        public IPoderosaCommand GetPasteCommand() {
            if (_pasteCommand == null) {
                if (_pasteCommandExt != null && _pasteCommandExt.GetExtensions().Length > 0) {
                    _pasteCommand = ((IPoderosaCommand[])_pasteCommandExt.GetExtensions())[0];
                }
                else {
                    _pasteCommand = new PasteToTerminalCommand();
                }
            }
            return _pasteCommand;
        }

    }

    internal class TEnv {
        private static StringResource _stringResource;

        public static void ReloadStringResource() {
            _stringResource = new StringResource("Poderosa.TerminalSession.strings", typeof(TEnv).Assembly);
            TerminalSessionsPlugin.Instance.PoderosaWorld.Culture.AddChangeListener(_stringResource);
        }

        public static StringResource Strings {
            get {
                return _stringResource;
            }
        }
    }

    internal class LoginDialogInitializeInfo : ITelnetSSHLoginDialogInitializeInfo {

        private List<string> _hosts;
        private List<string> _accounts;
        private List<string> _identityFiles;
        private List<int> _ports;

        public LoginDialogInitializeInfo() {
            _hosts = new List<string>();
            _accounts = new List<string>();
            _identityFiles = new List<string>();
            _ports = new List<int>();
            _ports.Add(22);
            _ports.Add(23); //これらはデフォ
        }

        public string[] Hosts {
            get {
                return _hosts.ToArray();
            }
        }

        public string[] Accounts {
            get {
                return _accounts.ToArray();
            }
        }

        public string[] IdentityFiles {
            get {
                return _identityFiles.ToArray();
            }
        }

        public int[] Ports {
            get {
                return _ports.ToArray();
            }
        }

        #region ITelnetSSHLoginDialogInitializeInfo
        public void AddHost(string value) {
            if (!_hosts.Contains(value) && value.Length > 0)
                _hosts.Add(value);
        }

        public void AddAccount(string value) {
            if (!_accounts.Contains(value) && value.Length > 0)
                _accounts.Add(value);
        }

        public void AddIdentityFile(string value) {
            if (!_identityFiles.Contains(value) && value.Length > 0)
                _identityFiles.Add(value);
        }

        public void AddPort(int value) {
            if (!_ports.Contains(value))
                _ports.Add(value);
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion
    }
}
