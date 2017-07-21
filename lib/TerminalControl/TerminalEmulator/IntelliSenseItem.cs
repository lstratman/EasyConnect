/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: IntelliSenseItem.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Util.Collections;

namespace Poderosa.Terminal {

    internal class IntelliSenseItemCollection : IIntelliSenseItemCollection {
        private LinkedList<IIntelliSenseItem> _items; //新しい入力ほど先頭
        public IntelliSenseItemCollection() {
            _items = new LinkedList<IIntelliSenseItem>();
        }

        public IEnumerable<IIntelliSenseItem> Items {
            get {
                return _items;
            }
        }

        public void UpdateItem(string[] command) {

            IntelliSenseItem newitem = new IntelliSenseItem(command);
            LinkedList<IIntelliSenseItem>.Enumerator e = _items.GetEnumerator();
            while (e.MoveNext()) {
                IntelliSenseItem item = e.Current as IntelliSenseItem;
                if (item.CompareTo(newitem) == 0) {
                    //先頭に持ってきてリターン
                    _items.Remove(item);
                    _items.AddFirst(newitem);
                    return;
                }
            }

            _items.AddFirst(newitem);
            if (_items.Count > TerminalEmulatorPlugin.Instance.TerminalEmulatorOptions.ShellHistoryLimitCount)
                _items.RemoveLast();
        }

        public IIntelliSenseItem FindItem(string[] command) {
            IntelliSenseItem newitem = new IntelliSenseItem(command);
            LinkedList<IIntelliSenseItem>.Enumerator e = _items.GetEnumerator();
            while (e.MoveNext()) {
                IntelliSenseItem item = e.Current as IntelliSenseItem;
                if (item.CompareTo(newitem) == 0)
                    return item;
            }
            return null;
        }
        public void RemoveItem(string[] command) {
            IntelliSenseItem newitem = new IntelliSenseItem(command);
            LinkedList<IIntelliSenseItem>.Enumerator e = _items.GetEnumerator();
            while (e.MoveNext()) {
                IntelliSenseItem item = e.Current as IntelliSenseItem;
                if (item.CompareTo(newitem) == 0) {
                    _items.Remove(item);
                    Debug.WriteLineIf(DebugOpt.IntelliSense, "Removed " + newitem.Format(' '));
                    break;
                }
            }
        }

        public IntelliSenseItemCollection Clone() {
            IntelliSenseItemCollection t = new IntelliSenseItemCollection();
            foreach (IIntelliSenseItem item in _items)
                t._items.AddLast(item.Clone());
            return t;
        }

        public void Clear() {
            _items.Clear();
        }
        public void RemoveAt(int index) {
            CollectionUtil.RemoveItemFromLinkedList(_items, index); //TODO 項目数多いときもい
        }
        public int Count {
            get {
                return _items.Count;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        public void AddLast(IIntelliSenseItem item) {
            _items.AddLast(item);
        }

        public string[] ToStringArray() {
            string[] t = new string[_items.Count];
            int i = 0;
            foreach (IIntelliSenseItem item in _items) {
                t[i++] = item.Format(' ');
            }
            return t;
        }
    }

    internal class IntelliSenseItem : IIntelliSenseItem {

        public enum MatchForwardResult {
            None,
            PartialArg,
            PartialChar
        }

        private IntelliSenseItem _originalItem; //部分補完のとき、元の要素を保持
        private string[] _text;
        private int _startIndex; //_textの_startIndex番目以降の要素を出す。ヒストリが直接持つやつはこれは0固定

        public IntelliSenseItem(string[] text) {
            _text = text;
            _startIndex = 0;
        }
        public IntelliSenseItem(string[] text, int startIndex, IntelliSenseItem original) {
            _text = text;
            _startIndex = startIndex;
            _originalItem = original;
            Debug.Assert(text.Length >= _startIndex);
        }

        public string[] Text {
            get {
                return _text;
            }
        }
        public int Length {
            get {
                return _text.Length - _startIndex;
            }
        }
        public IntelliSenseItem OriginalItem {
            get {
                return _originalItem;
            }
        }
        public IIntelliSenseItem Clone() {
            return new IntelliSenseItem(_text, _startIndex, _originalItem);
        }
        public string Format(char delimiter) {
            StringBuilder bld = new StringBuilder();
            for (int i = _startIndex; i < _text.Length; i++) {
                if (i > _startIndex)
                    bld.Append(delimiter);
                bld.Append(_text[i]);
            }
            return bld.ToString();
        }

        //前方一致チェック
        public MatchForwardResult MatchForward(string[] src) {
            if (src.Length == 0)
                return MatchForwardResult.PartialArg;
            if (this.Length < src.Length)
                return MatchForwardResult.None;

            for (int i = 0; i < src.Length - 1; i++) { //最終項以外は、完全一致でないと
                if (src[i] != _text[_startIndex + i])
                    return MatchForwardResult.None;
            }

            string last = src[src.Length - 1];
            string this_ = _text[_startIndex + src.Length - 1];

            //この判定ややこしいよ
            if (last == this_)
                return src.Length == this.Length ? MatchForwardResult.None : MatchForwardResult.PartialArg;
            else
                return this_.StartsWith(last) ? MatchForwardResult.PartialChar : MatchForwardResult.None;
        }

        public int CompareTo(object other0) {
            IntelliSenseItem other = other0 as IntelliSenseItem;
            Debug.Assert(other != null);
            int ml = Math.Min(this.Length, other.Length);
            for (int i = 0; i < ml; i++) {
                string this_ = _text[_startIndex + i];
                string other_ = other._text[other._startIndex + i];
                int r = this_.CompareTo(other_);
                if (r != 0)
                    return r;
            }

            return this.Length - other.Length; //長いほうが後
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }
}
