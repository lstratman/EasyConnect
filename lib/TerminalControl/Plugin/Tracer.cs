/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Tracer.cs,v 1.2 2011/10/27 23:21:56 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using Poderosa.Util.Collections;

namespace Poderosa.Boot {

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TraceDocItem {
        private string _data;
        public TraceDocItem(string data) {
            _data = data;
        }
        public string Data {
            get {
                return _data;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TraceDocument : IEnumerable<TraceDocItem> {

        private LinkedList<TraceDocItem> _items;

        public TraceDocument() {
            _items = new LinkedList<TraceDocItem>();
        }

        public bool IsEmpty {
            get {
                return _items.Count == 0;
            }
        }

        public void Append(string data) {
            _items.AddLast(new TraceDocItem(data));
#if UNITTEST || DEBUG
            Debug.WriteLine(data);
#endif
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
        IEnumerator<TraceDocItem> IEnumerable<TraceDocItem>.GetEnumerator() {
            return _items.GetEnumerator();
        }
#if UNITTEST
        //期待通りのエラーメッセージが出ていることを確認するために必要
        public string GetDataAt(int index) {
            return CollectionUtil.GetItemFromLinkedList(_items, index).Data;
        }
#endif

    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITracer {
        void Trace(string string_id);
        void Trace(string string_id, string param1);
        void Trace(string string_id, string param1, string param2);
        void Trace(Exception ex);

        TraceDocument Document {
            get;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class DefaultTracer : ITracer {
        private TraceDocument _document;
        private StringResource _strResource;

        public DefaultTracer(StringResource sr) {
            _document = new TraceDocument();
            _strResource = sr;
        }

        public TraceDocument Document {
            get {
                return _document;
            }
        }

        public void Trace(string string_id) {
            _document.Append(_strResource.GetString(string_id));
        }

        public void Trace(string string_id, string param1) {
            _document.Append(String.Format(_strResource.GetString(string_id), param1));
        }

        public void Trace(string string_id, string param1, string param2) {
            _document.Append(String.Format(_strResource.GetString(string_id), param1, param2));
        }

        public void Trace(Exception ex) {
            _document.Append(ex.Message);
            _document.Append(ex.StackTrace);
        }
    }
}
