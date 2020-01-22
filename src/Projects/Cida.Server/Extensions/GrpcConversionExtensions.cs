using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cida.Server.Infrastructure;
using Infrastructure;

namespace Cida.Server.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static SynchronizeResponse.Types.ExternalServerConnection ToGrpc(
            this GlobalConfigurationManager.ExternalServerConnectionManager
                connection)
            => new SynchronizeResponse.Types.ExternalServerConnection()
            {
                Host = connection.Host,
                Password = connection.Password,
                Port = connection.Port,
                Username = connection.Username
            };

        public static ExternalServerConnection FromGrpc(
            this SynchronizeResponse.Types.ExternalServerConnection connection)
            => new ExternalServerConnection()
            {
                Host = connection.Host,
                Password = connection.Password,
                Port = connection.Port,
                Username = connection.Username,
            };

        public static ExternalServerConnection[] FromGrpc(
            this IEnumerable<SynchronizeResponse.Types.ExternalServerConnection> connections)
        {
            var result = new List<ExternalServerConnection>();
            foreach (var connection in connections)
            {
                result.Add(connection.FromGrpc());
            }

            return result.ToArray();
        }

        public static SynchronizeResponse.Types.ExternalServerConnection[] ToGrpc(
            this IEnumerable<GlobalConfigurationManager.ExternalServerConnectionManager> connections)
        {
            var result = new List<SynchronizeResponse.Types.ExternalServerConnection>();
            foreach (var connection in connections.Where(x => x.Host != null))
            {
                result.Add(connection.ToGrpc());
            }

            return result.ToArray();
        }
        
        public static SynchronizeResponse.Types.DatabaseConnection ToGrpc(
            this GlobalConfigurationManager.DatabaseConnectionManager connection)
            => new SynchronizeResponse.Types.DatabaseConnection()
            {
                Connection = connection.Connection.ToGrpc(),
                DatabaseName = connection.DatabaseName,
            };

        public static DatabaseConnection FromGrpc(
            this SynchronizeResponse.Types.DatabaseConnection connection)
            => new DatabaseConnection()
            {
                DatabaseName = connection.DatabaseName,
                Connection = connection.Connection.FromGrpc(),
            };
    }
}