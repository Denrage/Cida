using System.IO;
using Cida.Server.Module;

namespace Cida.Server
{
    public class CidaServer
    {
        public CidaServer(string workingDirectory)
        {
            var loader = new ModuleLoaderManager(Path.Combine(workingDirectory, ModuleLoaderManager.ModuleFolderName));
            loader.LoadModules();
            var grpcManager = new Cida.Server.Api.GrpcManager();
            grpcManager.AddService().Wait();
        }
    }
}