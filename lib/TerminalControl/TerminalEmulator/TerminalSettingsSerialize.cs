/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSettingsSerialize.cs,v 1.4 2012/03/17 14:34:25 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#if UNITTEST
using System.IO;
using NUnit.Framework;
#endif

using Poderosa.View;
using Poderosa.Plugins;
using Poderosa.ConnectionParam;
using Poderosa.Serializing;
using Poderosa.Util;

namespace Poderosa.Terminal {
    //NOTE ログ設定はシリアライズしない。既存ファイルの上書きの危険などあり、ろくなことがないだろう

    internal class TerminalSettingsSerializer : ISerializeServiceElement {
        private ISerializeService _serializeService;
        public TerminalSettingsSerializer(IPluginManager pm) {
            _serializeService = (ISerializeService)pm.FindPlugin("org.poderosa.core.serializing", typeof(ISerializeService));
        }
#if UNITTEST
        public TerminalSettingsSerializer() {
        }
#endif

        public Type ConcreteType {
            get {
                return typeof(TerminalSettings);
            }
        }


        public StructuredText Serialize(object obj) {
            StructuredText storage = new StructuredText(this.ConcreteType.FullName);
            TerminalSettings ts = (TerminalSettings)obj;

            storage.Set("encoding", ts.Encoding.ToString());
            if (ts.TerminalType != TerminalType.XTerm)
                storage.Set("terminal-type", ts.TerminalType.ToString());
            if (ts.LocalEcho)
                storage.Set("localecho", "true");
            if (ts.LineFeedRule != LineFeedRule.Normal)
                storage.Set("linefeedrule", ts.LineFeedRule.ToString());
            if (ts.TransmitNL != NewLine.CR)
                storage.Set("transmit-nl", ts.TransmitNL.ToString());
            if (ts.EnabledCharTriggerIntelliSense)
                storage.Set("char-trigger-intellisense", "true");
            if (!ts.ShellScheme.IsGeneric)
                storage.Set("shellscheme", ts.ShellScheme.Name);
            storage.Set("caption", ts.Caption);
#if !UNITTEST
            //現在テストではRenderProfileは対象外
            if (!ts.UsingDefaultRenderProfile)
                storage.AddChild(_serializeService.Serialize(ts.RenderProfile));
#endif
            //アイコンはシリアライズしない
            return storage;
        }

        public object Deserialize(StructuredText node) {
            TerminalSettings ts = new TerminalSettings();
            ts.BeginUpdate();

            ts.Encoding = ParseEncodingType(node.Get("encoding", ""), EncodingType.ISO8859_1);
            ts.TerminalType = ParseUtil.ParseEnum<TerminalType>(node.Get("terminal-type"), TerminalType.XTerm);
            ts.LocalEcho = ParseUtil.ParseBool(node.Get("localecho"), false);
            ts.LineFeedRule = ParseUtil.ParseEnum<LineFeedRule>(node.Get("linefeedrule"), LineFeedRule.Normal);
            ts.TransmitNL = ParseUtil.ParseEnum<NewLine>(node.Get("transmit-nl"), NewLine.CR);
            ts.EnabledCharTriggerIntelliSense = ParseUtil.ParseBool(node.Get("char-trigger-intellisense"), false);
            string shellscheme = node.Get("shellscheme", ShellSchemeCollection.DEFAULT_SCHEME_NAME);
            if (shellscheme.Length > 0)
                ts.SetShellSchemeName(shellscheme);
            ts.Caption = node.Get("caption", "");
#if !UNITTEST
            //現在テストではRenderProfileは対象外
            StructuredText rp = node.FindChild(typeof(RenderProfile).FullName);
            if (rp != null)
                ts.RenderProfile = _serializeService.Deserialize(rp) as RenderProfile;
#endif
            ts.EndUpdate();
            return ts;
        }

        private EncodingType ParseEncodingType(string text, EncodingType defaultValue) {
            if (text == null || text.Length == 0)
                return defaultValue;

            EncodingType enc = defaultValue;
            if (ParseUtil.TryParseEnum<EncodingType>(text, ref enc))
                return enc;

            // compare with the localized names for the backward compatibility.
            foreach (EnumListItem<EncodingType> item in EnumListItem<EncodingType>.GetListItems()) {
                if (text == item.ToString()) {
                    return item.Value;
                }
            }

            // accept "utf-8" as EncodingType.UTF8 for the backward compatibility.
            if (text == "utf-8")
                return EncodingType.UTF8;

            return defaultValue;
        }

    }

#if UNITTEST
    [TestFixture]
    public class TerminalSettingsSerializeTests {

        private TerminalSettingsSerializer _terminalSettingsSerializer;

        [TestFixtureSetUp]
        public void Init() {
            _terminalSettingsSerializer = new TerminalSettingsSerializer();
            EnumDescAttribute.AddResourceTable(typeof(EncodingType).Assembly, new StringResource("Poderosa.TerminalEmulator.strings", typeof(EncodingType).Assembly));
        }

        [Test]
        public void Test0() {
            TerminalSettings ts1 = new TerminalSettings();
            StructuredText storage = _terminalSettingsSerializer.Serialize(ts1);
            TerminalSettings ts2 = (TerminalSettings)_terminalSettingsSerializer.Deserialize(storage);

            Assert.AreEqual(EncodingType.EUC_JP, ts1.Encoding);
            Assert.AreEqual(false, ts1.LocalEcho);
            Assert.AreEqual(NewLine.CR, ts1.TransmitNL);
            Assert.AreEqual(LineFeedRule.Normal, ts1.LineFeedRule);
            Assert.AreEqual(TerminalType.XTerm, ts1.TerminalType);

            Assert.AreEqual(EncodingType.EUC_JP, ts2.Encoding);
            Assert.AreEqual(false, ts2.LocalEcho);
            Assert.AreEqual(NewLine.CR, ts2.TransmitNL);
            Assert.AreEqual(LineFeedRule.Normal, ts2.LineFeedRule);
            Assert.AreEqual(TerminalType.XTerm, ts2.TerminalType);
        }

        [Test]
        public void Test1() {
            TerminalSettings ts1 = new TerminalSettings();
            ts1.BeginUpdate();
            ts1.Encoding = EncodingType.SHIFT_JIS;
            ts1.LocalEcho = true;
            ts1.TransmitNL = NewLine.CRLF;
            ts1.TerminalType = TerminalType.VT100;
            ts1.EndUpdate();

            StructuredText storage = _terminalSettingsSerializer.Serialize(ts1);
            //確認
            StringWriter wr = new StringWriter();
            new TextStructuredTextWriter(wr).Write(storage);
            wr.Close();
            Debug.WriteLine(wr.ToString());

            TerminalSettings ts2 = (TerminalSettings)_terminalSettingsSerializer.Deserialize(storage);

            Assert.AreEqual(ts1.Encoding, ts2.Encoding);
            Assert.AreEqual(ts1.LocalEcho, ts2.LocalEcho);
            Assert.AreEqual(ts1.TransmitNL, ts2.TransmitNL);
            Assert.AreEqual(ts1.LineFeedRule, ts2.LineFeedRule);
            Assert.AreEqual(ts1.TerminalType, ts2.TerminalType);
        }

        [Test]
        public void Test2() {
            StringReader reader = new StringReader("Poderosa.Terminal.TerminalSettings {\r\n encoding=xxx\r\n localecho=xxx\r\n transmit-nl=xxx}");
            StructuredText storage = new TextStructuredTextReader(reader).Read();

            TerminalSettings ts = (TerminalSettings)_terminalSettingsSerializer.Deserialize(storage);
            Assert.AreEqual(EncodingType.ISO8859_1, ts.Encoding);
            Assert.AreEqual(false, ts.LocalEcho);
            Assert.AreEqual(NewLine.CR, ts.TransmitNL);
            Assert.AreEqual(LineFeedRule.Normal, ts.LineFeedRule);
            Assert.AreEqual(TerminalType.XTerm, ts.TerminalType);
        }
    }
#endif
}
