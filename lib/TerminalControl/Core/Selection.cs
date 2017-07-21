/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Selection.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.View;
using Poderosa.Commands;
using Poderosa.Sessions;

//選択領域の管理と、選択したものに関わる基本コマンド（コピーなど）の実装

namespace Poderosa.Forms {
    internal class SelectionService : ISelectionService {

        private WindowManagerPlugin _parent;
        private SelectedTextCopyCommand _copyCommand;

        public SelectionService(WindowManagerPlugin parent) {
            _parent = parent;
            _copyCommand = new SelectedTextCopyCommand();
        }

        public ISelection ActiveSelection {
            get {
                IPoderosaMainWindow window = _parent.ActiveWindow;
                if (window == null)
                    return null;

                IPoderosaView view = window.LastActivatedView;
                if (view == null)
                    return null;

                return view.CurrentSelection;
            }
        }

        public IPoderosaCommand DefaultCopyCommand {
            get {
                return _copyCommand;
            }
        }
    }
}
