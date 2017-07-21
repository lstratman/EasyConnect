/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PluginEx.cs,v 1.2 2011/10/27 23:21:56 kzmi Exp $
 */
using System;
using System.Collections.Generic;

namespace Poderosa.Plugins {

    /// <summary>
    /// <ja>
    /// プラグインの属性を設定します。
    /// </ja>
    /// <en>
    /// Set the attribute of the plug-in
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// すべてのプラグインは、PluginInfoAttribute属性を備えなければなりません。
    /// </ja>
    /// <en>
    /// All plug-ins must have the PluginInfoAttribute attribute.
    /// </en>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class PluginInfoAttribute : Attribute {
        /// <summary>
        /// <ja>プラグインの識別子となる「プラグインID」です。必須です。</ja>
        /// <en>REQUIRED:Plug-in that identifies the plug-in.</en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// すべてのプラグインで唯一無二のものを指定する必要があります。
        /// 識別子は、Javaのパッケージ標準に準じた方式で識別されます。
        /// 開発者が保有するドメイン名があるならば、それに基づいてプラグインIDを定めてください（たとえば、「jp.co.example.任意名」など） 。
        /// Poderosa標準のプラグインのID属性では、「org.poderosa」が使われています。開発者が独自のプラグインを作成する際には、「org.poderosa」以下のID値を付けてはいけません。「org.poderosa」以下のID値を付ける場合には、Poderosa開発者コミュニティでの承認を要します。 
        /// </ja>
        /// <en>
        /// It is necessary to specify the unique one by all plug-ins. 
        /// The identifier is identified by the method based on the package standard of Java. 
        /// Please provide plug-in ID based on it if there is a domain name that the developer has (for instance, "jp.co.example.foo" etc.). 
        ///In the ID attribute of the plug-in of the Poderosa standard, "org.poderosa" is used.
        /// Do not set ID following "org.poderosa" when the developer makes an original plug-in. "org.poderosa"
        /// When ID is set, approval in the Poderosa developer community is required. 
        /// </en>
        /// </remarks>
        public string ID;
        /// <summary>
        /// <ja>プラグインの名称です</ja>
        /// <en>Name of the plug-in.</en>
        /// </summary>
        public string Name;
        /// <summary>
        /// <ja>依存する他のプラグインのプラグインIDです。</ja>
        /// <en>The plug-in ID that depends other plug-ins. </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// 依存する他のプラグインがある場合には、そのプラグインIDをセミコロン（;）で区切って列挙します。
        /// ここで列挙したプラグインよりも後に読み込まれることが保証されます。
        /// </ja>
        /// <en>
        /// When other depending plug-ins exist, the plug-in ID is delimited by semicolon (;) and enumerated. 
        /// It is guaranteed to be read from the plug-in enumerated here back. 
        /// </en>
        /// </remarks>
        public string Dependencies;
        /// <summary>
        /// <ja>プラグインのバージョン番号です。</ja>
        /// <en>Version number of the plug-in.</en>
        /// </summary>
        public string Version;
        /// <summary>
        /// <ja>プラグインの著作者情報です</ja>
        /// <en>Copyright information of the plug-in.</en>
        /// </summary>
        public string Author;
    }

    /// <summary>
    /// <ja>
    /// プラグインを構成するアセンブリが持つべき属性です。
    /// </ja>
    /// <en>
    /// It is an attribute that the assembly that composes the plug-in should have. 
    /// </en>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class PluginDeclarationAttribute : Attribute {
        /// <summary>
        /// <ja>このアセンブリに含まれるプラグインのクラスを定義します。</ja>
        /// <en>
        /// Define the class of the plug-in included in this assembly.
        /// </en>
        /// </summary>
        /// <param name="type"><ja>プラグインのクラスです</ja>
        /// <en>
        /// Class of plug-in.
        /// </en>
        /// </param>
        /// <remarks>
        /// <ja>
        /// Poderosaは<var>type</var>に指定されたクラスのインスタンスを作り、そのInitializePluginメソッドを呼び出すことでプラグインを初期化し、動作可能な状態にします。
        /// </ja>
        /// <en>
        /// The plug-in is initialized by making the instance of the class specified for type, 
        /// and calling the InitializePlugin method, and Poderosa is put into the state that can be operated. 
        /// </en>
        /// </remarks>
        public PluginDeclarationAttribute(Type type) {
            Target = type;
        }
        /// <summary>
        /// <ja>プラグインを構成するクラスです。</ja>
        /// <en>Class that composes plug-in</en>
        /// </summary>
        public Type Target;
    }

    /// <summary>
    /// <ja>
    /// すべてのプラグインが実装しなければならないインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that all plug-ins should implement
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 開発者は、IPluginインターフェイスを実装する代わりに、<seealso cref="PluginBase">PluginBaseクラス</seealso>
    /// から継承したクラスとして作成することもできます。
    /// </ja>
    /// <en>
    /// The developer can make it as a class that inheritances to from the 
    /// <seealso cref="PluginBase">PluginBase class</seealso> instead of implementing the IPlugin interface. 
    /// </en>
    /// </remarks>
    public interface IPlugin : IAdaptable {
        /// <summary>
        /// <ja>
        /// プラグインが初期化される際に呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// Method of call when plug-in is initialized
        /// </en>
        /// </summary>
        /// <param name="poderosa">
        /// <ja>
        /// Poderosa本体と通信するためのIPoderosaWorldインターフェイスです。
        /// </ja>
        /// <en>
        /// IPoderosaWorld interface to communicate with Poderosa
        /// </en>
        /// </param>
        /// <remarks>
        /// <ja>
        /// このメソッドは、Poderosa本体によってプラグインが読み込まれた直後に呼び出されます。<br/>
        /// 引き渡されるIPoderosaWorldインターフェイスはプラグインが解放されるまで不変です。<br/>
        /// プラグイン開発者は、このメソッド内でプラグインの初期化処理をすることになります。
        /// </ja>
        /// <en>
        /// This method is called immediately after the plug-in was read by Poderosa. 
        /// The IPoderosaWorld interface handed over is invariable until the plug-in is relesed.
        /// The developer will do the initialization of the plug-in in this method. 
        /// </en>
        /// </remarks>
        void InitializePlugin(IPoderosaWorld poderosa);
        /// <summary>
        /// <ja>
        /// プラグインが解放される直前に呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// Method of call immediately before plug-in is released.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// プラグイン開発者は、このメソッド内でプラグインの後処理をしてください。
        /// </ja>
        /// <en>
        /// The developer must postprocess the plug-in in this method. 
        /// </en>
        /// </remarks>
        void TerminatePlugin();
    }

    /// <summary>
    /// <ja>
    /// プラグインを統括管理する「プラグインマネージャ」のインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface of "Plug-in manager" that manages generalization as for the plug-in. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// IPluginManagerは、<seealso cref="IPoderosaWorld">IPoderosaWorld</seealso>の<see cref="IPoderosaWorld.PluginManager">PluginManagerプロパティ</see>から取得できます。
    /// </ja>
    /// <en>
    /// IPluginManager can be acquired from the <see cref="IPoderosaWorld.PluginManager">PluginManager property</see> of 
    /// <seealso cref="IPoderosaWorld">IPoderosaWorld</seealso>. 
    /// </en>
    /// </remarks>
    public interface IPluginManager : IAdaptable {
        //Plugins
        /// <summary>
        /// <ja>プラグインを検索します。</ja>
        /// <en>Retrieval of the plug-in.</en>
        /// </summary>
        /// <param name="id">
        /// <ja>検索するプラグインIDです。
        /// </ja>
        /// <en>Retrieved plug-in ID
        /// </en>
        /// </param>
        /// <param name="adapter">
        /// <ja>
        /// 取得するプラグインのインターフェイスの型です。
        /// </ja>
        /// <en>
        /// Type in interface of acquired plug-in.
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// 見つかったプラグインのインターフェイスを返します。該当のプラグインが見つからなかった場合には、nullが戻ります。
        /// </ja>
        /// <en>
        /// The interface of the found plug-in is returned. Null returns when the plug-in of the correspondence is not found. 
        /// </en>
        /// </returns>
        object FindPlugin(string id, Type adapter);
        //Extension Points
        /// <summary>
        /// <ja>
        /// 拡張ポイントを作成します。
        /// </ja>
        /// <en>
        /// Making of the extension point.
        /// </en>
        /// </summary>
        /// <param name="id">
        /// <ja>
        /// 作成する拡張ポイントの「拡張ポイントID」です。
        /// </ja>
        /// <en>
        /// Extension point ID of made extension point
        /// </en>
        /// </param>
        /// <param name="requiredInterface">
        /// <ja>
        /// 拡張ポイントが要求するインターフェイスです。
        /// </ja>
        /// <en>
        /// Interface that extension point demands.
        /// </en>
        /// </param>
        /// <param name="owner">
        /// <ja>
        /// 拡張ポイントの所有者となるプラグインのオブジェクトです。多くの場合、「this」を渡します。
        /// </ja>
        /// <en>
        /// It is an object of the plug-in that becomes the owner of the extension point.
        /// In many cases, "this" is passed. 
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// 正常に拡張ポイントが作成された場合、作成された拡張ポイントのIExtensionPointインターフェイスが戻ります。
        /// 拡張ポイントの作成に失敗した場合には、nullが戻ります。
        /// </ja>
        /// <en>
        /// The IExtensionPoint interface of the made extension point returns when the extension point is normally made. 
        /// Null returns when failing in making the extension point. 
        /// </en>
        /// </returns>
        IExtensionPoint CreateExtensionPoint(string id, Type requiredInterface, IPlugin owner);
        /// <summary>
        /// <ja>
        /// 拡張ポイントを検索します。
        /// </ja>
        /// <en>
        /// Retrieval of the extension point.
        /// </en>
        /// </summary>
        /// <param name="id">
        /// <ja>
        /// 検索する拡張ポイントIDです。
        /// </ja>
        /// <en>
        /// Retrieved extension point ID
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// 該当の拡張ポイントが見つかった場合には、そのIExtensionPointインターフェイスが戻ります。
        /// 拡張ポイントが見つからなかった場合には、nullが戻ります。
        /// </ja>
        /// <en>
        /// The IExtensionPoint interface returns when the extension point of the correspondence is found. 
        /// Null returns when the enhancing point is not found. 
        /// </en>
        /// </returns>
        IExtensionPoint FindExtensionPoint(string id);
    }

    /// <summary>
    /// <ja>
    /// 拡張ポイントを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows extension point.
    /// </en>
    /// </summary>
    public interface IExtensionPoint {
        /// <summary>
        /// <ja>
        /// 拡張ポイントを所有するプラグインのIPluginインターフェイスです。
        /// </ja>
        /// <en>
        /// IPlugin interface of plug-in to own extension point.
        /// </en>
        /// </summary>
        IPlugin OwnerPlugin {
            get;
        }
        /// <summary>
        /// <ja>
        /// 拡張ポイントIDです。
        /// </ja>
        /// <en>
        /// Extension point ID.
        /// </en>
        /// </summary>
        string ID {
            get;
        }
        /// <summary>
        /// <ja>
        /// 拡張ポイントが要求するインターフェイスです。
        /// </ja>
        /// <en>
        /// Interface that entension point demands
        /// </en>
        /// </summary>
        Type ExtensionInterface {
            get;
        }
        /// <summary>
        /// <ja>
        /// 拡張ポイントにオブジェクトを登録します。
        /// </ja>
        /// <en>
        /// The object is registered in the extension point. 
        /// </en>
        /// </summary>
        /// <param name="extension">
        /// <ja>
        /// 登録するオブジェクトです。このオブジェクトはExtensionInterfaceプロパティで指定される
        /// インターフェイスを備えていなければなりません。
        /// </ja>
        /// <en>
        /// It is a registered object. This object should have the interface specified in the ExtensionInterface property. 
        /// </en>
        /// </param>
        /// <exception cref="ArgumentException">
        /// <ja>
        /// extensionに指定されたオブジェクトがExtensionInterfaceプロパティで指定されるインターフェイスを備えていません。
        /// </ja>
        /// <en>
        /// The interface for which the object specified for extension is specified in the ExtensionInterface property is not provided with. 
        /// </en>
        /// </exception>
        void RegisterExtension(object extension);
        /// <summary>
        /// <ja>
        /// この拡張ポイントに登録されているオブジェクトの配列を取得します。
        /// </ja>
        /// <en>
        /// Get the array of the object registered in this extension point.
        /// </en>
        /// </summary>
        /// <returns>
        /// <ja>
        /// この拡張ポイントに登録されているオブジェクトの配列が返されます。
        /// </ja>
        /// <en>
        /// The array of the object registered in this extension point is returned. 
        /// </en>
        /// </returns>
        Array GetExtensions();
    }

    //最初のExtensionPoint用のインタフェース
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IRootExtension {
        void InitializeExtension();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IGUIMessageLoop : IRootExtension {
        void RunExtension();
    }

    //Plugin Inspector用に
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IPluginInspector : IAdaptable {
        IEnumerable<IPluginInfo> Plugins {
            get;
        }
        IEnumerable<IExtensionPoint> ExtensionPoints {
            get;
        }
        IPluginInfo GetPluginInfo(IPlugin plugin);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IPluginInfo : IAdaptable {
        IPlugin Body {
            get;
        }
        PluginInfoAttribute PluginInfoAttribute {
            get;
        }
        PluginStatus Status {
            get;
        }
    }

    /// <summary>
    /// <ja>
    /// プラグイン開発者に、IPluginインターフェイスとIAdaptableインターフェイスの標準実装を提供します。
    /// </ja>
    /// <en>
    /// A default implementation in the IPlugin interface and the IAdaptable interface is offered to the developer. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// プラグイン開発者は、このクラスから継承させることで少ないコードでプラグインを書くことができます。
    /// 下記の実装になっています。
    /// <code>
    /// public abstract class PluginBase : MarshalByRefObject, IPlugin
    /// {
    ///    protected IPoderosaWorld _poderosaWorld;
    ///    public virtual void InitializePlugin(IPoderosaWorld poderosa)
    ///    {
    ///        poderosaWorld = poderosa;
    ///    }
    ///    public IPoderosaWorld PoderosaWorld
    ///    {
    ///        get
    ///        {
    ///            return _poderosaWorld;
    ///        }
    ///    }
    ///    public virtual void TerminatePlugin()
    ///    {
    ///    }
    ///    public virtual IAdaptable GetAdapter(Type adapter)
    ///    {
    ///        return _poderosaWorld.AdapterManager.GetAdapter(this, adapter);
    ///    }
    /// }
    /// </code>
    /// </ja>
    /// <en>
    /// The plug-in developer can write the plug-in by a little code by making it inheritances to from this class. 
    /// It is the following implementation. 
    /// <code>
    /// public abstract class PluginBase : MarshalByRefObject, IPlugin
    /// {
    ///    protected IPoderosaWorld _poderosaWorld;
    ///    public virtual void InitializePlugin(IPoderosaWorld poderosa)
    ///    {
    ///        poderosaWorld = poderosa;
    ///    }
    ///    public IPoderosaWorld PoderosaWorld
    ///    {
    ///        get
    ///        {
    ///            return _poderosaWorld;
    ///        }
    ///    }
    ///    public virtual void TerminatePlugin()
    ///    {
    ///    }
    ///    public virtual IAdaptable GetAdapter(Type adapter)
    ///    {
    ///        return _poderosaWorld.AdapterManager.GetAdapter(this, adapter);
    ///    }
    /// }
    /// </code>
    /// </en>
    /// </remarks>
    public abstract class PluginBase : MarshalByRefObject, IPlugin {
        /// <summary>
        /// <ja>
        /// 初期化のときに受け取ったIPoderosaWorldインターフェイスを保持します。
        /// </ja>
        /// <en>
        /// The IPoderosaWorld interface received when initializing it is maintained. 
        /// </en>
        /// </summary>
        protected IPoderosaWorld _poderosaWorld;
        /// <summary>
        /// <ja>
        /// プラグインの初期化の際に呼び出されます。デフォルトの実装では、_poderosaWorldに受け取ったIPoderosaWorldインターフェイスを保存します。
        /// </ja>
        /// <en>
        /// When the plug-in is initialized, it is called.
        /// In default implementation, the IPoderosaWorld interface received in _poderosaWorld is preserved. 
        /// </en>
        /// </summary>
        /// <param name="poderosa">
        /// <ja>Poderosa本体が渡されるIPoderosaWorldインターフェイス</ja>
        /// <en>IPoderosaWorld interface to which Poderosa is passed</en>
        /// </param>
        public virtual void InitializePlugin(IPoderosaWorld poderosa) {
            _poderosaWorld = poderosa;
        }

        /// <summary>
        /// <ja>
        /// Poderosa本体と通信するためのIPoderosaWorldインターフェイスを返します。
        /// </ja>
        /// <en>
        /// The IPoderosaWorld interface to communicate with Poderosa is returned. 
        /// </en>
        /// </summary>
        public IPoderosaWorld PoderosaWorld {
            get {
                return _poderosaWorld;
            }
        }

        /// <summary>
        /// <ja>
        /// プラグインが解放される前に呼び出されます。
        /// </ja>
        /// <en>
        /// It is called before the plug-in is released. 
        /// </en>
        /// </summary>
        public virtual void TerminatePlugin() {
        }

        public virtual IAdaptable GetAdapter(Type adapter) {
            return _poderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

}
