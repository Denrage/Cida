using IrcClient.Commands;
using IrcClient.Commands.Helpers;
using IrcClient.Models;
using NLog;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrcClient.Connections
{
    public class IrcConnection : NetworkConnection
    {
        private string lastMessage;

        public IrcConnection(ILogger logger)
            : base(logger)
        {
        }

        public event Action<IrcMessage> DataReceived;

        public void SendMessage(string message, string target)
            => SendCommand(IrcCommand.PrivMsg, $"{target} :{message}");

        public void SendCommand(IrcCommand command, string parameter = "")
            => SendRawMessage($"{command.ToCommandString()} {parameter}");

        public void SendCtcpRequest(string target, IrcCommand command, string parameter = "")
            => SendCommand(IrcCommand.PrivMsg, $"{target} {command.ToCommandString()} {parameter}");

        public void SendCtcpResponse(string target, IrcCommand command, string parameter = "")
        {
            const char ctcpChar = '\x01';
            SendCommand(IrcCommand.Notice, $"{target} {ctcpChar} {command.ToCommandString()} {parameter} {ctcpChar}");
        }

        protected override void OnDataReceived(byte[] buffer, int index, int count)
        {
            var rawMessage = Encoding.UTF8.GetString(buffer, 0, count);

            // If lastMessage is not empty the first message is not a complete message
            if (!string.IsNullOrEmpty(lastMessage))
            {
                rawMessage = lastMessage + rawMessage;
                lastMessage = null;
            }

            // If the last char is not a line feed the last message was not complete
            if (rawMessage.Any() && rawMessage.Last() != '\n')
            {
                int lastIndexOfLineFeed = rawMessage.LastIndexOf('\n');
                lastMessage = rawMessage.Substring(lastIndexOfLineFeed);
                rawMessage = rawMessage.Remove(lastIndexOfLineFeed);
            }

            var messages = rawMessage.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var message in messages)
            {
                string parsableMessage = message;
                string sender = null;
                if (parsableMessage.First() == ':')
                {
                    int indexOfSpace = parsableMessage.IndexOf(' ');
                    sender = parsableMessage.Remove(indexOfSpace).Substring(1);
                    parsableMessage = parsableMessage.Substring(indexOfSpace + 1);
                }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => DataReceived?.Invoke(new IrcMessage(parsableMessage, sender))).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
    }
}
