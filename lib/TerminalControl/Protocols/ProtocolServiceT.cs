/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ProtocolServiceT.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
#if UNITTEST
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.IO;

using Granados;

using Poderosa.Boot;
using Poderosa.Plugins;

using NUnit.Framework;

namespace Poderosa.Protocols {
    //主に接続の確立(IProtocolService内)についてのテスト。
    //TCP接続をつくるところまでについてはInterruptableconnector.cs内にテストケースがある

    [PluginInfo(ID = "org.poderosa.core.protocols.test", Dependencies = "org.poderosa.protocols")]
    internal class ProtocolServiceTestPlugin : PluginBase, IConnectionResultEventHandler, ISSHHostKeyVerifier {
        public static ProtocolServiceTestPlugin Instance;

        private int _successCount;
        private int _failCount;
        private bool _acceptsHostKey;

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            Instance = this;

            IPluginManager pm = poderosa.PluginManager;
            pm.FindExtensionPoint("org.poderosa.protocols.resultEventHandler").RegisterExtension(this);
            pm.FindExtensionPoint(ProtocolsPluginConstants.HOSTKEYCHECKER_EXTENSION).RegisterExtension(this);

            _acceptsHostKey = true;
        }

        public bool AcceptsHostKey {
            get {
                return _acceptsHostKey;
            }
            set {
                _acceptsHostKey = value;
            }
        }

        //IConnectionResultEventHandler
        public void OnSucceeded(ITerminalParameter param) {
            _successCount++;
            Debug.Assert(param != null);
        }

        public void OnFailed(ITerminalParameter param, string msg) {
            _failCount++;
            Debug.Assert(param != null);
            Debug.WriteLine(msg);
        }

        //ISSHHostKeyVerifier
        public bool Verify(ISSHLoginParameter param, SSHConnectionInfo info) {
            return _acceptsHostKey;
        }

        public void Reset() {
            _successCount = 0;
            _failCount = 0;
        }

        public void AssertSuccess() {
            Assert.AreEqual(1, _successCount);
            Assert.AreEqual(0, _failCount);
        }
        public void AssertFail() {
            Assert.AreEqual(0, _successCount);
            Assert.AreEqual(1, _failCount);
        }
    }


    [TestFixture]
    public class ProtocolServiceTests {
        private static IPoderosaApplication _poderosaApplication;
        private static IPoderosaWorld _poderosaWorld;
        private static IProtocolService _protocolService;

        private static string CreatePluginManifest() {
            return String.Format("Root {{\r\n  {0} {{\r\n  plugin=Poderosa.Preferences.PreferencePlugin\r\n  plugin=Poderosa.Protocols.ProtocolsPlugin\r\n  plugin=Poderosa.Protocols.ProtocolServiceTestPlugin\r\n}}\r\n}}", "Poderosa.Monolithic.dll");
        }
        internal class ResultCallback : IInterruptableConnectorClient {
            private int _successCount;
            private int _failCount;
            private string _message;
            private ITerminalConnection _connection;
            private ManualResetEvent _event;

            public ResultCallback() {
                _event = new ManualResetEvent(false);
            }

            public void SuccessfullyExit(ITerminalConnection result) {
                _successCount++;
                _connection = result;
                _event.Set();
            }

            public void ConnectionFailed(string message) {
                _failCount++;
                _message = message;
                _event.Set();
            }

            //成功していることを確認し、Closeする
            public void AssertSuccess() {
                Assert.IsTrue(_event.WaitOne(5000, false)); //通知は受けないとだめ
                _event.Close();
                Assert.AreEqual(1, _successCount);
                Assert.AreEqual(0, _failCount);
                Assert.IsNotNull(_connection);

                _connection.Close();
            }
            //失敗していることの確認
            public void AssertFail() {
                Assert.IsTrue(_event.WaitOne(5000, false)); //通知は受けないとだめ
                _event.Close();
                Assert.AreEqual(0, _successCount);
                Assert.AreEqual(1, _failCount);
                Assert.IsNull(_connection);
                Assert.IsNotNull(_message);
                Debug.WriteLine(_message);
            }
        }


        [TestFixtureSetUp]
        public void Init() {
            try {
                _poderosaApplication = PoderosaStartup.CreatePoderosaApplication(CreatePluginManifest(), new StructuredText("Poderosa"));
                _poderosaWorld = _poderosaApplication.Start();
                _protocolService = ProtocolsPlugin.Instance;
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
        public void T00_TelnetSuccess() {
            ProtocolServiceTestPlugin.Instance.Reset();

            ITCPParameter tcp = _protocolService.CreateDefaultTelnetParameter();
            tcp.Destination = GetTelnetConnectableHost();
            Assert.AreEqual(23, tcp.Port);

            ResultCallback client = new ResultCallback();
            IInterruptable t = _protocolService.AsyncTelnetConnect(client, tcp);
            client.AssertSuccess();

            ProtocolServiceTestPlugin.Instance.AssertSuccess();
        }
        [Test]
        public void T01_SSH2Success() {
            SSHSuccess(SSHProtocol.SSH2);
        }
        [Test]
        public void T02_SSH2BadPassword() {
            SSHBadPassword(SSHProtocol.SSH2);
        }
        [Test]
        public void T03_SSH1Success() {
            SSHSuccess(SSHProtocol.SSH1);
        }
        [Test]
        public void T04_SSH1BadPassword() {
            SSHBadPassword(SSHProtocol.SSH1);
        }

        private void SSHSuccess(SSHProtocol sshprotocol) {
            ProtocolServiceTestPlugin.Instance.Reset();

            ISSHLoginParameter ssh = _protocolService.CreateDefaultSSHParameter();
            ssh.Method = sshprotocol;
            ssh.Account = UnitTestUtil.GetUnitTestConfig("protocols.ssh_account");
            ssh.PasswordOrPassphrase = UnitTestUtil.GetUnitTestConfig("protocols.ssh_password");
            ITCPParameter tcp = (ITCPParameter)ssh.GetAdapter(typeof(ITCPParameter));
            tcp.Destination = GetSSHConnectableHost();
            Assert.AreEqual(22, tcp.Port);

            ResultCallback client = new ResultCallback();
            IInterruptable t = _protocolService.AsyncSSHConnect(client, ssh);
            client.AssertSuccess();

            ProtocolServiceTestPlugin.Instance.AssertSuccess();
        }
        private void SSHBadPassword(SSHProtocol sshprotocol) {
            ProtocolServiceTestPlugin.Instance.Reset();

            ISSHLoginParameter ssh = _protocolService.CreateDefaultSSHParameter();
            ssh.Method = sshprotocol;
            ssh.Account = UnitTestUtil.GetUnitTestConfig("protocols.ssh_account");
            ssh.PasswordOrPassphrase = UnitTestUtil.GetUnitTestConfig("protocols.ssh_wrongpassword");
            ITCPParameter tcp = (ITCPParameter)ssh.GetAdapter(typeof(ITCPParameter));
            tcp.Destination = GetSSHConnectableHost();
            Assert.AreEqual(22, tcp.Port);

            ResultCallback client = new ResultCallback();
            IInterruptable t = _protocolService.AsyncSSHConnect(client, ssh);
            client.AssertFail();

            ProtocolServiceTestPlugin.Instance.AssertFail();
            Socket s = ((InterruptableConnector)t).RawSocket;
            Assert.IsFalse(s.Connected);
        }


        [Test]
        public void T05_DenyHostKey() {
            ProtocolServiceTestPlugin.Instance.Reset();
            ProtocolServiceTestPlugin.Instance.AcceptsHostKey = false;

            ISSHLoginParameter ssh = _protocolService.CreateDefaultSSHParameter();
            ssh.Method = SSHProtocol.SSH2;
            ssh.Account = UnitTestUtil.GetUnitTestConfig("protocols.ssh_account");
            ssh.PasswordOrPassphrase = UnitTestUtil.GetUnitTestConfig("protocols.ssh_password");
            ITCPParameter tcp = (ITCPParameter)ssh.GetAdapter(typeof(ITCPParameter));
            tcp.Destination = GetSSHConnectableHost();
            Assert.AreEqual(22, tcp.Port);

            ResultCallback client = new ResultCallback();
            IInterruptable t = _protocolService.AsyncSSHConnect(client, ssh);
            client.AssertFail();

            ProtocolServiceTestPlugin.Instance.AssertFail();
            ProtocolServiceTestPlugin.Instance.AcceptsHostKey = true;
        }

        //TODO Cygwinとシリアルのテスト

        [Test]
        public void T06_FormBaseSuccess() {
            ProtocolServiceTestPlugin.Instance.Reset();

            ITCPParameter tcp = _protocolService.CreateDefaultTelnetParameter();
            tcp.Destination = GetTelnetConnectableHost();
            Assert.AreEqual(23, tcp.Port);

            ISynchronizedConnector sc = _protocolService.CreateFormBasedSynchronozedConnector(null);
            IInterruptable t = _protocolService.AsyncTelnetConnect(sc.InterruptableConnectorClient, tcp);
            ITerminalConnection con = sc.WaitConnection(t, 5000);

            Assert.IsNotNull(con);
            Assert.IsFalse(con.IsClosed);

            ProtocolServiceTestPlugin.Instance.AssertSuccess();
        }
        [Test]
        public void T07_FormBaseFail() {
            ProtocolServiceTestPlugin.Instance.Reset();

            ISSHLoginParameter ssh = _protocolService.CreateDefaultSSHParameter();
            ssh.Method = SSHProtocol.SSH2;
            ssh.Account = UnitTestUtil.GetUnitTestConfig("protocols.ssh_account");
            ssh.PasswordOrPassphrase = UnitTestUtil.GetUnitTestConfig("protocols.ssh_wrongpassword");
            ITCPParameter tcp = (ITCPParameter)ssh.GetAdapter(typeof(ITCPParameter));
            tcp.Destination = GetSSHConnectableHost();
            Assert.AreEqual(22, tcp.Port);

            ISynchronizedConnector sc = _protocolService.CreateFormBasedSynchronozedConnector(null);
            IInterruptable t = _protocolService.AsyncSSHConnect(sc.InterruptableConnectorClient, ssh);
            ITerminalConnection con = sc.WaitConnection(t, 5000);

            ProtocolServiceTestPlugin.Instance.AssertFail();

            Socket s = ((InterruptableConnector)t).RawSocket;
            Assert.IsFalse(s.Connected);
            Assert.IsNull(con);
        }

        /*
         * SSHではないポートに接続してアウトになる実験をしたいが、適切なのがみつからない
        [Test]
        public void T03_SSHBadPort() {
            ProtocolServiceTestPlugin.Instance.Reset();

            ISSHLoginParameter ssh = _protocolService.CreateDefaultSSHParameter();
            ssh.Method = SSHProtocol.SSH2;
            ssh.Account = UnitTestUtil.GetUnitTestConfig("protocols.ssh_account");
            ssh.PasswordOrPassphrase = UnitTestUtil.GetUnitTestConfig("protocols.ssh_wrongpassword");
            ITCPParameter tcp = (ITCPParameter)ssh.GetAdapter(typeof(ITCPParameter));
            tcp.Destination = "www.google.com"; //Googleのポート80にSSH接続を試みる。当然失敗すべきだが
            tcp.Port = 80;

            ResultCallback client = new ResultCallback();
            IInterruptable t = _protocolService.AsyncSSHConnect(client, ssh);
            client.AssertFail();

            ProtocolServiceTestPlugin.Instance.AssertFail();
        }
         */

        private static string GetTelnetConnectableHost() {
            return UnitTestUtil.GetUnitTestConfig("protocols.telnet_connectable");
        }
        private static string GetSSHConnectableHost() {
            return UnitTestUtil.GetUnitTestConfig("protocols.ssh_connectable");
        }
    }
}
#endif
