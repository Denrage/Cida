﻿using System;
using System.Collections.Concurrent;
using System.Threading;

namespace IrcClient.Connections
{
    internal class DccConnection : NetworkConnection
    {
        private ConcurrentQueue<(byte[], int)> buffers; 
        private AutoResetEvent bufferEvent;

        public DccConnection(int bufferSize = 1024)
            : base(bufferSize)
        {
            this.buffers = new ConcurrentQueue<(byte[], int)>();
            this.bufferEvent = new AutoResetEvent(false);
        }
        protected override void OnDataReceived(byte[] buffer, int index, int count)
        {
            this.buffers.Enqueue((buffer, count));
            this.bufferEvent.Set();
        }
        public (byte[], int) GetBuffer()
        {
            (byte[], int) buffer;

            while (!this.buffers.TryDequeue(out buffer)) 
                this.bufferEvent.WaitOne();

            return buffer;
        }
    }
}