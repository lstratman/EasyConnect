/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SelectionEx.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

using Poderosa.Sessions;
using Poderosa.Commands;

namespace Poderosa.View {

    //選択サービス
    // 同時に複数のSelectionを持つことができるが（FireFoxなどもそうなっている）、アクティブなのは同時には一つだけ。

    /// <summary>
    /// <ja>
    /// オブジェクトの選択に関する機能を提供するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that offers function concerning selection of object
    /// </en>
    /// </summary>
    public interface ISelectionService {
        /// <summary>
        /// <ja>
        /// 現在の選択状況を含むISelectionです。
        /// </ja>
        /// <en>
        /// ISelection including present selection situation
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// 現在アクティブなビューの<see cref="IPoderosaView.CurrentSelection">CurrentSelectionプロパティ</see>を参照するのと同じです。
        /// </ja>
        /// <en>
        /// It is the same as the reference to the <see cref="IPoderosaView.CurrentSelection">CurrentSelection property</see> of an active view at present. 
        /// </en>
        /// </remarks>
        ISelection ActiveSelection {
            get;
        } //ActiveViewのSelectionと同義
        /// <summary>
        /// <ja>
        /// デフォルトのコピーや貼り付けに関するコマンドへのインターフェイスです。
        /// </ja>
        /// <en>
        /// Interface to command concerning copy and putting default.
        /// </en>
        /// </summary>
        IPoderosaCommand DefaultCopyCommand {
            get;
        }
    }

    /// <summary>
    /// <ja>
    /// 選択状態が変化したときの通知を受け取るリスナです。
    /// </ja>
    /// <en>
    /// Listener that receives notification when selection changes.
    /// </en>
    /// </summary>
    public interface ISelectionListener {
        /// <summary>
        /// <ja>
        /// 選択が開始されたときに呼び出されます。
        /// </ja>
        /// <en>
        /// When the selection is begun, it is called. 
        /// </en>
        /// </summary>
        void OnSelectionStarted();
        /// <summary>
        /// <ja>
        /// 選択が確定したときに呼び出されます。
        /// </ja>
        /// <en>
        /// When the selection is fixed, it is called. 
        /// </en>
        /// </summary>
        void OnSelectionFixed();
    }

    //選択しているオブジェクト 多くは各IPoderosaViewが管理するだろう
    /// <summary>
    /// <ja>
    /// 選択しているオブジェクトを操作するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that operates object that has been selected
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ターミナルエミュレータであるビューの場合、GetAdapterメソッドを使うことで
    /// <seealso cref="ITextSelection">ITextSelecton</seealso>へと変換できます。
    /// </ja>
    /// <en>
    /// It is possible to convert it into <seealso cref="ITextSelection">ITextSelecton</seealso> by using the GetAdapter 
    /// method for the view that is the terminal emulator. 
    /// </en>
    /// </remarks>
    public interface ISelection : ICommandTarget {
        /// <summary>
        /// <ja>
        /// 所有するビューを示します。
        /// </ja>
        /// <en>
        /// The owned view is shown. 
        /// </en>
        /// </summary>
        IPoderosaView OwnerView {
            get;
        }

        /// <summary>
        /// <ja>
        /// 選択範囲が変化したときのリスナを登録します。
        /// </ja>
        /// <en>
        /// The listener when the range of the selection changes is registered. 
        /// </en>
        /// </summary>
        /// <param name="listener"><ja>登録するリスナ</ja><en>The listener to regist.</en></param>
        void AddSelectionListener(ISelectionListener listener);
        /// <summary>
        /// <ja>
        /// 選択範囲が変化したときのリスナを解除します。
        /// </ja>
        /// <en>
        /// The listener when the range of the selection changes is released. 
        /// </en>
        /// </summary>
        /// <param name="listener">
        /// <ja>解除するリスナ</ja>
        /// <en>The listener to remove.</en>
        /// </param>
        void RemoveSelectionListener(ISelectionListener listener);
    }

    /* 典型的なシナリオ
     * 　選択開始・終了のUI操作はView内で閉じる。その中でSelectionの内部状態を更新する。
     * 　Viewの描画においては、自身のSelectionがあればそれをもとに描画する。
     * 　コピーなどの汎用コマンドは、Selectionに対してTranslateCommandを呼んで、Selectionのタイプによる固有コマンドを返させる。
     * 　コンテキストメニューは、SelectionをCommandTargetとするメニューツリーをビューが用意して表示する
     */

    //テキストの選択用
    /// <summary>
    /// <ja>
    /// テキストを選択するときの書式を指定します。
    /// </ja>
    /// <en>
    /// The format when the text is selected is specified. 
    /// </en>
    /// </summary>
    public enum TextFormatOption {
        /// <summary>
        /// <ja>
        /// 標準的なテキストとして返します。
        /// </ja>
        /// <en>
        /// Returns as a standard text. 
        /// </en>
        /// </summary>
        Default,
        /// <summary>
        /// <ja>
        /// 見たままの状態で返します。すなわちビューの右端で折り返された箇所に\r\nが付きます。
        /// </ja>
        /// <en>
        /// It returns it while seen. That is, \r\n adheres to the part turned on a right edge of the view. 
        /// </en>
        /// </summary>
        AsLook
    }

    /// <summary>
    /// <ja>
    /// 選択されているテキストを操作するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that operates text that has been selected.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ターミナルエミュレータを示すビューの場合、ISelectionは、このITextSelectionへと変換できます。
    /// </ja>
    /// <en>
    /// ISelection can be converted into this ITextSelection for the view that shows the terminal emulator. 
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// アクティブなビューで選択されているテキストを取得します。
    /// <code>
    /// // <value>target</value>からアクティブなビューを得ます
    /// IPoderosaView view = CommandTargetUtil.AsViewOrLastActivatedView(target);
    /// // ITextSelectionを得ます。
    /// ITextSelection select = (ITextSelection)view.CurrentSelection.GetAdapter(
    ///   typeof(ITextSelection));
    /// // 選択されているテキストを得ます
    /// if ((select != null) &amp;&amp; (!select.IsEmpty))
    /// {
    ///   MessageBox.Show(select.GetSelectedText(TextFormatOption.Default));
    /// }
    /// </code>
    /// </ja>
    /// <en>
    /// The text has been selected by an active view is got.
    /// <code>
    /// // Get the active view from <value>target</value>.
    /// IPoderosaView view = CommandTargetUtil.AsViewOrLastActivatedView(target);
    /// // Get ITextSelection
    /// ITextSelection select = (ITextSelection)view.CurrentSelection.GetAdapter(
    ///   typeof(ITextSelection));
    /// // Get the selected text.
    /// if ((select != null) &amp;&amp; (!select.IsEmpty))
    /// {
    ///   MessageBox.Show(select.GetSelectedText(TextFormatOption.Default));
    /// }
    /// </code>
    /// </en>
    /// </example>
    public interface ITextSelection : ISelection {
        /// <summary>
        /// <ja>
        /// 選択されているテキストを得ます。
        /// </ja>
        /// <en>
        /// The text that has been selected is obtained. 
        /// </en>
        /// </summary>
        /// <param name="opt"><ja>取得するフォーマットを指定します。</ja><en>Specifies the acquired format.</en></param>
        /// <returns><ja>選択されているテキストです。</ja><en>Selected text</en></returns>
        string GetSelectedText(TextFormatOption opt);
        /// <summary>
        /// <ja>
        /// 選択されているテキストが存在するかどうかを示します。何も選択されていないときにはtrue、選択されているときにはfalseです。
        /// </ja>
        /// <en>
        /// It is shown whether the text that has been selected exists. When true and selected, it is false when nothing has been selected. 
        /// </en>
        /// </summary>
        bool IsEmpty {
            get;
        }
        /// <summary>
        /// <ja>
        /// すべて選択状態にします。
        /// </ja>
        /// <en>
        /// It all puts it into the state of the selection. 
        /// </en>
        /// </summary>
        void SelectAll();
        /// <summary>
        /// <ja>
        /// 何も選択されていない状態にします。
        /// </ja>
        /// <en>
        /// It puts it into the state that nothing has been selected. 
        /// </en>
        /// </summary>
        void Clear();
    }

}
