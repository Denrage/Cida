using System;
using System.Text;
using Cida.Server.Infrastructure.Database;
using Cida.Server.Infrastructure.Database.EFC;
using Cida.Server.Infrastructure.Database.Models.DatabaseModels;
using Cida.Server.Infrastructure.Database.Settings;
using Grpc.Core;
using Hsnr;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbConnector = new DatabaseConnector(new CidaContext(new CidaDbConnectionProvider(new MockSettingsManager())), new CidaDbConnectionProvider(new MockSettingsManager()));
            using (var cidaContext = new CidaContext(new CidaDbConnectionProvider(new MockSettingsManager())))
            {
                var ftpInformation = new FtpInformation()
                {
                    FtpPath = @"testpath",
                };

                var module = new ModuleInformation()
                {
                    ModuleId = Guid.NewGuid(),
                    ModuleName = "testModule",
                    FtpInfomation = ftpInformation
                };

                cidaContext.Modules.Add(module);

                cidaContext.SaveChanges();

                dbConnector.GetDatabaseConnectionString(module.ModuleId, "testPassword");
            }


            Console.WriteLine("Done");

            Console.ReadKey();
        }
    }
}