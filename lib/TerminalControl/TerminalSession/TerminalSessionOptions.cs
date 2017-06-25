/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSessionOptions.cs,v 1.2 2011/10/27 23:21:59 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using Poderosa.Preferences;

namespace Poderosa.Sessions {
    internal class TerminalSessionOptions : SnapshotAwarePreferenceBase, ITerminalSessionOptions {
        private IBoolPreferenceItem _askCloseOnExit;
        private IIntPreferenceItem _terminalEstablishTimeout;
        private IStringPreferenceItem _telnetSSHLoginDialogUISupportTypeName;
        private IStringPreferenceItem _cygwinLoginDialogUISupportTypeName;

        public TerminalSessionOptions(IPreferenceFolder folder)
            : base(folder) {
        }
        public override void DefineItems(IPreferenceBuilder builder) {
            _askCloseOnExit = builder.DefineBoolValue(_folder, "askCloseOnExit", false, null);

            _terminalEstablishTimeout = builder.DefineIntValue(_folder, "terminalEstablishTimeout", 5000, PreferenceValidatorUtil.PositiveIntegerValidator);
            _telnetSSHLoginDialogUISupportTypeName = builder.DefineStringValue(_folder, "telnetSSHLoginDialogUISupport", "Poderosa.Usability.MRUList", null);
            _cygwinLoginDialogUISupportTypeName = builder.DefineStringValue(_folder, "cygwinLoginDialogUISupport", "Poderosa.Usability.MRUList", null);
        }
        public TerminalSessionOptions Import(TerminalSessionOptions src) {
            _askCloseOnExit = ConvertItem(src._askCloseOnExit);
            _terminalEstablishTimeout = ConvertItem(src._terminalEstablishTimeout);
            return this;
        }

        public bool AskCloseOnExit {
            get {
                return _askCloseOnExit.Value;
            }
            set {
                _askCloseOnExit.Value = value;
            }
        }

        public int TerminalEstablishTimeout {
            get {
                return _terminalEstablishTimeout.Value;
            }
        }
        public string GetDefaultLoginDialogUISupportTypeName(string logintype) {
            if (_telnetSSHLoginDialogUISupportTypeName.Id == logintype)
                return _telnetSSHLoginDialogUISupportTypeName.Value;
            else if (_cygwinLoginDialogUISupportTypeName.Id == logintype)
                return _cygwinLoginDialogUISupportTypeName.Value;
            else
                return "";
        }

    }

    internal class TerminalSessionOptionsSupplier : IPreferenceSupplier {
        private IPreferenceFolder _originalFolder;
        private TerminalSessionOptions _originalOptions;

        public string PreferenceID {
            get {
                return TerminalSessionsPlugin.PLUGIN_ID; //同じとする
            }
        }

        public void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
            _originalFolder = folder;

            _originalOptions = new TerminalSessionOptions(_originalFolder);
            _originalOptions.DefineItems(builder);
        }

        public object QueryAdapter(IPreferenceFolder folder, Type type) {
            Debug.Assert(_originalFolder.Id == folder.Id);
            if (type == typeof(ITerminalSessionOptions))
                return folder == _originalFolder ? _originalOptions : new TerminalSessionOptions(folder).Import(_originalOptions);
            else
                return null;
        }

        public void ValidateFolder(IPreferenceFolder folder, IPreferenceValidationResult output) {
        }

        public ITerminalSessionOptions OriginalOptions {
            get {
                return _originalOptions;
            }
        }
    }
}
