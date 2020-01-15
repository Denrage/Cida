using System;
using System.Collections.Generic;
using Grpc.Core;

namespace Cida.Api
{
    public interface IModule
    {
        IEnumerable<ServerServiceDefinition> GrpcServices { get; }

        void Load();
    }
}