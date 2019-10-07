using System;

namespace Cida.Server.Api
{
    [Serializable]
    public class GrpcConfiguration : IGrpcConfiguration
    {
        public (string Endpoint, int Port)[] Endpoints { get; set; }
    }
}