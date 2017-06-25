/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ProtocolsPlugin.cs,v 1.3 2011/12/23 07:18:44 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Poderosa.Plugins;
using Poderosa.Preferences;
using Poderosa.Forms;
using Poderosa.Util.Collections;

[assembly: PluginDeclaration(typeof(Poderosa.Protocols.ProtocolsPlugin))]

namespace Poderosa.Protocols {
    [PluginInfo(ID = ProtocolsPlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.core.preferences;org.poderosa.core.serializing")]
    internal class ProtocolsPlugin : PluginBase, IProtocolService, IProtocolTestService {
        public const string PLUGIN_ID = "org.poderosa.protocols";

        private static ProtocolsPlugin _instance;
        private IExtensionPoint _connectionResultEventHandler;
        private ProtocolOptionsSupplier _protocolOptionsSupplier;
        private PassphraseCache _passphraseCache;
        private IPoderosaLog _poderosaLog;
        private PoderosaLogCategoryImpl _netCategory;

        public static ProtocolsPlugin Instance {
            get {
                return _instance;
            }
        }

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;

            _protocolOptionsSupplier = new ProtocolOptionsSupplier();
            _passphraseCache = new PassphraseCache();
            _poderosaLog = ((IPoderosaApplication)poderosa.GetAdapter(typeof(IPoderosaApplication))).PoderosaLog;
            _netCategory = new PoderosaLogCategoryImpl("Network");

            IPluginManager pm = poderosa.PluginManager;
            RegisterTerminalParameterSerializers(pm.FindExtensionPoint("org.poderosa.core.serializeElement"));

            _connectionResultEventHandler = pm.CreateExtensionPoint(ProtocolsPluginConstants.RESULTEVENTHANDLER_EXTENSION, typeof(IConnectionResultEventHandler), this);
            pm.CreateExtensionPoint(ProtocolsPluginConstants.HOSTKEYCHECKER_EXTENSION, typeof(ISSHHostKeyVerifier2), ProtocolsPlugin.Instance);
            PEnv.Init((ICoreServices)poderosa.GetAdapter(typeof(ICoreServices)));
        }

        public ICygwinParameter CreateDefaultCygwinParameter() {
            return new LocalShellParameter();
        }

        public ITCPParameter CreateDefaultTelnetParameter() {
            return new TelnetParameter();
        }

        public ISSHLoginParameter CreateDefaultSSHParameter() {
            return new SSHLoginParameter();
        }

        public IInterruptable AsyncTelnetConnect(IInterruptableConnectorClient result_client, ITCPParameter destination) {
            InterruptableConnector swt = new TelnetConnector(destination);
            swt.AsyncConnect(result_client, destination);
            return swt;
        }
        public IInterruptable AsyncSSHConnect(IInterruptableConnectorClient result_client, ISSHLoginParameter destination) {
            InterruptableConnector swt = new SSHConnector(destination, new HostKeyVerifierBridge());
            ITCPParameter tcp = (ITCPParameter)destination.GetAdapter(typeof(ITCPParameter));
            swt.AsyncConnect(result_client, tcp);
            return swt;
        }
        public IInterruptable AsyncCygwinConnect(IInterruptableConnectorClient result_client, ICygwinParameter destination) {
            return LocalShellUtil.AsyncPrepareSocket(result_client, destination);
        }

        public ISynchronizedConnector CreateFormBasedSynchronozedConnector(IPoderosaForm form) {
            return new SilentClient(form);
        }

        public IProtocolOptions ProtocolOptions {
            get {
                return _protocolOptionsSupplier.OriginalOptions;
            }
        }
        public IPassphraseCache PassphraseCache {
            get {
                return _passphraseCache;
            }
        }

        internal IExtensionPoint ConnectionResultEventHandler {
            get {
                return _connectionResultEventHandler;
            }
        }

        public ProtocolOptionsSupplier ProtocolOptionsSupplier {
            get {
                return _protocolOptionsSupplier;
            }
        }

        public void NetLog(string text) {
            _poderosaLog.AddItem(_netCategory, text);
        }

        private void RegisterTerminalParameterSerializers(IExtensionPoint extp) {
            extp.RegisterExtension(new TelnetParameterSerializer());
            extp.RegisterExtension(new SSHParameterSerializer());
            extp.RegisterExtension(new LocalShellParameterSerializer());
        }

        //IProtocolTestService
        public ITerminalConnection CreateLoopbackConnection() {
            return new RawTerminalConnection(new LoopbackSocket(), new EmptyTerminalParameter());
        }
    }

    //Option, Stringへのアクセスポイント, 旧GEnvから必要なところを抜き出す
    internal class PEnv {
        private static StringResource _strings;
        private static IWindowManager _windowManager;

        public static void Init(ICoreServices cs) {
            cs.PreferenceExtensionPoint.RegisterExtension(ProtocolsPlugin.Instance.ProtocolOptionsSupplier);
            _strings = new StringResource("Poderosa.Protocols.strings", typeof(PEnv).Assembly);
            ProtocolsPlugin.Instance.PoderosaWorld.Culture.AddChangeListener(_strings);
            _windowManager = cs.WindowManager;
        }

        public static IProtocolOptions Options {
            get {
                return ProtocolsPlugin.Instance.ProtocolOptionsSupplier.OriginalOptions;
            }
        }
        public static StringResource Strings {
            get {
                return _strings;
            }
        }
        public static IPoderosaMainWindow ActiveForm {
            get {
                return _windowManager.ActiveWindow;
            }
        }
    }

    internal static class ProtocolUtil {
        //内部ではITCPParameterかICygwinParameterのどっちかしか呼ばれないはず
        public static void FireConnectionSucceeded(IAdaptable param) {
            ITerminalParameter t = (ITerminalParameter)param.GetAdapter(typeof(ITerminalParameter));
            Debug.Assert(t != null);
            if (ProtocolsPlugin.Instance != null) {
                foreach (IConnectionResultEventHandler h in ProtocolsPlugin.Instance.ConnectionResultEventHandler.GetExtensions()) {
                    h.OnSucceeded(t);
                }
            }
        }
        public static void FireConnectionFailure(IAdaptable param, string msg) {
            ITerminalParameter t = (ITerminalParameter)param.GetAdapter(typeof(ITerminalParameter));
            Debug.Assert(t != null);
            if (ProtocolsPlugin.Instance != null) {
                foreach (IConnectionResultEventHandler h in ProtocolsPlugin.Instance.ConnectionResultEventHandler.GetExtensions()) {
                    h.OnFailed(t, msg);
                }
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public static class ProtocolsPluginConstants {
        public const string HOSTKEYCHECKER_EXTENSION = "org.poderosa.protocols.sshHostKeyChecker";
        public const string RESULTEVENTHANDLER_EXTENSION = "org.poderosa.protocols.resultEventHandler";
    }


    //メモリにのみ保持
    internal class PassphraseCache : IPassphraseCache {
        private TypedHashtable<string, string> _data;

        public PassphraseCache() {
            _data = new TypedHashtable<string, string>();
        }
        public void Add(string host, string account, string passphrase) {
            _data.Add(String.Format("{0}@{1}", account, host), passphrase);
        }

        public string GetOrEmpty(string host, string account) {
            string t = _data[String.Format("{0}@{1}", account, host)];
            return t == null ? String.Empty : t;
        }
    }
}
