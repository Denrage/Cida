using IrcClient.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IrcClient.Handlers
{
    public class MessageHandler : IHandler
    {
        public Action<IrcMessage> MessageReceived { get; set; }

        public async Task<bool> Handle(IrcMessage message, CancellationToken token)
        {
            await Task.Run(() => MessageReceived?.Invoke(message), token).ConfigureAwait(false);
            return true;
        }
    }
}
