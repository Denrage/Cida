using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Cida.Server.Interfaces;
using Grpc.Core;
using ILogger = NLog.ILogger;

namespace Cida.Server.Console
{
    internal class Program
    {
        private readonly string currentWorkingDirectory = Path
            .GetDirectoryName(Assembly.GetExecutingAssembly().Location)
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
            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        public Program(string nodeName = "")
        {
            this.nodeName = string.IsNullOrEmpty(nodeName) ? "Node" : nodeName;
            this.container = this.InitializeDependencies();
        }

        public void Start()
        {
            GrpcEnvironment.SetLogger(new GrpcLogger(NLog.LogManager.GetLogger("GRPC")));

            var server =
                new CidaServer(this.currentWorkingDirectory, this.container.Resolve<ISettingsProvider>(),
                    NLog.LogManager.GetLogger("CIDA"));
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
            return builder.Build();
        }
    }
}