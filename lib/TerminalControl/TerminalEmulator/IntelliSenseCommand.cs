/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: IntelliSenseCommand.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Commands;

namespace Poderosa.Terminal {
    internal class ToggleIntelliSenseCommand : GeneralCommandImpl {
        public ToggleIntelliSenseCommand()
            : base("org.poderosa.terminalemulator.toggleCharTriggerIntelliSense", TerminalCommand.StringResource, "Command.ToggleCharTriggerIntelliSense", TerminalCommand.TerminalCommandCategory, new ExecuteDelegate(ToggleIntelliSense)) {
            _defaultShortcutKey = Keys.Alt | Keys.I;
            _canExecuteDelegate = TerminalCommand.DoesOpenTargetSession;
        }

        private static CommandResult ToggleIntelliSense(ICommandTarget target) {
            ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
            if (ts == null)
                return CommandResult.Failed;
            ITerminalSettings settings = ts.TerminalSettings;
            settings.BeginUpdate();
            settings.EnabledCharTriggerIntelliSense = !settings.EnabledCharTriggerIntelliSense;
            settings.EndUpdate();
            return CommandResult.Succeeded;
        }
    }

    internal class SetShellSchemeCommand : IPoderosaCommand {
        private static SetShellSchemeCommand _instance;

        public static SetShellSchemeCommand Instance {
            get {
                if (_instance == null)
                    _instance = new SetShellSchemeCommand();
                return _instance;
            }
        }

        public CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
            IShellScheme ss = (IShellScheme)args[0].GetAdapter(typeof(IShellScheme));
            Debug.Assert(ss != null);

            ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
            if (ts == null)
                return CommandResult.Failed;
            ITerminalSettings settings = ts.TerminalSettings;
            settings.BeginUpdate();
            settings.ShellScheme = ss;
            settings.EndUpdate();
            return CommandResult.Succeeded;
        }

        public bool CanExecute(ICommandTarget target) {
            return TerminalCommand.DoesOpenTargetSession(target);
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

    internal class IntelliSenseMenuGroup : PoderosaMenuGroupImpl {
        public IntelliSenseMenuGroup()
            : base(new IPoderosaMenu[] { new ToggleIntelliSenseMenuItem(), new SelectIntelliSenseSchemeMenuFolder() }) {
            _positionType = PositionType.Last;
        }
    }


    internal class ToggleIntelliSenseMenuItem : PoderosaMenuItemImpl {
        public ToggleIntelliSenseMenuItem()
            : base("org.poderosa.terminalemulator.toggleCharTriggerIntelliSense", TerminalCommand.StringResource, "Menu.ToggleCharTriggerIntelliSense") {
        }
        public override bool IsChecked(ICommandTarget target) {
            ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
            if (ts == null)
                return false;
            return ts.TerminalSettings.EnabledCharTriggerIntelliSense;
        }
    }

    internal class SelectIntelliSenseSchemeMenuFolder : IPoderosaMenuFolder {
        private SchemeMenuGroup _menuGroup;

        public SelectIntelliSenseSchemeMenuFolder() {
            _menuGroup = new SchemeMenuGroup();
        }

        public IPoderosaMenuGroup[] ChildGroups {
            get {
                return new IPoderosaMenuGroup[] { _menuGroup };
            }
        }

        public string Text {
            get {
                return TerminalCommand.StringResource.GetString("Menu.SelectIntelliSenseScheme");
            }
        }

        public bool IsEnabled(ICommandTarget target) {
            ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
            return ts != null;
        }

        public bool IsChecked(ICommandTarget target) {
            return false;
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        //子要素
        private class SchemeMenuGroup : PoderosaMenuGroupImpl {
            public override bool IsVolatileContent {
                get {
                    return true;
                }
            }
            public override IPoderosaMenu[] ChildMenus {
                get {
                    //TODO スキームコレクションから変更通知を受けた直後だけ再作成すれば効率はよい
                    Debug.WriteLineIf(DebugOpt.IntelliSenseMenu, "Building intellisense menu");
                    TerminalEmulatorPlugin.Instance.LaterInitialize();
                    IShellSchemeCollection sc = TerminalEmulatorPlugin.Instance.ShellSchemeCollection;
                    List<SchemeMenuItem> r = new List<SchemeMenuItem>();
                    foreach (IShellScheme ss in sc.Items) {
                        r.Add(new SchemeMenuItem(ss));
                    }
                    return r.ToArray();
                }
            }
        }

        private class SchemeMenuItem : IPoderosaMenuItemWithArgs {
            private IShellScheme _scheme;
            public SchemeMenuItem(IShellScheme scheme) {
                _scheme = scheme;
            }
            public IAdaptable[] AdditionalArgs {
                get {
                    return new IAdaptable[] { _scheme };
                }
            }

            public IPoderosaCommand AssociatedCommand {
                get {
                    return SetShellSchemeCommand.Instance;
                }
            }

            public string Text {
                get {
                    return _scheme.Name;
                }
            }

            public bool IsEnabled(ICommandTarget target) {
                return true;
            }

            public bool IsChecked(ICommandTarget target) {
                ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
                if (ts == null)
                    return false;
                ITerminalSettings settings = ts.TerminalSettings;
                return settings.ShellScheme.Name == _scheme.Name;
            }

            public IAdaptable GetAdapter(Type adapter) {
                return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }
        }
    }
}