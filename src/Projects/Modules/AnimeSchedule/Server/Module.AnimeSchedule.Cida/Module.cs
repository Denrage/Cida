using Animeschedule;
using Cida.Api;
using API = Cida.Api;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Services;
using Module.AnimeSchedule.Cida.Services.Actions;
using Module.AnimeSchedule.Cida.Services.Source;
using NLog;
using Module.AnimeSchedule.Cida.Models;

namespace Module.AnimeSchedule.Cida;

public class Module : IModule
{
    // Hack: Remove this asap
    private const string DatabasePassword = "AnimeSchedule";

    private const string Id = "32527EDA-48D9-4B38-B320-946FEDB56A05";
    private string connectionString;

    public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } = Array.Empty<ServerServiceDefinition>();

    public async Task Load(IDatabaseConnector databaseConnector, IFtpClient ftpClient, API.Models.Filesystem.Directory moduleDirectory, IModuleLogger moduleLogger)
    {
        this.connectionString =
            await databaseConnector.GetDatabaseConnectionStringAsync(Guid.Parse(Id), DatabasePassword);

        using (var context = this.GetContext())
        {
            await context.Database.EnsureCreatedAsync();
        }

        moduleLogger.Log(NLog.LogLevel.Info, "AnimeSchedule loaded successfully");

        var ircAnimeClient = new Ircanime.IrcAnimeService.IrcAnimeServiceClient(new Channel("127.0.0.1", 31564, ChannelCredentials.Insecure, new[] { new ChannelOption(ChannelOptions.MaxSendMessageLength, -1), new ChannelOption(ChannelOptions.MaxReceiveMessageLength, -1) }));
        var settingsService = new SettingsService(this.GetContext);
        var discordClient = new DiscordClient(moduleLogger.CreateSubLogger("Discord-Client"), this.GetContext);
        await discordClient.InitializeClients(default);
        var scheduleService = new ScheduleService(
            new IActionService[]
            {
                new DiscordNotificationActionService(moduleLogger.CreateSubLogger("Discord-Action"), settingsService, discordClient),
                new DatabaseActionService(this.GetContext),
            },
            new IMultiActionService[]
            {
                new DownloadActionService(ircAnimeClient, moduleLogger.CreateSubLogger("Download-Action"), settingsService, discordClient),
            }, this.GetContext, moduleLogger, moduleLogger.CreateSubLogger("Schedule-Service"));

        this.GrpcServices = new[] { AnimeScheduleService.BindService(new ScheduleAnimeImplementation(moduleLogger.CreateSubLogger("GRPC-Implementation"), this.GetContext)), };

        // HACK: Add a after loaded all modules method to execute this without a delay
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            await scheduleService.Initialize(default);
        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    private AnimeScheduleDbContext GetContext() => new AnimeScheduleDbContext(this.connectionString);

    public class ScheduleAnimeImplementation : AnimeScheduleService.AnimeScheduleServiceBase
    {
        private readonly ILogger logger;
        private readonly Func<AnimeScheduleDbContext> getContext;

        public ScheduleAnimeImplementation(ILogger logger, Func<AnimeScheduleDbContext> getContext)
        {
            this.logger = logger;
            this.getContext = getContext;
        }

        public override async Task<VersionResponse> Version(VersionRequest request, ServerCallContext context)
        {
            return await Task.FromResult(new VersionResponse() { Version = 1 });
        }

        public override async Task<CreateScheduleResponse> CreateSchedule(CreateScheduleRequest request, ServerCallContext context)
        {
            try
            {
                using var dbContext = this.getContext();

                if (request.Override)
                {
                    var existingSchedule = await dbContext.Schedules.FindAsync(new object[] { request.ScheduleId }, context.CancellationToken);

                    if (existingSchedule != null)
                    {
                        dbContext.Update(existingSchedule);
                        existingSchedule.Name = request.Name;
                        existingSchedule.Interval = request.Interval.ToTimeSpan();
                        existingSchedule.StartDate = request.StartDate.ToDateTime().ToLocalTime();
                        await dbContext.SaveChangesAsync(context.CancellationToken);

                        return new CreateScheduleResponse()
                        {
                            CreateResult = CreateScheduleResponse.Types.Result.Success,
                            ScheduleId = existingSchedule.Id,
                        };
                    }
                    else
                    {
                        return new CreateScheduleResponse()
                        {
                            ScheduleId = request.ScheduleId,
                            CreateResult = CreateScheduleResponse.Types.Result.Schedulenotfound,
                        };
                    }
                }

                if (await dbContext.Schedules.AnyAsync(x => x.Name == request.Name, context.CancellationToken))
                {
                    return new CreateScheduleResponse()
                    {
                        CreateResult = CreateScheduleResponse.Types.Result.Alreadyexists,
                        ScheduleId = 0,
                    };
                }

                var schedule = new Schedule()
                {
                    Interval = request.Interval.ToTimeSpan(),
                    Name = request.Name,
                    StartDate = request.StartDate.ToDateTime().ToLocalTime(),
                };

                var result = await dbContext.Schedules.AddAsync(schedule, context.CancellationToken);
                await dbContext.SaveChangesAsync(context.CancellationToken);

                return new CreateScheduleResponse()
                {
                    CreateResult = CreateScheduleResponse.Types.Result.Success,
                    ScheduleId = schedule.Id,
                };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return new CreateScheduleResponse()
                {
                    CreateResult = CreateScheduleResponse.Types.Result.Unknown,
                    ScheduleId = 0,
                };
            }
        }
    }
}
