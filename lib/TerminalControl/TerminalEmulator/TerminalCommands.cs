/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalCommands.cs,v 1.5 2012/03/15 15:47:29 kzmi Exp $
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Poderosa.Plugins;
using Poderosa.Util;
using Poderosa.Forms;
using Poderosa.Document;
using Poderosa.Commands;
using Poderosa.Protocols;
using Poderosa.View;
using Poderosa.ConnectionParam;
using Poderosa.Sessions;

namespace Poderosa.Terminal {

    //CommandTargetのキャスト系
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public static class TerminalCommandTarget {
        public static IPoderosaMainWindow AsWindow(ICommandTarget target) {
            IPoderosaMainWindow window = (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
            return window;
        }
        public static IPoderosaView AsLastActivatedView(ICommandTarget target) {
            IPoderosaMainWindow window = AsWindow(target);
            if (window != null)
                return window.LastActivatedView;
            else {
                IPoderosaPopupWindow popup = (IPoderosaPopupWindow)target.GetAdapter(typeof(IPoderosaPopupWindow));
                if (popup != null)
                    return popup.InternalView;
                else
                    return null;
            }
        }

        //ちょっと恣意的だが、ビュー直接またはLastActivatedViewで
        public static IPoderosaView AsViewOrLastActivatedView(ICommandTarget target) {
            IPoderosaView view = (IPoderosaView)target.GetAdapter(typeof(IPoderosaView));
            if (view != null)
                return view; //成功
            else
                return AsLastActivatedView(target);
        }

        //CommandTargetからTerminalSessionを得る
        public static ITerminalControlHost AsTerminal(ICommandTarget target) {
            IPoderosaView view = AsViewOrLastActivatedView(target);
            if (view == null)
                return null;
            else {
                IPoderosaDocument doc = view.Document;
                if (doc == null)
                    return null;
                return (ITerminalControlHost)doc.OwnerSession.GetAdapter(typeof(ITerminalControlHost));
            }
        }
        public static ITerminalControlHost AsOpenTerminal(ICommandTarget target) {
            ITerminalControlHost s = AsTerminal(target);
            return (s == null || s.TerminalConnection.IsClosed) ? null : s;
        }
        public static TerminalControl AsTerminalControl(ICommandTarget target) {
            IPoderosaView view = AsViewOrLastActivatedView(target);
            if (view == null)
                return null;
            else
                return (TerminalControl)view.GetAdapter(typeof(TerminalControl));
        }
        public static CharacterDocumentViewer AsCharacterDocumentViewer(ICommandTarget target) {
            IPoderosaView view = AsViewOrLastActivatedView(target);
            if (view == null)
                return null;
            else
                return (CharacterDocumentViewer)view.GetAdapter(typeof(CharacterDocumentViewer));
        }

        internal static TerminalDocument AsTerminalDocument(ICommandTarget target) {
            IPoderosaView view = AsViewOrLastActivatedView(target);
            if (view == null)
                return null;
            else {
                IPoderosaDocument doc = view.Document;
                if (doc == null)
                    return null;
                else
                    return (TerminalDocument)doc.GetAdapter(typeof(TerminalDocument));
            }
        }
    }

    internal class CommandCategory : ICommandCategory, IPositionDesignation {
        private string _nameID;
        private bool _keybindCustomizable;
        private PositionType _positionType;
        private CommandCategory _designationTarget;

        public CommandCategory(string name) {
            _nameID = name;
            _keybindCustomizable = true;
            _positionType = PositionType.DontCare;
        }
        public CommandCategory(string name, bool customizable) {
            _nameID = name;
            _keybindCustomizable = customizable;
            _positionType = PositionType.DontCare;
        }
        public CommandCategory SetPosition(PositionType positiontype, CommandCategory target) {
            _positionType = positiontype;
            _designationTarget = target;
            return this;
        }

        public string Name {
            get {
                return GEnv.Strings.GetString(_nameID);
            }
        }

        public bool IsKeybindCustomizable {
            get {
                return _keybindCustomizable;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        public IAdaptable DesignationTarget {
            get {
                return _designationTarget;
            }
        }

        public PositionType DesignationPosition {
            get {
                return _positionType;
            }
        }
    }

    internal class TerminalCommand : GeneralCommandImpl {

        public TerminalCommand(string id, string description, CommandCategory category, ExecuteDelegate body, CanExecuteDelegate enabled)
            :
            base(id, GEnv.Strings, description, category, body, enabled) {
        }

        private static CommandCategory _terminal;
        private static CommandCategory _terminalEdit;
        private static CommandCategory _encoding;
        private static CommandCategory _hiddenTerminal;

        public static CommandCategory TerminalCommandCategory {
            get {
                return _terminal;
            }
        }

        public static void Register(ICommandManager cm) {
            _terminalEdit = new CommandCategory("CommandCategory.TerminalEdit");
            _terminal = new CommandCategory("CommandCategory.Terminal").SetPosition(PositionType.NextTo, _terminalEdit);
            _encoding = new CommandCategory("CommandCategory.Encoding");
            _hiddenTerminal = new CommandCategory("", false);

            //以下、編集メニュー内にあるもの
            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.copyaslook",
                "Command.CopyAsLook", _terminalEdit, new ExecuteDelegate(CmdCopyAsLook), DoesExistSelection));
            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.copytofile",
                "Command.CopyToFile", _terminalEdit, new ExecuteDelegate(CmdCopyToFile), DoesExistSelection));
            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.pastefromfile",
                "Command.PasteFromFile", _terminalEdit, new ExecuteDelegate(CmdPasteFromFile), DoesOpenTargetSession));

            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.clearbuffer",
                "Command.ClearBuffer", _terminalEdit, new ExecuteDelegate(CmdClearBuffer), TerminalCommand.DoesExistTargetSession));
            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.clearscreen",
                "Command.ClearScreen", _terminalEdit, new ExecuteDelegate(CmdClearScreen), TerminalCommand.DoesExistTargetSession));

            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.selectall",
                "Command.SelectAll", _terminalEdit, new ExecuteDelegate(CmdSelectAll), TerminalCommand.DoesExistCharacterDocumentViewer));

            //以下、コンソールメニュー内にあるもの
            //TODO いくつかはTerminalSessionにあるべき意味合いだ
            //cm.Register(new TerminalCommand("org.poderosa.terminalemulator.reproduce", new ExecuteDelegate(CmdReproduce), new EnabledDelegate(DoesOpenTargetSession)));

            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.newline.cr",
                "Command.NewLine.CR", _hiddenTerminal, new ExecuteDelegate(CmdNewLineCR), DoesOpenTargetSession));
            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.newline.lf",
                "Command.NewLine.LF", _hiddenTerminal, new ExecuteDelegate(CmdNewLineLF), DoesOpenTargetSession));
            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.newline.crlf",
                "Command.NewLine.CRLF", _hiddenTerminal, new ExecuteDelegate(CmdNewLineCRLF), DoesOpenTargetSession));

            foreach (EncodingType enc in Enum.GetValues(typeof(EncodingType))) {
                EncodingType encodingType = enc;
                cm.Register(
                    new TerminalCommand("org.poderosa.terminalemulator.encoding." + encodingType.ToString(),
                    "Command.Encoding." + encodingType.ToString(), _encoding,
                    delegate(ICommandTarget target) {
                        return CmdEncoding(target, encodingType);
                    },
                    DoesOpenTargetSession));
            }

            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.receivelinebreak",
                "Command.ReceiveLineBreak", _terminal, new ExecuteDelegate(CmdReceiveLineBreak), DoesOpenTargetSession));

            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.togglelocalecho",
                "Command.ToggleLocalEcho", _terminal, new ExecuteDelegate(CmdToggleLocalEcho), DoesOpenTargetSession));


            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.sendbreak",
                "Command.SendBreak", _terminal, new ExecuteDelegate(CmdSendBreak), DoesOpenTargetSession));
            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.sendAYT",
                "Command.AreYouThere", _terminal, new ExecuteDelegate(CmdSendAYT), DoesOpenTargetSession));
            cm.Register(new TerminalCommand("org.poderosa.terminalemulator.resetterminal",
                "Command.ResetTerminal", _terminal, new ExecuteDelegate(CmdResetTerminal), DoesOpenTargetSession));

            //IntelliSense
            cm.Register(new ToggleIntelliSenseCommand());
        }


        //以下コマンドの実装
        private static CommandResult CmdCopyAsLook(ICommandTarget target) {
            string t = GetSelectedText(target, TextFormatOption.AsLook);
            if (t == null)
                return CommandResult.Ignored;

            if (t.Length > 0)
                CopyToClipboard(t);
            return CommandResult.Succeeded;
        }
        private static string GetSelectedText(ICommandTarget target, TextFormatOption opt) {
            CharacterDocumentViewer c = TerminalCommandTarget.AsCharacterDocumentViewer(target);
            if (c == null)
                return null;
            ITextSelection s = c.ITextSelection;
            if (s.IsEmpty || !c.EnabledEx)
                return null;

            return s.GetSelectedText(opt);
        }
        private static void CopyToClipboard(string data) {
            try {
                Clipboard.SetDataObject(data, false);
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }
        private static CommandResult CmdCopyToFile(ICommandTarget target) {
            string text = GetSelectedText(target, TextFormatOption.Default);
            if (text == null)
                return CommandResult.Ignored;

            IPoderosaMainWindow window = TerminalCommandTarget.AsTerminal(target).OwnerWindow;
            SaveFileDialog dlg = new SaveFileDialog();
            //dlg.InitialDirectory = GApp.Options.DefaultFileDir;
            dlg.Title = GEnv.Strings.GetString("Util.DestinationToSave");
            dlg.Filter = "All Files(*.*)|*.*";
            if (dlg.ShowDialog(window.AsForm()) == DialogResult.OK) {
                StreamWriter wr = null;
                try {
                    wr = new StreamWriter(new FileStream(dlg.FileName, FileMode.Create), Encoding.Default);
                    wr.Write(text);
                    return CommandResult.Succeeded;
                }
                catch (Exception ex) {
                    window.Warning(ex.Message);
                    return CommandResult.Failed;
                }
                finally {
                    if (wr != null)
                        wr.Close();
                }
            }
            else
                return CommandResult.Cancelled;
        }
        private static CommandResult CmdPasteFromFile(ICommandTarget target) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = GEnv.Strings.GetString("Caption.SelectPasteFile");
            dlg.Filter = "All Files(*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK) {
                StreamReader r = null;
                try {
                    ITerminalControlHost host = TerminalCommandTarget.AsOpenTerminal(target);
                    if (host == null)
                        return CommandResult.Failed;
                    r = new StreamReader(dlg.FileName, Encoding.Default);
                    host.TerminalTransmission.SendTextStream(r, true);
                    return CommandResult.Succeeded;
                }
                catch (Exception ex) {
                    RuntimeUtil.ReportException(ex);
                    return CommandResult.Failed;
                }
                finally {
                    if (r != null)
                        r.Close();
                }
            }
            return CommandResult.Cancelled;
        }
        private static CommandResult CmdSelectAll(ICommandTarget target) {

            CharacterDocumentViewer tc = TerminalCommandTarget.AsCharacterDocumentViewer(target);
            if (tc == null)
                return CommandResult.Ignored;
            tc.ITextSelection.SelectAll();
            tc.Invalidate();
            return CommandResult.Succeeded;
        }

        private static CommandResult CmdClearBuffer(ICommandTarget target) {
            TerminalDocument doc = TerminalCommandTarget.AsTerminalDocument(target);
            if (doc == null)
                return CommandResult.Ignored;

            ITerminalControlHost session = TerminalCommandTarget.AsTerminal(target);
            TerminalControl tc = TerminalCommandTarget.AsTerminalControl(target);
            lock (doc) {
                doc.Clear();
                session.Terminal.AdjustTransientScrollBar();
                if (tc != null) {
                    tc.ITextSelection.Clear();
                    tc.Invalidate();
                }
            }
            return CommandResult.Succeeded;
        }

        private static CommandResult CmdClearScreen(ICommandTarget target) {
            TerminalDocument doc = TerminalCommandTarget.AsTerminalDocument(target);
            if (doc == null)
                return CommandResult.Ignored;

            TerminalControl tc = TerminalCommandTarget.AsTerminalControl(target);
            lock (doc) {
                GLine l = doc.TopLine;
                int top_id = l.ID;
                int limit = l.ID + doc.TerminalHeight;
                while (l != null && l.ID < limit) {
                    l.Clear();
                    l = l.NextLine;
                }
                doc.CurrentLineNumber = top_id;
                doc.CaretColumn = 0;
                doc.InvalidatedRegion.InvalidatedAll = true;
                if (tc != null) {
                    tc.ITextSelection.Clear();
                    tc.Invalidate();
                }
            }
            return CommandResult.Succeeded;
        }

        private static CommandResult CmdNewLineCR(ICommandTarget target) {
            return CmdNewLine(target, NewLine.CR);
        }
        private static CommandResult CmdNewLineLF(ICommandTarget target) {
            return CmdNewLine(target, NewLine.LF);
        }
        private static CommandResult CmdNewLineCRLF(ICommandTarget target) {
            return CmdNewLine(target, NewLine.CRLF);
        }
        private static CommandResult CmdNewLine(ICommandTarget target, NewLine nl) {
            ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
            if (ts == null)
                return CommandResult.Failed;
            ITerminalSettings settings = ts.TerminalSettings;
            settings.BeginUpdate();
            settings.TransmitNL = nl;
            settings.EndUpdate();
            return CommandResult.Succeeded;
        }
        private static CommandResult CmdEncoding(ICommandTarget target, EncodingType encoding) {
            ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
            if (ts == null)
                return CommandResult.Failed;
            ITerminalSettings settings = ts.TerminalSettings;
            settings.BeginUpdate();
            settings.Encoding = encoding;
            settings.EndUpdate();
            return CommandResult.Succeeded;
        }
        private static CommandResult CmdReceiveLineBreak(ICommandTarget target) {
            //TODO unimplemented
            return CommandResult.Ignored;
        }
        private static CommandResult CmdToggleLocalEcho(ICommandTarget target) {
            ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
            if (ts == null)
                return CommandResult.Failed;
            ITerminalSettings settings = ts.TerminalSettings;
            settings.BeginUpdate();
            settings.LocalEcho = !settings.LocalEcho;
            settings.EndUpdate();
            return CommandResult.Succeeded;
        }
        //TODO Break.AYTは接続種別による可・不可あるはず。EnabledDelegate側にも反映させよ
        private static CommandResult CmdSendBreak(ICommandTarget target) {
            ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
            if (ts == null)
                return CommandResult.Failed;
            ts.TerminalConnection.TerminalOutput.SendBreak();
            return CommandResult.Succeeded;
        }
        private static CommandResult CmdSendAYT(ICommandTarget target) {
            ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
            if (ts == null)
                return CommandResult.Failed;
            ts.TerminalConnection.TerminalOutput.AreYouThere();
            return CommandResult.Succeeded;
        }
        private static CommandResult CmdResetTerminal(ICommandTarget target) {
            ITerminalControlHost ts = TerminalCommandTarget.AsOpenTerminal(target);
            if (ts == null)
                return CommandResult.Failed;
            ts.Terminal.FullReset();
            return CommandResult.Succeeded;
        }


        //delegate util
        private static CanExecuteDelegate _doesExistTargetSession;
        private static CanExecuteDelegate _doesOpenTargetSession;
        private static CanExecuteDelegate _doesExistSelection;
        private static CanExecuteDelegate _doesExistCharacterDocumentViewer;

        public static CanExecuteDelegate DoesExistTargetSession {
            get {
                if (_doesExistTargetSession == null)
                    _doesExistTargetSession = delegate(ICommandTarget target) {
                        return TerminalCommandTarget.AsTerminal(target) != null;
                    };
                return _doesExistTargetSession;
            }
        }
        public static CanExecuteDelegate DoesOpenTargetSession {
            get {
                if (_doesOpenTargetSession == null)
                    _doesOpenTargetSession = delegate(ICommandTarget target) {
                        ITerminalControlHost s = TerminalCommandTarget.AsTerminal(target);
                        return s != null && !s.TerminalConnection.IsClosed;
                    };
                return _doesOpenTargetSession;
            }
        }
        public static CanExecuteDelegate DoesExistSelection {
            get {
                if (_doesExistSelection == null)
                    _doesExistSelection = delegate(ICommandTarget target) {
                        TerminalControl tc = TerminalCommandTarget.AsTerminalControl(target);
                        return tc != null && !tc.ITextSelection.IsEmpty;
                    };
                return _doesExistSelection;
            }
        }
        public static CanExecuteDelegate DoesExistCharacterDocumentViewer {
            get {
                if (_doesExistCharacterDocumentViewer == null) {
                    _doesExistCharacterDocumentViewer = delegate(ICommandTarget target) {
                        IPoderosaView view = CommandTargetUtil.AsViewOrLastActivatedView(target);
                        CharacterDocumentViewer control = view == null ? null : (CharacterDocumentViewer)view.GetAdapter(typeof(CharacterDocumentViewer));
                        return control != null;
                    };
                }
                return _doesExistCharacterDocumentViewer;
            }
        }

        public static StringResource StringResource {
            get {
                return GEnv.Strings;
            }
        }
    }

    /////////////////////////////////////////////
    // メニュー

    internal class StandardTerminalMenuItem : PoderosaMenuItemImpl {
        public StandardTerminalMenuItem(string textID, string commandID)
            : base(commandID, GEnv.Strings, textID) {
        }
        public StandardTerminalMenuItem(string textID, string commandID, CheckedDelegate cd)
            : base(commandID, GEnv.Strings, textID) {
            _checked = cd;
        }
    }

    internal abstract class StandardTerminalEmulatorMenuGroup : PoderosaMenuGroupImpl {
        public StandardTerminalEmulatorMenuGroup() {
            _positionType = PositionType.First;
            _designationTarget = null;
        }
    }

    internal class StandardTerminalEmulatorMenuFolder : IPoderosaMenuFolder {
        private string _textID;
        private EnabledDelegate _enabled;
        private IPoderosaMenuGroup[] _children;

        public StandardTerminalEmulatorMenuFolder(string textID, EnabledDelegate enabled, IPoderosaMenuGroup grp) {
            _textID = textID;
            _enabled = enabled;
            _children = new IPoderosaMenuGroup[] { grp };
        }
        public StandardTerminalEmulatorMenuFolder(string textID, EnabledDelegate enabled, IPoderosaMenuGroup[] groups) {
            _textID = textID;
            _enabled = enabled;
            _children = groups;
        }

        public IPoderosaMenuGroup[] ChildGroups {
            get {
                return _children;
            }
        }

        public string Text {
            get {
                return GEnv.Strings.GetString(_textID);
            }
        }

        public bool IsEnabled(ICommandTarget target) {
            return _enabled(target);
        }

        public bool IsChecked(ICommandTarget target) {
            return false;
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

    internal class BasicCopyPasteMenuGroup : StandardTerminalEmulatorMenuGroup {
        public override IPoderosaMenu[] ChildMenus {
            get {
                return new IPoderosaMenu[] {
                    new StandardTerminalMenuItem("Menu.Copy", "org.poderosa.core.edit.copy"),
                    new StandardTerminalMenuItem("Menu.Paste","org.poderosa.core.edit.paste"),
                };
            }
        }
    }

    internal class AdvancedCopyPasteMenuGroup : StandardTerminalEmulatorMenuGroup {
        public override IPoderosaMenu[] ChildMenus {
            get {
                return new IPoderosaMenu[] {
                    new StandardTerminalMenuItem("Menu.CopyAsLook",    "org.poderosa.terminalemulator.copyaslook"),
                    new StandardTerminalMenuItem("Menu.CopyToFile",    "org.poderosa.terminalemulator.copytofile"),
                    new StandardTerminalMenuItem("Menu.PasteFromFile", "org.poderosa.terminalemulator.pastefromfile")
                };
            }
        }
    }

    internal class TerminalBufferMenuGroup : StandardTerminalEmulatorMenuGroup {
        public override IPoderosaMenu[] ChildMenus {
            get {
                return new IPoderosaMenu[] {
                    new StandardTerminalMenuItem("Menu.ClearScreen", "org.poderosa.terminalemulator.clearscreen"),
                    new StandardTerminalMenuItem("Menu.ClearBuffer", "org.poderosa.terminalemulator.clearbuffer")
                };
            }
        }
    }

    internal class SelectionMenuGroup : StandardTerminalEmulatorMenuGroup {
        public override IPoderosaMenu[] ChildMenus {
            get {
                return new IPoderosaMenu[] {
                    new StandardTerminalMenuItem("Menu.SelectAll", "org.poderosa.terminalemulator.selectall")
                };
            }
        }
    }

    internal class TerminalSettingMenuGroup : StandardTerminalEmulatorMenuGroup {
        public override IPoderosaMenu[] ChildMenus {
            get {
                EnabledDelegate enabled = delegate(ICommandTarget target) {
                    return TerminalCommand.DoesOpenTargetSession(target);
                };
                return new IPoderosaMenu[] {
                    new StandardTerminalEmulatorMenuFolder("Menu.NewLine", enabled, new TransmitNLGroup()),
                    new StandardTerminalEmulatorMenuFolder("Menu.Encoding", enabled, new EncodingGroup()),
                    //new StandardTerminalMenuItem("Menu.LineFeedRule", "org.poderosa.terminalemulator.receivelinebreak"),
                    new StandardTerminalMenuItem("Menu.LocalEcho", "org.poderosa.terminalemulator.togglelocalecho",
                        delegate(ICommandTarget target) { return TerminalCommandTarget.AsTerminal(target).TerminalSettings.LocalEcho; }),
                    new StandardTerminalEmulatorMenuFolder("Menu.SendSpecial", enabled, GetTerminalSpecialMenuGroups())
                };
            }
        }

        private class TransmitNLGroup : StandardTerminalEmulatorMenuGroup {
            public override IPoderosaMenu[] ChildMenus {
                get {
                    return new IPoderosaMenu[] {
                        new StandardTerminalMenuItem("Enum.NewLine.CR", "org.poderosa.terminalemulator.newline.cr",
                            delegate(ICommandTarget target) { return TerminalCommandTarget.AsTerminal(target).TerminalSettings.TransmitNL==NewLine.CR; }), 
                        new StandardTerminalMenuItem("Enum.NewLine.LF", "org.poderosa.terminalemulator.newline.lf",
                            delegate(ICommandTarget target) { return TerminalCommandTarget.AsTerminal(target).TerminalSettings.TransmitNL==NewLine.LF; }), 
                        new StandardTerminalMenuItem("Enum.NewLine.CRLF", "org.poderosa.terminalemulator.newline.crlf",
                            delegate(ICommandTarget target) { return TerminalCommandTarget.AsTerminal(target).TerminalSettings.TransmitNL==NewLine.CRLF; })
                    };
                }
            }
        }

        private class EncodingGroup : StandardTerminalEmulatorMenuGroup {
            public override IPoderosaMenu[] ChildMenus {
                get {
                    List<IPoderosaMenu> menus = new List<IPoderosaMenu>();
                    foreach (EnumListItem<EncodingType> item in EnumListItem<EncodingType>.GetListItemsWithTextID()) {
                        EncodingType encodingType = item.Value;
                        string textID = item.ToString();   // key of the string resource
                        menus.Add(
                            new StandardTerminalMenuItem(textID,
                                "org.poderosa.terminalemulator.encoding." + encodingType.ToString(),
                                delegate(ICommandTarget target) {
                                    return TerminalCommandTarget.AsTerminal(target).TerminalSettings.Encoding == encodingType;
                                }));
                    }
                    return menus.ToArray();
                }
            }
        }

        private class TerminalSendSpecialGroup : StandardTerminalEmulatorMenuGroup {
            public override IPoderosaMenu[] ChildMenus {
                get {
                    return new IPoderosaMenu[] {
                        new StandardTerminalMenuItem("Menu.SendBreak", "org.poderosa.terminalemulator.sendbreak"),
                        new StandardTerminalMenuItem("Menu.AreYouThere", "org.poderosa.terminalemulator.sendAYT"),
                        new StandardTerminalMenuItem("Menu.ResetTerminal", "org.poderosa.terminalemulator.resetterminal")
                    };
                }
            }
        }
        //ExtensionPointからの取得を含めて構築
        private IPoderosaMenuGroup[] GetTerminalSpecialMenuGroups() {
            return (IPoderosaMenuGroup[])TerminalEmulatorPlugin.Instance.PoderosaWorld.PluginManager.FindExtensionPoint(TerminalEmulatorConstants.TERMINALSPECIAL_EXTENSIONPOINT).GetExtensions();
        }
        public static void Initialize() {
            IExtensionPoint p = TerminalEmulatorPlugin.Instance.PoderosaWorld.PluginManager.CreateExtensionPoint(TerminalEmulatorConstants.TERMINALSPECIAL_EXTENSIONPOINT, typeof(IPoderosaMenuGroup), TerminalEmulatorPlugin.Instance);
            p.RegisterExtension(new TerminalSendSpecialGroup());
        }
    }

}
