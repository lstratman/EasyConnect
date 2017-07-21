/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SFTPPacketType.cs,v 1.1 2011/11/14 14:01:52 kzmi Exp $
 */
using System;

namespace Granados.Poderosa.SFTP {

    /// <summary>
    /// SFTP packet type
    /// </summary>
    internal enum SFTPPacketType : byte {
        SSH_FXP_INIT = 1,
        SSH_FXP_VERSION = 2,
        SSH_FXP_OPEN = 3,
        SSH_FXP_CLOSE = 4,
        SSH_FXP_READ = 5,
        SSH_FXP_WRITE = 6,
        SSH_FXP_LSTAT = 7,
        SSH_FXP_FSTAT = 8,
        SSH_FXP_SETSTAT = 9,
        SSH_FXP_FSETSTAT = 10,
        SSH_FXP_OPENDIR = 11,
        SSH_FXP_READDIR = 12,
        SSH_FXP_REMOVE = 13,
        SSH_FXP_MKDIR = 14,
        SSH_FXP_RMDIR = 15,
        SSH_FXP_REALPATH = 16,
        SSH_FXP_STAT = 17,
        SSH_FXP_RENAME = 18,
        SSH_FXP_READLINK = 19,
        SSH_FXP_SYMLINK = 20,
        SSH_FXP_STATUS = 101,
        SSH_FXP_HANDLE = 102,
        SSH_FXP_DATA = 103,
        SSH_FXP_NAME = 104,
        SSH_FXP_ATTRS = 105,
        SSH_FXP_EXTENDED = 200,
        SSH_FXP_EXTENDED_REPLY = 201,
    }
}
