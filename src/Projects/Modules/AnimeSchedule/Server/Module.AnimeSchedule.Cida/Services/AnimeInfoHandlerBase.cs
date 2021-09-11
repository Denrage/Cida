﻿using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models;
using NLog;

namespace Module.AnimeSchedule.Cida.Services;

public abstract class AnimeInfoHandlerBase
{
    protected Func<AnimeScheduleDbContext> GetContext { get; }
    protected ILogger Logger { get; }

    public AnimeInfoHandlerBase(ILogger logger, Func<AnimeScheduleDbContext> getContext)
    {
        this.GetContext = getContext;
        this.Logger = logger;
    }

    public abstract Task<IEnumerable<IActionable>> GetNewEpisodes(AnimeInfo animeInfo, CancellationToken cancellationToken);
}
