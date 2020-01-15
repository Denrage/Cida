using System;
using System.Net;

namespace Cida.Server.Api
{
    [Serializable]
    public class GrpcConfiguration : IGrpcConfiguration
    {
        public Endpoint[] Endpoints { get; set; } = new[]
        {
            new Endpoint()
            {
                Host = IPAddress.Loopback.ToString(),
                Port = 31564,
            }
        };

        public class Endpoint
        {
            public string Host { get; set; }

            public int Port { get; set; }
        }
    }
}