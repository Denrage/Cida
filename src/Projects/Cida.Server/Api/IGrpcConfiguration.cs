using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Api
{
    interface IGrpcConfiguration
    {
        (string Endpoint, int Port)[] Endpoints { get; }


    }
}
