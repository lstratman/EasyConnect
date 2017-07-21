/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SFTPStatusCodes.cs,v 1.1 2011/12/04 10:27:14 kzmi Exp $
 */
using System;
using System.Globalization;

namespace Granados.Poderosa.SFTP {

    /// <summary>
    /// SFTP status code
    /// </summary>
    /// <remarks>
    /// <para>Referenced: SSH File Transfer Protocol Internet-Draft Section 7. Responses from the Server to the Client</para>
    /// </remarks>
    public static class SFTPStatusCode {

        /// <summary>
        /// Indicates successful completion of the operation.
        /// </summary>
        public const int SSH_FX_OK = 0;

        /// <summary>
        /// indicates end-of-file condition.
        /// </summary>
        public const int SSH_FX_EOF = 1;

        /// <summary>
        /// returned when a reference is made to a file which should exist but doesn't.
        /// </summary>
        public const int SSH_FX_NO_SUCH_FILE = 2;

        /// <summary>
        /// returned when the authenticated user does not have sufficient permissions to perform the operation.
        /// </summary>
        public const int SSH_FX_PERMISSION_DENIED = 3;

        /// <summary>
        /// a generic catch-all error message.
        /// </summary>
        public const int SSH_FX_FAILURE = 4;

        /// <summary>
        /// may be returned if a badly formatted packet or protocol incompatibility is detected.
        /// </summary>
        public const int SSH_FX_BAD_MESSAGE = 5;

        /// <summary>
        /// a pseudo-error which indicates that the client has no connection to the server
        /// (it can only be generated locally by the client, and MUST NOT be returned by servers).
        /// </summary>
        public const int SSH_FX_NO_CONNECTION = 6;

        /// <summary>
        /// a pseudo-error which indicates that the connection to the server has been lost
        /// (it can only be generated locally by the client, and MUST NOT be returned by servers).
        /// </summary>
        public const int SSH_FX_CONNECTION_LOST = 7;

        /// <summary>
        /// indicates that an attempt was made to perform an operation which is not supported for the server
        /// (it may be generated locally by the client if e.g. the version number exchange indicates that
        /// a required feature is not supported by the server, or it may be returned by the server
        /// if the server does not implement an operation).
        /// </summary>
        public const int SSH_FX_OP_UNSUPPORTED = 8;


        /// <summary>
        /// Gets the text representing a specified status code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string ToString(int code) {
            switch (code) {
                case SSH_FX_OK:
                    return "SSH_FX_OK";
                case SSH_FX_EOF:
                    return "SSH_FX_EOF";
                case SSH_FX_NO_SUCH_FILE:
                    return "SSH_FX_NO_SUCH_FILE";
                case SSH_FX_PERMISSION_DENIED:
                    return "SSH_FX_PERMISSION_DENIED";
                case SSH_FX_FAILURE:
                    return "SSH_FX_FAILURE";
                case SSH_FX_BAD_MESSAGE:
                    return "SSH_FX_BAD_MESSAGE";
                case SSH_FX_NO_CONNECTION:
                    return "SSH_FX_NO_CONNECTION";
                case SSH_FX_CONNECTION_LOST:
                    return "SSH_FX_CONNECTION_LOST";
                case SSH_FX_OP_UNSUPPORTED:
                    return "SSH_FX_OP_UNSUPPORTED";
                default:
                    return code.ToString(NumberFormatInfo.InvariantInfo);
            }
        }

    }

}
