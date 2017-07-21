/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CommandManager.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

using Poderosa.Util.Collections;
using Poderosa.Plugins;
using Poderosa.Preferences;
using Poderosa.Util;

[assembly: PluginDeclaration(typeof(Poderosa.Commands.CommandManagerPlugin))]

namespace Poderosa.Commands {
    //NOTE publicに昇格？
    internal interface IKeyBindChangeListener {
        void OnKeyBindChanged(IKeyBinds newvalues);
    }

    [PluginInfo(ID = CommandManagerPlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.core.preferences")]
    internal class CommandManagerPlugin : PluginBase, ICommandManager, IPreferenceSupplier, IPreferenceChangeListener {
        public const string PLUGIN_ID = "org.poderosa.core.commands";
        private static CommandManagerPlugin _instance;

        private List<IGeneralCommand> _commands; //登録順に並べる
        private TypedHashtable<string, IGeneralCommand> _idToCommand;
        private KeyBindConfiguration _keyBind;
        private IPreferenceLooseNode _keyBindNode;

        private ListenerList<IKeyBindChangeListener> _keyBindChangeListener;

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;
            _commands = new List<IGeneralCommand>();
            _idToCommand = new TypedHashtable<string, IGeneralCommand>();
            _keyBind = new KeyBindConfiguration();
            _keyBindChangeListener = new ListenerList<IKeyBindChangeListener>();

            BasicCommandImplementation.Build();

            poderosa.PluginManager.FindExtensionPoint(PreferencePlugin.EXTENSIONPOINT_NAME).RegisterExtension(this);
        }

        public void Register(IGeneralCommand command) {
            string id = command.CommandID;
            if (id == null || id.Length == 0)
                throw new ArgumentException("command id must be defined");
            if (Find(id) != null)
                throw new ArgumentException(String.Format("command id {0} is duplicated", id)); //登録数が多いとオーバヘッドになりかねない

            _commands.Add(command);
            _idToCommand.Add(id, command);
        }

        //コマンド実行　後に実行のログを取るような仕組みがあるかも
        public CommandResult Execute(IPoderosaCommand command, ICommandTarget target, params IAdaptable[] args) {
            return command.InternalExecute(target, args);
        }

        public IGeneralCommand Find(string id) {
            return _idToCommand[id];
        }

        public IGeneralCommand Find(Keys key) {
            return _keyBind.FindCommand(key);
        }

        public IEnumerable<IGeneralCommand> Commands {
            get {
                return _commands;
            }
        }

        public IDefaultCommandCategories CommandCategories {
            get {
                return BasicCommandImplementation.DefaultCategories;
            }
        }

        public static CommandManagerPlugin Instance {
            get {
                return _instance;
            }
        }

        public string PreferenceID {
            get {
                return PLUGIN_ID;
            }
        }

        //TODO QueryAdapter経由にすべき？
        public IKeyBinds GetKeyBinds(IPreferenceFolder folder) {
            return GetKeyBindInternal(folder);
        }
        public IKeyBinds CurrentKeyBinds {
            get {
                return _keyBind;
            }
        }

        public void AddKeyBindChangeListener(IKeyBindChangeListener listener) {
            _keyBindChangeListener.Add(listener);
        }
        public void RemoveKeyBindChangeListener(IKeyBindChangeListener listener) {
            _keyBindChangeListener.Add(listener);
        }

        #region IPreferenceSupplier
        public void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
            _keyBindNode = builder.DefineLooseNode(folder, _keyBind, "keybinds");
            folder.AddChangeListener(this);
        }

        public object QueryAdapter(IPreferenceFolder folder, Type type) {
            return null;
        }

        public string GetDescription(IPreferenceItem item) {
            return "";
        }

        public void ValidateFolder(IPreferenceFolder folder, IPreferenceValidationResult output) {
        }
        #endregion

        #region IPreferenceChangeListener
        public void OnPreferenceImport(IPreferenceFolder oldvalues, IPreferenceFolder newvalues) {
            _keyBind = GetKeyBindInternal(newvalues);
            foreach (IKeyBindChangeListener l in _keyBindChangeListener)
                l.OnKeyBindChanged(_keyBind);
        }

        #endregion

        private KeyBindConfiguration GetKeyBindInternal(IPreferenceFolder folder) {
            Debug.Assert(folder.Id == PLUGIN_ID);
            IPreferenceLooseNode ln = (IPreferenceLooseNode)folder.ChildAt(_keyBindNode.Index);
            Debug.Assert(ln != null);
            KeyBindConfiguration kb = ln.Content as KeyBindConfiguration;
            Debug.Assert(kb != null);
            return kb;
        }
    }

    internal class KeyBindConfiguration : IPreferenceLooseNodeContent, IKeyBinds {
        private List<Tag> _data; //コマンドがCommandManagerPluginの登録順になるように
        private TypedHashtable<Keys, Tag> _keyToTag;
        private TypedHashtable<IGeneralCommand, Tag> _commandToTag;

        private class Tag {
            private int _index;
            private IGeneralCommand _command;
            private Keys _key;

            public Tag(int index, IGeneralCommand command, Keys key) {
                _index = index;
                _command = command;
                _key = key;
            }
            public int Index {
                get {
                    return _index;
                }
            }
            public IGeneralCommand Command {
                get {
                    return _command;
                }
            }
            public Keys Key {
                get {
                    return _key;
                }
                set {
                    _key = value;
                }
            }
            public Tag Clone() {
                return new Tag(_index, _command, _key);
            }
        }

        public KeyBindConfiguration() {
            _data = new List<Tag>();
            _keyToTag = new TypedHashtable<Keys, Tag>();
            _commandToTag = new TypedHashtable<IGeneralCommand, Tag>();
        }

        //keyToTagは空の状態で初期化。Cloneするときはその中で初期化するのでInitは不要。
        public void Init() {
            _data.Clear();
            _keyToTag.Clear();
            //登録順にサーチしていく
            int index = 0;
            foreach (IGeneralCommand command in CommandManagerPlugin.Instance.Commands) {
                Keys key = command.DefaultShortcutKey;
                Tag tag = new Tag(index++, command, Keys.None);
                _data.Add(tag);
                _commandToTag.Add(command, tag);
            }
        }

        #region IKeybinds
        public ICollection Commands {
            get {
                return _commandToTag.Keys;
            }
        }
        public Keys GetKey(IGeneralCommand command) {
            Tag tag = _commandToTag[command];
            return tag == null ? Keys.None : tag.Key;
        }
        //同一キーに重複登録は許さない
        public void SetKey(IGeneralCommand command, Keys key) {
            Tag tag = _commandToTag[command];
            if (key != Keys.None) {
                if (_keyToTag.Contains(key))
                    throw new ArgumentException("Duplicated Key in KeyBindConfiguration");
                if (tag.Key != Keys.None)
                    _keyToTag.Remove(tag.Key);
                _keyToTag.Add(key, tag);
            }
            else { //Keys.Noneの設定
                if (tag.Key != Keys.None) {
                    Debug.Assert(_keyToTag.Contains(tag.Key));
                    _keyToTag.Remove(tag.Key);
                }
            }
            tag.Key = key;

        }
        public IGeneralCommand FindCommand(Keys key) {
            Debug.Assert(key != Keys.None);
            Tag tag = _keyToTag[key];
            return tag == null ? null : tag.Command;
        }
        public void ClearAll() {
            _keyToTag.Clear();
            foreach (Tag tag in _data) {
                tag.Key = Keys.None;
            }
        }
        public void ResetToDefault() {
            Init();
            foreach (Tag tag in _data) {
                tag.Key = tag.Command.DefaultShortcutKey;
                if (tag.Key != Keys.None)
                    _keyToTag.Add(tag.Key, tag);
            }
        }
        public void Import(IKeyBinds keybinds) {
            _data.Clear();
            _keyToTag.Clear();
            _commandToTag.Clear();
            KeyBindConfiguration src = (KeyBindConfiguration)keybinds; //異なるIKeyBinds実装もあるかもしれないが
            foreach (Tag tag in src._data) {
                Tag newtag = tag.Clone();
                _data.Add(newtag);
                _commandToTag.Add(tag.Command, newtag);
                if (newtag.Key != Keys.None)
                    _keyToTag.Add(newtag.Key, newtag);
            }
            Debug.Assert(_keyToTag.Count == src._keyToTag.Count);
        }
        #endregion

        #region IPreferenceLooseNodeContent
        public IPreferenceLooseNodeContent Clone() {
            KeyBindConfiguration r = new KeyBindConfiguration();
            r.Import(this);
            return r;
        }

        public void Reset() {
            ResetToDefault();
        }

        public void LoadFrom(StructuredText node) {
            Init();
            foreach (Tag tag in _data) {
                string key_description = node.Get(tag.Command.CommandID);
                Keys key = key_description == null ? tag.Command.DefaultShortcutKey : WinFormsUtil.ParseKey(key_description.Split('+'));
                tag.Key = key;
                if (key != Keys.None)
                    _keyToTag.Add(key, tag);
            }
        }

        public void SaveTo(StructuredText node) {
            node.Clear();
            foreach (Tag tag in _data) {
                if (tag.Key != tag.Command.DefaultShortcutKey)
                    node.Set(tag.Command.CommandID, WinFormsUtil.FormatShortcut(tag.Key, '+'));
            }
        }
        #endregion
    }



}
