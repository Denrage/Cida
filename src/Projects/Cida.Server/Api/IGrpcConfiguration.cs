using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Api
{
    // TODO: Add credentials to interface
    public interface IGrpcConfiguration
    {
        (string Endpoint, int Port)[] Endpoints { get; }
    }
}
