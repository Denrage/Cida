using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace Cida.Client.Avalonia.Services
{
    public class CidaConnectionService
    {
        public Channel Channel { get; private set; }

        public async Task<bool> Connect(string address, int port)
        {
            this.Channel = new Channel(address, port, ChannelCredentials.Insecure, new [] { new ChannelOption(ChannelOptions.MaxSendMessageLength, -1), new ChannelOption(ChannelOptions.MaxReceiveMessageLength, -1) });
            await this.Channel.ConnectAsync(DateTime.Now.ToUniversalTime().AddSeconds(30));
            return this.Channel.State == ChannelState.Ready;
        }
    }
}
