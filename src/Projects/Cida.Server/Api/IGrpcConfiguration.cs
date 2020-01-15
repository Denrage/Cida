using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Api
{
    // TODO: Add credentials to interface
    public interface IGrpcConfiguration
    {
        GrpcConfiguration.Endpoint[] Endpoints { get; }
    }
}
