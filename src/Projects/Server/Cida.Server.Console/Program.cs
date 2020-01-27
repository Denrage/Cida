using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Cida.Server.Interfaces;
using Grpc.Core;
using ILogger = NLog.ILogger;

namespace Cida.Server.Console
{
    internal partial class Program
    {
        private readonly string currentWorkingDirectory = Path
            .GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)
            .Replace("file:", string.Empty).TrimStart('\\');

        private readonly IContainer container;
        private readonly string nodeName;

        private static void Main(string[] args)
        {
            string nodeName = string.Empty;
            if (args.Length > 0)
            {
                nodeName = args[0];
            }
            else
            {
                System.Console.Write("Node name: ");
                nodeName = System.Console.ReadLine();
            }


            var program = new Program(nodeName);
            program.Start();
            System.Console.ReadKey();
        }

        public Program(string nodeName = "")
        {
            this.nodeName = string.IsNullOrEmpty(nodeName) ? "Node" : nodeName;
            this.container = InitializeDependencies();
        }

        public void Start()
        {
            GrpcEnvironment.SetLogger(new GrpcLogger(this.container.Resolve<NLog.ILogger>()));

            var server =
                new CidaServer(this.currentWorkingDirectory, this.container.Resolve<ISettingsProvider>(),
                    this.container.Resolve<ILogger>());
            Task.Run(async () =>
            {
                try
                {
                    await server.Start();
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    
                    if (e.InnerException != null)
                    {
                        System.Console.WriteLine(e.InnerException.Message);
                    }
                    System.Console.ReadKey();
                    Environment.Exit(1);
                }
            });
        }

        private IContainer InitializeDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<IocModule>();
            builder.RegisterInstance(
                    new JsonSettingsProvider(new FileWriter(Path.Combine(this.currentWorkingDirectory,
                        $"{this.nodeName}.json"))))
                .As<ISettingsProvider>()
                .SingleInstance();
            builder.RegisterInstance(NLog.LogManager.GetCurrentClassLogger()).As<NLog.ILogger>().SingleInstance();
            return builder.Build();
        }
    }
}