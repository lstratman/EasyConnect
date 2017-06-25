/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Plugin.cs,v 1.3 2011/12/23 19:15:14 kzmi Exp $
 */
using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Poderosa.Util.Collections;
using Poderosa.Boot;

namespace Poderosa.Plugins {
    /// <summary>
    /// <ja>
    /// プラグインのステータスを示します。
    /// </ja>
    /// <en>
    /// Return the status of the plug-in.
    /// </en>
    /// </summary>
    public enum PluginStatus {
        /// <summary>
        /// <ja>名前で参照されただけの状態</ja>
        /// <en>State only declared.</en>
        /// </summary>
        Declared,  //名前で参照されただけの状態
        /// <summary>
        /// <ja>クラスとしてロードできた状態</ja>
        /// <en>Loaded as class.</en>
        /// </summary>
        Loaded,    //クラスとしてロードできた状態
        /// <summary>
        /// <ja>ロードはできたが依存先不明などで無効化された状態</ja>
        /// <en>It was nullified by the uncertainty etc. dependence ahead though it was possible to load.</en>
        /// </summary>
        Disabled,  //ロードはできたが依存先不明などで無効化された状態
        /// <summary>
        /// <ja>正常に読み込まれ、稼働済みの状態</ja>
        /// <en>Loaded successfully and activated.</en>
        /// </summary>
        Activated  //稼動済みの状態
    }

    internal class Plugin : IPluginInfo {
        private PluginManager _manager;
        private Assembly _assembly;
        private PluginStatus _status;

        private PluginInfoAttribute _attribute;
        private Plugin[] _dependencies;
        private Type _pluginType;
        private IPlugin _instance;

        public Plugin(PluginManager manager, Assembly assembly, Type type) {
            _manager = manager;
            _assembly = assembly;
            _pluginType = type;
            _status = PluginStatus.Declared;
        }

        public PluginInfoAttribute PluginInfo {
            get {
                return _attribute;
            }
        }
        public IPlugin Instance {
            get {
                return _instance;
            }
        }
        public string TypeName {
            get {
                return _pluginType.FullName;
            }
        }

        public Plugin[] Dependencies {
            get {
                return _dependencies;
            }
            set {
                _dependencies = value;
            }
        }


        public GenericResult TryToLoad() {
            string typename = _pluginType.FullName;
            if (!typeof(IPlugin).IsAssignableFrom(_pluginType)) {
                _manager.Tracer.Trace("PluginManager.Messages.IPluginIsNotImplemented", typename);
                return GenericResult.Failed;
            }

            object[] attrs = _pluginType.GetCustomAttributes(typeof(PluginInfoAttribute), false);
            if (attrs.Length != 1) {
                _manager.Tracer.Trace("PluginManager.Messages.PluginInfoAttributeNotFound", typename);
                return GenericResult.Failed;
            }

            _attribute = (PluginInfoAttribute)attrs[0];
            string id = _attribute.ID;
            if (id == null || id.Length == 0) {
                _manager.Tracer.Trace("PluginManager.Messages.IDNotFound", typename);
                return GenericResult.Failed;
            }
            _status = PluginStatus.Loaded;

            return GenericResult.Succeeded;
        }

        public void Disable() {
            _status = PluginStatus.Disabled;
        }

        public GenericResult Instantiate() {
            _instance = (IPlugin)_assembly.CreateInstance(_pluginType.FullName);
            _status = PluginStatus.Activated;
            return GenericResult.Succeeded;
        }

        #region IPluginInfo
        public IPlugin Body {
            get {
                return _instance;
            }
        }

        public PluginInfoAttribute PluginInfoAttribute {
            get {
                return _attribute;
            }
        }
        public PluginStatus Status {
            get {
                return _status;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return _manager.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion
    }

    internal class PluginManager : IPluginManager, IPluginInspector {
        private InternalPoderosaWorld _world;
        private List<Plugin> _allPlugins;
        private TypedHashtable<string, Plugin> _idToPlugin;
        private TypedHashtable<string, ExtensionPoint> _idToExtensionPoint;
        private List<Plugin> _orderedPlugins;
        private Plugin _currentInitializingPlugin;
        private ITracer _tracer;

        public PluginManager(InternalPoderosaWorld pw) {
            _world = pw;
            _idToExtensionPoint = new TypedHashtable<string, ExtensionPoint>();
        }

        public ITracer Tracer {
            get {
                return _tracer;
            }
        }

        public bool HasError {
            get {
                return !_tracer.Document.IsEmpty;
            }
        }

        public IPlugin[] GetOrderedPlugins() {
            IPlugin[] r = new IPlugin[_orderedPlugins.Count];
            for (int i = 0; i < r.Length; i++)
                r[i] = ((Plugin)_orderedPlugins[i]).Instance;
            return r;
        }

        //起動時によぶ
        public void InitializePlugins(PoderosaStartupContext sc) {
            try {
                _tracer = sc.Tracer;
                Debug.Assert(_tracer != null);

                _allPlugins = new List<Plugin>();
                ListPlugins(Assembly.GetEntryAssembly());
                ListPlugins(sc.PluginManifest);
                Load();
                Order();
                Instantiate();
            }
            catch (Exception ex) {
                _tracer.Trace(ex);
            }
        }

        //終了
        public void Shutdown() {
            //逆順で終了
            for (int i = _orderedPlugins.Count - 1; i >= 0; i--) {
                try {
                    Plugin p = _orderedPlugins[i];
                    p.Instance.TerminatePlugin();
                }
                catch (Exception ex) {
                    _tracer.Trace(ex);
                }
            }
        }

        #region IPluginManager
        public object FindPlugin(string id, Type adaptertype) {
            Plugin p = _idToPlugin[id];
            if (p == null)
                return null;
            else {
                if (p.Status != PluginStatus.Activated)
                    throw new InvalidOperationException("Plugin is not activated");

                return p.Instance.GetAdapter(adaptertype);
            }
        }

        public IExtensionPoint CreateExtensionPoint(string id, Type requiredInterface, IPlugin owner) {
            if (_currentInitializingPlugin == null && id != ExtensionPoint.ROOT) //ルートだけは
                throw new InvalidOperationException(InternalPoderosaWorld.Strings.GetString("PluginManager.Messages.NewExtensionPointOutsideInit"));
            if (_idToExtensionPoint.Contains(id))
                throw new ArgumentException(InternalPoderosaWorld.Strings.GetString("PluginManager.Messages.DuplicatedExtensionPointID"));
            ExtensionPoint e = new ExtensionPoint(id, requiredInterface, owner);
            _idToExtensionPoint[id] = e;
            return e;
        }
        public IExtensionPoint FindExtensionPoint(string id) {
            return _idToExtensionPoint[id];
        }
        #endregion

        private void ListPlugins(Assembly assembly) {
            _allPlugins.AddRange(EnumeratePlugins(this, assembly, _tracer));
        }

        private void ListPlugins(PluginManifest manifest) {
            foreach (PluginManifest.AssemblyEntry entry in manifest.Entries) {
                Assembly assembly;
                try {
                    assembly = Assembly.LoadFrom(entry.AssemblyName);
                }
                catch (Exception) {
                    _tracer.Trace("PluginManager.Messages.AssemblyLoadError", entry.AssemblyName);
                    continue;
                }

                if (entry.PluginTypeCount == 0) {
                    _allPlugins.AddRange(EnumeratePlugins(this, assembly, _tracer));
                }
                else {
                    _allPlugins.AddRange(EnumeratePlugins(this, assembly, entry, _tracer));
                }
            }
        }

        // Enumerate plugins from the specified assembly.
        private IEnumerable<Plugin> EnumeratePlugins(PluginManager manager, Assembly assembly, ITracer tracer) {
            PluginDeclarationAttribute[] decls = (PluginDeclarationAttribute[])assembly.GetCustomAttributes(typeof(PluginDeclarationAttribute), false);
            foreach (PluginDeclarationAttribute decl in decls) {
                Plugin plugin = new Plugin(manager, assembly, decl.Target);
                yield return plugin;
            }
        }

        // Enumerate plugins according to the specified node.
        private IEnumerable<Plugin> EnumeratePlugins(PluginManager manager, Assembly assembly, PluginManifest.AssemblyEntry assemblyEntry, ITracer tracer) {
            foreach (string pluginTypeName in assemblyEntry.PluginTypes) {
                Type pluginType;
                try {
                    pluginType = assembly.GetType(pluginTypeName);
                }
                catch (Exception) {
                    tracer.Trace("PluginManager.Messages.TypeLoadError", assembly.CodeBase, pluginTypeName);
                    continue;
                }

                if (pluginType == null) {
                    tracer.Trace("PluginManager.Messages.TypeLoadError", assembly.CodeBase, pluginTypeName);
                    continue;
                }

                Plugin plugin = new Plugin(manager, assembly, pluginType);

                yield return plugin;
            }
        }

        private void Load() {
            _idToPlugin = new TypedHashtable<string, Plugin>();
            foreach (Plugin p in _allPlugins) {
                if (p.TryToLoad() == GenericResult.Failed) {
                    _tracer.Trace("PluginManager.Messages.BootWithoutThisPlugin", p.TypeName);
                    continue;
                }

                string id = p.PluginInfo.ID;
                if (_idToPlugin.Contains(id)) {
                    _tracer.Trace("PluginManager.Messages.IDDuplication", p.TypeName, p.PluginInfo.ID);
                    continue;
                }

                _idToPlugin.Add(id, p);
            }
        }

        private void Order() {
            List<Plugin> unordered = new List<Plugin>();
            _orderedPlugins = new List<Plugin>();

            //TODO 簡単のためにコア部分の順番はズルして設定したい　たとえば、idがorg.poderosa.coreで始まるものは優先するなど
            foreach (Plugin p in _allPlugins) {
                if (p.Status == PluginStatus.Loaded) {
                    string d = p.PluginInfo.Dependencies;
                    if (d == null || d.Length == 0) {
                        _orderedPlugins.Add(p); //何にも依存していない奴は先に入れておく
                        continue;
                    }

                    string[] t = d.Split(';');
                    Plugin[] dependencies = new Plugin[t.Length];
                    bool failed = false;
                    for (int i = 0; i < t.Length; i++) {
                        //TODO バージョン指定つきをサポート？
                        Plugin r = _idToPlugin[t[i]];
                        if (r == null || r.Status == PluginStatus.Disabled) {
                            _tracer.Trace("PluginManager.Messages.DependencyNotFound", p.TypeName, t[i]);
                            failed = true;
                            break;
                        }
                        dependencies[i] = r;
                    }

                    if (failed) {
                        p.Disable();
                    }
                    else { //success
                        p.Dependencies = dependencies;
                        unordered.Add(p);
                    }
                }
            }

            //順番の作成
            while (unordered.Count > 0) {
                bool found = false;
                for (int i = 0; i < unordered.Count; i++) {
                    Plugin p = unordered[i];
                    Plugin dep = FindDisabledPlugin(p.Dependencies);
                    if (dep != null) {
                        _tracer.Trace("PluginManager.Messages.DependencyNotFound", p.TypeName, dep.PluginInfo.ID);
                        unordered.RemoveAt(i); //だめなのが入っていることもある。そのときはorderedにはいれずに抜ける
                        found = true;
                        break; //for文を抜ける
                    }

                    if (AllContainedInOrderedPlugins(p.Dependencies)) {
                        unordered.RemoveAt(i);
                        _orderedPlugins.Add(p);
                        found = true;
                        break; //for文を抜ける
                    }
                }

                if (!found) { //一巡して除去できなかったら循環依存がある
                    _tracer.Trace("PluginManager.Messages.DependencyLoopError", FormatIDs(unordered));
                    break; //whileを抜ける
                }
            }
        }

        private void Instantiate() {
            foreach (Plugin p in _orderedPlugins) {
                try {
                    if (p.Instantiate() == GenericResult.Failed) {
                        _tracer.Trace("PluginManager.Messages.PluginInitializeFailed", p.PluginInfo.ID);
                        continue;
                    }
                    _currentInitializingPlugin = p;
                    p.Instance.InitializePlugin(_world);
                    _currentInitializingPlugin = null;
                }
                catch (Exception ex) {
                    _tracer.Trace("PluginManager.Messages.PluginInitializeFailed", p.PluginInfo.ID);
                    _tracer.Trace(ex);
                }
            }
        }

        private bool AllContainedInOrderedPlugins(Plugin[] ps) {
            foreach (Plugin p in ps)
                if (!_orderedPlugins.Contains(p))
                    return false;
            return true;
        }
        private Plugin FindDisabledPlugin(Plugin[] ps) {
            foreach (Plugin p in ps)
                if (p.Status == PluginStatus.Disabled)
                    return p;
            return null;
        }

        private static string FormatIDs(ICollection<Plugin> ps) {
            StringBuilder bld = new StringBuilder();
            foreach (Plugin p in ps) {
                if (bld.Length > 0)
                    bld.Append(';');
                bld.Append(p.PluginInfo.ID);
            }
            return bld.ToString();
        }

        #region IPlugin
        public void InitializePlugin(IPoderosaWorld poderosa) {
        }

        public void TerminatePlugin() {
        }
        #endregion

        #region IAdaptable
        public IAdaptable GetAdapter(Type adapter) {
            return _world.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion

        public IPoderosaWorld PoderosaWorld {
            get {
                return _world;
            }
        }

        #region IPluginInspector
        public IEnumerable<IPluginInfo> Plugins {
            get {
                return new ConvertingEnumerable<IPluginInfo>(_orderedPlugins);
            }
        }

        public IEnumerable<IExtensionPoint> ExtensionPoints {
            get {
                return new ConvertingEnumerable<IExtensionPoint>(_idToExtensionPoint.Values);
            }
        }
        public IPluginInfo GetPluginInfo(IPlugin plugin) {
            foreach (Plugin p in _allPlugins) {
                if (p.Instance == plugin)
                    return p;
            }
            return null;
        }
        #endregion
    }
}
