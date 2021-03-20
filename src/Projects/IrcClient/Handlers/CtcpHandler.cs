// IrcClient.Handlers.CtcpHandler
using IrcClient.Commands;
using IrcClient.Commands.Helper;
using IrcClient.Connections;
using IrcClient.Handlers;
using IrcClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IrcClient.Handlers
{
    public class CtcpHandler : IHandler
    {
        private static ISet<IrcCommand> commands = new HashSet<IrcCommand>(new IrcCommand[4]
        {
            IrcCommand.PrivMsg,
            IrcCommand.CPrivMsg,
            IrcCommand.Notice,
            IrcCommand.CNotice
        });

        private readonly IrcConnection connection;

        public CtcpHandler(IrcConnection connection)
        {
            this.connection = connection;
        }
        public async Task<bool> Handle(IrcMessage message)
        {
            if (IrcCommandHelper.TryParse(message.Message, out var ircCommand, out var parameter)
                && commands.Contains(ircCommand))
            {
                var sender = message.Sender;
                if (sender != null)
                {
                    var idx = sender.IndexOf('!');
                    if (idx >= 0)
                    {
                        sender = sender.Remove(idx);
                    }
                }

                var privMessage = parameter;
                var firstIdx = privMessage.IndexOf('\x01');
                var lastIdx = privMessage.LastIndexOf('\x01');
                if (firstIdx >= 0 && lastIdx >= 0 && firstIdx != lastIdx)
                {
                    privMessage = privMessage
                        .Remove(lastIdx)
                        .Substring(firstIdx + 1);
                }

                if (IrcCommandHelper.TryParse(privMessage, out var ctcpCommand, out var ctcpParameter))
                {
                    switch (ctcpCommand)
                    {
                        case IrcCommand.Ping:
                            await this.connection.SendResponse(sender, IrcCommand.Ping, ctcpParameter).ConfigureAwait(false);
                            return true;
                        case IrcCommand.Time:
                            await this.connection.SendResponse(sender, IrcCommand.Time, DateTime.UtcNow.ToString("o")).ConfigureAwait(false);
                            return true;
                        case IrcCommand.Version:
                            await this.connection.SendResponse(sender, IrcCommand.Version, "C# IrcClient v2").ConfigureAwait(false);
                            return true;
                    }
                }

                return await this.connection.Handle(new IrcMessage(privMessage, sender)).ConfigureAwait(false);
            }

            return false;
        }
    }
}
