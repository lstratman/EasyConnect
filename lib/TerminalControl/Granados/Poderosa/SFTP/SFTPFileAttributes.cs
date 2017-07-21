/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SFTPFileAttributes.cs,v 1.1 2011/11/14 14:01:52 kzmi Exp $
 */
using System;

namespace Granados.Poderosa.SFTP {

    /// <summary>
    /// SFTP remote file attribute informations
    /// </summary>
    public class SFTPFileAttributes {

        private readonly ulong _fileSize;
        private readonly uint _uid;
        private readonly uint _gid;
        private readonly uint _permissions;
        private readonly uint _atime;
        private readonly uint _mtime;

        /// <summary>
        /// File size
        /// </summary>
        public ulong FileSize {
            get {
                return _fileSize;
            }
        }

        /// <summary>
        /// User ID
        /// </summary>
        public uint UID {
            get {
                return _uid;
            }
        }

        /// <summary>
        /// Group ID
        /// </summary>
        public uint GID {
            get {
                return _gid;
            }
        }

        /// <summary>
        /// Unix permissions
        /// </summary>
        public uint Permissions {
            get {
                return _permissions;
            }
        }

        /// <summary>
        /// Access time
        /// </summary>
        public uint Atime {
            get {
                return _atime;
            }
        }

        /// <summary>
        /// Modification time
        /// </summary>
        public uint Mtime {
            get {
                return _mtime;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileSize">file size</param>
        /// <param name="uid">user identifier</param>
        /// <param name="gid">group identifier</param>
        /// <param name="permissions">permissions</param>
        /// <param name="atime">access time</param>
        /// <param name="mtime">modification time</param>
        public SFTPFileAttributes(ulong fileSize, uint uid, uint gid, uint permissions, uint atime, uint mtime) {
            this._fileSize = fileSize;
            this._uid = uid;
            this._gid = gid;
            this._permissions = permissions;
            this._atime = atime;
            this._mtime = mtime;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="attributes">attributes</param>
        public SFTPFileAttributes(SFTPFileAttributes attributes) {
            this._fileSize = attributes._fileSize;
            this._uid = attributes._uid;
            this._gid = attributes._gid;
            this._permissions = attributes._permissions;
            this._atime = attributes._atime;
            this._mtime = attributes._mtime;
        }
    }

}
