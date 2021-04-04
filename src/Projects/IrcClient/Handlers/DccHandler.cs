// IrcClient.Handlers.DccHandler
using IrcClient.Clients;
using IrcClient.Commands;
using IrcClient.Commands.Helper;
using IrcClient.Connections;
using IrcClient.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IrcClient.Handlers
{
    public class DccHandler : IHandler
    {
        private readonly IrcConnection connection;

        public DccHandler(IrcConnection connection)
        {
            this.connection = connection;
        }

        public Action<DccClient> FileReceived { get; set; }

        public async Task<bool> Handle(IrcMessage message, CancellationToken token)
        {
            if (IrcCommandHelper.TryParse(message.Message, out IrcCommand ircCommand, out string ircParameter)
                && ircCommand == IrcCommand.Dcc
                && DccCommandHelper.TryParse(ircParameter, out DccCommand dccCommand, out string dccParameter))
            {
                Console.Write(dccParameter);
                switch (dccCommand)
                {
                    case DccCommand.Send:
                        var regex = new Regex("([\\x21-\\x7E]+|\"[\\x20-\\x7E]+\") (\\d+) (\\d+) (\\d+)");
                        var matches = regex.Match(dccParameter);

                        if (matches.Success
                            && matches.Groups.Count > 4
                            && uint.TryParse(matches.Groups[2].Value, out var bitwiseIp)
                            && int.TryParse(matches.Groups[3].Value, out var port)
                            && ulong.TryParse(matches.Groups[4].Value, out var filesize))
                        {
                            var client = new DccClient(matches.Groups[1].Value.Trim('"'), filesize);

                            var ipBytes = BitConverter.GetBytes(bitwiseIp);
                            var host = $"{ipBytes[3]}.{ipBytes[2]}.{ipBytes[1]}.{ipBytes[0]}";

                            await client.Connect(host, port, token).ConfigureAwait(false);

                            FileReceived?.Invoke(client);
                        }

                        return true;
                }
            }
            return false;
        }
    }
}
