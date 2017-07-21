/*
 Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 This file is a part of the Granados SSH Client Library that is subject to
 the license included in the distributed package.
 You may not use this file except in compliance with the license.

 $Id: Tutorial.cs,v 1.2 2011/10/27 23:21:56 kzmi Exp $
*/
using Granados.AgentForwarding;
using Granados.IO;
using Granados.KeyboardInteractive;
using Granados.PKI;
using Granados.SSH;
using Granados.SSH1;
using Granados.SSH2;
using Granados.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Granados.Tutorial {
#if ENABLE_TUTORIAL

    /**
     * Granados Tutorial
     *   To learn the usage of Granados, please read the code in this file.
     */
    /// <exclude/>
    class Tutorial {

        [STAThread]
        static void Main(string[] args) {

            //NOTE: modify this number to run these samples!
            int tutorial = 5;

            if (tutorial == 0)
                GenerateRSAKey();
            else if (tutorial == 1)
                GenerateDSAKey();
            else if (tutorial == 2)
                ConnectAndOpenShell();
            else if (tutorial == 3)
                ConnectSSH2AndPortforwarding();
            else if (tutorial == 5)
                AgentForward();
        }

        //Tutorial: Generating a new RSA key for user authentication
        private static void GenerateRSAKey() {
            //RSA KEY GENERATION TEST
            byte[] testdata = Encoding.ASCII.GetBytes("CHRISTIAN VIERI");
            RSAKeyPair kp = RSAKeyPair.GenerateNew(2048, RngManager.GetSecureRng());

            //sign and verify test
            byte[] sig = kp.Sign(testdata);
            kp.Verify(sig, testdata);

            //export / import test
            SSH2UserAuthKey key = new SSH2UserAuthKey(kp);
            key.WritePublicPartInOpenSSHStyle(new FileStream("newrsakey.pub", FileMode.Create));
            key.WritePrivatePartInSECSHStyleFile(new FileStream("newrsakey.bin", FileMode.Create), "comment", "passphrase");
            //read test
            SSH2UserAuthKey newpk = SSH2UserAuthKey.FromSECSHStyleFile("newrsakey.bin", "passphrase");
        }

        //Tutorial: Generating a new DSA key for user authentication
        private static void GenerateDSAKey() {
            //DSA KEY GENERATION TEST
            byte[] testdata = Encoding.ASCII.GetBytes("CHRISTIAN VIERI");
            DSAKeyPair kp = DSAKeyPair.GenerateNew(2048, RngManager.GetSecureRng());

            //sign and verify test
            byte[] sig = kp.Sign(testdata);
            kp.Verify(sig, testdata);

            //export / import test
            SSH2UserAuthKey key = new SSH2UserAuthKey(kp);
            key.WritePublicPartInOpenSSHStyle(new FileStream("newdsakey.pub", FileMode.Create));
            key.WritePrivatePartInSECSHStyleFile(new FileStream("newrsakey.bin", FileMode.Create), "comment", "passphrase");
            //read test
            SSH2UserAuthKey newpk = SSH2UserAuthKey.FromSECSHStyleFile("newrsakey.bin", "passphrase");
        }

        private class SampleKeyboardInteractiveAuthenticationHandler : IKeyboardInteractiveAuthenticationHandler {

            private readonly string _password;

            private readonly AtomicBox<bool> _box = new AtomicBox<bool>();

            public SampleKeyboardInteractiveAuthenticationHandler(string password) {
                _password = password;
            }

            public bool GetResult() {
                bool result = false;
                _box.TryGet(ref result, 10000);
                return result;
            }

            public string[] KeyboardInteractiveAuthenticationPrompt(string[] prompts, bool[] echoes) {
                return prompts.Select(s => s.Contains("assword") ? _password : "").ToArray();
            }

            public void OnKeyboardInteractiveAuthenticationStarted() {
            }

            public void OnKeyboardInteractiveAuthenticationCompleted(bool success, Exception error) {
                _box.TrySet(success, 10000);
            }
        }

        //Tutorial: Connecting to a host and opening a shell
        private static void ConnectAndOpenShell() {
            SampleKeyboardInteractiveAuthenticationHandler authHandler = null;
            SSHConnectionParameter f = new SSHConnectionParameter("172.22.1.15", 22, SSHProtocol.SSH2, AuthenticationType.PublicKey, "okajima", "aaa");
            //former algorithm is given priority in the algorithm negotiation
            f.PreferableHostKeyAlgorithms = new PublicKeyAlgorithm[] { PublicKeyAlgorithm.RSA, PublicKeyAlgorithm.DSA };
            f.PreferableCipherAlgorithms = new CipherAlgorithm[] { CipherAlgorithm.Blowfish, CipherAlgorithm.TripleDES };
            f.WindowSize = 0x1000; //this option is ignored with SSH1
            f.KeyboardInteractiveAuthenticationHandlerCreator =
                (connection) => {
                    return (authHandler = new SampleKeyboardInteractiveAuthenticationHandler("aaa"));
                };

            Tracer tracer = new Tracer();

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(new IPEndPoint(IPAddress.Parse(f.HostName), f.PortNumber)); //22 is the default SSH port

            ISSHConnection conn;

            if (f.AuthenticationType == AuthenticationType.KeyboardInteractive) {
                //Creating a new SSH connection over the underlying socket
                conn = SSHConnection.Connect(s, f, c => new Reader(c), c => new Tracer());
                bool result = authHandler.GetResult();
                Debug.Assert(result == true);
            }
            else {
                //NOTE: if you use public-key authentication, follow this sample instead of the line above:
                //f.AuthenticationType = AuthenticationType.PublicKey;
                f.IdentityFile = "C:\\P4\\tools\\keys\\aaa";
                f.VerifySSHHostKey = (info) => {
                    byte[] h = info.HostKeyFingerPrint;
                    foreach (byte b in h)
                        Debug.Write(String.Format("{0:x2} ", b));
                    return true;
                };

                //Creating a new SSH connection over the underlying socket
                conn = SSHConnection.Connect(s, f, c => new Reader(c), null);
            }

            //Opening a shell
            var ch = conn.OpenShell(channelOperator => new ChannelHandler(channelOperator));

            //you can get the detailed connection information in this way:
            //SSHConnectionInfo ci = _conn.ConnectionInfo;

            //Go to sample shell
            SampleShell(ch);
        }

        //Tutorial: port forwarding
        private static void ConnectSSH2AndPortforwarding() {
            SSHConnectionParameter f = new SSHConnectionParameter("10.10.9.8", 22, SSHProtocol.SSH2, AuthenticationType.Password, "root", "");

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(new IPEndPoint(IPAddress.Parse(f.HostName), f.PortNumber)); //22 is the default SSH port

            //NOTE: if you use public-key authentication, follow this sample instead of the line above:
            //  f.AuthenticationType = AuthenticationType.PublicKey;
            //  f.IdentityFile = "privatekey.bin";
            //  f.Password = "passphrase";

            //former algorithm is given priority in the algorithm negotiation
            f.PreferableHostKeyAlgorithms = new PublicKeyAlgorithm[] { PublicKeyAlgorithm.DSA };
            f.PreferableCipherAlgorithms = new CipherAlgorithm[] { CipherAlgorithm.Blowfish, CipherAlgorithm.TripleDES };

            f.WindowSize = 0x1000; //this option is ignored with SSH1

            //Creating a new SSH connection over the underlying socket
            Reader reader = null;
            var conn = SSHConnection.Connect(s, f,
                c => {
                    return reader = new Reader(c);
                },
                c => new Tracer());

            Debug.Assert(reader != null);

            //Local->Remote port forwarding
            ChannelHandler ch = conn.ForwardPort(
                    channelOperator => new ChannelHandler(channelOperator),
                    "www.google.co.jp", 80u, "localhost", 0u);
            while (!reader._ready)
                System.Threading.Thread.Sleep(100); //wait response
            byte[] data = Encoding.ASCII.GetBytes("GET / HTTP/1.0\r\n\r\n");
            ch.Operator.Send(new DataFragment(data, 0, data.Length)); //get the toppage

            //Remote->Local
            // if you want to listen to a port on the SSH server, follow this line:
            //_conn.ListenForwardedPort("0.0.0.0", 10000);

            //NOTE: if you use SSH2, dynamic key exchange feature is supported.
            //((SSH2Connection)_conn).ReexchangeKeys();
        }

        private static void SampleShell(ChannelHandler channelHandler) {
            byte[] b = new byte[1];
            while (true) {
                int input = System.Console.Read();

                b[0] = (byte)input;
                channelHandler.Operator.Send(new DataFragment(b, 0, b.Length));
            }
        }

        private static void AgentForward() {
            SSHConnectionParameter f = new SSHConnectionParameter("172.22.1.15", 22, SSHProtocol.SSH2, AuthenticationType.Password, "root", "");

            Tracer tracer = new Tracer();

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(new IPEndPoint(IPAddress.Parse(f.HostName), f.PortNumber)); //22 is the default SSH port

            //former algorithm is given priority in the algorithm negotiation
            f.PreferableHostKeyAlgorithms = new PublicKeyAlgorithm[] { PublicKeyAlgorithm.RSA, PublicKeyAlgorithm.DSA };
            f.PreferableCipherAlgorithms = new CipherAlgorithm[] { CipherAlgorithm.Blowfish, CipherAlgorithm.TripleDES };
            f.WindowSize = 0x1000; //this option is ignored with SSH1
            f.AgentForwardingAuthKeyProvider = new AgentForwardingAuthKeyProvider();

            //Creating a new SSH connection over the underlying socket
            Reader reader = null;
            var conn = SSHConnection.Connect(s, f,
                            c => {
                                return reader = new Reader(c);
                            },
                            c => new Tracer()
                       );
            Debug.Assert(reader != null);

            //Opening a shell
            var ch = conn.OpenShell(channelOperator => new ChannelHandler(channelOperator));

            while (!reader._ready)
                Thread.Sleep(100);

            Thread.Sleep(1000);
            byte[] data = Encoding.Default.GetBytes("ssh -A -l okajima localhost\r");
            ch.Operator.Send(new DataFragment(data, 0, data.Length));

            //Go to sample shell
            SampleShell(ch);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    class Reader : ISSHConnectionEventHandler {
        private readonly ISSHConnection _conn;
        public bool _ready;

        public Reader(ISSHConnection conn) {
            _conn = conn;
        }

        public void OnData(DataFragment data) {
            System.Console.Write(Encoding.ASCII.GetString(data.Data, data.Offset, data.Length));
        }
        public void OnDebugMessage(bool alwaysDisplay, string message) {
            Debug.WriteLine("DEBUG: " + message);
        }
        public void OnIgnoreMessage(byte[] data) {
            Debug.WriteLine("Ignore: " + Encoding.ASCII.GetString(data));
        }

        public void OnError(Exception error) {
            Debug.WriteLine("ERROR: " + error.Message);
            Debug.WriteLine(error.StackTrace);
        }
        public void OnChannelClosed() {
            Debug.WriteLine("Channel closed");
            //_conn.AsyncReceive(this);
        }
        public void OnChannelEOF() {
            Debug.WriteLine("Channel EOF");
        }
        public void OnExtendedData(uint type, DataFragment data) {
            Debug.WriteLine("EXTENDED DATA");
        }
        public void OnConnectionClosed() {
            Debug.WriteLine("Connection closed");
        }
        public void OnUnhandledMessage(byte type, byte[] data) {
            Debug.WriteLine("Unhandled Message " + type);
        }
        public void OnChannelReady() {
            _ready = true;
        }
        public void OnChannelError(Exception error) {
            Debug.WriteLine("Channel ERROR: " + error.Message);
        }
        public void OnMiscPacket(byte type, DataFragment data) {
        }
    }

    class ChannelHandler : ISSHChannelEventHandler {

        private readonly ISSHChannel _operator;

        public ISSHChannel Operator {
            get {
                return _operator;
            }
        }

        public ChannelHandler(ISSHChannel channelOperator) {
            _operator = channelOperator;
        }

        public void OnEstablished(DataFragment data) {
            Debug.WriteLine("Channel Established");
        }

        public void OnReady() {
            Debug.WriteLine("Channel Ready");
        }

        public void OnData(DataFragment data) {
            System.Console.Write(Encoding.UTF8.GetString(data.Data, data.Offset, data.Length));
        }

        public void OnExtendedData(uint type, DataFragment data) {
            System.Console.WriteLine("EXT[{0}] {1}", type, Encoding.UTF8.GetString(data.Data, data.Offset, data.Length));
        }

        public void OnClosing(bool byServer) {
            Debug.WriteLine("Channel Closing");
        }

        public void OnClosed(bool byServer) {
            Debug.WriteLine("Channel Closed");
        }

        public void OnEOF() {
            Debug.WriteLine("Channel EOF");
        }

        public void OnRequestFailed() {
            throw new NotImplementedException();
        }

        public void OnError(Exception error) {
            Debug.WriteLine("Channel ERROR: " + error.Message);
            Debug.WriteLine(error.StackTrace);
        }

        public void OnUnhandledPacket(byte packetType, DataFragment data) {
            Debug.WriteLine("Channel Unhandled Packet: {0}", packetType);
        }

        public void OnConnectionLost() {
            Debug.WriteLine("Connection Lost");
        }

        public void Dispose() {
            Debug.WriteLine("Channel Dispose");
        }

    }

    class Tracer : ISSHProtocolEventLogger {
        public void OnSend(string messageType, string details) {
            Debug.WriteLine("EVENT:[S] <{0}> {1}", messageType, details);
        }

        public void OnReceived(string messageType, string details) {
            Debug.WriteLine("EVENT:[R] <{0}> {1}", messageType, details);
        }

        public void OnTrace(string details) {
            Debug.WriteLine("TRACE: {0}", details);
        }
    }

    class AgentForwardingAuthKeyProvider : IAgentForwardingAuthKeyProvider {

        private SSH1UserAuthKey[] _ssh1Keys;
        private SSH2UserAuthKey[] _ssh2Keys;

        public bool IsAuthKeyProviderEnabled {
            get {
                // always active
                return true;
            }
        }

        public SSH1UserAuthKey[] GetAvailableSSH1UserAuthKeys() {
            if (_ssh1Keys == null) {
                try {
                    SSH1UserAuthKey k = new SSH1UserAuthKey(@"C:\P4\Tools\keys\aaa", "aaa");
                    _ssh1Keys = new SSH1UserAuthKey[] { k };
                }
                catch (Exception e) {
                    Debug.WriteLine(e.Message);
                    _ssh1Keys = new SSH1UserAuthKey[0];
                }
            }
            return _ssh1Keys;
        }

        public SSH2UserAuthKey[] GetAvailableSSH2UserAuthKeys() {
            if (_ssh2Keys == null) {
                try {
                    SSH2UserAuthKey k = SSH2UserAuthKey.FromSECSHStyleFile(@"C:\P4\Tools\keys\aaa", "aaa");
                    _ssh2Keys = new SSH2UserAuthKey[] { k };
                }
                catch (Exception e) {
                    Debug.WriteLine(e.Message);
                    _ssh2Keys = new SSH2UserAuthKey[0];
                }
            }
            return _ssh2Keys;
        }
    }
#endif
}
