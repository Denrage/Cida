using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Module.AnimeSchedule.Cida.Services
{
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

    public class Settings
    {
        public string PlexWebToken { get; set; }

        public string DiscordWebhookToken { get; set; }

        public string DiscordWebhookId { get; set; }

        public string PlexAnimeLibraryUrl { get; set; }

        public string PlexMediaFolder { get; set; }

        internal static Settings FromDb(IEnumerable<Models.Database.Settings> settings)
        {
            var scheduleSettings = new Settings();

            var properties = scheduleSettings.GetType().GetProperties();

            foreach (var item in properties)
            {
                var dbSetting = settings.First(x => x.Name == item.Name);
                item.SetValue(scheduleSettings, dbSetting.Value);
            }

            return scheduleSettings;
        }

        internal static IEnumerable<Models.Database.Settings> ToDb(Settings settings)
        {
            var properties = settings.GetType().GetProperties();
            var result = new List<Models.Database.Settings>();
            foreach (var item in properties)
            {
                result.Add(new Models.Database.Settings()
                {
                    Name = item.Name,
                    Value = item.GetValue(settings).ToString(),
                });
            }

            return result;
        }
    }

    public interface ISettingsService
    {
        Task<Settings> Get(CancellationToken cancellationToken);

        Task Set(Settings settings, CancellationToken cancellationToken);

        event Action OnSettingsChanged;
    }
}
