/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: AdapterEx.cs,v 1.2 2011/10/27 23:21:56 kzmi Exp $
 */
using System;

namespace Poderosa {
    /*

    */

    //Adapter関係
    // 原則１：インスタンスの区別をしない。型によってのみ成功するかどうかが決まる
    // 原則２：対称律・推移律を守る。COMのQueryInterfaceと同じ。
    /// <summary>
    /// <ja>
    /// 指定したインターフェイスを返す機構を提供します。
    /// </ja>
    /// <en>
    /// Return the mechanism of specified interface.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// オブジェクトがサポートするインターフェイスを返す機構を提供します。
    /// COM（Component Object Model）におけるQueryInterfaceと同じです。
    /// 実装者は、次の要件を守ってください。
    /// <ol>
    /// <li>インスタンスの区別をしないでください。型によってのみ成功するかどうかを決めてください</li>
    /// <li>対称律・推移律を守ってください。</li>
    /// </ol>
    /// </ja>
    /// <en>
    /// The mechanism that returns the interface supported by the object is offered. 
    /// It's same as QueryInterface on COM (Component Object Model).
    /// Developers must defend the following requirement.
    /// <ol>
    /// <li>Please do not distinguish the instance. Please decide whether to succeed only by the type. </li>
    /// <li>Please defend the symmetric law and the transition law. </li>
    /// </ol>
    /// </en>
    /// </remarks>
    public interface IAdaptable {
        /// <summary>
        /// <ja>
        /// 特定の型のインターフェイスを返します。
        /// </ja>
        /// <en>
        /// Return the interface of the specified type
        /// </en>
        /// </summary>
        /// <param name="adapter">
        /// <ja>
        /// 要求するインターフェイスの型
        /// </ja>
        /// <en>Type of required interface type.</en></param>
        /// <returns>
        /// <ja>要求したインターフェイスが戻ります。オブジェクトがそのインターフェイスを実装していない場合にはnullが戻ります。</ja>
        /// <en>Return the interface of required. Return null if the interface is not implemented on the object.</en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// 実装者は、このメソッド内で例外を返してはなりません。備えないインターフェイスの型が渡された場合にはnullを返してください。<br/>
        /// 多くの実装では、下記のコードを使い、IAdapterManagerインターフェイスの<seealso cref="IAdapterManager.GetAdapter">GetAdapterメソッド</seealso>
        /// を使って、AdapterManagerに変換を任せるようにします。
        /// <code>
        /// public IAdaptable GetAdapter(Type adapter)
        /// {
        ///     return poderosa_world.AdapterManager.GetAdapter(this, adapter);
        /// }
        /// </code>
        /// </ja>
        /// <en>
        /// When he or she returns the exception in this method, those who mount do not become it.
        /// Please return null when the type in the interface with which it doesn't provide is passed. 
        /// In a lot of mounting, conversion is left to AdapterManager by using the following code,
        /// and using the <seealso cref="IAdapterManager.GetAdapter">GetAdapter method</seealso> of the IAdapterManager interface. 
        /// <code>
        /// public IAdaptable GetAdapter(Type adapter)
        /// {
        ///     return poderosa_world.AdapterManager.GetAdapter(this, adapter);
        /// }
        /// </code>
        /// </en>
        /// </remarks>
        IAdaptable GetAdapter(Type adapter);
        //Note: ここにGenerics版( T GetAdapter<T>() )を作ることも考えたが、もしコード生成が動的に行われるとすれば起動時間に悪影響出るかもしれないのでやめておく。そのうち何とかするかも
    }

    /*
     * 古いタイプのAdapterFactory. Eclipseを真似てこうなったような記憶あるが不確か。これは使い勝手悪いので改める
    public interface IAdapterFactory {
        Type SourceType {
            get;
        }
        Type[] Adapters {
            get;
        }
        IAdaptable GetAdapter(IAdaptable obj, Type adapter);
    }
    */

    //双方向に変換できなければならない。
    /// <summary>
    /// <ja>
    /// アダプタファクトリを構成するインターフェイスです。
    /// </ja>
    /// <en>
    /// The interface that compose the adapter factory.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインターフェイスは、アダプタマネージャ（<seealso cref="IAdapterManager">IAdapterManager</seealso>）
    /// を使って型変換定義するときに使います。
    /// </ja>
    /// </remarks>
    public interface IDualDirectionalAdapterFactory {
        /// <summary>
        /// <ja>ソースの型を示します。</ja>
        /// <en>Type of source</en>
        /// </summary>
        Type SourceType {
            get;
        }
        /// <summary>
        /// <ja>アダプタの型を示します。
        /// </ja>
        /// <en>Type of adapter
        /// </en>
        /// </summary>
        Type AdapterType {
            get;
        }
        /// <summary>
        /// <ja>
        /// ソースからアダプタへと変換します。
        /// </ja>
        /// <en>
        /// Convert from source to adapter.
        /// </en>
        /// </summary>
        /// <param name="obj">
        /// <ja>ソースの型</ja>
        /// <en>Type of source</en>
        /// </param>
        /// <returns>
        /// <ja>アダプタの型が返されます。</ja>
        /// <en>Return the type of the adapter</en>
        /// </returns>
        IAdaptable GetAdapter(IAdaptable obj); //SourceType -> AdapterType
        /// <summary>
        /// <ja>
        /// アダプタからソースへと変換します。
        /// </ja>
        /// <en>
        /// Convert from adapter to source.
        /// </en>
        /// </summary>
        /// <param name="obj">
        /// <ja>アダプタの型</ja>
        /// <en>Type of adapter</en>
        /// </param>
        /// <returns>
        /// <ja>ソースの型が返されます。</ja>
        /// <en>Return the type of the source</en>
        /// </returns>
        IAdaptable GetSource(IAdaptable obj);  //AdapterType -> SourceType
    }

    //Generics版 IAdapterFactory
    /// <summary>
    /// <ja>
    /// Generics版のアダプタファクトリです。
    /// </ja>
    /// <en>
    /// Adapter factory of the Generics version.
    /// </en>
    /// </summary>
    /// <typeparam name="S">
    /// <ja>ソースの型</ja>
    /// <en>Type of the source</en>
    /// </typeparam>
    /// <typeparam name="T">
    /// <ja>アダプタの型</ja>
    /// <en>Type of adapter</en>
    /// </typeparam>
    /// <remarks>
    /// <ja>
    /// このインターフェイスは、アダプタマネージャ（<seealso cref="IAdapterManager">IAdapterManager</seealso>）
    /// を使って型変換定義するときに使います。
    /// </ja>
    /// <en>
    /// This interface is used when it defines the type conversation by adapter manager(<seealso cref="IAdapterManager">IAdapterManager</seealso>)
    /// </en>
    /// </remarks>
    public abstract class ITypedDualDirectionalAdapterFactory<S, T> : IDualDirectionalAdapterFactory
        where T : IAdaptable
        where S : IAdaptable {

        /// <summary>
        /// <ja>ソースの型を示します。</ja>
        /// <en>The type of the source</en>
        /// </summary>
        public Type SourceType {
            get {
                return typeof(S);
            }
        }

        /// <summary>
        /// <ja>アダプタの型を示します。</ja>
        /// <en>the type of the adapter</en>
        /// </summary>
        public Type AdapterType {
            get {
                return typeof(T);
            }
        }

        /// <summary>
        /// <ja>ソースからアダプタへと変換します。</ja>
        /// <en>Convert from the source th the adapter.</en>
        /// </summary>
        /// <param name="obj"><ja>ソースの型</ja><en>Type of the source</en></param>
        /// <returns><ja>アダプタの型が返されます。</ja><en>Return the type of the adapter.</en></returns>
        public IAdaptable GetAdapter(IAdaptable obj) {
            return GetAdapter((S)obj);
        }

        /// <summary>
        /// <ja>アダプタからソースへと変換します。</ja>
        /// <en>Convert from the adapter to the source.</en>
        /// </summary>
        /// <param name="obj">
        /// <ja>アダプタの型</ja>
        /// <en>Type of adapter</en>
        /// </param>
        /// <returns>
        /// <ja>ソースの型が返されます。</ja>
        /// <en>Return the type of the source.</en>
        /// </returns>
        public IAdaptable GetSource(IAdaptable obj) {
            return GetSource((T)obj);
        }

        /// <summary>
        /// <ja>ソースからアダプタへと変換します。</ja>
        /// <en>Convert from the source to the adapter.</en>
        /// </summary>
        /// <param name="obj">
        /// <ja>ソースの型</ja>
        /// <en>Type of the source</en>
        /// </param>
        /// <returns>
        /// <ja>アダプタの型が返されます。</ja>
        /// <en>Return the type of the adapter.</en>
        /// </returns>
        public abstract T GetAdapter(S obj);

        /// <summary>
        /// <ja>アダプタからソースへと変換します。</ja>
        /// <en>Convert from the adapter to the source</en>
        /// </summary>
        /// <param name="obj">
        /// <ja>アダプタの型</ja>
        /// <en>Type of the adapter</en>
        /// </param>
        /// <returns>
        /// <ja>ソースの型が返されます。</ja>
        /// <en>Return the type of the source.</en>
        /// </returns>
        public abstract S GetSource(T obj);
    }


    /// <summary>
    /// <ja>アダプタマネージャを示すインターフェイスです。</ja>
    /// <en>Interface that shows the adapter manager</en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// アダプタマネージャは、<seealso cref="Poderosa.Plugins.IPoderosaWorld">IPoderosaWorld</seealso>の
    /// <see cref="Poderosa.Plugins.IPoderosaWorld.AdapterManager">AdapterManagerプロパティ</see>から取得できます。
    /// </ja>
    /// <en>
    /// The adapter manager can be got by <see cref="Poderosa.Plugins.IPoderosaWorld.AdapterManager">AdapterManager property</see>
    /// on <seealso cref="Poderosa.Plugins.IPoderosaWorld">IPoderosaWorld</seealso>.
    /// </en>
    /// </remarks>
    public interface IAdapterManager {
        /// <summary>
        /// <ja>アダプタファクトリを登録します。</ja>
        /// <en>Regist the adapter factory.</en>
        /// </summary>
        /// <param name="factory">
        /// <ja>登録するアダプタファクトリ</ja>
        /// <en>Adapter factory to be regist.</en>
        /// </param>
        void RegisterFactory(IDualDirectionalAdapterFactory factory);
        /// <summary>
        /// <ja>
        /// アダプタファクトリを解除します。
        /// </ja>
        /// <en>
        /// Remove the adapter factory.
        /// </en>
        /// </summary>
        /// <param name="factory">
        /// <ja>解除するアダプタファクトリ</ja>
        /// <en>The adapter factory to remove.</en>
        /// </param>
        void RemoveFactory(IDualDirectionalAdapterFactory factory);
        /// <summary>
        /// <ja>
        /// アダプタファクトリを使った型変換機能を提供します。
        /// </ja>
        /// <en>
        /// Offers type conversation function by using adapter factory.
        /// </en>
        /// </summary>
        /// <param name="obj">
        /// <ja>変換対象となるオブジェクト</ja>
        /// <en>The object to convert.</en>
        /// </param>
        /// <param name="adapter">
        /// <ja>取得したいインターフェイス</ja>
        /// <en>The interface to get.</en>
        /// </param>
        /// <returns>
        /// <ja>変換されたインターフェイス</ja>
        /// <en>The converted interface</en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// 開発者は、このGetAdapterメソッドを使って、標準の型変換機構（<seealso cref="IAdaptable">IAdaptable</seealso>のGetAdapterの実装）を次のようにできます。
        /// <code>
        /// public IAdaptable GetAdapter(Type adapter)
        /// {
        ///     return poderosa_world.AdapterManager.GetAdapter(this, adapter);
        /// }
        /// </code>
        /// </ja>
        /// <en>
        /// The developer is good at a standard type conversion mechanism as follows by the use of this GetAdapter method. 
        /// <code>
        /// public IAdaptable GetAdapter(Type adapter)
        /// {
        ///     return poderosa_world.AdapterManager.GetAdapter(this, adapter);
        /// }
        /// </code>
        /// </en>
        /// </remarks>
        IAdaptable GetAdapter(IAdaptable obj, Type adapter);
        /// <summary>
        /// <ja>
        /// アダプタファクトリを使った型変換機能を提供します。
        /// </ja>
        /// <en>
        /// The type conversion function to use the adaptor factory is offered. 
        /// </en>
        /// </summary>
        /// <typeparam name="T">
        /// <ja>変換したいインターフェイスの型</ja>
        /// <en>Type in interface that wants to be converted</en>
        /// </typeparam>
        /// <param name="obj">
        /// <ja>変換対象となるオブジェクト</ja>
        /// <en>The object that wants to be converted.</en>
        /// </param>
        /// <returns>
        /// <ja>変換されたインターフェイス</ja>
        /// <en>Converted interface</en>
        /// </returns>
        T GetAdapter<T>(IAdaptable obj) where T : IAdaptable;
    }
}
