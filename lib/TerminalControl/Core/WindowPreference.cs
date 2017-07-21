/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: WindowPreference.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Globalization;

using Poderosa.Util;
using Poderosa.Plugins;
using Poderosa.Sessions;
using Poderosa.Preferences;
using Poderosa.View;
using Poderosa.Commands;

namespace Poderosa.Forms {

    //friendly interface実装
    internal class CoreServicePreferenceAdapter : SnapshotAwarePreferenceBase, ICoreServicePreference {

        //見た目調整
        private IStringPreferenceItem _viewSplitModifier; //マウスでの分割
        private IBoolPreferenceItem _showsToolBar;
        private IStringPreferenceItem _language;
        private IIntPreferenceItem _caretInterval;
        private IBoolPreferenceItem _autoCopyByLeftButton; //TerminalEmulatorから引越ししてきた

        private IIntPreferenceItem _splitLimitCount;

        //ダーティフラグとキャッシュ
        private bool _viewSplitModifierChecked;
        private Keys _viewSplitModifierKey;

        public CoreServicePreferenceAdapter(IPreferenceFolder folder)
            : base(folder) {
        }

        public override void DefineItems(IPreferenceBuilder builder) {
            _viewSplitModifier = builder.DefineStringValue(_folder, "viewSplitModifier", "None", null);
            _showsToolBar = builder.DefineBoolValue(_folder, "showsToolBar", true, null);
            _caretInterval = builder.DefineIntValue(_folder, "caretInterval", 300, PreferenceValidatorUtil.PositiveIntegerValidator);
            _autoCopyByLeftButton = builder.DefineBoolValue(_folder, "autoCopyByLeftButton", false, null);
            _language = builder.DefineStringValue(_folder, "language", GetNativeLanguage().ToString(), null);

            _splitLimitCount = builder.DefineIntValue(_folder, "splitLimitCount", 16, PreferenceValidatorUtil.IntRangeValidator(1, 50));
        }
        public CoreServicePreferenceAdapter Import(CoreServicePreferenceAdapter src) {
            _viewSplitModifier = ConvertItem(src._viewSplitModifier);
            _showsToolBar = ConvertItem(src._showsToolBar);
            _language = ConvertItem(src._language);
            _caretInterval = ConvertItem(src._caretInterval);
            _autoCopyByLeftButton = ConvertItem(src._autoCopyByLeftButton);
            _splitLimitCount = ConvertItem(src._splitLimitCount);
            return this;
        }


        public bool ShowsToolBar {
            get {
                return _showsToolBar.Value;
            }
            set {
                _showsToolBar.Value = value;
            }
        }

        public int CaretInterval {
            get {
                return _caretInterval.Value;
            }
            set {
                _caretInterval.Value = value;
            }
        }
        public bool AutoCopyByLeftButton {
            get {
                return _autoCopyByLeftButton.Value;
            }
            set {
                _autoCopyByLeftButton.Value = value;
            }
        }
        public int SplitLimitCount {
            get {
                return _splitLimitCount.Value;
            }
            set {
                _splitLimitCount.Value = value;
            }
        }


        public Keys ViewSplitModifier {
            get {
                if (!_viewSplitModifierChecked) {
                    _viewSplitModifierChecked = true;
                    try {
                        _viewSplitModifierKey = WinFormsUtil.ParseModifier(_viewSplitModifier.Value);
                    }
                    catch (Exception ex) {
                        RuntimeUtil.ReportException(ex);
                        _viewSplitModifierKey = Keys.None; //ここはOnMouseMoveから繰り返しチェックを受けるので、下手な値が入っていて繰り返しエラーが起きるのをさける
                    }
                }
                return _viewSplitModifierKey;
            }
            set {
                _viewSplitModifier.Value = value.ToString();
                _viewSplitModifierChecked = false;
            }
        }
        //フラグクリア
        public void ClearSplitModifierCheckedFlag() {
            _viewSplitModifierChecked = false;
        }

        public Language Language {
            get {
                return ParseUtil.ParseEnum<Language>(_language.Value, Language.English);
            }
            set {
                _language.Value = value.ToString();
            }
        }

        private Language GetNativeLanguage() {
            IPoderosaCulture c = WindowManagerPlugin.Instance.PoderosaWorld.Culture;
            return c.InitialCulture.Name.StartsWith("ja") ? Language.Japanese : Language.English;
        }
        public static CultureInfo LangToCulture(Language lang) {
            if (lang == Language.Japanese)
                return CultureInfo.GetCultureInfo("ja-JP");
            else
                return CultureInfo.InvariantCulture;
        }
    }



    internal class WindowPreference : IPreferenceSupplier, IWindowPreference {
        private IPreferenceFolder _originalFolder;
        private IPreferenceFolderArray _windowArrayPreference;
        private IPreferenceFolder _windowTemplatePreference;

        //以下、ウィンドウ毎
        private IStringPreferenceItem _windowPositionPreference;
        private IStringPreferenceItem _windowSplitFormatPreference;
        private IStringPreferenceItem _toolBarFormatPreference;
        private IIntPreferenceItem _tabRowCountPreference;

        private CoreServicePreferenceAdapter _adapter;

        private class ChangeListener : IPreferenceChangeListener {
            private CoreServicePreferenceAdapter _adapter;
            public ChangeListener(CoreServicePreferenceAdapter adapter) {
                _adapter = adapter;
            }
            public void OnPreferenceImport(IPreferenceFolder oldvalues, IPreferenceFolder newvalues) {
                ICoreServicePreference nv = (ICoreServicePreference)newvalues.QueryAdapter(typeof(ICoreServicePreference));
                WindowManagerPlugin.Instance.ReloadPreference(nv);
                _adapter.ClearSplitModifierCheckedFlag();

                //言語が変わっていたら...
                Language lang = nv.Language;
                if (lang != ((ICoreServicePreference)oldvalues.QueryAdapter(typeof(ICoreServicePreference))).Language) {
                    Debug.WriteLine("Change Language");
                    WindowManagerPlugin.Instance.PoderosaWorld.Culture.SetCulture(CoreServicePreferenceAdapter.LangToCulture(lang));
                }
            }
        }

        #region IPreferenceSupplier
        public string PreferenceID {
            get {
                return WindowManagerPlugin.PLUGIN_ID;
            }
        }
        public void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
            _originalFolder = folder;
            _adapter = new CoreServicePreferenceAdapter(folder);
            _adapter.DefineItems(builder);

            AboutBoxUtil.InitPreference(builder, folder);

            _windowTemplatePreference = builder.DefineFolderArray(folder, this, "mainwindow");
            _windowArrayPreference = folder.FindChildFolderArray("mainwindow");
            Debug.Assert(_windowArrayPreference != null);

            _windowPositionPreference = builder.DefineStringValue(_windowTemplatePreference, "position", "", null);
            _windowSplitFormatPreference = builder.DefineStringValue(_windowTemplatePreference, "format", "", null);
            _toolBarFormatPreference = builder.DefineStringValue(_windowTemplatePreference, "toolbar", "", null);
            _tabRowCountPreference = builder.DefineIntValue(_windowTemplatePreference, "tabrowcount", 1, null);

            //add listener
            folder.AddChangeListener(new ChangeListener(_adapter));
        }
        public IPreferenceFolderArray WindowArray {
            get {
                return _windowArrayPreference;
            }
        }

        public int WindowCount {
            get {
                return _windowArrayPreference.Folders.Length;
            }
        }

        public string WindowPositionAt(int index) {
            IPreferenceFolderArray a = _windowArrayPreference;
            return a.ConvertItem(a.Folders[index], _windowPositionPreference).AsString().Value;
        }

        public string WindowSplitFormatAt(int index) {
            IPreferenceFolderArray a = _windowArrayPreference;
            return a.ConvertItem(a.Folders[index], _windowSplitFormatPreference).AsString().Value;
        }

        public string ToolBarFormatAt(int index) {
            IPreferenceFolderArray a = _windowArrayPreference;
            return a.ConvertItem(a.Folders[index], _toolBarFormatPreference).AsString().Value;
        }

        public int TabRowCountAt(int index) {
            IPreferenceFolderArray a = _windowArrayPreference;
            return a.ConvertItem(a.Folders[index], _tabRowCountPreference).AsInt().Value;
        }

        public object QueryAdapter(IPreferenceFolder folder, Type type) {
            if (type == typeof(ICoreServicePreference))
                return folder == _originalFolder ? _adapter : new CoreServicePreferenceAdapter(folder).Import(_adapter);
            else if (type == typeof(IWindowPreference)) {
                Debug.Assert(folder == _originalFolder); //IWindowPreferenceについてはSnapshotサポートせず
                return this;
            }
            else
                return null;
        }

        public string GetDescription(IPreferenceItem item) {
            return "";
        }

        public void ValidateFolder(IPreferenceFolder folder, IPreferenceValidationResult output) {
        }
        #endregion

        public ICoreServicePreference OriginalPreference {
            get {
                return _adapter;
            }
        }



        public void FormatWindowPreference(MainWindow f) {
            IPreferenceFolder element = _windowArrayPreference.CreateNewFolder();

            FormWindowState st = f.WindowState;
            Rectangle rc = st == FormWindowState.Normal ? f.DesktopBounds : f.RestoreBounds; //Normal時にはRestoreBound取得できない、注意
            _windowArrayPreference.ConvertItem(element, _windowPositionPreference).AsString().Value = String.Format("({0}{1},{2},{3},{4})",
                st == FormWindowState.Maximized ? "Max," : "",
                rc.Left, rc.Top, rc.Width, rc.Height);
            //TODO PreferenceItemのテンプレートをViewManager側に移動したほうが汎用的
            ISplittableViewManager vm = (ISplittableViewManager)f.ViewManager.GetAdapter(typeof(ISplittableViewManager));
            if (vm != null)
                _windowArrayPreference.ConvertItem(element, _windowSplitFormatPreference).AsString().Value = vm.FormatSplitInfo();
            _windowArrayPreference.ConvertItem(element, _toolBarFormatPreference).AsString().Value = f.ToolBar.FormatLocations();
            _windowArrayPreference.ConvertItem(element, _tabRowCountPreference).AsInt().Value = f.DocumentTabFeature.TabRowCount;
        }


    }
}
