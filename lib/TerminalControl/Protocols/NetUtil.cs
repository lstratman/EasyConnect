/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: NetUtil.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
#if UNITTEST
using NUnit.Framework;
#endif

namespace Poderosa.Protocols {
    //V4/V6それぞれ１つのアドレスを持ち、「両対応、ただし両方使えるときはV6優先」という性質をもつようにする
    internal class IPAddressList {
        private IPAddress[] _addresses;

        public IPAddressList(string host) {
            _addresses = Dns.GetHostAddresses(host);
        }
        public IPAddressList(IPAddress a) {
            _addresses = new IPAddress[] { a };
        }
        public IPAddressList(IPAddress v4, IPAddress v6) {
            _addresses = new IPAddress[] { v4, v6 };
        }
        public IPAddressList(IPAddress[] addresses) {
            _addresses = addresses;
        }
        //V4,6両方取れるときはV6優先。オプション設定というのはあるかもしれんが
        public IPAddress[] AvailableAddresses {
            get {
                IPVersionPriority pr = ProtocolsPlugin.Instance.ProtocolOptions.IPVersionPriority;
                if (pr == IPVersionPriority.Both)
                    return _addresses; //無制限

                //でなければ適当にコピーを
                List<IPAddress> result = new List<IPAddress>();
                foreach (IPAddress a in _addresses) {
                    if (pr == IPVersionPriority.V6Only && a.AddressFamily == AddressFamily.InterNetworkV6)
                        result.Add(a);
                    else if (pr == IPVersionPriority.V4Only && a.AddressFamily == AddressFamily.InterNetwork)
                        result.Add(a);
                }
                return result.ToArray();
            }
        }

        //不正でもエラーにはしないタイプ
        public static IPAddressList SilentGetAddress(string host) {
            IPAddress address;
            if (IPAddress.TryParse(host, out address))
                return new IPAddressList(address);
            else {
                try {
                    return new IPAddressList(host);
                }
                catch (Exception) {
                    return new IPAddressList(new IPAddress[0]);
                }
            }
        }
    }

    internal class NetUtil {
        private class AsyncConnectProcessor {
            private Socket _socket;
            private bool _endConnectRequired;
            private string _errorMessage;

            public AsyncConnectProcessor(Socket s) {
                _socket = s;
                _endConnectRequired = true;
            }
            public void End(IAsyncResult ar) {
                if (_endConnectRequired) {
                    try {
                        _socket.EndConnect(ar);
                    }
                    catch (Exception ex) {
                        _errorMessage = ex.Message;
                    }
                }
            }

            public bool EndConnectRequired {
                get {
                    return _endConnectRequired;
                }
                set {
                    _endConnectRequired = value;
                }
            }

            public string ErrorMessage {
                get {
                    return _errorMessage;
                }
            }
        }

        public static Socket ConnectTCPSocket(IPAddressList addr, int port) {
#if UNITTEST
            return ConnectTCPSocket(addr, port, 3000);
#else
            return ConnectTCPSocket(addr, port, PEnv.Options.SocketConnectTimeout);
#endif
        }

        public static Socket ConnectTCPSocket(IPAddressList addrlist, int port, int timeout) {
            foreach (IPAddress addr in addrlist.AvailableAddresses) {
                try {
                    Socket s = ConnectTCPSocket(addr, port, timeout);
                    if (s != null)
                        return s; //一つでも成功すればそれを使う
                }
                catch (Exception ex) {
                    ProtocolsPlugin.Instance.NetLog(ex.Message);
                }
            }
            throw new Exception(String.Format(PEnv.Strings.GetString("Message.FailedToConnectAddress"), addrlist.AvailableAddresses[0].ToString()));
        }
        public static Socket ConnectTCPSocket(IPAddress addr, int port, int timeout) {

            Socket s = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            s.NoDelay = true;
            IAsyncResult ar = null;
            bool timed_out = false;
            AsyncConnectProcessor proc = new AsyncConnectProcessor(s);

            try {
                ar = s.BeginConnect(new IPEndPoint(addr, port), new AsyncCallback(proc.End), null);
                timed_out = !ar.AsyncWaitHandle.WaitOne(timeout, false);
            }
            catch (Exception ex) { //ブロック中の例外はここで
                proc.EndConnectRequired = false;
                s.Close();
                ProtocolsPlugin.Instance.NetLog(ex.Message);
                return null;
            }

            if (timed_out) {
                proc.EndConnectRequired = false;
                s.Close();
                ProtocolsPlugin.Instance.NetLog(String.Format("timed out connecting to {0}", addr.ToString()));
                return null;
            }

            if (!s.Connected) {
                ProtocolsPlugin.Instance.NetLog(proc.ErrorMessage);
                return null;
            }

            return s;
        }


    }
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class NetAddressUtil {
        public static bool IsNetworkAddress(string netaddress) {
            try {
                Regex re = new Regex("([\\dA-Fa-f\\.\\:]+)/\\d+");
                Match m = re.Match(netaddress);
                if (m.Length != netaddress.Length || m.Index != 0)
                    return false;

                //かっこがIPアドレスならOK
                string a = m.Groups[1].Value;
                IPAddress.Parse(a);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
        public static bool NetAddressIncludesIPAddress(string netaddress, IPAddress target) {
            int slash = netaddress.IndexOf('/');
            int bits = Int32.Parse(netaddress.Substring(slash + 1));
            IPAddress net = IPAddress.Parse(netaddress.Substring(0, slash));
            if (net.AddressFamily != target.AddressFamily)
                return false;

            byte[] bnet = net.GetAddressBytes();
            byte[] btarget = target.GetAddressBytes();
            Debug.Assert(bnet.Length == btarget.Length);

            for (int i = 0; i < bnet.Length; i++) {
                byte b1 = bnet[i];
                byte b2 = btarget[i];
                if (bits <= 0)
                    return true;
                else if (bits >= 8) {
                    if (b1 != b2)
                        return false;
                }
                else {
                    b1 >>= (8 - bits);
                    b2 >>= (8 - bits);
                    if (b1 != b2)
                        return false;
                }
                bits -= 8;
            }
            return true;
        }
    }

#if UNITTEST
    [TestFixture]
    public class NetUtilTests {
        [Test]
        public void TestTimeout() {
            Exception e = null;
            try {
                Socket s = NetUtil.ConnectTCPSocket(new IPAddress46Pair(IPAddress.Parse("1.1.1.1")), 10); //接続できないはずのものをテスト
            }
            catch (Exception ex) {
                e = ex;
            }

            Assert.IsTrue(e != null);
            Assert.AreEqual("TIMEOUT", e.Message);
        }

        [Test]
        public void TestSuccessful() {
            Socket s = NetUtil.ConnectTCPSocket(new IPAddress46Pair(IPAddress.Loopback), 8888); //listenしてる適当なポートにつなぐ
            Assert.IsTrue(s.Connected);
        }

        /*
        [Test]
        public void TestIPVersionPriority() {
            IPAddress t1 = IPAddress46Pair.DetermineIPAddress(IPAddress.Loopback, IPAddress.IPv6Loopback, IPVersionPriority.PriorityV6);
            Assert.IsTrue(t1.AddressFamily==AddressFamily.InterNetworkV6);
            IPAddress t2 = IPAddress46Pair.DetermineIPAddress(IPAddress.Loopback, IPAddress.IPv6Loopback, IPVersionPriority.PriorityV4);
            Assert.IsTrue(t2.AddressFamily==AddressFamily.InterNetwork);
            //片方nullならそれは使わない
            IPAddress t3 = IPAddress46Pair.DetermineIPAddress(null, IPAddress.IPv6Loopback, IPVersionPriority.PriorityV4);
            Assert.IsTrue(t3.AddressFamily==AddressFamily.InterNetworkV6);
            IPAddress t4 = IPAddress46Pair.DetermineIPAddress(IPAddress.Loopback, null, IPVersionPriority.PriorityV6);
            Assert.IsTrue(t4.AddressFamily==AddressFamily.InterNetwork);
        }
        */
    }
#endif
}
