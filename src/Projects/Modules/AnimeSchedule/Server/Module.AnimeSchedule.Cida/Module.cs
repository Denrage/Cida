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
using Microsoft.EntityFrameworkCore;
using Module.AnimeSchedule.Cida.Extensions;

namespace Module.AnimeSchedule.Cida;

public class Module : IModule
{
    // Hack: Remove this asap
    private const string DatabasePassword = "AnimeSchedule";

    private const string Id = "32527EDA-48D9-4B38-B320-946FEDB56A05";
    private string connectionString;
    private IDatabaseProvider databaseProvider;

    public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } = Array.Empty<ServerServiceDefinition>();

    public async Task Load(IDatabaseConnector databaseConnector, IFtpClient ftpClient, API.Models.Filesystem.Directory moduleDirectory, IModuleLogger moduleLogger)
    {
        this.connectionString =
            await databaseConnector.GetDatabaseConnectionStringAsync(Guid.Parse(Id), DatabasePassword);

        this.databaseProvider = databaseConnector.GetDatabaseProvider();

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
                new DownloadActionService(ircAnimeClient, moduleLogger.CreateSubLogger("Download-Action"), settingsService, discordClient, this.GetContext),
            }, this.GetContext, moduleLogger, moduleLogger.CreateSubLogger("Schedule-Service"));

        this.GrpcServices = new[] { AnimeScheduleService.BindService(new ScheduleAnimeImplementation(moduleLogger.CreateSubLogger("GRPC-Implementation"), this.GetContext, scheduleService, discordClient)), };

        // HACK: Add a after loaded all modules method to execute this without a delay
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            await scheduleService.Initialize(default);
        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    private AnimeScheduleDbContext GetContext() => new AnimeScheduleDbContext(this.connectionString, this.databaseProvider);

    public class ScheduleAnimeImplementation : AnimeScheduleService.AnimeScheduleServiceBase
    {
        private readonly ILogger logger;
        private readonly Func<AnimeScheduleDbContext> getContext;
        private readonly ScheduleService scheduleService;
        private readonly DiscordClient discordClient;
        private readonly Anilist4Net.Client anilistClient = new();

        public ScheduleAnimeImplementation(ILogger logger, Func<AnimeScheduleDbContext> getContext, ScheduleService scheduleService, DiscordClient discordClient)
        {
            this.logger = logger;
            this.getContext = getContext;
            this.scheduleService = scheduleService;
            this.discordClient = discordClient;
        }

        public override async Task<VersionResponse> Version(VersionRequest request, ServerCallContext context)
        {
            return await Task.FromResult(new VersionResponse() { Version = 1 });
        }

        public override async Task<CreateScheduleResponse> CreateSchedule(CreateScheduleRequest request, ServerCallContext context)
        {
            this.logger.Info($"Create/Edit schedule: {request.Name} - {request.Interval} - {request.StartDate} - {request.Override}");
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

                if (await EntityFrameworkQueryableExtensions.AnyAsync(dbContext.Schedules, x => x.Name == request.Name, context.CancellationToken))
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

        public override async Task<CreateWebhookResponse> CreateDiscordWebhook(CreateDiscordWebhookRequest request, ServerCallContext context)
        {
            this.logger.Info($"Create webhook: {request.WebhookId} - {request.WebhookToken}");
            try
            {
                if (!this.discordClient.TestWebhook(request.WebhookId, request.WebhookToken))
                {
                    return new CreateWebhookResponse()
                    {
                        CreateResult = CreateWebhookResponse.Types.Result.Invalidwebhook,
                    };
                }

                using var dbContext = this.getContext();

                if ((await dbContext.DiscordWebhooks.FindAsync(new object[] { request.WebhookId }, context.CancellationToken)) != null)
                {
                    return new CreateWebhookResponse()
                    {
                        CreateResult = CreateWebhookResponse.Types.Result.Alreadyexists,
                    };
                }

                await dbContext.DiscordWebhooks.AddAsync(new DiscordWebhook()
                {
                    WebhookId = request.WebhookId,
                    WebhookToken = request.WebhookToken,
                }, context.CancellationToken);

                await dbContext.SaveChangesAsync(context.CancellationToken);

                return new CreateWebhookResponse()
                {
                    CreateResult = CreateWebhookResponse.Types.Result.Success
                };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return new CreateWebhookResponse()
                {
                    CreateResult = CreateWebhookResponse.Types.Result.Unknown,
                };
            }
        }

        public override async Task<AssignWebhookToScheduleResponse> AssignWebhookToSchedule(AssignWebhookToScheduleRequest request, ServerCallContext context)
        {
            this.logger.Info($"Assign webhook: {request.WebhookId} - {request.ScheduleId}");
            try
            {
                using var dbContext = this.getContext();
                var schedule = await dbContext.Schedules.Include(x => x.DiscordWebhooks).FirstOrDefaultAsync(x => x.Id == request.ScheduleId, context.CancellationToken);
                var webhook = await dbContext.DiscordWebhooks.Include(x => x.Schedules).FirstOrDefaultAsync(x => x.WebhookId == request.WebhookId, context.CancellationToken);

                if (schedule == null)
                {
                    return new AssignWebhookToScheduleResponse()
                    {
                        AssignResult = AssignWebhookToScheduleResponse.Types.Result.Schedulenotfound,
                    };
                }

                if (webhook == null)
                {
                    return new AssignWebhookToScheduleResponse()
                    {
                        AssignResult = AssignWebhookToScheduleResponse.Types.Result.Webhooknotfound,
                    };
                }

                if (schedule.DiscordWebhooks.Select(x => x.WebhookId).Contains(webhook.WebhookId))
                {
                    return new AssignWebhookToScheduleResponse()
                    {
                        AssignResult = AssignWebhookToScheduleResponse.Types.Result.Alreadyexists,
                    };
                }

                dbContext.Update(schedule);
                schedule.DiscordWebhooks.Add(webhook);

                await dbContext.SaveChangesAsync(context.CancellationToken);
                await this.discordClient.InitializeClients(context.CancellationToken);

                return new AssignWebhookToScheduleResponse()
                {
                    AssignResult = AssignWebhookToScheduleResponse.Types.Result.Success,
                };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return new AssignWebhookToScheduleResponse()
                {
                    AssignResult = AssignWebhookToScheduleResponse.Types.Result.Unknown,
                };
            }
        }

        public override async Task<CreateAnimeResponse> CreateAnime(CreateAnimeRequest request, ServerCallContext context)
        {
            this.logger.Info($"Create/Edit anime: {request.Id} - {request.Identifier} - {request.Type} - {request.Filter} - {request.Folder} - {request.Override}");
            using var dbContext = this.getContext();

            try
            {

                if (request.Override)
                {
                    var existingeAnimeInfo = await dbContext.AnimeInfos.Include(x => x.AnimeFilter).Include(x => x.AnimeFolder).FirstOrDefaultAsync(x => x.Id == request.Id, context.CancellationToken);

                    if (existingeAnimeInfo != null)
                    {
                        if (existingeAnimeInfo.Type != request.Type.FromGrpc())
                        {
                            return new CreateAnimeResponse()
                            {
                                CreateResult = CreateAnimeResponse.Types.Result.Typechanged,
                            };
                        }

                        dbContext.Update(existingeAnimeInfo);
                        existingeAnimeInfo.Identifier = request.Identifier;

                        if (string.IsNullOrEmpty(request.Filter))
                        {
                            if (existingeAnimeInfo.AnimeFilter != null)
                            {
                                dbContext.Remove(existingeAnimeInfo.AnimeFilter);
                            }
                        }
                        else
                        {
                            if (existingeAnimeInfo.AnimeFilter == null)
                            {
                                existingeAnimeInfo.AnimeFilter = new AnimeFilter();
                            }

                            existingeAnimeInfo.AnimeFilter.Filter = request.Filter;
                        }

                        if (ValidForFolder(request.Type))
                        {
                            if (string.IsNullOrEmpty(request.Folder))
                            {
                                return new CreateAnimeResponse()
                                {
                                    CreateResult = CreateAnimeResponse.Types.Result.Missingfolder,
                                };
                            }
                            else
                            {
                                existingeAnimeInfo.AnimeFolder.FolderName = request.Folder;
                            }
                        }

                        

                        await dbContext.SaveChangesAsync(context.CancellationToken);

                        return new CreateAnimeResponse()
                        {
                            CreateResult = CreateAnimeResponse.Types.Result.Success,
                        };
                    }
                    else
                    {
                        return new CreateAnimeResponse()
                        {
                            CreateResult = CreateAnimeResponse.Types.Result.Animeinfonotfound,
                        };
                    }
                }

                if (await EntityFrameworkQueryableExtensions.AnyAsync(dbContext.AnimeInfos, x => x.Id == request.Id, context.CancellationToken))
                {
                    return new CreateAnimeResponse()
                    {
                        CreateResult = CreateAnimeResponse.Types.Result.Alreadyexists,
                    };
                }

                if ((await this.anilistClient.GetMediaById(request.Id)) == null)
                {
                    return new CreateAnimeResponse()
                    {
                        CreateResult = CreateAnimeResponse.Types.Result.Animenotfound,
                    };
                }

                var animeInfo = new AnimeInfo()
                {
                    Id = request.Id,
                    Identifier = request.Identifier,
                    Type = request.Type.FromGrpc(),
                };

                if (!string.IsNullOrEmpty(request.Filter))
                {
                    animeInfo.AnimeFilter = new AnimeFilter()
                    {
                        Anime = animeInfo,
                        Filter = request.Filter,
                    };
                }

                if (ValidForFolder(request.Type))
                {
                    if (string.IsNullOrEmpty(request.Folder))
                    {
                        return new CreateAnimeResponse()
                        {
                            CreateResult = CreateAnimeResponse.Types.Result.Missingfolder,
                        };
                    }

                    animeInfo.AnimeFolder = new AnimeFolder()
                    {
                        Anime = animeInfo,
                        FolderName = request.Folder,
                    };
                }

                await dbContext.AnimeInfos.AddAsync(animeInfo, context.CancellationToken);

                await dbContext.SaveChangesAsync(context.CancellationToken);

                return new CreateAnimeResponse()
                {
                    CreateResult = CreateAnimeResponse.Types.Result.Success,
                };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return new CreateAnimeResponse()
                {
                    CreateResult = CreateAnimeResponse.Types.Result.Unknown,
                };
            }
        }

        public override async Task<AssignAnimeInfoToScheduleResponse> AssignAnimeInfoToSchedule(AssignAnimeInfoToScheduleRequest request, ServerCallContext context)
        {
            this.logger.Info($"Assign anime: {request.ScheduleId} - {request.AnimeId}");

            try
            {
                using var dbContext = this.getContext();
                var schedule = await dbContext.Schedules.Include(x => x.Animes).FirstOrDefaultAsync(x => x.Id == request.ScheduleId, context.CancellationToken);
                var animeInfo = await dbContext.AnimeInfos.Include(x => x.Schedules).FirstOrDefaultAsync(x => x.Id == request.AnimeId, context.CancellationToken);

                if (schedule == null)
                {
                    return new AssignAnimeInfoToScheduleResponse()
                    {
                        AssignResult = AssignAnimeInfoToScheduleResponse.Types.Result.Schedulenotfound,
                    };
                }

                if (animeInfo == null)
                {
                    return new AssignAnimeInfoToScheduleResponse()
                    {
                        AssignResult = AssignAnimeInfoToScheduleResponse.Types.Result.Animeinfonotfound,
                    };
                }

                if (schedule.Animes.Select(x => x.Id).Contains(animeInfo.Id))
                {
                    return new AssignAnimeInfoToScheduleResponse()
                    {
                        AssignResult = AssignAnimeInfoToScheduleResponse.Types.Result.Alreadyexists,
                    };
                }

                dbContext.Update(schedule);
                schedule.Animes.Add(animeInfo);

                await dbContext.SaveChangesAsync(context.CancellationToken);

                return new AssignAnimeInfoToScheduleResponse()
                {
                    AssignResult = AssignAnimeInfoToScheduleResponse.Types.Result.Success,
                };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return new AssignAnimeInfoToScheduleResponse()
                {
                    AssignResult = AssignAnimeInfoToScheduleResponse.Types.Result.Unknown,
                };
            }
        }

        public override async Task<UnassignAnimeInfoToScheduleResponse> UnassignAnimeInfoToSchedule(UnassignAnimeInfoToScheduleRequest request, ServerCallContext context)
        {
            this.logger.Info($"Unassign anime: {request.ScheduleId} - {request.AnimeId}");

            try
            {
                using var dbContext = this.getContext();
                var schedule = await dbContext.Schedules.Include(x => x.Animes).FirstOrDefaultAsync(x => x.Id == request.ScheduleId, context.CancellationToken);
                if (schedule == null)
                {
                    return new UnassignAnimeInfoToScheduleResponse()
                    {
                        AssignResult = UnassignAnimeInfoToScheduleResponse.Types.Result.Notexists,
                    };
                }

                var anime = schedule.Animes.FirstOrDefault(x => x.Id == request.AnimeId);

                if (anime == null)
                {
                    return new UnassignAnimeInfoToScheduleResponse()
                    {
                        AssignResult = UnassignAnimeInfoToScheduleResponse.Types.Result.Notexists,
                    };
                }

                dbContext.Update(schedule);
                schedule.Animes.Remove(anime);

                await dbContext.SaveChangesAsync(context.CancellationToken);

                return new UnassignAnimeInfoToScheduleResponse()
                {
                    AssignResult = UnassignAnimeInfoToScheduleResponse.Types.Result.Success,
                };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return new UnassignAnimeInfoToScheduleResponse()
                {
                    AssignResult = UnassignAnimeInfoToScheduleResponse.Types.Result.Unknown,
                };
            }
        }

        public override async Task<GetSchedulesResponse> GetSchedules(GetSchedulesRequest request, ServerCallContext context)
        {
            this.logger.Info($"Get Schedules");
            using var dbContext = this.getContext();
            var schedules = await dbContext.Schedules.AsQueryable().ToArrayAsync(context.CancellationToken);
            var response = new GetSchedulesResponse();

            response.Schedules.AddRange(schedules.Select(x =>
            {
                var state = this.scheduleService.GetScheduleState(x.Id) ?? ScheduleState.Stopped;

                return new GetSchedulesResponse.Types.ScheduleItem()
                {
                    Name = x.Name,
                    Interval = x.Interval.ToDuration(),
                    StartDate = x.StartDate.ToUniversalTime().ToTimestamp(),
                    ScheduleId = x.Id,
                    State = state.ToGrpc(),
                };
            }));

            return response;
        }

        public override async Task<GetAnimesByScheduleResponse> GetAnimesBySchedule(GetAnimesByScheduleRequest request, ServerCallContext context)
        {
            this.logger.Info($"Get Animes by Schedule: {request.ScheduleId}");

            using var dbContext = this.getContext();
            var animeInfos = await dbContext.AnimeInfos
                .Include(x => x.Schedules)
                .Include(x => x.AnimeFilter)
                .Include(x => x.AnimeFolder)
                .Where(x => x.Schedules.Select(y => y.Id).Contains(request.ScheduleId))
                .AsQueryable()
                .ToArrayAsync(context.CancellationToken);
            var response = new GetAnimesByScheduleResponse();
            response.Animes.AddRange(animeInfos.Select(x => new GetAnimesByScheduleResponse.Types.AnimeItem()
            {
                Id = x.Id,
                Filter = x.AnimeFilter?.Filter ?? string.Empty,
                Folder = x.AnimeFolder?.FolderName ?? string.Empty,
                Identifier = x.Identifier,
                Type = x.Type.ToGrpc(),
            }));

            return response;
        }

        public override async Task<GetAnimesResponse> GetAnimes(GetAnimesRequest request, ServerCallContext context)
        {
            this.logger.Info($"Get animes");

            using var dbContext = this.getContext();
            var animeInfos = await dbContext.AnimeInfos
                .Include(x => x.AnimeFilter)
                .Include(x => x.AnimeFolder)
                .AsQueryable()
                .ToArrayAsync(context.CancellationToken);
            var response = new GetAnimesResponse();
            response.Animes.AddRange(animeInfos.Select(x => new GetAnimesResponse.Types.AnimeItem()
            {
                Id = x.Id,
                Filter = x.AnimeFilter?.Filter ?? string.Empty,
                Folder = x.AnimeFolder?.FolderName ?? string.Empty,
                Identifier = x.Identifier,
                Type = x.Type.ToGrpc(),
            }));

            return response;
        }

        public override Task<ForceRunScheduleResponse> ForceRunSchedule(ForceRunScheduleRequest request, ServerCallContext context)
        {
            this.logger.Info($"Force start schedule: {request.ScheduleId}");
            try
            {

                var result = this.scheduleService.ForceRunSchedule(request.ScheduleId);

                return Task.FromResult(new ForceRunScheduleResponse()
                {
                    ForceRunResult = result ? ForceRunScheduleResponse.Types.Result.Success : ForceRunScheduleResponse.Types.Result.Notexists,
                });

            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return Task.FromResult(new ForceRunScheduleResponse()
                {
                    ForceRunResult = ForceRunScheduleResponse.Types.Result.Unknown,
                });
            }
        }

        public override Task<StopScheduleResponse> StopSchedule(StopScheduleRequest request, ServerCallContext context)
        {
            this.logger.Info($"Stopped schedule: {request.ScheduleId}");
            try
            {

                var result = this.scheduleService.StopSchedule(request.ScheduleId);

                return Task.FromResult(new StopScheduleResponse()
                {
                    StopResult = result ? StopScheduleResponse.Types.Result.Success : StopScheduleResponse.Types.Result.Notexists,
                });

            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return Task.FromResult(new StopScheduleResponse()
                {
                    StopResult = StopScheduleResponse.Types.Result.Unknown,
                });
            }
        }

        public override async Task<StartScheduleResponse> StartSchedule(StartScheduleRequest request, ServerCallContext context)
        {
            this.logger.Info($"Start schedule: {request.ScheduleId}");
            try
            {

                var result = await this.scheduleService.StartSchedule(request.ScheduleId);

                return new StartScheduleResponse()
                {
                    StartResult = result ? StartScheduleResponse.Types.Result.Success : StartScheduleResponse.Types.Result.Notexistsorrunning,
                };

            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return new StartScheduleResponse()
                {
                    StartResult = StartScheduleResponse.Types.Result.Unknown,
                };
            }
        }

        public override async Task<TestAnimeResponse> TestAnime(TestAnimeRequest request, ServerCallContext context)
        {
            this.logger.Info($"Test Anime {request.Identifier}");
            var results = await this.scheduleService.TestAnime(new AnimeInfo()
            {
                AnimeFilter = new AnimeFilter() { Filter = request.Filter },
                Id = request.Id,
                Identifier = request.Identifier,
                Type = request.Type.FromGrpc(),
            }, context.CancellationToken);

            var response = new TestAnimeResponse();
            response.Animes.AddRange(results.Select(x => new TestAnimeResponse.Types.AnimeItem()
            {
                EpisodeName = x.EpisodeName,
                EpisodeNumber = x.EpisodeNumber,
                SeasonTitle = x.SeasonTitle,
                SeriesTitle = x.SeriesTitle,
            }));

            return response;
        }

        public override async Task<GetWebhooksResponse> GetWebhooks(GetWebhooksRequest request, ServerCallContext context)
        {
            this.logger.Info($"Get webhooks");

            using var dbContext = this.getContext();
            var webhooks = await dbContext.DiscordWebhooks
                .AsQueryable()
                .ToArrayAsync(context.CancellationToken);
            var response = new GetWebhooksResponse();
            response.Webhooks.AddRange(webhooks.Select(x => new GetWebhooksResponse.Types.WebhhokItem()
            {
                WebhookId = x.WebhookId,
                WebhookToken = x.WebhookToken,
            }));

            return response;
        }

        public override async Task<UnassignWebhookToScheduleResponse> UnassignWebhookToSchedule(UnassignWebhookToScheduleRequest request, ServerCallContext context)
        {
            this.logger.Info($"Unassign webhook: {request.ScheduleId} - {request.WebhookId}");

            try
            {
                using var dbContext = this.getContext();
                var schedule = await dbContext.Schedules.Include(x => x.DiscordWebhooks).FirstOrDefaultAsync(x => x.Id == request.ScheduleId, context.CancellationToken);
                if (schedule == null)
                {
                    return new UnassignWebhookToScheduleResponse()
                    {
                        AssignResult = UnassignWebhookToScheduleResponse.Types.Result.Notexists,
                    };
                }

                var webhook = schedule.DiscordWebhooks.FirstOrDefault(x => x.WebhookId == request.WebhookId);

                if (webhook == null)
                {
                    return new UnassignWebhookToScheduleResponse()
                    {
                        AssignResult = UnassignWebhookToScheduleResponse.Types.Result.Notexists,
                    };
                }

                dbContext.Update(schedule);
                schedule.DiscordWebhooks.Remove(webhook);

                await dbContext.SaveChangesAsync(context.CancellationToken);

                await this.discordClient.InitializeClients(context.CancellationToken);

                return new UnassignWebhookToScheduleResponse()
                {
                    AssignResult = UnassignWebhookToScheduleResponse.Types.Result.Success,
                };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return new UnassignWebhookToScheduleResponse()
                {
                    AssignResult = UnassignWebhookToScheduleResponse.Types.Result.Unknown,
                };
            }
        }

        public override async Task<GetSchedulesByWebhookResponse> GetSchedulesByWebhook(GetSchedulesByWebhookRequest request, ServerCallContext context)
        {
            this.logger.Info($"Get Schedules by Webhook: {request.WebhookId}");

            using var dbContext = this.getContext();
            var animeInfos = await dbContext.Schedules
                .Include(x => x.DiscordWebhooks)
                .Where(x => x.DiscordWebhooks.Select(y => y.WebhookId).Contains(request.WebhookId))
                .AsQueryable()
                .ToArrayAsync(context.CancellationToken);
            var response = new GetSchedulesByWebhookResponse();
            response.Schedules.AddRange(animeInfos.Select(x => new GetSchedulesByWebhookResponse.Types.ScheduleItem()
            {
                ScheduleId = x.Id,
                Name = x.Name,
                // TODO: Add rest
            }));

            return response;
        }

        private static bool ValidForFolder(Animeschedule.AnimeInfoType type)
        {
            switch (type)
            {
                case Animeschedule.AnimeInfoType.Crunchyroll:
                    return false;
                case Animeschedule.AnimeInfoType.Nibl:
                    return true;
                default:
                    return false;
            }
        }
    }
}
