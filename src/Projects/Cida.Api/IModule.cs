using System;
using Grpc.Core;

namespace Cida.Api
{
    
    public interface IModule
    {
        void Load();

        ServerServiceDefinition GrpcService { get; }
    }
}
