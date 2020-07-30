using System;

namespace IrcClient.Connections
{
    internal class DccConnection : NetworkConnection
    {
        public DccConnection(int bufferSize = 1024)
            : base(bufferSize)
        {
        }

        public event Action<byte[], int, int> DataReceived;

        protected override void OnDataReceived(byte[] buffer, int index, int count)
            => DataReceived?.Invoke(buffer, index, count);
    }
}