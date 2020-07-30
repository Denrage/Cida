using System;
using System.Linq;
using IrcClient.Commands;
using IrcClient.Commands.Helpers;
using IrcClient.Models;

namespace IrcClient.Handlers
{
    internal class CtcpHandler : BaseHandler<IrcCommand>
    {
        private readonly DccHandler dccHandler;

        public CtcpHandler()
        {
            dccHandler = new DccHandler();

            AddHandler(IrcCommand.Dcc, HandleDcc);
        }

        public Action<IrcMessage> HandleCtcp => OnMessageReceived;

        public void AddDccHandler(DccCommand command, Action<IrcMessage> handler)
            => dccHandler.AddHandler(command, handler);

        protected override void OnMessageReceived(IrcMessage message)
        {
            const char userChar = '!';
            string processedSender = message.Sender;
            if (processedSender != null && processedSender.Contains(userChar))
            {
                processedSender = processedSender.Remove(processedSender.IndexOf(userChar));
            }

            base.OnMessageReceived(new IrcMessage(message.Message, processedSender));
        }

        protected override bool TryParseCommand(string message, out IrcCommand? command, out string parameter)
            => IrcCommandHelper.TryParse(message, out command, out parameter);

        private void HandleDcc(IrcMessage message)
            => dccHandler.HandleDccReceived(message);
    }
}