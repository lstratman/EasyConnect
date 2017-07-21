/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: MainWindowMenuT.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
#if UNITTEST
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

using Poderosa;
using Poderosa.Preferences;
using Poderosa.Commands;
using Poderosa.Forms;
using Poderosa.UI;
using Poderosa.Plugins;
using Poderosa.Boot;

namespace Poderosa.Forms {
    [TestFixture]
    public class MainWindowMenuTests {

        private static IPoderosaApplication _poderosaApplication;
        private static IPoderosaWorld _poderosaWorld;

        private static string CreatePluginManifest() {
            return String.Format("Root {{\r\n  {0} {{\r\n  plugin=Poderosa.Forms.MenuTestPlugin\r\n  plugin=Poderosa.Preferences.PreferencePlugin\r\n  plugin=Poderosa.Commands.CommandManagerPlugin\r\n  plugin=Poderosa.Forms.WindowManagerPlugin\r\n}}\r\n}}", "Poderosa.Monolithic.dll");
        }

        [TestFixtureSetUp]
        public void Init() {
            try {
                _poderosaApplication = PoderosaStartup.CreatePoderosaApplication(CreatePluginManifest(), new StructuredText("Poderosa"));
                _poderosaWorld = _poderosaApplication.Start();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        [TestFixtureTearDown]
        public void Terminate() {
            _poderosaApplication.Shutdown();
        }

        [Test]
        public void Test0_Menu1() {
            string caption = "Init1";
            MenuTestPlugin._instance.SetContent(caption, 1);
            IWindowManager wm = (IWindowManager)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.core.window", typeof(IWindowManager));
            //TODO 明示的にリロード強要はいやらしい
            wm.ReloadMenu();

            GMainMenuItem fm = (GMainMenuItem)wm.MainWindows[0].AsForm().Menu.MenuItems[0];
            Assert.AreEqual(3, fm.MenuItems.Count); //デリミタが入るので
            Assert.AreEqual(caption, fm.MenuItems[0].Text);
            Assert.AreEqual(caption, fm.MenuItems[2].Text);
        }

        [Test]
        public void Test1_Enabled_checked() {
            string caption = "Init2";
            MenuTestPlugin._instance.InitEnabledChecked(caption, false, true);
            IWindowManager wm = (IWindowManager)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.core.window", typeof(IWindowManager));
            wm.ReloadMenu();

            GMainMenuItem fm = (GMainMenuItem)wm.MainWindows[0].AsForm().Menu.MenuItems[0];
            Assert.AreEqual(caption, fm.MenuItems[0].Text);
            Assert.IsTrue(fm.MenuItems[0].Checked);
            Assert.IsFalse(fm.MenuItems[0].Enabled);
        }

        [Test]
        public void Test2_Popup() {
            string caption = "Init2";
            MenuTestPlugin._instance.InitEnabledChecked(caption, false, true);
            IWindowManager wm = (IWindowManager)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.core.window", typeof(IWindowManager));
            wm.ReloadMenu();

            GMainMenuItem fm = (GMainMenuItem)wm.MainWindows[0].AsForm().Menu.MenuItems[0];
            Assert.IsTrue(fm.MenuItems[0].Checked);
            Assert.IsFalse(fm.MenuItems[0].Enabled);

            MenuTestPlugin._instance.ResetEnabledChecked(true, false); //逆向き
            fm.PerformPopup();
            Assert.IsFalse(fm.MenuItems[0].Checked);
            Assert.IsTrue(fm.MenuItems[0].Enabled);
        }

        [Test]
        public void Test3_DynamicContent() {
            string caption = "Init3";
            MenuTestPlugin._instance.SetContent(caption, 1);
            IWindowManager wm = (IWindowManager)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.core.window", typeof(IWindowManager));
            wm.ReloadMenu();

            GMainMenuItem fm = (GMainMenuItem)wm.MainWindows[0].AsForm().Menu.MenuItems[0];
            Assert.AreEqual(3, fm.MenuItems.Count);

            string caption2 = "Changed";
            MenuTestPlugin._instance.SetContent(caption2, 3);
            fm.PerformPopup();
            //VolatileなGroup2にのみ変更が反映されていることを確認
            Assert.AreEqual(5, fm.MenuItems.Count);
            Assert.AreEqual(caption, fm.MenuItems[0].Text);
            Assert.AreEqual(caption2, fm.MenuItems[4].Text);
        }
    }

    [PluginInfo(ID = "org.poderosa.test.menutest", Dependencies = "org.poderosa.core.window")]
    internal class MenuTestPlugin : PluginBase {
        public static MenuTestPlugin _instance;

        private MenuGroup1 _group1;
        private MenuGroup2 _group2;

        private class Item : IPoderosaMenuItem {
            private string _text;
            private bool _enabled;
            private bool _checked;

            public Item(string text) {
                _text = text;
                _enabled = true;
                _checked = false;
            }

            public IPoderosaCommand AssociatedCommand {
                get {
                    return null;
                }
            }

            public string Text {
                get {
                    return _text;
                }
            }

            public bool IsEnabled(ICommandTarget target) {
                return _enabled;
            }

            public bool IsChecked(ICommandTarget target) {
                return _checked;
            }

            public void SetEnabled(bool value) {
                _enabled = value;
            }
            public void SetChecked(bool value) {
                _checked = value;
            }

            public IAdaptable GetAdapter(Type adapter) {
                return adapter.IsInstanceOfType(this) ? this : null;
            }
        }

        private class MenuGroup1 : IPoderosaMenuGroup {
            private List<Item> _items;
            public MenuGroup1() {
                _items = new List<Item>();
            }

            public IPoderosaMenu[] ChildMenus {
                get {
                    return _items.ToArray();
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

            public void Init(string caption) {
                _items.Clear();
                _items.Add(new Item(caption));
            }
            public void SetStatus(bool enabled, bool checked_) {
                Item i = _items[0];
                i.SetEnabled(enabled);
                i.SetChecked(checked_);
            }
            public IAdaptable GetAdapter(Type adapter) {
                return adapter.IsInstanceOfType(this) ? this : null;
            }
        }

        private class MenuGroup2 : IPoderosaMenuGroup {
            private List<Item> _items;
            public MenuGroup2() {
                _items = new List<Item>();
            }

            public IPoderosaMenu[] ChildMenus {
                get {
                    return _items.ToArray();
                }
            }

            public bool IsVolatileContent {
                get {
                    return true;
                }
            }
            public bool ShowSeparator {
                get {
                    return true;
                }
            }

            public void SetItems(string caption, int count) {
                _items.Clear();
                for (int i = 0; i < count; i++)
                    _items.Add(new Item(caption));
            }

            public void Clear() {
                _items.Clear();
            }
            public IAdaptable GetAdapter(Type adapter) {
                return adapter.IsInstanceOfType(this) ? this : null;
            }
        }

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _group1 = new MenuGroup1();
            _group2 = new MenuGroup2();
            IExtensionPoint ep = poderosa.PluginManager.FindExtensionPoint("org.poderosa.menu.file");
            ep.RegisterExtension(_group1);
            ep.RegisterExtension(_group2);
            _instance = this;
        }

        public void SetContent(string caption, int group2_item_count) {
            _group1.Init(caption);
            _group2.SetItems(caption, group2_item_count);
        }

        public void InitEnabledChecked(string caption, bool enabled, bool checked_) {
            _group1.Init(caption);
            _group1.SetStatus(enabled, checked_);
            _group2.Clear();
        }
        public void ResetEnabledChecked(bool enabled, bool checked_) {
            _group1.SetStatus(enabled, checked_);
        }

    }
}
#endif
