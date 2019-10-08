using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cida.Api;
using Grpc.Core;
using Hsnr;

namespace Module.Hsnr
{
    public class Module : IModule
    {
        public void Load()
        {
        }

        public IEnumerable<ServerServiceDefinition> GrpcServices { get; } = new[]
        {
            HsnrService.BindService(new HsnrServiceImplementation()),
        };

    }

    public class HsnrServiceImplementation : HsnrService.HsnrServiceBase
    {
        public override Task<VersionResponse> Version(VersionRequest request, ServerCallContext context)
        {
            return Task.FromResult(new VersionResponse() {Version = 1});
        }
    }
}
