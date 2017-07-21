/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Preferences.cs,v 1.4 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.Plugins;
using Poderosa.Boot;
using Poderosa.Util;
using Poderosa.Util.Collections;
using Poderosa.Util.Generics;

[assembly: PluginDeclaration(typeof(Poderosa.Preferences.PreferencePlugin))]

namespace Poderosa.Preferences {

    [PluginInfo(ID = PreferencePlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Name = "Preferences")]
    internal class PreferencePlugin : PluginBase, IRootExtension, IPreferences {
        public const string PLUGIN_ID = "org.poderosa.core.preferences";
        public const string EXTENSIONPOINT_NAME = "org.poderosa.core.preferences";

        private static PreferencePlugin _instance;

        private IExtensionPoint _extensionPoint;
        private TypedHashtable<string, PlugInHost> _idToHosts;

        public static PreferencePlugin Instance {
            get {
                return _instance;
            }
        }

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;
            _idToHosts = new TypedHashtable<string, PlugInHost>();
            poderosa.PluginManager.FindExtensionPoint("org.poderosa.root").RegisterExtension(this);
            _extensionPoint = poderosa.PluginManager.CreateExtensionPoint(EXTENSIONPOINT_NAME, typeof(IPreferenceSupplier), this);
        }
        public override void TerminatePlugin() {
            base.TerminatePlugin();
            IStartupContextSupplier s = (IStartupContextSupplier)_poderosaWorld.GetAdapter(typeof(IStartupContextSupplier));

            Flush(); //ファイルの有無に関わらず内部のStorageNodeは更新しとく

            if (s.PreferenceFileName != null) {
                string preferenceFileName = s.PreferenceFileName;
                string tempPreferenceFileName = preferenceFileName + ".tmp";
                string prevPreferenceFileName = preferenceFileName + ".prev";

                try {
                    using (TextWriter writer = new StreamWriter(tempPreferenceFileName, false, System.Text.Encoding.Default)) {
                        WritePreference(writer);
                    }
                    if (File.Exists(preferenceFileName)) {
                        File.Delete(prevPreferenceFileName);
                        File.Move(preferenceFileName, prevPreferenceFileName);
                    }
                    File.Move(tempPreferenceFileName, preferenceFileName);
                }
                catch (Exception ex) {
                    RuntimeUtil.ReportException(ex);
                }
            }
        }

        public void InitializeExtension() {
            IStartupContextSupplier s = (IStartupContextSupplier)_poderosaWorld.GetAdapter(typeof(IStartupContextSupplier));

            int index = 0;
            foreach (IPreferenceSupplier supplier in _extensionPoint.GetExtensions()) {
                PlugInHost ph = new PlugInHost(this, supplier, s.Preferences, index++);
                ph.Build(); //Note 遅延読み込みをしてもいいかも
                _idToHosts[ph.PreferenceSupplier.PreferenceID] = ph;
            }
        }

        public void Flush() {
            foreach (PlugInHost host in _idToHosts.Values)
                host.Flush();
        }

        public void WritePreference(TextWriter writer) {
            IStartupContextSupplier s = (IStartupContextSupplier)_poderosaWorld.GetAdapter(typeof(IStartupContextSupplier));
            new TextStructuredTextWriter(writer).Write(s.Preferences);
        }

        #region IPreferences
        public IPreferenceFolder FindPreferenceFolder(string id) {
            PlugInHost ph = _idToHosts[id];
            return ph == null ? null : ph.RootFolder;
        }
        public IPreferenceFolder[] GetAllFolders() {
            List<IPreferenceFolder> result = new List<IPreferenceFolder>();
            foreach (PlugInHost ph in _idToHosts.Values)
                result.Add(ph.RootFolder);
            return result.ToArray();
        }
        #endregion
    }

    //SupplierがValidateした結果を格納する
    internal class PreferenceValidationResult : IPreferenceValidationResult {
        private string _message;

        public PreferenceValidationResult() {
            _message = null;
        }

        //IPreferenceValidationResult
        public bool Validated {
            get {
                return _message == null; //何かセットされていればエラー
            }
        }

        public string ErrorMessage {
            get {
                return _message;
            }
            set {
                _message = value;
            }
        }

        internal void Reset() {
            _message = null;
        }

        internal PreferenceValidationResult Clone() {
            PreferenceValidationResult r = new PreferenceValidationResult();
            r._message = _message;
            return r;
        }
    }

    //こいつがプラグインをホストして初期化を行う
    internal class PlugInHost : IPreferenceBuilder {

        private PreferencePlugin _parent;
        private IPreferenceSupplier _supplier;
        private string _supplierID;
        private int _index;

        private StructuredText _storageNode;
        private PreferenceFolder _supplierRootFolder;

        private PreferenceValidationResult _sharedResult;
        private bool _loading;

        public PlugInHost(PreferencePlugin parent, IPreferenceSupplier supplier, StructuredText root, int index) {
            _parent = parent;
            _supplier = supplier;
            _storageNode = root.FindChild(supplier.PreferenceID);
            if (_storageNode == null)
                _storageNode = root.AddChild(supplier.PreferenceID); //空で作成しておく
            _supplierID = supplier.PreferenceID;
            _index = index;
            _sharedResult = new PreferenceValidationResult();
        }

        //properties
        internal IPreferenceFolder RootFolder {
            get {
                return _supplierRootFolder;
            }
        }
        internal IPreferenceSupplier PreferenceSupplier {
            get {
                return _supplier;
            }
        }
        internal PreferenceValidationResult SharedValidationResult {
            get {
                return _sharedResult;
            }
        }
        internal bool IsLoading {
            get {
                return _loading;
            }
        }
        internal PreferencePlugin PreferencePlugin {
            get {
                return _parent;
            }
        }

        //Preferenceの内容を構築し、StorageNodeから読み込む
        internal void Build() {
            _supplierRootFolder = new PreferenceFolder(this, null, _supplierID, _index);
            _supplierRootFolder.PreferenceSupplier = _supplier;
            _supplier.InitializePreference(this, _supplierRootFolder);
            try {
                _loading = true;
                Debug.Assert(_storageNode.Parent != null);
                _supplierRootFolder.LoadFrom(_storageNode);
            }
            finally {
                _loading = false;
            }

        }

        //書き出し
        internal void Flush() {
            Debug.Assert(_storageNode.Parent != null);
            _supplierRootFolder.SaveTo(_storageNode);
        }

        //ValidationErrorの通知
        internal void ValidationError(IPreferenceItemBase item, PreferenceValidationResult result) {
            if (_loading) //ロード中のエラーは初期値に戻すだけ。どこかにWarningは出してもいいかもな
                item.ResetValue();
            else
                throw new ValidationException(item, result.Clone());
        }

        //IPreferenceBuilder実装
        //TODO 以下でid duplication check だがこのあたりは起動するたびに必要なので過剰なチェックはいやらしいが
        public IPreferenceFolder DefineFolder(IPreferenceFolder parent, IPreferenceSupplier supplier, string id) {
            PreferenceFolder p = CastFolder(parent);
            PreferenceFolder ch = new PreferenceFolder(this, p, id, parent.ChildCount);
            p.AddChild(ch);
            ch.PreferenceSupplier = supplier == null ? _supplier : supplier; //nullのときは自分自身を使う
            return ch;
        }
        public IPreferenceFolder DefineFolderArray(IPreferenceFolder parent, IPreferenceSupplier supplier, string id) {
            PreferenceFolder p = CastFolder(parent);
            PreferenceFolder template = new PreferenceFolder(this, p, id, parent.ChildCount);
            PreferenceFolderArray array = new PreferenceFolderArray(this, p, id, parent.ChildCount, template);
            p.AddChild(array);
            template.PreferenceSupplier = supplier == null ? _supplier : supplier; //nullのときは自分自身のを使う
            return template;
        }

        public IPreferenceLooseNode DefineLooseNode(IPreferenceFolder parent, IPreferenceLooseNodeContent content, string id) {
            PreferenceFolder p = CastFolder(parent);
            PreferenceLooseNode n = new PreferenceLooseNode(content, p, id, parent.ChildCount);
            p.AddChild(n);
            return n;
        }


        public IBoolPreferenceItem DefineBoolValue(IPreferenceFolder parent, string id, bool initial_value, PreferenceItemValidator<bool> validator) {
            PreferenceFolder f = CastFolder(parent);
            BoolPreferenceItem item = new BoolPreferenceItem(f, id, parent.ChildCount, initial_value, validator);
            f.AddChild(item);
            return item;
        }

        public IIntPreferenceItem DefineIntValue(IPreferenceFolder parent, string id, int initial_value, PreferenceItemValidator<int> validator) {
            PreferenceFolder f = CastFolder(parent);
            IntPreferenceItem item = new IntPreferenceItem(f, id, parent.ChildCount, initial_value, validator);
            f.AddChild(item);
            return item;
        }

        public IStringPreferenceItem DefineStringValue(IPreferenceFolder parent, string id, string initial_value, PreferenceItemValidator<string> validator) {
            PreferenceFolder f = CastFolder(parent);
            StringPreferenceItem item = new StringPreferenceItem(f, id, parent.ChildCount, initial_value, validator);
            f.AddChild(item);
            return item;
        }

        private PreferenceFolder CastFolder(IPreferenceFolder folder) {
            return (PreferenceFolder)folder;
        }
    }

    //Folder, Itemの基底
    internal abstract class PreferenceItemBase : IPreferenceItemBase {

        protected PreferenceFolder _parent;
        protected string _id;
        protected int _index;

        protected PreferenceItemBase(PreferenceFolder folder, string id, int index) {
            _parent = folder;
            _id = id;
            _index = index;
        }

        public string Id {
            get {
                return _id;
            }
        }

        public string FullQualifiedId {
            get {
                return _parent == null ? _id : _parent.FullQualifiedId + "." + _id;
            }
        }

        public int Index {
            get {
                return _index;
            }
        }

        //IPreferenceItemBase

        //非nullを返すやつのみoverrideせよ
        public virtual IPreferenceFolder AsFolder() {
            return null;
        }
        public virtual IPreferenceFolderArray AsFolderArray() {
            return null;
        }
        public virtual IPreferenceItem AsItem() {
            return null;
        }

        public abstract void ResetValue();

        //utils
        internal abstract IPreferenceSupplier GetSupplier();
        internal abstract PreferenceItemBase CreateSnapshot();

    }

    //Folder
    internal sealed class PreferenceFolder : PreferenceItemBase, IPreferenceFolder {

        private IPreferenceSupplier _supplier;
        private ArrayList _children;
        private PlugInHost _master;
        private ListenerList<IPreferenceChangeListener> _listenerList;

        public PreferenceFolder(PlugInHost master, PreferenceFolder parent, string id, int index)
            : base(parent, id, index) {
            _children = new ArrayList();
            _master = master;
        }

        internal IPreferenceSupplier PreferenceSupplier {
            get {
                return _supplier;
            }
            set {
                _supplier = value;
            }
        }

        internal override IPreferenceSupplier GetSupplier() {
            return _supplier == null ? _parent.GetSupplier() : _supplier;
        }
        internal PlugInHost GetHost() {
            return _master;
        }

        //IPreferenceFolder
        /*
        public IAdaptable GetAdapter(Type type) {
            return _master.PreferencePlugin.PoderosaWorld.AdapterManager.GetAdapter(this, type);
        }
         */
        public override IPreferenceFolder AsFolder() {
            return this;
        }
        public int ChildCount {
            get {
                return _children.Count;
            }
        }

        public override void ResetValue() {
            foreach (PreferenceItemBase t in _children)
                t.ResetValue();
        }
        public IPreferenceFolder FindChildFolder(string id) {
            IPreferenceItemBase t = FindChild(id);
            return t == null ? null : t.AsFolder();
        }
        public IPreferenceFolderArray FindChildFolderArray(string id) {
            IPreferenceItemBase t = FindChild(id);
            return t == null ? null : t.AsFolderArray();
        }
        public IPreferenceItem FindItem(string id) {
            IPreferenceItemBase t = FindChild(id);
            return t == null ? null : t.AsItem();
        }
        //TODO FindLooseNode

        public IPreferenceItemBase ChildAt(int index) {
            return (IPreferenceItemBase)_children[index];
        }

        public IPreferenceFolder Clone() {
            return CreateSnapshot() as IPreferenceFolder; //CreateSnapshotはFolderを返すのでOK
        }
        public object QueryAdapter(Type type) {
            return _supplier == null ? _parent.QueryAdapter(type) : _supplier.QueryAdapter(this, type);
        }

        public void AddChangeListener(IPreferenceChangeListener listener) {
            if (_listenerList == null)
                _listenerList = new ListenerList<IPreferenceChangeListener>();
            _listenerList.Add(listener);
        }

        public void RemoveChangeListener(IPreferenceChangeListener listener) {
            if (_listenerList != null)
                _listenerList.Remove(listener);
        }

        public void Import(IPreferenceFolder newvalues) {
            if (this.FullQualifiedId != newvalues.FullQualifiedId)
                throw new InvalidOperationException("ID mismatch");

            PreferenceValidationResult r = GetHost().SharedValidationResult;
            r.Reset();

            GetSupplier().ValidateFolder(newvalues, r);
            if (!r.Validated)
                GetHost().ValidationError(this, r);
            else {
                //fire listener
                //このあたりの仕様いまいちだな。Folderの階層とイベントの関係を明らかにしておきたい
                if (_listenerList != null) {
                    foreach (IPreferenceChangeListener l in _listenerList)
                        l.OnPreferenceImport(this, newvalues);
                }

                ImportSnapshot(newvalues as PreferenceFolder);
            }
        }

        //内部のサポート系

        internal void AddChild(PreferenceItemBase item) {
            _children.Add(item);
        }

        internal IPreferenceItemBase FindChild(string id) {
            foreach (IPreferenceItemBase t in _children) {
                if (t.Id == id)
                    return t;
            }
            return null;
        }


        internal void LoadFrom(StructuredText node) {
            bool dirty = false;
            int child_index = 0;
            int data_index = 0;

            while (child_index < _children.Count) {
                IPreferenceItemBase child = (IPreferenceItemBase)_children[child_index];
                PreferenceFolder ch_folder = child as PreferenceFolder;
                PreferenceFolderArray ch_array = child as PreferenceFolderArray;
                PreferenceItem ch_item = child as PreferenceItem;
                PreferenceLooseNode ch_loose = child as PreferenceLooseNode;

                //TODO 以下の分岐汚すぎ、何とかする

                if (ch_folder != null) {
                    StructuredText ch_data = node.GetChildOrNull(data_index); //大抵はうまく整列しているので検索の手間を省く
                    if (ch_data == null || ch_data.Name != ch_folder.Id) {
                        dirty = true;
                        ch_data = node.FindChild(ch_folder.Id);
                    }
                    else
                        data_index++;

                    if (ch_data == null)
                        ch_folder.ResetValue();
                    else
                        ch_folder.LoadFrom(ch_data);
                }
                else if (ch_item != null) {
                    StructuredText.Entry ch_data = node.GetEntryOrNull(data_index); //大抵はうまく整列しているので検索の手間を省く
                    if (ch_data == null || ch_data.name != ch_item.Id) {
                        dirty = true;
                        ch_data = node.FindEntry(ch_item.Id);
                    }
                    else
                        data_index++;

                    if (ch_data == null)
                        ch_item.ResetValue();
                    else
                        ch_item.TryToParse(ch_data.value, PreferenceItem.ErrorMode.Reset);
                }
                else if (ch_array != null) {
                    ArrayList ch_data = new ArrayList();
                    StructuredText t = node.GetChildOrNull(data_index);
                    if (t == null || t.Name != ch_array.Id) {
                        dirty = true;
                        ch_data.Clear();
                        ch_data.AddRange(node.FindMultipleNote(ch_array.Id));
                    }
                    else { //最初の一つが合格だったら継続して読み続ける
                        data_index++;
                        while (t != null && t.Name == ch_array.Id) {
                            ch_data.Add(t);
                            t = node.GetChildOrNull(data_index++);
                        }
                    }

                    ch_array.LoadFrom(ch_data);
                }
                else if (ch_loose != null) {
                    //TODO これはFolderのときと同じだ。まとめよう
                    StructuredText ch_data = node.GetChildOrNull(data_index); //大抵はうまく整列しているので検索の手間を省く
                    if (ch_data == null || ch_data.Name != ch_loose.Id) {
                        dirty = true;
                        ch_data = node.FindChild(ch_loose.Id);
                    }
                    else
                        data_index++;

                    if (ch_data == null)
                        ch_loose.ResetValue();
                    else
                        ch_loose.LoadFrom(ch_data);
                }

                child_index++; //自分の子をステップ
            }

            //一度エラーなく読むことができていればdirtyはfalseのまま
            node.IsDirty = dirty;
        }

        internal void SaveTo(StructuredText node) {
            node.Clear();
            foreach (PreferenceItemBase t in _children) {
                PreferenceFolder ch_folder = t as PreferenceFolder;
                PreferenceFolderArray ch_array = t as PreferenceFolderArray;
                PreferenceItem ch_item = t as PreferenceItem;
                PreferenceLooseNode ch_loose = t as PreferenceLooseNode;

                if (ch_folder != null) {
                    StructuredText ch = node.AddChild(ch_folder.Id);
                    ch_folder.SaveTo(ch);
                }
                else if (ch_item != null) { //item
                    if (ch_item.IsChanged) //デフォルト値と変わっていた場合のみ記録
                        node.Set(ch_item.Id, ch_item.FormatValue());
                }
                else if (ch_array != null) { // array
                    ch_array.SaveTo(node);
                }
                else if (ch_loose != null) {
                    ch_loose.SaveTo(node.AddChild(ch_loose.Id));
                }
            }
        }

        internal bool Contains(IPreferenceItem item) {
            return _children.Contains(item);
        }

        internal override PreferenceItemBase CreateSnapshot() {
            PreferenceFolder snapshot = new PreferenceFolder(_master, _parent, _id, _index);
            snapshot.PreferenceSupplier = _supplier;

            foreach (PreferenceItemBase t in _children) {
                snapshot._children.Add(t.CreateSnapshot());
            }
            return snapshot;
        }

        //中身はValidation済みであることに注意
        internal void ImportSnapshot(PreferenceFolder newvalues) {
            foreach (PreferenceItemBase t in _children) {
                PreferenceFolder child_folder = t as PreferenceFolder;
                if (child_folder != null)
                    child_folder.ImportSnapshot(newvalues.ChildAt(child_folder.Index) as PreferenceFolder);
                else {
                    PreferenceItem child_item = t as PreferenceItem;
                    if (child_item != null)
                        child_item.ImportSnapshot(newvalues.ChildAt(child_item.Index) as PreferenceItem);
                    else {
                        PreferenceLooseNode child_loosenode = t as PreferenceLooseNode;
                        if (child_loosenode != null)
                            child_loosenode.ImportSnapshot(newvalues.ChildAt(child_loosenode.Index) as PreferenceLooseNode);
                        //FolderArray未サポート
                    }
                }

            }
        }

    }

    //Folder Array
    internal class PreferenceFolderArray : PreferenceItemBase, IPreferenceFolderArray {

        private ArrayList _folders;
        private PlugInHost _master;
        private PreferenceFolder _template;

        public PreferenceFolderArray(PlugInHost master, PreferenceFolder parent, string id, int index, PreferenceFolder template)
            : base(parent, id, index) {
            _master = master;
            _folders = new ArrayList();
            _template = template;
        }

        public IPreferenceFolderArray Clone() {
            return CreateSnapshot() as IPreferenceFolderArray;
        }

        public void Import(IPreferenceFolderArray newvalues) {
            throw new Exception("unsupported");
        }

        public IPreferenceFolder[] Folders {
            get {
                return (IPreferenceFolder[])_folders.ToArray(typeof(IPreferenceFolder));
            }
        }
        public void Clear() {
            _folders.Clear();
        }
        public IPreferenceItem ConvertItem(IPreferenceFolder child_folder, IPreferenceItem item_in_template) {
            if (_id != child_folder.Id)
                throw new ArgumentException("child_folder is not compatible for this array");
            if (!_template.Contains(item_in_template))
                throw new ArgumentException("item_in_template must be a member of the template folder");

            IPreferenceItem r = (IPreferenceItem)child_folder.ChildAt(item_in_template.Index);
            Debug.Assert(r.GetType() == item_in_template.GetType());
            return r;
        }

        public IPreferenceFolder CreateNewFolder() {
            IPreferenceFolder f = _CreateNewFolder();
            f.ResetValue();
            return f;
        }
        internal PreferenceFolder _CreateNewFolder() {
            PreferenceFolder f = (PreferenceFolder)_template.CreateSnapshot();
            _folders.Add(f);
            return f;
        }


        public override IPreferenceFolderArray AsFolderArray() {
            return this;
        }

        public override void ResetValue() {
            _folders.Clear();
        }

        //これらはFolderとはちょっとコンベンション違う
        internal void LoadFrom(ArrayList notes) {
            _folders.Clear();
            foreach (StructuredText ch in notes) {
                PreferenceFolder f = _CreateNewFolder();
                f.LoadFrom(ch);
            }
        }
        internal void SaveTo(StructuredText parent) {
            foreach (PreferenceFolder f in _folders) {
                StructuredText ch = parent.AddChild(_id);
                f.SaveTo(ch);
            }
        }

        internal override IPreferenceSupplier GetSupplier() {
            return _template.GetSupplier();
        }
        internal override PreferenceItemBase CreateSnapshot() {
            PreferenceFolderArray cl = new PreferenceFolderArray(_master, _parent, _id, _index, _template);
            foreach (IPreferenceFolder f in _folders)
                cl._folders.Add(f.Clone());
            return cl;
        }
    }


    //Itemの基底。ここから各型に分岐
    internal abstract class PreferenceItem : PreferenceItemBase, IPreferenceItem {

        public enum ErrorMode {
            Reset,
            NotifyHost
        }

        public PreferenceItem(PreferenceFolder parent, string id, int index)
            : base(parent, id, index) {
        }

        public override IPreferenceItem AsItem() {
            return this;
        }
        internal override IPreferenceSupplier GetSupplier() {
            return _parent.GetSupplier();
        }

        /*
        public string Description {
            get {
                return _parent.GetHost().PreferenceSupplier.GetDescription(this);
            }
        }
         */

        protected PreferenceValidationResult GetSharedValidationResult() {
            PreferenceValidationResult r = _parent.GetHost().SharedValidationResult;
            r.Reset();
            return r;
        }
        protected void ValidationError(PreferenceValidationResult result) {
            _parent.GetHost().ValidationError(this, result);
        }

        //子で実装
        public virtual IBoolPreferenceItem AsBool() {
            return null;
        }
        public virtual IIntPreferenceItem AsInt() {
            return null;
        }
        public virtual IStringPreferenceItem AsString() {
            return null;
        }


        internal abstract string FormatValue();
        internal abstract void TryToParse(string value, ErrorMode errormode);
        internal abstract void ImportSnapshot(PreferenceItem item);
        internal abstract bool IsChanged {
            get;
        } //デフォルト値と違うかどうか

        protected bool Validate<T>(T value, PreferenceItemValidator<T> validator, ErrorMode errormode) {
            if (validator != null) {
                PreferenceValidationResult r = GetSharedValidationResult();
                validator(value, r);
                if (!r.Validated) {
                    if (errormode == ErrorMode.Reset)
                        ResetValue();
                    else
                        ValidationError(r);
                    return false;
                }
            }

            return true;
        }
    }

    internal abstract class TypedPreferenceItem<T> : PreferenceItem, ITypedPreferenceItem<T> {

        protected T _value;
        protected T _initialValue;
        protected PreferenceItemValidator<T> _validator;
        protected IPrimitiveAdapter<T> _primitiveAdapter; //Parse等はGenericsパラメータで直接使えない

        public TypedPreferenceItem(PreferenceFolder parent, string id, int index, T initialValue, PreferenceItemValidator<T> validator, IPrimitiveAdapter<T> adapter)
            : base(parent, id, index) {
            _initialValue = initialValue;
            _validator = validator;
            _primitiveAdapter = adapter;
        }

        public T Value {
            get {
                return _value;
            }
            set {
                if (!Validate(value, ErrorMode.NotifyHost))
                    return;
                _value = value;
            }
        }
        public PreferenceItemValidator<T> Validator {
            get {
                return _validator;
            }
            set {
                _validator = value;
            }
        }

        public T InitialValue {
            get {
                return _initialValue;
            }
        }

        public override void ResetValue() {
            _value = _initialValue;
        }

        internal override PreferenceItemBase CreateSnapshot() {
            TypedPreferenceItem<T> item = InternalClone();
            item._value = _value; //ノーイベント
            return item;
        }
        internal override void ImportSnapshot(PreferenceItem item) {
            TypedPreferenceItem<T> i = item as TypedPreferenceItem<T>;
            Debug.Assert(i != null);

            _value = i._value; //チェック済みなのでノーイベントで
        }
        internal override string FormatValue() {
            return _value.ToString();
        }
        internal override void TryToParse(string value, ErrorMode errormode) {
            T v = _primitiveAdapter.Parse(value);
            if (Validate(v, errormode))
                _value = v;
        }
        internal override bool IsChanged {
            get {
                return !_primitiveAdapter.Equals(_value, _initialValue);
            }
        }

        private bool Validate(T value, ErrorMode errormode) {
            if (_validator != null) {
                PreferenceValidationResult r = GetSharedValidationResult();
                _validator(value, r);
                if (!r.Validated) {
                    if (errormode == ErrorMode.Reset)
                        ResetValue();
                    else
                        ValidationError(r);
                    return false;
                }
            }

            return true;
        }

        protected abstract TypedPreferenceItem<T> InternalClone();
    }

    //NOTE
    // ここが型ごとのクラスに分かれているのは、I***PreferenceItemの個別インタフェースのサポートのためと、
    // object型をGenericsパラメータにしたとき, EqualsあたりはなんとかなってもParseがないためにトリックが必要だったことによる
    internal class BoolPreferenceItem : TypedPreferenceItem<bool>, IBoolPreferenceItem {
        private static IPrimitiveAdapter<bool> adapter = new BoolPrimitiveAdapter();
        public BoolPreferenceItem(PreferenceFolder parent, string id, int index, bool initialValue, PreferenceItemValidator<bool> validator)
            : base(parent, id, index, initialValue, validator, adapter) {
        }
        protected override TypedPreferenceItem<bool> InternalClone() {
            return new BoolPreferenceItem(_parent, _id, _index, _initialValue, _validator);
        }
        public override IBoolPreferenceItem AsBool() {
            return this;
        }
    }
    internal class IntPreferenceItem : TypedPreferenceItem<int>, IIntPreferenceItem {
        private static IPrimitiveAdapter<int> adapter = new IntPrimitiveAdapter();
        public IntPreferenceItem(PreferenceFolder parent, string id, int index, int initialValue, PreferenceItemValidator<int> validator)
            : base(parent, id, index, initialValue, validator, adapter) {
        }
        protected override TypedPreferenceItem<int> InternalClone() {
            return new IntPreferenceItem(_parent, _id, _index, _initialValue, _validator);
        }
        public override IIntPreferenceItem AsInt() {
            return this;
        }
    }
    internal class StringPreferenceItem : TypedPreferenceItem<string>, IStringPreferenceItem {
        private static IPrimitiveAdapter<string> adapter = new StringPrimitiveAdapter();
        public StringPreferenceItem(PreferenceFolder parent, string id, int index, string initialValue, PreferenceItemValidator<string> validator)
            : base(parent, id, index, initialValue, validator, adapter) {
        }
        protected override TypedPreferenceItem<string> InternalClone() {
            return new StringPreferenceItem(_parent, _id, _index, _initialValue, _validator);
        }
        public override IStringPreferenceItem AsString() {
            return this;
        }
    }

    //Loose Node
    internal class PreferenceLooseNode : PreferenceItemBase, IPreferenceLooseNode {
        private IPreferenceLooseNodeContent _content;

        public PreferenceLooseNode(IPreferenceLooseNodeContent content, PreferenceFolder folder, string id, int index)
            : base(folder, id, index) {
            _content = content;
        }
        public IPreferenceLooseNodeContent Content {
            get {
                return _content;
            }
        }

        public override void ResetValue() {
            _content.Reset();
        }

        public void LoadFrom(StructuredText node) {
            _content.LoadFrom(node);
        }
        public void SaveTo(StructuredText node) {
            _content.SaveTo(node);
        }

        internal override IPreferenceSupplier GetSupplier() {
            return _parent.GetSupplier();
        }
        internal override PreferenceItemBase CreateSnapshot() {
            return new PreferenceLooseNode(_content.Clone(), _parent, _id, _index);
        }
        internal void ImportSnapshot(PreferenceLooseNode node) {
            _content = node._content.Clone();
        }
    }

    //Enum Util
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exclude/>
    public class EnumPreferenceItem<T> where T : struct {
        private IStringPreferenceItem _preferenceItem;
        private string _description; //共有しているPrefItemが書き換えられた場合のカバー
        private T _defaultValue;
        private T _value;

        public IStringPreferenceItem PreferenceItem {
            get {
                return _preferenceItem;
            }
        }
        public T Value {
            get {
                if (_description != _preferenceItem.Value) {
                    _value = ParseUtil.ParseEnum<T>(_preferenceItem.Value, _defaultValue);
                    _description = _value.ToString();
                    _preferenceItem.Value = _description;
                }
                return _value;
            }
            set {
                _description = value.ToString();
                _value = value;
                _preferenceItem.Value = _description;
            }
        }

        public EnumPreferenceItem(IStringPreferenceItem item, T initial) {
            _preferenceItem = item;
            _description = "";
            _value = initial;
            _defaultValue = initial;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class ColorPreferenceItem {
        private IStringPreferenceItem _preferenceItem;
        private string _description;
        private Color _defaultValue;
        private Color _value;

        public IStringPreferenceItem PreferenceItem {
            get {
                return _preferenceItem;
            }
        }
        public Color Value {
            get {
                if (_description != _preferenceItem.Value) {
                    _value = ParseUtil.ParseColor(_preferenceItem.Value, _defaultValue);
                    _description = _value.Name;
                    _preferenceItem.Value = _description;
                }
                return _value;
            }
            set {
                _description = value.IsEmpty ? "Empty" : value.Name;
                _value = value;
                _preferenceItem.Value = _description;
            }
        }

        public Color DefaultValue {
            get {
                return _defaultValue;
            }
        }

        public ColorPreferenceItem(IStringPreferenceItem item, KnownColor initial) {
            _preferenceItem = item;
            _description = "";
            _value = Color.FromKnownColor(initial);
            _defaultValue = _value;
        }
        public ColorPreferenceItem(IStringPreferenceItem item, Color initial) {
            _preferenceItem = item;
            _description = "";
            _value = initial;
            _defaultValue = _value;
        }
    }

    //SnapshotとFriendlyInterface付きのPreferenceベースクラス
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public abstract class SnapshotAwarePreferenceBase {
        protected IPreferenceFolder _folder;

        public SnapshotAwarePreferenceBase(IPreferenceFolder folder) {
            _folder = folder;
        }

        protected IIntPreferenceItem ConvertItem(IIntPreferenceItem item) {
            return _folder.ChildAt(item.Index).AsItem().AsInt();
        }
        protected IStringPreferenceItem ConvertItem(IStringPreferenceItem item) {
            return _folder.ChildAt(item.Index).AsItem().AsString();
        }
        protected IBoolPreferenceItem ConvertItem(IBoolPreferenceItem item) {
            return _folder.ChildAt(item.Index).AsItem().AsBool();
        }
        protected ColorPreferenceItem ConvertItem(ColorPreferenceItem item) {
            return new ColorPreferenceItem(ConvertItem(item.PreferenceItem), item.DefaultValue);
        }
        protected EnumPreferenceItem<T> ConvertItem<T>(EnumPreferenceItem<T> item) where T : struct {
            IStringPreferenceItem t = _folder.ChildAt(item.PreferenceItem.Index).AsItem().AsString();
            return new EnumPreferenceItem<T>(t, item.Value);
        }

        //新規作成用
        public abstract void DefineItems(IPreferenceBuilder builder);

    }

    //バリデータ系
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public static class PreferenceValidatorUtil {
        private static PreferenceItemValidator<string> _keyWithModifierValidator;

        public static PreferenceItemValidator<string> KeyWithModifierValidator {
            get {
                if (_keyWithModifierValidator == null)
                    _keyWithModifierValidator = new PreferenceItemValidator<string>(KeyWithModifierValidatorBody);
                return _keyWithModifierValidator;
            }
        }

        private static void KeyWithModifierValidatorBody(string value, IPreferenceValidationResult result) {
            try {
                Keys k = WinFormsUtil.ParseKey(value);
                if ((k & Keys.Modifiers) == Keys.None)
                    result.ErrorMessage = "Modifier key is required";
            }
            catch (Exception ex) {
                result.ErrorMessage = ex.Message;
            }
        }

        private static PreferenceItemValidator<int> _positiveIntegerValidator;

        public static PreferenceItemValidator<int> PositiveIntegerValidator {
            get {
                if (_positiveIntegerValidator == null)
                    _positiveIntegerValidator = new PreferenceItemValidator<int>(PositiveIntegerValidatorBody);
                return _positiveIntegerValidator;
            }
        }
        private static void PositiveIntegerValidatorBody(int value, IPreferenceValidationResult result) {
            if (value < 0)
                result.ErrorMessage = CoreUtil.Strings.GetString("Message.ValueMustBePositive");
        }

        private class IntRange {
            private int _min;
            private int _max;

            public IntRange(int min, int max) {
                _min = min;
                _max = max;
            }

            public void Check(int value, IPreferenceValidationResult result) {
                if (value < _min || value > _max)
                    result.ErrorMessage = String.Format(CoreUtil.Strings.GetString("Message.ValueMustBeContainedRange"), _min, _max);
            }
        }

        public static PreferenceItemValidator<int> IntRangeValidator(int min, int max) {
            return new PreferenceItemValidator<int>(new IntRange(min, max).Check);
        }
    }

}
