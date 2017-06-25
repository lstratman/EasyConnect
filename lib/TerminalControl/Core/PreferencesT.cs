/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PreferencesT.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
#if UNITTEST
using System;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Diagnostics;
using NUnit.Framework;

using Poderosa.Preferences;
using Poderosa.Plugins;
using Poderosa.Boot;

namespace Poderosa.Preferences {

    [PluginInfo(ID = "org.poderosa.unittests.preferences", Dependencies = PreferencePlugin.EXTENSIONPOINT_NAME)]
    internal class PreferenceTestPlugin : PluginBase {
        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            poderosa.PluginManager.FindExtensionPoint(PreferencePlugin.EXTENSIONPOINT_NAME)
                .RegisterExtension(PreferenceTests.CurrentTestSupplier);
        }
    }

    [TestFixture]
    public class PreferenceTests {
        private IPoderosaApplication _poderosaApplication;
        private StructuredText _rootNote;

        //Note staticなのはちょっといやらしい
        private static IPreferenceSupplier _testSupplier;
        public static IPreferenceSupplier CurrentTestSupplier {
            get {
                return _testSupplier;
            }
        }

        private abstract class EmptyPreferenceSupplier : IPreferenceSupplier {

            public IPreferenceFolder _rootFolder;

            public IPreferenceFolder RootFolder {
                get {
                    return _rootFolder;
                }
            }

            public virtual void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
                _rootFolder = folder;
            }

            public virtual object QueryAdapter(IPreferenceFolder folder, Type type) {
                return null;
            }

            public virtual string GetDescription(IPreferenceItem item) {
                return "";
            }

            public virtual void ValidateItem(IPreferenceItem item, IPreferenceValidationResult output) {
            }

            public virtual void ValidateFolder(IPreferenceFolder folder, IPreferenceValidationResult output) {
            }

            public string PreferenceID {
                get {
                    return "unittest";
                }
            }
        }


        private string Dump(StructuredText node) {
            StringWriter w = new StringWriter();
            new TextStructuredTextWriter(w).Write(node);
            w.Close();
            return w.ToString();
        }
        private StructuredText CreateRoot() {
            return CreateRoot("unittest {\r\n}");
        }
        private StructuredText CreateRoot(string expr) {
            StringReader r = new StringReader(expr);
            return new TextStructuredTextReader(r).Read();
        }
        private void InitPreference(IPreferenceSupplier s, string expr) {
            _testSupplier = s;
            _rootNote = expr == null ? CreateRoot() : CreateRoot(expr);

            _poderosaApplication = PoderosaStartup.CreatePoderosaApplication(CreatePluginManifest(), new StructuredText(null, "Poderosa").AddChild(_rootNote));
            _poderosaApplication.Start();
        }

        private string CreatePluginManifest() {
            return String.Format("Root {{\r\n  {0} {{\r\n  plugin=Poderosa.Preferences.PreferenceTestPlugin\r\n  plugin=Poderosa.Preferences.PreferencePlugin\r\n}}\r\n}}", this.GetType().Assembly.CodeBase);
        }

        [TestFixtureSetUp]
        public void Init() {
            //TODO PluginManifestのいくつかはここに入れることできそう
        }

        [Test]
        public void TestEmpty() {
            InitPreference(new SimpleSupplier(), null);
            _poderosaApplication.Shutdown();
            Assert.AreEqual("unittest {\r\n}\r\n", Dump(_rootNote));
        }

        [Test]
        public void TestWrite() {
            SimpleSupplier supplier = new SimpleSupplier();
            InitPreference(supplier, null);
            supplier.SetI(12);
            _poderosaApplication.Shutdown();
            Assert.AreEqual("unittest {\r\n  i=12\r\n}\r\n", Dump(_rootNote)); //デフォルト値は記録されないことに注意
        }

        [Test]
        public void TestRead() {
            SimpleSupplier supplier = new SimpleSupplier();
            InitPreference(supplier, "unittest {\r\n  i=20\r\n  s=vieri\r\n}\r\n");
            Assert.AreEqual(20, supplier.GetI());
            Assert.AreEqual("vieri", supplier.GetS());
        }

        private class SimpleSupplier : EmptyPreferenceSupplier {
            private IIntPreferenceItem _i;
            private IStringPreferenceItem _s;
            public override void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
                base.InitializePreference(builder, folder);
                _i = builder.DefineIntValue(folder, "i", 10, null);
                _s = builder.DefineStringValue(folder, "s", "bobo", null);
            }

            public int GetI() {
                return _i.Value;
            }
            public void SetI(int value) {
                _i.Value = value;
            }
            public string GetS() {
                return _s.Value;
            }
            public void SetS(string value) {
                _s.Value = value;
            }
        }

        [Test]
        public void TestUserFriendlyInterface() {
            UserFriendlySupplier supplier = new UserFriendlySupplier();
            InitPreference(supplier, "unittest {\r\n  i=20\r\n  s=k\r\n}\r\n");

            //Query
            IUserFriendlyInterface if2 = (IUserFriendlyInterface)supplier.RootFolder.QueryAdapter(typeof(IUserFriendlyInterface));
            IUserFriendlyInterface ifx = (IUserFriendlyInterface)supplier.RootFolder.QueryAdapter(typeof(IUserFriendlyInterface));
            Assert.IsTrue(Object.ReferenceEquals(if2, ifx));

            Assert.AreEqual(20, if2.i);
            Assert.AreEqual("k", if2.s);

            if2.i = 30;
            if2.s = "z";
            _poderosaApplication.Shutdown();
            Assert.AreEqual("unittest {\r\n  i=30\r\n  s=z\r\n}\r\n", Dump(_rootNote));
        }

        private interface IUserFriendlyInterface {
            int i {
                get;
                set;
            }
            string s {
                get;
                set;
            }
        }

        private class UserFriendlySupplier : EmptyPreferenceSupplier, IUserFriendlyInterface {
            private IIntPreferenceItem _i;
            private IStringPreferenceItem _s;
            private IPreferenceFolder _folder;

            public override void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
                base.InitializePreference(builder, folder);
                _folder = folder;
                _i = builder.DefineIntValue(folder, "i", 10, null);
                _s = builder.DefineStringValue(folder, "s", "bobo", null);
            }

            private UserFriendlySupplier CloneFor(IPreferenceFolder snapshot) {
                UserFriendlySupplier n = new UserFriendlySupplier();
                n._folder = snapshot;
                n._i = snapshot.ChildAt(0) as IIntPreferenceItem;
                n._s = snapshot.ChildAt(1) as IStringPreferenceItem;
                return n;
            }

            public int i {
                get {
                    return _i.Value;
                }
                set {
                    _i.Value = value;
                }
            }

            public string s {
                get {
                    return _s.Value;
                }
                set {
                    _s.Value = value;
                }
            }

            public override object QueryAdapter(IPreferenceFolder folder, Type adapter) {
                //Snapshotに対するアクションがあるので、folder==_folderの比較ではアウト
                if (adapter == typeof(IUserFriendlyInterface)) {
                    if (_folder == folder)
                        return this;
                    else if (folder.Id == _folder.Id)
                        return this.CloneFor(folder);
                }

                //失敗ケース
                return null;
            }
        }

        [Test]
        public void TestValidators1() {
            ValidatingSupplier supplier = new ValidatingSupplier();
            InitPreference(supplier, null);

            Assert.AreEqual(0, supplier._validatedI);
            Assert.AreEqual(0, supplier._validatedS); //initial valueで初期化されたはず

            supplier.SetI(5);
            supplier.SetS("aaaaa");
            Assert.AreEqual(1, supplier._validatedI);
            Assert.AreEqual(1, supplier._validatedS); //それぞれ呼ばれたはず
        }
        [Test]
        public void TestValidators2() {
            ValidatingSupplier supplier = new ValidatingSupplier();
            InitPreference(supplier, "unittest {\r\n  s=vieri\r\n  i=30\r\n}\r\n"); //順番いれかえてみた

            Assert.AreEqual(1, supplier._validatedI);
            Assert.AreEqual(1, supplier._validatedS); //パースした値をValidateしたはず

            Assert.AreEqual(10, supplier.GetI()); //値のエラーにより初期化されたはず

        }
        [Test]
        public void TestValidators3() {
            ValidatingSupplier supplier = new ValidatingSupplier();
            InitPreference(supplier, null);

            bool caught = false;
            IPreferenceValidationResult result = null;
            try {
                supplier.SetI(20);
            }
            catch (ValidationException ex) {
                caught = true;
                result = ex.Result;
            }
            Assert.AreEqual(true, caught); //エラーにならないとおかしい
            supplier.SetS("aaaaa");
            Assert.AreEqual(1, supplier._validatedI);
            Assert.AreEqual(1, supplier._validatedS); //それぞれ呼ばれたはず

            Assert.AreEqual("must be 0-10", result.ErrorMessage); //他をValidationしてもコピーが残っていないとダメ
        }
        private class ValidatingSupplier : EmptyPreferenceSupplier {
            private IIntPreferenceItem _i;
            private IStringPreferenceItem _s;
            public override void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
                base.InitializePreference(builder, folder);
                _i = builder.DefineIntValue(folder, "i", 10, new PreferenceItemValidator<int>(ValidateI));
                _s = builder.DefineStringValue(folder, "s", "bobo", new PreferenceItemValidator<string>(ValidateS));
            }

            public void SetI(int value) {
                _i.Value = value;
            }
            public int GetI() {
                return _i.Value;
            }
            public void SetS(string value) {
                _s.Value = value;
            }
            private void ValidateI(int value, IPreferenceValidationResult result) {
                if (value < 0 || value > 10)
                    result.ErrorMessage = "must be 0-10";
                _validatedI++;
            }
            private void ValidateS(string value, IPreferenceValidationResult result) {
                if (value.Length > 10)
                    result.ErrorMessage = "too long";
                _validatedS++;
            }

            public int _validatedI;
            public int _validatedS;
        }

        [Test]
        public void TestCloneImport() {
            UserFriendlySupplier supplier = new UserFriendlySupplier();
            InitPreference(supplier, "unittest {\r\n  i=20\r\n  s=vieri\r\n}\r\n");

            //Query and Snapshot
            IUserFriendlyInterface if1 = (IUserFriendlyInterface)supplier.RootFolder.QueryAdapter(typeof(IUserFriendlyInterface));
            IPreferenceFolder cl = supplier.RootFolder.Clone();
            IUserFriendlyInterface if2 = (IUserFriendlyInterface)cl.QueryAdapter(typeof(IUserFriendlyInterface));
            Assert.IsTrue(if1 != if2);

            Assert.AreEqual(20, if2.i);
            Assert.AreEqual("vieri", if2.s); //値がコピーされたことを確認

            if2.i = 10;
            Assert.AreEqual(10, if2.i);
            Assert.AreEqual(20, if1.i); //片方だけ変更されたことを確認

            supplier.RootFolder.Import(cl);
            Assert.AreEqual(10, if2.i);
            Assert.AreEqual(10, if1.i); //インポートされたことを確認
        }

        [Test]
        public void TestListener() {
            ValidatingSupplier supplier = new ValidatingSupplier();
            InitPreference(supplier, null);

            ChangeListener l = new ChangeListener();
            supplier.RootFolder.AddChangeListener(l);

            IPreferenceFolder f = supplier.RootFolder.Clone();
            supplier.RootFolder.Import(f);
            Assert.AreEqual(1, l._count);
            //もうちょっと確認したい

        }

        private class ChangeListener : IPreferenceChangeListener {

            public void OnPreferenceImport(IPreferenceFolder oldvalues, IPreferenceFolder newvalues) {
                _lastImportedFolder = newvalues;
                _count++;
            }


            public int _count;
            //public IPreferenceItem _lastChangeItem;
            public IPreferenceFolder _lastImportedFolder;

        }

        [Test]
        public void TestFolderArrayRead() {
            FolderArraySupplier supplier = new FolderArraySupplier();
            InitPreference(supplier, "unittest {\r\n  foo {\r\n  i=20\r\n  s=vieri\r\n}\r\nfoo {\r\n  i=10\r\n  s=bobo\r\n}\r\n");

            IPreferenceFolder root = supplier.RootFolder;
            IPreferenceFolderArray array = root.FindChildFolderArray("foo");

            Assert.AreEqual(2, array.Folders.Length);
            IPreferenceFolder foo1 = array.Folders[0];
            Assert.AreEqual("vieri", ((IStringPreferenceItem)foo1.FindItem("s")).Value); //ToDo 文字列検索はいやらしい
            IPreferenceFolder foo2 = array.Folders[1];
            Assert.AreEqual("bobo", ((IStringPreferenceItem)foo2.FindItem("s")).Value);
        }

        [Test]
        public void TestFolderArrayWrite() {
            FolderArraySupplier supplier = new FolderArraySupplier();
            InitPreference(supplier, "unittest {\r\n}\r\n");

            IPreferenceFolder root = supplier.RootFolder;
            IPreferenceFolderArray array = root.FindChildFolderArray("foo");

            IPreferenceFolder foo1 = array.CreateNewFolder();
            IPreferenceFolder foo2 = array.CreateNewFolder();
            //((IStringPreferenceItem)foo2.FindItem("s")).Value = "zlatan";
            //((IIntPreferenceItem)foo1.FindItem("i")).Value = 5;
            array.ConvertItem(foo2, supplier._s).AsString().Value = "zlatan";
            array.ConvertItem(foo1, supplier._i).AsInt().Value = 5;

            _poderosaApplication.Shutdown();
            Assert.AreEqual("unittest {\r\n  foo {\r\n    i=5\r\n  }\r\n  foo {\r\n    s=zlatan\r\n  }\r\n}\r\n", Dump(_rootNote));

        }

        private class FolderArraySupplier : EmptyPreferenceSupplier {
            public IIntPreferenceItem _i;
            public IStringPreferenceItem _s;
            public override void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
                base.InitializePreference(builder, folder);
                IPreferenceFolder foo = builder.DefineFolderArray(folder, this, "foo");
                _i = builder.DefineIntValue(foo, "i", 10, null);
                _s = builder.DefineStringValue(foo, "s", "bobo", null);
            }
        }
    }
}
#endif