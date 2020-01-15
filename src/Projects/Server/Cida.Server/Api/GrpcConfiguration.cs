using System;
using System.Net;
using Cida.Server.Models;

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
    }
}