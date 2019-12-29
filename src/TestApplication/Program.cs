using System;
using Crunchyroll;
using Grpc.Core;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var client =
                new CrunchyrollService.CrunchyrollServiceClient(new Channel("127.0.0.1", 31564,
                    ChannelCredentials.Insecure));

            var items = client.Search(new SearchRequest()
            {
                SearchTerm = "Sward"
            }).Items;

            foreach (var item in items)
            {
                Console.WriteLine(item.Name);
            }
            
            Console.WriteLine("Done");

            Console.ReadKey();
        }
    }
}