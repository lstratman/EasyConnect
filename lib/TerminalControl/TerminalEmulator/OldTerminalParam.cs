/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: OldTerminalParam.cs,v 1.12 2012/03/18 14:15:24 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

using Poderosa.Document;
using Poderosa.Terminal;
using Poderosa.Util;
using Poderosa.View;
using Poderosa.Protocols;
using Granados;

namespace Poderosa.ConnectionParam {

    /*
     * TerminalParamはマクロからもフルにアクセス可能にするためpublicにする
     * 公開する必要のないメソッドをinternalにする
     */

    //Granados内のAuthenticationTypeと同一だが、起動の高速化のため使わない

    /// <summary>
    /// <ja>SSHでの認証方法を示します。</ja>
    /// <en>Specifies the authemtication method of SSH.</en>
    /// </summary>
    /// <exclude/>
    public enum AuthType {
        /// <summary>
        /// <ja>パスワード認証</ja>
        /// <en>Authentication using password.</en>
        /// </summary>
        [EnumValue(Description = "Enum.AuthType.Password")]
        Password,

        /// <summary>
        /// <ja>手元の秘密鍵とリモートホストに登録した公開鍵を使った認証</ja>
        /// <en>Authentication using the local private key and the remote public key.</en>
        /// </summary>
        [EnumValue(Description = "Enum.AuthType.PublicKey")]
        PublicKey,

        /// <summary>
        /// <ja>コンソール上でパスワードを入力する認証</ja>
        /// <en>Authentication by sending the password through the console.</en>
        /// </summary>
        [EnumValue(Description = "Enum.AuthType.KeyboardInteractive")]
        KeyboardInteractive
    }

    /// <summary>
    /// Utility methods for conversion between <see cref="AuthType"/> and <see cref="AuthenticationType"/>.
    /// </summary>
    public static class AuthTypeMixin {
        /// <summary>
        /// Converts <see cref="AuthType"/> to <see cref="AuthenticationType"/>.
        /// </summary>
        /// <param name="authType"></param>
        /// <returns></returns>
        public static AuthenticationType ToAuthenticationType(this AuthType authType) {
            switch (authType) {
                case AuthType.Password:
                    return AuthenticationType.Password;

                case AuthType.PublicKey:
                    return AuthenticationType.PublicKey;

                case AuthType.KeyboardInteractive:
                    return AuthenticationType.KeyboardInteractive;

                default:
                    throw new ArgumentException("Unsupported AuthType", "authType");
            }
        }

        /// <summary>
        /// Converts <see cref="AuthenticationType"/> to <see cref="AuthType"/>.
        /// </summary>
        /// <param name="authenticationType"></param>
        /// <returns></returns>
        public static AuthType ToAuthType(this AuthenticationType authenticationType) {
            switch (authenticationType) {
                case AuthenticationType.Password:
                    return AuthType.Password;

                case AuthenticationType.PublicKey:
                    return AuthType.PublicKey;

                case AuthenticationType.KeyboardInteractive:
                    return AuthType.KeyboardInteractive;

                default:
                    throw new ArgumentException("Unsupported AuthenticationType", "authenticationType");
            }
        }
    }

    /// <summary>
    /// <ja>接続の種類を示します。</ja>
    /// <en>Specifies the type of the connection.</en>
    /// </summary>
    /// <exclude/>
    public enum ConnectionMethod {
        /// <summary>
        /// Telnet
        /// </summary>
        Telnet,
        /// <summary>
        /// SSH1
        /// </summary>
        SSH1,
        /// <summary>
        /// SSH2
        /// </summary>
        SSH2
    }

    /// <summary>
    /// <ja>エンコーディングを示します。</ja>
    /// <en>Specifies the encoding of the connection.</en>
    /// <!--
    /// <seealso cref="Poderosa.ConnectionParam.TerminalParam.Encoding"/>
    /// -->
    /// </summary>
    /// <exclude/>
    public enum EncodingType {

        // For supporting the third-party plugin, integer value of each member shouldn't be changed.
        // However, the item order according to the integer value may not be suitable for the UI (like combo box).
        // So we specify integer values explicitly, and place members in order that we want to show.

        /// <summary>
        /// <ja>ISO 8859-1</ja>
        /// <en>ISO 8859-1</en>
        /// </summary>
        [EnumValue(Description = "Enum.EncodingType.ISO8859_1")]
        ISO8859_1 = 0,
        /// <summary>
        /// <ja>UTF-8 (CJKテキスト表示用)</ja>
        /// <en>UTF-8 (for displaying CJK text)</en>
        /// </summary>
        /// <remarks>
        /// <ja>CJKキャラクタセットに含まれる記号、罫線、欧文文字等は、CJKフォントで全角表示されます。</ja>
        /// <en>Characters like symbols, box-drawing characters or european characters that are contained in CJK character sets are displayed in zenkaku using CJK font.</en>
        /// </remarks>
        [EnumValue(Description = "Enum.EncodingType.UTF8")]
        UTF8 = 1,
        /// <summary>
        /// <ja>UTF-8 (欧文表示用)</ja>
        /// <en>UTF-8 (for displaying american or european text)</en>
        /// </summary>
        /// <remarks>
        /// <ja>記号、罫線、欧文文字等は、メインフォントで半角表示されます。
        /// 漢字等の東アジアの文字はCJKフォントで全角表示されます。</ja>
        /// <en>Characters like symbols, box-drawing characters or european characters are displayed in Hankaku using main font.
        /// East asian characters like Kanji are displayed in Zenkaku using CJK font.</en>
        /// </remarks>
        [EnumValue(Description = "Enum.EncodingType.UTF8_Latin")]
        UTF8_Latin = 8,
        /// <summary>
        /// <ja>EUC JP (主に日本語の文字で使用)</ja>
        /// <en>EUC JP (This encoding is primarily used with Japanese characters.)</en>
        /// </summary>
        [EnumValue(Description = "Enum.EncodingType.EUC_JP")]
        EUC_JP = 2,
        /// <summary>
        /// <ja>Shift JIS (主に日本語の文字で使用)</ja>
        /// <en>Shift JIS (This encoding is primarily used with Japanese characters.)</en>
        /// </summary>
        [EnumValue(Description = "Enum.EncodingType.SHIFT_JIS")]
        SHIFT_JIS = 3,
        /// <summary>
        /// <ja>GB2312 (主に簡体字で使用)</ja>
        /// <en>GB2312 (This encoding is primarily used with simplified Chinese characters.)</en>
        /// </summary>
        [EnumValue(Description = "Enum.EncodingType.GB2312")]
        GB2312 = 4,
        /// <summary>
        /// <ja>Big5 (主に繁体字で使用)</ja>
        /// <en>Big5 (This encoding is primarily used with traditional Chinese characters.)</en>
        /// </summary>
        [EnumValue(Description = "Enum.EncodingType.BIG5")]
        BIG5 = 5,
        /// <summary>
        /// <ja>EUC CN (主に簡体字で使用)</ja>
        /// <en>EUC CN (This encoding is primarily used with simplified Chinese characters.)</en>
        /// </summary>
        [EnumValue(Description = "Enum.EncodingType.EUC_CN")]
        EUC_CN = 6,
        /// <summary>
        /// <ja>EUC KR (主に韓国語文字で使用)</ja>
        /// <en>EUC KR (This encoding is primarily used with Korean characters.)</en>
        /// </summary>
        [EnumValue(Description = "Enum.EncodingType.EUC_KR")]
        EUC_KR = 7,
        /// <summary>
        /// <ja>OEM 850</ja>
        /// <en>OEM 850</en>
        /// </summary>
        [EnumValue(Description = "Enum.EncodingType.OEM850")]
        OEM850 = 9,
    }

    /// <summary>
    /// <ja>ログの種類を示します。</ja>
    /// <en>Specifies the log type.</en>
    /// </summary>
    /// <exclude/>
    public enum LogType {
        /// <summary>
        /// <ja>ログはとりません。</ja>
        /// <en>The log is not recorded.</en>
        /// </summary>
        [EnumValue(Description = "Enum.LogType.None")]
        None,
        /// <summary>
        /// <ja>テキストモードのログです。これが標準です。</ja>
        /// <en>The log is a plain text file. This is standard.</en>
        /// </summary>
        [EnumValue(Description = "Enum.LogType.Default")]
        Default,
        /// <summary>
        /// <ja>テキストモードのログ、タイムスタンプ付。</ja>
        /// <en>Plain text file, logged with timestamp.</en>
        /// </summary>
        [EnumValue(Description = "Enum.LogType.PlainTextWithTimestamp")]
        PlainTextWithTimestamp,
        /// <summary>
        /// <ja>バイナリモードのログです。</ja>
        /// <en>The log is a binary file.</en>
        /// </summary>
        [EnumValue(Description = "Enum.LogType.Binary")]
        Binary,
        /// <summary>
        /// <ja>XMLで保存します。また内部的なバグ追跡においてこのモードでのログ採取をお願いすることがあります。</ja>
        /// <en>The log is an XML file. We may ask you to record the log in this type for debugging.</en>
        /// </summary>
        [EnumValue(Description = "Enum.LogType.Xml")]
        Xml
    }

    /// <summary>
    /// <ja>送信時の改行の種類を示します。</ja>
    /// <en>Specifies the new-line characters for transmission.</en>
    /// </summary>
    /// <exclude/>
    public enum NewLine {
        /// <summary>
        /// CR
        /// </summary>
        [EnumValue(Description = "Enum.NewLine.CR")]
        CR,
        /// <summary>
        /// LF
        /// </summary>
        [EnumValue(Description = "Enum.NewLine.LF")]
        LF,
        /// <summary>
        /// CR+LF
        /// </summary>
        [EnumValue(Description = "Enum.NewLine.CRLF")]
        CRLF
    }

    /// <summary>
    /// <ja>ターミナルの種別を示します。</ja>
    /// <en>Specifies the type of the terminal.</en>
    /// </summary>
    /// <remarks>
    /// <ja>XTermにはVT100にはないいくつかのエスケープシーケンスが含まれています。</ja>
    /// <en>XTerm supports several escape sequences in addition to VT100.</en>
    /// <ja>KTermは中身はXTermと一緒ですが、SSHやTelnetの接続オプションにおいてターミナルの種類を示す文字列として"kterm"がセットされます。</ja>
    /// <en>Though the functionality of KTerm is identical to XTerm, the string "kterm" is used for specifying the type of the terminal in the connection of Telnet or SSH.</en>
    /// <ja>この設定は、多くの場合TERM環境変数の値に影響します。</ja>
    /// <en>In most cases, this setting affects the TERM environment variable.</en>
    /// </remarks>
    /// <exclude/>
    public enum TerminalType {
        /// <summary>
        /// vt100
        /// </summary>
        [EnumValue(Description = "Enum.TerminalType.VT100")]
        VT100,
        /// <summary>
        /// xterm
        /// </summary>
        [EnumValue(Description = "Enum.TerminalType.XTerm")]
        XTerm,
        /// <summary>
        /// kterm
        /// </summary>
        [EnumValue(Description = "Enum.TerminalType.KTerm")]
        KTerm
    }

    /// <summary>
    /// Utility methods for conversion of <see cref="TerminalType"/>.
    /// </summary>
    public static class TerminalTypeMixin {

        /// <summary>
        /// Converts <see cref="TerminalType"/> to a value of the TERM environment variable.
        /// </summary>
        /// <param name="type">terminal type</param>
        /// <returns></returns>
        public static string ToTermValue(this TerminalType type) {
            switch (type) {
                case TerminalType.VT100:
                    return "vt100";
                case TerminalType.XTerm:
                    return "xterm";
                case TerminalType.KTerm:
                    return "kterm";
                default:
                    throw new ArgumentException("Unknown TerminalType : " + type.ToString(), "type");
            }
        }

    }

    /// <summary>
    /// <ja>受信した文字に対する改行方法を示します。</ja>
    /// <en>Specifies line breaking style.</en>
    /// </summary>
    /// <exclude/>
    public enum LineFeedRule {
        /// <summary>
        /// <ja>標準</ja>
        /// <en>Standard</en>
        /// </summary>
        [EnumValue(Description = "Enum.LineFeedRule.Normal")]
        Normal,
        /// <summary>
        /// <ja>LFで改行しCRを無視</ja>
        /// <en>LF:Line Break, CR:Ignore</en>
        /// </summary>
        [EnumValue(Description = "Enum.LineFeedRule.LFOnly")]
        LFOnly,
        /// <summary>
        /// <ja>CRで改行しLFを無視</ja>
        /// <en>CR:Line Break, LF:Ignore</en>
        /// </summary>
        [EnumValue(Description = "Enum.LineFeedRule.CROnly")]
        CROnly
    }

#if !MACRODOC
    /// <summary>
    /// <ja>フローコントロールの設定</ja>
    /// <en>Specifies the flow control.</en>
    /// </summary>
    /// <exclude/>
    public enum FlowControl {
        /// <summary>
        /// <ja>なし</ja>
        /// <en>None</en>
        /// </summary>
        [EnumValue(Description = "Enum.FlowControl.None")]
        None,
        /// <summary>
        /// X ON / X OFf
        /// </summary>
        [EnumValue(Description = "Enum.FlowControl.Xon_Xoff")]
        Xon_Xoff,
        /// <summary>
        /// <ja>ハードウェア</ja>
        /// <en>Hardware</en>
        /// </summary>
        [EnumValue(Description = "Enum.FlowControl.Hardware")]
        Hardware
    }

    /// <summary>
    /// <ja>パリティの設定</ja>
    /// <en>Specifies the parity.</en>
    /// </summary>
    /// <exclude/>
    public enum Parity {
        /// <summary>
        /// <ja>なし</ja>
        /// <en>None</en>
        /// </summary>
        [EnumValue(Description = "Enum.Parity.NOPARITY")]
        NOPARITY = 0,
        /// <summary>
        /// <ja>奇数</ja>
        /// <en>Odd</en>
        /// </summary>
        [EnumValue(Description = "Enum.Parity.ODDPARITY")]
        ODDPARITY = 1,
        /// <summary>
        /// <ja>偶数</ja>
        /// <en>Even</en>
        /// </summary>
        [EnumValue(Description = "Enum.Parity.EVENPARITY")]
        EVENPARITY = 2
        //MARKPARITY  =        3,
        //SPACEPARITY =        4
    }

    /// <summary>
    /// <ja>ストップビットの設定</ja>
    /// <en>Specifies the stop bits.</en>
    /// </summary>
    /// <exclude/>
    public enum StopBits {
        /// <summary>
        /// <ja>1ビット</ja>
        /// <en>1 bit</en>
        /// </summary>
        [EnumValue(Description = "Enum.StopBits.ONESTOPBIT")]
        ONESTOPBIT = 0,
        /// <summary>
        /// <ja>1.5ビット</ja>
        /// <en>1.5 bits</en>
        /// </summary>
        [EnumValue(Description = "Enum.StopBits.ONE5STOPBITS")]
        ONE5STOPBITS = 1,
        /// <summary>
        /// <ja>2ビット</ja>
        /// <en>2 bits</en>
        /// </summary>
        [EnumValue(Description = "Enum.StopBits.TWOSTOPBITS")]
        TWOSTOPBITS = 2
    }
#endif

}