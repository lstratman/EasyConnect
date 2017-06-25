/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalToolBar.cs,v 1.6 2012/03/18 12:51:15 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;

using Poderosa.Commands;
using Poderosa.Forms;
using Poderosa.ConnectionParam;
using Poderosa.Util;
using Poderosa.Sessions;
using Poderosa.Util.Collections;
using Poderosa.UI;

namespace Poderosa.Terminal {
    internal class TerminalToolBar : IToolBarComponent, IPositionDesignation, IActiveDocumentChangeListener {

        private TypedHashtable<IPoderosaMainWindow, TerminalToolBarInstance> _toolbarInstances;

        public TerminalToolBar() {
            _toolbarInstances = new TypedHashtable<IPoderosaMainWindow, TerminalToolBarInstance>();
        }

        public IAdaptable DesignationTarget {
            get {
                return null;
            }
        }

        public PositionType DesignationPosition {
            get {
                return PositionType.DontCare;
            }
        }

        public bool ShowSeparator {
            get {
                return true;
            }
        }

        public IToolBarElement[] ToolBarElements {
            get {
                return new IToolBarElement[] {
                    new ToolBarLabelImpl(GEnv.Strings, "Form.ToolBar._newLineLabel", 60),
                    new NewLineChangeHandler(),
                    new ToolBarLabelImpl(GEnv.Strings, "Form.ToolBar._encodingLabel", 88), //TODO サイズ指定はいやらしいな
                    new EncodingChangeHandler(),
                    new LocalEchoHandler(),
                    new IntelliSenseHandler(),
                    new ShellSchemeChangeHandler()
                };
            }
        }

        #region IAdaptable
        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion

        //それぞれの要素
        private class NewLineChangeHandler : ToolBarComboBoxImpl {
            public override object[] Items {
                get {
                    // recreate list for updating texts according to the language setting.
                    _items = EnumListItem<NewLine>.GetListItems();
                    return _items;
                }
            }
            public override int Width {
                get {
                    return 72;
                }
            }

            public override bool IsEnabled(ICommandTarget target) {
                return TerminalCommandTarget.AsOpenTerminal(target) != null;
            }

            public override int GetSelectedIndex(ICommandTarget target) {
                ITerminalControlHost session = TerminalCommandTarget.AsTerminal(target);
                NewLine val = session.TerminalSettings.TransmitNL; //TODO intへのキャストは乱暴だな

                EnumListItem<NewLine>[] items = (EnumListItem<NewLine>[])_items;
                if (items != null) {
                    for (int i = 0; i < items.Length; i++) {
                        if (items[i].Value == val) {
                            return i;
                        }
                    }
                }

                return -1;
            }

            public override void OnChange(ICommandTarget target, int selectedIndex, object selectedItem) {
                EnumListItem<NewLine> item = selectedItem as EnumListItem<NewLine>;
                if (item != null) {
                    ITerminalControlHost session = TerminalCommandTarget.AsTerminal(target);
                    ITerminalSettings ts = session.TerminalSettings;
                    ts.BeginUpdate();
                    ts.TransmitNL = item.Value;
                    ts.EndUpdate();
                }
            }
        }
        private class EncodingChangeHandler : ToolBarComboBoxImpl {
            public override object[] Items {
                get {
                    // recreate list for updating texts according to the language setting.
                    _items = EnumListItem<EncodingType>.GetListItems();
                    return _items;
                }
            }
            public override int Width {
                get {
                    return 88;
                }
            }
            public override bool IsEnabled(ICommandTarget target) {
                return TerminalCommandTarget.AsOpenTerminal(target) != null;
            }

            public override int GetSelectedIndex(ICommandTarget target) {
                ITerminalControlHost session = TerminalCommandTarget.AsTerminal(target);
                EncodingType val = session.TerminalSettings.Encoding;

                EnumListItem<EncodingType>[] items = (EnumListItem<EncodingType>[])_items;
                if (items != null) {
                    for (int i = 0; i < items.Length; i++) {
                        if (items[i].Value == val) {
                            return i;
                        }
                    }
                }

                return -1;
            }

            public override void OnChange(ICommandTarget target, int selectedIndex, object selectedItem) {
                EnumListItem<EncodingType> item = selectedItem as EnumListItem<EncodingType>;
                if (item != null) {
                    ITerminalControlHost session = TerminalCommandTarget.AsTerminal(target);
                    ITerminalSettings ts = session.TerminalSettings;
                    ts.BeginUpdate();
                    ts.Encoding = item.Value;
                    ts.EndUpdate();
                }
            }
        }
        private class LocalEchoHandler : ToolBarToggleButtonImpl {
            public override Image Icon {
                get {
                    return Poderosa.TerminalEmulator.Properties.Resources.LocalEcho16x16;
                }
            }
            public override string ToolTipText {
                get {
                    return GEnv.Strings.GetString("Command.ToggleLocalEcho");
                }
            }
            public override bool IsEnabled(ICommandTarget target) {
                return TerminalCommandTarget.AsOpenTerminal(target) != null;
            }

            public override bool IsChecked(ICommandTarget target) {
                ITerminalControlHost session = TerminalCommandTarget.AsTerminal(target);
                return session.TerminalSettings.LocalEcho;
            }

            public override void OnChange(ICommandTarget target, bool is_checked) {
                ITerminalControlHost session = TerminalCommandTarget.AsTerminal(target);
                ITerminalSettings ts = session.TerminalSettings;
                ts.BeginUpdate();
                ts.LocalEcho = is_checked;
                ts.EndUpdate();
            }
        }
        private class IntelliSenseHandler : ToolBarToggleButtonImpl {
            public override Image Icon {
                get {
                    return Poderosa.TerminalEmulator.Properties.Resources.Intellisense16x16;
                }
            }
            public override string ToolTipText {
                get {
                    return GEnv.Strings.GetString("Command.ToggleCharTriggerIntelliSense");
                }
            }
            public override bool IsEnabled(ICommandTarget target) {
                return TerminalCommandTarget.AsOpenTerminal(target) != null;
            }

            public override bool IsChecked(ICommandTarget target) {
                ITerminalControlHost session = TerminalCommandTarget.AsOpenTerminal(target);
                return session.TerminalSettings.EnabledCharTriggerIntelliSense;
            }

            public override void OnChange(ICommandTarget target, bool is_checked) {
                ITerminalControlHost session = TerminalCommandTarget.AsOpenTerminal(target);
                ITerminalSettings ts = session.TerminalSettings;
                ts.BeginUpdate();
                ts.EnabledCharTriggerIntelliSense = is_checked;
                ts.EndUpdate();
            }
        }

        private class ShellSchemeChangeHandler : ToolBarComboBoxImpl {
            public override object[] Items {
                get {
                    List<ListItem<IShellScheme>> list = new List<ListItem<IShellScheme>>();
                    TerminalEmulatorPlugin.Instance.LaterInitialize();
                    foreach (IShellScheme ss in TerminalEmulatorPlugin.Instance.ShellSchemeCollection.Items) {
                        list.Add(new ListItem<IShellScheme>(ss, ss.Name));
                    }
                    return list.ToArray();
                }
            }
            public override string ToolTipText {
                get {
                    return GEnv.Strings.GetString("Caption.CurrentShellScheme");
                }
            }
            public override int Width {
                get {
                    return 104;
                }
            }
            public override bool IsEnabled(ICommandTarget target) {
                ITerminalControlHost session = TerminalCommandTarget.AsOpenTerminal(target);
                return session != null;
            }

            public override int GetSelectedIndex(ICommandTarget target) {
                ITerminalControlHost session = TerminalCommandTarget.AsOpenTerminal(target);
                return TerminalEmulatorPlugin.Instance.ShellSchemeCollection.IndexOf(session.TerminalSettings.ShellScheme);
            }

            public override void OnChange(ICommandTarget target, int selectedIndex, object selectedItem) {
                ListItem<IShellScheme> item = selectedItem as ListItem<IShellScheme>;
                if (item != null) {
                    IShellScheme scheme = item.Value;

                    ITerminalControlHost session = TerminalCommandTarget.AsOpenTerminal(target);
                    ITerminalSettings ts = session.TerminalSettings;
                    ts.BeginUpdate();
                    ts.ShellScheme = scheme;
                    ts.EndUpdate();
                }
            }
        }

        #region IActiveDocumentChangeListener
        public void OnDocumentActivated(IPoderosaMainWindow window, IPoderosaDocument document) {
            //Debug.WriteLine("OnDocumentActivated");
            TerminalToolBarInstance tb = FindToolbarInstance(window);
            if (tb == null) {
                tb = new TerminalToolBarInstance(this, window.ToolBar);
                _toolbarInstances[window] = tb;
                //TODO コレクションの削除がない！
            }
            IAbstractTerminalHost session = (IAbstractTerminalHost)document.OwnerSession.GetAdapter(typeof(IAbstractTerminalHost));
            if (session != null)
                tb.Attach(session.TerminalSettings);

        }
        public void OnDocumentDeactivated(IPoderosaMainWindow window) {
            //Debug.WriteLine("OnDocumentDeactivated");
            TerminalToolBarInstance tb = FindToolbarInstance(window);
            if (tb != null)
                tb.Detach();
        }
        #endregion

        private TerminalToolBarInstance FindToolbarInstance(IPoderosaMainWindow window) {
            return _toolbarInstances[window];
        }
    }

    internal class TerminalToolBarInstance : ITerminalSettingsChangeListener {
        private TerminalToolBar _parent;
        private IToolBar _toolBar;
        private ITerminalSettings _terminalSettings;

        public TerminalToolBarInstance(TerminalToolBar parent, IToolBar toolbar) {
            _parent = parent;
            _toolBar = toolbar;
        }

        public void Attach(ITerminalSettings ts) {
            if (_terminalSettings != null)
                _terminalSettings.RemoveListener(this);
            _terminalSettings = ts;
            _terminalSettings.AddListener(this);
            _toolBar.RefreshComponent(_parent);
        }
        public void Detach() {
            if (_terminalSettings != null)
                _terminalSettings.RemoveListener(this);
            _terminalSettings = null;
            _toolBar.RefreshComponent(_parent);
        }

        #region ITerminalSettingsChangeListener
        public void OnBeginUpdate(ITerminalSettings current) {
        }

        public void OnEndUpdate(ITerminalSettings current) {
            _toolBar.RefreshComponent(_parent);
        }
        #endregion
    }
}
