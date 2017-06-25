/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: StructuredText.cs,v 1.3 2012/05/20 09:10:30 kzmi Exp $
 */
using System;
using System.IO;
using System.Collections;

/*
    [SPEC] 設定の基本原則
    全体はツリーを構成し、デリミタは . 。
    極力デフォルト値を使う。そうすればファイルのパース時間を短縮できる。
    name-valueは同一名の存在許さず。これはXML形式との整合性のため

    まあニセXMLですよこれは。パースが早いだけで。

 */

using Poderosa.Util.Collections;

namespace Poderosa {

    /// <summary>
    /// </summary>
    /// <exclude/>
    public class StructuredText : ICloneable {
        private StructuredText _parent; //null in case of root
        private string _name;
        private ArrayList _children;

        //外部からセットできる他、状態変更があったら立つフラグ
        private bool _dirtyFlag;

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public class Entry : ICloneable {
            public string name;
            public string value;

            public Entry(string n, string v) {
                name = n;
                value = v;
            }

            public object Clone() {
                return new Entry(name, value);
            }
        }

        public string Name {
            get {
                return _name;
            }
        }
        public StructuredText Parent {
            get {
                return _parent;
            }
        }
        public string FullName {
            get {
                return _parent == null ? _name : _parent.FullName + "." + _name;
            }
        }
        public int ChildCount {
            get {
                return _children.Count;
            }
        }
        public bool IsDirty {
            get {
                return _dirtyFlag;
            }
            set {
                _dirtyFlag = value;
            }
        }


        //values
        public string Get(string name) {
            return Get(name, null);
        }
        public string Get(string name, string defval) {
            Entry e = FindEntry(name);
            return e == null ? defval : e.value;
        }
        public void Set(string name, string value) {
            _dirtyFlag = true;

            _children.Add(new Entry(name, value));
        }
        public void SetOrReplace(string name, string value) {
            _dirtyFlag = true;

            Entry e = FindEntry(name);
            if (e == null)
                _children.Add(new Entry(name, value));
            else
                e.value = value;
        }
        public void SetByIndex(int index, string value) {
            Entry e = GetEntryOrNull(index);
            if (e == null)
                throw new ArgumentException("the entry is not found");
            e.value = value;
        }
        public void ClearValue(string name) {
            _dirtyFlag = true;

            for (int i = 0; i < _children.Count; i++) {
                Entry e = _children[i] as Entry;
                if (e != null && e.name == name) {
                    _children.RemoveAt(i);
                    return;
                }
            }
        }
        public void Clear() {
            _dirtyFlag = true;

            _children.Clear();
        }

        public Entry FindEntry(string name) {
            foreach (object o in _children) {
                Entry e = o as Entry;
                if (e != null && e.name == name)
                    return e;
            }
            return null;
        }

        public Entry GetEntryOrNull(int index) {
            return index < _children.Count ? _children[index] as Entry : null;
        }
        public StructuredText GetChildOrNull(int index) {
            return index < _children.Count ? _children[index] as StructuredText : null;
        }

        //Entry or child nodes
        public IEnumerable Children {
            get {
                return _children;
            }
        }
        // child nodes
        //子孫まで一気に作成もOK
        public StructuredText GetOrCreateChild(string name) {
            _dirtyFlag = true;

            int comma = name.IndexOf('.');
            string local = comma == -1 ? name : name.Substring(0, comma);
            StructuredText n = FindChild(local);
            if (n == null) {
                n = new StructuredText(this, local);
                _children.Add(n);
            }
            return comma == -1 ? n : n.GetOrCreateChild(name.Substring(comma + 1));
        }
        public StructuredText AddChild(string name) {
            _dirtyFlag = true;

            StructuredText n = new StructuredText(this, name);
            _children.Add(n);
            return n;
        }
        public StructuredText AddChild(StructuredText child) {
            _dirtyFlag = true;
            child._parent = this;
            _children.Add(child);
            return this;
        }
        public void RemoveChild(StructuredText child) {
            _dirtyFlag = true;

            _children.Remove(child);
        }

        public StructuredText FindChild(string name) {
            foreach (object o in _children) {
                StructuredText n = o as StructuredText;
                if (n != null && n.Name == name)
                    return n;
            }
            return null;
        }
        //TODO 実はこの戻りをまたコレクションにコピーすることがよくある。むだだ
        public IList FindMultipleNote(string name) {
            ArrayList r = new ArrayList();
            foreach (object o in _children) {
                StructuredText n = o as StructuredText;
                if (n != null && n.Name == name)
                    r.Add(n);
            }
            return r;
        }
        public IList FindMultipleEntries(string name) {
            ArrayList r = new ArrayList();
            foreach (object o in _children) {
                Entry e = o as Entry;
                if (e != null && e.name == name)
                    r.Add(e.value); //string collection返しだよ
            }
            return r;
        }

        /// <summary>
        /// ディープコピーするが、親からは切り離される
        /// </summary>
        public object Clone() {
            StructuredText np = new StructuredText(null, _name);
            np._children = CollectionUtil.DeepCopyArrayList(_children);
            return np;
        }

        //note that assembly private
        internal StructuredText(StructuredText parent, string name) {
            _name = name;
            _parent = parent;
            _children = new ArrayList();
        }


        //constructor with empty content    
        public StructuredText(string name) {
            _name = name;
            _children = new ArrayList();
        }

    }

    //Reader, Writer
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public abstract class StructuredTextReader {
        public abstract StructuredText Read();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public abstract class StructuredTextWriter {
        public abstract void Write(StructuredText node);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TextStructuredTextReader : StructuredTextReader {

        private TextReader _reader;
        private StructuredText _current;
        private StructuredText _root;

        public TextStructuredTextReader(TextReader reader) {
            _reader = reader;
        }

        public override StructuredText Read() {
            _current = null;
            _root = null;
            string line = _reader.ReadLine();
            while (line != null) {
                ProcessLine(CutPrecedingSpace(line));
                line = _reader.ReadLine();
            }
            return _root;
        }
        private void ProcessLine(string line) {
            if (line.Length == 0 || line[0] == '#')
                return; //comment line

            int e = line.IndexOf('=');
            if (e != -1 && _current != null) {
                string name0 = CutPrecedingSpace(line.Substring(0, e));
                string value = e == line.Length - 1 ? "" : CutPrecedingSpace(line.Substring(e + 1));
                _current.Set(name0, value);
            }
            else if (line[line.Length - 1] == '{') {
                string name = line.Substring(0, line.IndexOf(' '));
                _current = _current == null ? new StructuredText(null, name) : _current.AddChild(name);
                if (_root == null)
                    _root = _current; //最初のNodeをルートとする
            }
            else if (line[line.Length - 1] == '}') {
                _current = _current.Parent;
            }
        }

        private static string CutPrecedingSpace(string s) {
            int i = 0;
            do {
                if (i == s.Length)
                    return "";
                char ch = s[i++];
                if (ch != ' ' && ch != '\t')
                    return s.Substring(i - 1);
            } while (true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TextStructuredTextWriter : StructuredTextWriter {
        private TextWriter _writer;
        private int _indent;
        public const int INDENT_UNIT = 2;

        public TextStructuredTextWriter(TextWriter writer) {
            _writer = writer;
        }

        public override void Write(StructuredText node) {
            _indent = 0;
            WriteNode(node);
        }

        private void WriteNode(StructuredText node) {
            WriteIndent();
            _writer.Write(node.Name);
            _writer.WriteLine(" {");
            _indent += INDENT_UNIT;

            foreach (object ch in node.Children) {
                StructuredText.Entry e = ch as StructuredText.Entry;
                if (e != null) { //entry
                    WriteIndent();
                    _writer.Write(e.name);
                    _writer.Write('=');
                    _writer.WriteLine(e.value);
                }
                else { //child node
                    WriteNode((StructuredText)ch);
                }
            }

            _indent -= INDENT_UNIT;
            WriteIndent();
            _writer.WriteLine("}");
        }
        private void WriteIndent() {
            for (int i = 0; i < _indent; i++)
                _writer.Write(' ');
        }

    }

}
