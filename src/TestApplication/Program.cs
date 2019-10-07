using System;
using Grpc.Core;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
            var client = new Cida.CidaApiService.CidaApiServiceClient(channel);
            Console.WriteLine(client.Version(new Cida.VersionRequest()).Version);


            Console.ReadKey();
            channel.ShutdownAsync().Wait();
        }
    }
}
