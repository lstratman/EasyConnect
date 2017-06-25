/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PreferencesEx.cs,v 1.3 2012/05/20 09:10:30 kzmi Exp $
 */
using System;
using System.IO;
using System.Collections;

/*
 * StructuredTextの上に、型情報、トランザクションなどの機能を載せてPreferenceとして使えるようにする
 */

namespace Poderosa.Preferences {

    // Exported Part
    /// <summary>
    /// <ja>
    /// ユーザー設定値の検証結果を示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows verification result of user setting value.
    /// </en>
    /// </summary>
    public interface IPreferenceValidationResult {
        /// <summary>
        /// <ja>
        /// 検証が成功したか否かを示します。
        /// </ja>
        /// <en>
        /// It is shown whether the verification succeeded. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// trueのとき成功、falseのとき失敗を意味します。
        /// </ja>
        /// <en>
        /// It succeeds at true, and the failure is meant at false. 
        /// </en>
        /// </remarks>
        bool Validated {
            get;
        }
        /// <summary>
        /// <ja>
        /// 検証時のエラーメッセージを示します。
        /// </ja>
        /// <en>
        /// The error message when verifying it is shown. 
        /// </en>
        /// </summary>
        string ErrorMessage {
            get;
            set;
        }
    }

    /// <summary>
    /// <ja>
    /// 検証が失敗したときの例外を示すクラスです。
    /// </ja>
    /// <en>
    /// Class that shows exception when verification fails
    /// </en>
    /// </summary>
    public class ValidationException : Exception {
        private IPreferenceItemBase _sourceItem;
        private IPreferenceValidationResult _result;

        /// <summary>
        /// <ja>
        /// 検証が失敗したときの例外を生成します。
        /// </ja>
        /// <en>
        /// The exception when the verification fails is generated. 
        /// </en>
        /// </summary>
        /// <param name="source"><ja>例外の原因となったソースです。</ja><en>Source that causes exception</en></param>
        /// <param name="result"><ja>検証結果です。</ja><en>Verification result</en></param>
        public ValidationException(IPreferenceItemBase source, IPreferenceValidationResult result)
            : base(result.ErrorMessage) {
            _sourceItem = source;
            _result = result;
        }

        /// <summary>
        /// <ja>
        /// 例外の原因となったソースを示します。
        /// </ja>
        /// <en>
        /// The source that causes the exception is shown. 
        /// </en>
        /// </summary>
        public IPreferenceItemBase SourceItem {
            get {
                return _sourceItem;
            }
        }

        /// <summary>
        /// <ja>
        /// 検証結果を示します。
        /// </ja>
        /// <en>
        /// The verification result is shown. 
        /// </en>
        /// </summary>
        public IPreferenceValidationResult Result {
            get {
                return _result;
            }
        }
    }

    //Preference Pluginが提供
    /// <summary>
    /// <ja>
    /// PreferencePluginプラグインが提供するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that PreferencePlugin plug-in offers.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインターフェイスは、<seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>の<see cref="Poderosa.Plugins.ICoreServices.Preferences">Preferencesプロパティ</see>
    /// から取得できます。
    /// </ja>
    /// <en>
    /// This interface can be get from the <see cref="Poderosa.Plugins.ICoreServices.Preferences">Preferences property</see> of <seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>. 
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// IPreferencesを得ます。
    /// <code>
    /// ICoreServices cs = PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // IPreferencesを取得します
    /// IPreferences pref = cs.Preferences;
    /// </code>
    /// </ja>
    /// <en>
    /// Get IPreferences.
    /// <code>
    /// ICoreServices cs = PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // Get IPreferences.
    /// IPreferences pref = cs.Preferences;
    /// </code>
    /// </en>
    /// </example>
    public interface IPreferences {
        /// <summary>
        /// <ja>
        /// フォルダを検索します。
        /// </ja>
        /// <en>
        /// Retrieve the folder.
        /// </en>
        /// </summary>
        /// <param name="id"><ja>検索するフォルダ名</ja><en>Retrieved folder name</en></param>
        /// <returns><ja>見つかったフォルダを示すIPreferenceFolder。見つからないときにはnull</ja><en>IPreferenceFolder that shows found folder. When not found, null returns.</en></returns>
        IPreferenceFolder FindPreferenceFolder(string id);
        /// <summary>
        /// <ja>すべてのフォルダを配列として得ます。</ja>
        /// <en>All folders are obtained as an array.</en>
        /// </summary>
        /// <returns><ja>保持しているすべてのフォルダ</ja><en>All held folders</en></returns>
        IPreferenceFolder[] GetAllFolders();
    }


    //設定項目を定義する側が提供
    /// <summary>
    /// <ja>
    /// ユーザー設定項目を定義するプラグインが実装すべきインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that plug-in that defines user setting item should implement.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// ユーザー設定項目を読み書きするプラグインは、このインターフェイスを実装したオブジェクトを用意し、
    /// PreferencePluginプラグインが提供する「org.poderosa.core.preferences」という拡張ポイントへと
    /// 登録します。
    /// </para>
    /// <para>
    /// 「org.poderosa.core.preferences」拡張ポイントは、<seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>の
    /// <see cref="Poderosa.Plugins.ICoreServices.PreferenceExtensionPoint">PreferenceExtensionPointプロパティ</see>から取得できます。
    /// </para>
    /// <code>
    /// ICoreServices cs = PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // PreferencesPluginプラグインの拡張ポイントを取得します
    /// IExtensionPoint prefext =cs.PreferenceExtensionPoint;
    /// 
    /// // 自身を登録します。
    /// prefext.RegisterExtension(this);
    /// </code>
    /// <para>
    /// 具体的な使い方については、<see href="/chap04_05.html">ユーザー設定値の操作</see>を参照してください。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// The object that implements this interface is prepared, and the plug-in to read and 
    /// write the user setting item is registered to the extension point of "org.poderosa.core.preferences" 
    /// that the PreferencePlugin plug-in offers. 
    /// </para>
    /// <para>
    /// "org.poderosa.core.preferences" The extension point can be got from the 
    /// <see cref="Poderosa.Plugins.ICoreServices.PreferenceExtensionPoint">PreferenceExtensionPoint property</see> 
    /// of <seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>. 
    /// </para>
    /// <code>
    /// ICoreServices cs = PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // Get the extension point of PreferencesPlugin plug-in.
    /// IExtensionPoint prefext =cs.PreferenceExtensionPoint;
    /// 
    /// // Regist this.
    /// prefext.RegisterExtension(this);
    /// </code>
    /// <para>
    /// Please refer to <see href="/chap04_05.html">Operation of user setting value</see> for a concrete usage. 
    /// </para>
    /// </en>
    /// </remarks>
    public interface IPreferenceSupplier {
        /// <summary>
        /// <ja>
        /// 設定値をプラグインごとに識別する項目名です。
        /// </ja>
        /// <en>
        /// Item name that identifies set value of each plug-in.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// この値は、options.confに書き込まれるときのルート直下の名前として採用されます。他のプラグインと重複しないようにするため、
        /// プラグインIDと同じものを設定することが推奨されます（あえて他のプラグインの設定値を読み書きしたい場合には、この限りではありません）。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// This value is adopted as a name right under the route when written in options.conf. 
        /// To make it not overlap with other plug-ins, the same one as plug-in ID is recommended to be set 
        /// (It is not this to dare to read and write a set value of other plug-ins). 
        /// </para>
        /// </en>
        /// </remarks>
        string PreferenceID {
            get;
        }
        /// <summary>
        /// <ja>
        /// 初期化時にPreferencesPluginプラグインから呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// Method that calls from PreferencesPlugin plug-in when initializing it
        /// </en>
        /// </summary>
        /// <param name="builder"><ja>設定項目を登録するためのインターフェイスです。</ja><en>Interface to register set item</en></param>
        /// <param name="folder"><ja>親となるフォルダです。</ja><en>Folder that becomes parents</en></param>
        /// <remarks>
        /// <ja>
        /// 開発者は、このメソッド内で<paramref name="bulder"/>の各メソッドを呼び出して、
        /// 設定値を登録します。
        /// </ja>
        /// <en>
        /// The developer calls each method of <paramref name="bulder"/> in this method, and registers a set value. 
        /// </en>
        /// </remarks>
        void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder);
        /// <summary>
        /// <ja>
        /// ユーザー設定値を特有のインターフェイスへと変換したいときに用います。
        /// </ja>
        /// <en>
        /// It uses it to convert the user setting value into a peculiar interface. 
        /// </en>
        /// </summary>
        /// <param name="folder"><ja>親となるフォルダです。</ja><en>Folder that becomes parents</en></param>
        /// <param name="type"><ja>変換しようとする型です。</ja><en>Type that tries to be converted</en></param>
        /// <returns><ja>変換後の型を返します。</ja><en>The type after it converts it is returned. </en></returns>
        /// <remarks>
        /// <ja>
        /// 型の変換機能を必要としないときには、単純にnullを返すように実装してください。
        /// </ja>
        /// <en>
        /// Please implement to return null simply when the conversion function of the type is not needed. 
        /// </en>
        /// </remarks>
        object QueryAdapter(IPreferenceFolder folder, Type type);

        /// <summary>
        /// <ja>
        /// 同じフォルダに含まれる複数のユーザー設定値を検査したいときに用います。
        /// </ja>
        /// <en>
        /// It uses it to inspect two or more user setting values included in the same folder. 
        /// </en>
        /// </summary>
        /// <param name="folder"><ja>検証の対象となるフォルダが渡されます。</ja><en>Target folder for verification</en></param>
        /// <param name="output"><ja>検証結果を設定します。</ja><en>The verification result is set. </en></param>
        /// <remarks>
        /// <ja>
        /// 複数のユーザー設定値を検査する機能を必要としないときには、この処理は空でかまいません。
        /// </ja>
        /// <en>
        /// This processing is not cared about in the sky when the function to inspect two or more user setting values is not needed. 
        /// </en>
        /// </remarks>
        void ValidateFolder(IPreferenceFolder folder, IPreferenceValidationResult output);
    }

    //変更通知
    /// <summary>
    /// <ja>
    /// フォルダ内のユーザー設定値が変化したときに通知を受け取るインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that receives notification when user setting value in folder changes.
    /// </en>
    /// </summary>
    public interface IPreferenceChangeListener {
        /// <summary>
        /// <ja>
        /// ユーザー設定値が変化したときに呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// Method of call when user setting value changes.
        /// </en>
        /// </summary>
        /// <param name="oldvalues"><ja>設定前の古い値です。</ja><en>Old value before it sets it</en></param>
        /// <param name="newvalues"><ja>設定後の新しい値です。</ja><en>New value after it sets it</en></param>
        void OnPreferenceImport(IPreferenceFolder oldvalues, IPreferenceFolder newvalues);
    }

    //初期化時のみ有効
    /// <summary>
    /// <ja>
    /// ユーザー設定値（Preference）として項目を登録する機能を提供します。
    /// </ja>
    /// <en>
    /// The function to register the item as user setting value (Preference) is offered. 
    /// </en>
    /// </summary>
    public interface IPreferenceBuilder {
        /// <summary>
        /// <ja>
        /// 階層化するフォルダを定義します。
        /// </ja>
        /// <en>
        /// The hierarchized folder is defined. 
        /// </en>
        /// </summary>
        /// <param name="parent"><ja>親となるフォルダ</ja><en>Folder that becomes parents</en></param>
        /// <param name="supplier"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// <ja>
        /// <paramref name="parent"/>には、<seealso cref="IPreferenceSupplier">IPreferenceSupplier</seealso>の
        /// <see href="IPreferenceSupplier.InitializePreference">InitializePreference</see>の第2引数に渡された
        /// 値をそのまま引き渡すのが通例です。
        /// </ja>
        /// <en>
        /// In <paramref name="parent"/>, it is usual to pass the value passed to the second argument of 
        /// <see href="IPreferenceSupplier.InitializePreference">InitializePreference</see> of 
        /// <seealso cref="IPreferenceSupplier">IPreferenceSupplier</seealso> in off as it is. 
        /// </en>
        /// </remarks>
        /// <exclude/>
        IPreferenceFolder DefineFolder(IPreferenceFolder parent, IPreferenceSupplier supplier, string id); //子のSupplierはnull
        /// <exclude/>
        IPreferenceFolder DefineFolderArray(IPreferenceFolder parent, IPreferenceSupplier supplier, string id); //Arrayを返すわけではないことに注意
        /// <exclude/>
        IPreferenceLooseNode DefineLooseNode(IPreferenceFolder parent, IPreferenceLooseNodeContent content, string id);

        //validator不要なときはnull
        /// <summary>
        /// <ja>
        /// bool型のユーザー設定値を定義します。
        /// </ja>
        /// <en>
        /// The user setting value of the bool type is defined. 
        /// </en>
        /// </summary>
        /// <param name="parent"><ja>親となるフォルダ</ja><en>Folder that becomes parents</en></param>
        /// <param name="id"><ja>値のキーとなる設定名</ja><en>Set name that becomes key to value.</en></param>
        /// <param name="initial_value"><ja>初期値</ja><en>Initial value</en></param>
        /// <param name="validator"><ja>値を検証する際のバリデータ</ja><en>Validator when value is verified</en></param>
        /// <returns><ja>値を読み書きするための<seealso cref="IBoolPreferenceItem">IBoolPreferenceItem</seealso></ja><en><seealso cref="IBoolPreferenceItem">IBoolPreferenceItem</seealso> to read and write value.</en></returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// <paramref name="parent"/>には、<seealso cref="IPreferenceSupplier">IPreferenceSupplier</seealso>の
        /// <see href="IPreferenceSupplier.InitializePreference">InitializePreference</see>の第2引数に渡された
        /// 値をそのまま引き渡すのが通例です。
        /// </para>
        /// <para>
        /// <paramref name="id"/>は、ユーザー設定値を識別するための任意の名前です。<paramref name="parent"/>
        /// で階層化されるため、ほかのプラグインとの名前の重複を考える必要はありません。
        /// </para>
        /// <para>
        /// <paramref name="initial_value"/>は、該当するユーザー設定値がまだ存在しない場合の初期値です。
        /// すでにこのユーザー設定値が存在する場合には無視され、既存の値が読み込まれます。
        /// </para>
        /// <para>
        /// 検証機能を必要としないときには、<paramref name="validator"/>をnullにすることもできます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// In <paramref name="parent"/>, it is usual to pass the value passed to the second argument of 
        /// <see href="IPreferenceSupplier.InitializePreference">InitializePreference</see> of 
        /// <seealso cref="IPreferenceSupplier">IPreferenceSupplier</seealso> in off as it is. 
        /// </para>
        /// <para>
        /// <paramref name="id"/> is an arbitrary name to identify the user setting value. 
        /// It is not necessary to think about the repetition of the name with other plug-ins 
        /// because it is hierarchized by <paramref name="parent"/>. 
        /// </para>
        /// <para>
        /// <paramref name="initial_value"/> is a value in the early the case where the corresponding user 
        /// setting value has not existed yet. It is disregarded when this user setting value already exists, 
        /// and an existing value is read. 
        /// </para>
        /// <para>
        /// When the verification function is not needed, <paramref name="validator"/> can be made null. 
        /// </para>
        /// </en>
        /// </remarks>
        IBoolPreferenceItem DefineBoolValue(IPreferenceFolder parent, string id, bool initial_value, PreferenceItemValidator<bool> validator);
        /// <summary>
        /// <ja>
        /// int型のユーザー設定値を定義します。
        /// </ja>
        /// <en>
        /// The user setting value of the int type is defined. 
        /// </en>
        /// </summary>
        /// <param name="parent"><ja>親となるフォルダ</ja><en>Folder that becomes parents</en></param>
        /// <param name="id"><ja>値のキーとなる設定名</ja><en>Set name that becomes key to value.</en></param>
        /// <param name="initial_value"><ja>初期値</ja><en>Initial value</en></param>
        /// <param name="validator"><ja>値を検証する際のバリデータ</ja><en>Validator when value is verified</en></param>
        /// <returns><ja>値を読み書きするための<seealso cref="IIntPreferenceItem">IIntPreferenceItem</seealso></ja><en><seealso cref="IIntPreferenceItem">IIntPreferenceItem</seealso> to read and write value.</en></returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// <paramref name="parent"/>には、<seealso cref="IPreferenceSupplier">IPreferenceSupplier</seealso>の
        /// <see href="IPreferenceSupplier.InitializePreference">InitializePreference</see>の第2引数に渡された
        /// 値をそのまま引き渡すのが通例です。
        /// </para>
        /// <para>
        /// <paramref name="id"/>は、ユーザー設定値を識別するための任意の名前です。<paramref name="parent"/>
        /// で階層化されるため、ほかのプラグインとの名前の重複を考える必要はありません。
        /// </para>
        /// <para>
        /// <paramref name="initial_value"/>は、該当するユーザー設定値がまだ存在しない場合の初期値です。
        /// すでにこのユーザー設定値が存在する場合には無視され、既存の値が読み込まれます。
        /// </para>
        /// <para>
        /// 検証機能を必要としないときには、<paramref name="validator"/>をnullにすることもできます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// In <paramref name="parent"/>, it is usual to pass the value passed to the second argument of 
        /// <see href="IPreferenceSupplier.InitializePreference">InitializePreference</see> of 
        /// <seealso cref="IPreferenceSupplier">IPreferenceSupplier</seealso> in off as it is. 
        /// </para>
        /// <para>
        /// <paramref name="id"/> is an arbitrary name to identify the user setting value. 
        /// It is not necessary to think about the repetition of the name with other plug-ins 
        /// because it is hierarchized by <paramref name="parent"/>. 
        /// </para>
        /// <para>
        /// <paramref name="initial_value"/> is a value in the early the case where the corresponding user 
        /// setting value has not existed yet. It is disregarded when this user setting value already exists, 
        /// and an existing value is read. 
        /// </para>
        /// <para>
        /// When the verification function is not needed, <paramref name="validator"/> can be made null. 
        /// </para>
        /// </en>
        /// </remarks>
        IIntPreferenceItem DefineIntValue(IPreferenceFolder parent, string id, int initial_value, PreferenceItemValidator<int> validator);

        /// <summary>
        /// <ja>
        /// string型のユーザー設定値を定義します。
        /// </ja>
        /// <en>
        /// The user setting value of the string type is defined. 
        /// </en>
        /// </summary>
        /// <param name="parent"><ja>親となるフォルダ</ja><en>Folder that becomes parents</en></param>
        /// <param name="id"><ja>値のキーとなる設定名</ja><en>Set name that becomes key to value.</en></param>
        /// <param name="initial_value"><ja>初期値</ja><en>Initial value</en></param>
        /// <param name="validator"><ja>値を検証する際のバリデータ</ja><en>Validator when value is verified</en></param>
        /// <returns><ja>値を読み書きするための<seealso cref="IIntPreferenceItem">IIntPreferenceItem</seealso></ja><en><seealso cref="IIntPreferenceItem">IIntPreferenceItem</seealso> to read and write value.</en></returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// <paramref name="parent"/>には、<seealso cref="IPreferenceSupplier">IPreferenceSupplier</seealso>の
        /// <see href="IPreferenceSupplier.InitializePreference">InitializePreference</see>の第2引数に渡された
        /// 値をそのまま引き渡すのが通例です。
        /// </para>
        /// <para>
        /// <paramref name="id"/>は、ユーザー設定値を識別するための任意の名前です。<paramref name="parent"/>
        /// で階層化されるため、ほかのプラグインとの名前の重複を考える必要はありません。
        /// </para>
        /// <para>
        /// <paramref name="initial_value"/>は、該当するユーザー設定値がまだ存在しない場合の初期値です。
        /// すでにこのユーザー設定値が存在する場合には無視され、既存の値が読み込まれます。
        /// </para>
        /// <para>
        /// 検証機能を必要としないときには、<paramref name="validator"/>をnullにすることもできます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// In <paramref name="parent"/>, it is usual to pass the value passed to the second argument of 
        /// <see href="IPreferenceSupplier.InitializePreference">InitializePreference</see> of 
        /// <seealso cref="IPreferenceSupplier">IPreferenceSupplier</seealso> in off as it is. 
        /// </para>
        /// <para>
        /// <paramref name="id"/> is an arbitrary name to identify the user setting value. 
        /// It is not necessary to think about the repetition of the name with other plug-ins 
        /// because it is hierarchized by <paramref name="parent"/>. 
        /// </para>
        /// <para>
        /// <paramref name="initial_value"/> is a value in the early the case where the corresponding user 
        /// setting value has not existed yet. It is disregarded when this user setting value already exists, 
        /// and an existing value is read. 
        /// </para>
        /// <para>
        /// When the verification function is not needed, <paramref name="validator"/> can be made null. 
        /// </para>
        /// </en>
        /// </remarks>
        IStringPreferenceItem DefineStringValue(IPreferenceFolder parent, string id, string initial_value, PreferenceItemValidator<string> validator);
    }


    /// <summary>
    /// <ja>
    /// ユーザー設定値（Preference）の項目やフォルダの基底となるインターフェイスです。
    /// </ja>
    /// <en>
    /// It is an interface that becomes the item of user setting value (Preference) and basic of the folder. 
    /// </en>
    /// </summary>
    public interface IPreferenceItemBase {
        /// <summary>
        /// <ja>
        /// 設定名です。
        /// </ja>
        /// <en>
        /// Name of setting
        /// </en>
        /// </summary>
        string Id {
            get;
        }
        /// <summary>
        /// <ja>
        /// フォルダ名も含めた完全な設定名です。
        /// </ja>
        /// <en>
        /// Complete set name including folder name
        /// </en>
        /// </summary>
        string FullQualifiedId {
            get;
        }
        /// <summary>
        /// <ja>
        /// 親からのインデックス位置です。
        /// </ja>
        /// <en>
        /// It is an index position from parents. 
        /// </en>
        /// </summary>
        int Index {
            get;
        }
        //cast: NOTE:廃止予定
        /// <exclude/>
        IPreferenceItem AsItem();
        /// <exclude/>
        IPreferenceFolder AsFolder();
        /// <exclude/>
        IPreferenceFolderArray AsFolderArray();
        //すべて初期化
        /// <summary>
        /// <ja>
        /// 値をすべて初期化します。
        /// </ja>
        /// <en>
        /// All the values are initialized. 
        /// </en>
        /// </summary>
        void ResetValue();
    }

    /// <summary>
    /// <ja>
    /// ユーザー設定値（Preference）を階層化するフォルダを操作するインターフェイスです。
    /// </ja>
    /// <en>
    /// It is an interface that operates the folder that hierarchizes user setting value (Preference). 
    /// </en>
    /// </summary>
    public interface IPreferenceFolder : IPreferenceItemBase {
        /// <summary>
        /// <ja>
        /// 複製を作成します。
        /// </ja>
        /// <en>
        /// Create a copy.
        /// </en>
        /// </summary>
        /// <returns><ja>複製したIPreferenceFolder</ja><en>Duplicated IPreferenceFolder</en></returns>
        IPreferenceFolder Clone();
        /// <summary>
        /// <ja>
        /// 別のフォルダからインポートします。
        /// </ja>
        /// <en>
        /// Import from another folder.
        /// </en>
        /// </summary>
        /// <param name="newvalues"><ja>インポートする値を含むフォルダ</ja><en>Folder including value in which import.</en></param>
        void Import(IPreferenceFolder newvalues);

        //TODO 外部からの明示的folder validationをここへ

        /// <summary>
        /// <ja>
        /// 子のフォルダを検索します。
        /// </ja>
        /// <en>
        /// Child's folder is retrieved. 
        /// </en>
        /// </summary>
        /// <param name="id"><ja>子のフォルダのID</ja><en>ID of child's folder.</en></param>
        /// <returns><ja>見つかったフォルダを示すIPreferenceFolder。見つからないときにはnull</ja><en>IPreferenceFolder that shows found folder. When not found, null returns.</en></returns>
        IPreferenceFolder FindChildFolder(string id);
        /// <summary>
        /// <ja>
        /// 子のフォルダの配列を検索します。
        /// </ja>
        /// <en>
        /// Array of child's folder is retrieved. 
        /// </en>
        /// </summary>
        /// <param name="id">
        /// <ja>子のフォルダのID</ja>
        /// <en>ID of child's folder.</en>
        /// </param>
        /// <returns><ja>見つかったフォルダを示すIPreferenceFolder。見つからないときにはnull</ja><en>IPreferenceFolder that shows found folder. When not found, null returns.</en></returns>
        IPreferenceFolderArray FindChildFolderArray(string id);
        /// <summary>
        /// <ja>
        /// このフォルダ内の設定値を検索します。
        /// </ja>
        /// <en>
        /// A set value in this folder is retrieved. 
        /// </en>
        /// </summary>
        /// <param name="id"><ja>検索する設定名</ja><en>Retrieved set name</en></param>
        /// <returns><ja>見つかったフォルダを示すIPreferenceFolder。見つからないときにはnull</ja><en>IPreferenceFolder that shows found folder. When not found, null returns.</en></returns>
        IPreferenceItem FindItem(string id);
        /// <summary>
        /// <ja>
        /// 子の数を示します。
        /// </ja>
        /// <en>
        /// Number of children
        /// </en>
        /// </summary>
        int ChildCount {
            get;
        }
        /// <summary>
        /// <ja>
        /// 指定したインデックス位置にある設定項目を返します。
        /// </ja>
        /// <en>
        /// A set item at the specified index position is returned. 
        /// </en>
        /// </summary>
        /// <param name="index"><ja>インデックス位置</ja><en>Position of index.</en></param>
        /// <returns><ja>そのインデックス位置にある設定項目を示すIPreferenceItemBase。見つからないときにはnull</ja><en>IPreferenceItemBase that shows set item at the index position. When not found, returns null.</en></returns>
        IPreferenceItemBase ChildAt(int index);

        //UserFriendly interfaceへのキャスト用
        /// <summary>
        /// <ja>
        /// それぞれのプラグインに特有のPreferenceへと変換します。
        /// </ja>
        /// <en>
        /// It converts it into peculiar Preference to each plug-in. 
        /// </en>
        /// </summary>
        /// <param name="type"><ja>変換するインターフェイスの型</ja><en>Type in converted interface</en></param>
        /// <returns><ja>変換後のインターフェイス。変換できないときにはnull</ja><en>Interface after it converts it. When it is not possible to convert it, returns null. </en></returns>
        object QueryAdapter(Type type);

        //先祖へも伝播
        /// <summary>
        /// <ja>
        /// このフォルダ内の設定値が変化したときに通知するオブジェクトを登録します。
        /// </ja>
        /// <en>
        /// The object notified when a set value in this folder changes is registered. 
        /// </en>
        /// </summary>
        /// <param name="listener"><ja>通知先のオブジェクト</ja><en>Object at notification destination</en></param>
        void AddChangeListener(IPreferenceChangeListener listener);
        /// <summary>
        /// <ja>
        /// <seealso cref="AddChangeListener">AddChangeListener</seealso>で登録したオブジェクトを解除します。
        /// </ja>
        /// <en>
        /// The object registered with AddChangeListener is released. 
        /// </en>
        /// </summary>
        /// <param name="listener"><ja>解除するオブジェクト</ja><en>Object to release.</en></param>
        void RemoveChangeListener(IPreferenceChangeListener listener);
    }

    /// <summary>
    /// <ja>
    /// ユーザー設定値（Preference）のフォルダをまとめて扱うためのインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to handle the folder of the user setting value collectively. 
    /// </en>
    /// </summary>
    public interface IPreferenceFolderArray : IPreferenceItemBase {
        /// <summary>
        /// <ja>
        /// 複製を作成します。
        /// </ja>
        /// <en>
        /// Create a copy.
        /// </en>
        /// </summary>
        /// <returns><ja>複製したIPreferenceFolderArray</ja><en>Duplicated IPreferenceFolderArray</en></returns>
        IPreferenceFolderArray Clone();
        /// <summary>
        /// <ja>
        /// 別のフォルダからインポートします。
        /// </ja>
        /// <en>
        /// Import from another folder.
        /// </en>
        /// </summary>
        /// <param name="newvalues"><ja>インポートする値を含むフォルダ</ja><en>Folder including value in which import is done</en></param>
        void Import(IPreferenceFolderArray newvalues);

        /// <summary>
        /// <ja>
        /// 保持している内容をIPreferenceFolderの配列として得ます。
        /// </ja>
        /// <en>
        /// The held content is obtained as an array of IPreferenceFolder. 
        /// </en>
        /// </summary>
        IPreferenceFolder[] Folders {
            get;
        }

        /// <summary>
        /// <ja>
        /// 保持している内容をクリアします。
        /// </ja>
        /// <en>
        /// Clear the held content.
        /// </en>
        /// </summary>
        void Clear();

        /// <summary>
        /// <ja>
        /// 新しいフォルダを作成します。
        /// </ja>
        /// <en>
        /// Create a new folder.
        /// </en>
        /// </summary>
        /// <returns><ja>作られた新しいフォルダを示すIPreferenceFolder</ja>
        /// <en>IPreferenceFolder that shows made new folder</en>
        /// </returns>
        IPreferenceFolder CreateNewFolder();

        /// <summary>
        /// <ja>
        /// テンプレートを用いて、子のフォルダをアイテムへと変換します。
        /// </ja>
        /// <en>
        /// Child's folder is converted into the item with a template. 
        /// </en>
        /// </summary>
        /// <param name="child_folder"><ja>変換する子フォルダ</ja><en>Converted child folder</en></param>
        /// <param name="item_in_template"><ja>用いるテンプレート</ja><en>Used template.</en></param>
        /// <returns><ja>テンプレートによって変換された項目を示すIPreferenceItem</ja><en>IPreferenceItem that shows item converted with template</en></returns>
        IPreferenceItem ConvertItem(IPreferenceFolder child_folder, IPreferenceItem item_in_template);
    }

    /// <summary>
    /// <ja>
    /// ユーザー設定値を検証するためのデリゲートです。
    /// </ja>
    /// <en>
    /// It is delegate to verify the user setting value. 
    /// </en>
    /// </summary>
    /// <typeparam name="T"><ja>ユーザー設定値の型情報です。</ja><en>It is type information on the user setting value. </en></typeparam>
    /// <param name="value"><ja>検証すべき値です。</ja><en>It is a value that should be verified. </en></param>
    /// <param name="result"><ja>検証結果を格納します。</ja><en>The verification result is stored. </en></param>
    /// <remarks>
    /// <ja>
    /// ユーザー設定値を検証する機能を提供するプラグインでは、<paramref name="value"/>の値の妥当性を検証し、
    /// その結果を<paramref name="result"/>に設定してください。
    /// </ja>
    /// <en>Please verify the validity of the value of <paramref name="value"/>, and set the result to <paramref name="result"/> in the plug-in that offers the function to verify the user setting value. </en>
    /// </remarks>
    public delegate void PreferenceItemValidator<T>(T value, IPreferenceValidationResult result);

    /// <summary>
    /// <ja>
    /// ユーザー設定値（Preference）の項目を示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows item of user setting value (Preference).
    /// </en>
    /// </summary>
    public interface IPreferenceItem : IPreferenceItemBase {
        /// <summary>
        /// <ja>
        /// bool型として値を得るためのインターフェイスを取得します。
        /// </ja>
        /// <en>
        /// The interface to obtain the value as bool type is got. 
        /// </en>
        /// </summary>
        /// <returns><ja>bool値としてアクセスするためのIBoolPreferenceItem</ja><en>IBoolPreferenceItem to access it as bool value</en></returns>
        IBoolPreferenceItem AsBool();
        /// <summary>
        /// <ja>
        /// int型として値を得るためのインターフェイスを取得します。
        /// </ja>
        /// <en>
        /// The interface to obtain the value as int type is got. 
        /// </en>
        /// </summary>
        /// <returns><ja>int値としてアクセスするためのIIntPreferenceItem</ja><en>IIntPreferenceItem to access it as int value</en></returns>
        IIntPreferenceItem AsInt();
        /// <summary>
        /// <ja>
        /// string型として値を得るためのインターフェイスを取得します。
        /// </ja>
        /// <en>
        /// The interface to obtain the value as string type is got. 
        /// </en>
        /// </summary>
        /// <returns><ja>string値としてアクセスするためのIStringPreferenceItem</ja><en>IStringPreferenceItem to access it as string value</en></returns>
        IStringPreferenceItem AsString();

    }

    /// <summary>
    /// <ja>
    /// IPoderosaItemを型付けしたインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that puts the type as for IPoderosaItem. 
    /// </en>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITypedPreferenceItem<T> : IPreferenceItem {
        /// <summary>
        /// <ja>
        /// ユーザー設定値を読み書きします。
        /// </ja>
        /// <en>
        /// Read and write the user setting value.
        /// </en>
        /// </summary>
        T Value {
            get;
            set;
        }

        /// <summary>
        /// <ja>
        /// ユーザー設定値の初期値を示します。
        /// </ja>
        /// <en>
        /// Show the initial value of the user setting value.
        /// </en>
        /// </summary>
        T InitialValue {
            get;
        }

        /// <summary>
        /// <ja>
        /// ユーザー設定値を検証するためのPreferenceItemValidatorを取得／設定します。
        /// </ja>
        /// <en>
        /// Get / set the PreferenceItemValidator to verify the user setting value.
        /// </en>
        /// </summary>
        PreferenceItemValidator<T> Validator {
            get;
            set;
        }
    }

    //Generic Parameterをプログラマに毎回指定させるのもいやらしいし、IPreferenceItem等のキャスト用メソッドを気軽に使いたいので
    /// <summary>
    /// <ja>
    /// bool型のユーザー設定値を読み書きする機能を提供します。
    /// </ja>
    /// <en>
    /// Offered the function to read and write the user setting value of the bool type.
    /// </en>
    /// </summary>
    public interface IBoolPreferenceItem : ITypedPreferenceItem<bool> {
    }

    /// <summary>
    /// <ja>
    /// int型のユーザー設定値を読み書きする機能を提供します。
    /// </ja>
    /// <en>
    /// Offered the function to read and write the user setting value of the int type.
    /// </en>
    /// </summary>
    public interface IIntPreferenceItem : ITypedPreferenceItem<int> {
    }

    /// <summary>
    /// <ja>
    /// string型のユーザー設定値を読み書きする機能を提供します。
    /// </ja>
    /// <en>
    /// Offered the function to read and write the user setting value of the string type.
    /// </en>
    /// </summary>
    public interface IStringPreferenceItem : ITypedPreferenceItem<string> {
    }

    //Loose Node

    /// <summary>
    /// </summary>
    /// <exclude/>
    public interface IPreferenceLooseNode : IPreferenceItemBase {
        IPreferenceLooseNodeContent Content {
            get;
        }
    }

    //TODO FolderはクローンできるのにContentはできないのは変だ。対照的でない
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IPreferenceLooseNodeContent {
        IPreferenceLooseNodeContent Clone();
        void Reset();
        void LoadFrom(StructuredText node);
        void SaveTo(StructuredText node);
    }

    //End Exported Part
}
