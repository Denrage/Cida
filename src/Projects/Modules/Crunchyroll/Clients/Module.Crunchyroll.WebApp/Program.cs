using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;

namespace Module.Crunchyroll.WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddSingleton(services =>
            {
                var uri = new Uri("ipv4:127.0.0.1:31564");

                var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());

                return GrpcChannel.ForAddress(uri, new GrpcChannelOptions { HttpHandler = httpHandler });
            });

            await builder.Build().RunAsync();
        }
    }
}
