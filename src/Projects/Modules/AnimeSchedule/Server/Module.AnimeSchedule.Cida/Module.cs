﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Animeschedule;
using Cida.Api;
using Cida.Api.Models.Filesystem;
using Grpc.Core;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models.Schedule;
using Module.AnimeSchedule.Cida.Services;
using Module.AnimeSchedule.Cida.Services.Actions;
using Module.AnimeSchedule.Cida.Services.Source;
using NLog;

namespace Module.AnimeSchedule.Cida
{
    public class Module : IModule
    {
        // Hack: Remove this asap
        private const string DatabasePassword = "AnimeSchedule";

        private const string Id = "32527EDA-48D9-4B38-B320-946FEDB56A05";
        private string connectionString;

        public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } = Array.Empty<ServerServiceDefinition>();

        public async Task Load(IDatabaseConnector databaseConnector, IFtpClient ftpClient, Directory moduleDirectory, IModuleLogger moduleLogger)
        {
            this.connectionString =
                await databaseConnector.GetDatabaseConnectionStringAsync(Guid.Parse(Id), DatabasePassword);

            using (var context = this.GetContext())
            {
                await context.Database.EnsureCreatedAsync();
            }

            this.GrpcServices = new[] { AnimeScheduleService.BindService(new ScheduleAnimeImplementation(moduleLogger.CreateSubLogger("GRPC-Implementation"))), };

            moduleLogger.Log(NLog.LogLevel.Info, "AnimeSchedule loaded successfully");

            var ircAnimeClient = new Ircanime.IrcAnimeService.IrcAnimeServiceClient(new Channel("127.0.0.1", 31564, ChannelCredentials.Insecure, new[] { new ChannelOption(ChannelOptions.MaxSendMessageLength, -1), new ChannelOption(ChannelOptions.MaxReceiveMessageLength, -1) }));
            var crunchyrollSourceService = new CrunchyrollSourceService(moduleLogger.CreateSubLogger("Crunchyroll-Source"));
            var nibleSourceService = new NiblSourceService(moduleLogger.CreateSubLogger("Nibl-Source"));
            var settingsService = new SettingsService(this.GetContext);
            var discordClient = new DiscordClient(moduleLogger.CreateSubLogger("Discord-Client"), settingsService);
            await discordClient.InitializeClient();
            var scheduleService = new ScheduleService(
                new IActionService[]
                {
                    new DiscordNotificationActionService(moduleLogger.CreateSubLogger("Discord-Action"), settingsService, discordClient),
                    new DownloadActionService(ircAnimeClient, moduleLogger.CreateSubLogger("Download-Action"), settingsService, discordClient),
                    new DatabaseActionService(this.GetContext),
                }, this.GetContext, moduleLogger.CreateSubLogger("Schedule-Service"));

            // HACK: Add a after loaded all modules method to execute this without a delay
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                await scheduleService.Initialize(default, crunchyrollSourceService, nibleSourceService);
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private AnimeScheduleDbContext GetContext() => new AnimeScheduleDbContext(this.connectionString);

        public class ScheduleAnimeImplementation : AnimeScheduleService.AnimeScheduleServiceBase
        {
            private readonly ILogger logger;

            public ScheduleAnimeImplementation(ILogger logger)
            {
                this.logger = logger;
            }

            public override async Task<VersionResponse> Version(VersionRequest request, ServerCallContext context)
            {
                return await Task.FromResult(new VersionResponse() { Version = 1 });
            }
        }
    }
}