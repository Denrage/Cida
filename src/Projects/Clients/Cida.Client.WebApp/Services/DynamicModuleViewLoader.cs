using Cida.Api;
using Cida.Client.WebApp.Api.BaseClasses;
using Cida.Client.WebApp.Interfaces;
using Cida.Client.WebApp.Module;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Cida.Server.Api.GrpcManager;

namespace Cida.Client.WebApp.Services
{
    public class DynamicModuleViewLoader : IModuleLoaderService
    {
        public IAsyncEnumerable<WebModule> LoadModulesAsync()
        {
            throw new NotImplementedException();

            //var client = new CidaApiService.CidaApiServiceClient(new Channel("127.0.0.1", 31564, ChannelCredentials.Insecure));
            //var modules = (await client.ClientModuleAsync(new ClientModuleRequest() { Id = "68376FB3-E650-4832-B7C2-4846A8DA7E91" })).Streams.Select(x => WebModule.Extract(x.ToByteArray())).Select(x => x.ModuleView);

            //var moduleSet = new HashSet<ModuleViewBase>(modules);
            //return moduleSet;
        }
    }
}
