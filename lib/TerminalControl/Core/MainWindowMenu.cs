/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: MainWindowMenu.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

using Poderosa.Util;
using Poderosa.Util.Collections;
using Poderosa.Plugins;
//using Poderosa.UI;
using Poderosa.Commands;

namespace Poderosa.Forms {
    //メインメニュー
    //  マルチウィンドウが可能であるので、MainMenuItemのツリーはアプリケーションで唯一(WindowManagerが保有)し、
    //  Windows.Formsベースのメニュー項目は必要に応じてその都度作成されることに注意する
    internal class MainWindowMenu {
        private TypedSequentialTable<string, MainMenuItem> _idToMenu;
        private MainMenuItem _fileMenu;
        private MainMenuItem _editMenu;
        private MainMenuItem _consoleMenu;
        private MainMenuItem _toolMenu;
        private MainMenuItem _windowMenu;
        private MainMenuItem _pluginMenu;
        private MainMenuItem _helpMenu;

        //他から参照されるメニューグループ
        private SplitMenuGroup _splitMenuGroup;

        public MainWindowMenu() {
            _idToMenu = new TypedSequentialTable<string, MainMenuItem>();

            _fileMenu = CreateMainMenu("Menu.File", "org.poderosa.menu.file");
            _editMenu = CreateMainMenu("Menu.Edit", "org.poderosa.menu.edit");
            _consoleMenu = CreateMainMenu("Menu.Console", "org.poderosa.menu.console");
            _toolMenu = CreateMainMenu("Menu.Tool", "org.poderosa.menu.tool");
            _windowMenu = CreateMainMenu("Menu.Window", "org.poderosa.menu.window");
            _pluginMenu = CreateMainMenu("Menu.Plugin", "org.poderosa.menu.plugin");
            _helpMenu = CreateMainMenu("Menu.Help", "org.poderosa.menu.help");

            _splitMenuGroup = new SplitMenuGroup();
#if !UNITTEST
            _windowMenu.ExtensionPoint.RegisterExtension(_splitMenuGroup);
            _windowMenu.ExtensionPoint.RegisterExtension(new CloseAllMenuGroup(_splitMenuGroup));
            _windowMenu.ExtensionPoint.RegisterExtension(new SetTabRowCountMenuGroup(_splitMenuGroup));
            _windowMenu.ExtensionPoint.RegisterExtension(new DocActivationMenuGroup());
            _fileMenu.ExtensionPoint.RegisterExtension(new NewWindowMenuGroup());
            _fileMenu.ExtensionPoint.RegisterExtension(new QuitMenuGroup());
            _editMenu.ExtensionPoint.RegisterExtension(new CopyGroup());
            _editMenu.ExtensionPoint.RegisterExtension(new PasteGroup());
            _consoleMenu.ExtensionPoint.RegisterExtension(new CloseDocumentGroup());
            _pluginMenu.ExtensionPoint.RegisterExtension(new DefaultPluginMenuGroup());
            _helpMenu.ExtensionPoint.RegisterExtension(new DefaultHelpMenuGroup());
#endif
        }
        private MainMenuItem CreateMainMenu(string text_id, string extension_point_name) {
            StringResource str = CoreUtil.Strings;
            MainMenuItem item = new MainMenuItem(text_id, extension_point_name, _idToMenu.Count);
            _idToMenu.Add(extension_point_name, item);
            return item;
        }

        public void FullBuild(MenuStrip mainmenu, ICommandTarget target) {
            foreach (Pair<string, MainMenuItem> p in _idToMenu.Pairs) {
                mainmenu.Items.Add(MenuUtil.CreateMenuItem(p.Second, target));
            }
        }

        public SplitMenuGroup SplitMenuGroup {
            get {
                return _splitMenuGroup;
            }
        }

        public MainMenuItem FindMainMenuItem(string extension_point_name) {
            return _idToMenu[extension_point_name];
        }

    }

    internal class MainMenuItem : IPoderosaMenuFolder {
        private List<IPoderosaMenuGroup> _children;
        private bool _created;
        private int _index;
        private string _text;
        private IExtensionPoint _extensionPoint;

        public MainMenuItem(string text, string extension_point_name, int index) {
            _children = new List<IPoderosaMenuGroup>();
            _index = index;
            _text = text;
            _extensionPoint = WindowManagerPlugin.Instance.PoderosaWorld.PluginManager.CreateExtensionPoint(extension_point_name, typeof(IPoderosaMenuGroup), WindowManagerPlugin.Instance);
            _created = false;
        }

        public void Create() {
            if (_created)
                return;

            _children.Clear();
            IPoderosaMenuGroup[] me = (IPoderosaMenuGroup[])_extensionPoint.GetExtensions();

            //ソートしてコレクションに追加
            foreach (IPoderosaMenuGroup g in PositionDesignationSorter.SortItems(me)) {
                _children.Add(g);
            }

            _created = true;
        }

        public IPoderosaMenuGroup[] ChildGroups {
            get {
                return _children.ToArray();
            }
        }

        public string Text {
            get {
                return CoreUtil.Strings.GetString(_text);
            }
        }
        public int Index {
            get {
                return _index;
            }
        }
        public IExtensionPoint ExtensionPoint {
            get {
                return _extensionPoint;
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
    }

    internal class DefaultMenuItemGroup : IPoderosaMenuGroup {
        private List<IPoderosaMenu> _children;

        public DefaultMenuItemGroup() {
            _children = new List<IPoderosaMenu>();
        }


        public IPoderosaMenu[] ChildMenus {
            get {
                return _children.ToArray();
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
            return WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }


}
