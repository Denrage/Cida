﻿using Module.AnimeSchedule.Cida.Services;

namespace Module.AnimeSchedule.Cida.Interfaces
{
    public interface ISettingsService
    {
        Task<Settings> Get(CancellationToken cancellationToken);

        Task Set(Settings settings, CancellationToken cancellationToken);

        event Action OnSettingsChanged;
    }
}
