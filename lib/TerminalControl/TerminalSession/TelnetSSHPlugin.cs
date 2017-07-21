/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TelnetSSHPlugin.cs,v 1.4 2011/10/27 23:21:59 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Plugins;
using Poderosa.Commands;
using Poderosa.Terminal;
using Poderosa.ConnectionParam;
using Poderosa.Protocols;
using Poderosa.Forms;
using Poderosa.MacroEngine;

[assembly: PluginDeclaration(typeof(Poderosa.Sessions.TelnetSSHPlugin))]

namespace Poderosa.Sessions {
    [PluginInfo(ID = "org.poderosa.telnet_ssh", Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.core.window")]
    internal class TelnetSSHPlugin : PluginBase {

        private static TelnetSSHPlugin _instance;

        private ICommandManager _commandManager;
        private LoginDialogCommand _loginDialogCommand;
        private LoginMenuGroup _loginMenuGroup;
        private LoginToolBarComponent _loginToolBarComponent;
        private IMacroEngine _macroEngine;

        public static TelnetSSHPlugin Instance {
            get {
                return _instance;
            }
        }

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;

            IPluginManager pm = poderosa.PluginManager;
            _commandManager = (ICommandManager)pm.FindPlugin("org.poderosa.core.commands", typeof(ICommandManager));
            _loginDialogCommand = new LoginDialogCommand();
            _commandManager.Register(_loginDialogCommand);

            IExtensionPoint ep = pm.FindExtensionPoint("org.poderosa.menu.file");
            _loginMenuGroup = new LoginMenuGroup();
            ep.RegisterExtension(_loginMenuGroup);

            IExtensionPoint toolbar = pm.FindExtensionPoint("org.poderosa.core.window.toolbar");
            _loginToolBarComponent = new LoginToolBarComponent();
            toolbar.RegisterExtension(_loginToolBarComponent);
        }

        public IPoderosaMenuGroup TelnetSSHMenuGroup {
            get {
                return _loginMenuGroup;
            }
        }
        public IToolBarComponent TelnetSSHToolBar {
            get {
                return _loginToolBarComponent;
            }
        }

        public IMacroEngine MacroEngine {
            get {
                if (_macroEngine == null) {
                    _macroEngine = _poderosaWorld.PluginManager.FindPlugin("org.poderosa.macro", typeof(IMacroEngine)) as IMacroEngine;
                }
                return _macroEngine;
            }
        }

        private class LoginMenuGroup : IPoderosaMenuGroup, IPositionDesignation {
            public IPoderosaMenu[] ChildMenus {
                get {
                    return new IPoderosaMenu[] { new LoginMenuItem() };
                }
            }

            public bool IsVolatileContent {
                get {
                    return false;
                }
            }
            public bool ShowSeparator {
                get {
                    return true;
                }
            }

            public IAdaptable GetAdapter(Type adapter) {
                return _instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }

            public IAdaptable DesignationTarget {
                get {
                    return null;
                }
            }

            public PositionType DesignationPosition {
                get {
                    return PositionType.First;
                }
            }
        }

        private class LoginMenuItem : IPoderosaMenuItem {
            public IPoderosaCommand AssociatedCommand {
                get {
                    return _instance._loginDialogCommand;
                }
            }

            public string Text {
                get {
                    return TEnv.Strings.GetString("Menu.NewConnection");
                }
            }

            public bool IsEnabled(ICommandTarget target) {
                return true;
            }
            public bool IsChecked(ICommandTarget target) {
                return false;
            }

            public IAdaptable GetAdapter(Type adapter) {
                return _instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }
        }

        private class LoginToolBarComponent : IToolBarComponent, IPositionDesignation {

            public IAdaptable DesignationTarget {
                get {
                    return null;
                }
            }

            public PositionType DesignationPosition {
                get {
                    return PositionType.First;
                }
            }

            public bool ShowSeparator {
                get {
                    return true;
                }
            }
            public IToolBarElement[] ToolBarElements {
                get {
                    return new IToolBarElement[] { new ToolBarCommandButtonImpl(_instance._loginDialogCommand, Poderosa.TerminalSession.Properties.Resources.NewConnection16x16) };
                }
            }

            public IAdaptable GetAdapter(Type adapter) {
                return _instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }

        }


        private class LoginDialogCommand : IGeneralCommand {
            public CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
                IPoderosaMainWindow window = (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
                if (window == null)
                    return CommandResult.Ignored;

#if OLD_LOGIN_DIALOG
                TelnetSSHLoginDialog dlg = new TelnetSSHLoginDialog(window);

                dlg.ApplyParam();

                CommandResult res = CommandResult.Cancelled;
                if (dlg.ShowDialog() == DialogResult.OK) {
                    ITerminalConnection con = dlg.Result;
                    if (con != null) {
                        ISessionManager sm = (ISessionManager)TelnetSSHPlugin.Instance.PoderosaWorld.PluginManager.FindPlugin("org.poderosa.core.sessions", typeof(ISessionManager));
                        TerminalSession ts = new TerminalSession(con, dlg.TerminalSettings);
                        sm.StartNewSession(ts, (IPoderosaView)dlg.TargetView.GetAdapter(typeof(IPoderosaView)));
                        sm.ActivateDocument(ts.Terminal.IDocument, ActivateReason.InternalAction);

                        IAutoExecMacroParameter autoExecParam = con.Destination.GetAdapter(typeof(IAutoExecMacroParameter)) as IAutoExecMacroParameter;
                        if (autoExecParam != null && autoExecParam.AutoExecMacroPath != null && TelnetSSHPlugin.Instance.MacroEngine != null) {
                            TelnetSSHPlugin.Instance.MacroEngine.RunMacro(autoExecParam.AutoExecMacroPath, ts);
                        }

                        return CommandResult.Succeeded;
                    }
                }
                dlg.Dispose();

                return res;
#else
                using (OpenSessionDialog dlg = new OpenSessionDialog(window)) {
                    if (dlg.ShowDialog() == DialogResult.OK) {
                        IContentReplaceableView view = (IContentReplaceableView)window.ViewManager.GetCandidateViewForNewDocument().GetAdapter(typeof(IContentReplaceableView));
                        IPoderosaView targetView = view.AssureViewClass(typeof(TerminalView));

                        ISessionManager sm = (ISessionManager)TelnetSSHPlugin.Instance.PoderosaWorld.PluginManager.FindPlugin("org.poderosa.core.sessions", typeof(ISessionManager));
                        TerminalSession ts = new TerminalSession(dlg.TerminalConnection, dlg.TerminalSettings);
                        sm.StartNewSession(ts, targetView);
                        sm.ActivateDocument(ts.Terminal.IDocument, ActivateReason.InternalAction);

                        IAutoExecMacroParameter autoExecParam = dlg.TerminalConnection.Destination.GetAdapter(typeof(IAutoExecMacroParameter)) as IAutoExecMacroParameter;
                        if (autoExecParam != null && autoExecParam.AutoExecMacroPath != null && TelnetSSHPlugin.Instance.MacroEngine != null) {
                            TelnetSSHPlugin.Instance.MacroEngine.RunMacro(autoExecParam.AutoExecMacroPath, ts);
                        }
                        return CommandResult.Succeeded;
                    }
                    return CommandResult.Cancelled;
                }
#endif
            }

            public string CommandID {
                get {
                    return "org.poderosa.session.telnetSSH";
                }
            }
            public Keys DefaultShortcutKey {
                get {
                    return Keys.Alt | Keys.N;
                }
            }
            public string Description {
                get {
                    return TEnv.Strings.GetString("Command.TelnetSSH");
                }
            }
            public ICommandCategory CommandCategory {
                get {
                    return ConnectCommandCategory._instance;
                }
            }

            public bool CanExecute(ICommandTarget target) {
                return true;
            }

            public IAdaptable GetAdapter(Type adapter) {
                return _instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }
        }
    }
}
