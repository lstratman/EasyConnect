/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: socks.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
*/
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Poderosa.Protocols {
    internal class Socks {
        private int _version;
        private string _serverName;
        private short _serverPort;
        private string _account;
        private string _password;
        private string _destName;       //_destName, _destAddressはどちらかのみ使用
        private string _excludingNetworks;
        private IPAddress _destAddress;
        private short _destPort;

        public Socks() {
            _version = 5;
        }
        public int Version {
            get {
                return _version;
            }
            set {
                if (value != 5 && value != 4)
                    throw new IOException("Wrong SOCKS version");
                _version = value;
            }
        }
        public string Account {
            get {
                return _account;
            }
            set {
                _account = value;
            }
        }
        public string Password {
            get {
                return _password;
            }
            set {
                _password = value;
            }
        }
        public string DestName {
            get {
                return _destName;
            }
            set {
                _destName = value;
            }
        }
        public IPAddress DestAddress {
            get {
                return _destAddress;
            }
            set {
                _destAddress = value;
            }
        }

        public short DestPort {
            get {
                return _destPort;
            }
            set {
                _destPort = value;
            }
        }

        public string ServerName {
            get {
                return _serverName;
            }
            set {
                _serverName = value;
            }
        }
        public short ServerPort {
            get {
                return _serverPort;
            }
            set {
                _serverPort = value;
            }
        }
        public string ExcludingNetworks {
            get {
                return _excludingNetworks;
            }
            set {
                _excludingNetworks = value;
            }
        }

        public void Connect(Socket s) {
            if (_version == 4)
                ConnectBySocks4(s);
            else
                ConnectBySocks5(s);
        }

        private void ConnectBySocks4(Socket s) {
            MemoryStream u = new MemoryStream();
            BinaryWriter wr = new BinaryWriter(u);
            wr.Write((byte)4);
            wr.Write((byte)1);
            wr.Write(IPAddress.HostToNetworkOrder(_destPort));
            if (_destAddress == null) { //hostが指定された
                //以下でよいと思ったが、どうも動かない。プロトコル4aというやつがサポートされていないのか？
                throw new IOException("The address is not specified.");
                /*
                wr.Write(IPAddress.HostToNetworkOrder(1));
                wr.Write(Encoding.ASCII.GetBytes(_account));
                wr.Write((byte)0); //null char
                wr.Write(Encoding.ASCII.GetBytes(_destName));
                wr.Write((byte)0);
                */
            }
            else {
                wr.Write(_destAddress.GetAddressBytes());
                wr.Write(Encoding.ASCII.GetBytes(_account));
                wr.Write((byte)0); //null char
            }
            wr.Close();
            byte[] payload = u.ToArray();
            s.Send(payload, 0, payload.Length, SocketFlags.None);

            byte[] response = new byte[8];
            s.Receive(response, 0, response.Length, SocketFlags.None);
            if (response[1] != 90)
                throw new IOException("SOCKS authentication failed.");
            /*
            #define SOCKS4_REP_SUCCEEDED	90	 rquest granted (succeeded) 
            #define SOCKS4_REP_REJECTED	91	 request rejected or failed
            #define SOCKS4_REP_IDENT_FAIL	92	 cannot connect identd 
            #define SOCKS4_REP_USERID	93	user id not matched 
            */

        }

        private void ConnectBySocks5(Socket s) {
            byte[] first = new byte[] { 5, 2, 0, 2 };
            s.Send(first, 0, 4, SocketFlags.None);
            //s.WriteByte((byte)5);
            //s.WriteByte((byte)2);
            //s.WriteByte((byte)0); //no auth
            //s.WriteByte((byte)2); //user/pass

            byte[] response = new byte[4];
            int r = s.Receive(response, 0, 2, SocketFlags.None);
            //Debug.WriteLine("[0] len="+r+" res="+response[1]);
            if (r != 2)
                throw new IOException("Failed to communicate with the SOCKS server.");
            if (response[0] != 5)
                throw new IOException(String.Format("The SOCKS server specified an unsupported authentication method [{0}].", response[0]));

            if (response[1] == 0) {
                ; //no auth
            }
            else if (response[1] == 2) {
                Sock5Auth(s);
            }
            else
                throw new IOException(String.Format("The SOCKS server specified an unsupported authentication method [{0}].", response[1]));


            MemoryStream u = new MemoryStream();
            BinaryWriter wr = new BinaryWriter(u);
            wr.Write((byte)5);
            wr.Write((byte)1); //command
            wr.Write((byte)0);
            if (_destAddress == null) {
                wr.Write((byte)3);
                wr.Write((byte)_destName.Length);
                wr.Write(Encoding.ASCII.GetBytes(_destName));
            }
            else {
                wr.Write((byte)1);
                wr.Write(_destAddress.GetAddressBytes());
            }
            wr.Write(IPAddress.HostToNetworkOrder(_destPort));
            wr.Close();
            byte[] payload = u.ToArray();
            s.Send(payload, 0, payload.Length, SocketFlags.None);

            r = s.Receive(response, 0, 4, SocketFlags.None);
            if (response[1] != 0)
                throw new IOException("Failed to communicate with the SOCKS server." + GetSocks5ErrorMessage(response[1]));

            //read addr and port
            if (response[3] == 3) {
                byte[] t = new byte[1];
                s.Receive(t, 0, 1, SocketFlags.None);
                byte[] t2 = new byte[t[0]];
                s.Receive(t2, 0, t2.Length, SocketFlags.None);
            }
            else if (response[3] == 1) {
                byte[] t = new byte[6];
                s.Receive(t, 0, 6, SocketFlags.None);
            }
            else
                throw new IOException("unexpected destination addr type " + response[3]);
        }

        private void Sock5Auth(Socket s) {
            MemoryStream u = new MemoryStream();
            u.WriteByte(1);
            byte[] t = Encoding.ASCII.GetBytes(_account);
            u.WriteByte((byte)t.Length);
            u.Write(t, 0, t.Length);
            t = Encoding.ASCII.GetBytes(_password);
            u.WriteByte((byte)t.Length);
            u.Write(t, 0, t.Length);
            byte[] payload = u.ToArray();
            s.Send(payload, 0, payload.Length, SocketFlags.None);

            byte[] response = new byte[2];
            int r = s.Receive(response, 0, 2, SocketFlags.None);
            if (r != 2 || response[1] != 0)
                throw new IOException("SOCKS authentication failed.");
        }

        private static string GetSocks5ErrorMessage(byte code) {
            /*
             o  X'01' general SOCKS server failure
             o  X'02' connection not allowed by ruleset
             o  X'03' Network unreachable
             o  X'04' Host unreachable
             o  X'05' Connection refused
             o  X'06' TTL expired
             o  X'07' Command not supported
             o  X'08' Address type not supported
             o  X'09' to X'FF' unassigned
             */
            switch (code) {
                case 1:
                    return "SOCKS server error";
                case 2:
                    return "The connection is not allowed.";
                case 3:
                    return "Failed to reach the destination network.";
                case 4:
                    return "Failed to reach the destination host.";
                case 5:
                    return "The access is denied.";
                case 6:
                    return "TTL is disposed.";
                case 7:
                    return "The SOCKS command is not supported.";
                case 8:
                    return "The type of address is not supported.";
                default:
                    return String.Format("Unknown Code {0}", code);
            }
        }

        /*
        public static void Test() {
            TcpClient tcp = new TcpClient();
            tcp.Connect("akamatsu", 1080);
            Socks s = new Socks();
            s.Version = 5;
            s.DestName = "ume";
            //s.DestAddress = IPAddress.Parse("192.168.10.23");
            s.DestPort = 23;
            s.Account = "okajima";
            s.Password = "okajima";

            s.Connect(tcp.GetStream());
        }
        */
    }

}
