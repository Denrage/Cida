using Cida.Server.Infrastructure;
using Infrastructure;

namespace Cida.Server.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static SynchronizeResponse.Types.ExternalServerConnection ToGrpc(this GlobalConfigurationService.GlobalConfiguration.ExternalServerConnection connection)
        => new SynchronizeResponse.Types.ExternalServerConnection()
        {
            Host = connection.Host,
            Password = connection.Password,
            Port =  connection.Port,
            Username = connection.Username
        };

        public static GlobalConfigurationService.GlobalConfiguration.ExternalServerConnection FromGrpc(
            this SynchronizeResponse.Types.ExternalServerConnection connection)
            => new GlobalConfigurationService.GlobalConfiguration.ExternalServerConnection(connection.Host, connection.Port, connection.Username, connection.Password);
    }
}