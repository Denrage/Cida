using System;
using System.Collections.Generic;
using IrcClient.Models;
using NLog;

namespace IrcClient.Handlers
{
    internal abstract class BaseHandler<T>
        where T : struct
    {
        private readonly ILogger logger;

        protected BaseHandler(ILogger logger = null)
        {
            this.logger = logger;
            Handler = new Dictionary<T, Action<IrcMessage>>();
        }

        public Action<IrcMessage> MessageReceived { get; set; }

        protected Dictionary<T, Action<IrcMessage>> Handler { get; }

        public void AddHandler(T command, Action<IrcMessage> handler)
        {
            if (Handler.TryGetValue(command, out Action<IrcMessage> value))
            {
                value += handler;
            }
            else
            {
                Handler.Add(command, handler);
            }
        }

        protected abstract bool TryParseCommand(string message, out T? command, out string parameter);

        protected virtual void OnMessageReceived(IrcMessage message)
        {
            logger?.Log(LogLevel.Debug, $"Received \"{message.Message}\" from {message.Sender}");
            if (this.TryParseCommand(message.Message, out T? command, out string parameter))
            {
                if (command != null)
                {
                    if (Handler.TryGetValue(command.Value, out Action<IrcMessage> handler))
                    {
                        handler(new IrcMessage(parameter, message.Sender));
                    }
                }
            }
            else
            {
                MessageReceived?.Invoke(message);
            }
        }
    }
}