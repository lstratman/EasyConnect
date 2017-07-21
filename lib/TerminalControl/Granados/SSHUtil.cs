// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

namespace Granados {

    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Exception about SSH operation.
    /// </summary>
    public class SSHException : Exception {

        public SSHException(string message)
            : base(message) {
        }

        public SSHException(string message, Exception cause)
            : base(message, cause) {
        }

    }

    /// <summary>
    /// Attribute to define algorithm name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AlgorithmSpecAttribute : Attribute {
        /// <summary>
        /// Algorithm name
        /// </summary>
        public string AlgorithmName {
            get;
            set;
        }

        /// <summary>
        /// Default priority
        /// </summary>
        /// <remarks>
        /// Larger number means higher priority.
        /// </remarks>
        public int DefaultPriority {
            get;
            set;
        }
    }

    /// <summary>
    /// Utility methods for accessing <see cref="AlgorithmSpecAttribute"/>.
    /// </summary>
    public static class AlgorithmSpecUtil<TEnum> {

        // prepare dictionary of AlgorithmSpecAttribute for each given generics type parameter.
        private static readonly ConcurrentDictionary<TEnum, AlgorithmSpecAttribute> _enumToSpec;

        static AlgorithmSpecUtil() {
            _enumToSpec = new ConcurrentDictionary<TEnum, AlgorithmSpecAttribute>();
            Type enumType = typeof(TEnum);
            if (enumType.IsEnum) {
                foreach (string enumName in enumType.GetEnumNames()) {
                    FieldInfo field = enumType.GetField(enumName, BindingFlags.Public | BindingFlags.Static);
                    TEnum enumValue = (TEnum)field.GetValue(null);
                    object[] attrs = field.GetCustomAttributes(typeof(AlgorithmSpecAttribute), false);
                    if (attrs.Length > 0) {
                        AlgorithmSpecAttribute spec = (AlgorithmSpecAttribute)attrs[0];
                        _enumToSpec.TryAdd(enumValue, spec);
                    }
                }
            }
        }

        /// <summary>
        /// Get algorithm name from <see cref="AlgorithmSpecAttribute"/> of the specified enum value.
        /// </summary>
        /// <param name="enumValue">enum value</param>
        /// <returns>algorithm name, or null if failed</returns>
        public static string GetAlgorithmName(TEnum enumValue) {
            AlgorithmSpecAttribute spec;
            if (_enumToSpec.TryGetValue(enumValue, out spec)) {
                return spec.AlgorithmName;
            }
            return null;
        }

        /// <summary>
        /// Get default priority from <see cref="AlgorithmSpecAttribute"/> of the specified enum value.
        /// </summary>
        /// <param name="enumValue">enum value</param>
        /// <returns>default priority, or <see cref="Int32.MinValue"/> if failed</returns>
        public static int GetDefaultPriority(TEnum enumValue) {
            AlgorithmSpecAttribute spec;
            if (_enumToSpec.TryGetValue(enumValue, out spec)) {
                return spec.DefaultPriority;
            }
            return Int32.MinValue;
        }

        /// <summary>
        /// Get all algorithm names about <typeparamref name="TEnum"/> in priority order.
        /// </summary>
        /// <returns>algorithm names sorted by the default priority (high priority first)</returns>
        public static string[] GetAlgorithmNamesByPriorityOrder() {
            return _enumToSpec
                    .OrderByDescending(kv => kv.Value.DefaultPriority)
                    .Select(kv => kv.Value.AlgorithmName)
                    .ToArray();
        }

        /// <summary>
        /// Get all <typeparamref name="TEnum"/> vales in priority order.
        /// </summary>
        /// <returns>algorithms sorted by the default priority (high priority first)</returns>
        public static TEnum[] GetAlgorithmsByPriorityOrder() {
            return _enumToSpec
                    .OrderByDescending(kv => kv.Value.DefaultPriority)
                    .Select(kv => kv.Key)
                    .ToArray();
        }
    }

    /// <summary>
    /// <ja>SSHプロトコルの種類を示します</ja>
    /// <en>Kind of SSH protocol</en>
    /// </summary>
    public enum SSHProtocol {
        /// <summary>
        /// <ja>SSH1</ja>
        /// <en>SSH1</en>
        /// </summary>
        SSH1,
        /// <summary>
        /// <ja>SSH2</ja>
        /// <en>SSH2</en>
        /// </summary>
        SSH2
    }

    /// <summary>
    /// <ja>
    /// アルゴリズムの種類を示します。
    /// </ja>
    /// <en>
    /// Kind of algorithm
    /// </en>
    /// </summary>
    public enum CipherAlgorithm {
        /// <summary>
        /// TripleDES
        /// </summary>
        [AlgorithmSpec(AlgorithmName = "TripleDES", DefaultPriority = 1)]
        TripleDES = 3,
        /// <summary>
        /// BlowFish
        /// </summary>
        [AlgorithmSpec(AlgorithmName = "Blowfish", DefaultPriority = 2)]
        Blowfish = 6,
        /// <summary>
        /// <ja>AES128（SSH2のみ有効）</ja>
        /// <en>AES128（SSH2 only）</en>
        /// </summary>
        [AlgorithmSpec(AlgorithmName = "AES128", DefaultPriority = 3)]
        AES128 = 10,
        /// <summary>
        /// <ja>AES192（SSH2のみ有効）</ja>
        /// <en>AES192（SSH2 only）</en>
        /// </summary>
        [AlgorithmSpec(AlgorithmName = "AES192", DefaultPriority = 5)]
        AES192 = 11,
        /// <summary>
        /// <ja>AES256（SSH2のみ有効）</ja>
        /// <en>AES256（SSH2 only）</en>
        /// </summary>
        [AlgorithmSpec(AlgorithmName = "AES256", DefaultPriority = 7)]
        AES256 = 12,
        /// <summary>
        /// <ja>AES128-CTR（SSH2のみ有効）</ja>
        /// <en>AES128-CTR（SSH2 only）</en>
        /// </summary>
        [AlgorithmSpec(AlgorithmName = "AES128CTR", DefaultPriority = 4)]
        AES128CTR = 13,
        /// <summary>
        /// <ja>AES192-CTR（SSH2のみ有効）</ja>
        /// <en>AES192-CTR（SSH2 only）</en>
        /// </summary>
        [AlgorithmSpec(AlgorithmName = "AES192CTR", DefaultPriority = 6)]
        AES192CTR = 14,
        /// <summary>
        /// <ja>AES256-CTR（SSH2のみ有効）</ja>
        /// <en>AES256-CTR（SSH2 only）</en>
        /// </summary>
        [AlgorithmSpec(AlgorithmName = "AES256CTR", DefaultPriority = 8)]
        AES256CTR = 15,
    }

    /// <summary>
    /// Extension methods for <see cref="CipherAlgorithm"/>
    /// </summary>
    public static class CipherAlgorithmMixin {

        public static string GetAlgorithmName(this CipherAlgorithm value) {
            return AlgorithmSpecUtil<CipherAlgorithm>.GetAlgorithmName(value);
        }

        public static int GetDefaultPriority(this CipherAlgorithm value) {
            return AlgorithmSpecUtil<CipherAlgorithm>.GetDefaultPriority(value);
        }
    }

    /// <summary>
    /// <ja>認証方式を示します。</ja>
    /// <en>Kind of authentification method</en>
    /// </summary>
    public enum AuthenticationType {
        /// <summary>
        /// <ja>公開鍵方式</ja>
        /// <en>Public key cryptosystem</en>
        /// </summary>
        PublicKey = 2, //uses identity file
        /// <summary>
        /// <ja>パスワード方式</ja>
        /// <en>Password Authentication</en>
        /// </summary>
        Password = 3,
        /// <summary>
        /// <ja>キーボードインタラクティブ</ja>
        /// <en>KeyboardInteractive</en>
        /// </summary>
        KeyboardInteractive = 4
    }

    /// <summary>
    /// <ja>
    /// 認証状態
    /// </ja>
    /// <en>
    /// Status of the authentication.
    /// </en>
    /// </summary>
    public enum AuthenticationStatus {
        /// <summary>
        /// <ja>認証がまだ開始されていない</ja>
        /// <en>The authentication has not started yet.</en>
        /// </summary>
        NotStarted,

        /// <summary>
        /// <ja>認証に成功</ja>
        /// <en>The authentication has succeeded.</en>
        /// </summary>
        Success,

        /// <summary>
        /// <ja>認証に失敗</ja>
        /// <en>The authentication has failed.</en>
        /// </summary>
        Failure,

        /// <summary>
        /// <ja>認証のためキー入力が必要(キーボードインタラクティブ認証)</ja>
        /// <en>Need keyboard input for the authentication (keyboard interactive authentication)</en>
        /// </summary>
        NeedKeyboardInput,
    }

    /// <summary>
    /// Disconnection reason code
    /// </summary>
    public enum DisconnectionReasonCode {
        /// <summary>SSH_DISCONNECT_HOST_NOT_ALLOWED_TO_CONNECT (SSH2)</summary>
        HostNotAllowedToConnect = 1,
        /// <summary>SSH_DISCONNECT_PROTOCOL_ERROR (SSH2)</summary>
        ProtocolError = 2,
        /// <summary>SSH_DISCONNECT_KEY_EXCHANGE_FAILED (SSH2)</summary>
        KeyExchangeFailed = 3,
        /// <summary>SSH_DISCONNECT_RESERVED (SSH2)</summary>
        Reserved = 4,
        /// <summary>SSH_DISCONNECT_MAC_ERROR (SSH2)</summary>
        MacError = 5,
        /// <summary>SSH_DISCONNECT_COMPRESSION_ERROR (SSH2)</summary>
        CompressionError = 6,
        /// <summary>SSH_DISCONNECT_SERVICE_NOT_AVAILABLE (SSH2)</summary>
        ServiceNotAvailable = 7,
        /// <summary>SSH_DISCONNECT_PROTOCOL_VERSION_NOT_SUPPORTED (SSH2)</summary>
        ProtocolVersionNotSupported = 8,
        /// <summary>SSH_DISCONNECT_HOST_KEY_NOT_VERIFIABLE (SSH2)</summary>
        HostKeyNotVerifiable = 9,
        /// <summary>SSH_DISCONNECT_CONNECTION_LOST (SSH2)</summary>
        ConnectionLost = 10,
        /// <summary>SSH_DISCONNECT_BY_APPLICATION (SSH2)</summary>
        ByApplication = 11,
        /// <summary>SSH_DISCONNECT_TOO_MANY_CONNECTIONS (SSH2)</summary>
        TooManyConnections = 12,
        /// <summary>SSH_DISCONNECT_AUTH_CANCELLED_BY_USER (SSH2)</summary>
        AuthCancelledByUser = 13,
        /// <summary>SSH_DISCONNECT_NO_MORE_AUTH_METHODS_AVAILABLE (SSH2)</summary>
        NoMoreAuthMethodsAvailable = 14,
        /// <summary>SSH_DISCONNECT_ILLEGAL_USER_NAME (SSH2)</summary>
        IllegalUserName = 15
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public enum MACAlgorithm {
        HMACSHA1
    }

    /// <summary>
    /// <ja>鍵交換アルゴリズム</ja>
    /// <en>key exchange algorighm</en>
    /// </summary>
    /// <exclude/>
    public enum KexAlgorithm {
        /// <summary>diffie-hellman-group1-sha1 described in RFC4253</summary>
        [AlgorithmSpec(AlgorithmName = "diffie-hellman-group1-sha1", DefaultPriority = 1)]
        DH_G1_SHA1,
        /// <summary>diffie-hellman-group14-sha1 described in RFC4253</summary>
        [AlgorithmSpec(AlgorithmName = "diffie-hellman-group14-sha1", DefaultPriority = 2)]
        DH_G14_SHA1,
        /// <summary>diffie-hellman-group14-sha256 described in draft-ietf-curdle-ssh-kex-sha2</summary>
        [AlgorithmSpec(AlgorithmName = "diffie-hellman-group14-sha256", DefaultPriority = 3)]
        DH_G14_SHA256,
        /// <summary>diffie-hellman-group16-sha512 described in draft-ietf-curdle-ssh-kex-sha2</summary>
        [AlgorithmSpec(AlgorithmName = "diffie-hellman-group16-sha512", DefaultPriority = 5)]
        DH_G16_SHA512,
        /// <summary>diffie-hellman-group18-sha512 described in draft-ietf-curdle-ssh-kex-sha2</summary>
        [AlgorithmSpec(AlgorithmName = "diffie-hellman-group18-sha512", DefaultPriority = 4)]
        DH_G18_SHA512,
        /// <summary>ecdh-sha2-nistp256 described in RFC5656</summary>
        [AlgorithmSpec(AlgorithmName = "ecdh-sha2-nistp256", DefaultPriority = 8)]
        ECDH_SHA2_NISTP256,
        /// <summary>ecdh-sha2-nistp384 described in RFC5656</summary>
        [AlgorithmSpec(AlgorithmName = "ecdh-sha2-nistp384", DefaultPriority = 7)]
        ECDH_SHA2_NISTP384,
        /// <summary>ecdh-sha2-nistp521 described in RFC5656</summary>
        [AlgorithmSpec(AlgorithmName = "ecdh-sha2-nistp521", DefaultPriority = 6)]
        ECDH_SHA2_NISTP521,
    }

    /// <summary>
    /// Extension methods for <see cref="KexAlgorithm"/>.
    /// </summary>
    public static class KexAlgorithmMixin {

        public static string GetAlgorithmName(this KexAlgorithm value) {
            return AlgorithmSpecUtil<KexAlgorithm>.GetAlgorithmName(value);
        }

        public static int GetDefaultPriority(this KexAlgorithm value) {
            return AlgorithmSpecUtil<KexAlgorithm>.GetDefaultPriority(value);
        }

        public static bool IsECDH(this KexAlgorithm value) {
            string algorithmName = GetAlgorithmName(value);
            if (algorithmName != null && algorithmName.StartsWith("ecdh-")) {
                return true;
            }
            return false;
        }
    }

}

namespace Granados.Util {

    using System;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    internal static class SSHUtil {

        /// <summary>
        /// Get version string of the Granados.
        /// </summary>
        /// <param name="p">SSH protocol type</param>
        /// <returns>a version string</returns>
        public static string ClientVersionString(SSHProtocol p) {
            Assembly assy = Assembly.GetAssembly(typeof(SSHUtil));
            Version ver = assy.GetName().Version;
            string s = String.Format("{0}-{1}.{2}",
                            (p == SSHProtocol.SSH1) ? "SSH-1.5-Granados" : "SSH-2.0-Granados",
                            ver.Major, ver.Minor);
            return s;
        }

        /// <summary>
        /// Read Int32 value in network byte order.
        /// </summary>
        /// <param name="data">source byte array</param>
        /// <param name="offset">index to start reading</param>
        /// <returns>Int32 value</returns>
        public static int ReadInt32(byte[] data, int offset) {
            return (int)ReadUInt32(data, offset);
        }

        /// <summary>
        /// Read UInt32 value in network byte order.
        /// </summary>
        /// <param name="data">source byte array</param>
        /// <param name="offset">index to start reading</param>
        /// <returns>UInt32 value</returns>
        public static uint ReadUInt32(byte[] data, int offset) {
            uint ret = 0;
            ret |= data[offset];
            ret <<= 8;
            ret |= data[offset + 1];
            ret <<= 8;
            ret |= data[offset + 2];
            ret <<= 8;
            ret |= data[offset + 3];
            return ret;
        }

        /// <summary>
        /// Write Int32 value in network byte order.
        /// </summary>
        /// <param name="dst">byte array to be written</param>
        /// <param name="pos">index to start writing</param>
        /// <param name="data">Int32 value</param>
        public static void WriteIntToByteArray(byte[] dst, int pos, int data) {
            WriteUIntToByteArray(dst, pos, (uint)data);
        }

        /// <summary>
        /// Write UInt32 value in network byte order.
        /// </summary>
        /// <param name="dst">byte array to be written</param>
        /// <param name="pos">index to start writing</param>
        /// <param name="data">UInt32 value</param>
        public static void WriteUIntToByteArray(byte[] dst, int pos, uint data) {
            dst[pos] = (byte)(data >> 24);
            dst[pos + 1] = (byte)(data >> 16);
            dst[pos + 2] = (byte)(data >> 8);
            dst[pos + 3] = (byte)(data);
        }

        /// <summary>
        /// Check if a string array contains a particular string.
        /// </summary>
        /// <param name="s">a string array</param>
        /// <param name="v">a string</param>
        /// <returns>true if <paramref name="s"/> contains <paramref name="v"/>.</returns>
        public static bool ContainsString(string[] s, string v) {
            foreach (string x in s) {
                if (x == v) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the contents of two byte arrays are identical.
        /// </summary>
        /// <param name="d1">a byte array</param>
        /// <param name="d2">a byte array</param>
        /// <returns>true if the contents of two byte arrays are identical.</returns>
        public static bool ByteArrayEqual(byte[] d1, byte[] d2) {
            if (d1.Length != d2.Length) {
                return false;
            }
            return ByteArrayEqual(d1, 0, d2, 0, d1.Length);
        }

        /// <summary>
        /// Check if the partial contents of two byte arrays are identical.
        /// </summary>
        /// <param name="d1">a byte array</param>
        /// <param name="o1">index of <paramref name="d1"/> to start comparison</param>
        /// <param name="d2">a byte array</param>
        /// <param name="o2">index of <paramref name="d2"/> to start comparison</param>
        /// <param name="len">number of bytes to compare</param>
        /// <returns>true if the partial contents of two byte arrays are identical.</returns>
        public static bool ByteArrayEqual(byte[] d1, int o1, byte[] d2, int o2, int len) {
            for (int i = 0; i < len; ++i) {
                if (d1[o1++] != d2[o2++]) {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class Strings {
        private static StringResources _strings;
        public static string GetString(string id) {
            if (_strings == null)
                Reload();
            return _strings.GetString(id);
        }

        //load resource corresponding to current culture
        public static void Reload() {
            _strings = new StringResources("Granados.strings", typeof(Strings).Assembly);
        }
    }

    internal class DebugUtil {
        public static string DumpByteArray(byte[] data) {
            return DumpByteArray(data, 0, data.Length);
        }
        public static string DumpByteArray(byte[] data, int offset, int length) {
            StringBuilder bld = new StringBuilder();
            for (int i = 0; i < length; i++) {
                bld.Append(data[offset + i].ToString("X2"));
                if ((i % 4) == 3)
                    bld.Append(' ');
            }
            return bld.ToString();
        }

        public static string CurrentThread() {
            Thread t = Thread.CurrentThread;
            return t.GetHashCode().ToString();
        }
    }

    /// <summary>
    /// Utility class for pass an object to the single recipient.
    /// </summary>
    /// <typeparam name="T">type of the object</typeparam>
    internal class AtomicBox<T> {
        private readonly object _syncSet = new object();
        private readonly object _syncGet = new object();
        private readonly object _syncObject = new object();
        private volatile bool _hasObject;
        private T _object;

        /// <summary>
        /// Constructor
        /// </summary>
        public AtomicBox() {
            _hasObject = false;
            _object = default(T);
        }

        /// <summary>
        /// Clear the state of this box.
        /// </summary>
        public void Clear() {
            lock (_syncObject) {
                _object = default(T);
                _hasObject = false;
                Monitor.PulseAll(_syncObject);
            }
        }

        /// <summary>
        /// <para>Sets an object in the box.</para>
        /// <para>If another object exists in the box, the thread will be blocked until the object has been received by the recipient thread.</para>
        /// </summary>
        /// <param name="obj">an object to set</param>
        /// <param name="msecTimeout">timeout in milliseconds</param>
        /// <returns>true if an object has been set into the box.</returns>
        public bool TrySet(T obj, int msecTimeout) {
            lock (_syncSet) {
                lock (_syncObject) {
                    while (_hasObject) {
                        bool signaled = Monitor.Wait(_syncObject, msecTimeout);
                        if (_hasObject && !signaled) {
                            return false;
                        }
                    }
                    _object = obj;
                    _hasObject = true;
                    Monitor.PulseAll(_syncObject);
                    return true;
                }
            }
        }

        /// <summary>
        /// <para>Gets an object from the box.</para>
        /// <para>If no object exists in the box, the thread will be blocked until the object has been set by the sender thread.</para>
        /// </summary>
        /// <param name="obj">an object if succeeded</param>
        /// <param name="msecTimeout">timeout in milliseconds</param>
        /// <returns>true if an object has been obtained.</returns>
        public bool TryGet(ref T obj, int msecTimeout) {
            lock (_syncGet) {
                lock (_syncObject) {
                    while (!_hasObject) {
                        bool signaled = Monitor.Wait(_syncObject, msecTimeout);
                        if (!_hasObject && !signaled) {
                            return false;
                        }
                    }
                    obj = _object;
                    _object = default(T);
                    _hasObject = false;
                    Monitor.PulseAll(_syncObject);
                    return true;
                }
            }
        }
    }

    /// <summary>
    /// An internal class to pass the protocol events to <see cref="ISSHProtocolEventLogger"/>.
    /// </summary>
    internal class SSHProtocolEventManager {

        private readonly ISSHProtocolEventLogger _coreHandler;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="coreHandler">listener object or null</param>
        public SSHProtocolEventManager(ISSHProtocolEventLogger coreHandler) {
            _coreHandler = coreHandler;
        }

        /// <summary>
        /// Notifies OnSend event.
        /// </summary>
        /// <typeparam name="MessageTypeEnum">SSH message type enum</typeparam>
        /// <param name="messageType">message type</param>
        /// <param name="format">format string for the "details" text</param>
        /// <param name="args">format arguments for the "details" text</param>
        public void NotifySend<MessageTypeEnum>(MessageTypeEnum messageType, string format, params object[] args) {
            if (_coreHandler == null) {
                return;
            }

            try {
                string details = (args.Length == 0) ? format : String.Format(format, args);
                _coreHandler.OnSend(messageType.ToString(), details);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        /// Notifies OnReceived event.
        /// </summary>
        /// <typeparam name="MessageTypeEnum">SSH message type enum</typeparam>
        /// <param name="messageType">message type</param>
        /// <param name="format">format string for the "details" text</param>
        /// <param name="args">format arguments for the "details" text</param>
        public void NotifyReceive<MessageTypeEnum>(MessageTypeEnum messageType, string format, params object[] args) {
            if (_coreHandler == null) {
                return;
            }

            try {
                string details = (args.Length == 0) ? format : String.Format(format, args);
                _coreHandler.OnReceived(messageType.ToString(), details);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        /// Notifies OnTrace event.
        /// </summary>
        /// <param name="format">format string for the "details" text</param>
        /// <param name="args">format arguments for the "details" text</param>
        public void Trace(string format, params object[] args) {
            if (_coreHandler == null) {
                return;
            }

            try {
                string details = (args.Length == 0) ? format : String.Format(format, args);
                _coreHandler.OnTrace(details);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }
    }

}
