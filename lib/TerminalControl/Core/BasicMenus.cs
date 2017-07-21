/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: BasicMenus.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

using Poderosa.Forms;
using Poderosa.Sessions;
using Poderosa.Document;
using Poderosa.UI;
using Poderosa.View;

namespace Poderosa.Commands {
    // コマンドに関連付けられたメニュー
    internal class BasicMenuItem : PoderosaMenuItemImpl {
        public BasicMenuItem(string textID, string commandID)
            : base(BindCommand(commandID), CoreUtil.Strings, textID) {
        }
        private static IGeneralCommand BindCommand(string commandID) {
            IGeneralCommand cmd = CommandManagerPlugin.Instance.Find(commandID);
            Debug.Assert(cmd != null, commandID + " not found");
            return cmd;
        }
    }

    internal class NewWindowMenuGroup : PoderosaMenuGroupImpl {
        public NewWindowMenuGroup()
            : base(CreateChildMenus()) {
            _positionType = PositionType.First;
        }
        private static IPoderosaMenu[] CreateChildMenus() {
            return new IPoderosaMenu[] { new BasicMenuItem("Menu.NewWindow", "org.poderosa.core.application.newwindow") };
        }
    }
    internal class QuitMenuGroup : PoderosaMenuGroupImpl {
        public QuitMenuGroup()
            : base(CreateChildMenus()) {
            _positionType = PositionType.Last;
        }
        private static IPoderosaMenu[] CreateChildMenus() {
            return new IPoderosaMenu[] { new BasicMenuItem("Menu.Quit", "org.poderosa.core.application.quit") };
        }
    }

    //コピーとペーストはあえて別グループにする。「みたままコピー」などの変種が挿入されることを考慮
    internal class CopyGroup : PoderosaMenuGroupImpl {
        public CopyGroup()
            : base(CreateChildMenus()) {
            _positionType = PositionType.First;
        }
        private static IPoderosaMenu[] CreateChildMenus() {
            return new IPoderosaMenu[] { new BasicMenuItem("Menu.Copy", "org.poderosa.core.edit.copy") };
        }
    }
    internal class PasteGroup : PoderosaMenuGroupImpl {
        public PasteGroup()
            : base(CreateChildMenus()) {
            _positionType = PositionType.First;
            _showSeparator = false;
        }
        private static IPoderosaMenu[] CreateChildMenus() {
            return new IPoderosaMenu[] { new BasicMenuItem("Menu.Paste", "org.poderosa.core.edit.paste") };
        }
    }

    internal class CloseDocumentGroup : PoderosaMenuGroupImpl {
        public CloseDocumentGroup()
            : base(CreateChildMenus()) {
            _positionType = PositionType.Last;
        }
        private static IPoderosaMenu[] CreateChildMenus() {
            return new IPoderosaMenu[] { new BasicMenuItem("Menu.ConsoleClose", "org.poderosa.core.session.closedocument") }; //今や必ずしもConsoleではない
        }
    }

    internal class SplitMenuGroup : PoderosaMenuGroupImpl {
        internal delegate bool ItemEnableChecker(ISplittableViewManager vm, IPoderosaView view);

        public SplitMenuGroup()
            : base(CreateChildMenus()) {
            _positionType = PositionType.First;
        }

        private static IPoderosaMenu[] CreateChildMenus() {
            return new IPoderosaMenu[] {
                new BasicMenuItem("Menu.DivideFrameHorizontal", "org.poderosa.core.window.splithorizontal"),
                new BasicMenuItem("Menu.DivideFrameVertical",   "org.poderosa.core.window.splitvertical"),
                new BasicMenuItem("Menu.UnifyFrame",            "org.poderosa.core.window.splitunify"),
                new BasicMenuItem("Menu.UnifyAllFrame",         "org.poderosa.core.window.unifyall")
            };
        }
    }

    internal class CloseAllMenuGroup : PoderosaMenuGroupImpl {
        public CloseAllMenuGroup(SplitMenuGroup reference)
            : base(CreateChildMenus()) {
            _positionType = PositionType.NextTo;
            _designationTarget = reference;
        }
        private static IPoderosaMenu[] CreateChildMenus() {
            return new IPoderosaMenu[] {
                new BasicMenuItem("Menu.CloseAll", "org.poderosa.core.window.closeall")
            };
        }
    }

    internal class SetTabRowCountMenuGroup : PoderosaMenuGroupImpl {
        public SetTabRowCountMenuGroup(SplitMenuGroup reference)
            : base(CreateChildMenus()) {
            _positionType = PositionType.NextTo;
            _designationTarget = reference;
            _isVolatile = true;
        }
        private static IPoderosaMenu[] CreateChildMenus() {
            return new IPoderosaMenu[] {
                new SetTabRowCountMenu()
            };
        }
    }

    internal class DefaultPluginMenuGroup : PoderosaMenuGroupImpl {
        public DefaultPluginMenuGroup()
            : base(CreateChildMenus()) {
            _positionType = PositionType.Last;
        }
        private static IPoderosaMenu[] CreateChildMenus() {
            return new IPoderosaMenu[] {
             new BasicMenuItem("Menu.PluginList", "org.poderosa.core.dialog.pluginlist") ,
             new BasicMenuItem("Menu.ExtensionPointList", "org.poderosa.core.dialog.extensionpointlist") };
        }
    }

    internal class DefaultHelpMenuGroup : PoderosaMenuGroupImpl {
        public DefaultHelpMenuGroup()
            : base(CreateChildMenus()) {
            _positionType = PositionType.Last;
        }
        private static IPoderosaMenu[] CreateChildMenus() {
            return new IPoderosaMenu[] {
                 new BasicMenuItem("Menu.AboutBox", "org.poderosa.core.dialog.aboutbox") ,
                 new BasicMenuItem("Menu.PoderosaWeb", "org.poderosa.core.application.openweb")
            };
        }
    }

    //ちょっと特殊な奴
    internal class SetTabRowCountMenu : IPoderosaMenuFolder {
        public IPoderosaMenuGroup[] ChildGroups {
            get {
                IPoderosaMainWindow w = WindowManagerPlugin.Instance.ActiveWindow; //TODO DocActivationと同じ問題抱えてる
                return new IPoderosaMenuGroup[] { new PoderosaMenuGroupImpl(
                    new IPoderosaMenu[] {
                        new SetTabRowMenuItem(w, 1), new SetTabRowMenuItem(w, 2), new SetTabRowMenuItem(w, 3)
                    })};
            }
        }

        public string Text {
            get {
                return CoreUtil.Strings.GetString("Menu.SetTabRowCount");
            }
        }

        public bool IsEnabled(ICommandTarget target) {
            return true;
        }

        public bool IsChecked(ICommandTarget target) {
            return false;
        }

        public IAdaptable GetAdapter(Type adapter) {
            return WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        private class SetTabRowMenuItem : IPoderosaMenuItem, IPoderosaCommand {
            private IPoderosaMainWindow _window;
            private int _count;

            public SetTabRowMenuItem(IPoderosaMainWindow w, int count) {
                _window = w;
                _count = count;
            }
            public IPoderosaCommand AssociatedCommand {
                get {
                    return this;
                }
            }

            public string Text {
                get {
                    return String.Format("&{0}", _count);
                }
            }

            public bool IsEnabled(ICommandTarget target) {
                return true;
            }

            public bool IsChecked(ICommandTarget target) {
                return _window != null && _count == _window.DocumentTabFeature.TabRowCount;
            }

            public IAdaptable GetAdapter(Type adapter) {
                return WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }

            public CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
                _window.DocumentTabFeature.SetTabRowCount(_count);
                return CommandResult.Succeeded;
            }

            public bool CanExecute(ICommandTarget target) {
                return true;
            }
        }
    }
}
