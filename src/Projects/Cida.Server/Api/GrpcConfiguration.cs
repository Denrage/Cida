using System;

namespace Cida.Server.Api
{
    [Serializable]
    public class GrpcConfiguration : IGrpcConfiguration
    {
        public Endpoint[] Endpoints { get; set; }

        public class Endpoint
        {
            public string Host { get; set; }
            
            public int Port { get; set; }
        }
    }
}