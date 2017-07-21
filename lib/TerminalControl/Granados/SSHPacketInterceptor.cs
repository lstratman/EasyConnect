// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Granados.SSH {

    /// <summary>
    /// Return value of the <see cref="ISSHPacketInterceptor.InterceptPacket(DataFragment)"/>.
    /// </summary>
    internal enum SSHPacketInterceptorResult {
        /// <summary>the packet was not consumed</summary>
        PassThrough,
        /// <summary>the packet was consumed</summary>
        Consumed,
        /// <summary>the packet was not consumed. the interceptor has already finished.</summary>
        Finished,
    }

    /// <summary>
    /// An interface of a class that can intercept a received packet.
    /// </summary>
    internal interface ISSHPacketInterceptor {
        SSHPacketInterceptorResult InterceptPacket(DataFragment packet);
        void OnConnectionClosed();
    }

    /// <summary>
    /// Collection of the <see cref="ISSHPacketInterceptor"/>.
    /// </summary>
    internal class SSHPacketInterceptorCollection {

        private readonly LinkedList<ISSHPacketInterceptor> _interceptors = new LinkedList<ISSHPacketInterceptor>();

        /// <summary>
        /// Add packet interceptor to the collection.
        /// </summary>
        /// <remarks>
        /// Do nothing if the packet interceptor already exists in the collection.
        /// </remarks>
        /// <param name="interceptor">a packet interceptor</param>
        public void Add(ISSHPacketInterceptor interceptor) {
            lock (_interceptors) {
                if (_interceptors.All(i => i.GetType() != interceptor.GetType())) {
                    _interceptors.AddLast(interceptor);
                    Debug.WriteLine("PacketInterceptor: Add {0}", interceptor.GetType());
                }
            }
        }

        /// <summary>
        /// Feed packet to the packet interceptors.
        /// </summary>
        /// <param name="packet">a packet object</param>
        /// <returns>true if the packet was consumed.</returns>
        public bool InterceptPacket(DataFragment packet) {
            lock (_interceptors) {
                var node = _interceptors.First;
                while (node != null) {
                    var result = node.Value.InterceptPacket(packet);
                    if (result == SSHPacketInterceptorResult.Consumed) {
                        return true;
                    }
                    if (result == SSHPacketInterceptorResult.Finished) {
                        var nodeToRemove = node;
                        node = node.Next;
                        _interceptors.Remove(nodeToRemove);
                        Debug.WriteLine("PacketInterceptor: Del {0}", nodeToRemove.Value.GetType());
                    }
                    else {
                        node = node.Next;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Call the specified action on each interceptor of the collection.
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<ISSHPacketInterceptor> action) {
            lock (_interceptors) {
                foreach (var entry in _interceptors) {
                    try {
                        action(entry);
                    }
                    catch (Exception e) {
                        Debug.WriteLine(e.Message);
                        Debug.WriteLine(e.StackTrace);
                    }
                }
            }
        }
    }



}
