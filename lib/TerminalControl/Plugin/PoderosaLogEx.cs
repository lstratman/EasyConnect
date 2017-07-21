/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PoderosaLogEx.cs,v 1.2 2011/10/27 23:21:56 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Poderosa {
    /// <summary>
    /// <ja>
    /// ログのカテゴリを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface of category of log.
    /// </en>
    /// </summary>
    public interface IPoderosaLogCategory : IAdaptable {
        /// <summary>
        /// <ja>
        /// カテゴリ名です。
        /// </ja>
        /// <en>
        /// Name of category.
        /// </en>
        /// </summary>
        string Name {
            get;
        }
    }
    /// <summary>
    /// <ja>
    /// ログのアイテムを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface of log items
    /// </en>
    /// </summary>
    public interface IPoderosaLogItem : IAdaptable {
        /// <summary>
        /// <ja>
        /// ログのカテゴリです。
        /// </ja>
        /// <en>
        /// Category of log.
        /// </en>
        /// </summary>
        IPoderosaLogCategory Category {
            get;
        }
        /// <summary>
        /// <ja>
        /// ログのテキストです。
        /// </ja>
        /// <en>
        /// Text of log.
        /// </en>
        /// </summary>
        string Text {
            get;
        }

        /// <summary>
        /// <ja>
        /// ログアイテムのインデックス番号です。
        /// </ja>
        /// <en>
        /// Index number of log item.
        /// </en>
        /// </summary>
        int Index {
            get;
        }
        //Timeあたりもあっていいかも
    }

    /// <summary>
    /// <ja>
    /// ログ機能を提供します。
    /// </ja>
    /// <en>
    /// Offered log function.
    /// </en>
    /// </summary>
    public interface IPoderosaLog : IAdaptable, IEnumerable<IPoderosaLogItem> {
        /// <summary>
        /// <ja>
        /// ログの容量を取得／設定します。
        /// </ja>
        /// <en>
        /// Get / set the capacity of log.
        /// </en>
        /// </summary>
        int Capacity {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// ログに書き込みます。
        /// </ja>
        /// <en>
        /// Write log.
        /// </en>
        /// </summary>
        /// <param name="category">
        /// <ja>ログのカテゴリです。</ja>
        /// <en>Category of log.</en>
        /// </param>
        /// <param name="text">
        /// <ja>書き込むテキストです。</ja>
        /// <en>Text to write.</en>
        /// </param>
        void AddItem(IPoderosaLogCategory category, string text);
        /// <summary>
        /// <ja>
        /// ログのアイテムの個数を示します。
        /// </ja>
        /// <en>
        /// Count of log item.
        /// </en>
        /// </summary>
        int Count {
            get;
        }

        /// <summary>
        /// <ja>
        /// ログのリスナを登録します。
        /// </ja>
        /// <en>
        /// Add the log listener
        /// </en>
        /// </summary>
        /// <param name="listener">
        /// <ja>登録するリスナ</ja>
        /// <en>Listener to regist.</en>
        /// </param>
        void AddChangeListener(IPoderosaLogListener listener);

        /// <summary>
        /// <ja>
        /// ログのリスナを解除します。
        /// </ja>
        /// <en>
        /// Remove the log listener
        /// </en>
        /// </summary>
        /// <param name="listener">
        /// <ja>解除するリスナ</ja>
        /// <en>Listener to release.</en>
        /// </param>
        void RemoveChangeListener(IPoderosaLogListener listener);

        /// <summary>
        /// <ja>
        /// 標準のログカテゴリを示します。
        /// </ja>
        /// <en>
        /// Get the generic category of log.
        /// </en>
        /// </summary>
        IPoderosaLogCategory GenericCategory {
            get;
        }

    }

    /// <summary>
    /// <ja>
    /// ログリスナを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface of loglistener.
    /// </en>
    /// </summary>
    public interface IPoderosaLogListener {
        /// <summary>
        /// <ja>
        /// 新しいログアイテムが追加されるときに呼び出されます。
        /// </ja>
        /// <en>
        /// Called when added the new log item.
        /// </en>
        /// </summary>
        /// <param name="item">
        /// <ja>追加されるアイテムです。</ja>
        /// <en>Item to add.</en>
        /// </param>
        void OnNewItem(IPoderosaLogItem item);
    }

    /// <summary>
    /// <ja>
    /// ログカテゴリを実装するヘルパクラスです。
    /// </ja>
    /// <en>
    /// Helperclasss for implementation of category of log.
    /// </en>
    /// </summary>
    public class PoderosaLogCategoryImpl : IPoderosaLogCategory {
        private string _name;
        /// <summary>
        /// <ja>
        /// ログカテゴリを作成します。
        /// </ja>
        /// <en>
        /// Create the category of log.
        /// </en>
        /// </summary>
        /// <param name="name">
        /// <ja>ログカテゴリの名前です。</ja>
        /// <en>Name of category of log.</en>
        /// </param>
        public PoderosaLogCategoryImpl(string name) {
            _name = name;
        }
        /// <summary>
        /// <ja>
        /// ログカテゴリ名を返します。
        /// </ja>
        /// <en>
        /// Get the category of log.
        /// </en>
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return PoderosaLog.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

    /// <summary>
    /// <ja>
    /// ログアイテムを構成するヘルパクラスです。
    /// </ja>
    /// <en>
    /// Helper class to compose the log item.
    /// </en>
    /// </summary>
    public class PoderosaLogItemImpl : IPoderosaLogItem {
        private IPoderosaLogCategory _category;
        private string _text;
        private int _index;

        /// <summary>
        /// <ja>
        /// ログアイテムを作成します。
        /// </ja>
        /// <en>
        /// Create the log item.
        /// </en>
        /// </summary>
        /// <param name="category"><ja>ログアイテムのカテゴリです。</ja>
        /// <en>Category of log item.</en>
        /// </param>
        /// <param name="text"><ja>ログのテキストです。</ja>
        /// <en>Text of log.</en>
        /// </param>
        /// <param name="index"><ja>ログのインデックス位置です。</ja>
        /// <en>Index position of log.</en></param>
        public PoderosaLogItemImpl(IPoderosaLogCategory category, string text, int index) {
            _category = category;
            _text = text;
            _index = index;
        }

        /// <summary>
        /// <ja>
        /// ログカテゴリを示します。
        /// </ja>
        /// <en>
        /// Category of log.
        /// </en>
        /// </summary>
        public IPoderosaLogCategory Category {
            get {
                return _category;
            }
        }

        /// <summary>
        /// <ja>
        /// ログのテキストを示します。
        /// </ja>
        /// <en>
        /// Text of log.
        /// </en>
        /// </summary>
        public string Text {
            get {
                return _text;
            }
        }

        /// <summary>
        /// <ja>
        /// ログのインデックス位置を示します。
        /// </ja>
        /// <en>
        /// Index of log.
        /// </en>
        /// </summary>
        public int Index {
            get {
                return _index;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return PoderosaLog.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }
}
