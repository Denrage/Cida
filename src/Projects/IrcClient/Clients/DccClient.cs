using IrcClient.Connections;
using NLog;
using System;
using System.Net;

namespace IrcClient.Clients
{
    public class DccClient : IDisposable
    {
        private readonly DccConnection connection;

        public DccClient(string host, int port, ILogger logger = null, int bufferSize = 1024)
        {
            Host = host;
            Port = port;

            connection = new DccConnection(logger, bufferSize);
        }

        public string Host { get; private set; }

        public int Port { get; private set; }

        public bool IsConnected
            => connection.IsConnected;

        public bool IsDataAvailable
            => connection.IsDataAvailable;

        public (byte[], int) GetBuffer()
            => connection.GetBuffer();

        public static string GetHostFromBitwiseIp(long bitwiseIp)
        {
            var endPoint = new IPEndPoint(bitwiseIp, 0);

            var reverseIp = endPoint.Address.ToString();
            var ipParts = reverseIp.Split('.');
            var ipAddr = ipParts[0];

            for (int i = 1; i < ipParts.Length; i++)
            {
                ipAddr = ipParts[i] + "." + ipAddr;
            }

            return ipAddr;
        }

        public void Connect()
            => connection.Connect(Host, Port);

        public void Disconnect()
            => connection.Disconnect();

        public void Dispose()
            => connection.Dispose();
    }
}