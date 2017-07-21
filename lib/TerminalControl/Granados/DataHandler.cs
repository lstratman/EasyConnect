// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

//#define DEBUG_SYNCHRONOUSPACKETHANDLER

using System;

namespace Granados.IO {

    /// <summary>
    /// <see cref="IDataHandler"/> adapter which redirects to the delegate methods.
    /// </summary>
    internal class DataHandlerAdapter : IDataHandler {

        private readonly Action<DataFragment> _onData;
        private readonly Action _onClosed;
        private readonly Action<Exception> _onError;

        public DataHandlerAdapter(Action<DataFragment> onData, Action onClosed, Action<Exception> onError) {
            _onData = onData;
            _onClosed = onClosed;
            _onError = onError;
        }

        public void OnData(DataFragment data) {
            _onData(data);
        }

        public void OnClosed() {
            _onClosed();
        }

        public void OnError(Exception error) {
            _onError(error);
        }
    }

    /// <summary>
    /// An interface for sending a packet.
    /// </summary>
    /// <typeparam name="PacketType">type of the packet object.</typeparam>
    internal interface IPacketSender<PacketType> {
        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="packet">a packet object.</param>
        void Send(PacketType packet);
    }

    /// <summary>
    /// A base class for the synchronization of sending/receiving packets.
    /// </summary>
    /// <typeparam name="PacketType">type of the packet object.</typeparam>
    internal abstract class AbstractSynchronousPacketHandler<PacketType> : IDataHandler, IPacketSender<PacketType> {

        // lock object for synchronization of sending packet
        private readonly object _syncSend = new object();
        // lock object for synchronization of receiving packet
        private readonly object _syncReceive = new object();

        private readonly IGranadosSocket _socket;
        private readonly IDataHandler _handler;

        private volatile bool _disconnected = false;

        /// <summary>
        /// Gets the binary image of the packet to be sent.
        /// </summary>
        /// <remarks>
        /// The packet object will be allowed to be reused.
        /// </remarks>
        /// <param name="packet">a packet object</param>
        /// <returns>binary image of the packet</returns>
        protected abstract DataFragment GetPacketImage(PacketType packet);

        /// <summary>
        /// Allows to reuse a packet object.
        /// </summary>
        /// <param name="packet">a packet object</param>
        protected abstract void Recycle(PacketType packet);

        /// <summary>
        /// Do additional work for a packet to be sent.
        /// </summary>
        /// <param name="packet">a packet object</param>
        protected abstract void BeforeSend(PacketType packet);

        /// <summary>
        /// Do additional work for a received packet.
        /// </summary>
        /// <param name="packet">a packet image</param>
        protected abstract void AfterReceived(DataFragment packet);

        /// <summary>
        /// Gets the packet type name of the packet to be sent. (for debugging)
        /// </summary>
        /// <param name="packet">a packet object</param>
        /// <returns>packet name.</returns>
        protected abstract String GetMessageName(PacketType packet);

        /// <summary>
        /// Gets the packet type name of the received packet. (for debugging)
        /// </summary>
        /// <param name="packet">a packet image</param>
        /// <returns>packet name.</returns>
        protected abstract String GetMessageName(DataFragment packet);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">socket object for sending packets.</param>
        /// <param name="handler">the next handler received packets are redirected to.</param>
        protected AbstractSynchronousPacketHandler(IGranadosSocket socket, IDataHandler handler) {
            _socket = socket;
            _handler = handler;
        }

        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="packet">a packet object.</param>
        public void Send(PacketType packet) {
            BeforeSend(packet);
            lock (_syncSend) {
                if (_disconnected) {
#if DEBUG_SYNCHRONOUSPACKETHANDLER
                    System.Diagnostics.Debug.WriteLine("(blocked) <-- [{0}]", new object[] { GetMessageName(packet) });
#endif
                    Recycle(packet);
                    return;
                }

#if DEBUG_SYNCHRONOUSPACKETHANDLER
                System.Diagnostics.Debug.WriteLine("S <-- [{0}]", new object[] { GetMessageName(packet) });
#endif
                _socket.Write(GetPacketImage(packet));
            }
        }

        /// <summary>
        /// Sends a DISCONNECT packet.
        /// </summary>
        /// <param name="packet">a packet object.</param>
        public void SendDisconnect(PacketType packet) {
            BeforeSend(packet);
            lock (_syncSend) {
                if (_disconnected) {
#if DEBUG_SYNCHRONOUSPACKETHANDLER
                    System.Diagnostics.Debug.WriteLine("(blocked) <-- [{0}]", new object[] { GetMessageName(packet) });
#endif
                    Recycle(packet);
                    return;
                }

                _disconnected = true;

#if DEBUG_SYNCHRONOUSPACKETHANDLER
                System.Diagnostics.Debug.WriteLine("S <-- [{0}]", new object[] { GetMessageName(packet) });
#endif
                _socket.Write(GetPacketImage(packet));
            }
        }

        /// <summary>
        /// Handles received packet.
        /// </summary>
        /// <param name="data">packet image</param>
        public void OnData(DataFragment data) {
            lock (_syncReceive) {
                if (_disconnected) {
#if DEBUG_SYNCHRONOUSPACKETHANDLER
                    System.Diagnostics.Debug.WriteLine("S --> [{0}] {1} bytes --> (blocked)", GetMessageName(data), data.Length);
#endif
                    return;
                }

                AfterReceived(data);
#if DEBUG_SYNCHRONOUSPACKETHANDLER
                System.Diagnostics.Debug.WriteLine("S --> [{0}] {1} bytes --> OnData", GetMessageName(data), data.Length);
#endif
                _handler.OnData(data);
            }
        }

        /// <summary>
        /// Handles closed event.
        /// </summary>
        public void OnClosed() {
            _handler.OnClosed();
        }

        /// <summary>
        /// Handles error event.
        /// </summary>
        public void OnError(Exception error) {
            _handler.OnError(error);
        }
    }

}
