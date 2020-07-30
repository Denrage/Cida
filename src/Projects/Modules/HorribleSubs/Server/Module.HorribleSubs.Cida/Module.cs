using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cida.Api;
using Grpc.Core;
using Horriblesubs;

namespace Module.Crunchyroll.Cida
{
    public class Module : IModule
    {
        // Hack: Remove this asap
        private const string DatabasePassword = "HorribleSubs";
        private const string Id = "109F8F54-DE94-479A-840A-7B4EF0F284D2";
        private string connectionString;
        public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } = Array.Empty<ServerServiceDefinition>();

        public async Task Load(IDatabaseConnector databaseConnector, IFtpClient ftpClient)
        {
            this.connectionString =
                await databaseConnector.GetDatabaseConnectionStringAsync(Guid.Parse(Id), DatabasePassword);
            this.GrpcServices = new[] {HorribleSubsService.BindService(new HorribleSubsImplementation()), };
            Console.WriteLine("Loaded CR");
        }

        public class HorribleSubsImplementation : HorribleSubsService.HorribleSubsServiceBase
        {
            public HorribleSubsImplementation()
            {
            }
        }
    }
}