// IrcClient.Handlers.IrcHandler
using IrcClient.Commands;
using IrcClient.Commands.Helper;
using IrcClient.Connections;
using IrcClient.Handlers;
using IrcClient.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IrcClient.Handlers
{
    public class IrcHandler : IHandler
    {
        private readonly IrcConnection connection;

        public IrcHandler(IrcConnection connection)
        {
            this.connection = connection;
        }

        public async Task<bool> Handle(IrcMessage message)
        {
            if (IrcCommandHelper.TryParse(message.Message, out var command, out var parameter))
            {
                switch (command)
                {
                    /*
                    case IrcCommand.Cap:
                        if (parameter.Contains("LS"))
                            await this.connection.Send(IrcCommand.Cap, "REQ away-notify chghost multi-prefix userhost-in-names");
                        if (parameter.Contains("ACK"))
                            await this.connection.Send(IrcCommand.Cap, "END");
                        return true;
                    */
                    case IrcCommand.Ping:
                        await this.connection.Send(IrcCommand.Pong, parameter);
                        return true;
                }
            }

            return false;
        }
    }
}
