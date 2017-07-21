/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PluginList.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Poderosa.Plugins;

namespace Poderosa.Forms {
    internal partial class PluginList : Form {
        public PluginList() {
            InitializeComponent();
            InitText();
            FillList();
        }
        private void InitText() {
            StringResource sr = CoreUtil.Strings;
            this.Text = sr.GetString("Form.PluginList.Text");
            _enableHeader.Text = sr.GetString("Form.PluginList._enableHeader");
            _idHeader.Text = "ID";
            _versionHeader.Text = sr.GetString("Form.PluginList._versionHeader");
            _venderHeader.Text = sr.GetString("Form.PluginList._venderHeader");
            _okButton.Text = sr.GetString("Common.OK");
            _cancelButton.Text = sr.GetString("Common.Cancel");
            //構成変更はとりあえず先送り
            _createShortcutButton.Visible = false;
        }

        private void FillList() {
            IPluginInspector pi = (IPluginInspector)WindowManagerPlugin.Instance.PoderosaWorld.PluginManager.GetAdapter(typeof(IPluginInspector));
            foreach (IPluginInfo plugin in pi.Plugins) {
                ListViewItem li = new ListViewItem();
                //li.Checked = plugin.Status==PluginStatus.Activated;
                li.Text = plugin.PluginInfoAttribute.ID;
                //li.SubItems.Add(plugin.PluginInfoAttribute.ID);
                li.SubItems.Add(plugin.PluginInfoAttribute.Version);
                li.SubItems.Add(plugin.PluginInfoAttribute.Author);

                _list.Items.Add(li);
            }
        }
    }
}