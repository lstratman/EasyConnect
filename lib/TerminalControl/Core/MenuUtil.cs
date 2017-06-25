/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: MenuUtil.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Commands;
using Poderosa.Util;

namespace Poderosa.Forms {
    internal class MenuUtil {
        public static ToolStripMenuItem CreateMenuItem(MainMenuItem item, ICommandTarget target) {
            item.Create();
            ToolStripMenuItem ui_menu = new ToolStripMenuItem();
            ui_menu.DropDownOpening += new EventHandler(OnRootPopupMenu);
            ui_menu.Text = item.Text;
            ui_menu.Tag = new MenuItemTag(null, item, target);
            ui_menu.Enabled = true;
            //メニュー項目を遅延作成させるためDropDownOpeningイベントを使ったが、このイベントは
            //「子メニュー項目がなく、キーボードでメニューが選択されたとき」には発生しない。結果、キーボードでのメニュー選択に支障が出る。
            //一応ダミー項目をセットするなどで回避できそうだが、面倒なのでここで一回自前でイベントを出して
            OnRootPopupMenu(ui_menu, null);

            return ui_menu;
        }

        //既存メニューの中身を構築：MainMenuItemおよび子のfolder用
        public static void BuildMenuContents(ToolStripMenuItem menuitem, IPoderosaMenuFolder contents) {
            ICommandTarget target = ((MenuItemTag)menuitem.Tag).CommandTarget;
            int count = 0;
            foreach (IPoderosaMenuGroup grp in contents.ChildGroups) {
                if (count > 0 && grp.ShowSeparator)
                    menuitem.DropDownItems.Add(CreateBar()); //直前のグループに要素があるならデリミタを入れる
                count = BuildMenuContentsForGroup(menuitem.DropDownItems.Count, target, menuitem.DropDownItems, grp);
            }
        }

        //コンテキストメニューの構築
        public static void BuildContextMenu(ContextMenuStrip cm, IEnumerable<IPoderosaMenuGroup> groups, ICommandTarget target) {
            cm.Tag = new MenuItemTag(null, null, target);
            int count = 0;
            foreach (IPoderosaMenuGroup grp in groups) {
                if (count > 0 && grp.ShowSeparator)
                    cm.Items.Add(CreateBar()); //直前のグループに要素があるならデリミタを入れる
                count = BuildMenuContentsForGroup(cm.Items.Count, target, cm.Items, grp);
            }

        }

        public static void RefreshMenuContents(ToolStripMenuItem menuitem, IPoderosaMenuFolder contents) {
            //若干手抜きだが、VolatileContentsと混合していると比較が面倒。なので一つでもVolatileならリビルドしてしまう
            bool contains_volatile = false;
            foreach (IPoderosaMenuGroup grp in contents.ChildGroups) {
                if (grp.IsVolatileContent) {
                    contains_volatile = true;
                    break;
                }
            }

            if (contains_volatile) {
                menuitem.DropDownItems.Clear();
                BuildMenuContents(menuitem, contents);
            }
            else {
                foreach (ToolStripItem mi in menuitem.DropDownItems) {
                    MenuItemTag tag = mi.Tag as MenuItemTag;
                    if (tag != null) { //BarではTagなしだ
                        ToolStripMenuItem mi2 = mi as ToolStripMenuItem;
                        Debug.Assert(mi2 != null);
                        mi2.Enabled = tag.Menu.IsEnabled(tag.CommandTarget);
                        mi2.Checked = mi.Enabled ? tag.Menu.IsChecked(tag.CommandTarget) : false;
                    }
                }
            }
        }

        private static int BuildMenuContentsForGroup(int index, ICommandTarget target, ToolStripItemCollection children, IPoderosaMenuGroup grp) {
            int count = 0;
            foreach (IPoderosaMenu m in grp.ChildMenus) {
                ToolStripMenuItem mi = new ToolStripMenuItem();
                children.Insert(index++, mi); //途中挿入のことも
                mi.DropDownOpening += new EventHandler(OnPopupMenu);
                mi.Enabled = m.IsEnabled(target);
                mi.Checked = mi.Enabled ? m.IsChecked(target) : false;
                mi.Text = m.Text; //Enabledを先に
                mi.Tag = new MenuItemTag(grp, m, target);

                IPoderosaMenuFolder folder;
                IPoderosaMenuItem leaf;
                if ((folder = m as IPoderosaMenuFolder) != null) {
                    BuildMenuContents(mi, folder);
                }
                else if ((leaf = m as IPoderosaMenuItem) != null) {
                    mi.Click += new EventHandler(OnClickMenu);
                    IGeneralCommand gc = leaf.AssociatedCommand as IGeneralCommand;
                    if (gc != null)
                        mi.ShortcutKeyDisplayString = WinFormsUtil.FormatShortcut(CommandManagerPlugin.Instance.CurrentKeyBinds.GetKey(gc));
                }

                count++;
            }

            return count;
        }

        private static ToolStripItem CreateBar() {
            ToolStripSeparator m = new ToolStripSeparator();
            return m;
        }

        //EventHandler
        private static void OnPopupMenu(object sender, EventArgs args) {
            try {
                ToolStripMenuItem mi = sender as ToolStripMenuItem;
                if (mi.DropDownItems.Count == 0)
                    return; //子がなくてもイベントは発生してしまう模様
                IPoderosaMenuFolder folder = ((MenuItemTag)mi.Tag).Menu as IPoderosaMenuFolder;
                Debug.Assert(folder != null);
                RefreshMenuContents(mi, folder);
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }
        //メインメニュー用 遅延作成
        private static void OnRootPopupMenu(object sender, EventArgs args) {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            MenuItemTag tag = (MenuItemTag)mi.Tag;
            if (!tag.Created) {
                BuildMenuContents(mi, (IPoderosaMenuFolder)tag.Menu);
                tag.Created = true;
                return;
            }
            OnPopupMenu(sender, args); //通常のポップアップ動作
        }
        private static void OnClickMenu(object sender, EventArgs args) {
            try {
                ToolStripMenuItem mi = sender as ToolStripMenuItem;
                MenuItemTag tag = ((MenuItemTag)mi.Tag);
                IPoderosaMenuItem item = tag.Menu as IPoderosaMenuItem;
                Debug.Assert(item != null);

                if (item is IPoderosaMenuItemWithArgs)
                    CommandManagerPlugin.Instance.Execute(item.AssociatedCommand, tag.CommandTarget, ((IPoderosaMenuItemWithArgs)item).AdditionalArgs);
                else
                    CommandManagerPlugin.Instance.Execute(item.AssociatedCommand, tag.CommandTarget);
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }

        private static IPoderosaMenuGroup SafeMenuItemToMenuGroup(ToolStripMenuItem item) {
            object t = item.Tag;
            if (t == null)
                return null;

            MenuItemTag mitag = t as MenuItemTag;
            return mitag == null ? null : mitag.MenuGroup;
        }
    }

    internal class MenuItemTag {
        private IPoderosaMenuGroup _group;
        private IPoderosaMenu _menu;
        private ICommandTarget _commandTarget;
        private bool _createdFlag;

        public MenuItemTag(IPoderosaMenuGroup grp, IPoderosaMenu menu, ICommandTarget target) {
            _group = grp;
            _menu = menu;
            _commandTarget = target;
        }
        public IPoderosaMenuGroup MenuGroup {
            get {
                return _group;
            }
        }
        public IPoderosaMenu Menu {
            get {
                return _menu;
            }
        }
        public ICommandTarget CommandTarget {
            get {
                return _commandTarget;
            }
        }

        public bool Created {
            get {
                return _createdFlag;
            }
            set {
                _createdFlag = value;
            }
        }
    }
}
