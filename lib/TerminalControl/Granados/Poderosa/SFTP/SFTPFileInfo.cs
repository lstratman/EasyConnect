/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SFTPFileInfo.cs,v 1.1 2011/11/14 14:01:52 kzmi Exp $
 */
using System;

namespace Granados.Poderosa.SFTP {

    /// <summary>
    /// SFTP remote file informations
    /// </summary>
    public class SFTPFileInfo : SFTPFileAttributes {

        private readonly string _fileName;
        private readonly string _longName;

        /// <summary>
        /// File name
        /// </summary>
        public string FileName {
            get {
                return _fileName;
            }
        }

        /// <summary>
        /// Long format line. Commonly this will be a result of ls -l.
        /// </summary>
        public string LongName {
            get {
                return _longName;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="longName">long name</param>
        /// <param name="attributes">attributes</param>
        public SFTPFileInfo(string fileName, string longName, SFTPFileAttributes attributes)
            : base(attributes) {

            this._fileName = fileName;
            this._longName = longName;
        }
    }
}
