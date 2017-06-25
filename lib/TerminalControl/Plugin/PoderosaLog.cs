/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PoderosaLog.cs,v 1.2 2011/10/27 23:21:56 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Text;

using Poderosa.Plugins;
using Poderosa.Util.Collections;

namespace Poderosa {
    internal class PoderosaLogItem : IPoderosaLogItem {
        private PoderosaLog _parent;
        private IPoderosaLogCategory _category;
        private string _text;
        private int _index;

        public PoderosaLogItem(PoderosaLog parent, IPoderosaLogCategory cat, string text, int index) {
            _parent = parent;
            _category = cat;
            _text = text;
            _index = index;
        }

        public IPoderosaLogCategory Category {
            get {
                return _category;
            }
        }

        public string Text {
            get {
                return _text;
            }
        }

        public int Index {
            get {
                return _index;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return _parent.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

    internal class PoderosaLog : IPoderosaLog {
        public static PoderosaLog _instance;

        private IPoderosaWorld _world;
        private int _capacity;
        private LinkedList<PoderosaLogItem> _items;
        private PoderosaLogCategoryImpl _genericCategory;
        private List<IPoderosaLogListener> _listeners;

        public PoderosaLog(IPoderosaWorld world) {
            _capacity = 10000;
            _instance = this;
            _world = world;
            _items = new LinkedList<PoderosaLogItem>();
            _genericCategory = new PoderosaLogCategoryImpl("Poderosa");
            _listeners = new List<IPoderosaLogListener>();
        }
        public static PoderosaLog Instance {
            get {
                return _instance;
            }
        }

        public void AddItem(IPoderosaLogCategory category, string text) {
            lock (this) {
                PoderosaLogItem item = new PoderosaLogItem(this, category, text, _items.Count);
                _items.AddLast(item);
                while (_items.Count > _capacity)
                    _items.RemoveFirst();

                if (_listeners.Count > 0) {
                    foreach (IPoderosaLogListener l in _listeners)
                        l.OnNewItem(item);
                }
            }
        }

        public int Count {
            get {
                return _items.Count;
            }
        }

        public void AddChangeListener(IPoderosaLogListener listener) {
            _listeners.Add(listener);
        }

        public void RemoveChangeListener(IPoderosaLogListener listener) {
            _listeners.Remove(listener);
        }

        public IPoderosaLogCategory GenericCategory {
            get {
                return _genericCategory;
            }
        }

        public int Capacity {
            get {
                return _capacity;
            }
            set {
                _capacity = value;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return _world.AdapterManager.GetAdapter(this, adapter);
        }

        public IEnumerator<IPoderosaLogItem> GetEnumerator() {
            return new ConvertingEnumerator<IPoderosaLogItem>(_items.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }

        public IPoderosaWorld PoderosaWorld {
            get {
                return _world;
            }
        }
    }
}
