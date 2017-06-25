/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CommunicationUtil.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

using Granados;
using Poderosa.Util;
using Poderosa.Forms;

namespace Poderosa.Protocols {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class CommunicationUtil {
        //cygwinの同期的接続
        public static ITerminalConnection CreateNewLocalShellConnection(IPoderosaForm form, ICygwinParameter param) {
            return LocalShellUtil.PrepareSocket(form, param);
        }


    }
}
