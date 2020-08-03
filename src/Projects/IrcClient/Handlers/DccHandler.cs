using System;
using IrcClient.Commands;
using IrcClient.Commands.Helpers;
using IrcClient.Models;

namespace IrcClient.Handlers
{
    internal class DccHandler : BaseHandler<DccCommand>
    {
        public Action<IrcMessage> HandleDccReceived => OnMessageReceived;

        protected override bool TryParseCommand(string message, out DccCommand? command, out string parameter)
            => DccCommandHelper.TryParse(message, out command, out parameter);
    }
}