/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PluginT.cs,v 1.2 2011/10/27 23:21:56 kzmi Exp $
 */
#if UNITTEST
using System;
using System.Diagnostics;
using System.IO;

using NUnit.Framework;

using Poderosa.Preferences;
using Poderosa.Boot;

namespace Poderosa.Plugins {
    [TestFixture]
    public class PluginTests {
        private IPoderosaApplication _poderosaApplication;
        private IPoderosaWorld _poderosaWorld;

        private PluginManager Init(string pluginManifest) {
            _poderosaApplication = PoderosaStartup.CreatePoderosaApplication(pluginManifest, new StructuredText("Poderosa"));
            _poderosaWorld = _poderosaApplication.Start();
            return (PluginManager)_poderosaWorld.PluginManager;
        }

        private string CreateManifest(string classname) {
            return CreateManifest("Poderosa.Monolithic.dll", classname);
        }
        private string CreateManifest(string asmname, string classname) {
            return String.Format("Root {{\r\n  {0} {{\r\n  plugin=Poderosa.Plugins.{1}\r\n}}\r\n}}", asmname, classname);
        }


        private string CreateManifest2(string classname1, string classname2) {
            return String.Format("Root {{\r\n  Poderosa.Monolithic.dll {{\r\n  plugin=Poderosa.Plugins.{0}\r\n  plugin=Poderosa.Plugins.{1}\r\n}}\r\n}}", classname1, classname2);
        }

        private StringResource GetStringResource() {
            return InternalPoderosaWorld.Strings;
        }

        //期待通りのエラーメッセージが出ていることを確認
        private void CheckErrorMessage(PluginManager pm, string string_id) {
            CheckOneErrorMessage(pm, GetStringResource().GetString(string_id));
        }
        private void CheckErrorMessage(PluginManager pm, string string_id, string param1) {
            CheckOneErrorMessage(pm, String.Format(GetStringResource().GetString(string_id), param1));
        }
        private void CheckErrorMessage(PluginManager pm, string string_id, string param1, string param2) {
            CheckOneErrorMessage(pm, String.Format(GetStringResource().GetString(string_id), param1, param2));
        }
        private void CheckOneErrorMessage(PluginManager pm, string msg) {
            string actual = pm.Tracer.Document.GetDataAt(0);
            if (actual != msg) {
                //しばしば長くなる。Debugに出さないとわかりづらい
                Debug.WriteLine("actual=" + actual);
            }
            Assert.AreEqual(msg, actual);
        }

        private void VerifyPluginOrder(IPlugin[] actual, Type[] expected) {
            Assert.AreEqual(actual.Length, expected.Length);
            for (int i = 0; i < actual.Length; i++)
                Assert.AreSame(actual[i].GetType(), expected[i]);
        }


        [Test]
        public void TestAssemblyNotFound() {
            PluginManager pm = Init(CreateManifest("MustNotExist", "TestPlugin"));
            CheckErrorMessage(pm, "PluginManager.Messages.AssemblyLoadError", "MustNotExist");
        }
        [Test]
        public void TestTypeNotFound() {
            PluginManager pm = Init(CreateManifest("MustNotExist"));
            CheckErrorMessage(pm, "PluginManager.Messages.TypeLoadError", this.GetType().Assembly.CodeBase, "Poderosa.Plugins.MustNotExist");
        }
        [Test]
        public void TestMissingIPlugin() {
            PluginManager pm = Init(CreateManifest("MissingIPlugin"));
            CheckErrorMessage(pm, "PluginManager.Messages.IPluginIsNotImplemented", "Poderosa.Plugins.MissingIPlugin");
        }
        [Test]
        public void TestMissingPluginInfo() {
            PluginManager pm = Init(CreateManifest("MissingPluginInfo"));
            CheckErrorMessage(pm, "PluginManager.Messages.PluginInfoAttributeNotFound", "Poderosa.Plugins.MissingPluginInfo");
        }
        [Test]
        public void TestMissingPluginID() {
            PluginManager pm = Init(CreateManifest("MissingID"));
            CheckErrorMessage(pm, "PluginManager.Messages.IDNotFound", "Poderosa.Plugins.MissingID");
        }
        [Test]
        public void TestDuplicatedID() {
            PluginManager pm = Init(CreateManifest2("TestPlugin1", "DuplicatedPlugin"));
            CheckErrorMessage(pm, "PluginManager.Messages.IDDuplication", "Poderosa.Plugins.DuplicatedPlugin", "org.poderosa.test1");
        }
        [Test]
        public void TestMissingDependency() {
            PluginManager pm = Init(CreateManifest("MissingDependency"));
            CheckErrorMessage(pm, "PluginManager.Messages.DependencyNotFound", "Poderosa.Plugins.MissingDependency", "MustNotExist");
        }
        [Test]
        public void TestMissingDependency2() {
            //MissingDependency2はMissingDependencyに依存しているが、MissingDependencyがロードできないときにどうなるか
            PluginManager pm = Init(CreateManifest2("MissingDependency", "MissingDependency2"));
            CheckErrorMessage(pm, "PluginManager.Messages.DependencyNotFound", "Poderosa.Plugins.MissingDependency", "MustNotExist");
        }
        [Test]
        public void TestDependencyLoopError() {
            PluginManager pm = Init(CreateManifest2("DepLoop1", "DepLoop2"));
            CheckErrorMessage(pm, "PluginManager.Messages.DependencyLoopError", "org.poderosa.depLoop1;org.poderosa.depLoop2");
        }


        //TODO テスト追加：IDの書式が正しいことの確認 

        //以下、正常系のテスト
        [Test]
        public void TestNoDependencies() {
            PluginManager pm = Init(CreateManifest2("TestPlugin1", "TestPlugin2"));
            Assert.IsFalse(pm.HasError);

            VerifyPluginOrder(pm.GetOrderedPlugins(), new Type[] { typeof(TestPlugin1), typeof(TestPlugin2) });
        }
        [Test]
        public void TestDependency() {
            PluginManager pm = Init(CreateManifest2("TestPluginDep", "TestPlugin1"));
            Assert.IsFalse(pm.HasError);

            //Manifestでの順番と逆転していることを確認
            VerifyPluginOrder(pm.GetOrderedPlugins(), new Type[] { typeof(TestPlugin1), typeof(TestPluginDep) });
        }

    }

    internal class MissingIPlugin {
    }
    internal class MissingPluginInfo : PluginBase {
    }
    [PluginInfo(Name = "test")]
    internal class MissingID : PluginBase {
    }
    [PluginInfo(ID = "org.poderosa.test1", Name = "test")]
    internal class DuplicatedPlugin : PluginBase {
    }
    [PluginInfo(ID = "org.poderosa.test1", Name = "test", Dependencies = "MustNotExist")]
    internal class MissingDependency : PluginBase {
    }
    [PluginInfo(ID = "org.poderosa.test2", Name = "test", Dependencies = "org.poderosa.test1")]
    internal class MissingDependency2 : PluginBase {
    }
    [PluginInfo(ID = "org.poderosa.depLoop1", Name = "test", Dependencies = "org.poderosa.depLoop2")]
    internal class DepLoop1 : PluginBase {
    }
    [PluginInfo(ID = "org.poderosa.depLoop2", Name = "test", Dependencies = "org.poderosa.depLoop1")]
    internal class DepLoop2 : PluginBase {
    }

    [PluginInfo(ID = "org.poderosa.test1", Name = "test1")]
    internal class TestPlugin1 : PluginBase {
    }
    [PluginInfo(ID = "org.poderosa.test2", Name = "test2")]
    internal class TestPlugin2 : PluginBase {
    }
    [PluginInfo(ID = "org.poderosa.testDep", Name = "testDep", Dependencies = "org.poderosa.test1")]
    internal class TestPluginDep : PluginBase {
    }
}
#endif