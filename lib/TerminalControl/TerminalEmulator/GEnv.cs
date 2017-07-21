/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: GEnv.cs,v 1.3 2012/03/18 10:57:33 kzmi Exp $
 */
using System;
using System.Resources;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;

using Poderosa.Terminal;
using Poderosa.Forms;
using Poderosa.ConnectionParam;
using Poderosa.Util;

using Poderosa.Document;
using Poderosa.View;

namespace Poderosa {
    internal class GEnv {
        //内部で管理
        private static Win32.SystemMetrics _systemMetrics;
        private static RenderProfile _defaultRenderProfile;
        private static StringResource _stringResource;

#if false
        //BACK-BURNER 
        public static void Init(IPoderosaContainer container) {
            GConst.Init();
            _frame = container;
            _textSelection = new TextSelection();
            _connections = new Connections();
        }
        public static void Terminate() {
            CygwinUtil.Terminate();
        }
#endif
        public static void Init() {
            ReloadStringResource();
        }

        public static StringResource Strings {
            get {
                if (_stringResource == null) {
                    ReloadStringResource();
                }
                return _stringResource;
            }
        }
        private static void ReloadStringResource() {
            _stringResource = new StringResource("Poderosa.TerminalEmulator.strings", typeof(GEnv).Assembly, true);
            TerminalEmulatorPlugin.Instance.PoderosaWorld.Culture.AddChangeListener(_stringResource);
        }
        internal static Win32.SystemMetrics SystemMetrics {
            get {
                if (_systemMetrics == null)
                    _systemMetrics = new Win32.SystemMetrics();
                return _systemMetrics;
            }
        }
        public static ITerminalEmulatorOptions Options {
            get {
                return TerminalEmulatorPlugin.Instance.OptionSupplier.OriginalOptions;
            }
        }

        public static RenderProfile DefaultRenderProfile {
            get {
                if (_defaultRenderProfile == null)
                    _defaultRenderProfile = GEnv.Options.CreateRenderProfile();
                return _defaultRenderProfile;
            }
            set {
                _defaultRenderProfile = value;
            }
        }

#if false
        //BACK-BURNER
        public static GlobalCommandTarget GlobalCommandTarget {
            get {
                if (_commandTarget == null)
                    _commandTarget = new GlobalCommandTarget();
                return _commandTarget;
            }
            set {
                _commandTarget = value;
            }
        }
        public static InterThreadUIService InterThreadUIService {
            get {
                if (_interThreadUIService == null)
                    _interThreadUIService = new InterThreadUIService();
                return _interThreadUIService;
            }
            set {
                _interThreadUIService = value;
            }
        }
        public static ISSHKnownHosts SSHKnownHosts {
            get {
                if (_sshKnownHosts == null)
                    _sshKnownHosts = new DefaultSSHKnownHosts();
                return _sshKnownHosts;
            }
            set {
                _sshKnownHosts = value;
            }
        }


        // no args -> CommandTarget for active connection
        internal static ConnectionCommandTarget GetConnectionCommandTarget() {
            TerminalConnection con = GEnv.Connections.ActiveConnection;
            return con == null ? null : new ConnectionCommandTarget(con);
        }
        internal static ConnectionCommandTarget GetConnectionCommandTarget(TerminalConnection con) {
            return new ConnectionCommandTarget(con);
        }
        internal static ConnectionCommandTarget GetConnectionCommandTarget(ConnectionTag tag) {
            return new ConnectionCommandTarget(tag.Connection);
        }
#endif
    }

    internal class GConst {

        //WMG_MAINTHREADTASKのWParam
        public const int WPG_ADJUSTIMECOMPOSITION = 1;
        public const int WPG_TOGGLESELECTIONMODE = 2;
        public const int WPG_DATA_ARRIVED = 3;

        public const string NSURI = "http://www.poderosa.org/";
    }


}
