// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

namespace Granados.KnownHosts {

    //
    // Interfaces for delegating process that verify a host key with known_hosts file.
    //

    /// <summary>
    /// An interface that provides informations about a host key.
    /// </summary>
    public interface ISSHHostKeyInformationProvider {

        /// <summary>
        /// SSH protocol version.
        /// </summary>
        SSHProtocol Protocol {
            get;
        }

        /// <summary>
        /// Host name.
        /// </summary>
        string HostName {
            get;
        }

        /// <summary>
        /// Port number.
        /// </summary>
        int PortNumber {
            get;
        }

        /// <summary>
        /// A string in ssh_known_hosts format of the Poderosa.
        /// </summary>
        string KnownHostsString {
            get;
        }

        /// <summary>
        /// Finger print of the host key.
        /// </summary>
        byte[] HostKeyFingerPrint {
            get;
        }
    }

    /// <summary>
    /// A delegate to the function that verify a host key with known_hosts file.
    /// </summary>
    /// <param name="info">informations about a hot key.</param>
    /// <returns>true to success, or false to failure.</returns>
    public delegate bool VerifySSHHostKeyDelegate(ISSHHostKeyInformationProvider info);

}
