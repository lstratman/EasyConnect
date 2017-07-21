// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using System;

namespace Granados.PortForwarding {

    /// <summary>
    /// Remote port forwarding request
    /// </summary>
    public class RemotePortForwardingRequest {
        /// <summary>
        /// Connected IP address on the server.
        /// </summary>
        public readonly string ConnectedAddress;

        /// <summary>
        /// Connected port number on the server.
        /// </summary>
        public readonly uint ConnectedPort;

        /// <summary>
        /// Originator IP address.
        /// </summary>
        public readonly string OriginatorIp;

        /// <summary>
        /// Originator port number.
        /// </summary>
        public readonly uint OriginatorPort;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectedAddress">Connected IP address on the server.</param>
        /// <param name="connectedPort">Connected port number on the server.</param>
        /// <param name="originatorIp">Originator IP address.</param>
        /// <param name="originatorPort">Originator port number.</param>
        public RemotePortForwardingRequest(string connectedAddress, uint connectedPort, string originatorIp, uint originatorPort) {
            this.ConnectedAddress = connectedAddress;
            this.ConnectedPort = connectedPort;
            this.OriginatorIp = originatorIp;
            this.OriginatorPort = originatorPort;
        }
    }

    /// <summary>
    /// A reply to the new request on the server-to-client port forwarding.
    /// </summary>
    public class RemotePortForwardingReply {

        /// <summary>
        /// Reson code for rejection.
        /// </summary>
        public enum Reason {
            /// <summary>No reason</summary>
            None = 0,
            /// <summary>SSH_OPEN_ADMINISTRATIVELY_PROHIBITED</summary>
            AdministrativelyProhibited = 1,
            /// <summary>SSH_OPEN_CONNECT_FAILED</summary>
            ConnectFailed = 2,
            /// <summary>SSH_OPEN_UNKNOWN_CHANNEL_TYPE</summary>
            UnknownChannelType = 3,
            /// <summary>SSH_OPEN_RESOURCE_SHORTAGE</summary>
            ResourceShortage = 4,
        }

        /// <summary>
        /// true if the requested port-forwarding has been accepted.
        /// </summary>
        public readonly bool Accepted;

        /// <summary>
        /// An event handler object for the new channel.
        /// </summary>
        public readonly ISSHChannelEventHandler EventHandler;

        /// <summary>
        /// Reason code for denaial (SSH2 only)
        /// </summary>
        /// <remarks>This field is meaningful when the connection uses SSH2 and the <see cref="Accept"/> property was false.</remarks>
        public readonly Reason ReasonCode;

        /// <summary>
        /// Reason message for denaial (SSH2 only)
        /// </summary>
        /// <remarks>This field is meaningful when the connection uses SSH2 and the <see cref="Accept"/> property was false.</remarks>
        public readonly string ReasonMessage;

        /// <summary>
        /// Constructor
        /// </summary>
        protected RemotePortForwardingReply(bool accepted, ISSHChannelEventHandler eventHandler, Reason reasonCode, string reasonMessage) {
            this.Accepted = accepted;
            this.EventHandler = eventHandler;
            this.ReasonCode = reasonCode;
            this.ReasonMessage = reasonMessage;
        }

        /// <summary>
        /// Create an instance for accepting the request.
        /// </summary>
        /// <param name="eventHandler">channel event handler for the new channel</param>
        /// <rereturns>An instance.</rereturns>
        public static RemotePortForwardingReply Accept(ISSHChannelEventHandler eventHandler) {
            return new RemotePortForwardingReply(true, eventHandler, Reason.None, null);
        }

        /// <summary>
        /// Create an instance for rejecting the request.
        /// </summary>
        /// <param name="reason">reason code</param>
        /// <param name="descrition">description</param>
        /// <rereturns>An instance.</rereturns>
        public static RemotePortForwardingReply Reject(Reason reason, string descrition) {
            if (reason == Reason.None) {
                throw new ArgumentException("reason must not be None.");
            }
            if (descrition == null) {
                throw new ArgumentException("descrition is null.");
            }
            return new RemotePortForwardingReply(false, null, reason, descrition);
        }
    }

    /// <summary>
    /// An interface for handling server-to-client port forwarding.
    /// </summary>
    public interface IRemotePortForwardingHandler {

        /// <summary>
        /// Notifies that the server-to-client port forwarding has been started.
        /// </summary>
        /// <param name="port">
        /// port number which has been bound.
        /// if the requested port number was 0, this value tells which port was actually bound on the server.
        /// </param>
        void OnRemotePortForwardingStarted(uint port);

        /// <summary>
        /// Notifies that the server-to-client port forwarding has failed to start.
        /// </summary>
        void OnRemotePortForwardingFailed();

        /// <summary>
        /// Check new request of the server-to-client port forwarding.
        /// </summary>
        /// <param name="request">address/port informations about the new request</param>
        /// <param name="channel">temporary channel object</param>
        /// <returns>informations about acceptance or rejection of this port-forwarding.</returns>
        RemotePortForwardingReply OnRemotePortForwardingRequest(RemotePortForwardingRequest request, ISSHChannel channel);

    }

    /// <summary>
    /// A wrapper class of <see cref="IRemotePortForwardingHandler"/> for internal use.
    /// </summary>
    internal class RemotePortForwardingHandlerIgnoreErrorWrapper : IRemotePortForwardingHandler {

        private readonly IRemotePortForwardingHandler _coreHandler;

        public RemotePortForwardingHandlerIgnoreErrorWrapper(IRemotePortForwardingHandler coreHandler) {
            _coreHandler = coreHandler;
        }

        public void OnRemotePortForwardingStarted(uint port) {
            try {
                _coreHandler.OnRemotePortForwardingStarted(port);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnRemotePortForwardingFailed() {
            try {
                _coreHandler.OnRemotePortForwardingFailed();
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public RemotePortForwardingReply OnRemotePortForwardingRequest(RemotePortForwardingRequest request, ISSHChannel channel) {
            // exception is not handled here.
            // caller should handle exception appropriately.
            return _coreHandler.OnRemotePortForwardingRequest(request, channel);
        }
    }
}
