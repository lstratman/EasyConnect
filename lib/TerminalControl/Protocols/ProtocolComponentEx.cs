/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ProtocolComponentEx.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Poderosa.Protocols {
    //データ送受信の引数セット(byte[], int, int)の束
    /// <summary>
    /// <ja>
    /// データ送受信の引数セットを取り扱うクラスです。
    /// </ja>
    /// <en>
    /// Class that handles set of argument of data transmitting and receiving.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このクラスは、「バイトデータ」「オフセット」「長さ」をセットとして扱うもので、データを送受信する際の引数として使われます。
    /// </ja>
    /// <en>
    /// This class is used as an argument when data is transmit and received by the one to treat "Byte data", "Offset", and "Length" as a set. 
    /// </en>
    /// </remarks>
    public class ByteDataFragment {
        private byte[] _buffer;
        private int _offset;
        private int _length;

        /// <summary>
        /// <ja>
        /// データ送受信セットを作成します。
        /// </ja>
        /// <en>
        /// Create a set of data transmitting / recieving.
        /// </en>
        /// </summary>
        public ByteDataFragment() {
        }
        /// <summary>
        /// <ja>
        /// 「データ」「オフセット」「長さ」を指定してデータ送受信セットを作成します。
        /// </ja>
        /// <en>
        /// This class is used as an argument when data is transmit and received by the one to treat "Byte data", "Offset", and "Length" as a set. 
        /// </en>
        /// </summary>
        /// <param name="data"><ja>送受信されるデータを示す配列です。</ja><en>It is an array that shows the transmit and received data. </en></param>
        /// <param name="offset"><ja>送受信先を示す<paramref name="data"/>のオフセット位置です。</ja><en>It is an offset position of <paramref name="data"/> that shows the transmitting and receiving destination. </en></param>
        /// <param name="length"><ja>送受信する長さです。</ja><en>It is sent and received length. </en></param>
        public ByteDataFragment(byte[] data, int offset, int length) {
            Set(data, offset, length);
        }

        /// <summary>
        /// <ja>
        /// 「データ」「オフセット」「長さ」を設定します。
        /// </ja>
        /// <en>
        /// Set "Data", "Offset", "Length".
        /// </en>
        /// </summary>
        /// <param name="buffer"><ja>送受信されるデータを示す配列です。</ja><en>It is an array that shows the tranmit and received data. </en></param>
        /// <param name="offset"><ja>送受信先を示す<paramref name="buffer"/>へのオフセット位置です。</ja><en>It is an offset position to <paramref name="buffer"/> that shows the transmitting and receiving destination. </en></param>
        /// <param name="length"><ja>送受信する長さです。</ja><en>It is transmit and received length. </en></param>
        /// <returns></returns>
        public ByteDataFragment Set(byte[] buffer, int offset, int length) {
            _buffer = buffer;
            _offset = offset;
            _length = length;
            return this;
        }

        /// <summary>
        /// <ja>送受信バッファです。</ja>
        /// <en>Tranmit / recieve buffer</en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// 送受信データはここに格納します。
        /// </ja>
        /// <en>
        /// Tranmit / Recieved data stored here.
        /// </en>
        /// </remarks>
        public byte[] Buffer {
            get {
                return _buffer;
            }
        }

        /// <summary>
        /// <ja>
        /// 送受信のオフセットです。
        /// </ja>
        /// <en>
        /// Offset of tranmitting and receiving. 
        /// </en>
        /// </summary>
        public int Offset {
            get {
                return _offset;
            }
        }

        /// <summary>
        /// <ja>
        /// 送受信する長さです。
        /// </ja>
        /// <en>
        /// Length of tranmitting and receiving. 
        /// </en>
        /// </summary>
        public int Length {
            get {
                return _length;
            }
        }
    }

    //byte[]ベースの出力。旧AbstractGuevaraSocket
    /// <summary>
    /// <ja>
    /// データを送信するためのインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to transmit data.
    /// </en>
    /// </summary>
    public interface IByteOutputStream {
        /// <summary>
        /// <ja>
        /// ByteDataFragmentオブジェクトを指定してデータを送信します。
        /// </ja>
        /// <en>
        /// Data is transmitted specifying the ByteDataFragment object. 
        /// </en>
        /// </summary>
        /// <param name="data"><ja>送信するデータが入っているオブジェクトです。</ja><en>Object with transmitted data</en></param>
        /// <overloads>
        /// <summary>
        /// <ja>
        /// データを送信します。
        /// </ja>
        /// <en>
        /// Transmitting data.
        /// </en>
        /// </summary>
        /// </overloads>
        void Transmit(ByteDataFragment data);
        /// <summary>
        /// <ja>
        /// 「バイト配列」「オフセット」「長さ」を指定してデータを送信します。
        /// </ja>
        /// <en>
        /// Data is transmitted specifying "Byte array", "Offset", and "Length". 
        /// </en>
        /// </summary>
        /// <param name="data"><ja>データが入っているバイト配列</ja><en>Byte array with data</en></param>
        /// <param name="offset"><ja>データを送信する位置を示したオフセット</ja><en>Offset that showed position in which data is transmitted</en></param>
        /// <param name="length"><ja>送信する長さ</ja><en>Transmitted length</en></param>
        void Transmit(byte[] data, int offset, int length);
        /// <summary>
        /// <ja>
        /// 接続を閉じます。
        /// </ja>
        /// <en>
        /// Close the connection.
        /// </en>
        /// </summary>
        void Close();
    }
    //byte[]ベースの非同期入力。旧IDataReceiver
    /// <summary>
    /// <ja>
    /// <seealso cref="IPoderosaSocket">IPoderosaSocket</seealso>を通じてデータを非同期で
    /// 受信するときに用いるインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that 	used when data is asynchronously received through <seealso cref="IPoderosaSocket">IPoderosaSocket</seealso>. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// データを受信したいプラグインは、このインターフェイスを実装したオブジェクトを用意し、
    /// <seealso cref="IPoderosaSocket">IPoderosaSocket</seealso>の<see cref="IPoderosaSocket.RepeatAsyncRead">
    /// RepeatAsyncReadメソッド</see>を呼び出して登録します。
    /// </ja>
    /// <en>
    /// The object that implements this interface is prepared, and the plug-in that wants to receive the data calls and registers the <see cref="IPoderosaSocket.RepeatAsyncRead">
    /// RepeatAsyncRead method</see> of <seealso cref="IPoderosaSocket">IPoderosaSocket</seealso>. 
    /// </en>
    /// </remarks>
    public interface IByteAsyncInputStream {
        /// <summary>
        /// <ja>
        /// データが届いたときに呼び出されます。
        /// </ja>
        /// <en>
        /// Called when data recieves
        /// </en>
        /// </summary>
        /// <param name="data"><ja>受信データを示すオブジェクト</ja><en>Object that show the recieved data.</en></param>
        void OnReception(ByteDataFragment data);
        /// <summary>
        /// <ja>
        /// 接続が通常の切断手順で終了したときに呼び出されます。
        /// </ja>
        /// <en>
        /// When the connection terminates normally, it is called. 
        /// </en>
        /// </summary>
        void OnNormalTermination();
        /// <summary>
        /// <ja>
        /// 接続がエラーなどの異常によって終了したときに呼び出されます。
        /// </ja>
        /// <en>
        /// When the connection terminates due to abnormality of the error etc. , it is called. 
        /// </en>
        /// </summary>
        /// <param name="message"><ja>切断された理由を示す文字列</ja><en>String that shows closed reason</en></param>
        void OnAbnormalTermination(string message);
    }

    //接続の成功・失敗の通知。たとえばMRUコンポーネントがこれを受信して自身の情報を更新する
    //Interruptされた場合は通知なし
    /// <summary>
    /// <ja>
    /// 接続の成功・失敗の通知を受け取るインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that receives notification of success and failure of connection
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインターフェイスは、Protocolsプラグインの拡張ポイント「org.poderosa.protocols.resultEventHandler」によって提供されます。
    /// </ja>
    /// <en>
    /// This interface is offered with the extension point (org.poderosa.protocols.resultEventHandler) of the Protocols plug-in. 
    /// </en>
    /// </remarks>
    public interface IConnectionResultEventHandler {
        /// <summary>
        /// <ja>
        /// 接続が成功したときに呼び出されます。
        /// </ja>
        /// <en>
        /// When the connection succeeds, it is called. 
        /// </en>
        /// </summary>
        /// <param name="param"><ja>接続パラメータです</ja><en>Connection parameter.</en></param>
        void OnSucceeded(ITerminalParameter param);
        /// <summary>
        /// <ja>
        /// 接続が失敗したときに呼び出されます。
        /// </ja>
        /// <en>
        /// When the connection fails, it is called. 
        /// </en>
        /// </summary>
        /// <param name="param"><ja>接続パラメータです</ja><en>Connection parameter.</en></param>
        /// <param name="msg"><ja>失敗した理由が含まれるメッセージです</ja><en>Message being included for failing reason</en></param>
        void OnFailed(ITerminalParameter param, string msg);

        /// <summary>
        /// <ja>
        /// 非同期接続開始前に呼ばれます
        /// </ja>
        /// <en>
        /// Called before the asynchronous connection starts
        /// </en>
        /// </summary>
        /// <param name="param"></param>
        void BeforeAsyncConnect(ITerminalParameter param);
    }

}
