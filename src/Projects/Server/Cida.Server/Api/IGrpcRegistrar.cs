using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;

namespace Cida.Server.Api
{
    public interface IGrpcRegistrar
    {
        Task AddServicesAsync(IEnumerable<ServerServiceDefinition> definitions);
    }
}
