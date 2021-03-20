using System.Collections.Generic;
using System.Linq;
using IrcClient.Commands;

namespace IrcClient.Commands.Helper
{
    public static class DccCommandHelper
    {
        private static readonly Dictionary<DccCommand, string> dccCommands = new Dictionary<DccCommand, string>()
        {
            { DccCommand.Accept, "ACCEPT" },
            { DccCommand.Chat, "CHAT" },
            { DccCommand.Recv, "RECV" },
            { DccCommand.Reject, "REJECT" },
            { DccCommand.Resume, "RESUME" },
            { DccCommand.Reverse, "REVERSE" },
            { DccCommand.RSend, "RSEND" },
            { DccCommand.Send, "SEND" },
            { DccCommand.Xmit, "XMIT" }
        };

        public static string ToCommandString(this DccCommand command)
        {
            return dccCommands[command];
        }

        public static bool TryParse(string message, out DccCommand command, out string parameter)
        {
            string commandString = message;
            int position = message.IndexOf(' ');

            if (position >= 0)
            {
                parameter = commandString.Substring(position + 1);
                commandString = commandString.Remove(position).ToUpper();
            }
            else
            {
                parameter = string.Empty;
            }
            
            var matches = dccCommands
                .Where((x) => x.Value == commandString)
                .Select((x) => x.Key);

            command = matches.FirstOrDefault();
            return matches.Any();
        }
    }

}