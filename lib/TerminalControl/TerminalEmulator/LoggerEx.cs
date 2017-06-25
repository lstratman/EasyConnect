/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: LoggerEx.cs,v 1.2 2011/12/19 17:14:35 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

using Poderosa.Document;
using Poderosa.Protocols;

namespace Poderosa.Terminal {
    /// <summary>
    /// <ja>
    /// ログの基底インターフェイスです。
    /// </ja>
    /// <en>
    /// Base interface of the log.
    /// </en>
    /// </summary>
    public interface ILoggerBase {
        /// <summary>
        /// <ja>ログを閉じます。</ja>
        /// <en>Close log</en>
        /// </summary>
        void Close();
        /// <summary>
        /// <ja>ログをフラッシュします。</ja>
        /// <en>Flush log</en>
        /// </summary>
        void Flush();
        /// <summary>
        /// <ja>自動フラッシュの処理を行います。</ja>
        /// <en>Do the auto flush.</en>
        /// </summary>
        void AutoFlush();
    }

    /// <summary>
    /// <ja>
    /// バイナリのロガーを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that show the logger of binary.
    /// </en>
    /// </summary>
    public interface IBinaryLogger : ILoggerBase {
        /// <summary>
        /// <ja>バイナリログを書き込みます。</ja><en>Write a binary log</en>
        /// </summary>
        /// <param name="data"><ja>書き込まれようとしているデータです。</ja><en>Data to write.</en></param>
        /// <remarks>
        /// <ja>バイナリロガーの実装者は、<paramref name="data"/>に渡されたデータを書き込むように実装します。</ja><en>Those who implements about binary logger implement like writing the data passed to <paramref name="data"/>. </en>
        /// </remarks>
        void Write(ByteDataFragment data);
    }

    /// <summary>
    /// <ja>
    /// テキストのロガーを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that show the logger of text.
    /// </en>
    /// </summary>
    public interface ITextLogger : ILoggerBase {
        /// <summary>
        /// <ja>
        /// テキストログを書き込みます。
        /// </ja>
        /// <en>Write a text log</en>
        /// </summary>
        /// <param name="line"><ja>書き込まれようとしているデータです。</ja><en>Data to write.</en></param>
        /// <remarks>
        /// <ja>テキストロガーの実装者は、<paramref name="line"/>に渡されたデータを書き込むように実装します。</ja><en>Those who implements about text logger implement like writing the data passed to <paramref name="line"/>. </en>
        /// </remarks>
        void WriteLine(GLine line); //テキストベースはLine単位
        /// <summary>
        /// <ja>コメントを書き込みます。</ja>
        /// <en>Write a comment</en>
        /// </summary>
        /// <param name="comment"><ja>書き込まれようとしているコメントです。</ja><en>Comment to write.</en></param>
        /// <remarks>
        /// <ja>テキストロガーの実装者は、<paramref name="comment"/>に渡されたデータを書き込むように実装します。</ja><en>Those who implements about text logger implement like writing the data passed to <paramref name="comment"/>. </en>
        /// </remarks>
        void Comment(string comment);
    }

    /// <summary>
    /// <ja>
    /// XMLのロガーを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that show the logger of XML.
    /// </en>
    /// </summary>
    public interface IXmlLogger : ILoggerBase {
        /// <summary>
        /// <ja>
        /// XMLログを書き込みます。
        /// </ja>
        /// <en>Write a XML log</en>
        /// </summary>
        /// <param name="ch"><ja>書き込まれようとしているデータです。</ja><en>Data to write.</en></param>
        /// <remarks>
        /// <ja>XMLロガーの実装者は、<paramref name="ch"/>に渡されたデータを書き込むように実装します。</ja>
        /// <en>Those who implements about XML logger implement like writing the data passed to <paramref name="ch"/>. </en>
        /// </remarks>
        void Write(char ch);
        /// <summary>
        /// <ja>
        /// XMLログをエスケープして書き込みます。
        /// </ja>
        /// <en>
        /// writes log escaping in the XML.
        /// </en>
        /// </summary>
        /// <param name="body"><ja>書き込まれようとしているデータです。</ja><en>Data to write.</en></param>
        /// <remarks>
        /// <ja>XMLロガーの実装者は、<paramref name="body"/>に渡されたデータをエスケープして書き込むように実装します。</ja>
        /// <en>Those who implements about XML logger implement like writing the data passed to <paramref name="body"/> with escaping. </en>
        /// </remarks>
        void EscapeSequence(char[] body);
        /// <summary>
        /// <ja>
        /// コメントを書き込みます。
        /// </ja>
        /// <en>Write a comment</en>
        /// </summary>
        /// <param name="comment"><ja>書き込まれようとしているコメントです。</ja><en>Comment to write.</en></param>
        /// <remarks>
        /// <ja>XMLロガーの実装者は、<paramref name="comment"/>に渡されたデータを書き込むように実装します。</ja><en>Those who implements about XML logger implement like writing the data passed to <paramref name="comment"/>. </en>
        /// </remarks>
        void Comment(string comment);
    }

    /// <summary>
    /// <ja>
    /// ログサービスにアクセスするインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface accessed log service.
    /// </en>
    /// </summary>
    public interface ILogService {
        /// <summary>
        /// <ja>
        /// バイナリのロガーを登録します。
        /// </ja>
        /// <en>
        /// Regist the logger of binary
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>登録するロガー</ja><en>Logger to regist.</en></param>
        void AddBinaryLogger(IBinaryLogger logger);
        /// <summary>
        /// <ja>
        /// バイナリのロガーを解除します。
        /// </ja>
        /// <en>
        /// Remove the logger of binary
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>解除するロガー</ja><en>Logger to remove.</en></param>
        void RemoveBinaryLogger(IBinaryLogger logger);
        /// <summary>
        /// <ja>
        /// テキストのロガーを登録します。
        /// </ja>
        /// <en>
        /// Regist the logger of text
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>登録するロガー</ja><en>Logger to regist.</en></param>
        void AddTextLogger(ITextLogger logger);
        /// <summary>
        /// <ja>
        /// テキストのロガーを解除します。
        /// </ja>
        /// <en>
        /// Remove the logger of text
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>解除するロガー</ja><en>Logger to remove.</en></param>
        void RemoveTextLogger(ITextLogger logger);
        /// <summary>
        /// <ja>
        /// XMLのロガーを登録します。
        /// </ja>
        /// <en>
        /// Regist the logger of XML
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>登録するロガー</ja><en>Logger to regist.</en></param>
        void AddXmlLogger(IXmlLogger logger);
        /// <summary>
        /// <ja>
        /// XMLのロガーを解除します。
        /// </ja>
        /// <en>
        /// Remove the logger of XML
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>解除するロガー</ja><en>Logger to remove.</en></param>
        void RemoveXmlLogger(IXmlLogger logger);

        /// <summary>
        /// <ja>
        /// ログ設定を反映させます。
        /// </ja>
        /// <en>
        /// Apply the log setting.
        /// </en>
        /// </summary>
        /// <param name="settings"><ja>ログの設定</ja><en>Set of log.</en></param>
        /// <param name="clear_previous"><ja>設定前にクリアするかどうかのフラグ</ja><en>Flag whether clear before it sets it</en></param>
        /// <exclude/>
        void ApplyLogSettings(ILogSettings settings, bool clear_previous);
        /// <summary>
        /// <ja>
        /// ログのコメントを設定します。
        /// </ja>
        /// <en>
        /// Set the comment on the log.
        /// </en>
        /// </summary>
        /// <param name="comment"><ja>設定するコメント</ja><en>Comment to set.</en></param>
        /// <exclude/>
        void Comment(string comment);
    }

}
