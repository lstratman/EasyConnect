/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: StructuredTextT.cs,v 1.1 2010/11/19 15:40:51 kzmi Exp $
 */
#if UNITTEST
using System;
using System.Collections;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace Poderosa
{
    [TestFixture]
    public class StructuredTextTests {

        private string Dump(StructuredText node) {
            StringWriter w = new StringWriter();
            new TextStructuredTextWriter(w).Write(node);
            w.Close();
            return w.ToString();
        }
        private StructuredText CreateRoot() {
            StringReader r = new StringReader("Poderosa {\r\n}");
            return new TextStructuredTextReader(r).Read();
        }

        [Test]
        public void Test0_Preliminary() {
            Assert.AreEqual(2, TextStructuredTextWriter.INDENT_UNIT); //これが失敗すると他のテストケースのインデント調整が必要
        }

        [Test]
        public void Test1_Values1() {
            StructuredText r = CreateRoot();
            r.Set("A", "B");
            Assert.AreEqual("Poderosa {\r\n  A=B\r\n}\r\n", Dump(r));
        }

        [Test]
        public void Test1_Values2() {
            StructuredText r = CreateRoot();
            r.Set("A", "B");
            Assert.AreEqual("B", r.Get("A"));
            Assert.IsNull(r.Get("Q"));
            Assert.AreEqual("V", r.Get("Q", "V"));
            r.Set("A", "C"); //duplicated

            Assert.AreEqual("Poderosa {\r\n  A=B\r\n  A=C\r\n}\r\n", Dump(r));
        }

        [Test]
        public void Test1_Values3() {
            StructuredText r = CreateRoot();
            r.Set("A", "B");
            Assert.AreEqual("B", r.Get("A"));
            Assert.IsNull(r.Get("Q"));
            Assert.AreEqual("V", r.Get("Q", "V"));
            r.SetOrReplace("A", "C");

            Assert.AreEqual("Poderosa {\r\n  A=C\r\n}\r\n", Dump(r));
        }

        [Test]
        public void Test1_Values4() {
            StructuredText r = CreateRoot();
            r.Set("A", "P");
            r.Set("B", "Q");
            r.Set("C", "R");
            r.ClearValue("B");
            Assert.AreEqual("Poderosa {\r\n  A=P\r\n  C=R\r\n}\r\n", Dump(r));
        }

        [Test]
        public void Test2_Nodes1() {
            StructuredText r = CreateRoot();
            StructuredText c1 = r.GetOrCreateChild("C");
            Assert.AreEqual("C", c1.Name);
            Assert.AreSame(r, c1.Parent);

            StructuredText c2 = r.AddChild("C");
            Assert.AreSame(r, c2.Parent);

            Assert.AreSame(c1, r.FindChild("C")); //must be the first child
            IList il = r.FindMultipleNote("C");
            Assert.AreEqual(2, il.Count);
            Assert.AreSame(c2, il[1]);
        }

        [Test]
        public void Test2_Nodes2() {
            StructuredText r = CreateRoot();
            StructuredText c = r.GetOrCreateChild("XXX.YYY.ZZZ");
            Assert.AreEqual("ZZZ", c.Name);
            Assert.AreEqual("YYY", c.Parent.Name);
            Assert.AreEqual("XXX", c.Parent.Parent.Name);
            Assert.AreSame(r, c.Parent.Parent.Parent);
            Assert.AreSame(c, r.GetOrCreateChild("XXX.YYY.ZZZ"));
        }

        [Test]
        public void Test3_Parse1() {
            StructuredText r = CreateRoot();
            StructuredText z = r.GetOrCreateChild("XXX.YYY.ZZZ");
            z.Set("A", "B");
            z.Set("C", "D");
            StructuredText z2 = z.Parent.AddChild("ZZZ");
            z2.Set("E", "F");

            //一回変換してOK？
            StructuredText r2 = new TextStructuredTextReader(new StringReader(Dump(r))).Read();
            Assert.AreEqual(Dump(r), Dump(r2));
        }

        [Test]
        public void Test4_Clone() {
            StructuredText r = CreateRoot();
            StructuredText y = r.GetOrCreateChild("XXX.YYY");
            y.Set("A", "B");

            StructuredText nx = (StructuredText)y.Parent.Clone();
            nx.FindChild("YYY").SetOrReplace("A", "C");
            Assert.IsNull(nx.Parent);
            Assert.AreEqual("XXX {\r\n  YYY {\r\n    A=C\r\n  }\r\n}\r\n", Dump(nx));
            Assert.AreEqual("YYY {\r\n  A=B\r\n}\r\n", Dump(y));
        }
    }
}
#endif
