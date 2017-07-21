/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: WindowManagerEx.cs,v 1.4 2012/03/18 11:02:29 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using Poderosa.Document;
using Poderosa.Sessions;
using Poderosa.Commands;

using Poderosa.View;
using Poderosa.Util;

namespace Poderosa.Forms {

    //System.Windows.Forms.Controlと同等だが、必要なもののみを抽出したやつ
    /// <summary>
    /// <ja>
    /// ウィンドウを.NET Frameworkのコントロールとして扱うためのインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to treat the window as a control of .NET Framework. 
    /// </en>
    /// </summary>
    public interface IPoderosaControl : IAdaptable {
        /// <summary>
        /// <ja>.NET FrameworkのControlオブジェクトに変換します。</ja>
        /// <en>Convert to the Control object of .NET Framework</en>
        /// </summary>
        /// <returns><ja>変換されたControlオブジェクト</ja><en>Converted Control object.</en></returns>
        Control AsControl();
    }

    /// <summary>
    /// <ja>
    /// ウィンドウを.NET Frameworkのフォームとして扱うためのインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to treat the window as a form of .NET Framework. 
    /// </en>
    /// </summary>
    public interface IPoderosaForm : IPoderosaControl, ICommandTarget {
        /// <summary>
        /// <ja>
        /// .NET FrameworkのFormオブジェクトに変換します。
        /// </ja>
        /// <en>Convert to the Form object of .NET Framework</en>
        /// </summary>
        /// <returns><ja>変換されたFormオブジェクト</ja><en>Converted Form object.</en></returns>
        Form AsForm();
        /// <summary>
        /// <ja>
        /// ウィンドウを閉じます。
        /// </ja>
        /// <en>
        /// Close the window.
        /// </en>
        /// </summary>
        /// <returns><ja>閉じられたかどうかを示します。正常に閉じられた場合、CommandResult.Succeededが返されます。</ja><en>Whether it was closed is shown. CommandResult.Succeeded is returned when close normally. </en></returns>
        /// <remarks>
        /// <ja>
        /// セッションの処理によっては、ユーザーに閉じてもよいかどうかを問い合わせることができるため、閉じる動作がキャンセルされることもあります。
        /// キャンセルされたかどうかは、戻り値で判断してください。
        /// </ja>
        /// <en>
        /// Because it can be inquired whether I may close to the user according to the processing of the session, the closing operation might be canceled. 
        /// Please judge whether to have been canceled from the return value. 
        /// </en>
        /// </remarks>
        CommandResult CancellableClose();

        //ポップアップメニュー
        /// <summary>
        /// <ja>
        /// ポップアップメニュー（コンテキストメニュー）を表示します。
        /// </ja>
        /// <en>
        /// Show the popup menu (context menu).
        /// </en>
        /// </summary>
        /// <param name="menus"><ja>表示するメニューを示すメニューグループです。</ja><en>It is a menu group that shows the displayed menu. </en></param>
        /// <param name="target"><ja>メニューのターゲットです。</ja><en>It is a target of the menu. </en></param>
        /// <param name="point_screen"><ja>表示する位置です。</ja><en>It is a displayed position. </en></param>
        /// <param name="flags"><ja>メニューを表示するときに先頭の項目を選択状態にするかどうかのフラグです</ja><en>Flag whether to put the first item into state of selection when menu is displayed</en></param>
        void ShowContextMenu(IPoderosaMenuGroup[] menus, ICommandTarget target, Point point_screen, ContextMenuFlags flags);

        //ユーザに対する警告系。このFormを所有していないスレッドからも呼ばれることを考慮すること Note: 別インタフェースに分離するか？
        /// <summary>
        /// <ja>
        /// 警告メッセージボックスを表示します。
        /// </ja>
        /// <en>
        /// Show the message box of warning.
        /// </en>
        /// </summary>
        /// <param name="msg"><ja>表示するメッセージです。</ja><en>message to display</en></param>
        /// <remarks>
        /// <ja>
        /// <para>
        /// このメソッドは、フォームを所有するスレッド以外から呼び出してもかまいません。
        /// </para>
        /// <para>
        /// ただしオブジェクトへのロックをもったまま呼び出すと、そのロックはメッセージボックスを閉じるまで解放されません。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// You may call this method excluding the thread to own the form. 
        /// </para>
        /// <para>
        /// However, when the lock to the object is called while had, the lock is not released until the message box is closed. 
        /// </para>
        /// </en>
        /// </remarks>
        void Warning(string msg);
        /// <summary>
        /// <ja>
        /// 情報メッセージボックスを表示します。
        /// </ja>
        /// <en>
        /// Show the message box of information.
        /// </en>
        /// </summary>
        /// <param name="msg"><ja>表示するメッセージです。</ja><en>message to display</en></param>
        /// <remarks>
        /// <ja>
        /// <para>
        /// このメソッドは、フォームを所有するスレッド以外から呼び出してもかまいません。
        /// </para>
        /// <para>
        /// ただしオブジェクトへのロックをもったまま呼び出すと、そのロックはメッセージボックスを閉じるまで解放されません。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// You may call this method excluding the thread to own the form. 
        /// </para>
        /// <para>
        /// However, when the lock to the object is called while had, the lock is not released until the message box is closed. 
        /// </para>
        /// </en>
        /// </remarks>
        void Information(string msg);
        /// <summary>
        /// <ja>
        /// ［はい］か［いいえ］かを尋ねるメッセージボックスを表示します。
        /// </ja>
        /// <en>
        /// Show the messaage box that asks "Yes" or "No".
        /// </en>
        /// </summary>
        /// <param name="msg"><ja>表示するメッセージです。</ja><en>message to display</en></param>
        /// <returns><ja>どのボタンが押されたのかを示す値です。［はい］のときにはDialogResult.Yes、［いいえ］のときにはDialogResult.Noとなります。</ja><en>It is a value in which which button was pushed is shown.When DialogResult.Yes getting it at "Yes", at the time of good it becomes DialogResult.No. </en></returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// このメソッドは、フォームを所有するスレッド以外から呼び出してもかまいません。
        /// </para>
        /// <para>
        /// ただしオブジェクトへのロックをもったまま呼び出すと、そのロックはメッセージボックスを閉じるまで解放されません。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// You may call this method excluding the thread to own the form. 
        /// </para>
        /// <para>
        /// However, when the lock to the object is called while had, the lock is not released until the message box is closed. 
        /// </para>
        /// </en>
        /// </remarks>
        DialogResult AskUserYesNo(string msg);
    }

    /// <summary>
    /// <ja>
    /// コンテキストメニューを表示するときのフラグを示します。
    /// </ja>
    /// <en>
    /// The flag when the context menu is displayed is shown. 
    /// </en>
    /// </summary>
    [Flags]
    public enum ContextMenuFlags {
        /// <summary>
        /// <ja>
        /// 表示時に何もしません。
        /// </ja>
        /// <en>
        /// Do nothing when displayed.
        /// </en>
        /// </summary>
        None = 0,
        /// <summary>
        /// <ja>
        /// 表示時に先頭の項目を選択された状態にします。
        /// </ja>
        /// <en>
        /// It puts it into the state that the first item was selected when displaying it. 
        /// </en>
        /// </summary>
        SelectFirstItem = 1
    }

    /// <summary>
    /// <ja>
    /// ウィンドウマネージャのIDです。
    /// </ja>
    /// <en>
    /// ID of Window manager.
    /// </en>
    /// </summary>
    /// <exclude/>
    public class WindowManagerConstants {
        public const string MAINWINDOWCONTENT_ID = "org.poderosa.core.window.mainWindowContent";
        public const string VIEW_FACTORY_ID = "org.poderosa.core.window.viewFactory";
        public const string VIEWFORMATEVENTHANDLER_ID = "org.poderosa.core.window.viewFormatEventHandler";
        public const string TOOLBARCOMPONENT_ID = "org.poderosa.core.window.toolbar";
        public const string MAINWINDOWEVENTHANDLER_ID = "org.poderosa.core.window.mainWindowEventHandler";
        public const string FILEDROPHANDLER_ID = "org.poderosa.core.window.fileDropHandler";
    }

    //WindowManager全体
    /// <summary>
    /// <ja>
    /// ウィンドウマネージャを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that show the window manager.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// ウィンドウマネージャは、プラグインID「<c>org.poderosa.core.window</c>」をもつプラグインで提供されています。
    /// </para>
    /// <para>
    /// 次のように<seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>を経由して取得できます。
    /// </para>
    /// <code>
    /// // ICoreServicesを取得
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // IWindowManagerを取得
    /// IWindowManager wm = cs.WindowManager;
    /// </code>
    /// </ja>
    /// <en>
    /// <para>
    /// Window manager is offered as the plug-in with plug-in ID [<c>org.poderosa.core.window</c>].
    /// </para>
    /// <para>
    /// <ja>次のように<seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>を経由して取得できます。</ja><en>It is possible to acquire it via <seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso> as follows. </en>
    /// </para>
    /// <code>
    /// // Get ICoreServices
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // Get IWindowManager
    /// IWindowManager wm = cs.WindowManager;
    /// </code>
    /// </en>
    /// </remarks>
    public interface IWindowManager : IAdaptable {
        /// <summary>
        /// <ja>
        /// すべてのメインウィンドウを示す配列です。
        /// </ja>
        /// <en>
        /// Array that shows all the main windows.
        /// </en>
        /// </summary>
        IPoderosaMainWindow[] MainWindows {
            get;
        }
        /// <summary>
        /// <ja>
        /// アクティブなウィンドウを示します。
        /// </ja>
        /// <en>
        /// Show the active window.
        /// </en>
        /// </summary>
        IPoderosaMainWindow ActiveWindow {
            get;
        }

        /// <summary>
        /// <ja>
        /// オブジェクトの選択とコピーに関するアクセスを提供するISelectionServiceを返します。
        /// </ja>
        /// <en>
        /// ISelectionService that offers the selection of the object and the access concerning the copy is returned. 
        /// </en>
        /// </summary>
        ISelectionService SelectionService {
            get;
        }

        //PopupView作成 : コレで作成したビューは、セッションマネージャのAttachDocAndView->Activateをすることで初めて見えるようになる。CreatePopupViewだけでは見えるようにはならないことに注意
        /// <summary>
        /// <ja>
        /// ポップアップビューを作成します。
        /// </ja>
        /// <en>
        /// Create the popup view.
        /// </en>
        /// </summary>
        /// <param name="viewcreation"><ja>ポップアップビューを作成する際のパラメータです</ja><en>It is a parameter when the pop up view is made. </en></param>
        /// <returns><ja>作成されたポップアップウィンドウが返されます。</ja><en>Return the made pop up window.</en></returns>
        /// <remarks>
        /// <ja>
        /// 作成されたビューは、セッションマネージャ（<seealso cref="ISessionManager">ISessionManager</seealso>）の
        /// <see cref="ISessionManager.AttachDocumentAndView">AttachDocumentAndViewメソッド</see>を呼び出してドキュメントとビューをアタッチしてから、
        /// アクティベートすることで、初めて見えるようになります。このメソッドで作成しただけでは見えるようにはなりません。
        /// </ja>
        /// <en>
        /// It comes to see the made view for the first time by the activate after session manager(<seealso cref="ISessionManager">ISessionManager</seealso>)'s <see cref="ISessionManager.AttachDocumentAndView">AttachDocumentAndView method</see> is called and the document and the view are activated. It doesn't come to see it only by making it by this method. 
        /// </en>
        /// </remarks>
        /// <exclude/>
        IPoderosaPopupWindow CreatePopupView(PopupViewCreationParam viewcreation);

        //Reload
        /// <summary>
        /// <ja>
        /// メニューをリロードします。
        /// </ja>
        /// <en>
        /// Reload the menu.
        /// </en>
        /// </summary>
        void ReloadMenu();

        //Preference系のリロード
        /// <summary>
        /// <ja>指定したアセンブリに関するユーザー設定値（Preference）を再読込します。</ja>
        /// <en>User setting value (Preference) concerning the specified assembly is read again. </en>
        /// </summary>
        /// <param name="preference"><ja>再読込したいICoreServicePreference</ja><en>ICoreServicePreference to read again.</en></param>
        /// <overloads>
        /// <summary>
        /// <ja>
        /// ユーザー設定値（Preference）を再読込します。
        /// </ja>
        /// <en>
        /// User setting value (Preference) is read again. 
        /// </en>
        /// </summary>
        /// </overloads>
        void ReloadPreference(ICoreServicePreference preference);

        /// <summary>
        /// <ja>
        /// ユーザー設定値（Preference）を再読込します。
        /// </ja>
        /// <en>
        /// User setting value (Preference) is read again. 
        /// </en>
        /// </summary>
        void ReloadPreference();
    }

    //アプリ全体に関係し、かつsystem.Windows.Formsがらみ
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IWinFormsService : IAdaptable {
        //Timer Support
        ITimerSite CreateTimer(int interval, TimerDelegate callback);

        //Drag & Drop
        object GetDraggingObject(IDataObject data, Type required_type);
        void BypassDragEnter(Control target, DragEventArgs args);
        void BypassDragDrop(Control target, DragEventArgs args);
    }

    //event handler
    public interface IMainWindowEventHandler : IAdaptable {
        void OnFirstMainWindowLoaded(IPoderosaMainWindow window);
        void OnMainWindowLoaded(IPoderosaMainWindow window);
        void OnMainWindowUnloaded(IPoderosaMainWindow window);
        void OnLastMainWindowUnloaded(IPoderosaMainWindow window);
    }

    //File Drop : ファイル以外を扱うことはまずないだろうから考えない。そのときはまた別のインタフェースで。
    public interface IFileDropHandler : IAdaptable {
        bool CanAccept(ICommandTarget target, string[] filenames);
        void DoDropAction(ICommandTarget target, string[] filenames);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IViewManagerFactory : IAdaptable {
        IViewManager Create(IPoderosaMainWindow parent);
        IViewFactory DefaultViewFactory {
            get;
            set;
        }
    }

    /// <summary>
    /// <ja>
    /// Poderosaのメインウィンドウを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that show the main window of Poderosa.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// メニューやツールバーから呼び出されるコマンドでは、ターゲットはメインウィンドウです。そのためGetAdapterメソッドを呼び出すことで、IPoderosaMainWindowへと変換できます。
    /// <code>
    /// // <var>target</var>はコマンドに引き渡されたターゲットであると想定します。
    /// IPoderosaMainWindow window = 
    ///     (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
    /// </code>
    /// もしくはウィンドウマネージャ（<seealso cref="IWindowManager">IWindowManager</seealso>）の<see cref="IWindowManager.ActiveWindow">ActiveWindowプロパティ</see>を
    /// 使って、アクティブなウィンドウを取得することもできます。
    /// <code>
    /// // csは<seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>を示していると想定します。
    /// IPoderosaMainWindow mainwin = cs.WindowManager.ActiveWindow;
    /// </code>
    /// </ja>
    /// <en>
    /// In the command called from the menu and the toolbar, the target is the main window. Therefore, it is possible to convert it into IPoderosaMainWindow by calling the GetAdapter method. 
    /// <code>
    /// // It is assumed that <var>target</var> is a target handed over to the command. 
    /// IPoderosaMainWindow window = 
    ///     (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
    /// </code>
    /// Or, an active window can be acquired by using window manager(<seealso cref="IWindowManager">IWindowManager</seealso>)'s <see cref="IWindowManager.ActiveWindow">ActiveWindow property</see>. 
    /// <code>
    /// // cs is assumed that <seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso> is shown. 
    /// IPoderosaMainWindow mainwin = cs.WindowManager.ActiveWindow;
    /// </code>
    /// </en>
    /// </remarks>
    public interface IPoderosaMainWindow : IPoderosaForm {
        /// <summary>
        /// <ja>
        /// ビューマネージャを返します。
        /// </ja>
        /// <en>
        /// Return the view manager.
        /// </en>
        /// </summary>
        IViewManager ViewManager {
            get;
        }

        /// <summary>
        /// <ja>
        /// ドキュメントタブを示すオブジェクトを返します。
        /// </ja>
        /// <en>
        /// Return the object which show the document tab.
        /// </en>
        /// </summary>
        IDocumentTabFeature DocumentTabFeature {
            get;
        }
        /// <summary>
        /// <ja>
        /// ツールバーを示すオブジェクトを返します。
        /// </ja>
        /// <en>
        /// Return the object which show the toolbar.
        /// </en>
        /// </summary>
        IToolBar ToolBar {
            get;
        }

        /// <summary>
        /// <ja>
        /// ステータスバーを示すオブジェクトを返します。
        /// </ja>
        /// <en>
        /// Return the object which show the status bar.
        /// </en>
        /// </summary>
        IPoderosaStatusBar StatusBar {
            get;
        }

        /// <summary>
        /// <ja>
        /// 最後にアクティブになったビューを返します。
        /// </ja>
        /// <en>
        /// Return the view that last active.
        /// </en>
        /// </summary>
        IContentReplaceableView LastActivatedView {
            get;
        }
    }

    /// <summary>
    /// <ja>
    /// ポップアップウィンドウを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows pop up window.
    /// </en>
    /// </summary>
    public interface IPoderosaPopupWindow : IPoderosaForm {
        /// <summary>
        /// <ja>
        /// ポップアップウィンドウ内のビューを示します。
        /// </ja>
        /// <en>
        /// The view in the pop up window is shown. 
        /// </en>
        /// </summary>
        IPoderosaView InternalView {
            get;
        }
        /// <summary>
        /// <ja>
        /// ステータスを更新します。
        /// </ja>
        /// <en>
        /// Update the status.
        /// </en>
        /// </summary>
        void UpdateStatus();
    }


    //TabBar相当
    /// <summary>
    /// <ja>
    /// メインウィンドウのドキュメントタブを示します。
    /// </ja>
    /// <en>
    /// The document tab in the main window is shown. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインターフェイスは、<seealso cref="IPoderosaMainWindow">IPoderosaMainWindow</seealso>の
    /// <see cref="IPoderosaMainWindow.DocumentTabFeature">DocumentTabFeatureプロパティ</see>から
    /// 取得できます。
    /// </ja>
    /// <en>
    /// This interface can be got from the <see cref="IPoderosaMainWindow.DocumentTabFeature">
    /// DocumentTabFeature property</see> of <seealso cref="IPoderosaMainWindow">IPoderosaMainWindow</seealso>. 
    /// </en>
    /// </remarks>
    public interface IDocumentTabFeature : IAdaptable {
        /// <summary>
        /// <ja>
        /// ドキュメントを追加します。
        /// </ja>
        /// <en>
        /// Add the document
        /// </en>
        /// </summary>
        /// <param name="document"><ja>追加するドキュメントです。</ja><en>Document to add.</en></param>
        void Add(IPoderosaDocument document);
        /// <summary>
        /// <ja>
        /// ドキュメントを削除します。
        /// </ja>
        /// <en>
        /// Remove the document
        /// </en>
        /// </summary>
        /// <param name="document"><ja>削除するドキュメントです。</ja><en>Document to remove.</en></param>
        void Remove(IPoderosaDocument document);
        /// <summary>
        /// <ja>
        /// ドキュメントを更新します。
        /// </ja>
        /// <en>
        /// Update the document
        /// </en>
        /// </summary>
        /// <param name="document"><ja>更新したいドキュメントです。</ja><en>Document to be update.</en></param>
        void Update(IPoderosaDocument document);
        /// <summary>
        /// <ja>
        /// ドキュメントをアクティブにします。
        /// </ja>
        /// <en>
        /// Activate the document
        /// </en>
        /// </summary>
        /// <param name="document"><ja>アクティブにしたいドキュメントです。</ja><en>Document to active.</en></param>
        void Activate(IPoderosaDocument document);
        /// <summary>
        /// <ja>
        /// アクティブなドキュメントを返します。
        /// </ja>
        /// <en>
        /// Return the active document.
        /// </en>
        /// </summary>
        IPoderosaDocument ActiveDocument {
            get;
        }

        /// <summary>
        /// <ja>
        /// ドキュメントの数を返します。
        /// </ja>
        /// <en>
        /// Return the conut of the document.
        /// </en>
        /// </summary>
        int DocumentCount {
            get;
        }
        /// <summary>
        /// <ja>
        /// 指定位置のドキュメントを返します。
        /// </ja>
        /// <en>
        /// Return the document at a specified position.
        /// </en>
        /// </summary>
        /// <param name="index"><ja>取得したいドキュメントのインデックス位置です。</ja><en>It is an index position of the document that wants to be got. </en></param>
        /// <returns><ja>ドキュメントがあればそのドキュメントを、ドキュメントがない場合にはnullが戻ります。</ja><en>Null returns in the document when there is no document if there is a document. </en></returns>
        IPoderosaDocument GetAtOrNull(int index);
        /// <summary>
        /// <ja>
        /// 指定したドキュメントのインデックス位置を返します。
        /// </ja>
        /// <en>
        /// Return the index position of the specified document.
        /// </en>
        /// </summary>
        /// <param name="document"><ja>インデックス位置を知りたいドキュメント</ja><en>Document that wants to know index position</en></param>
        /// <returns><ja>ドキュメントがあればそのドキュメント位置。ドキュメントが見つからない場合には-1が戻ります。</ja><en>It is the document position if there is a document. -1 returns when the document is not found. </en></returns>
        int IndexOf(IPoderosaDocument document);

        //タブ関係
        int TabRowCount {
            get;
        }
        void SetTabRowCount(int count);

        /// <summary>
        /// Activate next view on the tab bar.
        /// </summary>
        void ActivateNextTab();

        /// <summary>
        /// Activate previous view on the tab bar.
        /// </summary>
        void ActivatePrevTab();
    }

    //StatusBar
    /// <summary>
    /// <ja>
    /// ステータスバーを示します。
    /// </ja>
    /// <en>
    /// The status bar is shown. 
    /// </en>
    /// </summary>
    public interface IPoderosaStatusBar {
        /// <summary>
        /// <ja>
        /// ステータスバーのテキストを設定します。
        /// </ja>
        /// <en>
        /// The text of the status bar is set. 
        /// </en>
        /// </summary>
        /// <param name="msg"><ja>設定するテキスト</ja><en>Text to set.</en></param>
        void SetMainText(string msg);
        /// <summary>
        /// <ja>ステータスバーのアイコンを設定します。</ja><en>Set the icon of the status bar.</en>
        /// </summary>
        /// <param name="icon"><ja>設定するアイコン</ja><en>Icon to set.</en></param>
        void SetStatusIcon(Image icon);
    }


    //Timer Suppoer
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public delegate void TimerDelegate();

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITimerSite {
        void Close();
    }

    /// <summary>
    /// <ja>
    /// 言語を示します。
    /// </ja>
    /// <en>
    /// The language is shown. 
    /// </en>
    /// </summary>
    public enum Language {
        /// <summary>
        /// <ja>
        /// 英語</ja>
        /// <en>
        /// English</en>
        /// </summary>
        [EnumValue(Description = "Enum.Language.English")]
        English,
        /// <summary>
        /// <ja>日本語</ja>
        /// <en>Japanese</en>
        /// </summary>
        [EnumValue(Description = "Enum.Language.Japanese")]
        Japanese
    }


    /// <exclude/>
    public interface IWindowPreference {
        int WindowCount {
            get;
        }
        //Window個別
        string WindowPositionAt(int index);
        string WindowSplitFormatAt(int index);
        string ToolBarFormatAt(int index);
        int TabRowCountAt(int index);
    }

    //このアセンブリ内のPreference
    /// <summary>
    /// <ja>
    /// このアセンブリ内のPreferenceを示します。
    /// </ja>
    /// <en>
    /// The Preference in this assembly is shown.
    /// </en>
    /// </summary>
    /// <exclude/>
    public interface ICoreServicePreference {
        //全体共通
        bool ShowsToolBar {
            get;
            set;
        }
        Keys ViewSplitModifier {
            get;
            set;
        }
        int CaretInterval {
            get;
            set;
        }
        bool AutoCopyByLeftButton {
            get;
            set;
        }

        //非GUI
        int SplitLimitCount {
            get;
        }

        //動的変更可能言語
        Language Language {
            get;
            set;
        }
    }

    //PopupView作成パラメータ
    /// <summary>
    /// <ja>
    /// PopupViewを作成する際のパラメータとなるオブジェクトです。
    /// </ja>
    /// <en>
    /// Object that becomes parameter when PopupView is made.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このパラメータは、<seealso cref="IWindowManager">IWindowManager</seealso>の
    /// <see cref="IWindowManager.CreatePopupView">CreatePopupViewメソッド</see>の引数として使われます。
    /// </ja>
    /// <en>
    /// This parameter is used as an argument of the <see cref="IWindowManager.CreatePopupView">CreatePopupView method</see> of <seealso cref="IWindowManager">IWindowManager</seealso>. 
    /// </en>
    /// </remarks>
    /// <exclude/>
    public class PopupViewCreationParam {
        private IViewFactory _viewFactory;
        private Size _initialSize;
        private bool _ownedByCommandTargetWindow;
        private bool _showInTaskBar;

        public PopupViewCreationParam(IViewFactory factory) {
            _viewFactory = factory;
            _initialSize = new Size(300, 300);
        }

        public IViewFactory ViewFactory {
            get {
                return _viewFactory;
            }
            set {
                _viewFactory = value;
            }
        }
        public Size InitialSize {
            get {
                return _initialSize;
            }
            set {
                _initialSize = value;
            }
        }
        public bool OwnedByCommandTargetWindow {
            get {
                return _ownedByCommandTargetWindow;
            }
            set {
                _ownedByCommandTargetWindow = value;
            }
        }
        public bool ShowInTaskBar {
            get {
                return _showInTaskBar;
            }
            set {
                _showInTaskBar = value;
            }
        }
    }


}
