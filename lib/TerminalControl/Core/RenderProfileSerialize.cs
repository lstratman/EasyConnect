/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: RenderProfileSerialize.cs,v 1.6 2012/05/27 15:22:50 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

#if UNITTEST
using System.IO;
using NUnit.Framework;
#endif

using Poderosa.Serializing;

namespace Poderosa.View {
    internal class RenderProfileSerializer : ISerializeServiceElement {
        public Type ConcreteType {
            get {
                return typeof(RenderProfile);
            }
        }

        public StructuredText Serialize(object obj) {
            StructuredText storage = new StructuredText(typeof(RenderProfile).FullName);
            RenderProfile prof = (RenderProfile)obj;
            storage.Set("font-name", prof.FontName);
            storage.Set("cjk-font-name", prof.CJKFontName);
            storage.Set("font-size", prof.FontSize.ToString());
            storage.Set("line-spacing", prof.LineSpacing.ToString());
            if (prof.UseClearType)
                storage.Set("clear-type", "true");
            if (!prof.EnableBoldStyle)
                storage.Set("enable-bold-style", "false");
            if (prof.ForceBoldStyle)
                storage.Set("force-bold-style", "true");
            storage.Set("text-color", prof.ForeColor.Name);
            storage.Set("back-color", prof.BackColor.Name);
            if (prof.BackgroundImageFileName.Length > 0) {
                storage.Set("back-image", prof.BackgroundImageFileName);
                storage.Set("back-style", prof.ImageStyle.ToString());
            }
            if (!prof.ESColorSet.IsDefault)
                storage.Set("escape-sequence-color", prof.ESColorSet.Format());
            storage.Set("darken-escolor-for-background", prof.DarkenEsColorForBackground.ToString());
            return storage;
        }

        public object Deserialize(StructuredText storage) {
            RenderProfile prof = new RenderProfile();
            prof.FontName = storage.Get("font-name", "Courier New");
            prof.CJKFontName = storage.Get("cjk-font-name",
                               storage.Get("japanese-font-name",
                               storage.Get("chinese-font-name", "Courier New")));
            prof.FontSize = ParseUtil.ParseFloat(storage.Get("font-size"), 10.0f);
            prof.LineSpacing = ParseUtil.ParseInt(storage.Get("line-spacing"), 0);
            prof.UseClearType = ParseUtil.ParseBool(storage.Get("clear-type"), false);
            prof.EnableBoldStyle = ParseUtil.ParseBool(storage.Get("enable-bold-style"), true);
            prof.ForceBoldStyle = ParseUtil.ParseBool(storage.Get("force-bold-style"), false);
            prof.ForeColor = ParseUtil.ParseColor(storage.Get("text-color"), Color.FromKnownColor(KnownColor.WindowText));
            prof.BackColor = ParseUtil.ParseColor(storage.Get("back-color"), Color.FromKnownColor(KnownColor.Window));
            prof.ImageStyle = ParseUtil.ParseEnum<ImageStyle>(storage.Get("back-style"), ImageStyle.Center);
            prof.BackgroundImageFileName = storage.Get("back-image", "");

            prof.ESColorSet = new EscapesequenceColorSet();
            string escolor = storage.Get("escape-sequence-color");
            if (escolor != null)
                prof.ESColorSet.Load(escolor);
            prof.DarkenEsColorForBackground = ParseUtil.ParseBool(storage.Get("darken-escolor-for-background"), true);

            return prof;
        }
    }

#if UNITTEST
    [TestFixture]
    public class RenderProfileSerializeTests {

        private RenderProfileSerializer _renderProfileSerializer;

        [TestFixtureSetUp]
        public void Init() {
            _renderProfileSerializer = new RenderProfileSerializer();
        }

        [Test]
        public void Test1() {
            RenderProfile prof1 = new RenderProfile();
            prof1.FontName = "console";
            prof1.JapaneseFontName = "ＭＳ ゴシック";
            prof1.UseClearType = true;
            prof1.FontSize = 12;
            prof1.BackColor = Color.FromKnownColor(KnownColor.Yellow);
            prof1.ForeColor = Color.FromKnownColor(KnownColor.White);
            prof1.BackgroundImageFileName = "image-file";
            prof1.ImageStyle = ImageStyle.Scaled;
            prof1.ESColorSet = new EscapesequenceColorSet();
            prof1.ESColorSet[1] = Color.Pink;

            StructuredText storage = _renderProfileSerializer.Serialize(prof1);
            //確認
            StringWriter wr = new StringWriter();
            new TextStructuredTextWriter(wr).Write(storage);
            wr.Close();
            Debug.WriteLine(wr.ToString());

            RenderProfile prof2 = (RenderProfile)_renderProfileSerializer.Deserialize(storage);

            Assert.AreEqual(prof1.FontName, prof2.FontName);
            Assert.AreEqual(prof1.JapaneseFontName, prof2.JapaneseFontName);
            Assert.AreEqual(prof1.UseClearType, prof2.UseClearType);
            Assert.AreEqual(prof1.FontSize, prof2.FontSize);
            Assert.AreEqual(prof1.BackColor.Name, prof2.BackColor.Name);
            Assert.AreEqual(prof1.ForeColor.Name, prof2.ForeColor.Name);
            Assert.AreEqual(prof1.BackgroundImageFileName, prof2.BackgroundImageFileName);
            Assert.AreEqual(prof1.ImageStyle, prof2.ImageStyle);
            Assert.AreEqual(prof1.ESColorSet.Format(), prof2.ESColorSet.Format());

        }

    }
#endif
}
