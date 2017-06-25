/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ProtocolComponent.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Poderosa.Protocols {
    internal class MonitorableStreamTemplate<T> {
        protected LinkedList<T> _monitors;  //can be null
        protected IEnumerator<T> _enumerator;

        public void AddMonitor(T monitor) {
            if (_monitors == null)
                _monitors = new LinkedList<T>();
            _monitors.AddLast(monitor);
        }
        public void RemoveMonitor(T monitor) {
            if (_monitors != null)
                _monitors.Remove(monitor);
        }

        protected void ResetEnumerator() {
            if (_enumerator == null)
                _enumerator = _monitors.GetEnumerator();
            else
                _enumerator.Reset(); //TODO コレクションの中身変わってもResetが有効なことを確認
        }
    }

    internal class MonitorableByteAsyncInputStream : MonitorableStreamTemplate<IByteAsyncInputStream>, IByteAsyncInputStream {
        private IByteAsyncInputStream _receiver;

        public MonitorableByteAsyncInputStream(IByteAsyncInputStream receiver) {
            _receiver = receiver;
        }

        public void OnReception(ByteDataFragment data) {
            if (_monitors != null) {
                ResetEnumerator();
                while (_enumerator.MoveNext())
                    _enumerator.Current.OnReception(data);
            }

            _receiver.OnReception(data);
        }

        public void OnNormalTermination() {
            if (_monitors != null) {
                ResetEnumerator();
                while (_enumerator.MoveNext())
                    _enumerator.Current.OnNormalTermination();
            }

            _receiver.OnNormalTermination();
        }

        public void OnAbnormalTermination(string message) {
            if (_monitors != null) {
                ResetEnumerator();
                while (_enumerator.MoveNext())
                    _enumerator.Current.OnAbnormalTermination(message);
            }

            _receiver.OnAbnormalTermination(message);
        }

    }

    internal class MonitorableByteOutputStream : MonitorableStreamTemplate<IByteOutputStream>, IByteOutputStream {
        private IByteOutputStream _transmitter;

        public MonitorableByteOutputStream(IByteOutputStream transmitter) {
            _transmitter = transmitter;
        }

        public void Transmit(ByteDataFragment data) {
            if (_monitors != null) {
                ResetEnumerator();
                while (_enumerator.MoveNext())
                    _enumerator.Current.Transmit(data);
            }
            _transmitter.Transmit(data);
        }

        public void Transmit(byte[] buffer, int offset, int length) {
            if (_monitors != null) {
                ResetEnumerator();
                while (_enumerator.MoveNext())
                    _enumerator.Current.Transmit(buffer, offset, length);
            }
            _transmitter.Transmit(buffer, offset, length);
        }

        public void Close() {
            if (_monitors != null) {
                ResetEnumerator();
                while (_enumerator.MoveNext())
                    _enumerator.Current.Close();
            }
            _transmitter.Close();
        }

    }
}
