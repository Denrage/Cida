using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace IrcClient.Connections
{
    public abstract class NetworkConnection : IDisposable
    {
        private readonly int bufferSize;

        private Socket socket;
        private NetworkStream stream;
        private CancellationTokenSource cancellationTokenSource;
        private Task readTask;

        protected ILogger Logger { get; }

        protected NetworkConnection(ILogger logger, int bufferSize = 1024)
        {
            this.bufferSize = bufferSize;
            this.Logger = logger;
        }

        public bool IsConnected => socket?.Connected ?? false;

        public bool IsDataAvailable => (socket?.Available ?? 0) > 0;

        public void Connect(string host, int port)
        {
            Logger.Info($"Connecting to '{host}:{port}'");
            if (socket == null)
            {
                InitializeSocket();
            }
            cancellationTokenSource = new CancellationTokenSource();
            socket.Connect(host, port);
            stream = new NetworkStream(socket);
            readTask = Task.Run(new Action(BeginReceiving), cancellationTokenSource.Token);
            Logger.Info("Connected");
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                Logger.Info("Disconnecting");
                cancellationTokenSource.Cancel();
                socket.Shutdown(SocketShutdown.Both);
                readTask.Wait();
                socket.Disconnect(true);
                stream.Dispose();
                Logger.Info("Disconnected");
                socket = null;
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
                this.Logger.Info($"Sending message: '{message}'");
                var data = Encoding.UTF8.GetBytes(message + Environment.NewLine);
                stream.BeginWrite(data, 0, data.Length, (x) => stream.EndWrite(x), null);
            }
            else
            {
                this.Logger.Info($"Not connected anymore. Message will not be sent. '{message}'");
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
