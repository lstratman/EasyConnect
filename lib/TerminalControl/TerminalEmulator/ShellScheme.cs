/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ShellScheme.cs,v 1.4 2011/10/27 23:21:58 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Poderosa.Preferences;
using Poderosa.Util.Collections;

#if UNITTEST
using NUnit.Framework;
#endif

namespace Poderosa.Terminal {

    internal class ShellSchemeCollection : IShellSchemeCollection, IPreferenceSupplier {
        public const string DEFAULT_SCHEME_NAME = "generic";

        private List<IShellScheme> _data;
        private List<IShellSchemeDynamicChangeListener> _listeners;
        private IShellScheme _defaultScheme;

        private IPreferenceFolderArray _preferenceFolderArray;
        private IPreferenceFolder _schemeTemplate;
        private IStringPreferenceItem _namePreference;
        private IStringPreferenceItem _promptPreference;
        private IStringPreferenceItem _backspacePreference;
        private IStringPreferenceItem _commandListPreference;

        public ShellSchemeCollection() {
            _data = new List<IShellScheme>();
            _listeners = new List<IShellSchemeDynamicChangeListener>();
        }
        public IShellScheme DefaultScheme {
            get {
                return _defaultScheme;
            }
        }
        #region IPreferenceSupplier
        public string PreferenceID {
            get {
                return "org.poderosa.terminalemulator.shellscheme";
            }
        }

        public void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
            _schemeTemplate = builder.DefineFolderArray(folder, this, "scheme");
            _preferenceFolderArray = folder.FindChildFolderArray("scheme");
            Debug.Assert(_preferenceFolderArray != null);
            _namePreference = builder.DefineStringValue(_schemeTemplate, "name", "", null);
            _promptPreference = builder.DefineStringValue(_schemeTemplate, "prompt", GenericShellScheme.DEFAULT_PROMPT_REGEX, null);
            _backspacePreference = builder.DefineStringValue(_schemeTemplate, "backspace", "", null);
            _commandListPreference = builder.DefineStringValue(_schemeTemplate, "commands", "", null);
        }

        public object QueryAdapter(IPreferenceFolder folder, Type type) {
            return null;
        }

        public string GetDescription(IPreferenceItem item) {
            return null;
        }

        public void ValidateFolder(IPreferenceFolder folder, IPreferenceValidationResult output) {
        }
        #endregion

        public IShellScheme FindShellScheme(string name) {
            foreach (IShellScheme ss in _data)
                if (ss.Name == name)
                    return ss;
            return null;
        }

        #region IShellSchemeCollection
        public IShellScheme FindShellSchemeOrDefault(string name) {
            foreach (IShellScheme ss in _data)
                if (ss.Name == name)
                    return ss;
            return _defaultScheme;
        }
        public int IndexOf(IShellScheme ss) {
            return _data.IndexOf(ss);
        }
        public IShellScheme GetAt(int index) {
            return _data[index];
        }

        public IEnumerable<IShellScheme> Items {
            get {
                return _data;
            }
        }
        public void UpdateAll(IShellScheme[] values, TypedHashtable<IShellScheme, IShellScheme> table) {
            _data.Clear();
            foreach (IShellScheme ss in values) {
                _data.Add(ss);
                if (ss.IsGeneric)
                    _defaultScheme = ss;
            }

            //変更通知 これは_dataの更新後でないと、ハンドラから再検索等きたときに困る
            foreach (IShellSchemeDynamicChangeListener l in _listeners)
                l.OnShellSchemeCollectionChanged(values, table);

        }
        public IShellScheme CreateEmptyScheme(string name) {
            return new GenericShellScheme(name, GenericShellScheme.DEFAULT_PROMPT_REGEX);
        }
        public void AddDynamicChangeListener(IShellSchemeDynamicChangeListener listener) {
            _listeners.Add(listener);
        }
        public void RemoveDynamicChangeListener(IShellSchemeDynamicChangeListener listener) {
            _listeners.Remove(listener);
        }

        #endregion

        public void Load() {
            _data.Clear();
            IPreferenceFolder[] folders = _preferenceFolderArray.Folders;
            foreach (IPreferenceFolder content in folders) {
                GenericShellScheme ss = new GenericShellScheme(
                    _preferenceFolderArray.ConvertItem(content, _namePreference).AsString().Value,
                    _preferenceFolderArray.ConvertItem(content, _promptPreference).AsString().Value);
                string bs = _preferenceFolderArray.ConvertItem(content, _backspacePreference).AsString().Value;
                if (bs == "7F")
                    ss.BackSpaceChar = ss.BackSpaceChar = (char)0x7F; //TODO パースが手抜き
                ss.SetCommandList(_preferenceFolderArray.ConvertItem(content, _commandListPreference).AsString().Value);
                _data.Add(ss);
            }

            _defaultScheme = FindShellScheme(DEFAULT_SCHEME_NAME) as GenericShellScheme;

            if (_defaultScheme == null) {
                _defaultScheme = new GenericShellScheme(DEFAULT_SCHEME_NAME, GenericShellScheme.DEFAULT_PROMPT_REGEX); //なければこれで
                _data.Add(_defaultScheme);
            }
        }

        public void PreClose() {
            _preferenceFolderArray.Clear();
            foreach (GenericShellScheme ss in _data) {
                IPreferenceFolder content = _preferenceFolderArray.CreateNewFolder();
                _preferenceFolderArray.ConvertItem(content, _namePreference).AsString().Value = ss.Name;
                _preferenceFolderArray.ConvertItem(content, _promptPreference).AsString().Value = ss.PromptExpression;
                _preferenceFolderArray.ConvertItem(content, _commandListPreference).AsString().Value = ss.FormatCommandList();
                if (ss.BackSpaceChar != (char)0x08)
                    _preferenceFolderArray.ConvertItem(content, _backspacePreference).AsString().Value = ((int)ss.BackSpaceChar).ToString("X2");
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

    internal class GenericShellScheme : IShellScheme {
        public const string DEFAULT_PROMPT_REGEX = "^[^>()$#%]*[>()$#%]";

        private StringBuilder _buffer;
        private string _name;
        private string _promptExpression;
        private string _commandList; //遅延評価用
        private char _backSpaceChar;
        private IntelliSenseItemCollection _intelliSenseItemCollection;

        public GenericShellScheme(string name, string prompt) {
            _name = name;
            _promptExpression = prompt;
            _backSpaceChar = (char)0x08;
            _buffer = new StringBuilder();
            _commandList = "";
            _intelliSenseItemCollection = new IntelliSenseItemCollection();
        }
        public string PromptExpression {
            get {
                return _promptExpression;
            }
            set {
                _promptExpression = value;
            }
        }
        public char DefaultDelimiter {
            get {
                return ' ';
            }
        }
        public char BackSpaceChar {
            get {
                return _backSpaceChar;
            }
            set {
                _backSpaceChar = value;
            }
        }
        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }
        public bool IsGeneric {
            get {
                return _name == ShellSchemeCollection.DEFAULT_SCHEME_NAME;
            }
        }
        public IIntelliSenseItemCollection CommandHistory {
            get {
                if (_commandList != null) {
                    ParseCommandList(_commandList);
                    _commandList = null;
                }
                return _intelliSenseItemCollection;
            }
        }
        public IShellScheme Clone() {
            GenericShellScheme ns = new GenericShellScheme(_name, _promptExpression);
            ns._backSpaceChar = _backSpaceChar;
            ns._commandList = _commandList;
            ns._intelliSenseItemCollection = _intelliSenseItemCollection.Clone();
            return ns;
        }

        public bool IsDelimiter(char ch) {
            return ch == ' ';
        }
        public string[] ParseCommandInput(string src) {
            List<string> result = new List<string>();
            int c = 0;
            while (c < src.Length) {
                string r = ParseWord(src, ref c);
                if (r.Length > 0)
                    result.Add(r);
            }

            return result.ToArray();
        }

        public string ParseFirstCommand(string src, out bool is_partial) {
            int c = 0;
            string r = ParseWord(src, ref c);
            is_partial = c < src.Length;
            return r;
        }

        //srcのcursor文字目からパースして１コマンド分だけ返す
        private string ParseWord(string src, ref int cursor) {
            char quote = '\0'; //ダブルクオーテーションなどの囲み判定。囲まれていないときは\0

            _buffer.Remove(0, _buffer.Length);
            bool in_element = false;
            while (cursor < src.Length) {
                char ch = src[cursor];
                if (in_element) {
                    bool end = quote == '\0' ? IsDelimiter(ch) : quote == ch;
                    if (!end || quote != '\0')
                        _buffer.Append(ch); //囲みのあるときはそれを入れる

                    if (end) { //区切りが見つかったので即時リターン
                        cursor++;
                        return _buffer.ToString();
                    }
                }
                else {
                    bool start;
                    if (ch == '"' || ch == '`') {
                        start = true;
                        quote = ch;
                    }
                    else {
                        start = !IsDelimiter(ch);
                        quote = '\0';
                    }

                    if (start) {
                        _buffer.Append(ch);
                        in_element = true;
                    }
                }

                cursor++;
            }

            return _buffer.ToString();
        }

        //保存用
        public string FormatCommandList() {
            StringBuilder bld = new StringBuilder();
            foreach (IntelliSenseItem item in _intelliSenseItemCollection.Items) {
                if (bld.Length > 0)
                    bld.Append(';');

                string t = item.Format(this.DefaultDelimiter);
                if (t.IndexOf(';') != -1) { //ちょっと苦しいが
                    if (t[0] != '\\') {
                        if (t.IndexOf(']') == -1)
                            WriteEscaping(bld, '[', ']', t);
                        else if (t.IndexOf('>') == -1)
                            WriteEscaping(bld, '<', '>', t);
                        else if (t.IndexOf('}') == -1)
                            WriteEscaping(bld, '{', '}', t);
                    }
                    //これでもだめなやつは無視、まあいいだろう
                }
                else
                    bld.Append(t);
            }
            return bld.ToString();
        }
        private void WriteEscaping(StringBuilder bld, char start, char end, string value) {
            bld.Append('\\'); //エスケープマーク
            bld.Append(start);
            bld.Append(value);
            bld.Append(end);
        }
        public void SetCommandList(string value) {
            _commandList = value; //遅延パース
        }
        private void ParseCommandList(string value) {
            Debug.WriteLineIf(DebugOpt.IntelliSense, "ParseCommand");
            _intelliSenseItemCollection.Clear();
            int cursor = 0;
            while (cursor < value.Length) {
                char mark = DetermineDelimiter(value, cursor);
                int delim = value.IndexOf(mark, cursor);
                if (delim == -1)
                    delim = value.Length;
                if (mark != ';')
                    cursor += 2; // "\\["等の分
                string command = value.Substring(cursor, delim - cursor);
                _intelliSenseItemCollection.AddLast(new IntelliSenseItem(this.ParseCommandInput(command)));
                cursor = delim + 1;
                if (mark != ';')
                    cursor++;
            }
        }

        private char DetermineDelimiter(string value, int cursor) {
            char ch = value[cursor];
            if (ch == '\\') {
                switch (value[cursor + 1]) {
                    case '[':
                        return ']';
                    case '<':
                        return '>';
                    case '{':
                        return '}';
                }
            }

            return ';';
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }
#if UNITTEST
    [TestFixture]
    public class ShellSchemeTests {
        [Test]
        public void ParseTests() {
            GenericShellScheme g = new GenericShellScheme("generic", "");
            Confirm(g.ParseCommandInput("a b c"), "a", "b", "c");
            Confirm(g.ParseCommandInput(" a  b  c "), "a", "b", "c");
            Confirm(g.ParseCommandInput("abc \"abc abc\""), "abc", "\"abc abc\"");
            Confirm(g.ParseCommandInput(" abc \"abc abc "), "abc", "\"abc abc ");
        }
        private void Confirm(string[] actual, params string[] expected) {
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < actual.Length; i++) {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [Test]
        public void CommandListTests1() {
            CommandListOne("a;b;c", "a", "b", "c");
            CommandListOne("\\[a;b];b;c", "a;b", "b", "c");
            CommandListOne("\\<a;[]b>;\\[a;b;c];c", "a;[]b", "a;b;c", "c");
        }
        private void CommandListOne(string input, params string[] expected) {
            GenericShellScheme g = new GenericShellScheme("generic", "");
            g.SetCommandList(input);
            IntelliSenseItemCollection col = (IntelliSenseItemCollection)g.CommandHistory;
            Confirm(col.ToStringArray(), expected);
            Assert.AreEqual(input, g.FormatCommandList()); //再フォーマット
        }

    }
#endif

}
