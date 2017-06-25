/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalParameterEx.cs,v 1.5 2012/03/14 16:33:38 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Poderosa.Util;
using Granados;
using Granados.AgentForwarding;
using Granados.X11Forwarding;

namespace Poderosa.Protocols {
    /// <summary>
    /// <ja>
    /// ターミナル接続のためのパラメータを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows parameter for terminal connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <seealso cref="ITCPParameter">ITCPParameter</seealso>、
    /// <seealso cref="ISSHLoginParameter">ISSHLoginParameter</seealso>
    /// <seealso cref="ICygwinParameter">ICygwinParameter</seealso>は、GetAdapterメソッドを使って
    /// このITerminalParameterへと変換できます。
    /// </ja>
    /// <en>
    /// <seealso cref="ITCPParameter">ITCPParameter</seealso>,
    /// <seealso cref="ISSHLoginParameter">ISSHLoginParameter</seealso>,
    /// <seealso cref="ICygwinParameter">ICygwinParameter</seealso> can be converted to ITerminalParameter by GetAdapter method.
    /// </en>
    /// </remarks>
    public interface ITerminalParameter : IAdaptable, ICloneable {
        /// <summary>
        /// <ja>
        /// 内部幅です。
        /// </ja>
        /// <en>
        /// Internal width.
        /// </en>
        /// </summary>
        int InitialWidth {
            get;
        }
        /// <summary>
        /// <ja>
        /// 内部高さです。
        /// </ja>
        /// <en>
        /// Internal height.
        /// </en>
        /// </summary>
        int InitialHeight {
            get;
        }
        /// <summary>
        /// <ja>ターミナルタイプです。</ja>
        /// <en>Terminal type.</en>
        /// </summary>
        string TerminalType {
            get;
        }
        /// <summary>
        /// <ja>ターミナル名を設定します。</ja>
        /// <en>Set the terminal name.</en>
        /// </summary>
        /// <param name="terminaltype"><ja>設定するターミナル名</ja><en>Terminal name to set.</en></param>
        void SetTerminalName(string terminaltype);
        /// <summary>
        /// <ja>
        /// ターミナルのサイズを変更します。
        /// </ja>
        /// <en>
        /// Change the terminal size.
        /// </en>
        /// </summary>
        /// <param name="width"><ja>変更後の幅</ja><en>Width after it changes</en></param>
        /// <param name="height"><ja>変更後の高さ</ja><en>Height after it changes</en></param>
        void SetTerminalSize(int width, int height);

        /// <summary>
        /// <ja>
        /// 2つのインターフェイスが「見た目として」同じであるかどうかを調べます。
        /// </ja>
        /// <en>
        /// Comparing two interfaces examine and "Externals" examines be the same. 
        /// </en>
        /// </summary>
        /// <param name="t"><ja>比較対象となるオブジェクト</ja><en>Object to exemine</en></param>
        /// <returns><en>Result of comparing. If it is equal, return true. </en><ja>比較結果。等しいならtrue</ja></returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// 「見た目として」とは、SSHプロトコルのバージョンの違いなど、「接続先を比較する場合」
        /// の同一視を意味します。
        /// </para>
        /// <para>
        /// MRUプラグインではこのメソッドを利用して、些細な違いの項目が複数個、最近使った接続の部分に表示されてしまうことを防いでいます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// "Externals" means one seeing of "The connection destinations are compared" of the difference etc. of the version of the SSH protocol. 
        /// </para>
        /// <para>
        /// The item of a trifling difference is two or more pieces, and it is prevented from being displayed by using this method in the MRU plug-in in the part of the connection used recently. 
        /// </para>
        /// </en>
        /// </remarks>
        bool UIEquals(ITerminalParameter t);
    }

    /// <summary>
    /// <ja>
    /// TelnetまたはSSH接続の共通のパラメータです。
    /// </ja>
    /// <en>
    /// Common parameters for the telnet or the SSH connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <en>
    /// <para>
    /// Default parameter of Telnet connnection can be got by <see cref="Poderosa.Protocols.IProtocolService.CreateDefaultTelnetParameter">CreateDefaultTelnetParameter method</see> on <seealso cref="IProtocolService">IProtocolService</seealso>
    /// </para>
    /// <para>
    /// This interface can be convater to <seealso cref="ITerminalParameter">ITerminalParameter</seealso> by GetAdapter method.
    /// </para>
    /// </en>
    /// <ja>
    /// <para>
    /// デフォルトのTelnet接続パラメータは、<seealso cref="IProtocolService">IProtocolService</seealso>の
    /// <see cref="Poderosa.Protocols.IProtocolService.CreateDefaultTelnetParameter">CreateDefaultTelnetParameterメソッド</see>を使って取得できます。
    /// </para>
    /// <para>
    /// このインターフェイスは、GetAdapterメソッドを使うことで、<seealso cref="ITerminalParameter">ITerminalParameter</seealso>
    /// へと変換できます。
    /// </para>
    /// </ja>
    /// </remarks>
    public interface ITCPParameter : IAdaptable, ICloneable {
        /// <summary>
        /// <ja>
        /// 接続先のホスト名（またはIPアドレス）です。
        /// </ja>
        /// <en>
        /// Hostname or IP Address to connect.
        /// </en>
        /// </summary>
        string Destination {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// 接続先のポート番号です。
        /// </ja>
        /// <en>
        /// Port number to connect.
        /// </en>
        /// </summary>
        int Port {
            get;
            set;
        }
    }

    /// <summary>
    /// Telnet-specific parameters.
    /// </summary>
    public interface ITelnetParameter {

        /// <summary>
        /// Whether the "New Line" code of the telnet protocol is used for sending CR+LF.
        /// </summary>
        bool TelnetNewLine {
            get;
            set;
        }
    }

    /// <summary>
    /// <ja>
    /// SSH接続時のログインパラメータを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Inteface that show the login parameter on SSH connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// デフォルトのSSH接続パラメータは、<seealso cref="IProtocolService">IProtocolService</seealso>の
    /// <see cref="IProtocolService.CreateDefaultSSHParameter">CreateDefaultSSHParameterメソッド</see>を使って取得できます。
    /// </para>
    /// <para>
    /// 接続先のホスト名やポート番号は、GetAdapterメソッドを用いて<seealso cref="ITCPParameter">ITCPParameter</seealso>へと
    /// 変換して設定します。
    /// </para>
    /// <para>
    /// このインターフェイスは、GetAdapterメソッドを使うことで、<seealso cref="ITerminalParameter">ITerminalParameter</seealso>
    /// へと変換できます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// Default parameter of SSH connnection can be got by > by <see cref="Poderosa.Protocols.IProtocolService.CreateDefaultSSHParameter">CreateDefaultTelnetParameter method</see> on<seealso cref="IProtocolService">IProtocolService</seealso>
    /// </para>
    /// <para>
    /// The host name and the port number at the connection destination are converted into <seealso cref="ITCPParameter">ITCPParameter</seealso> by using the GetAdapter method and set. 
    /// </para>
    /// <para>
    /// This interface can be convater to <seealso cref="ITerminalParameter">ITerminalParameter</seealso> by GetAdapter method.
    /// </para>
    /// </en>
    /// </remarks>
    public interface ISSHLoginParameter : IAdaptable, ICloneable {
        /// <summary>
        /// <ja>SSHプロトコルのバージョンです。</ja>
        /// <en>Version of the SSH protocol.</en>
        /// </summary>
        SSHProtocol Method {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// 認証方式です。
        /// </ja>
        /// <en>
        /// Authentification method.
        /// </en>
        /// </summary>
        AuthenticationType AuthenticationType {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// ログインするアカウント名（ユーザー名）です。
        /// </ja>
        /// <en>
        /// Account name (User name) to login.
        /// </en>
        /// </summary>
        string Account {
            get;
            set;
        }
        /// <summary>
        /// <ja>ユーザの認証に使用する秘密鍵のファイル名です。</ja>
        /// <en>Private key file name to use to user authentification.</en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// このプロパティは、AuthenticationTypeがAutehnticationType.PublicKeyのときのみ使われます。
        /// </ja>
        /// <en>
        /// This property is used when AuthenticationType is AutehnticationType.PublicKey only.
        /// </en>
        /// </remarks>
        string IdentityFileName {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// パスワードまたはパスフレーズです。
        /// </ja>
        /// <en>
        /// Password or passphrase
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// AuthenticationTypeプロパティがAuthenticationType.Passwordのときには「パスワード」を、
        /// AuthenticationType.PublicKeyのときには「パスフレーズ」を設定します。
        /// AuthenticationType.KeyboardInteractiveのときには、このプロパティは無視されます。
        /// </ja>
        /// <en>
        /// Set password when AuthenticationType is AuthenticationType.Password, and, set passphrase when AuthenticationType.PublicKey.
        /// This property is ignored if it is AuthenticationType.KeyboardInteractive.
        /// </en>
        /// </remarks>
        string PasswordOrPassphrase {
            get;
            set;
        }
        //ユーザにパスワードを入力させるかどうか。trueのときはPasswordOrPassphraseは使用しない
        /// <summary>
        /// <ja>
        /// ユーザーに パスワードを入力させるかどうかのフラグです。
        /// </ja>
        /// <en>
        /// Flag whether to make user input password
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>trueの場合、PasswordOrPassphraseプロパティは使われません。</ja><en>If it is true, PasswordOrPassphrase property is not used.</en>
        /// </remarks>
        /// <exclude/>
        bool LetUserInputPassword {
            get;
            set;
        }

        /// <summary>
        /// Whether the agent forwarding is enabled.
        /// </summary>
        bool EnableAgentForwarding {
            get;
            set;
        }

        /// <summary>
        /// Key provider for the agent forwarding.
        /// </summary>
        IAgentForwardingAuthKeyProvider AgentForwardingAuthKeyProvider {
            get;
            set;
        }

        /// <summary>
        /// Whether the X11 forwarding is enabled.
        /// </summary>
        bool EnableX11Forwarding {
            get;
            set;
        }

        /// <summary>
        /// X11 forwarding settings.
        /// </summary>
        X11ForwardingParams X11Forwarding {
            get;
            set;
        }
    }

    /// <summary>
    /// <ja>
    /// Cygwin接続するときに使われるパラメータを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that show the parameter using on Cygwin connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// このインターフェイスは、<seealso cref="IProtocolService">IProtocolService</seealso>の
    /// <see cref="IProtocolService.CreateDefaultCygwinParameter">CreateDefaultCygwinParameterメソッド</see>
    /// を呼び出すことで取得できます。
    /// </para>
    /// <para>
    /// このインターフェイスは、GetAdapterメソッドを使うことで、<seealso cref="ITerminalParameter">ITerminalParameter</seealso>
    /// へと変換できます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// This interface can be  got using <see cref="IProtocolService.CreateDefaultCygwinParameter">CreateDefaultCygwinParameter method</see> on <seealso cref="IProtocolService">IProtocolService</seealso>.
    /// </para>
    /// <para>
    /// This interface is convert to <seealso cref="ITerminalParameter">ITerminalParameter</seealso> by GetAdapter method.
    /// </para>
    /// </en>
    /// </remarks>
    public interface ICygwinParameter : IAdaptable, ICloneable {
        /// <summary>
        /// <ja>
        /// シェルの名前を取得／設定します。
        /// </ja>
        /// <en>
        /// Get / set shell name.
        /// </en>
        /// </summary>
        string ShellName {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// ホームディレクトリを取得／設定します。
        /// </ja>
        /// <en>
        /// Get / set the home directory.
        /// </en>
        /// </summary>
        string Home {
            get;
            set;
        }
        /// <summary>
        /// <ja>
        /// シェルから引数部分を取り除いたコマンド部分だけを返します。
        /// </ja>
        /// <en>
        /// Only the command part where the argument part was removed from the shell is returned. 
        /// </en>
        /// </summary>
        string ShellBody {
            get;
        }
        /// <summary>
        /// <ja>
        /// Cygwinの場所を取得／設定します。
        /// 設定されない場合はレジストリから検出します。
        /// </ja>
        /// <en>
        /// Get or Set path where Cygwin is installed.
        /// If this property was not set, the path will be detected from the registry entry.
        /// </en>
        /// </summary>
        string CygwinDir {
            get;
            set;
        }
    }

    /// <summary>
    /// <ja>
    /// マクロの自動実行のパラメータを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// This interface represents parameters for macro auto execution.
    /// </en>
    /// </summary>
    public interface IAutoExecMacroParameter : IAdaptable, ICloneable {
        /// <summary>
        /// <ja>
        /// 接続後に自動実行するマクロのパス。未指定のときはnull。
        /// </ja>
        /// <en>
        /// Path to a macro which will be run automatically after the connection was established.
        /// Null if it is not specified.
        /// </en>
        /// </summary>
        String AutoExecMacroPath {
            get;
            set;
        }
    }
}
