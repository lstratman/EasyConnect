/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SFTPClientException.cs,v 1.1 2011/11/14 14:01:52 kzmi Exp $
 */
using System;
using System.Text;

using Granados.IO;

namespace Granados.Poderosa.SFTP {

    /// <summary>
    /// Common exception class thrown by SFTPClient
    /// </summary>
    public class SFTPClientException : Exception {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">exception message</param>
        public SFTPClientException(string message)
            : base(message) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="innerException">inner exception</param>
        public SFTPClientException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }

    /// <summary>
    /// Exception thrown when timeout occurred.
    /// </summary>
    public class SFTPClientTimeoutException : SFTPClientException {
        /// <summary>
        /// Constructor
        /// </summary>
        public SFTPClientTimeoutException()
            : base("operation timeout") {
        }
    }

    /// <summary>
    /// Exception thrown when inoperable channel status.
    /// </summary>
    public class SFTPClientInvalidStatusException : SFTPClientException {
        /// <summary>
        /// Constructor
        /// </summary>
        public SFTPClientInvalidStatusException()
            : base("inoperable channel status") {
        }
    }

    /// <summary>
    /// Exception representing error informations that reported by SSH_FXP_STATUS.
    /// </summary>
    public class SFTPClientErrorException : SFTPClientException {

        private readonly uint _id;
        private readonly uint _code;
        private readonly string _languageTag;

        /// <summary>
        /// SFTP request ID
        /// </summary>
        public uint ID {
            get {
                return _id;
            }
        }

        /// <summary>
        /// SFTP error / status code
        /// </summary>
        public uint Code {
            get {
                return _code;
            }
        }

        /// <summary>
        /// Language tag
        /// </summary>
        public string LanguageTag {
            get {
                return _languageTag;
            }
        }

        /// <summary>
        /// Create new instance from SSH_FXP_STATUS packet data.
        /// </summary>
        /// <param name="dataReader">data reader</param>
        /// <returns>new instance</returns>
        internal static SFTPClientErrorException Create(SSHDataReader dataReader) {
            uint id = (uint)dataReader.ReadInt32();
            uint code = (uint)dataReader.ReadInt32();
            string message = dataReader.ReadUTF8String();
            string languageTag = dataReader.ReadString();

            return new SFTPClientErrorException(id, code, message, languageTag);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected SFTPClientErrorException(uint id, uint code, string message, string languageTag)
            : base(message) {
            this._id = id;
            this._code = code;
            this._languageTag = languageTag;
        }
    }

}
