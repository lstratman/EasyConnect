/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ProtocolOptions.cs,v 1.5 2012/03/18 11:02:29 kzmi Exp $
 */
using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Text;

using Poderosa.Util;
using Poderosa.Preferences;

namespace Poderosa.Protocols {
    /// <summary>
    /// <ja>
    /// IPv4とIPv6の優先順位を決めます。
    /// </ja>
    /// <en>
    /// Decide the priority level of IPv4 and IPv6
    /// </en>
    /// </summary>
    public enum IPVersionPriority {
        /// <summary>
        /// <ja>IPv4とIPv6の両方を使います。</ja>
        /// <en>Both IPv4 and IPv6 are used.</en>
        /// </summary>
        [EnumValue(Description = "Enum.IPVersionPriority.Both")]
        Both,
        /// <summary>
        /// <ja>IPv4しか使いません。</ja>
        /// <en>Only IPv4 is used.</en>
        /// </summary>
        [EnumValue(Description = "Enum.IPVersionPriority.V4Only")]
        V4Only,
        /// <summary>
        /// <ja>
        /// IPv6しか使いません。
        /// </ja>
        /// <en>Only IPv6 is used.</en>
        /// </summary>
        [EnumValue(Description = "Enum.IPVersionPriority.V6Only")]
        V6Only
    }

    /// <summary>
    /// <ja>
    /// 接続オプションを提供するインターフェイスです。
    /// </ja>
    /// <en>
    /// It is an interface that offers connected option. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>このインターフェイスの解説は、まだありません。</ja><en>It has not explained this interface yet. </en>
    /// </remarks>
    public interface IProtocolOptions {
        string[] CipherAlgorithmOrder {
            get;
            set;
        }
        string[] HostKeyAlgorithmOrder {
            get;
            set;
        }
        int SSHWindowSize {
            get;
            set;
        }
        bool SSHCheckMAC {
            get;
            set;
        }

        bool RetainsPassphrase {
            get;
            set;
        }
        int SocketConnectTimeout {
            get;
            set;
        }
        bool UseSocks {
            get;
            set;
        }
        string SocksServer {
            get;
            set;
        }
        int SocksPort {
            get;
            set;
        }
        string SocksAccount {
            get;
            set;
        }
        string SocksPassword {
            get;
            set;
        }
        string SocksNANetworks {
            get;
            set;
        }
        string HostKeyCheckerVerifierTypeName {
            get;
            set;
        }
        IPVersionPriority IPVersionPriority {
            get;
            set;
        }
        bool LogSSHEvents {
            get;
            set;
        }

        //PreferenceEditorのみ
        int SocketBufferSize {
            get;
        }
        bool ReadSerializedPassword {
            get;
        }
        bool SavePassword {
            get;
        }
        bool SavePlainTextPassword {
            get;
        }
    }

    internal class ProtocolOptions : SnapshotAwarePreferenceBase, IProtocolOptions {

        //SSH
        private IBoolPreferenceItem _retainsPassphrase;
        private IStringPreferenceItem _cipherAlgorithmOrder;
        private IStringPreferenceItem _hostKeyAlgorithmOrder;
        private IIntPreferenceItem _sshWindowSize;
        private IBoolPreferenceItem _sshCheckMAC;
        private IStringPreferenceItem _hostKeyCheckerVerifierTypeName;

        //ソケット
        private IIntPreferenceItem _socketConnectTimeout;
        private EnumPreferenceItem<IPVersionPriority> _ipVersionPriority;
        private IBoolPreferenceItem _logSSHEvents;

        //SOCKS関係
        private IBoolPreferenceItem _useSocks;
        private IStringPreferenceItem _socksServer;
        private IIntPreferenceItem _socksPort;
        private IStringPreferenceItem _socksAccount;
        private IStringPreferenceItem _socksPassword;
        private IStringPreferenceItem _socksNANetworks;

        //PreferenceEditorのみ
        private IIntPreferenceItem _socketBufferSize;
        private IBoolPreferenceItem _readSerializedPassword;
        private IBoolPreferenceItem _savePassword;
        private IBoolPreferenceItem _savePlainTextPassword;

        public ProtocolOptions(IPreferenceFolder folder)
            : base(folder) {
        }

        public override void DefineItems(IPreferenceBuilder builder) {
            //SSH関係
            _retainsPassphrase = builder.DefineBoolValue(_folder, "retainPassphrase", false, null);
            //Note: Validator Required
            _cipherAlgorithmOrder = builder.DefineStringValue(_folder, "cipherAlgorithmOrder", "", null);
            FixCipherAlgorithms(_cipherAlgorithmOrder);
            _hostKeyAlgorithmOrder = builder.DefineStringValue(_folder, "hostKeyAlgorithmOrder", "", null);
            FixHostKeyAlgorithms(_hostKeyAlgorithmOrder);
            _sshWindowSize = builder.DefineIntValue(_folder, "sshWindowSize", 2097152, PreferenceValidatorUtil.PositiveIntegerValidator);
            _sshCheckMAC = builder.DefineBoolValue(_folder, "sshCheckMAC", true, null);
            _hostKeyCheckerVerifierTypeName = builder.DefineStringValue(_folder, "hostKeyCheckerVerifierTypeName", "Poderosa.Usability.SSHKnownHosts", null);
            _logSSHEvents = builder.DefineBoolValue(_folder, "logSSHEvents", false, null);
            _socketConnectTimeout = builder.DefineIntValue(_folder, "socketConnectTimeout", 3000, PreferenceValidatorUtil.PositiveIntegerValidator);
            _ipVersionPriority = new EnumPreferenceItem<IPVersionPriority>(builder.DefineStringValue(_folder, "ipVersionPriority", "Both", null), IPVersionPriority.Both);

            //SOCKS関係
            _useSocks = builder.DefineBoolValue(_folder, "useSocks", false, null);
            _socksServer = builder.DefineStringValue(_folder, "socksServer", "", null);
            _socksPort = builder.DefineIntValue(_folder, "socksPort", 1080, PreferenceValidatorUtil.PositiveIntegerValidator);
            _socksAccount = builder.DefineStringValue(_folder, "socksAccount", "", null);
            _socksPassword = builder.DefineStringValue(_folder, "socksPassword", "", null);
            _socksNANetworks = builder.DefineStringValue(_folder, "socksNANetworks", "", null);

            //PreferenceEditorのみ
            _socketBufferSize = builder.DefineIntValue(_folder, "socketBufferSize", 0x1000, PreferenceValidatorUtil.PositiveIntegerValidator);
            _readSerializedPassword = builder.DefineBoolValue(_folder, "readSerializedPassword", false, null);
            _savePassword = builder.DefineBoolValue(_folder, "savePassword", false, null);
            _savePlainTextPassword = builder.DefineBoolValue(_folder, "savePlainTextPassword", false, null);
        }
        public ProtocolOptions Import(ProtocolOptions src) {
            Debug.Assert(src._folder.Id == _folder.Id);

            //SSH関係
            _retainsPassphrase = ConvertItem(src._retainsPassphrase);

            _cipherAlgorithmOrder = ConvertItem(src._cipherAlgorithmOrder);
            FixCipherAlgorithms(_cipherAlgorithmOrder);
            _hostKeyAlgorithmOrder = ConvertItem(src._hostKeyAlgorithmOrder);
            FixHostKeyAlgorithms(_hostKeyAlgorithmOrder);
            _sshWindowSize = ConvertItem(src._sshWindowSize);
            _sshCheckMAC = ConvertItem(src._sshCheckMAC);
            _hostKeyCheckerVerifierTypeName = ConvertItem(src._hostKeyCheckerVerifierTypeName);
            _logSSHEvents = ConvertItem(src._logSSHEvents);

            _socketConnectTimeout = ConvertItem(src._socketConnectTimeout);
            _ipVersionPriority = ConvertItem<IPVersionPriority>(src._ipVersionPriority);

            //SOCKS関係
            _useSocks = ConvertItem(src._useSocks);
            _socksServer = ConvertItem(src._socksServer);
            _socksPort = ConvertItem(src._socksPort);
            _socksAccount = ConvertItem(src._socksAccount);
            _socksPassword = ConvertItem(src._socksPassword);
            _socksNANetworks = ConvertItem(src._socksNANetworks);

            _socketBufferSize = ConvertItem(src._socketBufferSize);
            _readSerializedPassword = ConvertItem(src._readSerializedPassword);
            _savePassword = ConvertItem(src._savePassword);
            _savePlainTextPassword = ConvertItem(src._savePlainTextPassword);

            return this;
        }


        public string[] CipherAlgorithmOrder {
            get {
                return _cipherAlgorithmOrder.Value.Split(';');
            }
            set {
                _cipherAlgorithmOrder.Value = String.Join(";", FixCipherAlgorithms(value));
            }
        }

        public string[] HostKeyAlgorithmOrder {
            get {
                return _hostKeyAlgorithmOrder.Value.Split(';');
            }
            set {
                _hostKeyAlgorithmOrder.Value = String.Join(";", FixHostKeyAlgorithms(value));
            }
        }
        public int SSHWindowSize {
            get {
                return _sshWindowSize.Value;
            }
            set {
                _sshWindowSize.Value = value;
            }
        }
        public bool SSHCheckMAC {
            get {
                return _sshCheckMAC.Value;
            }
            set {
                _sshCheckMAC.Value = value;
            }
        }

        public bool RetainsPassphrase {
            get {
                return _retainsPassphrase.Value;
            }
            set {
                _retainsPassphrase.Value = value;
            }
        }
        public int SocketConnectTimeout {
            get {
                return _socketConnectTimeout.Value;
            }
            set {
                _socketConnectTimeout.Value = value;
            }
        }

        public bool UseSocks {
            get {
                return _useSocks.Value;
            }
            set {
                _useSocks.Value = value;
            }
        }
        public string SocksServer {
            get {
                return _socksServer.Value;
            }
            set {
                _socksServer.Value = value;
            }
        }
        public int SocksPort {
            get {
                return _socksPort.Value;
            }
            set {
                _socksPort.Value = value;
            }
        }
        public string SocksAccount {
            get {
                return _socksAccount.Value;
            }
            set {
                _socksAccount.Value = value;
            }
        }
        public string SocksPassword {
            get {
                return _socksPassword.Value;
            }
            set {
                _socksPassword.Value = value;
            }
        }
        public string SocksNANetworks {
            get {
                return _socksNANetworks.Value;
            }
            set {
                _socksNANetworks.Value = value;
            }
        }
        public string HostKeyCheckerVerifierTypeName {
            get {
                return _hostKeyCheckerVerifierTypeName.Value;
            }
            set {
                _hostKeyCheckerVerifierTypeName.Value = value;
            }
        }
        public IPVersionPriority IPVersionPriority {
            get {
                return _ipVersionPriority.Value;
            }
            set {
                _ipVersionPriority.Value = value;
            }
        }
        public bool LogSSHEvents {
            get {
                return _logSSHEvents.Value;
            }
            set {
                _logSSHEvents.Value = value;
            }
        }

        public int SocketBufferSize {
            get {
                return _socketBufferSize.Value;
            }
        }
        public bool ReadSerializedPassword {
            get {
                return _readSerializedPassword.Value;
            }
        }
        public bool SavePassword {
            get {
                return _savePassword.Value;
            }
        }
        public bool SavePlainTextPassword {
            get {
                return _savePlainTextPassword.Value;
            }
        }

        private void FixCipherAlgorithms(IStringPreferenceItem item) {
            item.Value = FixCipherAlgorithms(item.Value);
        }

        private string FixCipherAlgorithms(string algorithms) {
            if (algorithms == null) {
                return null;
            }
            return String.Join(";", FixCipherAlgorithms(algorithms.Split(';')));
        }

        private string[] FixCipherAlgorithms(string[] algorithms) {
            if (algorithms == null) {
                return null;
            }

            var algorithmVals =
                LocalSSHUtil.AppendMissingCipherAlgorithm(
                    LocalSSHUtil.ParseCipherAlgorithm(algorithms));

            return algorithmVals.Select(a => a.ToString()).ToArray();
        }

        private void FixHostKeyAlgorithms(IStringPreferenceItem item) {
            item.Value = FixHostKeyAlgorithms(item.Value);
        }

        private string FixHostKeyAlgorithms(string algorithms) {
            if (algorithms == null) {
                return null;
            }
            return String.Join(";", FixHostKeyAlgorithms(algorithms.Split(';')));
        }

        private string[] FixHostKeyAlgorithms(string[] algorithms) {
            if (algorithms == null) {
                return null;
            }

            var algorithmVals =
                LocalSSHUtil.AppendMissingPublicKeyAlgorithm(
                    LocalSSHUtil.ParsePublicKeyAlgorithm(algorithms));

            return algorithmVals.Select(a => a.ToString()).ToArray();
        }
    }


    internal class ProtocolOptionsSupplier : IPreferenceSupplier, IAdaptable {
        private ProtocolOptions _originalOptions;
        private IPreferenceFolder _originalFolder;

        /*
        //SSH
        [ConfigBoolElement(Initial = false)]
        protected bool _retainsPassphrase;
        [ConfigStringArrayElement(Initial = new string[] { "AES128", "Blowfish", "TripleDES" })]
        protected string[] _cipherAlgorithmOrder;
        [ConfigStringArrayElement(Initial = new string[] { "DSA", "RSA" })]
        protected string[] _hostKeyAlgorithmOrder;
        [ConfigIntElement(Initial = 4096)]
        protected int _sshWindowSize;
        [ConfigBoolElement(Initial = true)]
        protected bool _sshCheckMAC;

        //SOCKS関係
        [ConfigBoolElement(Initial = false)]
        protected bool _useSocks;
        [ConfigStringElement(Initial = "")]
        protected string _socksServer;
        [ConfigIntElement(Initial = 1080)]
        protected int _socksPort;
        [ConfigStringElement(Initial = "")]
        protected string _socksAccount;
        [ConfigStringElement(Initial = "")]
        protected string _socksPassword;
        [ConfigStringElement(Initial = "")]
        protected string _socksNANetworks;
        */


        //IPreferenceSupplier

        public string PreferenceID {
            get {
                return ProtocolsPlugin.PLUGIN_ID;
            }
        }

        public void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
            _originalFolder = folder;
            _originalOptions = new ProtocolOptions(folder);
            _originalOptions.DefineItems(builder);
        }

        public object QueryAdapter(IPreferenceFolder folder, Type type) {
            Debug.Assert(folder.Id == _originalFolder.Id);
            if (type == typeof(IProtocolOptions))
                return _originalFolder == folder ? _originalOptions : new ProtocolOptions(folder).Import(_originalOptions);
            else
                return null;
        }

        public string GetDescription(IPreferenceItem item) {
            return "";
        }

        public void ValidateFolder(IPreferenceFolder folder, IPreferenceValidationResult output) {
            //TODO
        }

        //IAdaptable
        public IAdaptable GetAdapter(Type adapter) {
            return ProtocolsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        public IProtocolOptions OriginalOptions {
            get {
                return _originalOptions;
            }
        }
    }
}
