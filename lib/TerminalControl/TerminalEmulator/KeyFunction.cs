/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: KeyFunction.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
#if UNITTEST
using NUnit.Framework;
#endif

using Poderosa.Util;

namespace Poderosa.Terminal {

    //繰り上げて実装することにした、キーの割当のためのクラス。
    //典型的には、例えば 0x1Fの送信は Ctrl+_ だが、英語キーボードでは実際には Ctrl+Shift+- が必要であり、押しづらい。このあたりを解決する。
    //ついでに、文字列に対してバインドを可能にすれば、"ls -la"キーみたいなのを定義できる。
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class KeyFunction {
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public class Entry {
            private Keys _key;
            private string _data;

            public Keys Key {
                get {
                    return _key;
                }
            }
            public string Data {
                get {
                    return _data;
                }
            }

            public Entry(Keys key, string data) {
                _key = key;
                _data = data;
            }

            //0x形式も含めて扱えるように
            public string FormatData() {
                StringBuilder bld = new StringBuilder();
                foreach (char ch in _data) {
                    if (ch < ' ' || (int)ch == 0x7F) { //制御文字とdel
                        bld.Append("0x");
                        bld.Append(((int)ch).ToString("X2"));
                    }
                    else
                        bld.Append(ch);
                }
                return bld.ToString();
            }

            public static string ParseData(string s) {
                StringBuilder bld = new StringBuilder();
                int c = 0;
                while (c < s.Length) {
                    char ch = s[c];
                    if (ch == '0' && c + 3 <= s.Length && s[c + 1] == 'x') { //0x00形式。
                        int t;
                        if (Int32.TryParse(s.Substring(c + 2, 2), NumberStyles.HexNumber, null, out t)) {
                            bld.Append((char)t);
                        }
                        c += 4;
                    }
                    else {
                        bld.Append(ch);
                        c++;
                    }
                }

                return bld.ToString();
            }

        }

        private List<Entry> _elements;

        public KeyFunction() {
            _elements = new List<Entry>();
        }

        internal FixedStyleKeyFunction ToFixedStyle() {
            Keys[] keys = new Keys[_elements.Count];
            char[][] datas = new char[_elements.Count][];
            for (int i = 0; i < _elements.Count; i++) {
                keys[i] = _elements[i].Key;
                datas[i] = _elements[i].Data.ToCharArray();
            }

            FixedStyleKeyFunction r = new FixedStyleKeyFunction(keys, datas);
            return r;
        }

        public string Format() {
            StringBuilder bld = new StringBuilder();
            foreach (Entry e in _elements) {
                if (bld.Length > 0)
                    bld.Append(", ");
                bld.Append(Poderosa.UI.GMenuItem.FormatShortcut(e.Key));
                bld.Append("=");
                bld.Append(e.FormatData());
            }
            return bld.ToString();
        }

        public static KeyFunction Parse(string format) {
            string[] elements = format.Split(',');
            KeyFunction f = new KeyFunction();
            foreach (string e in elements) {
                int eq = e.IndexOf('=');
                if (eq != -1) {
                    string keypart = e.Substring(0, eq).Trim();
                    f._elements.Add(new Entry(WinFormsUtil.ParseKey(keypart.Split('+')), Entry.ParseData(e.Substring(eq + 1))));
                }
            }
            return f;
        }
    }

    internal class FixedStyleKeyFunction {
        public Keys[] _keys;
        public char[][] _datas;

        public FixedStyleKeyFunction(Keys[] keys, char[][] data) {
            _keys = keys;
            _datas = data;
        }
    }

#if UNITTEST
    [TestFixture]
    public class KeyFunctionTests {
        [Test]
        public void SingleChar1() {
            KeyFunction f = KeyFunction.Parse("C=0x03");
            FixedStyleKeyFunction fs = f.ToFixedStyle();
            Assert.AreEqual(1, fs._keys.Length);
            Assert.AreEqual(1, fs._datas.Length);
            Assert.AreEqual(Keys.C, fs._keys[0]);
            Assert.AreEqual(1, fs._datas[0].Length);
            Assert.AreEqual(3, (int)fs._datas[0][0]);
        }

        [Test]
        public void SingleChar2() {
            KeyFunction f = KeyFunction.Parse("Ctrl+6=0x1F");
            FixedStyleKeyFunction fs = f.ToFixedStyle();
            Assert.AreEqual(1, fs._keys.Length);
            Assert.AreEqual(1, fs._datas.Length);
            Assert.AreEqual(Keys.Control | Keys.D6, fs._keys[0]);
            Assert.AreEqual(1, fs._datas[0].Length);
            Assert.AreEqual(31, (int)fs._datas[0][0]);
        }

        [Test]
        public void String1() {
            KeyFunction f = KeyFunction.Parse("Ctrl+Question=0x010x020x1F0x7F");
            FixedStyleKeyFunction fs = f.ToFixedStyle();
            Assert.AreEqual(1, fs._keys.Length);
            Assert.AreEqual(1, fs._datas.Length);
            Assert.AreEqual(Keys.Control | Keys.OemQuestion, fs._keys[0]);
            Assert.AreEqual(4, fs._datas[0].Length);
            Assert.AreEqual(2, (int)fs._datas[0][1]);
            Assert.AreEqual(127, (int)fs._datas[0][3]);

            Assert.AreEqual("Ctrl+OemQuestion=0x010x020x1F0x7F", f.Format());
        }
        [Test]
        public void String2() {
            KeyFunction f = KeyFunction.Parse("Ctrl+Shift+L=ls -la");
            FixedStyleKeyFunction fs = f.ToFixedStyle();
            Assert.AreEqual(1, fs._keys.Length);
            Assert.AreEqual(1, fs._datas.Length);
            Assert.AreEqual(Keys.Control | Keys.Shift | Keys.L, fs._keys[0]);
            Assert.AreEqual("ls -la", fs._datas[0]);

            Assert.AreEqual("Ctrl+Shift+L=ls -la", f.Format());
        }
        [Test]
        public void Multi1() {
            KeyFunction f = KeyFunction.Parse("Ctrl+Shift+L=ls -la, Ctrl+Shift+F=find -name");
            FixedStyleKeyFunction fs = f.ToFixedStyle();
            Assert.AreEqual(2, fs._keys.Length);
            Assert.AreEqual(2, fs._datas.Length);
            Assert.AreEqual(Keys.Control | Keys.Shift | Keys.L, fs._keys[0]);
            Assert.AreEqual(Keys.Control | Keys.Shift | Keys.F, fs._keys[1]);
            Assert.AreEqual("ls -la", fs._datas[0]);
            Assert.AreEqual("find -name", fs._datas[1]);

            Assert.AreEqual("Ctrl+Shift+L=ls -la, Ctrl+Shift+F=find -name", f.Format());
        }
    }
#endif

}
