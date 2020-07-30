using System.Collections.Generic;
using System.Linq;

namespace IrcClient.Commands.Helpers
{
    internal static class DccCommandHelper
    {
        private static readonly Dictionary<DccCommand, string> DccCommandString = new Dictionary<DccCommand, string>()
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
            return DccCommandString[command];
        }

        public static bool TryParse(string message, out DccCommand? command, out string parameter)
        {
            const char space = ' ';
            string commandString = message;
            if (commandString.Contains(space))
            {
                parameter = commandString.Substring(message.IndexOf(space) + 1);
                commandString = commandString.Remove(commandString.IndexOf(space)).ToUpper();
            }
            else
            {
                parameter = string.Empty;
            }

            if (DccCommandString.ContainsValue(commandString))
            {
                command = DccCommandString.First((x) => x.Value == commandString).Key;
                return true;
            }

            command = null;
            return false;
        }
    }
}
