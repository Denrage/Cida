using IrcClient.Commands;
using IrcClient.Downloaders;
using IrcClient.Handlers;
using IrcClient.Models;
using IrcClient.Models.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace IrcClient.Clients
{
    public class IrcClient : IDisposable
    {
        private readonly IrcHandler handler;
        private readonly List<string> channels;
        private readonly ConcurrentBag<IrcMessage> errors;
        private readonly string tempFolder;
        private bool motdReceived;
        private bool namesReceived;
        private ILogger logger;

        public IrcClient(string host, int port, string userName, string realName, string nickName, string tempFolder, ILogger logger = null)
        {
            Host = host;
            Port = port;
            UserName = userName;
            RealName = realName;
            NickName = nickName;
            handler = new IrcHandler(logger);
            channels = new List<string>();
            errors = new ConcurrentBag<IrcMessage>();
            this.logger = logger;
            this.tempFolder = tempFolder;

            handler.MessageReceived += MessageReceived;
            handler.MessageReceived += IrcHandler_MessageReceived;
            handler.AddHandler(IrcCommand.Error, ErrorReceivedHandler);
            handler.AddHandler(IrcCommand.Ping, PingReceivedHandler);
            handler.AddCtcpHandler(IrcCommand.Ping, CtcpPingReceivedHandler);
            handler.AddCtcpHandler(IrcCommand.Time, CtcpTimeReceivedHandler);
            handler.AddCtcpHandler(IrcCommand.Version, CtcpVersionReceivedHandler);
            handler.AddDccHandler(DccCommand.Send, DccSendReceivedHandler);
            namesReceived = true;
        }

        public event Action<IrcMessage> MessageReceived;

        public event Action<DccDownloader> DownloadRequested;

        public string Host { get; private set; }

        public int Port { get; private set; }

        public string UserName { get; private set; }

        public string RealName { get; private set; }

        public string NickName { get; private set; }

        public bool IsConnected => handler.IsConnected && motdReceived;

        public void Connect()
        {
            handler.Connect(Host, Port);
            handler.SendCommand(IrcCommand.User, $"{UserName} hostname servername {RealName}");
            handler.SendCommand(IrcCommand.Nick, NickName);

            while (!IsConnected)
            {
                Thread.Sleep(100);
            }
        }

        public void Disconnect()
        {
            Quit();
            handler.Disconnect();
        }

        public void SendMessage(string message)
            => SendMessage(message, channels.Last());

        public void SendMessage(string message, string target)
            => handler.SendMessage(message, target);

        public void JoinChannel(string channel)
        {
            if (channels.Contains(channel))
            {
                // If the channel was already joined remove the entry so it will be readded as the last entry => current channel
                channels.Remove(channel);
            }
            else
            {
                logger.Log(LogLevel.Debug, $"Joining channel {channel}");
                while (!namesReceived)
                {
                    System.Threading.Thread.Sleep(100);
                }

                namesReceived = false;
                handler.SendCommand(IrcCommand.Join, channel);

                while (!namesReceived)
                {
                    System.Threading.Thread.Sleep(100);
                }
                logger.Log(LogLevel.Debug, $"Channel {channel} joined");
            }

            channels.Add(channel);
        }

        public void Quit(string message = "")
        {
            while (this.errors.Count > 0)
            {
                this.errors.TryTake(out _);
            }

            handler.SendCommand(IrcCommand.Quit, message);
            var timeout = 3000;
            while (!errors.Any() && timeout > 0)
            {
                Task.Delay(100);
                timeout -= 100;
            }
        }

        public void GetMotd()
            => handler.SendCommand(IrcCommand.Motd);

        public void Dispose()
        {
            if (IsConnected)
            {
                Disconnect();
            }

            handler.Dispose();
        }

        protected virtual void OnDownloadRequested(DccDownloader downloader)
        {
            DownloadRequested?.Invoke(downloader);
        }

        private void IrcHandler_MessageReceived(IrcMessage message)
        {
            if (!motdReceived && message.Message.ToUpper().Contains("MOTD"))
            {
                motdReceived = true;
            }

            if (!namesReceived && message.Message.ToUpper().Contains("NAMES"))
            {
                namesReceived = true;
            }
        }

        private void ErrorReceivedHandler(IrcMessage message)
            => errors.Add(message);

        private void PingReceivedHandler(IrcMessage message)
            => handler.SendCommand(IrcCommand.Pong, message.Message);

        private void CtcpVersionReceivedHandler(IrcMessage message)
            => handler.SendCtcpResponse(message.Sender, IrcCommand.Version, "AD ver1.0");

        // Responses with UTC DateTime in ISO-8601 format
        private void CtcpTimeReceivedHandler(IrcMessage message)
            => handler.SendCtcpResponse(message.Sender, IrcCommand.Time, DateTime.UtcNow.ToString("o"));

        private void CtcpPingReceivedHandler(IrcMessage message)
            => handler.SendCtcpResponse(message.Sender, IrcCommand.Ping, message.Message);

        private void DccSendReceivedHandler(IrcMessage message)
        {
            if (DownloadRequested != null)
            {
                if (DccDownloader.TryCreateFromSendMessage(message.Message, tempFolder, out var downloader, logger))
                {
                    OnDownloadRequested(downloader);
                }
                else
                {
                    throw new DccSendParsingException(message.Message);
                }
            }
        }
    }
}