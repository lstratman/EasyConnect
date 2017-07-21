/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CollectionUtil.cs,v 1.2 2011/10/27 23:21:56 kzmi Exp $
 */
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
#if UNITTEST
using NUnit.Framework;
#endif

namespace Poderosa.Util.Collections {

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class CollectionUtil {

        public static T GetItemFromLinkedList<T>(LinkedList<T> collection, int index) {
            LinkedListNode<T> node = collection.First;
            for (int i = 0; i < index; i++)
                node = node.Next;

            return node.Value;
        }

        public static void RemoveItemFromLinkedList<T>(LinkedList<T> collection, int index) {
            LinkedListNode<T> node = collection.First;
            for (int i = 0; i < index; i++)
                node = node.Next;
            collection.Remove(node);
        }

        public static ArrayList DeepCopyArrayList(ArrayList src) {
            ArrayList r = new ArrayList(src.Capacity);
            foreach (ICloneable ic in src)
                r.Add(ic.Clone());
            return r;
        }

        public static T[] ICollectionToArray<T>(ICollection collection) {
            T[] r = new T[collection.Count];
            int i = 0;
            foreach (object t in collection)
                r[i++] = (T)t; //キャスト必須かつnull許さない
            return r;
        }

        public static int ArrayIndexOf<T>(T[] array, T obj) {
            for (int i = 0; i < array.Length; i++)
                if (Object.ReferenceEquals(array[i], obj))
                    return i;
            return -1;
        }
    }

    //STLのpairと同等
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="F"></typeparam>
    /// <typeparam name="S"></typeparam>
    /// <exclude/>
    public class Pair<F, S> {
        private F _first;
        private S _second;
        public Pair(F f, S s) {
            _first = f;
            _second = s;
        }
        public F First {
            get {
                return _first;
            }
            set {
                _first = value;
            }
        }
        public S Second {
            get {
                return _second;
            }
            set {
                _second = value;
            }
        }
    }


    //Generic版Hashtable
    //System.Collections.Generic.Dictionary は、Itemプロパティが「キーが存在しないと例外を投げる」というクサれ仕様のため
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <exclude/>
    public class TypedHashtable<K, V> {
        private Hashtable _data;

        public TypedHashtable() {
            _data = new Hashtable();
        }
        public int Count {
            get {
                return _data.Count;
            }
        }
        public void Add(K key, V value) {
            _data[key] = value;
        }
        public V this[K key] {
            get {
                return (V)_data[key];
            }
            set {
                _data[key] = value;
            }
        }
        public void Remove(K key) {
            _data.Remove(key);
        }
        public void Clear() {
            _data.Clear();
        }
        public bool Contains(K key) {
            return _data.Contains(key);
        }
        public ICollection Values { //これはタイプセーフにできないな
            get {
                return _data.Values;
            }
        }
        public ICollection Keys {
            get {
                return _data.Keys;
            }
        }
        public IDictionaryEnumerator GetEnumerator() {
            return _data.GetEnumerator();
        }
    }

    //追加した順番を保持するテーブル。要素の少ないやつ用
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <exclude/>
    public class TypedSequentialTable<K, V>
        where K : class
        where V : class {
        private List<Pair<K, V>> _data;

        public TypedSequentialTable() {
            _data = new List<Pair<K, V>>();
        }
        public int Count {
            get {
                return _data.Count;
            }
        }
        public void Add(K key, V value) {
            _data.Add(new Pair<K, V>(key, value));
        }
        public V this[K key] {
            get {
                for (int i = 0; i < _data.Count; i++)
                    if (_data[i].First == key)
                        return _data[i].Second;
                return null;
            }
            set {
                for (int i = 0; i < _data.Count; i++) {
                    if (_data[i].First == key) {
                        _data[i].Second = value;
                        return;
                    }
                }
                Add(key, value);
            }
        }
        public void Remove(K key) {
            for (int i = 0; i < _data.Count; i++)
                if (_data[i].First == key)
                    _data.RemoveAt(i);
        }
        public void Clear() {
            _data.Clear();
        }
        public bool Contains(K key) {
            for (int i = 0; i < _data.Count; i++)
                if (_data[i].First == key)
                    return true;
            return false;
        }
        public ICollection<Pair<K, V>> Pairs {
            get {
                return _data;
            }
        }
    }


    //変換可能型についてIEnumerator, IEnumerableを実装
    //ことの起こりは、継承関係にあるクラスのIEnumerator同士がキャストできないことに業を煮やしたことによる。
    //たとえば、IEnumerator<Form>からIEnumerator<Control>にはやっぱり簡単に変換したいものだ。
    //ついでにGenericでない、普通のIEnumeratorからの変換もつけた。

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TX"></typeparam>
    /// <typeparam name="TY"></typeparam>
    /// <exclude/>
    public class ConvertingEnumerator<TX, TY> : IEnumerator<TY>
        where TX : class
        where TY : class {

        //変換するdelegate
        public delegate TY Converter(TX value);

        private IEnumerator<TX> _ie;
        private Converter<TX, TY> _converter;

        public ConvertingEnumerator(IEnumerator<TX> ie, Converter<TX, TY> conv) {
            _ie = ie;
            _converter = conv;
        }
        public void Dispose() {
            _ie.Dispose();
        }

        public TY Current {
            get {
                return _converter(_ie.Current);
            }
        }

        object IEnumerator.Current {
            get {
                return _converter(_ie.Current);
            }
        }

        public bool MoveNext() {
            return _ie.MoveNext();
        }

        public void Reset() {
            _ie.Reset();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TX"></typeparam>
    /// <typeparam name="TY"></typeparam>
    /// <exclude/>
    public class ConvertingEnumerable<TX, TY> : IEnumerable<TY>
        where TX : class
        where TY : class {

        private IEnumerable<TX> _ie;
        private Converter<TX, TY> _converter;

        public ConvertingEnumerable(IEnumerable<TX> ie) {
            _ie = ie;
            _converter = delegate(TX value) {
                return value as TY;
            }; //(TY)valueではエラーだ
        }
        public ConvertingEnumerable(IEnumerable<TX> ie, Converter<TX, TY> conv) {
            _ie = ie;
            _converter = conv;
        }

        public IEnumerator<TY> GetEnumerator() {
            return new ConvertingEnumerator<TX, TY>(_ie.GetEnumerator(), _converter);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new ConvertingEnumerator<TX, TY>(_ie.GetEnumerator(), _converter);
        }
    }

    //普通にキャストできると仮定しての列挙

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TX"></typeparam>
    /// <exclude/>
    public class ConvertingEnumerator<TX> : IEnumerator<TX>
        where TX : class {

        //変換するdelegate
        public delegate TX Converter(object value);

        private IEnumerator _ie;
        private Converter _converter;

        public ConvertingEnumerator(IEnumerator ie, Converter converter) {
            _ie = ie;
            _converter = converter;
        }
        public ConvertingEnumerator(IEnumerator ie) {
            _ie = ie;
            _converter = delegate(object value) {
                return value as TX;
            };
        }

        public TX Current {
            get {
                return _converter(_ie.Current);
            }
        }
        public void Dispose() {
            //Dispose()はGenerics.IEnumerator固有！プギャー
        }

        object IEnumerator.Current {
            get {
                return _converter(_ie.Current);
            }
        }

        public bool MoveNext() {
            return _ie.MoveNext();
        }

        public void Reset() {
            _ie.Reset();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TX"></typeparam>
    /// <exclude/>
    public class ConvertingEnumerable<TX> : IEnumerable<TX>
        where TX : class {

        private IEnumerable _ie;
        private ConvertingEnumerator<TX>.Converter _converter;

        public ConvertingEnumerable(IEnumerable ie) {
            _ie = ie;
            _converter = delegate(object value) {
                return value as TX;
            };
        }
        public ConvertingEnumerable(IEnumerable ie, ConvertingEnumerator<TX>.Converter converter) {
            _ie = ie;
            _converter = converter;
        }

        public IEnumerator<TX> GetEnumerator() {
            return new ConvertingEnumerator<TX>(_ie.GetEnumerator(), _converter);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new ConvertingEnumerator<TX>(_ie.GetEnumerator(), _converter);
        }
    }

#if UNITTEST
    [TestFixture]
    public class ConvertingCollectionTests {

        public class V {
            public int _value;
            public V(int v) {
                _value = v;
            }
            public static implicit operator V(int v) {
                return new V(v);
            }
        }

        [Test]
        public void Test1() {
            V[] t = new V[] { 10, 20, 30 };
            StringBuilder bld = new StringBuilder();
            //delegateが効いていることを確認すべく2倍にしてみる
            foreach (string x in new ConvertingEnumerable<V, string>(t, delegate(V v) {
                return (v._value * 2).ToString();
            })) {
                bld.Append(x);
            }
            Assert.AreEqual("204060", bld.ToString());
        }
        [Test]
        public void Test2() {
            int[] t = new int[] { 10, 20, 30 };
            StringBuilder bld = new StringBuilder();
            //単なるIEnumerableはint[]等にも適用可能
            foreach (string x in new ConvertingEnumerable<string>(t, delegate(object v) {
                return v.ToString();
            })) {
                bld.Append(x);
            }
            Assert.AreEqual("102030", bld.ToString());
        }
    }
#endif
}
