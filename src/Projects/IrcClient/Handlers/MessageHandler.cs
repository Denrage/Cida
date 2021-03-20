using IrcClient.Models;
using System;
using System.Threading.Tasks;

namespace IrcClient.Handlers
{
    public class MessageHandler : IHandler
    {
        public Action<IrcMessage> MessageReceived
        {
            get;
            set;
        }

        public async Task<bool> Handle(IrcMessage message)
        {
            await Task.Run(() => MessageReceived?.Invoke(message)).ConfigureAwait(false);
            return true;
        }
    }
}
