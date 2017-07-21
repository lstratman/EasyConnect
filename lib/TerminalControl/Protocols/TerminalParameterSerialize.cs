/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalParameterSerialize.cs,v 1.8 2012/03/17 13:51:01 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Granados;
#if UNITTEST
using System.IO;
using NUnit.Framework;
#endif

using Poderosa.Serializing;
using System.Globalization;
using Granados.X11Forwarding;

namespace Poderosa.Protocols {
    internal abstract class TerminalParameterSerializer : ISerializeServiceElement {

        private Type _concreteType;

        public TerminalParameterSerializer(Type concreteType) {
            _concreteType = concreteType;
        }

        public void Serialize(TerminalParameter tp, StructuredText node) {
            if (tp.TerminalType != TerminalParameter.DEFAULT_TERMINAL_TYPE)
                node.Set("terminal-type", tp.TerminalType);
            if (tp.AutoExecMacroPath != null)
                node.Set("autoexec-macro", tp.AutoExecMacroPath);
        }
        public void Deserialize(TerminalParameter tp, StructuredText node) {
            tp.SetTerminalName(node.Get("terminal-type", TerminalParameter.DEFAULT_TERMINAL_TYPE));
            tp.AutoExecMacroPath = node.Get("autoexec-macro", null);
        }

        public Type ConcreteType {
            get {
                return _concreteType;
            }
        }

        public abstract StructuredText Serialize(object obj);
        public abstract object Deserialize(StructuredText node);
    }

    internal abstract class TCPParameterSerializer : TerminalParameterSerializer {
        private int _defaultPort;

        public TCPParameterSerializer(Type concreteType, int defaultport)
            : base(concreteType) {
            _defaultPort = defaultport;
        }


        public void Serialize(TCPParameter tp, StructuredText node) {
            base.Serialize(tp, node);
            node.Set("destination", tp.Destination);
            if (tp.Port != _defaultPort)
                node.Set("port", tp.Port.ToString());
        }
        public void Deserialize(TCPParameter tp, StructuredText node) {
            base.Deserialize(tp, node);
            tp.Destination = node.Get("destination", "");
            Debug.Assert(tp.Destination != null);
            tp.Port = ParseUtil.ParseInt(node.Get("port"), _defaultPort);
        }
    }

    internal class TelnetParameterSerializer : TCPParameterSerializer {

        public TelnetParameterSerializer()
            : base(typeof(TelnetParameter), 23) {
        }

        public override StructuredText Serialize(object obj) {
            StructuredText node = new StructuredText(this.ConcreteType.FullName);
            base.Serialize((TCPParameter)obj, node);
            node.Set("telnetNewLine", ((TelnetParameter)obj).TelnetNewLine.ToString());
            return node;
        }

        public override object Deserialize(StructuredText node) {
            TelnetParameter t = new TelnetParameter();
            base.Deserialize(t, node);
            // Note:
            //   for the backward compatibility, TelnetNewLine becomes false
            //   if parameter "telnetNewLine" doesn't exist.
            t.TelnetNewLine = Boolean.Parse(node.Get("telnetNewLine", Boolean.FalseString));
            return t;
        }
    }

    internal class SSHParameterSerializer : TCPParameterSerializer {
        public SSHParameterSerializer()
            : base(typeof(SSHLoginParameter), 22) {
        }
        //派生クラスからの指定用
        protected SSHParameterSerializer(Type t)
            : base(t, 22) {
        }

        public void Serialize(SSHLoginParameter tp, StructuredText node) {
            base.Serialize(tp, node);
            if (tp.Method != SSHProtocol.SSH2)
                node.Set("method", tp.Method.ToString());
            if (tp.AuthenticationType != AuthenticationType.Password)
                node.Set("authentication", tp.AuthenticationType.ToString());
            node.Set("account", tp.Account);
            if (tp.IdentityFileName.Length > 0)
                node.Set("identityFileName", tp.IdentityFileName);
            if (tp.PasswordOrPassphrase != null) {
                if (ProtocolsPlugin.Instance.ProtocolOptions.SavePlainTextPassword) {
                    node.Set("passphrase", tp.PasswordOrPassphrase);
                }
                else if (ProtocolsPlugin.Instance.ProtocolOptions.SavePassword) {
                    string pw = new SimpleStringEncrypt().EncryptString(tp.PasswordOrPassphrase);
                    if (pw != null) {
                        node.Set("password", pw);
                    }
                }
            }

            node.Set("enableAgentForwarding", tp.EnableAgentForwarding.ToString());

            node.Set("enableX11Forwarding", tp.EnableX11Forwarding.ToString());

            if (tp.X11Forwarding != null) {
                StructuredText x11Node = node.AddChild("x11Forwarding");
                x11Node.Set("display", tp.X11Forwarding.Display.ToString(NumberFormatInfo.InvariantInfo));
                x11Node.Set("screen", tp.X11Forwarding.Screen.ToString(NumberFormatInfo.InvariantInfo));
                x11Node.Set("needAuth", tp.X11Forwarding.NeedAuth.ToString());
                if (tp.X11Forwarding.XauthorityFile != null) {
                    x11Node.Set("xauthorityFile", tp.X11Forwarding.XauthorityFile);
                }
                x11Node.Set("useCygwinUnixDomainSocket", tp.X11Forwarding.UseCygwinUnixDomainSocket.ToString());
                if (tp.X11Forwarding.X11UnixFolder != null) {
                    x11Node.Set("x11UnixFolder", tp.X11Forwarding.X11UnixFolder);
                }
            }
        }
        public void Deserialize(SSHLoginParameter tp, StructuredText node) {
            base.Deserialize(tp, node);
            tp.Method = "SSH1".Equals(node.Get("method")) ? SSHProtocol.SSH1 : SSHProtocol.SSH2;
            tp.AuthenticationType = ParseUtil.ParseEnum<AuthenticationType>(node.Get("authentication", ""), AuthenticationType.Password);
            tp.Account = node.Get("account", "");
            tp.IdentityFileName = node.Get("identityFileName", "");
            if (ProtocolsPlugin.Instance.ProtocolOptions.ReadSerializedPassword) {
                string pw = node.Get("passphrase", null);
                if (pw != null) {
                    tp.PasswordOrPassphrase = pw;
                    tp.LetUserInputPassword = false;
                }
                else {
                    pw = node.Get("password", null);
                    if (pw != null) {
                        pw = new SimpleStringEncrypt().DecryptString(pw);
                        if (pw != null) {
                            tp.PasswordOrPassphrase = pw;
                            tp.LetUserInputPassword = false;
                        }
                    }
                }
            }

            tp.EnableAgentForwarding = GetBoolValue(node, "enableAgentForwarding", false);

            tp.EnableX11Forwarding = GetBoolValue(node, "enableX11Forwarding", false);

            StructuredText x11Node = node.FindChild("x11Forwarding");
            if (x11Node != null) {
                int display = GetIntValue(x11Node, "display", 0);
                X11ForwardingParams x11params = new X11ForwardingParams(display);
                x11params.Screen = GetIntValue(x11Node, "screen", 0);
                x11params.NeedAuth = GetBoolValue(x11Node, "needAuth", false);
                x11params.XauthorityFile = x11Node.Get("xauthorityFile", null);
                x11params.UseCygwinUnixDomainSocket = GetBoolValue(x11Node, "useCygwinUnixDomainSocket", false);
                x11params.X11UnixFolder = x11Node.Get("x11UnixFolder", null);
                tp.X11Forwarding = x11params;
            }
        }

        public override StructuredText Serialize(object obj) {
            StructuredText t = new StructuredText(this.ConcreteType.FullName);
            Serialize((SSHLoginParameter)obj, t);
            return t;
        }

        public override object Deserialize(StructuredText node) {
            SSHLoginParameter t = new SSHLoginParameter();
            Deserialize(t, node);
            return t;
        }

        private bool GetBoolValue(StructuredText node, string key, bool defaultValue) {
            string str = node.Get(key);
            if (str != null) {
                bool val;
                if (Boolean.TryParse(str, out val)) {
                    return val;
                }
            }
            return defaultValue;
        }

        private int GetIntValue(StructuredText node, string key, int defaultValue) {
            string str = node.Get(key);
            if (str != null) {
                int val;
                if (Int32.TryParse(str, out val)) {
                    return val;
                }
            }
            return defaultValue;
        }
    }

    internal class LocalShellParameterSerializer : TerminalParameterSerializer {
        public LocalShellParameterSerializer()
            : base(typeof(LocalShellParameter)) {
        }

        public void Serialize(LocalShellParameter tp, StructuredText node) {
            base.Serialize(tp, node);
            if (CygwinUtil.DefaultHome != tp.Home)
                node.Set("home", tp.Home);
            if (CygwinUtil.DefaultShell != tp.ShellName)
                node.Set("shellName", tp.ShellName);
            if (CygwinUtil.DefaultCygwinDir != tp.CygwinDir)
                node.Set("cygwin-directory", tp.CygwinDir);
        }
        public void Deserialize(LocalShellParameter tp, StructuredText node) {
            base.Deserialize(tp, node);
            tp.Home = node.Get("home", CygwinUtil.DefaultHome);
            tp.ShellName = node.Get("shellName", CygwinUtil.DefaultShell);
            tp.CygwinDir = node.Get("cygwin-directory", CygwinUtil.DefaultCygwinDir);
        }
        public override StructuredText Serialize(object obj) {
            StructuredText t = new StructuredText(this.ConcreteType.FullName);
            Serialize((LocalShellParameter)obj, t);
            return t;
        }

        public override object Deserialize(StructuredText node) {
            LocalShellParameter t = new LocalShellParameter();
            Deserialize(t, node);
            return t;
        }
    }

    //TODO シリアルポート

#if UNITTEST
    [TestFixture]
    public class TerminalParameterTests {

        private TelnetParameterSerializer _telnetSerializer;
        private SSHParameterSerializer _sshSerializer;
        private LocalShellParameterSerializer _localShellSerializer;

        [TestFixtureSetUp]
        public void Init() {
            _telnetSerializer = new TelnetParameterSerializer();
            _sshSerializer = new SSHParameterSerializer();
            _localShellSerializer = new LocalShellParameterSerializer();
        }

        [Test]
        public void Telnet0() {
            TelnetParameter p1 = new TelnetParameter();
            StructuredText t = _telnetSerializer.Serialize(p1);
            Assert.IsNull(t.Parent);
            Assert.IsNull(t.Get("port"));
            TelnetParameter p2 = (TelnetParameter)_telnetSerializer.Deserialize(t);
            Assert.AreEqual(23, p2.Port);
            Assert.AreEqual(TerminalParameter.DEFAULT_TERMINAL_TYPE, p2.TerminalType);
        }
        [Test]
        public void Telnet1() {
            TelnetParameter p1 = new TelnetParameter();
            p1.SetTerminalName("TERMINAL");
            p1.Port = 80;
            p1.Destination = "DESTINATION";
            StructuredText t = _telnetSerializer.Serialize(p1);
            TelnetParameter p2 = (TelnetParameter)_telnetSerializer.Deserialize(t);
            Assert.AreEqual(80, p2.Port);
            Assert.AreEqual("TERMINAL", p2.TerminalType);
            Assert.AreEqual("DESTINATION", p2.Destination);
        }
        [Test]
        public void SSH0() {
            SSHLoginParameter p1 = new SSHLoginParameter();
            StructuredText t = _sshSerializer.Serialize(p1);
            //確認
            StringWriter wr = new StringWriter();
            new TextStructuredTextWriter(wr).Write(t);
            wr.Close();
            Debug.WriteLine(wr.ToString());

            Assert.IsNull(t.Get("port"));
            Assert.IsNull(t.Get("method"));
            Assert.IsNull(t.Get("authentication"));
            Assert.IsNull(t.Get("identityFileName"));
            SSHLoginParameter p2 = (SSHLoginParameter)_sshSerializer.Deserialize(t);
            Assert.AreEqual(22, p2.Port);
            Assert.AreEqual(SSHProtocol.SSH2, p2.Method);
            Assert.AreEqual(AuthenticationType.Password, p2.AuthenticationType);
            Assert.AreEqual("", p2.IdentityFileName);
        }
        [Test]
        public void SSH1() {
            SSHLoginParameter p1 = new SSHLoginParameter();
            p1.Method = SSHProtocol.SSH1;
            p1.Account = "account";
            p1.IdentityFileName = "identity-file";
            p1.AuthenticationType = AuthenticationType.PublicKey;

            StructuredText t = _sshSerializer.Serialize(p1);
            UnitTestUtil.DumpStructuredText(t);
            //確認
            Debug.WriteLine(UnitTestUtil.DumpStructuredText(t));

            SSHLoginParameter p2 = (SSHLoginParameter)_sshSerializer.Deserialize(t);
            Assert.AreEqual(SSHProtocol.SSH1, p2.Method);
            Assert.AreEqual(AuthenticationType.PublicKey, p2.AuthenticationType);
            Assert.AreEqual("identity-file", p2.IdentityFileName);
            Assert.AreEqual("account", p2.Account);
        }
        //TODO CYGWIN
        //TODO StructuredTextを手で作成し、本来ありえないデータが入っていてもちゃんと読めることをテスト
    }
#endif
}
