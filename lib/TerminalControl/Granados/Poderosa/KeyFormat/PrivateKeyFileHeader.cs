/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PrivateKeyFileHeader.cs,v 1.1 2011/11/03 16:27:38 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Granados.Poderosa.KeyFormat {

    /// <summary>
    /// Header and footer of the SSH private key file
    /// </summary>
    internal static class PrivateKeyFileHeader {

        public const string SSH1_HEADER = "SSH PRIVATE KEY FILE FORMAT 1.1\n";
        public const string SSH2_OPENSSH_HEADER_RSA = "-----BEGIN RSA PRIVATE KEY-----";
        public const string SSH2_OPENSSH_HEADER_DSA = "-----BEGIN DSA PRIVATE KEY-----";
        public const string SSH2_OPENSSH_HEADER_ECDSA = "-----BEGIN EC PRIVATE KEY-----";
        public const string SSH2_OPENSSH_HEADER_OPENSSH = "-----BEGIN OPENSSH PRIVATE KEY-----";
        public const string SSH2_SSHCOM_HEADER = "---- BEGIN SSH2 ENCRYPTED PRIVATE KEY ----";
        public const string SSH2_SSHCOM_FOOTER = "---- END SSH2 ENCRYPTED PRIVATE KEY ----";
        public const string SSH2_PUTTY_HEADER_1 = "PuTTY-User-Key-File-1:";
        public const string SSH2_PUTTY_HEADER_2 = "PuTTY-User-Key-File-2:";

    }

}
