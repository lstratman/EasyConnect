// Copyright (c) 2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using System;

namespace Granados.X11Forwarding {

    /// <summary>
    /// X11 Forwarding Parameters
    /// </summary>
    public class X11ForwardingParams {

        /// <summary>
        /// Display number of the local X server.
        /// </summary>
        public int Display {
            get;
            private set;
        }

        /// <summary>
        /// Screen number
        /// </summary>
        /// <remarks>
        /// This property specifies which screen is used by the X client.<br/>
        /// Default is null.
        /// </remarks>
        public int Screen {
            get;
            set;
        }

        /// <summary>
        /// Specifies whether the X server needs authorization.
        /// </summary>
        /// <remarks>
        /// If this property was true, an entry in the .Xauthority file is used for the authorization.<br/>
        /// Default is false.
        /// </remarks>
        /// <seealso cref="XauthorityFile"/>
        public bool NeedAuth {
            get;
            set;
        }

        /// <summary>
        /// Path to the .Xauthority file.
        /// </summary>
        /// <remarks>
        /// This property should be set if <see cref="NeedAuth"/> was set to true.<br/>
        /// If this value was null or empty, $HOME/.Xauthority is used.<br/>
        /// Default is null.
        /// </remarks>
        public string XauthorityFile {
            get;
            set;
        }

        /// <summary>
        /// Use Cygwin's unix domain socket instead of the TCP connection to the X server's standard TCP port.
        /// </summary>
        /// <remarks>
        /// <see cref="X11UnixFolder"/> must be set for finding a domain socket file.<br/>
        /// Default is false;
        /// </remarks>
        /// <seealso cref="X11UnixFolder"/>
        public bool UseCygwinUnixDomainSocket {
            get;
            set;
        }

        /// <summary>
        /// Path to the folder that the unix domain socket files are created.
        /// </summary>
        /// <remarks>
        /// Generally, this property will be (cygwin-root)<code>/tmp/.X11-unix</code>.<br/>
        /// This property must be set if <see cref="UseCygwinUnixDomainSocket"/> was set to true.<br/>
        /// If this value was null or empty, using unix domain socket will fail.<br/>
        /// Default is null.
        /// </remarks>
        public string X11UnixFolder {
            get;
            set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="display">display number of the local X server.</param>
        public X11ForwardingParams(int display) {
            this.Display = display;
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns>new instance</returns>
        public X11ForwardingParams Clone() {
            return (X11ForwardingParams)this.MemberwiseClone();
        }
    }

}
