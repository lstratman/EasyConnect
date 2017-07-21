/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ToolBar.cs,v 1.5 2012/03/11 12:19:21 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

using Poderosa.UI;
using Poderosa.Commands;

namespace Poderosa.Forms {
    internal class PoderosaToolStripContainer : ToolStripContainer, IToolBar {
        private IPoderosaMainWindow _parent;
        private string _initialLocationInfo;

        private List<ToolStrip> _toolStrips;
        private ToolStrip _currentToolStrip;

        public PoderosaToolStripContainer(IPoderosaMainWindow parent, string location_info) {
            _parent = parent;
            _initialLocationInfo = location_info;

            //this.Height = 25; //適切なサイズを決める方法がわからない
            this.Dock = DockStyle.Fill;
            //左右に入れるのは見苦しい。やめる。
            this.LeftToolStripPanelVisible = false;
            this.RightToolStripPanelVisible = false;
            this.BottomToolStripPanelVisible = false;
            this.TopToolStripPanelVisible = WindowManagerPlugin.Instance.WindowPreference.OriginalPreference.ShowsToolBar;
            this.AllowDrop = true;
            CreateInternal();
        }

        private void CreateInternal() {
            IToolBarComponent[] components = (IToolBarComponent[])WindowManagerPlugin.Instance.PoderosaWorld.PluginManager.FindExtensionPoint(WindowManagerConstants.TOOLBARCOMPONENT_ID).GetExtensions();

            _toolStrips = new List<ToolStrip>();
            string[] locations = _initialLocationInfo.Split(',');
            this.TopToolStripPanel.Size = new Size(TryParseInt(locations, 0), TryParseInt(locations, 1));
            int index = 1;
            foreach (IToolBarComponent comp in PositionDesignationSorter.SortItems(components)) {
                //Locationの判定
                Point pt = new Point(TryParseInt(locations, index * 2), TryParseInt(locations, index * 2 + 1));
                CreateToolBarComponent(comp, pt);
                index++;
            }
        }

        //位置指定はフォームのOnLoad後にやらなくちゃいかんのか。めんどうだな
        public void RestoreLayout() {
            ICommandTarget target = (ICommandTarget)_parent.GetAdapter(typeof(ICommandTarget));

            ToolStripPanel panel = this.TopToolStripPanel;
            panel.BeginInit();
            panel.SuspendLayout();
            bool location_available = _initialLocationInfo.Length > 0;

            //ToolStripPanelへの追加はかなり不可思議。
            //BeginInitやSuspendLayoutを呼ぶかどうかでも相当様子が違う。
            //なのでややいい加減だが、初回起動時など位置情報がないときはControls.AddRangeで一括登録で.NETに任せ、それ以降は位置指定という方針でいく

            if (!location_available)
                panel.Controls.AddRange(_toolStrips.ToArray());

            foreach (ToolStrip t in _toolStrips) {
                if (location_available)
                    panel.Join(t, t.Location);
                foreach (ToolStripItem c in t.Items) {
                    ControlTagBase tag = c.Tag as ControlTagBase;
                    if (tag != null) {
                        RefreshElement(c, tag, target);
                    }
                }
            }

            panel.ResumeLayout();
            panel.EndInit();
        }
        public void ReloadPreference(ICoreServicePreference pref) {
            this.TopToolStripPanelVisible = pref.ShowsToolBar;
        }
        public void Reload() {
            //本当は全部構築すべき
            RefreshAll();
        }

        //現在はTop限定
        private ControlCollection GetContents() {
            return this.TopToolStripPanel.Controls;
        }

        private void CreateToolBarComponent(IToolBarComponent comp, Point pt) {
            //この中でIToolBarの各メソッドが呼ばれ、モロモロの登録が行われる
            _currentToolStrip = new ToolStrip();
            IToolBarElement[] elements = comp.ToolBarElements;
            foreach (IToolBarElement e in elements) {
                if (e is IToolBarCommandButton)
                    DefineCommandButton(comp, (IToolBarCommandButton)e);
                else if (e is IToolBarLabel)
                    DefineLabel(comp, (IToolBarLabel)e);
                else if (e is IToolBarComboBox)
                    DefineComboBox(comp, (IToolBarComboBox)e);
                else if (e is IToolBarToggleButton)
                    DefineToggleButton(comp, (IToolBarToggleButton)e);
                else
                    throw new ArgumentException("Unexpected IToolBarElement type");
            }
            _currentToolStrip.Location = pt;
            _toolStrips.Add(_currentToolStrip);
            Debug.WriteLineIf(DebugOpt.BuildToolBar, "toolbar " + comp.GetType().Name + " location=" + _currentToolStrip.Location.ToString());
        }


        private void DefineCommandButton(IToolBarComponent comp, IToolBarCommandButton element) {
            ToolStripButton button = new ToolStripButton();
            button.Image = element.Icon;
            button.Tag = new ButtonTag(GetCommandTarget(), comp, element.Command);
            button.Size = new Size(24, 23);
            button.Click += delegate(object sender, EventArgs args) {
                DoCommand(element.Command);
            };
            IGeneralCommand gc = (IGeneralCommand)element.Command.GetAdapter(typeof(IGeneralCommand));
            if (gc != null) {
                if (!String.IsNullOrEmpty(gc.Description))
                    button.ToolTipText = gc.Description;
            }
            else if (!String.IsNullOrEmpty(element.ToolTipText)) {
                button.ToolTipText = element.ToolTipText;
            }

            _currentToolStrip.Items.Add(button);
        }
        private void DefineLabel(IToolBarComponent comp, IToolBarLabel element) {
            ToolStripLabel label = new ToolStripLabel();
            label.Width = element.Width;
            label.Text = element.Text;
            label.Tag = new LabelTag(comp, element);
            label.TextAlign = ContentAlignment.MiddleRight;
            _currentToolStrip.Items.Add(label);
        }
        private void DefineComboBox(IToolBarComponent comp, IToolBarComboBox element) {
            ToolStripComboBox cb = new ToolStripComboBox();
            cb.Items.AddRange(element.Items);
            cb.Size = new Size(element.Width, cb.Height); //Widthを直接設定してもいかんらしい。なんじゃいな
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxTag tag = new ComboBoxTag(GetCommandTarget(), comp, element);
            cb.Tag = tag;
            cb.SelectedIndexChanged += new EventHandler(tag.OnSelectedIndexChanged);
            if (!String.IsNullOrEmpty(element.ToolTipText))
                cb.ToolTipText = element.ToolTipText;

            _currentToolStrip.Items.Add(cb);
        }
        private void DefineToggleButton(IToolBarComponent comp, IToolBarToggleButton element) {
            ToolStripButton tb = new ToolStripButton();
            tb.Image = element.Icon;
            ToggleButtonTag tag = new ToggleButtonTag(GetCommandTarget(), comp, element);
            tb.Tag = tag;
            tb.Click += new EventHandler(tag.OnClick);
            if (!String.IsNullOrEmpty(element.ToolTipText))
                tb.ToolTipText = element.ToolTipText;

            _currentToolStrip.Items.Add(tb);
        }

        #region IToolBar
        //UIのAdjustment
        public void RefreshComponent(IToolBarComponent component) {
            ICommandTarget target = (ICommandTarget)_parent.GetAdapter(typeof(ICommandTarget));
            foreach (ToolStrip st in GetContents()) {
                //TODO タグの関連付け工夫できる
                foreach (ToolStripItem c in st.Items) {
                    ControlTagBase tag = c.Tag as ControlTagBase;
                    if (tag != null && tag.OwnerComponent == component) {
                        RefreshElement(c, tag, target);
                    }
                }
            }
        }
        public void RefreshAll() {
            ICommandTarget target = (ICommandTarget)_parent.GetAdapter(typeof(ICommandTarget));
            foreach (ToolStrip st in GetContents()) {
                foreach (ToolStripItem c in st.Items) {
                    ControlTagBase tag = c.Tag as ControlTagBase;
                    if (tag != null) {
                        RefreshElement(c, tag, target);
                    }
                }
            }
        }
        public IPoderosaMainWindow ParentWindow {
            get {
                return _parent;
            }
        }
        public string FormatLocations() {
            StringBuilder bld = new StringBuilder();
            //最初はサイズ
            bld.Append(this.TopToolStripPanel.Width.ToString());
            bld.Append(',');
            bld.Append(this.TopToolStripPanel.Height.ToString());
            foreach (ToolStrip st in _toolStrips) {
                bld.Append(',');
                bld.Append(st.Location.X.ToString());
                bld.Append(',');
                bld.Append(st.Location.Y.ToString());
            }
            return bld.ToString();
        }
        #endregion

        private void RefreshElement(ToolStripItem c, ControlTagBase tag, ICommandTarget target) {
            tag.Refresh(c);
        }

        #region IAdaptable
        public IAdaptable GetAdapter(Type adapter) {
            return WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion

        private void DoCommand(IPoderosaCommand command) {
            try {
                ICommandTarget target = GetCommandTarget();
                Debug.Assert(target != null);
                if (command.CanExecute(target))
                    CommandManagerPlugin.Instance.Execute(command, target);
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }

        private ICommandTarget GetCommandTarget() {
            return (ICommandTarget)_parent.GetAdapter(typeof(ICommandTarget));
        }

        private static int TryParseInt(string[] values, int index) {
            if (values.Length <= index)
                return 0;
            else
                return ParseUtil.ParseInt(values[index], 0);
        }

        //UI初期化
        protected override void OnCreateControl() {
            base.OnCreateControl();
            this.RefreshAll();
        }
        protected override void OnDragEnter(DragEventArgs drgevent) {
            base.OnDragEnter(drgevent);
            try {
                WindowManagerPlugin.Instance.BypassDragEnter(this.ParentForm, drgevent);
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }
        protected override void OnDragDrop(DragEventArgs drgevent) {
            base.OnDragDrop(drgevent);
            try {
                WindowManagerPlugin.Instance.BypassDragDrop(this.ParentForm, drgevent);
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }

        private abstract class ControlTagBase {
            private IToolBarComponent _ownerComponent;

            public ControlTagBase(IToolBarComponent component) {
                _ownerComponent = component;
            }

            public IToolBarComponent OwnerComponent {
                get {
                    return _ownerComponent;
                }
            }

            public abstract void Refresh(ToolStripItem c);
        }

        private class ButtonTag : ControlTagBase {
            private IPoderosaCommand _associatedCommand;
            private IGeneralCommand _generalCommand;
            private ICommandTarget _target;

            public ButtonTag(ICommandTarget target, IToolBarComponent owner, IPoderosaCommand command)
                : base(owner) {
                _target = target;
                _associatedCommand = command;
                _generalCommand = (IGeneralCommand)command.GetAdapter(typeof(IGeneralCommand)); //取得できなきゃnull
            }

            public IPoderosaCommand AssociatedCommand {
                get {
                    return _associatedCommand;
                }
            }

            public override void Refresh(ToolStripItem c) {
                c.Enabled = _associatedCommand.CanExecute(_target);
                if (_generalCommand != null && _generalCommand.Description.Length > 0)
                    c.ToolTipText = _generalCommand.Description;
            }
        }

        private class LabelTag : ControlTagBase {
            private IToolBarLabel _label;
            public LabelTag(IToolBarComponent owner, IToolBarLabel label)
                : base(owner) {
                _label = label;
            }
            public override void Refresh(ToolStripItem c) {
                c.Text = _label.Text;
            }
        }

        private class ComboBoxTag : ControlTagBase {
            private IToolBarComboBox _handler;
            private ICommandTarget _target;
            private bool _eventHandlerGuard;

            public ComboBoxTag(ICommandTarget target, IToolBarComponent component, IToolBarComboBox handler)
                : base(component) {
                _target = target;
                _handler = handler;
            }

            public override void Refresh(ToolStripItem combobox_) {
                Debug.Assert(combobox_.Tag == this);
                _eventHandlerGuard = true;
                try {
                    ToolStripComboBox combobox = (ToolStripComboBox)combobox_;
                    combobox.Enabled = _handler.IsEnabled(_target);
                    combobox.Items.Clear();
                    combobox.Items.AddRange(_handler.Items); //TODO 項目数可変かどうかをhandlerに尋ねるようにもできる
                    if (combobox.Enabled)
                        combobox.SelectedIndex = _handler.GetSelectedIndex(_target);
                    else
                        combobox.SelectedIndex = -1;
                }
                finally {
                    _eventHandlerGuard = false;
                }
            }

            //NOTE 仮にOnChangeからRefreshするハンドラがいても、このあたりのコードを通るので再帰防ぐ
            public void OnSelectedIndexChanged(object sender, EventArgs args) {
                if (_eventHandlerGuard)
                    return;
                ToolStripComboBox cb = sender as ToolStripComboBox;
                Debug.Assert(cb != null && cb.Tag == this);

                try {
                    _eventHandlerGuard = true;
                    if (cb.Enabled) //一応ガード
                        _handler.OnChange(_target, cb.SelectedIndex, cb.SelectedItem);
                }
                catch (Exception ex) {
                    RuntimeUtil.ReportException(ex);
                }
                finally {
                    _eventHandlerGuard = false;
                }
            }
        }

        private class ToggleButtonTag : ControlTagBase {
            private IToolBarToggleButton _handler;
            private ICommandTarget _target;
            private bool _eventHandlerGuard;

            public ToggleButtonTag(ICommandTarget target, IToolBarComponent component, IToolBarToggleButton handler)
                : base(component) {
                _target = target;
                _handler = handler;
            }

            public override void Refresh(ToolStripItem button_) {
                Debug.Assert(button_.Tag == this);
                ToolStripButton button = (ToolStripButton)button_;
                button.Enabled = _handler.IsEnabled(_target);
                button.Checked = button.Enabled && _handler.IsChecked(_target); //enabledはcheckedのための必要条件
                button.ToolTipText = _handler.ToolTipText;
            }

            public void OnClick(object sender, EventArgs args) {
                if (_eventHandlerGuard)
                    return;
                ToolStripButton tb = sender as ToolStripButton;
                Debug.Assert(tb != null && tb.Tag == this);

                try {
                    bool value = tb.Checked;
                    _eventHandlerGuard = true;
                    tb.Checked = !value;
                    _handler.OnChange(_target, !value); //反転値をセット
                }
                catch (Exception ex) {
                    RuntimeUtil.ReportException(ex);
                }
                finally {
                    _eventHandlerGuard = false;
                }
            }
        }
    }
}
