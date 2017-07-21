/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CommandPositionEx.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Poderosa.Commands {
    /// <summary>
    /// <ja>
    /// メニューやツールバーの位置を指定します。
    /// </ja>
    /// <en>
    /// Specifies the position of the menu and the toolbar.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 詳細は、<seealso cref="IPositionDesignation">IPositionDesignation</seealso>の解説を参照してください。
    /// </ja>
    /// <en>
    /// For more information, please refer to <seealso cref="IPositionDesignation">IPositionDesignation</seealso>.
    /// </en>
    /// </remarks>
    public enum PositionType {
        /// <summary>
        /// <ja>先頭</ja>
        /// <en>First</en>
        /// </summary>
        First,
        /// <summary>
        /// <ja>末尾</ja>
        /// <en>Last</en>
        /// </summary>
        Last,
        /// <summary>
        /// <ja>対象の直前</ja>
        /// <en>Previous to the object.</en>
        /// </summary>
        PreviousTo,
        /// <summary>
        /// <ja>対象の直後</ja>
        /// <en>Next to the object.</en>
        /// </summary>
        NextTo,
        /// <summary>
        /// <ja>明示的に指定しない</ja>
        /// <en>It doesn't specify it specifying it. </en>
        /// </summary>
        DontCare
    }


    /// <summary>
    /// <ja>
    /// メニューやツールバーの位置を制御するためのインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to control position of menu and toolbar
    /// </en>
    /// </summary>
    public interface IPositionDesignation : IAdaptable {
        //Targetにnullを指定したときは、First, Last, DontCareのどれか。
        //Targetが非nullのときは、PreviousTo, NextToのどれか。
        /// <summary>
        /// <ja>
        /// どの項目に対して前後関係を示すのかを指定します。
        /// </ja>
        /// <en>
        /// Specifies items the context is shown.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// このプロパティは、<seealso cref="DesignationPosition">DesignationPosition</seealso>の対象を示します。
        /// </para>
        /// <para>
        /// メニューの場合には、<seealso cref="IPoderosaMenuGroup">IPoderosaMenuGroup</seealso>を、ツールバーの場合には
        /// <seealso cref="Poderosa.Forms.IToolBarComponent">IToolBarComponent</seealso>を指定します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// This property shows the object of <seealso cref="DesignationPosition">DesignationPosition</seealso>. 
        /// </para>
        /// <para>
        /// <seealso cref="IPoderosaMenuGroup">IPoderosaMenuGroup</seealso> is specified for the menu and <seealso cref="Poderosa.Forms.IToolBarComponent">IToolBarComponent</seealso> is specified for the toolbar. 
        /// </para>
        /// </en>
        /// </remarks>
        IAdaptable DesignationTarget {
            get;
        } //can be null
        /// <summary>
        /// <ja>
        /// <seealso cref="DesignationTarget">DesignationTarget</seealso>に対する位置を指定します。
        /// </ja>
        /// <en>
        /// The position to <seealso cref="DesignationTarget">DesignationTarget</seealso> is specified. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// 指定できる位置は、「先頭」「末尾」「直前」「直後」「明示的に指定しない」のいずれかです。
        /// </para>
        /// <note type="implementnotes">
        /// 2つの異なるメニューが両者とも「先頭」を要求した場合など、実現不能な位置が構成されたときには、順序はPoderosaによって調停されます。そのため、必ずしも指定した位置どおりに並ぶとは限りません。
        /// </note>
        /// <list type="table">
        ///     <listheader>
        ///         <term>値</term>
        ///         <description>意味</description>
        ///         <description><seealso cref="DesignationTarget">DesignationTarget</seealso>の値</description>
        ///     </listheader>
        ///     <item>
        ///         <term>First</term>
        ///         <description>先頭</description>
        ///         <description>nullを指定してください</description>
        ///     </item>
        ///     <item>
        ///         <term>Last</term>
        ///         <description>末尾</description>
        ///         <description>nullを指定してください</description>
        ///     </item>
        ///     <item>
        ///         <term>PreviousTo</term>
        ///         <description><seealso cref="DesignationTarget">DesignationTarget</seealso>で指定した項目の直前</description>
        ///         <description>対象項目を渡してください</description>
        ///     </item>
        ///     <item>
        ///         <term>NextTo</term>
        ///         <description><seealso cref="DesignationTarget">DesignationTarget</seealso>で指定した項目の直後</description>
        ///         <description>対象項目を渡してください</description>
        ///     </item>
        ///     <item>
        ///         <term>DontCare</term>
        ///         <description>明示的に指定しない</description>
        ///         <description>nullを指定してください</description>
        ///     </item>
        /// </list>
        /// </ja>
        /// <en>
        /// <para>
        /// The position that can be specified is either "Head", "End", "Immediately before", "Immediately after" or "Do not specify it specifying it". 
        /// </para>
        /// <note type="implementnotes">
        /// When the position that cannot be achieved is composed when two different menus demand both and "Head", the order is mediated by Poderosa. 
        /// Therefore, it doesn't necessarily queue up as it is a specified position. 
        /// </note>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///         <description>Value of <seealso cref="DesignationTarget">DesignationTarget</seealso></description>
        ///     </listheader>
        ///     <item>
        ///         <term>First</term>
        ///         <description>Head</description>
        ///         <description>Set null</description>
        ///     </item>
        ///     <item>
        ///         <term>Last</term>
        ///         <description>Last</description>
        ///         <description>Set null</description>
        ///     </item>
        ///     <item>
        ///         <term>PreviousTo</term>
        ///         <description>The item specified with <seealso cref="DesignationTarget">DesignationTarget</seealso></description>
        ///         <description>Pass the specified item.</description>
        ///     </item>
        ///     <item>
        ///         <term>NextTo</term>
        ///         <description>The item specified with <seealso cref="DesignationTarget">DesignationTarget</seealso> just behind</description>
        ///         <description>Pass the specified item.</description>
        ///     </item>
        ///     <item>
        ///         <term>DontCare</term>
        ///         <description>It doesn't specify it specifying it. </description>
        ///         <description>Set null</description>
        ///     </item>
        /// </list>
        /// </en>
        /// </remarks>
        PositionType DesignationPosition {
            get;
        }
    }

    //これだけなので同じファイルにソータとテストケースまで書いてしまう
    /// <exclude/>
    public class PositionDesignationSorter {
        private class Entry : IComparable<Entry> {
            public int index;
            public IAdaptable content;
            public IPositionDesignation designation;
            public Entry dependency;

            public Entry(int i, IAdaptable c) {
                index = i;
                content = c;
                designation = (IPositionDesignation)c.GetAdapter(typeof(IPositionDesignation));
            }

            private bool IsIndependent {
                get {
                    return dependency == null;
                }
            }

            //依存しているものが先に来るように並び替える
            public int CompareTo(Entry other) {
                if (this.IsIndependent) {
                    if (other.IsIndependent)
                        return this.index - other.index; //元の順序を保持
                    else
                        return -1; //自分が前に来る
                }
                else {
                    if (other.IsIndependent)
                        return 1; //自分が後に来る
                    else {
                        int r = this.dependency.CompareTo(other.dependency);
                        if (r == 0)
                            r = this.index - other.index; //依存先で判定できない場合は仕方ない
                        return r;
                    }
                }
            }
        }

        //依存関係に従ってソートする。各IAdaptableは、オプショナルでIPositionDesignationを実装する。
        //依存先があるならば、srcに含まれている必要がある。
        public static ICollection SortItems(ICollection src) {
            List<Entry> map = new List<Entry>(src.Count);
            int i = 0;
            //Entryを構成
            foreach (IAdaptable a in src) {
                Debug.Assert(a != null);
                map.Add(new Entry(i++, a));
            }
            //依存先をチェック
            foreach (Entry e in map)
                e.dependency = FindDependencyFor(e, map);
            //ソート
            //TODO 依存関係にループがあるときを救済
            map.Sort();

            //結果の構築
            return BuildResult(map);
        }

        private static ICollection BuildResult(List<Entry> map) {
            LinkedList<IAdaptable> result = new LinkedList<IAdaptable>();
            LinkedListNode<IAdaptable> firstzone = null;
            LinkedListNode<IAdaptable> lastzone = null;
            foreach (Entry e in map) {
                if (e.dependency == null) {
                    //依存物なしのばあい、first-dontcare-lastの各ゾーン順で並ぶ。各ゾーン内は元の入力順を保持

                    //designationなしはDontCareに等しい
                    if (e.designation == null || e.designation.DesignationPosition == PositionType.DontCare) {
                        if (lastzone == null)
                            result.AddLast(e.content);
                        else
                            result.AddBefore(lastzone, e.content);
                    }
                    else if (e.designation.DesignationPosition == PositionType.First) {
                        if (firstzone == null)
                            firstzone = result.AddFirst(e.content);
                        else
                            firstzone = result.AddAfter(firstzone, e.content);
                    }
                    else if (e.designation.DesignationPosition == PositionType.Last) {
                        if (lastzone == null)
                            lastzone = result.AddLast(e.content);
                        else
                            lastzone = result.AddBefore(lastzone, e.content);
                    }
                }
                else { //依存物あり
                    LinkedListNode<IAdaptable> n = result.Find(e.dependency.content);
                    Debug.Assert(n != null);
                    Debug.Assert(e.designation.DesignationPosition != PositionType.DontCare);
                    if (e.designation.DesignationPosition == PositionType.NextTo)
                        result.AddAfter(n, e.content);
                    else
                        result.AddBefore(n, e.content);
                }
            }
            return result;
        }

        //依存先を見つける
        private static Entry FindDependencyFor(Entry e, List<Entry> map) {
            if (e.designation == null)
                return null;
            else if (e.designation.DesignationTarget == null) {
                PositionType p = e.designation.DesignationPosition;
                if (!(p == PositionType.First || p == PositionType.Last || p == PositionType.DontCare))
                    throw new ArgumentException("if IPositionDesignation#Target returns null, #Position must be First, Last, or DontCare");
                return null;
            }
            else {
                IAdaptable target = e.designation.DesignationTarget;
                Entry r = Find(target, map);
                if (r == null)
                    throw new ArgumentException("IPositionDesignation#Target must return a member of the argument collection of SortItem()");
                if (!(e.designation.DesignationPosition == PositionType.NextTo || e.designation.DesignationPosition == PositionType.PreviousTo))
                    throw new ArgumentException("if IPositionDesignation#Target returns an object, #Position must be PreviousTo or NextTo");
                return r;
            }
        }

        private static Entry Find(IAdaptable target, List<Entry> map) {
            foreach (Entry e in map)
                if (e.content == target)
                    return e;
            return null;
        }
    }
}
