/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CygwinPlugin.cs,v 1.5 2011/10/27 23:21:58 kzmi Exp $
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

[assembly: PluginDeclaration(typeof(Poderosa.Sessions.CygwinPlugin))]

namespace Poderosa.Sessions {
    //TODO シリアルポートが位置指定するためにpublicにしたが、文字列で検索できるようにすべき
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ICygwinPlugin : IAdaptable {
        IPoderosaMenuGroup CygwinMenuGroupTemp {
            get;
        }
        IToolBarComponent CygwinToolBarComponentTemp {
            get;
        }

        ITerminalSettings CreateDefaultCygwinTerminalSettings();
    }

    [PluginInfo(ID = "org.poderosa.cygwin", Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.core.window")]
    internal class CygwinPlugin : PluginBase, ICygwinPlugin {

        private static CygwinPlugin _instance;
        public static CygwinPlugin Instance {
            get {
                return _instance;
            }
        }
        private ICommandManager _commandManager;
        private CygwinLoginDialogCommand _loginDialogCommand;
        private IPoderosaMenuGroup _cygwinMenuGroup;
        private IToolBarComponent _cygwinToolBarComponent;
        private IMacroEngine _macroEngine;

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;

            IPluginManager pm = poderosa.PluginManager;
            _commandManager = (ICommandManager)pm.FindPlugin("org.poderosa.core.commands", typeof(ICommandManager));
            _loginDialogCommand = new CygwinLoginDialogCommand();
            _commandManager.Register(_loginDialogCommand);

            IExtensionPoint ep = poderosa.PluginManager.FindExtensionPoint("org.poderosa.menu.file");
            _cygwinMenuGroup = new CygwinMenuGroup();
            ep.RegisterExtension(_cygwinMenuGroup);

            _cygwinToolBarComponent = new CygwinToolBarComponent();
            poderosa.PluginManager.FindExtensionPoint("org.poderosa.core.window.toolbar").RegisterExtension(_cygwinToolBarComponent);
        }

        private class CygwinMenuGroup : IPoderosaMenuGroup, IPositionDesignation {
            public IPoderosaMenu[] ChildMenus {
                get {
                    return new IPoderosaMenu[] { new CygwinMenuItem() };
                }
            }

            public bool IsVolatileContent {
                get {
                    return false;
                }
            }

            public bool ShowSeparator {
                get {
                    return false; //CygwinはTelnet/SSH接続の直後に来る。セパレータ不要
                }
            }

            public IAdaptable GetAdapter(Type adapter) {
                return _instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }

            public IAdaptable DesignationTarget {
                get {
                    return TelnetSSHPlugin.Instance.TelnetSSHMenuGroup;
                }
            }

            public PositionType DesignationPosition {
                get {
                    return PositionType.NextTo;
                }
            }
        }

        private class CygwinMenuItem : IPoderosaMenuItem {
            public IPoderosaCommand AssociatedCommand {
                get {
                    return _instance._loginDialogCommand;
                }
            }

            public string Text {
                get {
                    return TEnv.Strings.GetString("Menu.CygwinNewConnection");
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

        private class CygwinToolBarComponent : IToolBarComponent, IPositionDesignation {

            public IAdaptable DesignationTarget {
                get {
                    return TelnetSSHPlugin.Instance.TelnetSSHToolBar;
                }
            }

            public PositionType DesignationPosition {
                get {
                    return PositionType.NextTo;
                }
            }

            public bool ShowSeparator {
                get {
                    return true;
                }
            }

            public IToolBarElement[] ToolBarElements {
                get {
                    return new IToolBarElement[] { new ToolBarCommandButtonImpl(_instance._loginDialogCommand, Poderosa.TerminalSession.Properties.Resources.Cygwin16x16) };
                }
            }

            public IAdaptable GetAdapter(Type adapter) {
                return _instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }

        }

        private class CygwinLoginDialogCommand : IGeneralCommand {
            public CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
                IPoderosaMainWindow window = (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
                if (window == null)
                    return CommandResult.Ignored;
                LocalShellLoginDialog dlg = new LocalShellLoginDialog(window);
                using (dlg) {

                    dlg.ApplyParam();

                    if (dlg.ShowDialog(window.AsForm()) == DialogResult.OK) {
                        ITerminalConnection con = dlg.Result;
                        if (con != null) {
                            ISessionManager sm = (ISessionManager)CygwinPlugin.Instance.PoderosaWorld.PluginManager.FindPlugin("org.poderosa.core.sessions", typeof(ISessionManager));
                            TerminalSession ts = new TerminalSession(con, dlg.TerminalSettings);
                            sm.StartNewSession(ts, dlg.TargetView);
                            sm.ActivateDocument(ts.Terminal.IDocument, ActivateReason.InternalAction);

                            IAutoExecMacroParameter autoExecParam = con.Destination.GetAdapter(typeof(IAutoExecMacroParameter)) as IAutoExecMacroParameter;
                            if (autoExecParam != null && autoExecParam.AutoExecMacroPath != null && CygwinPlugin.Instance.MacroEngine != null) {
                                CygwinPlugin.Instance.MacroEngine.RunMacro(autoExecParam.AutoExecMacroPath, ts);
                            }

                            return CommandResult.Succeeded;
                        }
                    }
                }

                return CommandResult.Cancelled;
            }

            public string CommandID {
                get {
                    return "org.poderosa.session.cygwin";
                }
            }
            public string Description {
                get {
                    return TEnv.Strings.GetString("Command.Cygwin");
                }
            }
            public Keys DefaultShortcutKey {
                get {
                    return Keys.None;
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


        public IPoderosaMenuGroup CygwinMenuGroupTemp {
            get {
                return _cygwinMenuGroup;
            }
        }

        public IToolBarComponent CygwinToolBarComponentTemp {
            get {
                return _cygwinToolBarComponent;
            }
        }

        public ITerminalSettings CreateDefaultCygwinTerminalSettings() {
            ITerminalSettings settings = TerminalSessionsPlugin.Instance.TerminalEmulatorService.CreateDefaultTerminalSettings("", Poderosa.TerminalSession.Properties.Resources.Cygwin16x16);
            settings.BeginUpdate();
            settings.Encoding = EncodingType.UTF8;
            settings.EndUpdate();
            return settings;
        }

        public IMacroEngine MacroEngine {
            get {
                if (_macroEngine == null) {
                    _macroEngine = _poderosaWorld.PluginManager.FindPlugin("org.poderosa.macro", typeof(IMacroEngine)) as IMacroEngine;
                }
                return _macroEngine;
            }
        }
    }
}
