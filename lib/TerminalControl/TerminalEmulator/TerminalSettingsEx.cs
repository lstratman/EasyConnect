/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSettingsEx.cs,v 1.3 2012/03/18 02:52:24 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Poderosa.ConnectionParam;
using Poderosa.View;
using Poderosa.Terminal;
using Poderosa.Util;

namespace Poderosa.Terminal {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITerminalSettingsChangeListener {
        void OnBeginUpdate(ITerminalSettings current);
        void OnEndUpdate(ITerminalSettings current);
    }

    /// <summary>
    /// <ja>
    /// 各種ログ設定のインターフェイスの基底です。
    /// </ja>
    /// <en>
    /// Base class of interface of log setting.
    /// </en>
    /// </summary>
    public interface ILogSettings : IAdaptable {
        /// <summary>
        /// <ja>ログ設定を複製します。</ja><en>Duplicate the log setting.</en>
        /// </summary>
        /// <returns><en>Interface that shows object after it duplidates</en><ja>複製後のオブジェクトを示すインターフェイス</ja></returns>
        ILogSettings Clone();
    }
    //ログ設定　Terminalの設定上は複数ストリームに出力できるようになっているが、TerminalSetting上はファイルへの１種のみ
    /// <summary>
    /// <ja>
    /// 簡易なログ設定を示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows simple log setting
    /// </en>
    /// </summary>
    public interface ISimpleLogSettings : ILogSettings {
        /// <summary>
        /// <ja>
        /// ログの種類を示します。
        /// </ja>
        /// <en>
        /// Type of log.
        /// </en>
        /// </summary>
        LogType LogType {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// ログのパスを示します。
        /// </ja>
        /// <en>
        /// Path of the log.
        /// </en>
        /// </summary>
        string LogPath {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// ログを追記するかしないかを示します。trueのとき追記します。
        /// </ja>
        /// <en>
        /// Whether the log is append is shown. At true, append
        /// </en>
        /// </summary>
        bool LogAppend {
            get;
            set;
        }
    }

    //複数出力
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IMultiLogSettings : ILogSettings, IEnumerable<ILogSettings> {
        /// <summary>
        /// Clears all settings, then adds one setting.
        /// </summary>
        /// <param name="log">logging setting</param>
        void Reset(ILogSettings log);

        /// <summary>
        /// Adds setting to the list.
        /// </summary>
        /// <param name="log">logging setting</param>
        void Add(ILogSettings log);

        /// <summary>
        /// Remove setting from the list.
        /// </summary>
        /// <param name="log">logging setting</param>
        void Remove(ILogSettings log);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="newvalue"></param>
    /// <exclude/>
    public delegate void ChangeHandler<T>(T newvalue);

    /// <summary>
    /// <ja>
    /// ターミナル設定を操作するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that control terminal setting.
    /// </en>
    /// </summary>
    public interface ITerminalSettings : IListenerRegistration<ITerminalSettingsChangeListener>, IAdaptable {
        /// <summary>
        /// <ja>
        /// ターミナル設定の複製を作ります。
        /// </ja>
        /// <en>
        /// Duplicate terminal setting.
        /// </en>
        /// </summary>
        /// <returns><ja>複製されたターミナル設定オブジェクトを示すインターフェイスです。</ja><en>Interface that shows duplicated terminal setting object</en></returns>
        ITerminalSettings Clone();
        /// <summary>
        /// <ja>
        /// ターミナル設定をインポートします。
        /// </ja>
        /// <en>
        /// Import the terminal setting.
        /// </en>
        /// </summary>
        /// <param name="src"><ja>インポートするターミナル設定。</ja><en>Terminal setting to import.</en></param>
        void Import(ITerminalSettings src);

        //変更するときはStartUpdate...EndUpdateを行う。EndUpdateの時点でリスナに通知
        /// <summary>
        /// <ja>
        /// プロパティの変更を開始します。
        /// </ja>
        /// <en>
        /// Start changing the property.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// プロパティを変更する前には、<see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出し、プロパティの変更が終わったら、
        /// <see cref="EndUpdate">EndUpdateメソッド</see>を呼び出さなければなりません。
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出す前にプロパティを変更しようとすると例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        void BeginUpdate();
        /// <summary>
        /// <ja>
        /// プロパティの変更を完了します。
        /// </ja>
        /// <en>
        /// Finish changing the property.
        /// </en>
        /// <remarks>
        /// <ja>このメソッドを呼び出すとプロパティの変更が完了したものとされ、各種イベントが発生します。</ja><en>It is assumed that the change in property was completed when this method is called, and generates various events. </en>
        /// </remarks>
        /// </summary>
        void EndUpdate();
        /// <summary>
        /// <ja>
        /// エンコード方式を取得／設定します。
        /// </ja>
        /// <en>
        /// Set / get the encode type.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// プロパティを変更する前には、<see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出し、プロパティの変更が終わったら、
        /// <see cref="EndUpdate">EndUpdateメソッド</see>を呼び出さなければなりません。
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出す前にプロパティを変更しようとすると例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        EncodingType Encoding {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// ターミナルの種類を取得／設定します。
        /// </ja>
        /// <en>
        /// Set / get the type of termina.
        /// </en>
        /// </summary>
        TerminalType TerminalType {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// 改行コードの取り扱い方法を取得／設定します。
        /// </ja>
        /// <en>
        /// Set / get the rule of line feed code.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// プロパティを変更する前には、<see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出し、プロパティの変更が終わったら、
        /// <see cref="EndUpdate">EndUpdateメソッド</see>を呼び出さなければなりません。
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出す前にプロパティを変更しようとすると例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        LineFeedRule LineFeedRule {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// 送信時の改行コードの種類を取得／設定します。
        /// </ja>
        /// <en>
        /// Set / get the line feed code when transmitting.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// プロパティを変更する前には、<see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出し、プロパティの変更が終わったら、
        /// <see cref="EndUpdate">EndUpdateメソッド</see>を呼び出さなければなりません。
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出す前にプロパティを変更しようとすると例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        NewLine TransmitNL {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// ローカルエコーの有無を取得／設定します。
        /// </ja>
        /// <en>
        /// Set / get the local echo.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// プロパティを変更する前には、<see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出し、プロパティの変更が終わったら、
        /// <see cref="EndUpdate">EndUpdateメソッド</see>を呼び出さなければなりません。
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出す前にプロパティを変更しようとすると例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        bool LocalEcho {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// プロンプトの認識やコマンドの履歴を記憶する機能をもつオブジェクトを示します。
        /// </ja>
        /// <en>
        /// Object that memorizes recognition of prompt and history of command.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// プロパティを変更する前には、<see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出し、プロパティの変更が終わったら、
        /// <see cref="EndUpdate">EndUpdateメソッド</see>を呼び出さなければなりません。
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出す前にプロパティを変更しようとすると例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        IShellScheme ShellScheme {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// 通常の文字入力に伴うインテリセンスを有効にするか否かの設定です。
        /// </ja>
        /// <en>
        /// Setting whether to make IntelliSense according to usual character input effective.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// プロパティを変更する前には、<see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出し、プロパティの変更が終わったら、
        /// <see cref="EndUpdate">EndUpdateメソッド</see>を呼び出さなければなりません。
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出す前にプロパティを変更しようとすると例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        bool EnabledCharTriggerIntelliSense {
            get;
            set;
        }

        /// <summary>
        /// <ja>
        /// ドキュメントバーに表示するキャプションです。
        /// </ja>
        /// <en>
        /// Caption displayed in document bar.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// プロパティを変更する前には、<see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出し、プロパティの変更が終わったら、
        /// <see cref="EndUpdate">EndUpdateメソッド</see>を呼び出さなければなりません。
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出す前にプロパティを変更しようとすると例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        string Caption {
            get;
            set;
        }

        /// <summary>
        /// <ja>
        /// ドキュメントバーに表示するアイコンです。
        /// </ja>
        /// <en>
        /// Icon displayed in document bar
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// プロパティを変更する前には、<see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出し、プロパティの変更が終わったら、
        /// <see cref="EndUpdate">EndUpdateメソッド</see>を呼び出さなければなりません。
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出す前にプロパティを変更しようとすると例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        Image Icon {
            get;
            set;
        }

        /// <summary>
        /// <ja>
        /// コンソールの表示方法を指定するRenderProfileオブジェクトです。フォント、背景色などの情報が含まれます。
        /// </ja>
        /// <en>
        /// It is RenderProfile object that specifies the method of displaying the console. Information of the font and the background color, etc. is included. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// プロパティを変更する前には、<see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出し、プロパティの変更が終わったら、
        /// <see cref="EndUpdate">EndUpdateメソッド</see>を呼び出さなければなりません。
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateメソッド</see>を呼び出す前にプロパティを変更しようとすると例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        RenderProfile RenderProfile {
            get;
            set;
        }

        /// <summary>
        /// <ja>
        /// デフォルトの表示プロファイルを用いているかどうかを取得します。
        /// </ja>
        /// <en>
        /// Get whether to use the display profile of default. 
        /// </en>
        /// </summary>
        bool UsingDefaultRenderProfile {
            get;
        }

        /// <summary>
        /// <ja>
        /// ログの設定情報です。
        /// </ja>
        /// <en>
        /// Setting of log.
        /// </en>
        /// </summary>
        IMultiLogSettings LogSettings {
            get;
        }

        //TODO これらはITerminalSettingsChangeListenerに統合せよ
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        event ChangeHandler<string> ChangeCaption;
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        event ChangeHandler<RenderProfile> ChangeRenderProfile;
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        event ChangeHandler<EncodingType> ChangeEncoding;
    }

}
