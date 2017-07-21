/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CopyPaste.cs,v 1.3 2011/10/27 23:21:58 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using Poderosa.Terminal;
using Poderosa.Sessions;
using Poderosa.View;
using Poderosa.Forms;

namespace Poderosa.Commands {

    /// <summary>
    /// Default Paste command
    /// </summary>
    internal class PasteToTerminalCommand : IPoderosaCommand {

        public PasteToTerminalCommand() {
        }

        // Get Text (UNICODE or ANSI) from Clipboard
        private string GetClipboardText() {
            var clipboardData = Clipboard.GetDataObject();
            if (clipboardData != null) {
                if (clipboardData.GetDataPresent(DataFormats.UnicodeText)) {
                    return clipboardData.GetData(DataFormats.UnicodeText) as string;
                }
                if (clipboardData.GetDataPresent(DataFormats.Text)) {
                    return clipboardData.GetData(DataFormats.Text) as string;
                }
            }
            return null;
        }

        public CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
            IPoderosaView view;
            ITerminalSession session;
            if (!GetViewAndSession(target, out view, out session))
                return CommandResult.Ignored;

            string data = GetClipboardText();
            if (data == null)
                return CommandResult.Ignored;

            ITerminalEmulatorOptions options = TerminalSessionsPlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions;
            if (options.AlertOnPasteNewLineChar) {
                // Data will be split by CR, LF, CRLF or Environment.NewLine by TextReader.ReadLine,
                // So we check the data about CR, LF and Environment.NewLine.
                if (data.IndexOfAny(new char[] { '\r', '\n' }) >= 0 || data.Contains(Environment.NewLine)) {
                    IPoderosaForm form = view.ParentForm;
                    if (form != null) {
                        DialogResult res = form.AskUserYesNo(TEnv.Strings.GetString("Message.AskPasteNewLineChar"));
                        if (res != DialogResult.Yes) {
                            return CommandResult.Ignored;
                        }
                    }
                }
            }

            StringReader reader = new StringReader(data);
            TerminalTransmission output = session.TerminalTransmission;
            output.SendTextStream(reader, data.Length > 0 && data[data.Length - 1] == '\n');
            return CommandResult.Succeeded;
        }

        public bool CanExecute(ICommandTarget target) {
            IPoderosaView view;
            ITerminalSession session;
            if (!GetViewAndSession(target, out view, out session))
                return false;
            var clipboardData = Clipboard.GetDataObject();
            if (clipboardData == null || (
                       !clipboardData.GetDataPresent(DataFormats.UnicodeText)
                    && !clipboardData.GetDataPresent(DataFormats.Text))) {
                return false;
            }
            return true;
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        private bool GetViewAndSession(ICommandTarget target, out IPoderosaView view, out ITerminalSession session) {
            view = (IPoderosaView)target.GetAdapter(typeof(IPoderosaView));
            if (view != null && view.Document != null) {
                session = (ITerminalSession)view.Document.OwnerSession.GetAdapter(typeof(ITerminalSession));
                if (!session.TerminalConnection.IsClosed) {
                    return true;
                }
            }
            else {
                session = null;
            }
            return false;
        }
    }
}
