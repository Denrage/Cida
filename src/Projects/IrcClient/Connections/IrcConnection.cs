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
        private string host;
        private int port;
        private readonly Socket socket;
        private readonly ConcurrentBag<IHandler> handler;
        private Task receiver;

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
            Disconnect(false);
            this.socket.Dispose();
            GC.SuppressFinalize(this);
        }

        public void AddHandler(IHandler handler)
        {
            this.handler.Add(handler);
        }

        public async Task<bool> Handle(IrcMessage message)
        {
            foreach (var handler in this.handler)
            {
                if (await handler.Handle(message).ConfigureAwait(false))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task Connect(CancellationToken token = default)
        {
            await this.socket.ConnectAsync(this.host, this.port/*, token*/).ConfigureAwait(false);

            this.receiver = Task.Factory.StartNew(async () => 
            {
                using var reader = new StreamReader(new NetworkStream(this.socket));

                while (this.socket.Connected)
                {
                    token.ThrowIfCancellationRequested();

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
                        var idx = message.IndexOf(' ');
                        sender = message.Remove(idx).Substring(1);
                        message = message.Substring(idx + 1);
                    }

                    await Handle(new IrcMessage(message, sender)).ConfigureAwait(false);
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        public void Disconnect(bool reuse = true)
        {
            this.socket.Disconnect(reuse);
        }

        public async Task Send(IrcCommand command, string parameter = "", CancellationToken token = default)
        {
            var message = $"{command.ToCommandString()} {parameter}";
            var data = Encoding.UTF8.GetBytes(message + "\r\n");
            //this.socket.SendBufferSize = data.Length;
            await this.socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None, token).ConfigureAwait(false);

            MessageSent?.Invoke(message);
        }

        public async Task SendRequest(string target, IrcCommand command, string parameter = "")
        {
            await Send(IrcCommand.PrivMsg, $"{target} {command.ToCommandString()} {parameter}").ConfigureAwait(false);
        }

        public async Task SendResponse(string target, IrcCommand command, string parameter = "")
        {
            await Send(IrcCommand.Notice, $"{target} \x01{command.ToCommandString()} {parameter}\x01").ConfigureAwait(false);
        }
    }
}