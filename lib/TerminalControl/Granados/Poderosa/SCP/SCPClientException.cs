/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SCPClientException.cs,v 1.1 2011/11/22 09:04:13 kzmi Exp $
 */
using System;
using System.Text;

using Granados.IO;

namespace Granados.Poderosa.SCP {

    /// <summary>
    /// Common exception class thrown by SCPClient
    /// </summary>
    public class SCPClientException : Exception {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">exception message</param>
        public SCPClientException(string message)
            : base(message) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="innerException">inner exception</param>
        public SCPClientException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }

    /// <summary>
    /// Exception thrown when timeout occurred.
    /// </summary>
    public class SCPClientTimeoutException : SCPClientException {
        /// <summary>
        /// Constructor
        /// </summary>
        public SCPClientTimeoutException()
            : base("operation timeout") {
        }
    }

    /// <summary>
    /// Exception thrown when inoperable channel status.
    /// </summary>
    public class SCPClientInvalidStatusException : SCPClientException {
        /// <summary>
        /// Constructor
        /// </summary>
        public SCPClientInvalidStatusException()
            : base("inoperable channel status") {
        }
    }

}
