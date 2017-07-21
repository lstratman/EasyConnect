/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PaneBridge.cs,v 1.2 2011/10/27 23:21:58 kzmi Exp $
 */
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Poderosa.Terminal;
using Poderosa.Sessions;
using Poderosa.View;
using Poderosa.Commands;
using Poderosa.UI;
using Poderosa.Forms;

namespace Poderosa.Terminal {

    internal class TerminalViewFactory : IViewFactory {
        public IPoderosaView CreateNew(IPoderosaForm parent) {
            return new TerminalView(parent, new TerminalControl());
        }

        public Type GetViewType() {
            return typeof(TerminalView);
        }
        public Type GetDocumentType() {
            return typeof(TerminalDocument);
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

    //TerminalControlにビュー機能を与えるクラス
    public class TerminalView : IPoderosaView, IContentReplaceableViewSite, IGeneralViewCommands {
        private IPoderosaForm _parent;
        private TerminalControl _control;
        private IContentReplaceableView _contentReplaceableView; //包含するやつ
        private IPoderosaCommand _copyCommand;
        private IPoderosaCommand _pasteCommand;

        public TerminalView(IPoderosaForm parent, TerminalControl control) {
            _parent = parent;
            _control = control;
            _copyCommand = TerminalSessionsPlugin.Instance.WindowManager.SelectionService.DefaultCopyCommand;
            _pasteCommand = TerminalSessionsPlugin.Instance.GetPasteCommand();
            control.Tag = this;
        }

        public TerminalControl TerminalControl {
            get {
                return _control;
            }
        }

        #region IContentReplaceableViewSite
        public IContentReplaceableView CurrentContentReplaceableView {
            get {
                return _contentReplaceableView;
            }
            set {
                _contentReplaceableView = value;
            }
        }
        #endregion

        #region IPoderosaView
        public IPoderosaDocument Document {
            get {
                return _control.CharacterDocument;
            }
        }

        public ISelection CurrentSelection {
            get {
                return _control.ITextSelection;
            }
        }

        public Control AsControl() {
            return _control;
        }
        public IPoderosaForm ParentForm {
            get {
                return _parent;
            }
        }
        #endregion

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        #region IGeneralViewCommands
        public IPoderosaCommand Copy {
            get {
                return _copyCommand;
            }
        }

        public IPoderosaCommand Paste {
            get {
                return _pasteCommand;
            }
        }
        #endregion

    }

    //TerminalControl to TerminalView
    internal class PaneBridgeAdapter : ITypedDualDirectionalAdapterFactory<TerminalControl, TerminalView> {

        public override TerminalView GetAdapter(TerminalControl obj) {
            return ((TerminalView)obj.Tag);
        }

        public override TerminalControl GetSource(TerminalView obj) {
            return ((TerminalView)obj).TerminalControl;
        }

    }

}
