/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ShortcutFile.cs,v 1.3 2012/03/18 01:35:15 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

using Poderosa.Serializing;
using Poderosa.Terminal;
using Poderosa.Protocols;
using Poderosa.ConnectionParam;
using Poderosa.Util;
using Granados;

namespace Poderosa.Sessions {
    internal class ShortcutFileContent {
        private ITerminalSettings _settings;
        private ITerminalParameter _param;

        public ShortcutFileContent(ITerminalSettings settings, ITerminalParameter param) {
            _settings = settings;
            _param = param;
        }
        public ITerminalSettings TerminalSettings {
            get {
                return _settings;
            }
        }
        public ITerminalParameter TerminalParameter {
            get {
                return _param;
            }
        }

        public void SaveToXML(string filename) {
            ISerializeService ss = TerminalSessionsPlugin.Instance.SerializeService;
            StructuredText settings_text = ss.Serialize(_settings);
            StructuredText parameter_text = ss.Serialize(_param);
            //新形式で
            StructuredText root = new StructuredText("poderosa-shortcut");
            root.Set("version", "4.0");
            root.AddChild(settings_text);
            root.AddChild(parameter_text);

            XmlWriter wr = CreateDefaultWriter(filename);
            new XmlStructuredTextWriter(wr).Write(root);
            wr.WriteEndDocument();
            wr.Close();

        }

        public static ShortcutFileContent LoadFromXML(string filename) {
            XmlDocument doc = FileToDOM(filename);
            XmlElement root = doc.DocumentElement;
            if (root.LocalName == "poderosa-shortcut") {
                if (root.GetAttribute("version") == "4.0")
                    return ParseV4(root);
                else
                    return ParseOldFormat(root);
            }
            else
                throw new FormatException("Unknown file"); //TODO message
        }
        private static ShortcutFileContent ParseV4(XmlElement root) {
            XmlElement first = null;
            XmlElement second = null;
            //ちょっといいかげんだが、最初のElement、２個目のElementを。
            foreach (XmlNode node in root.ChildNodes) {
                XmlElement e = node as XmlElement;
                if (e != null) {
                    if (first == null)
                        first = e;
                    else if (second == null) {
                        second = e;
                        break;
                    }
                }
            }

            if (second == null)
                throw new FormatException("Unknown XML Format");
            StructuredText setting_text = new XmlStructuredTextReader(first).Read();
            StructuredText parameter_text = new XmlStructuredTextReader(second).Read();

            ISerializeService ss = TerminalSessionsPlugin.Instance.SerializeService;
            ITerminalSettings setting = ss.Deserialize(setting_text) as ITerminalSettings;
            if (setting == null)
                throw new FormatException("TerminalSettings could not be loaded");
            ITerminalParameter param = ss.Deserialize(parameter_text) as ITerminalParameter;
            if (param == null)
                throw new FormatException("TerminalParameter could not be loaded");

            return new ShortcutFileContent(setting, param);
        }

        //旧バージョンフォーマットの読み込み
        private static ShortcutFileContent ParseOldFormat(XmlElement root) {
            if (root.GetAttribute("type") != "tcp")
                throw new FormatException("Unknown File Format");

            //accountの有無でTelnet/SSHを切り替え
            ITerminalParameter param;
            ISSHLoginParameter ssh = null;
            ITCPParameter tcp = null;
            string account = root.GetAttribute("account");
            if (account.Length > 0) {
                ssh = TerminalSessionsPlugin.Instance.ProtocolService.CreateDefaultSSHParameter();
                ssh.Account = account;
                tcp = (ITCPParameter)ssh.GetAdapter(typeof(ITCPParameter));
            }
            else
                tcp = TerminalSessionsPlugin.Instance.ProtocolService.CreateDefaultTelnetParameter();

            param = (ITerminalParameter)tcp.GetAdapter(typeof(ITerminalParameter));
            ITerminalSettings settings = TerminalSessionsPlugin.Instance.TerminalEmulatorService.CreateDefaultTerminalSettings("", null);

            settings.BeginUpdate();
            //アトリビュート舐めて設定
            foreach (XmlAttribute attr in root.Attributes) {
                switch (attr.Name) {
                    case "auth":
                        if (ssh != null)
                            ssh.AuthenticationType = ParseUtil.ParseEnum<AuthenticationType>(attr.Value, AuthenticationType.Password);
                        break;
                    case "keyfile":
                        if (ssh != null)
                            ssh.IdentityFileName = attr.Value;
                        break;
                    case "encoding":
                        settings.Encoding = EncodingType.ISO8859_1;
                        foreach (EnumListItem<EncodingType> item in EnumListItem<EncodingType>.GetListItems()) {
                            if (attr.Value == item.ToString()) {
                                settings.Encoding = item.Value;
                                break;
                            }
                        }
                        break;
                    case "terminal-type":
                        settings.TerminalType = ParseUtil.ParseEnum<TerminalType>(attr.Value, TerminalType.XTerm);
                        param.SetTerminalName(attr.Value);
                        break;
                    case "localecho":
                        settings.LocalEcho = ParseUtil.ParseBool(attr.Value, false);
                        break;
                    case "caption":
                        settings.Caption = attr.Value;
                        break;
                    case "transmit-nl":
                        settings.TransmitNL = ParseUtil.ParseEnum<NewLine>(attr.Value, NewLine.CR);
                        break;
                    case "host":
                        tcp.Destination = attr.Value;
                        break;
                    case "port":
                        tcp.Port = ParseUtil.ParseInt(attr.Value, ssh != null ? 22 : 23);
                        break;
                    case "method":
                        if (ssh != null)
                            ssh.Method = attr.Value == "SSH1" ? SSHProtocol.SSH1 : SSHProtocol.SSH2;
                        break;
                }
            }
            //ts.LineFeedRule = ParseUtil.ParseEnum<LineFeedRule>(node.Get("linefeedrule"), LineFeedRule.Normal);
            settings.EndUpdate();

            return new ShortcutFileContent(settings, param);
        }

        private static XmlWriter CreateDefaultWriter(string filename) {
            XmlTextWriter wr = new XmlTextWriter(filename, Encoding.Default);
            wr.Formatting = Formatting.Indented;
            wr.Indentation = 2;
            wr.IndentChar = ' ';
            wr.Namespaces = true;
            wr.WriteStartDocument(); //XML PI
            return wr;
        }
        private static XmlDocument FileToDOM(string filename) {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            return doc;
        }

    }

}
