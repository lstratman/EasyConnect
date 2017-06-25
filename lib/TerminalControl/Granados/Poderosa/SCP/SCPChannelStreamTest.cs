/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SCPChannelStreamTest.cs,v 1.1 2011/11/22 09:04:13 kzmi Exp $
 */

#if UNITTEST

using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using Granados.IO;
using System.Diagnostics;
using System.Threading;

namespace Granados.Poderosa.SCP {

    [TestFixture]
    public class SCPChannelStreamTest {

        private SCPChannelStream _stream;

        private const int INITAL_BUFFER_SIZE = 10;

        [SetUp]
        public void Setup() {
            _stream = new SCPChannelStream();
            _stream.DataBuffer = new byte[INITAL_BUFFER_SIZE];  // for testing
            _stream.OpenForTest(DummySSHChannel.Create());
        }

        //-----------------------------------------------
        // Open()
        //-----------------------------------------------

        [Test]
        public void Opened_Success() {
            Assert.AreEqual("Opened", _stream.Status);
        }

        [Test]
        [ExpectedException(typeof(SCPClientInvalidStatusException))]
        public void Open_AlreadyOpened() {
            _stream.Open(null, null, 0);
        }

        [Test]
        [ExpectedException(typeof(SCPClientInvalidStatusException))]
        public void Open_AlreadyClosed() {
            _stream.Close();
            _stream.Open(null, null, 0);
        }

        //-----------------------------------------------
        // Close()
        //-----------------------------------------------

        [Test]
        public void Close_Success() {
            _stream.Close();
            Assert.AreEqual("Closed", _stream.Status);
        }

        [Test]
        public void Close_AlreadyClosed() {
            _stream.Close();
            _stream.Close();
            Assert.AreEqual("Closed", _stream.Status);
        }

        [Test]
        public void Close_OpenedButError() {
            _stream.ChannelReceiver.OnChannelError(new Exception("Channel error"));
            Assert.AreEqual("Error", _stream.Status);
            _stream.Close();
            Assert.AreEqual("Closed", _stream.Status);
        }

        [Test]
        public void Close_NotOpened() {
            _stream = new SCPChannelStream();
            _stream.Close();
            Assert.AreEqual("NotOpened", _stream.Status);
        }

        //-----------------------------------------------
        // Internal buffer
        //-----------------------------------------------

        [Test]
        public void Buffer_Initial() {
            Assert.AreEqual(0, _stream.BufferOffset);
            Assert.AreEqual(0, _stream.BufferLength);
        }

        [Test]
        public void Buffer_AddSmallData() {
            StreamTester tester = new StreamTester(_stream);
            tester.ScheduleData(7);
            tester.StartAndWait();
            // there was enough room
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 0, 7);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 0, 7);
        }

        [Test]
        public void Buffer_AddSmallData_Offset() {
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 3;
            tester.ScheduleData(7);
            tester.StartAndWait();
            // there was enough room
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 3, 7);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 3, 7);
        }

        [Test]
        public void Buffer_AddSmallData_Refill() {
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 4;
            tester.ScheduleData(7);
            tester.StartAndWait();
            // buffer overflow -> refill
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 0, 7);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 0, 7);
        }

        [Test]
        public void Buffer_AddSmallData_Refill_BufferFull() {
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 4;
            tester.ScheduleData(INITAL_BUFFER_SIZE);
            tester.StartAndWait();
            // buffer overflow -> refill
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 0, INITAL_BUFFER_SIZE);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 0, INITAL_BUFFER_SIZE);
        }

        [Test]
        public void Buffer_AddSmallData_LargeOffset_Refill() {
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = INITAL_BUFFER_SIZE;
            _stream.BufferLength = 0;
            tester.ScheduleData(7);
            tester.StartAndWait();
            // buffer overflow -> refill
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 0, 7);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 0, 7);
        }

        [Test]
        public void Buffer_AddSmallData_ExtendBuffer() {
            StreamTester tester = new StreamTester(_stream);
            tester.ScheduleData(7);
            tester.ScheduleData(4);
            tester.StartAndWait();
            // there was enough room
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 0, 7);
            // buffer overflow -> extend
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE * 2, 0, 11);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 0, 11);
        }

        [Test]
        public void Buffer_AddSmallData_ExtendBuffer_BufferFull() {
            StreamTester tester = new StreamTester(_stream);
            tester.ScheduleData(7);
            tester.ScheduleData(INITAL_BUFFER_SIZE * 2 - 7);
            tester.StartAndWait();
            // there was enough room
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 0, 7);
            // buffer overflow -> extend
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE * 2, 0, INITAL_BUFFER_SIZE * 2);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 0, INITAL_BUFFER_SIZE * 2);
        }

        [Test]
        public void Buffer_AddLargeData() {
            StreamTester tester = new StreamTester(_stream);
            tester.ScheduleData(11);
            tester.StartAndWait();
            // buffer overflow -> extend
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE * 2, 0, 11);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 0, 11);
        }

        [Test]
        public void Buffer_AddLargeData_Offset() {
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 3;
            tester.ScheduleData(11);
            tester.StartAndWait();
            // buffer overflow -> extend
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE * 2, 0, 11);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 0, 11);
        }

        [Test]
        public void Buffer_AddLargeData_LargeOffset() {
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = INITAL_BUFFER_SIZE;
            _stream.BufferLength = 0;
            tester.ScheduleData(11);
            tester.StartAndWait();
            // buffer overflow -> extend
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE * 2, 0, 11);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 0, 11);
        }

        [Test]
        public void Buffer_AddHugeData() {
            StreamTester tester = new StreamTester(_stream);
            tester.ScheduleData(21);
            tester.StartAndWait();
            // buffer overflow -> extend to 20 -> extend to 40
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE * 4, 0, 21);
            tester.AssertFinal();
            CheckBufferContent(_stream.DataBuffer, 0, 21);
        }

        //-----------------------------------------------
        // ReadByte()
        //-----------------------------------------------

        [Test]
        [ExpectedException(typeof(SCPClientTimeoutException))]
        public void ReadByte_Timeout() {
            _stream.ReadByte(1000);
        }

        [Test]
        [ExpectedException(typeof(SCPClientInvalidStatusException))]
        public void ReadByte_NotOpened() {
            _stream = new SCPChannelStream();
            Assert.AreEqual("NotOpened", _stream.Status);
            _stream.ReadByte(1000);
        }

        [Test]
        [ExpectedException(typeof(SCPClientInvalidStatusException))]
        public void ReadByte_AlreadyClosed() {
            _stream.Close();
            Assert.AreEqual("Closed", _stream.Status);
            _stream.ReadByte(1000);
        }

        [Test]
        [ExpectedException(typeof(SCPClientInvalidStatusException))]
        public void ReadByte_OpenedButError() {
            _stream.ChannelReceiver.OnChannelError(new Exception("Channel error"));
            Assert.AreEqual("Error", _stream.Status);
            _stream.ReadByte(1000);
        }

        [Test]
        public void ReadByte_HasData() {
            _stream.DataBuffer[3] = 0xaa;
            _stream.BufferOffset = 3;
            _stream.BufferLength = 1;
            byte b = _stream.ReadByte(1000);
            Assert.AreEqual((byte)0xaa, b);
            Assert.AreEqual(4, _stream.BufferOffset);
            Assert.AreEqual(0, _stream.BufferLength);
        }

        [Test]
        public void ReadByte_WaitForData() {
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 3;   // offset=3, length=0
            tester.ScheduleData(1);
            tester.StartAfter(200);
            byte b = _stream.ReadByte(1000);
            tester.WaitForCompletion();
            // data incoming --> (offset=3, length=1)
            // ReadByte() obtains data --> (offset=4, length=0)
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 4, 0);
            tester.AssertFinal();
            Assert.AreEqual((byte)1, b);
        }

        [Test]
        [ExpectedException(typeof(SCPClientInvalidStatusException))]
        public void ReadByte_ErrorWhileWaiting() {
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 3;   // offset=3, length=0
            tester.ScheduleChannelError();
            tester.StartAfter(200);
            try {
                _stream.ReadByte(1000);
            }
            finally {
                tester.WaitForCompletion();
                tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 3, 0);
                tester.AssertFinal();
            }
        }

        //-----------------------------------------------
        // Read()
        //-----------------------------------------------

        [Test]
        public void Read_BufferHasOneByte() {
            _stream.DataBuffer[3] = 0xaa;
            _stream.BufferOffset = 3;
            _stream.BufferLength = 1;
            byte[] buff = new byte[100];
            int readLength = _stream.Read(buff, 1000);
            Assert.AreEqual(1, readLength);
            Assert.AreEqual((byte)0xaa, buff[0]);
            Assert.AreEqual(4, _stream.BufferOffset);
            Assert.AreEqual(0, _stream.BufferLength);
        }

        [Test]
        public void Read_BufferHasSomeBytes() {
            _stream.DataBuffer[3] = 0xaa;
            _stream.DataBuffer[4] = 0xbb;
            _stream.DataBuffer[5] = 0xcc;
            _stream.BufferOffset = 3;
            _stream.BufferLength = 3;
            byte[] buff = new byte[100];
            int readLength = _stream.Read(buff, 1000);
            Assert.AreEqual(3, readLength);
            Assert.AreEqual((byte)0xaa, buff[0]);
            Assert.AreEqual((byte)0xbb, buff[1]);
            Assert.AreEqual((byte)0xcc, buff[2]);
            Assert.AreEqual(6, _stream.BufferOffset);
            Assert.AreEqual(0, _stream.BufferLength);
        }

        [Test]
        public void Read_WithMaxLength_BufferHasSomeBytes() {
            _stream.DataBuffer[3] = 0xaa;
            _stream.DataBuffer[4] = 0xbb;
            _stream.DataBuffer[5] = 0xcc;
            _stream.BufferOffset = 3;
            _stream.BufferLength = 3;
            byte[] buff = new byte[100];
            int readLength = _stream.Read(buff, 2, 1000);
            Assert.AreEqual(2, readLength);
            Assert.AreEqual((byte)0xaa, buff[0]);
            Assert.AreEqual((byte)0xbb, buff[1]);
            Assert.AreEqual((byte)0, buff[2]);
            Assert.AreEqual(5, _stream.BufferOffset);
            Assert.AreEqual(1, _stream.BufferLength);
        }

        [Test]
        [ExpectedException(typeof(SCPClientTimeoutException))]
        public void Read_Timeout() {
            byte[] buff = new byte[100];
            _stream.Read(buff, 1000);
        }

        [Test]
        public void Read_WaitData_ReadAll() {
            byte[] buff = new byte[100];
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 3;
            _stream.BufferLength = 0;
            tester.ScheduleData(5);
            tester.StartAfter(200);
            int readLength = _stream.Read(buff, 1000);
            tester.WaitForCompletion();
            // data incoming --> (offset=3, length=5)
            // ReadByte() obtains data --> (offset=8, length=0)
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 8, 0);
            tester.AssertFinal();
            Assert.AreEqual(5, readLength);
            CheckBufferContent(buff, 0, 5);
        }

        [Test]
        public void Read_WaitData_ReadPartial() {
            byte[] buff = new byte[2];
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 3;
            _stream.BufferLength = 0;
            tester.ScheduleData(5); // add [ 1, 2, 3, 4, 5 ]
            tester.StartAfter(200);
            int readLength = _stream.Read(buff, 1000);
            tester.WaitForCompletion();
            // data incoming --> (offset=3, length=5)
            // ReadByte() obtains data --> (offset=5, length=3)
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 5, 3);
            tester.AssertFinal();
            Assert.AreEqual(2, readLength);
            CheckBufferContent(buff, 0, 2);
            // remained data is [ 3, 4, 5 ]
            Assert.AreEqual(3, _stream.DataBuffer[5]);
            Assert.AreEqual(4, _stream.DataBuffer[6]);
            Assert.AreEqual(5, _stream.DataBuffer[7]);
        }

        [Test]
        [ExpectedException(typeof(SCPClientInvalidStatusException))]
        public void Read_ErrorWhileWaiting() {
            byte[] buff = new byte[100];
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 3;   // offset=3, length=0
            tester.ScheduleChannelError();
            tester.StartAfter(200);
            try {
                _stream.Read(buff, 1000);
            }
            finally {
                tester.WaitForCompletion();
                tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 3, 0);
                tester.AssertFinal();
            }
        }

        //-----------------------------------------------
        // ReadUntil()
        //-----------------------------------------------

        [Test]
        [ExpectedException(typeof(SCPClientTimeoutException))]
        public void ReadUntil_Timeout() {
            _stream.ReadUntil(0xff, 1000);
        }

        [Test]
        [ExpectedException(typeof(SCPClientTimeoutException))]
        public void ReadUntil_BufferHasData_Timeout() {
            _stream.DataBuffer[4] = 1;
            _stream.DataBuffer[5] = 2;
            _stream.DataBuffer[6] = 3;
            _stream.DataBuffer[7] = 4;
            _stream.BufferOffset = 4;
            _stream.BufferLength = 4;

            try {
                _stream.ReadUntil(0xff, 1000);
            }
            finally {
                // offset and length are not changed
                Assert.AreEqual(4, _stream.BufferOffset);
                Assert.AreEqual(4, _stream.BufferLength);
            }
        }

        [Test]
        public void ReadUntil_BufferHasData_Success() {
            _stream.DataBuffer[4] = 1;
            _stream.DataBuffer[5] = 2;
            _stream.DataBuffer[6] = 3;
            _stream.DataBuffer[7] = 4;
            _stream.BufferOffset = 4;
            _stream.BufferLength = 4;

            byte[] buff = _stream.ReadUntil(3, 1000);

            Assert.AreEqual(3, buff.Length);
            CheckBufferContent(buff, 0, 3);
            Assert.AreEqual(7, _stream.BufferOffset);
            Assert.AreEqual(1, _stream.BufferLength);
        }

        [Test]
        public void ReadUntil_BufferHasData_WaitMore_Success() {
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 3;
            tester.ScheduleData(4);
            tester.ScheduleData(4);
            tester.ScheduleData(4);
            tester.StartAfter(200);
            byte[] buff = _stream.ReadUntil(9, 1000);
            tester.WaitForCompletion();

            // data incoming --> (offset=3, length=4)
            // ReadUntil() consumes no data
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 3, 4);

            // data incoming --> refill --> (offset=0, length=8)
            // ReadUntil() consumes no data
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 0, 8);

            // data incoming --> extend buffer --> (offset=0, length=12)
            // ReadUntil() reads 9 bytes --> (offset=9, length=3)
            tester.AssertBufferStatus(INITAL_BUFFER_SIZE * 2, 9, 3);

            tester.AssertFinal();

            Assert.AreEqual(9, buff.Length);
            CheckBufferContent(buff, 0, 9);
        }

        [Test]
        [ExpectedException(typeof(SCPClientInvalidStatusException))]
        public void ReadUntil_ErrorWhileWaiting() {
            StreamTester tester = new StreamTester(_stream);
            _stream.BufferOffset = 3;   // offset=3, length=0
            tester.ScheduleData(6);
            tester.ScheduleChannelError();
            tester.StartAfter(200);
            try {
                _stream.ReadUntil(10, 1000);
            }
            finally {
                tester.WaitForCompletion();
                tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 3, 6);
                tester.AssertBufferStatus(INITAL_BUFFER_SIZE, 3, 6);
                tester.AssertFinal();
            }
        }


        // Check if the buffer has valid serial values in the specified region.
        private void CheckBufferContent(byte[] buff, int offset, int length) {
            int limit = offset + length;
            int expected = 1;
            for (int i = offset; i < limit; i++) {
                Assert.AreEqual((byte)expected, buff[i]);
                expected++;
            }
        }

    }

    internal class StreamStateLog {
        public readonly int BufferSize;
        public readonly int BufferOffset;
        public readonly int BufferLength;

        public StreamStateLog(SCPChannelStream stream) {
            BufferSize = stream.DataBuffer.Length;
            BufferOffset = stream.BufferOffset;
            BufferLength = stream.BufferLength;
        }
    }

    /// <summary>
    /// Schedule OnData() event and execute them.
    /// After each OnData() event, status of internal buffer of SCPChannelStream is recorded.
    /// </summary>
    internal class StreamTester {

        private enum ScheduledEventType {
            OnData,
            OnChannelError,
        }

        private class ScheduledEvent {
            public readonly ScheduledEventType Type;
            public readonly int Size;

            public ScheduledEvent(ScheduledEventType type, int size) {
                this.Type = type;
                this.Size = size;
            }
        }

        private readonly SCPChannelStream _stream;

        private int _nextValue = 1;

        private readonly List<ScheduledEvent> _schedule = new List<ScheduledEvent>();
        private readonly Queue<StreamStateLog> _logs = new Queue<StreamStateLog>();

        private Thread _thread;

        public StreamTester(SCPChannelStream stream) {
            this._stream = stream;
        }

        /// <summary>
        /// Secdule OnData event.
        /// Passed data contains serial values like (byte)0x1, (byte)0x2, (byte)0x3 ...
        /// </summary>
        /// <param name="size"></param>
        public void ScheduleData(int size) {
            _schedule.Add(new ScheduledEvent(ScheduledEventType.OnData, size));
        }

        /// <summary>
        /// Schedule channel error.
        /// </summary>
        public void ScheduleChannelError() {
            _schedule.Add(new ScheduledEvent(ScheduledEventType.OnChannelError, 0));
        }

        public void StartAfter(int delay) {
            StartCore(delay);
        }

        public void StartAndWait() {
            StartCore(0);
            WaitForCompletion();
        }

        public void WaitForCompletion() {
            _thread.Join();
        }

        private void StartCore(int delay) {
            _logs.Clear();
            _thread = new Thread((ThreadStart)delegate() {
                if (delay > 0) {
                    Thread.Sleep(delay);
                }
                foreach (ScheduledEvent ev in _schedule) {
                    if (ev.Type == ScheduledEventType.OnData) {
                        byte[] data = new byte[ev.Size + 10];
                        for (int i = 0; i < ev.Size; i++) {
                            data[10 + i] = (byte)_nextValue++;
                        }
                        _stream.ChannelReceiver.OnData(data, 10, ev.Size);
                        Thread.Sleep(200);  // process Read()
                        _logs.Enqueue(new StreamStateLog(_stream));
                    }
                    else if (ev.Type == ScheduledEventType.OnChannelError) {
                        _stream.ChannelReceiver.OnChannelError(new Exception("Channel error"));
                        Thread.Sleep(200);  // process Read()
                        _logs.Enqueue(new StreamStateLog(_stream));
                    }
                }
                _schedule.Clear();
            });
            _thread.Start();
        }

        public void AssertBufferStatus(int bufferSize, int bufferOffset, int bufferLength) {
            StreamStateLog log = _logs.Dequeue();
            Assert.IsNotNull(log);
            Assert.AreEqual(bufferSize, log.BufferSize);
            Assert.AreEqual(bufferOffset, log.BufferOffset);
            Assert.AreEqual(bufferLength, log.BufferLength);
        }

        public void AssertFinal() {
            Assert.AreEqual(0, _logs.Count);
        }
    }

    internal class DummySSHChannel : SSHChannel {

        internal static DummySSHChannel Create() {
            SSHConnectionParameter param = new SSHConnectionParameter();
            DummySSHConnection conn = new DummySSHConnection(param, null, null);
            conn.ChannelCollection.RegisterChannelEventReceiver(null, null);
            return new DummySSHChannel(conn);
        }

        private DummySSHChannel(SSHConnection conn)
            : base(conn, ChannelType. ExecCommand, 0) {
        }

        public override void ResizeTerminal(int width, int height, int pixel_width, int pixel_height) {
        }

        public override void Transmit(byte[] data) {
        }

        public override void Transmit(byte[] data, int offset, int length) {
        }

        public override void SendEOF() {
        }

        public override void Close() {
        }
    }

    internal class DummySSHConnection : SSHConnection {

        internal DummySSHConnection(SSHConnectionParameter param, AbstractGranadosSocket strm, ISSHConnectionEventReceiver receiver)
            : base(param, strm, receiver) {
        }


        internal override Granados.IO.IDataHandler PacketBuilder {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override SSHConnectionInfo ConnectionInfo {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        internal override AuthenticationResult Connect() {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Disconnect(string msg) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override SSHChannel OpenShell(ISSHChannelEventReceiver receiver) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override SSHChannel ForwardPort(ISSHChannelEventReceiver receiver, string remote_host, int remote_port, string originator_host, int originator_port) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void ListenForwardedPort(string allowed_host, int bind_port) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void CancelForwardedPort(string host, int port) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override SSHChannel DoExecCommand(ISSHChannelEventReceiver receiver, string command) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SendIgnorableData(string msg) {
            throw new Exception("The method or operation is not implemented.");
        }
    }

}
#endif
