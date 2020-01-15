using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Autofac;
using Cida.Server.Interfaces;
using Grpc.Core;
using Grpc.Core.Logging;

namespace Cida.Server.Console
{
    internal class Program
    {
        private readonly string currentWorkingDirectory = Path
            .GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)
            .Replace("file:", string.Empty).TrimStart('\\');

        private readonly IContainer container;

        private static void Main(string[] args)
        {
            var program = new Program();
            program.Start();
            System.Console.ReadKey();
        }

        public Program()
        {
            this.container = InitializeDependencies();
        }

        public void Start()
        {
            GrpcEnvironment.SetLogger(new ConsoleLogger());
            var server =
                new CidaServer(this.currentWorkingDirectory, container.Resolve<ISettingsProvider>());
        }

        private IContainer InitializeDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<IocModule>();
            builder.RegisterInstance(new JsonSettingsProvider(new FileWriter(Path.Combine(this.currentWorkingDirectory, "settings.json"))))
                .As<ISettingsProvider>()
                .SingleInstance();

            return builder.Build();
        }
    }
}
