using System.Collections.Generic;
using System.Threading.Tasks;
using Cida.Api.Models.Filesystem;
using Grpc.Core;

namespace Cida.Api
{
    public interface IModule
    {
        IEnumerable<ServerServiceDefinition> GrpcServices { get; }

        Task Load(IDatabaseConnector connector, IFtpClient ftpClient, Directory moduleDirectory);
    }
}