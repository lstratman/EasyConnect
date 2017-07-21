/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ExtensionPointList.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Poderosa.Plugins;

namespace Poderosa.Forms {
    internal partial class ExtensionPointList : Form {
        public ExtensionPointList() {
            InitializeComponent();
            InitText();
            FillList();
        }
        private void InitText() {
            StringResource sr = CoreUtil.Strings;
            this.Text = sr.GetString("Form.ExtensionPointList.Text");
            _idHeader.Text = "ID";
            _ownerHeader.Text = sr.GetString("Form.ExtensionPointList._ownerHeader");
            _countHeader.Text = sr.GetString("Form.ExtensionPointList._countHeader");
            _okButton.Text = sr.GetString("Common.OK");
            _cancelButton.Text = sr.GetString("Common.Cancel");
        }

        private void FillList() {
            _list.BeginUpdate();
            IPluginInspector pi = (IPluginInspector)WindowManagerPlugin.Instance.PoderosaWorld.PluginManager.GetAdapter(typeof(IPluginInspector));
            foreach (IExtensionPoint pt in pi.ExtensionPoints) {
                ListViewItem li = new ListViewItem(pt.ID);
                li.SubItems.Add(pt.OwnerPlugin == null ? "" : pi.GetPluginInfo(pt.OwnerPlugin).PluginInfoAttribute.ID); //Rootではオーナなし
                li.SubItems.Add(pt.GetExtensions().Length.ToString());

                _list.Items.Add(li);
            }
            _list.EndUpdate();
        }
    }
}