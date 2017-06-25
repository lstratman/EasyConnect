// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using System;
using Granados.PKI;

namespace Granados.SSH2 {

    /// <summary>
    /// SSH_MSG_CHANNEL_OPEN_FAILURE reason code
    /// </summary>
    internal static class SSH2ChannelOpenFailureCode {
        public const uint SSH_OPEN_ADMINISTRATIVELY_PROHIBITED = 1;
        public const uint SSH_OPEN_CONNECT_FAILED = 2;
        public const uint SSH_OPEN_UNKNOWN_CHANNEL_TYPE = 3;
        public const uint SSH_OPEN_RESOURCE_SHORTAGE = 4;
    }

}
