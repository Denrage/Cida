using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models;

namespace Module.AnimeSchedule.Cida.Services;

public class DatabaseSettings
{
    public string Key {  get; set; }
    public string Value {  get; set; }
}

public class Settings
{
    public string MediaFolder
    {
        get => this.KeyValuePairs[nameof(MediaFolder)];
        set => this.KeyValuePairs[nameof(MediaFolder)] = value;
    }

    public Dictionary<string, string> KeyValuePairs = new Dictionary<string, string>()
    {
        { nameof(MediaFolder), string.Empty },
    };

    internal static Settings FromDb(IEnumerable<DatabaseSettings> dbSettings)
    {
        var settings = new Settings();
        foreach (var item in dbSettings)
        {
            settings.KeyValuePairs[item.Key] = item.Value;
        }

        return settings;
    }

    internal static IEnumerable<DatabaseSettings> ToDb(Settings settings)
    {
        foreach (var item in settings.KeyValuePairs)
        {
            yield return new DatabaseSettings()
            {
                Key = item.Key,
                Value = item.Value
            };
        }
    }
}

public class SettingsService : ISettingsService
{
    private readonly Func<AnimeScheduleDbContext> getContext;
    private Settings cache;

    public event Action OnSettingsChanged;

    public SettingsService(Func<AnimeScheduleDbContext> getContext)
    {
        this.getContext = getContext;
    }

    public async Task Set(Settings settings, CancellationToken cancellationToken)
    {
        this.cache = settings;

        using var context = this.getContext();
        foreach (var item in context.Settings)
        {
            context.Settings.Remove(item);
        }

        await context.Settings.AddRangeAsync(Settings.ToDb(settings), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

#pragma warning disable CS4014 // Exactly what we want to do
        Task.Run(() => this.OnSettingsChanged?.Invoke());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    public async Task<Settings> Get(CancellationToken cancellationToken)
    {
        if (this.cache is null)
        {
            using var context = this.getContext();

            this.cache = Settings.FromDb(await EntityFrameworkQueryableExtensions.ToArrayAsync(context.Settings, cancellationToken));
        }

        return this.cache;
    }
}
