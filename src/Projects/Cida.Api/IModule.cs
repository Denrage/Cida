using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;

namespace Cida.Api
{
    public interface IModule
    {
        IEnumerable<ServerServiceDefinition> GrpcServices { get; }

        Task Load(IDatabaseConnector connector);
    }
}