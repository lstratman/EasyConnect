/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: InterruptableConnectorT.cs,v 1.1 2010/11/19 15:41:03 kzmi Exp $
 */
#if UNITTEST
using System;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using NUnit.Framework;

namespace Poderosa.Protocols {

    [TestFixture]
    public class InterruptableConnectorTests {

        private class TestConnector : InterruptableConnector {
            private bool _errorThrow;
            private bool _wait1sec;

            public void SetError(bool value) {
                _errorThrow = value;
            }
            public void SetWait1Sec(bool value) {
                _wait1sec = value;
            }


            protected override void Negotiate() {
                if(_wait1sec) Thread.Sleep(1000);
                if(_errorThrow) throw new Exception("TEST ERROR");
            }
            internal override TerminalConnection Result {
                get {
                    return null; //テストではnullでよい
                }
            }
        }

        private class TestClient : IInterruptableConnectorClient {
            private bool _succeeded;
            private bool _notified;
            private string _message;
            private ManualResetEvent _event;

            public TestClient() {
                _event = new ManualResetEvent(false);
            }

            public void SuccessfullyExit(ITerminalConnection result) {
                _succeeded = true;
                _message = null;
                _notified = true;
                _event.Set();
            }

            public void ConnectionFailed(string message) {
                _succeeded = false;
                _message = message;
                _notified = true;
                _event.Set();
            }

            public void WaitAndClose() {
                _event.WaitOne();
                _event.Close();
            }

            public bool Succeeded {
                get {
                    return _succeeded;
                }
            }
            public bool Notified {
                get {
                    return _notified;
                }
            }
        }

        private static string GetConnectableDNSName() {
            return UnitTestUtil.GetUnitTestConfig("protocols.connectable_dnsname");
        }
        private static string GetUnknownDNSName() {
            return UnitTestUtil.GetUnitTestConfig("protocols.unknown_dnsname");
        }
        private static int GetConnectablePort() {
            return Int32.Parse(UnitTestUtil.GetUnitTestConfig("protocols.connectable_port"));
        }
        private static int GetClosedPort() {
            return Int32.Parse(UnitTestUtil.GetUnitTestConfig("protocols.closed_port"));
        }
        private static string GetConnectableIP() {
            return UnitTestUtil.GetUnitTestConfig("protocols.connectable_ip");
        }
        private static string GetUnreachableIP() {
            return UnitTestUtil.GetUnitTestConfig("protocols.unreachable_ip");
        }

        [Test]
        public void T00_Successful1() {
            TestConnector c = new TestConnector();
            TestClient t= new TestClient();
            c.AsyncConnect(t, new TelnetParameter(GetConnectableDNSName(), GetConnectablePort()));
            t.WaitAndClose();

            Assert.IsTrue(c.Succeeded);

        }

        [Test]
        public void T01_Successful2() {
            TestConnector c = new TestConnector();
            TestClient t= new TestClient();
            c.AsyncConnect(t, new TelnetParameter(GetConnectableIP(), GetConnectablePort()));
            t.WaitAndClose();

            Assert.IsTrue(c.Succeeded);
        }

        [Test]
        public void T03_NotConnectable1() {
            TestConnector c = new TestConnector();
            TestClient t= new TestClient();
            c.AsyncConnect(t, new TelnetParameter(GetUnreachableIP(), GetConnectablePort())); //存在しないホスト
            t.WaitAndClose();

            Assert.IsFalse(c.Succeeded);
            Debug.WriteLine(String.Format("NotConnectable1 [{0}]", c.ErrorMessage));
        }

        [Test]
        public void T04_NotConnectable2() {
            TestConnector c = new TestConnector();
            TestClient t= new TestClient();
            c.AsyncConnect(t, new TelnetParameter(GetUnknownDNSName(), GetConnectablePort())); //DNSエラー
            t.WaitAndClose();

            Assert.IsFalse(c.Succeeded);
            Debug.WriteLine(String.Format("NotConnectable2 [{0}]", c.ErrorMessage));
        }

        [Test]
        public void T05_NotConnectable3() {
            TestConnector c = new TestConnector();
            TestClient t= new TestClient();
            c.AsyncConnect(t, new TelnetParameter(GetConnectableDNSName(), GetClosedPort())); //ホストはあるがポートが開いてない
            t.WaitAndClose();

            Assert.IsFalse(c.Succeeded);
            Debug.WriteLine(String.Format("NotConnectable3 [{0}]", c.ErrorMessage));
        }

        [Test]
        public void T06_NegotiationFailed() {
            TestConnector c = new TestConnector();
            c.SetError(true);
            TestClient t= new TestClient();
            c.AsyncConnect(t, new TelnetParameter(GetConnectableDNSName(), GetConnectablePort()));
            t.WaitAndClose();

            Assert.IsFalse(c.Succeeded);
            Debug.WriteLine(String.Format("NegotiationFailed [{0}]", c.ErrorMessage));
        }

        [Test]
        public void T07_Interrupt() {
            TestConnector c = new TestConnector();
            c.SetWait1Sec(true);
            TestClient t = new TestClient();
            c.AsyncConnect(t, new TelnetParameter(GetConnectableDNSName(), GetConnectablePort()));
            Thread.Sleep(500);
            c.Interrupt();

            Assert.IsTrue(c.Interrupted);
            Assert.IsFalse(c.Succeeded);
            Assert.IsFalse(t.Notified); //Clientには成功・失敗とも通知されないはず
        }

        //TODO SOCKSサポート
    }
}
#endif
