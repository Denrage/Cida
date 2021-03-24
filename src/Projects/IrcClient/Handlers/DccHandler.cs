using System;
using IrcClient.Commands;
using IrcClient.Commands.Helpers;
using IrcClient.Models;
using NLog;

namespace IrcClient.Handlers
{
    internal class DccHandler : BaseHandler<DccCommand>
    {
        public DccHandler(ILogger logger)
            : base(logger)
        {
        }

        public Action<IrcMessage> HandleDccReceived =>
            message =>
            {
                this.Logger?.Log(LogLevel.Debug, $"Received DCC from {message.Sender}: \"{message.Message}\"");
                OnMessageReceived(message);
            };

        protected override bool TryParseCommand(string message, out DccCommand? command, out string parameter)
            => DccCommandHelper.TryParse(message, out command, out parameter);
    }
}
