using System;
using System.Collections.Generic;
using Grpc.Core;

namespace Cida.Api
{
    
    public interface IModule
    {
        void Load();

        IEnumerable<ServerServiceDefinition> GrpcServices { get; }
    }
}
