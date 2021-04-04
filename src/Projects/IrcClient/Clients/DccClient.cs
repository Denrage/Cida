using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IrcClient.Clients
{
    public class DccClient
    {
        private readonly Socket socket;
        private readonly int buffersize;

        public DccClient(string filename, ulong filesize, int buffersize = 4096)
        {
            this.Filename = filename;
            this.Filesize = filesize;
            this.buffersize = buffersize;
            this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public string Filename { get; private set; }

        public ulong Filesize { get; private set; }

        public event Action<ulong, ulong> Progress;

        // TODO: use CancellationToken for socket when it's implemented in .net
        public async Task Connect(string host, int port, CancellationToken token)
        {
            await this.socket.ConnectAsync(host, port).ConfigureAwait(false);
        }

        public async Task<bool> WriteToStream(Stream output, CancellationToken token = default)
        {
            if (!this.socket.Connected)
                return false;

            using var input = new NetworkStream(this.socket);
            ulong count = 0;
            var buffer = new byte[this.buffersize];

            while (this.socket.Connected && count < this.Filesize)
            {
                token.ThrowIfCancellationRequested();

                var n = await input.ReadAsync(buffer, 0, this.buffersize, token);
                await output.WriteAsync(buffer, 0, n, token).ConfigureAwait(false);
                count += (ulong)n;

                NotifyProgress(count);
            }

            return true;
        }

        private async void NotifyProgress(ulong count)
        {
            await Task.Run(() => Progress?.Invoke(count, Filesize)).ConfigureAwait(false);
        }
    }
}
