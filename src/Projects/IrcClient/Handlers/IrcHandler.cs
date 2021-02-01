using System;
using System.Linq;
using IrcClient.Commands;
using IrcClient.Commands.Helpers;
using IrcClient.Connections;
using IrcClient.Models;
using NLog;

namespace IrcClient.Handlers
{
    internal class IrcHandler : BaseHandler<IrcCommand>, IDisposable
    {
        private readonly IrcConnection connection;
        private readonly CtcpHandler ctcpHandler;

        public IrcHandler(ILogger logger = null)
            :base(logger)
        {
            connection = new IrcConnection();
            ctcpHandler = new CtcpHandler(logger);

            connection.DataReceived += OnMessageReceived;
            AddHandler(IrcCommand.PrivMsg, HandlePrivateCtcpMessage);
            AddHandler(IrcCommand.CPrivMsg, HandleCtcpMessage);
            AddHandler(IrcCommand.Notice, HandleCtcpMessage);
            AddHandler(IrcCommand.CNotice, HandleCtcpMessage);
        }

        public new Action<IrcMessage> MessageReceived
        {
            get => base.MessageReceived;
            set
            {
                ctcpHandler.MessageReceived = value;
                base.MessageReceived = value;
            }
        }

        public bool IsConnected => connection.IsConnected;

        public void AddCtcpHandler(IrcCommand command, Action<IrcMessage> handler)
            => ctcpHandler.AddHandler(command, handler);

        public void AddDccHandler(DccCommand command, Action<IrcMessage> handler)
            => ctcpHandler.AddDccHandler(command, handler);

        public void Connect(string host, int port)
            => connection.Connect(host, port);

        public void Disconnect()
            => connection.Disconnect();

        public void SendMessage(string message, string target)
            => connection.SendMessage(message, target);

        public void SendCommand(IrcCommand command, string parameter = "")
            => connection.SendCommand(command, parameter);

        public void SendCtcpRequest(string target, IrcCommand command, string parameter = "")
            => connection.SendCtcpRequest(target, command, parameter);

        public void SendCtcpResponse(string target, IrcCommand command, string parameter = "")
            => connection.SendCtcpResponse(target, command, parameter);

        public void Dispose()
        {
            connection.Dispose();
        }

        protected override bool TryParseCommand(string message, out IrcCommand? command, out string parameter)
            => IrcCommandHelper.TryParse(message, out command, out parameter);

        private void HandleCtcpMessage(IrcMessage message)
        {
            const char ctcpChar = '\x01';
            string processedMessage = message.Message;
            if (processedMessage.Contains(ctcpChar))
            {
                processedMessage = processedMessage.Remove(processedMessage.LastIndexOf(ctcpChar));
                processedMessage = processedMessage.Substring(processedMessage.LastIndexOf(ctcpChar) + 1);
            }

            ctcpHandler.HandleCtcp(new IrcMessage(processedMessage, message.Sender));
        }

        private void HandlePrivateCtcpMessage(IrcMessage message)
        {
            this.Logger?.Log(LogLevel.Debug, $"{message.Sender}: \"{message.Message}\"");
            this.HandleCtcpMessage(message);
        }
    }
}