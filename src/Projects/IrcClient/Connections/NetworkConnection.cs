using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IrcClient.Connections
{
    public abstract class NetworkConnection : IDisposable
    {
        private readonly int bufferSize;

        private Socket socket;
        private NetworkStream stream;
        private CancellationTokenSource cancellationTokenSource;
        private Task readTask;

        protected NetworkConnection(int bufferSize = 1024)
        {
            this.bufferSize = bufferSize;
            InitializeSocket();
        }

        public bool IsConnected => socket.Connected;

        public bool IsDataAvailable => socket.Available > 0;

        public void Connect(string host, int port)
        {
            cancellationTokenSource = new CancellationTokenSource();
            socket.Connect(host, port);
            stream = new NetworkStream(socket);
            readTask = Task.Run(new Action(BeginReceiving), cancellationTokenSource.Token);
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                cancellationTokenSource.Cancel();
                socket.Shutdown(SocketShutdown.Both);
                readTask.Wait();
                socket.Disconnect(true);
            }
        }

        public void Dispose()
        {
            Disconnect();
            stream?.Dispose();
            socket.Dispose();
        }

        protected void SendRawMessage(string message)
        {
            if (IsConnected)
            {
                var data = Encoding.UTF8.GetBytes(message + Environment.NewLine);
                stream.BeginWrite(data, 0, data.Length, (x) => stream.EndWrite(x), null);
            }
        }

        protected abstract void OnDataReceived(byte[] buffer, int index, int count);

        private void InitializeSocket()
        {
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        private void BeginReceiving()
        {
            while (IsConnected && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (!IsDataAvailable)
                {
                    Thread.Sleep(10);
                    continue;
                }

                var buffer = new byte[bufferSize];
                int count = stream.Read(buffer, 0, bufferSize);
                if (count > 0)
                {
                    OnDataReceived(buffer, 0, count);
                }
            }
        }
    }
}