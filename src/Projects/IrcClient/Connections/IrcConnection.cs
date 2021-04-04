using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IrcClient.Commands;
using IrcClient.Commands.Helper;
using IrcClient.Handlers;
using IrcClient.Models;

namespace IrcClient.Connections
{
    public class IrcConnection : IDisposable
    {
        private readonly string host;
        private readonly int port;
        private readonly Socket socket;
        private readonly ConcurrentBag<IHandler> handler;
        private Task receiver;
        private CancellationTokenSource receiverCancellationTokenSource;
        private bool isDisposed;

        public IrcConnection(String host, int port)
        {
            this.host = host;
            this.port = port;
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.handler = new ConcurrentBag<IHandler>();
        }

        public Action<string> MessageSent { get; set; }

        public Action<string> MessageReceived { get; set; }

        public bool IsConnected => this.socket.Connected;

        ~IrcConnection()
        {
            Dispose();
        }

        public void Dispose()
        {
            this.isDisposed = true;
            this.Disconnect(false);
            this.socket.Dispose();
            GC.SuppressFinalize(this);
        }

        public void AddHandler(IHandler handler)
        {
            this.ThrowIfDisposed();
            this.handler.Add(handler);
        }

        public async Task<bool> Handle(IrcMessage message, CancellationToken token)
        {
            this.ThrowIfDisposed();
            foreach (var handler in this.handler)
            {
                if (await handler.Handle(message, token).ConfigureAwait(false))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task Connect(CancellationToken token = default)
        {
            this.ThrowIfDisposed();

            if (this.receiverCancellationTokenSource != default && this.receiver != null)
            {
                this.receiverCancellationTokenSource.Cancel();
            }

            this.receiverCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            // TODO: use CancellationToken for socket when it's implemented in .net
            await this.socket.ConnectAsync(this.host, this.port).ConfigureAwait(false);

            this.receiver = Task.Factory.StartNew(async () =>
            {
                using var reader = new StreamReader(new NetworkStream(this.socket));

                while (this.socket.Connected)
                {
                    this.receiverCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    var message = await reader.ReadLineAsync().ConfigureAwait(false);
                    // TODO: do this betterer
                    if (message == null)
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        continue;
                    }

                    string sender = null;

                    MessageReceived?.Invoke(message);

                    if (message.First() == ':')
                    {
                        // message contains sender
                        var index = message.IndexOf(' ');
                        sender = message.Remove(index)[1..];
                        message = message[(index + 1)..];
                    }

                    await Handle(new IrcMessage(message, sender), this.receiverCancellationTokenSource.Token).ConfigureAwait(false);
                }
            }, this.receiverCancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Disconnect(bool reuse = true)
        {
            this.ThrowIfDisposed();
            this.socket.Disconnect(reuse);
        }

        public async Task Send(IrcCommand command, string parameter = "", CancellationToken token = default)
        {
            this.ThrowIfDisposed();
            var message = $"{command.ToCommandString()} {parameter}";
            var data = Encoding.UTF8.GetBytes(message + "\r\n");
            //this.socket.SendBufferSize = data.Length;
            await this.socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None, token).ConfigureAwait(false);

            MessageSent?.Invoke(message);
        }

        public async Task SendRequest(string target, IrcCommand command, CancellationToken token, string parameter = "")
        {
            this.ThrowIfDisposed();
            await Send(IrcCommand.PrivMsg, $"{target} {command.ToCommandString()} {parameter}", token).ConfigureAwait(false);
        }

        public async Task SendResponse(string target, IrcCommand command, CancellationToken token, string parameter = "")
        {
            this.ThrowIfDisposed();
            await Send(IrcCommand.Notice, $"{target} \x01{command.ToCommandString()} {parameter}\x01", token).ConfigureAwait(false);
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException($"{nameof(IrcConnection)} is already disposed");
            }
        }
    }
}
