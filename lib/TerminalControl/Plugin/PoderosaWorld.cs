/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PoderosaWorld.cs,v 1.3 2011/10/27 23:21:56 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

using Poderosa.Plugins;

namespace Poderosa.Boot {
    internal class InternalPoderosaWorld : IPoderosaWorld, IPoderosaApplication, IStartupContextSupplier {

        private static InternalPoderosaWorld _instance;

        private AdapterManager _adapterManager;
        private StringResource _stringResource;
        private PoderosaCulture _poderosaCulture;
        private PoderosaLog _poderosaLog;
        private PluginManager _pluginManager;
        private IExtensionPoint _rootExtension;
        private PoderosaStartupContext _startupContext;

        public InternalPoderosaWorld(PoderosaStartupContext context) {
            _instance = this;
            _startupContext = context;
            _poderosaCulture = new PoderosaCulture();
            _poderosaLog = new PoderosaLog(this);
            _adapterManager = new AdapterManager();
            _stringResource = new StringResource("Poderosa.Plugin.strings", typeof(InternalPoderosaWorld).Assembly);
            _poderosaCulture.AddChangeListener(_stringResource);
            _pluginManager = new PluginManager(this);
            //ルート
            _rootExtension = _pluginManager.CreateExtensionPoint(ExtensionPoint.ROOT, typeof(IRootExtension), null);
        }

        #region IPoderosaWorld
        public IAdapterManager AdapterManager {
            get {
                return _adapterManager;
            }
        }
        public IPluginManager PluginManager {
            get {
                return _pluginManager;
            }
        }
        public IPoderosaCulture Culture {
            get {
                return _poderosaCulture;
            }
        }
        #endregion

        #region IStartupContextSupplier
        public StructuredText Preferences {
            get {
                return _startupContext.Preferences;
            }
        }
        public string PreferenceFileName {
            get {
                return _startupContext.PreferenceFileName;
            }
        }
        #endregion

        #region IAdaptable
        public IAdaptable GetAdapter(Type type) {
            //デフォルト実装でOK
            return _adapterManager.GetAdapter(this, type);
        }
        #endregion
        /*
        public StringResource StringResource {
            get {
                return _stringResource;
            }
        }
        */
        #region IPoderosaApplication
        /*public IPoderosaWorld PoderosaWorld {
            get {
                return this;
            }
        }*/
        public IPoderosaWorld Start() {
            DefaultTracer tracer = new DefaultTracer(_stringResource);
            _startupContext.Tracer = tracer;

            //Step1 プラグインの構成と初期化
            _pluginManager.InitializePlugins(_startupContext);

            //エラーレポート
            if (!tracer.Document.IsEmpty)
                ReportBootError(tracer.Document);

            //Step2 ルートエクステンションの実行
            RunRootExtensions();

            return this;
        }
        public void Shutdown() {
            _pluginManager.Shutdown();
        }
        public string HomeDirectory {
            get {
                return _startupContext.HomeDirectory;
            }
        }
        public string ProfileHomeDirectory {
            get {
                return _startupContext.ProfileHomeDirectory;
            }
        }
        public IPoderosaLog PoderosaLog {
            get {
                return _poderosaLog;
            }
        }
        public string[] CommandLineArgs {
            get {
                return _startupContext.CommandLineArgs;
            }
        }
        public string InitialOpenFile {
            get {
                return _startupContext.InitialOpenFile;
            }
        }
        #endregion

        private void RunRootExtensions() {
            IRootExtension[] rootextensions = (IRootExtension[])_rootExtension.GetExtensions();
            IGUIMessageLoop message_loop = null;
            foreach (IRootExtension extension in rootextensions) {
                if (extension is IGUIMessageLoop) {
                    if (message_loop != null)
                        _startupContext.Tracer.Trace("PoderosaWorld.Messages.DuplicatedMessageLoopExtension", message_loop.GetType().Name, extension.GetType().Name);
                    else
                        message_loop = (IGUIMessageLoop)extension;
                }
                extension.InitializeExtension();
            }

            //メッセージループ付きのやつが存在すればそれを実行。
            if (message_loop != null)
                message_loop.RunExtension();
        }


        //ショートカット
        public static StringResource Strings {
            get {
                return _instance._stringResource;
            }
        }

        private static void ReportBootError(TraceDocument document) {
            StringBuilder bld = new StringBuilder();
            foreach (TraceDocItem item in document) {
                if (bld.Length > 0)
                    bld.Append("\n");
                bld.Append(item.Data);
            }

            //WinFormsに頼らないでもいければベストだが
            System.Windows.Forms.MessageBox.Show(bld.ToString(), "Poderosa", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }
    }

    internal class PoderosaCulture : IPoderosaCulture {
        private CultureInfo _initialCulture;
        private CultureInfo _currentCulture;
        private List<ICultureChangeListener> _listeners;

        public PoderosaCulture() {
            _initialCulture = CultureInfo.CurrentUICulture;
            _currentCulture = _initialCulture;
            _listeners = new List<ICultureChangeListener>();
        }

        public CultureInfo InitialCulture {
            get {
                return _initialCulture;
            }
        }

        public CultureInfo CurrentCulture {
            get {
                return _currentCulture;
            }
        }

        public bool IsJapaneseOS {
            get {
                return _initialCulture.Name.StartsWith("ja");
            }
        }

        public bool IsSimplifiedChineseOS {
            get {
                return _initialCulture.Name.StartsWith("zh") && !IsTraditionalChineseOS;
            }
        }
        public bool IsTraditionalChineseOS {
            get {
                return _initialCulture.Name.StartsWith("zh") && (
                           _initialCulture.Name.EndsWith("TW")
                        || _initialCulture.Name.EndsWith("HK")
                        || _initialCulture.Name.EndsWith("MO")
                        || _initialCulture.Name.EndsWith("CHT"));
            }
        }
        public bool IsKoreanOS {
            get {
                return _initialCulture.Name.StartsWith("ko");
            }
        }

        public void SetCulture(CultureInfo culture) {
            _currentCulture = culture;
            //普通はプラグインのロード順で登録されるはず。多くのケースでは下流プラグインのEXTPを呼び出してテキストを取得しがち。なので、ここのループは逆順のほうがトラブル少なそう
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i].OnCultureChanged(culture);
        }

        public void AddChangeListener(ICultureChangeListener listener) {
            Debug.Assert(listener != null);
            _listeners.Add(listener);
        }

        public void RemoveChangeListener(ICultureChangeListener listener) {
            Debug.Assert(listener != null);
            _listeners.Remove(listener);
        }
    }
}
