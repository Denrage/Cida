using System;
using Cida.Api;
using Grpc.Core;

namespace Module.Hsnr
{
    public class Module : IModule
    {
        public void Load()
        {
        }

        public ServerServiceDefinition GrpcService { get; }
    }
}
