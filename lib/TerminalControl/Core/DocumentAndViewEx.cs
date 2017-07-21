/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: DocumentAndViewEx.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using Poderosa.Forms;
using Poderosa.Commands;
using Poderosa.UI;
using Poderosa.View;

namespace Poderosa.Sessions {
    /// <summary>
    /// <ja>
    /// ドキュメントを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows document
    /// </en>
    /// </summary>
    public interface IPoderosaDocument : ICommandTarget {
        /// <summary>
        /// <ja>
        /// ドキュメントのアイコンです。
        /// </ja>
        /// <en>
        /// Icon of the document.
        /// </en>
        /// </summary>
        Image Icon {
            get;
        }
        /// <summary>
        /// <ja>
        /// ドキュメントのキャプションです。
        /// </ja>
        /// <en>
        /// Caption of the document.
        /// </en>
        /// </summary>
        string Caption {
            get;
        }
        /// <summary>
        /// <ja>
        /// ドキュメントを構成するセッションです。
        /// </ja>
        /// <en>
        /// Session that composes the document.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// 標準のターミナルエミュレータとして用いる場合、このインターフェイスは、
        /// <seealso cref="Poderosa.Sessions.ITerminalSession">ITerminalSession</seealso>へと変換できます。
        /// </ja>
        /// <en>
        /// This interface can be converted into <seealso cref="Poderosa.Sessions.ITerminalSession">ITerminalSession</seealso> when using it as a standard terminal emulator. 
        /// </en>
        /// </remarks>
        ISession OwnerSession {
            get;
        }
    }

    /// <summary>
    /// <ja>
    /// ビューを表現するインターフェイスです。
    /// </ja>
    /// <en>
    /// The interface that show the view.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// 最後にアクティブになったビューは、<seealso cref="Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</seealso>
    /// の<see cref="IPoderosaMainWindow.LastActivatedView">LastActivatedViewプロパティ</see>で取得できます。
    /// </para>
    /// <para>
    /// また<seealso cref="CommandTargetUtil">CommandTargetUtil</seealso>の
    /// <see cref="CommandTargetUtil.AsViewOrLastActivatedView">AsViewOrLastActivatedViewメソッド</see>
    /// を呼び出すと、コマンド実行時の引数として渡されるターゲットをビューへと変換できます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// The view that became active at the end can be got in the 
    /// <see cref="IPoderosaMainWindow.LastActivatedView">LastActivatedView property</see> of 
    /// <seealso cref="Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</seealso>. 
    /// </para>
    /// <para>
    /// Moreover, the target passed as an argument when the command is executed can be converted into the 
    /// view by calling the <see cref="CommandTargetUtil.AsViewOrLastActivatedView">AsViewOrLastActivatedView method</see> 
    /// of <seealso cref="CommandTargetUtil">CommandTargetUtil</seealso>. 
    /// </para>
    /// </en>
    /// </remarks>
    public interface IPoderosaView : IPoderosaControl, ICommandTarget {
        /// <summary>
        /// <ja>
        /// ビューに結びつけられているドキュメントを示します。
        /// </ja>
        /// <en>
        /// Document tie to view
        /// </en>
        /// </summary>
        IPoderosaDocument Document {
            get;
        }
        /// <summary>
        /// <ja>
        /// 現在選択されている部分を示すISelectionです。
        /// </ja>
        /// <en>
        /// ISelection that shows part that has been selected now.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// ISelectionを<seealso cref="ITextSelection">ITextSelection</seealso>へと変換し、その<see cref="ITextSelection.GetSelectedText">GetSelectedTextメソッド</see>
        /// を呼び出すと、現在選択されている文字列を取得できます。
        /// 
        /// <code>
        /// ITextSelection select = (ITextSelection)<var>view</var>.CurrentSelection.GetAdapter(
        ///     typeof(ITextSelection));
        /// if ((select != null) &amp;&amp; (!select.IsEmpty))
        /// {
        ///     MessageBox.Show(select.GetSelectedText(TextFormatOption.Default));
        /// }
        /// </code>
        /// </ja>
        /// <en>
        /// The character string that has been selected now can be acquired by converting ISelection into 
        /// <seealso cref="ITextSelection">ITextSelection</seealso>, and calling the 
        /// <see cref="ITextSelection.GetSelectedText">GetSelectedText method</see>. 
        /// 
        /// <code>
        /// ITextSelection select = (ITextSelection)<var>view</var>.CurrentSelection.GetAdapter(
        ///     typeof(ITextSelection));
        /// if ((select != null) &amp;&amp; (!select.IsEmpty))
        /// {
        ///     MessageBox.Show(select.GetSelectedText(TextFormatOption.Default));
        /// }
        /// </code>
        /// </en>
        /// </remarks>
        ISelection CurrentSelection {
            get;
        }

        /// <summary>
        /// <ja>
        /// ビューの親となるフォームを示します。
        /// </ja>
        /// <en>
        /// Form that becomes parents of view.
        /// </en>
        /// </summary>
        IPoderosaForm ParentForm {
            get;
        }
    }

    //ビュークラス
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IViewFactory : IAdaptable {
        IPoderosaView CreateNew(IPoderosaForm parent);
        Type GetViewType();
        Type GetDocumentType();
    }


    //ビューのWindows.Forms上の型を動的に変更できるタイプのビュー
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IContentReplaceableView : IPoderosaView {
        IViewManager ViewManager {
            get;
        }
        IPoderosaView GetCurrentContent();
        IPoderosaView AssureViewClass(Type viewclass);
        void AssureEmptyViewClass();
    }
    //中身側が実装するインタフェース。ReplaceContentが呼ばれるたびに通知を受けられるようにする
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IContentReplaceableViewSite : IAdaptable {
        IContentReplaceableView CurrentContentReplaceableView {
            get;
            set;
        }
    }

    //ビュー用の標準コマンド。IPoderosaViewがオプショナルで提供する。

    /// <summary>
    /// <ja>
    /// ビューの標準コマンドを提供します。
    /// </ja>
    /// <en>
    /// Offered a standard command of the view.
    /// </en>
    /// </summary>
    public interface IGeneralViewCommands : IAdaptable {
        /// <summary>
        /// <ja>
        /// クリップボードへコピーします。
        /// </ja>
        /// <en>
        /// Copy to the clipboard. 
        /// </en>
        /// </summary>
        IPoderosaCommand Copy {
            get;
        }
        /// <summary>
        /// <ja>
        /// クリップボードから貼り付けます。
        /// </ja>
        /// <en>
        /// Paste from the clipboard. 
        /// </en>
        /// </summary>
        IPoderosaCommand Paste {
            get;
        }
        //IPoderosaCommand Cut { get; } モトがターミナルエミュレータということで、カットは標準に含めず
    }


    /// <summary>
    /// <ja>
    /// ビューマネージャを示します。
    /// </ja>
    /// <en>
    /// It shows the view manager.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ビューマネージャは、<seealso cref="IPoderosaMainWindow">IPoderosaMainWindow</seealso>の
    /// <see cref="IPoderosaMainWindow.ViewManager">ViewManagerプロパティ</see>から取得できます。
    /// </ja>
    /// <en>
    /// The view manager can acquire it from the <see cref="IPoderosaMainWindow.ViewManager">ViewManager property</see> of <seealso cref="IPoderosaMainWindow">IPoderosaMainWindow</seealso>. 
    /// </en>
    /// </remarks>
    public interface IViewManager : IAdaptable {
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        Control RootControl {
            get;
        }
        /// <summary>
        /// <ja>
        /// 新しいドキュメントを作成するためのビューを作ります。
        /// </ja>
        /// <en>
        /// Create the view to make a new document.
        /// </en>
        /// </summary>
        /// <returns><ja>作られたビューが返されます。</ja><en>return thr created view</en></returns>
        IPoderosaView GetCandidateViewForNewDocument();
        /// <summary>
        /// <ja>
        /// このビューが属するウィンドウです。
        /// </ja>
        /// <en>
        /// Window to which this view belongs
        /// </en>
        /// </summary>
        IPoderosaMainWindow ParentWindow {
            get;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ISplittableViewManager : IViewManager {
        //factoryがnullのときは、viewのクラスを提供するfactoryが使われる
        CommandResult SplitHorizontal(IContentReplaceableView view, IViewFactory factory);
        CommandResult SplitVertical(IContentReplaceableView view, IViewFactory factory);
        CommandResult Unify(IContentReplaceableView view, out IContentReplaceableView next_focus);
        CommandResult UnifyAll(out IContentReplaceableView next_focus);
        bool CanSplit(IContentReplaceableView view);
        bool CanUnify(IContentReplaceableView view);
        bool IsSplitted();

        IPoderosaView[] GetAllViews();

        string FormatSplitInfo();
        void ApplySplitInfo(string value);
    }

    //分割関係の変更のイベントハンドラ
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IViewFormatEventHandler {
        void OnSplit(ISplittableViewManager viewmanager);
        void OnUnify(ISplittableViewManager viewmanager);
    }
}
