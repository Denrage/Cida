using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Autofac;
using Autofac.Extras.NLog;
using Cida.Server.Interfaces;
using Grpc.Core;
using Grpc.Core.Logging;
using NLog;
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
                new CidaServer(this.currentWorkingDirectory, this.container.Resolve<ISettingsProvider>());
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