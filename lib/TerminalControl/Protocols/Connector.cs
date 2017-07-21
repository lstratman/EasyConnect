/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Connector.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

using Granados;
using Poderosa.Plugins;
using Poderosa.Util;
using Poderosa.Forms;
using Granados.SSH;

namespace Poderosa.Protocols {

    internal class SSHConnector : InterruptableConnector {

        private ISSHLoginParameter _destination;
        private HostKeyVerifierBridge _keycheck;
        private TerminalConnection _result;

        public SSHConnector(ISSHLoginParameter destination, HostKeyVerifierBridge keycheck) {
            _destination = destination;
            _keycheck = keycheck;
        }

        protected override void Negotiate() {
            ITerminalParameter term = (ITerminalParameter)_destination.GetAdapter(typeof(ITerminalParameter));
            ITCPParameter tcp = (ITCPParameter)_destination.GetAdapter(typeof(ITCPParameter));

            SSHTerminalConnection terminalConnection = new SSHTerminalConnection(_destination);

            SSHConnectionParameter con =
                new SSHConnectionParameter(
                    tcp.Destination,
                    tcp.Port,
                    _destination.Method,
                    _destination.AuthenticationType,
                    _destination.Account,
                    _destination.PasswordOrPassphrase);
#if DEBUG
            // con.EventTracer = new SSHDebugTracer();
#endif
            con.Protocol = _destination.Method;
            con.CheckMACError = PEnv.Options.SSHCheckMAC;
            con.UserName = _destination.Account;
            con.Password = _destination.PasswordOrPassphrase;
            con.AuthenticationType = _destination.AuthenticationType;
            con.IdentityFile = _destination.IdentityFileName;
            con.TerminalWidth = term.InitialWidth;
            con.TerminalHeight = term.InitialHeight;
            con.TerminalName = term.TerminalType;
            con.WindowSize = PEnv.Options.SSHWindowSize;
            con.PreferableCipherAlgorithms =
                LocalSSHUtil.AppendMissingCipherAlgorithm(
                    LocalSSHUtil.ParseCipherAlgorithm(PEnv.Options.CipherAlgorithmOrder));
            con.PreferableHostKeyAlgorithms =
                LocalSSHUtil.AppendMissingPublicKeyAlgorithm(
                    LocalSSHUtil.ParsePublicKeyAlgorithm(PEnv.Options.HostKeyAlgorithmOrder));
            con.AgentForwardingAuthKeyProvider = _destination.AgentForwardingAuthKeyProvider;
            con.X11ForwardingParams = _destination.X11Forwarding;
            if (_keycheck != null) {
                con.VerifySSHHostKey = (info) => {
                    return _keycheck.Vefiry(info);
                };
            }
            con.KeyboardInteractiveAuthenticationHandlerCreator =
                sshconn => terminalConnection.GetKeyboardInteractiveAuthenticationHandler();

            ISSHProtocolEventLogger protocolEventLogger;
            if (ProtocolsPlugin.Instance.ProtocolOptions.LogSSHEvents) {
                protocolEventLogger = new SSHEventTracer(tcp.Destination);
            }
            else {
                protocolEventLogger = null;
            }

            var connection = SSHConnection.Connect(_socket, con,
                                sshconn => terminalConnection.ConnectionEventReceiver,
                                sshconn => protocolEventLogger);
            if (PEnv.Options.RetainsPassphrase && _destination.AuthenticationType != AuthenticationType.KeyboardInteractive) {
                ProtocolsPlugin.Instance.PassphraseCache.Add(tcp.Destination, _destination.Account, _destination.PasswordOrPassphrase); //接続成功時のみセット
            }
            //_destination.PasswordOrPassphrase = ""; 接続の複製のためにここで消さずに残しておく
            terminalConnection.AttachTransmissionSide(connection, connection.AuthenticationStatus);
            _result = terminalConnection;
        }
        internal override TerminalConnection Result {
            get {
                return _result;
            }
        }
    }

    internal class TelnetConnector : InterruptableConnector {
        private ITCPParameter _destination;
        private TelnetTerminalConnection _result;

        public TelnetConnector(ITCPParameter destination) {
            _destination = destination;
        }

        protected override void Negotiate() {
            ITerminalParameter term = (ITerminalParameter)_destination.GetAdapter(typeof(ITerminalParameter));
            TelnetNegotiator neg = new TelnetNegotiator(term.TerminalType, term.InitialWidth, term.InitialHeight);
            TelnetTerminalConnection r = new TelnetTerminalConnection(_destination, neg, new PlainPoderosaSocket(_socket));
            //BACK-BURNER r.UsingSocks = _socks!=null;
            _result = r;
        }

        internal override TerminalConnection Result {
            get {
                return _result;
            }
        }
    }

    internal class SilentClient : ISynchronizedConnector, IInterruptableConnectorClient {
        private IPoderosaForm _form;
        private AutoResetEvent _event;
        private ITerminalConnection _result;
        private string _errorMessage;
        private bool _timeout;

        public SilentClient(IPoderosaForm form) {
            _event = new AutoResetEvent(false);
            _form = form;
        }

        public void SuccessfullyExit(ITerminalConnection result) {
            if (_timeout)
                return;
            _result = result;
            //_result.SetServerInfo(((TCPTerminalParam)_result.Param).Host, swt.IPAddress);
            _event.Set();
        }
        public void ConnectionFailed(string message) {
            Debug.Assert(message != null);
            _errorMessage = message;
            if (_timeout)
                return;
            _event.Set();
        }

        public IInterruptableConnectorClient InterruptableConnectorClient {
            get {
                return this;
            }
        }

        public ITerminalConnection WaitConnection(IInterruptable intr, int timeout) {
            //ちょっと苦しい判定
            if (!(intr is InterruptableConnector) && !(intr is LocalShellUtil.Connector))
                throw new ArgumentException("IInterruptable object is not correct");

            if (!_event.WaitOne(timeout, true)) {
                _timeout = true; //TODO 接続を中止すべきか
                _errorMessage = PEnv.Strings.GetString("Message.ConnectionTimedOut");
            }
            _event.Close();

            if (_result == null) {
                if (_form != null)
                    _form.Warning(_errorMessage);
                return null;
            }
            else
                return _result;
        }
    }

    internal class SSHEventTracer : ISSHProtocolEventLogger {
        private IPoderosaLog _log;
        private PoderosaLogCategoryImpl _category;

        public SSHEventTracer(string destination) {
            _log = ((IPoderosaApplication)ProtocolsPlugin.Instance.PoderosaWorld.GetAdapter(typeof(IPoderosaApplication))).PoderosaLog;
            _category = new PoderosaLogCategoryImpl(String.Format("SSH:{0}", destination));
        }

        public void OnSend(string messageType, string details) {
            _log.AddItem(_category, String.Format("Send    : {0} {1}", messageType, details));
        }

        public void OnReceived(string messageType, string details) {
            _log.AddItem(_category, String.Format("Receive : {0} {1}", messageType, details));
        }

        public void OnTrace(string details) {
            _log.AddItem(_category, String.Format("Trace   : {0}", details));
        }
    }
}
