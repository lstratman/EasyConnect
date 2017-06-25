/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ShortcutFileCommands.cs,v 1.6 2011/10/27 23:21:59 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.IO;

using Poderosa.Plugins;
using Poderosa.Forms;
using Poderosa.Protocols;
using Poderosa.Commands;
using Poderosa.Terminal;
using Poderosa.View;

[assembly: PluginDeclaration(typeof(Poderosa.Sessions.ShortcutFilePlugin))]

namespace Poderosa.Sessions {
    //ShortcutFile Plugin
    [PluginInfo(ID = ShortcutFilePlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.terminalsessions;org.poderosa.cygwin")]
    internal class ShortcutFilePlugin : PluginBase, IFileDropHandler {
        public const string PLUGIN_ID = "org.poderosa.shortcutfile";

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);

            ICoreServices cs = (ICoreServices)poderosa.GetAdapter(typeof(ICoreServices));
            ShortcutFileCommands.Register(poderosa.PluginManager, cs.CommandManager);

            poderosa.PluginManager.FindExtensionPoint(WindowManagerConstants.FILEDROPHANDLER_ID).RegisterExtension(this);
        }

        public bool CanAccept(ICommandTarget target, string[] filenames) {
            return true; //ファイルの中身を検査する余裕はない
        }

        public void DoDropAction(ICommandTarget target, string[] filenames) {
            foreach (string fn in filenames) {
                ShortcutFileCommands.OpenShortcutFile(target, fn);
            }
        }
    }

    internal class ShortcutFileCommands {
        public static void Register(IPluginManager pm, ICommandManager cm) {
            StringResource sr = TEnv.Strings;
            ICommandCategory filecat = cm.CommandCategories.File;
            GeneralCommandImpl open = new GeneralCommandImpl("org.poderosa.sessions.openShortcutFile", sr, "Command.OpenShortcutFile", filecat, new ExecuteDelegate(OpenShortcutFile));
            GeneralCommandImpl save = new GeneralCommandImpl("org.poderosa.sessions.saveShortcutFile", sr, "Command.SaveShortcutFile", filecat, new ExecuteDelegate(SaveShortcutFile),
                delegate(ICommandTarget target) {
                    return TerminalCommandTarget.AsTerminal(target) != null;
                });

            cm.Register(open);
            cm.Register(save);

            IExtensionPoint filemenu = pm.FindExtensionPoint("org.poderosa.menu.file");
            filemenu.RegisterExtension(new PoderosaMenuGroupImpl(new IPoderosaMenu[] {
                new PoderosaMenuItemImpl(open, sr, "Menu.OpenShortcutFile"),
                new PoderosaMenuItemImpl(save, sr, "Menu.SaveShortcutFile") }).SetPosition(PositionType.NextTo, CygwinPlugin.Instance.CygwinMenuGroupTemp));

            ShortcutFileToolBarComponent tb = new ShortcutFileToolBarComponent(open, save);
            pm.FindExtensionPoint("org.poderosa.core.window.toolbar").RegisterExtension(tb);
            TerminalSessionsPlugin.Instance.SessionManager.AddActiveDocumentChangeListener(tb);
        }

        private class ShortcutFileToolBarComponent : IToolBarComponent, IPositionDesignation, IActiveDocumentChangeListener {
            private IGeneralCommand _openCommand;
            private IGeneralCommand _saveCommand;

            public ShortcutFileToolBarComponent(IGeneralCommand open, IGeneralCommand save) {
                _openCommand = open;
                _saveCommand = save;
            }

            public IAdaptable DesignationTarget {
                get {
                    return CygwinPlugin.Instance.CygwinToolBarComponentTemp;
                }
            }

            public PositionType DesignationPosition {
                get {
                    return PositionType.NextTo;
                }
            }

            public bool ShowSeparator {
                get {
                    return true;
                }
            }

            public IToolBarElement[] ToolBarElements {
                get {
                    return new IToolBarElement[] {
                        new ToolBarCommandButtonImpl(_openCommand, Poderosa.TerminalSession.Properties.Resources.Open16x16),
                        new ToolBarCommandButtonImpl(_saveCommand, Poderosa.TerminalSession.Properties.Resources.Save16x16)
                    };
                }
            }

            public IAdaptable GetAdapter(Type adapter) {
                return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }
            #region IActiveDocumentChangeListener
            public void OnDocumentActivated(IPoderosaMainWindow window, IPoderosaDocument document) {
                window.ToolBar.RefreshComponent(this);
            }
            public void OnDocumentDeactivated(IPoderosaMainWindow window) {
                window.ToolBar.RefreshComponent(this);
            }
            #endregion

        }

        public static CommandResult OpenShortcutFile(ICommandTarget target, string filename) {
            IPoderosaMainWindow window = CommandTargetUtil.AsWindow(target);
            if (window == null)
                window = (IPoderosaMainWindow)CommandTargetUtil.AsViewOrLastActivatedView(target).ParentForm.GetAdapter(typeof(IPoderosaMainWindow));
            if (window == null)
                return CommandResult.Ignored;

            if (!File.Exists(filename)) {
                window.Warning(String.Format("{0} is not a file", filename));
                return CommandResult.Failed;
            }

            ShortcutFileContent f = null;
            try {
                f = ShortcutFileContent.LoadFromXML(filename);
            }
            catch (Exception ex) {
                //変なファイルをドロップしたなどで例外は簡単に起こりうる
                window.Warning(String.Format("Failed to read {0}\n{1}", filename, ex.Message));
                return CommandResult.Failed;
            }

            try {
                //独立ウィンドウにポップアップさせるようなことは考えていない
                IContentReplaceableView rv = (IContentReplaceableView)target.GetAdapter(typeof(IContentReplaceableView));
                if (rv == null) {
                    rv = (IContentReplaceableView)window.ViewManager.GetCandidateViewForNewDocument().GetAdapter(typeof(IContentReplaceableView));
                }

                TerminalControl tc = (TerminalControl)rv.GetCurrentContent().GetAdapter(typeof(TerminalControl));
                if (tc != null) { //ターミナルコントロールがないときは無理に設定しにいかない
                    RenderProfile rp = f.TerminalSettings.UsingDefaultRenderProfile ? TerminalSessionsPlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions.CreateRenderProfile() : f.TerminalSettings.RenderProfile;
                    Size sz = tc.CalcTerminalSize(rp);
                    f.TerminalParameter.SetTerminalSize(sz.Width, sz.Height);
                }

                ITerminalSession s = TerminalSessionsPlugin.Instance.TerminalSessionStartCommand.StartTerminalSession(target, f.TerminalParameter, f.TerminalSettings);
                return s != null ? CommandResult.Succeeded : CommandResult.Failed;
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
                return CommandResult.Failed;
            }
        }


        //コマンド本体
        private static CommandResult OpenShortcutFile(ICommandTarget target) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = TEnv.Strings.GetString("Caption.OpenShortcutFile");
            dlg.Multiselect = false;
            dlg.DefaultExt = "gts";
            dlg.AddExtension = true;
            dlg.Filter = "Terminal Shortcut(*.gts)|*.gts|All Files(*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK) {
                return OpenShortcutFile(target, dlg.FileName);
            }

            return CommandResult.Cancelled;
        }
        private static CommandResult SaveShortcutFile(ICommandTarget target) {
            ITerminalControlHost t = TerminalCommandTarget.AsTerminal(target);
            if (t == null)
                return CommandResult.Failed;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = TEnv.Strings.GetString("Caption.SaveShortcutFile");
            dlg.DefaultExt = "gts";
            dlg.AddExtension = true;
            dlg.Filter = "Terminal Shortcut(*.gts)|*.gts";

            if (dlg.ShowDialog() == DialogResult.OK) {
                try {
                    new ShortcutFileContent(t.TerminalSettings, t.TerminalConnection.Destination).SaveToXML(dlg.FileName);
                    return CommandResult.Succeeded;
                }
                catch (Exception ex) {
                    RuntimeUtil.ReportException(ex);
                    return CommandResult.Failed;
                }
            }
            else
                return CommandResult.Cancelled;
        }

    }
}
