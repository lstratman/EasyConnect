/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSettings.cs,v 1.4 2011/11/01 15:35:45 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

using Poderosa.ConnectionParam;
using Poderosa.View;
using Poderosa.Util;
using Poderosa.Plugins;
using Poderosa.MacroEngine;

namespace Poderosa.Terminal {
    //IShellSchemeDynamicChangeListenerに関しては、ShellSchemeを確定するときにListenする
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TerminalSettings : ITerminalSettings, IShellSchemeDynamicChangeListener {
        private EncodingType _encoding;
        private TerminalType _terminalType;
        private bool _localecho;
        private LineFeedRule _lineFeedRule;
        private NewLine _transmitnl;
        private RenderProfile _renderProfile;
        private IMultiLogSettings _multiLogSettings;
        private string _caption;
        private Image _icon;
        private IShellScheme _shellScheme;
        private string _shellSchemeName;
        private bool _enabledCharTriggerIntelliSense;

        private ListenerList<ITerminalSettingsChangeListener> _listeners;
        private bool _updating;

        public event ChangeHandler<string> ChangeCaption;
        public event ChangeHandler<RenderProfile> ChangeRenderProfile;
        public event ChangeHandler<EncodingType> ChangeEncoding;

        public TerminalSettings() {
            IPoderosaCulture culture = TerminalEmulatorPlugin.Instance.PoderosaWorld.Culture;
            if (culture.IsJapaneseOS || culture.IsSimplifiedChineseOS || culture.IsTraditionalChineseOS || culture.IsKoreanOS)
                _encoding = EncodingType.UTF8;
            else
                _encoding = EncodingType.ISO8859_1;

            _terminalType = TerminalType.XTerm;
            _localecho = false;
            _lineFeedRule = LineFeedRule.Normal;
            _transmitnl = NewLine.CR;
            _renderProfile = null;
            _shellSchemeName = ShellSchemeCollection.DEFAULT_SCHEME_NAME;
            _enabledCharTriggerIntelliSense = false;
            _multiLogSettings = new MultiLogSettings();

            _listeners = new ListenerList<ITerminalSettingsChangeListener>();
        }

        //Clone, でもIConeableではない。Listener類はコピーしない。
        public virtual ITerminalSettings Clone() {
            TerminalSettings t = new TerminalSettings();
            t.Import(this);
            return t;
        }
        //Listener以外を持ってくる
        public virtual void Import(ITerminalSettings src) {
            _encoding = src.Encoding;
            _terminalType = src.TerminalType;
            _localecho = src.LocalEcho;
            _lineFeedRule = src.LineFeedRule;
            _transmitnl = src.TransmitNL;
            _caption = src.Caption;
            _icon = src.Icon;
            TerminalSettings src_r = (TerminalSettings)src;
            _shellSchemeName = src_r._shellSchemeName; //ちょっとインチキ
            if (src_r._shellScheme != null) {
                _shellScheme = src_r._shellScheme;
                TerminalEmulatorPlugin.Instance.ShellSchemeCollection.AddDynamicChangeListener(this);
            }
            _enabledCharTriggerIntelliSense = src.EnabledCharTriggerIntelliSense;
            _renderProfile = src.RenderProfile == null ? null : (RenderProfile)src.RenderProfile.Clone();
            _multiLogSettings = src.LogSettings == null ? null : (IMultiLogSettings)_multiLogSettings.Clone();
        }

        public RenderProfile RenderProfile {
            get {
                return _renderProfile;
            }
            set {
                EnsureUpdating();
                _renderProfile = value;
                if (this.ChangeRenderProfile != null)
                    this.ChangeRenderProfile(value);
            }
        }
        public bool UsingDefaultRenderProfile {
            get {
                return _renderProfile == null;
            }
        }


        [MacroConnectionParameter]
        public EncodingType Encoding {
            get {
                return _encoding;
            }
            set {
                EnsureUpdating();
                _encoding = value;
                if (this.ChangeEncoding != null)
                    this.ChangeEncoding(value);
            }
        }

        [MacroConnectionParameter]
        public TerminalType TerminalType {
            get {
                return _terminalType;
            }
            set {
                EnsureUpdating();
                _terminalType = value;
            }
        }

        public IMultiLogSettings LogSettings {
            get {
                return _multiLogSettings;
            }
        }

        [MacroConnectionParameter]
        public NewLine TransmitNL {
            get {
                return _transmitnl;
            }
            set {
                EnsureUpdating();
                _transmitnl = value;
            }
        }

        [MacroConnectionParameter]
        public bool LocalEcho {
            get {
                return _localecho;
            }
            set {
                EnsureUpdating();
                _localecho = value;
            }
        }

        [MacroConnectionParameter]
        public LineFeedRule LineFeedRule {
            get {
                return _lineFeedRule;
            }
            set {
                EnsureUpdating();
                _lineFeedRule = value;
            }
        }

        [MacroConnectionParameter]
        public string Caption {
            get {
                return _caption;
            }
            set {
                EnsureUpdating();
                _caption = value;
                if (this.ChangeCaption != null)
                    this.ChangeCaption(value);
            }
        }

        public Image Icon {
            get {
                return _icon;
            }
            set {
                EnsureUpdating();
                _icon = value;
            }
        }
        public IShellScheme ShellScheme {
            get {
                //ShellSchemeNameはレイトバインド専用
                if (_shellScheme == null) {
                    _shellScheme = TerminalEmulatorPlugin.Instance.ShellSchemeCollection.FindShellSchemeOrDefault(_shellSchemeName);
                    TerminalEmulatorPlugin.Instance.ShellSchemeCollection.AddDynamicChangeListener(this);
                }

                return _shellScheme;
            }
            set {
                EnsureUpdating();
                _shellScheme = value;
            }
        }
        public bool EnabledCharTriggerIntelliSense {
            get {
                return _enabledCharTriggerIntelliSense;
            }
            set {
                EnsureUpdating();
                _enabledCharTriggerIntelliSense = value;
            }
        }

        public virtual IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        //
        public void BeginUpdate() {
            if (_updating)
                throw new InvalidOperationException("EndUpdate() was missed");
            _updating = true;
            if (!_listeners.IsEmpty) {
                foreach (ITerminalSettingsChangeListener l in _listeners)
                    l.OnBeginUpdate(this);
            }
        }
        public void EndUpdate() {
            if (!_updating)
                throw new InvalidOperationException("BeginUpdate() was missed");
            _updating = false;
            if (!_listeners.IsEmpty) {
                foreach (ITerminalSettingsChangeListener l in _listeners)
                    l.OnEndUpdate(this);
            }
        }

        public void AddListener(ITerminalSettingsChangeListener l) {
            _listeners.Add(l);
        }
        public void RemoveListener(ITerminalSettingsChangeListener l) {
            _listeners.Remove(l);
        }

        public void SetShellSchemeName(string value) {
            _shellSchemeName = value;
            _shellScheme = null;
        }

        private void EnsureUpdating() {
            if (!_updating)
                throw new InvalidOperationException("NOT UPDATE STATE");
        }

        //IShellSchemeDynamicChangeListener
        public void OnShellSchemeCollectionChanged(IShellScheme[] values, Poderosa.Util.Collections.TypedHashtable<IShellScheme, IShellScheme> table) {
            if (_shellScheme == null)
                return;

            IShellScheme ns = table[_shellScheme];
            Debug.Assert(ns != null);
            BeginUpdate();
            _shellScheme = ns;
            _shellSchemeName = ns.Name;
            EndUpdate(); //これで通知が出る。例えばShellScheme選択コンボボックス。
        }
    }

    internal class SimpleLogSettings : ISimpleLogSettings {
        private LogType _logtype;
        private string _logpath;
        private bool _logappend;

        public SimpleLogSettings() {
            _logtype = LogType.None;
        }
        public SimpleLogSettings(LogType lt, string path) {
            _logtype = lt;
            _logpath = path;
            _logappend = false;
        }

        public ILogSettings Clone() {
            SimpleLogSettings t = new SimpleLogSettings();
            t._logappend = _logappend;
            t._logpath = _logpath;
            t._logtype = _logtype;
            return t;
        }

        public LogType LogType {
            get {
                return _logtype;
            }
            set {
                _logtype = value;
            }
        }

        public string LogPath {
            get {
                return _logpath;
            }
            set {
                _logpath = value;
            }
        }

        public bool LogAppend {
            get {
                return _logappend;
            }
            set {
                _logappend = value;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }
}
