// Copyright 2016 The Poderosa Project.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

using Poderosa.Forms;
using System;
using System.Collections.Generic;

namespace Poderosa.Session {

    /// <summary>
    /// Storage that stores items associating with the particular <see cref="IPoderosaMainWindow"/>.
    /// </summary>
    /// <typeparam name="T">type of the item to be stored</typeparam>
    internal class StoragePerWindow<T> {

        private class StorageItem<ItemType> {
            public readonly WeakReference<IPoderosaMainWindow> Window;
            public ItemType Item;

            public StorageItem(IPoderosaMainWindow window, ItemType item) {
                this.Item = item;
                this.Window = new WeakReference<IPoderosaMainWindow>(window);
            }
        }

        private readonly object _sync = new object();
        private readonly List<StorageItem<T>> _list = new List<StorageItem<T>>();

        /// <summary>
        /// Gets an item associating with the specified window.
        /// </summary>
        /// <param name="window">window</param>
        /// <param name="item">found value is set</param>
        /// <returns>true if the matched item was found.</returns>
        public bool Get(IPoderosaMainWindow window, out T item) {
            lock (_sync) {
                StorageItem<T> itemFound = FindStorageItem(window);
                if (itemFound != null) {
                    item = itemFound.Item;
                    return true;
                }
                else {
                    item = default(T);
                    return false;
                }
            }
        }

        /// <summary>
        /// Puts an item associating with the specified window.
        /// </summary>
        /// <param name="window">window</param>
        /// <param name="item">item to be stored</param>
        public void Put(IPoderosaMainWindow window, T item) {
            lock (_sync) {
                StorageItem<T> itemFound = FindStorageItem(window);
                if (itemFound != null) {
                    itemFound.Item = item;
                }
                else {
                    _list.Add(new StorageItem<T>(window, item));
                }
            }
        }

        /// <summary>
        /// Find list item associating with the specified window.
        /// </summary>
        /// <param name="window">window</param>
        /// <returns>storage item</returns>
        private StorageItem<T> FindStorageItem(IPoderosaMainWindow window) {
            StorageItem<T> itemFound = null;
            // find a matched item in a loop which reduces items
            _list.RemoveAll(item => {
                try {
                    IPoderosaMainWindow target;
                    if (item.Window.TryGetTarget(out target)) {
                        if (Object.ReferenceEquals(window, target)) {
                            itemFound = item;
                        }
                        return false;   // keep this item
                    }
                }
                catch (Exception) {
                }
                return true;    // remove this item
            });
            return itemFound;
        }
    }
}
