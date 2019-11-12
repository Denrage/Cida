using System;
using System.Collections.Generic;
using System.Text;
using Cida.Server.Models;

namespace Cida.Server.Api
{
    // TODO: Add credentials to interface
    public interface IGrpcConfiguration
    {
        Endpoint[] Endpoints { get; }
    }
}
