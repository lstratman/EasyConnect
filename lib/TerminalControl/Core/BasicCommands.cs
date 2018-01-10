/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: BasicCommands.cs,v 1.5 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

using Poderosa.Forms;
using Poderosa.Sessions;
using Poderosa.Document;
using Poderosa.UI;
using Poderosa.View;

namespace Poderosa.Commands {
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
        public CommandCategory SetPosition(PositionType positiontype, CommandCategory target) {
            _positionType = positiontype;
            _designationTarget = target;
            return this;
        }

        public string Name {
            get {
                return CoreUtil.Strings.GetString(_nameID);
            }
        }

        public bool IsKeybindCustomizable {
            get {
                return _keybindCustomizable;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
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

    // コマンド群
    internal class BasicCommand : GeneralCommandImpl {

        public BasicCommand(string id, string description, CommandCategory category, Keys defaultkey, ExecuteDelegate body, CanExecuteDelegate enabled)
            :
            base(id, CoreUtil.Strings, description, category, body, enabled) {
            _defaultShortcutKey = defaultkey;
        }
        public BasicCommand(string id, string description, CommandCategory category, Keys defaultkey, ExecuteDelegate body)
            :
            base(id, CoreUtil.Strings, description, category, body) {
            _defaultShortcutKey = defaultkey;
        }

    }


    internal static class BasicCommandImplementation {
        private static BasicCommand _closeAll;
        private static DocActivationCommand _docActivationCommand;

        //この２つはなぜ要るんだっけ
        public static BasicCommand CloseAll {
            get {
                return _closeAll;
            }
        }
        public static DocActivationCommand DocActivationCommand {
            get {
                return _docActivationCommand;
            }
        }

        internal class DefaultCommandCategories : IDefaultCommandCategories {
            public ICommandCategory File {
                get {
                    return _file;
                }
            }

            public ICommandCategory Dialogs {
                get {
                    return _dialog;
                }
            }

            public ICommandCategory Edit {
                get {
                    return _edit;
                }
            }

            public ICommandCategory Window {
                get {
                    return _window;
                }
            }
        }

        private static DefaultCommandCategories _commandCategories = new DefaultCommandCategories();
        private static CommandCategory _file;
        private static CommandCategory _edit;
        private static CommandCategory _window;
        private static CommandCategory _dialog;

        //Coreコンポーネントで
        public static void Build() {
            CreateCategories();

            ICommandManager cm = CommandManagerPlugin.Instance;
            cm.Register(new BasicCommand("org.poderosa.core.application.newwindow",
                "Command.NewWindow", _file, CtrlShift(Keys.N), new ExecuteDelegate(CmdNewWindow)));
            cm.Register(new BasicCommand("org.poderosa.core.application.quit",
                "Command.Quit", _file, CtrlShift(Keys.W), new ExecuteDelegate(CmdQuit)));

            cm.Register(new BasicCommand("org.poderosa.core.edit.copy",
                "Command.Copy", _edit, Alt(Keys.C), new ExecuteDelegate(CmdCopy), new CanExecuteDelegate(CanCopy)));
            cm.Register(new BasicCommand("org.poderosa.core.edit.paste",
                "Command.Paste", _edit, Alt(Keys.V), new ExecuteDelegate(CmdPaste), new CanExecuteDelegate(CanPaste)));

            cm.Register(new BasicCommand("org.poderosa.core.session.closedocument",
                "Command.CloseDocument", _window, Alt(Keys.W), new ExecuteDelegate(CmdCloseCurrentDocument), DoesExistCurrentDocument));
            cm.Register(_closeAll = new BasicCommand("org.poderosa.core.window.closeall",
                "Command.CloseAll", _window, Keys.None, new ExecuteDelegate(CmdCloseAll), DoesExistAnyDocument));

            cm.Register(new BasicCommand("org.poderosa.core.window.splithorizontal",
                "Command.SplitHorizontal", _window, Alt(Keys.H), new ExecuteDelegate(CmdSplitHorizontal), new CanExecuteDelegate(CanSplit)));
            cm.Register(new BasicCommand("org.poderosa.core.window.splitvertical",
                "Command.SplitVertical", _window, Alt(Keys.J), new ExecuteDelegate(CmdSplitVertical), new CanExecuteDelegate(CanSplit)));
            cm.Register(new BasicCommand("org.poderosa.core.window.splitunify",
                "Command.SplitUnify", _window, Alt(Keys.U), new ExecuteDelegate(CmdSplitUnify), new CanExecuteDelegate(CanSplitUnify)));
            cm.Register(new BasicCommand("org.poderosa.core.window.unifyall",
                "Command.UnifyAll", _window, Keys.None, new ExecuteDelegate(CmdUnifyAll), new CanExecuteDelegate(CanUnifyAll)));

            cm.Register(new BasicCommand("org.poderosa.core.window.nexttab",
                "Command.NextTab", _window, Keys.None, new ExecuteDelegate(CmdNextTab), DoesExistAnyDocument));
            cm.Register(new BasicCommand("org.poderosa.core.window.prevtab",
                "Command.PreviousTab", _window, Keys.None, new ExecuteDelegate(CmdPrevTab), DoesExistAnyDocument));

            cm.Register(new BasicCommand("org.poderosa.core.dialog.pluginlist",
                "Command.PluginList", _dialog, Keys.None, new ExecuteDelegate(CmdPluginList)));
            cm.Register(new BasicCommand("org.poderosa.core.dialog.extensionpointlist",
                "Command.ExtensionPointList", _dialog, Keys.None, new ExecuteDelegate(CmdExtensionPointList)));
            cm.Register(new BasicCommand("org.poderosa.core.dialog.aboutbox",
                "Command.AboutBox", _dialog, Keys.None, new ExecuteDelegate(CmdAboutBox)));
            cm.Register(new BasicCommand("org.poderosa.core.application.openweb",
                "Command.PoderosaWeb", _dialog, Keys.None, new ExecuteDelegate(CmdOpenWeb)));

            //これはGeneralCommandではない
            _docActivationCommand = new DocActivationCommand();
        }

        public static IDefaultCommandCategories DefaultCategories {
            get {
                return _commandCategories;
            }
        }

        private static void CreateCategories() {
            _file = new CommandCategory("CommandCategory.Application").SetPosition(PositionType.First, null);
            _edit = new CommandCategory("CommandCategory.Edit").SetPosition(PositionType.NextTo, _file);
            _window = new CommandCategory("CommandCategory.Window").SetPosition(PositionType.NextTo, _edit);
            _dialog = new CommandCategory("CommandCategory.Dialog").SetPosition(PositionType.NextTo, _window);
        }

        private static CommandResult CmdNewWindow(ICommandTarget target) {
            IPoderosaMainWindow window = CommandTargetUtil.AsWindow(target);
            Form f = window.AsForm();
            Rectangle location = f.WindowState == FormWindowState.Normal ? f.DesktopBounds : f.RestoreBounds;
            location.X += 20;
            location.Y += 20; //少し右下に表示
            MainWindowArgument arg = new MainWindowArgument(location, FormWindowState.Normal, "", "", 1);
            WindowManagerPlugin.Instance.CreateNewWindow(arg);
            return CommandResult.Succeeded;
        }
        private static CommandResult CmdQuit(ICommandTarget target) {
            WindowManagerPlugin p = WindowManagerPlugin.Instance;
            return p.CloseAllWindows();
        }

        private static CommandResult CmdCopy(ICommandTarget target) {
            IPoderosaView view = CommandTargetUtil.AsViewOrLastActivatedView(target);
            IPoderosaCommand cmd = GetCopyCommand(view);
            return cmd == null ? CommandResult.Ignored : cmd.InternalExecute(view);
        }
        private static CommandResult CmdPaste(ICommandTarget target) {
            IPoderosaView view = CommandTargetUtil.AsViewOrLastActivatedView(target);
            IPoderosaCommand cmd = GetPasteCommand(view);
            return cmd == null ? CommandResult.Ignored : cmd.InternalExecute(view);
        }
        private static bool CanCopy(ICommandTarget target) {
            IPoderosaView view = CommandTargetUtil.AsViewOrLastActivatedView(target);
            IPoderosaCommand cmd = GetCopyCommand(view);
            return cmd == null ? false : cmd.CanExecute(view);
        }
        private static bool CanPaste(ICommandTarget target) {
            IPoderosaView view = CommandTargetUtil.AsViewOrLastActivatedView(target);
            IPoderosaCommand cmd = GetPasteCommand(view);
            return cmd == null ? false : cmd.CanExecute(view);
        }
        private static IPoderosaCommand GetCopyCommand(IPoderosaView view) {
            IGeneralViewCommands cmds = CommandTargetUtil.AsGeneralViewCommands(view);
            return cmds == null ? null : cmds.Copy;
        }
        private static IPoderosaCommand GetPasteCommand(IPoderosaView view) {
            IGeneralViewCommands cmds = CommandTargetUtil.AsGeneralViewCommands(view);
            return cmds == null ? null : cmds.Paste;
        }

        private static CommandResult CmdCloseCurrentDocument(ICommandTarget target) {
            IPoderosaDocument document = CommandTargetUtil.AsDocumentOrViewOrLastActivatedDocument(target);
            if (document == null)
                return CommandResult.Ignored;

            SessionManagerPlugin sm = SessionManagerPlugin.Instance;
            IPoderosaView view = sm.FindDocumentHost(document).LastAttachedView;
            IPoderosaMainWindow window = view == null ? null : (IPoderosaMainWindow)view.ParentForm.GetAdapter(typeof(IPoderosaMainWindow));
            bool was_active = window == null ? false : window.DocumentTabFeature.ActiveDocument == document;

            PrepareCloseResult result = sm.CloseDocument(document);
            if (result == PrepareCloseResult.Cancel)
                return CommandResult.Cancelled;

            //同じビューに別のドキュメントが来ていればそれをアクティブに
            if (was_active) {
                IPoderosaDocument newdoc = view.Document;
                if (newdoc != null)
                    sm.ActivateDocument(newdoc, ActivateReason.InternalAction);
            }

            return CommandResult.Succeeded;
        }
        private static CommandResult CmdCloseAll(ICommandTarget target) {
            IPoderosaMainWindow window = CommandTargetUtil.AsWindow(target);
            if (window == null)
                return CommandResult.Ignored;

            IPoderosaDocument[] hosted_documents = SessionManagerPlugin.Instance.GetDocuments(window);
            PrepareCloseResult r = SessionManagerPlugin.Instance.CloseMultipleDocuments(new ClosingContext(window), hosted_documents);
            return r == PrepareCloseResult.Cancel ? CommandResult.Cancelled : CommandResult.Succeeded;
        }

        private static CommandResult CmdSplitHorizontal(ICommandTarget target) {
            IContentReplaceableView view = CommandTargetUtil.AsContentReplaceableViewOrLastActivatedView(target);
            if (view == null)
                return CommandResult.Ignored;

            ISplittableViewManager svm = (ISplittableViewManager)view.ViewManager.GetAdapter(typeof(ISplittableViewManager));
            return svm.SplitHorizontal(view, null);
        }

        private static CommandResult CmdSplitVertical(ICommandTarget target) {
            IContentReplaceableView view = CommandTargetUtil.AsContentReplaceableViewOrLastActivatedView(target);
            if (view == null)
                return CommandResult.Ignored;

            ISplittableViewManager svm = (ISplittableViewManager)view.ViewManager.GetAdapter(typeof(ISplittableViewManager));
            return svm.SplitVertical(view, null);
        }
        private static bool CanSplit(ICommandTarget target) {
            IContentReplaceableView view = CommandTargetUtil.AsContentReplaceableViewOrLastActivatedView(target);
            if (view == null)
                return false;

            ISplittableViewManager svm = (ISplittableViewManager)view.ViewManager.GetAdapter(typeof(ISplittableViewManager));
            return svm.CanSplit(view);
        }

        private static CommandResult CmdSplitUnify(ICommandTarget target) {
            IContentReplaceableView view = CommandTargetUtil.AsContentReplaceableViewOrLastActivatedView(target);
            if (view == null)
                return CommandResult.Ignored;

            ISplittableViewManager svm = (ISplittableViewManager)view.ViewManager.GetAdapter(typeof(ISplittableViewManager));
            IContentReplaceableView next = null;
            IPoderosaDocument document_unifying = view.Document;
            CommandResult r = svm.Unify(view, out next);

            if (r == CommandResult.Succeeded) {
                ISessionManager sm = SessionManagerPlugin.Instance;
                ISessionManagerForViewSplitter smp = SessionManagerPlugin.Instance;
                smp.ChangeLastAttachedViewForAllDocuments(view, next);

                //次のフォーカスのドキュメントがなければ旧ドキュメントを移行。そしてnextのドキュメントをアクティブに
                if (document_unifying != null && next.Document == null) {
                    sm.AttachDocumentAndView(document_unifying, next);
                    Debug.Assert(next.Document == document_unifying);
                }

                if (next.Document != null)
                    sm.ActivateDocument(next.Document, ActivateReason.InternalAction);
            }
            return r;
        }
        private static CommandResult CmdUnifyAll(ICommandTarget target) {
            IContentReplaceableView view = CommandTargetUtil.AsContentReplaceableViewOrLastActivatedView(target);
            if (view == null)
                return CommandResult.Ignored;

            IPoderosaDocument doc = view.Document;
            ISplittableViewManager svm = (ISplittableViewManager)view.ViewManager.GetAdapter(typeof(ISplittableViewManager));
            IContentReplaceableView next = null;

            CommandResult r = svm.UnifyAll(out next);
            if (r == CommandResult.Succeeded) {
                ISessionManager sm = SessionManagerPlugin.Instance;
                ISessionManagerForViewSplitter smp = SessionManagerPlugin.Instance;
                smp.ChangeLastAttachedViewForWindow(view.ViewManager.ParentWindow, next);
                if (doc != null)
                    sm.ActivateDocument(doc, ActivateReason.InternalAction);
            }
            return r;
        }

        private static bool CanSplitUnify(ICommandTarget target) {
            IContentReplaceableView view = CommandTargetUtil.AsContentReplaceableViewOrLastActivatedView(target);
            if (view == null)
                return false;

            ISplittableViewManager svm = (ISplittableViewManager)view.ViewManager.GetAdapter(typeof(ISplittableViewManager));
            return svm.CanUnify(view);
        }
        private static bool CanUnifyAll(ICommandTarget target) {
            IPoderosaMainWindow window = CommandTargetUtil.AsWindow(target);
            if (window == null)
                return false;

            ISplittableViewManager svm = (ISplittableViewManager)window.ViewManager.GetAdapter(typeof(ISplittableViewManager));
            return svm.IsSplitted();
        }

        // Tab switch
        private static CommandResult CmdNextTab(ICommandTarget target) {
            IContentReplaceableView view = CommandTargetUtil.AsContentReplaceableViewOrLastActivatedView(target);
            if (view == null)
                return CommandResult.Ignored;

            view.ViewManager.ParentWindow.DocumentTabFeature.ActivateNextTab();
            return CommandResult.Succeeded;
        }

        private static CommandResult CmdPrevTab(ICommandTarget target) {
            IContentReplaceableView view = CommandTargetUtil.AsContentReplaceableViewOrLastActivatedView(target);
            if (view == null)
                return CommandResult.Ignored;

            view.ViewManager.ParentWindow.DocumentTabFeature.ActivatePrevTab();
            return CommandResult.Succeeded;
        }

        //プラグインリスト表示のメニューとコマンド
        private static CommandResult CmdPluginList(ICommandTarget target) {
            IPoderosaMainWindow window = CommandTargetUtil.AsWindow(target);
            if (window == null)
                return CommandResult.Ignored;

            PluginList dlg = new PluginList();
            dlg.ShowDialog(window.AsForm());
            return CommandResult.Succeeded;
        }
        private static CommandResult CmdExtensionPointList(ICommandTarget target) {
            IPoderosaMainWindow window = CommandTargetUtil.AsWindow(target);
            if (window == null)
                return CommandResult.Ignored;

            ExtensionPointList dlg = new ExtensionPointList();
            dlg.ShowDialog(window.AsForm());
            return CommandResult.Succeeded;
        }

        //AboutBox表示のメニューとコマンド
        private static CommandResult CmdAboutBox(ICommandTarget target) {
            IPoderosaMainWindow window = CommandTargetUtil.AsWindow(target);
            if (window == null)
                return CommandResult.Ignored;

            IPoderosaAboutBoxFactory f = AboutBoxUtil.GetCurrentAboutBoxFactory();
            if (f != null) {
                AboutBoxUtil.ResetKeyBufferInAboutBox();
                f.CreateAboutBox().ShowDialog(window.AsForm());
                return CommandResult.Succeeded;
            }
            else {
                return CommandResult.Failed;
            }
        }
        private static CommandResult CmdOpenWeb(ICommandTarget target) {
            return CommandResult.Succeeded;
        }

        //delegate util
        public static CanExecuteDelegate DoesExistCurrentDocument {
            get {
                return delegate(ICommandTarget target) {
                    return CommandTargetUtil.AsDocumentOrViewOrLastActivatedDocument(target) != null;
                };
            }
        }
        public static CanExecuteDelegate DoesExistAnyDocument {
            get {
                return delegate(ICommandTarget target) {
                    IPoderosaMainWindow window = CommandTargetUtil.AsWindow(target);
                    return window != null && window.DocumentTabFeature.DocumentCount > 0;
                };
            }
        }

        //キーのモディファイア
        private static Keys Alt(Keys key) {
            return Keys.Alt | key;
        }
        private static Keys CtrlShift(Keys key) {
            return Keys.Control | Keys.Shift | key;
        }
    }


    //メインメニューの項目は、ICommandTarget経由で起動元のフォームが取れるようになっている。
    /// <summary>
    /// <ja>
    /// コマンドの実行時に渡されるターゲットをさまざまな型に変換する静的なメソッドを提供します。
    /// </ja>
    /// <en>
    /// A static method of converting the target passed when the command is executed into various types is offered. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// メインメニューやツールバーから呼び出される場合、ターゲットはメインウィンドウを示す<seealso cref="IPoderosaMainWindow">IPoderosaMainWindow</seealso>です。
    /// </para>
    /// <para>
    /// ポップアップウィンドウから呼び出される場合、ターゲットはポップアップウィンドウを示す<seealso cref="IPoderosaPopupWindow">IPoderosaPopupWindow</seealso>です。
    /// </para>
    /// <para>
    /// このクラスに実装されている静的なメソッドを用いることで、渡されたターゲットを<seealso cref="IPoderosaMainWindow">IPoderosaMainWindow</seealso>、<seealso cref="IPoderosaView">IPoderosaView</seealso>などへと変換できます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// The target is <seealso cref="IPoderosaMainWindow">IPoderosaMainWindow</seealso> that shows the main window when called from the main menu and the toolbar. 
    /// </para>
    /// <para>
    /// The target is <seealso cref="IPoderosaPopupWindow">IPoderosaPopupWindow</seealso> that shows the pop up window when called from the pop up window. 
    /// </para>
    /// <para>
    /// The passed target can be converted into <seealso cref="IPoderosaMainWindow">IPoderosaMainWindow</seealso> and <seealso cref="IPoderosaView">IPoderosaView</seealso>, etc. by using the static method being implemented by this class. 
    /// </para>
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// コマンド実行時に渡された<var>target</var>を有効なビューへと変換します。
    /// </ja>
    /// <en>
    /// <var>target</var> passed when the command is executed is converted into an effective view. 
    /// </en>
    /// <code>
    /// IPoderosaView view = CommandTargetUtil.AsViewOrLastActivatedView(<var>target</var>);
    /// </code>
    /// </example>
    public class CommandTargetUtil {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ICommandTarget AsCommandTarget(IAdaptable obj) {
            if (obj == null)
                return null;
            else
                return (ICommandTarget)obj.GetAdapter(typeof(ICommandTarget));
        }


        /// <summary>
        /// <ja>
        /// ウィンドウへと変換します。
        /// </ja>
        /// <en>
        /// Convert to the window.
        /// </en>
        /// </summary>
        /// <param name="target">
        /// <ja>対象となるターゲットです。</ja>
        /// <en>It is a target that becomes an object. </en>
        /// </param>
        /// <returns>
        /// <ja>変換したインターフェイスです。変換できないときにはnullが戻ります。</ja>
        /// <en>It is a converted interface. Null returns when it is not possible to convert it. </en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// メニューやツールバーのボタンから呼び出されたコマンドに引き渡された<paramref name="target">target</paramref>の場合、この呼び出しは成功します。
        /// </ja>
        /// <en>
        /// This call succeeds for <paramref name="target">target</paramref> handed over to the command called from the button of the menu and the toolbar. 
        /// </en>
        /// </remarks>
        public static IPoderosaMainWindow AsWindow(ICommandTarget target) {
            if (target == null)
                return null;
            IPoderosaMainWindow window = (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
            return window;
        }
        /// <summary>
        /// <ja>
        /// ポップアップウィンドウへと変換します。
        /// </ja>
        /// <en>
        /// Convert to the popup window.
        /// </en>
        /// </summary>
        /// <param name="target">
        /// <ja>対象となるターゲットです。</ja>
        /// <en>It is a target that becomes an object. </en>
        /// </param>
        /// <returns>
        /// <ja>変換したインターフェイスです。変換できないときにはnullが戻ります。</ja>
        /// <en>It is a converted interface. Null returns when it is not possible to convert it. </en>
        /// </returns>
        public static IPoderosaPopupWindow AsPopupWindow(ICommandTarget target) {
            if (target == null)
                return null;
            IPoderosaPopupWindow window = (IPoderosaPopupWindow)target.GetAdapter(typeof(IPoderosaPopupWindow));
            return window;
        }
        /// <summary>
        /// <ja>
        /// 最後にアクティブになったビューへと変換します。
        /// </ja>
        /// <en>
        /// Convert it into the view that became active at the end. 
        /// </en>
        /// </summary>
        /// <param name="target">
        /// <ja>対象となるターゲットです。</ja>
        /// <en>It is a target that becomes an object. </en>
        /// </param>
        /// <returns>
        /// <ja>変換したインターフェイスです。変換できないときにはnullが戻ります。</ja>
        /// <en>It is a converted interface. Null returns when it is not possible to convert it. </en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ウィンドウの場合には、<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>の
        /// <see cref="IPoderosaMainWindow.LastActivatedView">LastActivatedViewプロパティ</see>を用いてビューへと変換します。
        /// </para>
        /// <para>
        /// ポップアップウィンドウの場合には、<see cref="IPoderosaPopupWindow">IPoderosaPopupWindow</see>の
        /// <see cref="IPoderosaPopupWindow.InternalView">InternalViewプロパティ</see>を用いてビューへと変換します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// It converts it into the view by using the <see cref="IPoderosaPopupWindow.InternalView">InternalView property</see> 
        /// of <see cref="IPoderosaPopupWindow">IPoderosaPopupWindow</see> for the window. 
        /// </para>
        /// <para>
        /// It converts it into the view by using the <see cref="IPoderosaPopupWindow.InternalView">InternalView property</see> 
        /// of <see cref="IPoderosaPopupWindow">IPoderosaPopupWindow</see> for the pop up window. 
        /// </para>
        /// </en>
        /// </remarks>
        public static IPoderosaView AsLastActivatedView(ICommandTarget target) {
            IPoderosaMainWindow window = AsWindow(target);
            if (window != null)
                return window.LastActivatedView;
            else {
                IPoderosaPopupWindow popup = AsPopupWindow(target);
                if (popup != null)
                    return popup.InternalView;
                else
                    return null;
            }
        }

        //ちょっと恣意的だが、ビュー直接またはLastActivatedViewで
        /// <summary>
        /// <ja>
        /// ビューまたは最後にアクティブであったビューに変換します。
        /// </ja>
        /// <en>
        /// It converts it into an active view at the view or the end. 
        /// </en>
        /// </summary>
        /// <param name="target">
        /// <ja>対象となるターゲットです。</ja>
        /// <en>It is a target that becomes an object. </en>
        /// </param>
        /// <returns>
        /// <ja>変換したインターフェイスです。変換できないときにはnullが戻ります。</ja>
        /// <en>It is a converted interface. Null returns when it is not possible to convert it. </en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// このメソッドは、まず、<paramref name="target"/>をIPoderosaViewに変換しようとし、変換できたならそのインターフェイスを返します。
        /// 変換できない場合には、<see cref="AsLastActivatedView">AsLastActivatedViewメソッド</see>の戻り値をそのまま返します。
        /// </ja>
        /// <en>
        /// If it tries to convert <paramref name="target"/> into IPoderosaView first of all, and it was possible to convert it, this method returns the interface. 
        /// When it is not possible to convert it, the return value of the <see cref="AsLastActivatedView">AsLastActivatedView method</see> is returned as it is. 
        /// </en>
        /// </remarks>
        public static IPoderosaView AsViewOrLastActivatedView(ICommandTarget target) {
            if (target == null)
                return null;
            IPoderosaView view = (IPoderosaView)target.GetAdapter(typeof(IPoderosaView));
            if (view != null)
                return view; //成功
            else
                return AsLastActivatedView(target);
        }

        //ドキュメント直接指定か、Viewか、ウィンドウの最後にアクティブになったドキュメント
        /// <summary>
        /// <ja>
        /// ドキュメントまたは最後にアクティブであったドキュメントに変換します。
        /// </ja>
        /// <en>
        /// It converts it at the document or the end into an active document. 
        /// </en>
        /// </summary>
        /// <param name="target">
        /// <ja>対象となるターゲット。</ja>
        /// <en>It is a target that becomes an object. </en>
        /// </param>
        /// <returns>
        /// <ja>変換したインターフェイスです。変換できないときにはnullが戻ります。</ja>
        /// <en>It is a converted interface. Null returns when it is not possible to convert it. </en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// このメソッドは、まず、<paramref name="target"/>をIPoderosaDocumentに変換しようとし、変換できたならそのインターフェイスを返します。
        /// 変換できない場合には、<see cref="AsViewOrLastActivatedView">AsViewOrLastActivatedViewメソッド</see>を呼び出して得たビューの<see cref="IPoderosaView.Document">Documentプロパティ</see>
        /// からドキュメントを得ます。
        /// </ja>
        /// <en>
        /// If it tries to convert <paramref name="target"/> into IPoderosaDocument first of all, and it was possible to convert it, this method returns the interface. 
        /// The document is obtained from the <see cref="IPoderosaView.Document">Document property</see> of the view that calls the <see cref="AsViewOrLastActivatedView">AsViewOrLastActivatedView method</see> when it is not possible to convert it and obtains it. 
        /// </en>
        /// </remarks>
        public static IPoderosaDocument AsDocumentOrViewOrLastActivatedDocument(ICommandTarget target) {
            IPoderosaDocument doc = null;
            if (target != null)
                doc = (IPoderosaDocument)target.GetAdapter(typeof(IPoderosaDocument));

            if (doc != null)
                return doc;
            else {
                IPoderosaView view = AsViewOrLastActivatedView(target);
                if (view != null)
                    return view.Document;
                else
                    return null;
            }
        }


        /// <exclude/>
        public static IContentReplaceableView AsContentReplaceableViewOrLastActivatedView(ICommandTarget target) {
            if (target == null)
                return null;
            IContentReplaceableViewSite view = (IContentReplaceableViewSite)target.GetAdapter(typeof(IContentReplaceableViewSite));
            if (view != null)
                return view.CurrentContentReplaceableView; //成功
            else {
                IContentReplaceableView view2 = (IContentReplaceableView)target.GetAdapter(typeof(IContentReplaceableView));
                if (view2 != null)
                    return view2;
                else {
                    IPoderosaMainWindow window = AsWindow(target);
                    if (window != null)
                        return window.LastActivatedView;
                    else
                        return null;
                }
            }
        }

        //ViewまたはLastActivatedViewからcopyコマンドがあれば取得
        /// <summary>
        /// <ja>
        /// 標準的なビューのコマンドをもつ<seealso cref="IGeneralViewCommands">IGeneralViewCommands</seealso>へと変換します。
        /// </ja>
        /// <en>
        /// It converts it into <seealso cref="IGeneralViewCommands">IGeneralViewCommands</seealso> with the command of a standard view. 
        /// </en>
        /// </summary>
        /// <param name="target">
        /// <ja>対象となるターゲット。</ja>
        /// <en>It is a target that becomes an object. </en>
        /// </param>
        /// <returns>
        /// <ja>変換したインターフェイスです。変換できないときにはnullが戻ります。</ja>
        /// <en>It is a converted interface. Null returns when it is not possible to convert it. </en>
        /// </returns>
        /// <remarks>
        /// <ja>このメソッドは、<see cref="AsViewOrLastActivatedView">AsViewOrLastActivatedViewメソッド</see>を呼び出して得たインターフェイスをIGeneralViewCommandsへと変換します。</ja>
        /// <en>This method converts the interface that calls the <see cref="AsViewOrLastActivatedView">AsViewOrLastActivatedView method</see> and obtains it into IGeneralViewCommands. </en>
        /// </remarks>
        public static IGeneralViewCommands AsGeneralViewCommands(ICommandTarget target) {
            IPoderosaView view = AsViewOrLastActivatedView(target);
            if (view == null)
                return null;

            return (IGeneralViewCommands)view.GetAdapter(typeof(IGeneralViewCommands));
        }
    }

    //CharacterDocumentViewerに対して起動するテキストコピーコマンド
    internal class SelectedTextCopyCommand : IPoderosaCommand {
        public SelectedTextCopyCommand() {
        }

        public CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
            CharacterDocumentViewer control = (CharacterDocumentViewer)target.GetAdapter(typeof(CharacterDocumentViewer));
            ITextSelection s = control.ITextSelection;
            if (s.IsEmpty || !control.EnabledEx)
                return CommandResult.Ignored;

            string t = s.GetSelectedText(TextFormatOption.Default);
            if (t.Length > 0)
                CopyToClipboard(t);
            return CommandResult.Succeeded;
        }


        public bool CanExecute(ICommandTarget target) {
            CharacterDocumentViewer control = (CharacterDocumentViewer)target.GetAdapter(typeof(CharacterDocumentViewer));
            return control.EnabledEx && !control.ITextSelection.IsEmpty;
        }

        public IAdaptable GetAdapter(Type adapter) {
            return CommandManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        private void CopyToClipboard(string data) {
            try {
                Clipboard.SetDataObject(data, false);
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }
    }

}
