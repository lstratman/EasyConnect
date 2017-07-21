/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PoderosaWorldEx.cs,v 1.3 2011/10/27 23:21:56 kzmi Exp $
 */
using System;
using System.Globalization;

namespace Poderosa.Plugins {
    /// <summary>
    /// <ja>
    /// プラグインからプラグイン本体と通信するためのインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to communicate from plug-in with main body of plug-in.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインターフェイスは、Poderosa本体が
    /// <seealso cref="IPlugin.InitializePlugin">IPlugin.InitializePlugin</seealso>メソッドを呼び出して
    /// 初期化する際、引数として渡されます。<br/>
    /// プラグイン側では、このインターフェイスをローカル変数などに保存しておき、プラグイン本体との通信に利用します。<br/>
    /// </ja>
    /// <en>
    /// When Poderosa calls the IPlugin.InitializePlugin method and it initializes it, this interface is passed as an argument. 
    /// This interface is preserved in the local variable etc. , and it uses it to communicate with the main body of the plug-in on the plug-in side. 
    /// </en>
    /// </remarks>
    /// <ja><see href="chap01_01.html">本体とプラグインの基本インターフェイス</see></ja>
    /// <en><see href="chap01_01.html">Basic interface between Poderosa and plug-in.</see></en>
    public interface IPoderosaWorld : IAdaptable {
        /// <summary>
        /// <ja>
        /// <seealso cref="IAdapterManager">IAdapterManagerインターフェイス</seealso>を返します。
        /// </ja>
        /// <en>
        /// Return the <seealso cref="IAdapterManager">IAdapterManager interface.</seealso>
        /// </en>
        /// </summary>
        IAdapterManager AdapterManager {
            get;
        }
        /// <summary>
        /// <ja>
        /// <seealso cref="IPluginManager">IPluginManagerインターフェイス</seealso>を返します。
        /// </ja>
        /// <en>
        /// Return the <seealso cref="IPluginManager">IPluginManager interface</seealso>.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="IPluginManager">IPluginManagerインターフェイス</seealso>を返します。
        /// プラグイン側では、このインターフェイスを通じて、他のプラグインが提供するインターフェイスや拡張ポイントを取得できます。
        /// </ja>
        /// <en>
        /// Return the <seealso cref="IPluginManager">IPluginManager interface</seealso>を返します。
        /// The interface and the extension point that other plug-ins offer through this interface can be acquired on the plug-in side. 
        /// </en>
        /// </remarks>
        /// <ja><see href="chap02.html">Poderosa本体とプラグインとのやりとり</see></ja>
        /// <en><see href="chap02.html">Communication of Poderosa and plug-in</see></en>
        IPluginManager PluginManager {
            get;
        }
        /// <summary>
        /// <ja>
        /// <seealso cref="IPoderosaCulture">IPoderosaCulture</seealso>インターフェイスを返します。
        /// </ja>
        /// <en>
        /// Return the <seealso cref="IPoderosaCulture">IPoderosaCulture</seealso> interface.
        /// </en>
        /// </summary>
        IPoderosaCulture Culture {
            get;
        }

        void InitializePlugins();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IPoderosaApplication : IAdaptable {
        string HomeDirectory {
            get;
        }
        string ProfileHomeDirectory {
            get;
        }
        IPoderosaLog PoderosaLog {
            get;
        }
        string[] CommandLineArgs {
            get;
        }
        IPoderosaWorld Start();
        void Shutdown();
        string InitialOpenFile {
            get;
        } //無指定はnull
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IStartupContextSupplier : IAdaptable {
        StructuredText Preferences {
            get;
        }
        string PreferenceFileName {
            get;
        } //preferenceは常にファイルから読むとは限らない。nullのこともある
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IPoderosaCulture {
        CultureInfo InitialCulture {
            get;
        }
        CultureInfo CurrentCulture {
            get;
        }
        void SetCulture(CultureInfo culture);
        void AddChangeListener(ICultureChangeListener listener);
        void RemoveChangeListener(ICultureChangeListener listener);

        //OSが日本語かどうか
        bool IsJapaneseOS {
            get;
        }
        bool IsSimplifiedChineseOS {
            get;
        }
        bool IsTraditionalChineseOS {
            get;
        }
        bool IsKoreanOS {
            get;
        }
    }

    //言語変更通知

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ICultureChangeListener {
        void OnCultureChanged(CultureInfo newculture);
    }
}
