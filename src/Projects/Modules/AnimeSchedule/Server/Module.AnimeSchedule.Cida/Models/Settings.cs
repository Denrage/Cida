using System.Collections.Generic;
using System.Linq;

namespace Module.AnimeSchedule.Cida.Models
{
    public class Settings
    {
        public string DiscordWebhookToken { get; set; }

        public string DiscordWebhookId { get; set; }

        public string MediaFolder { get; set; }

        internal static Settings FromDb(IEnumerable<Database.Settings> settings)
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

        internal static IEnumerable<Database.Settings> ToDb(Settings settings)
        {
            var properties = settings.GetType().GetProperties();
            var result = new List<Database.Settings>();
            foreach (var item in properties)
            {
                result.Add(new Database.Settings()
                {
                    Name = item.Name,
                    Value = item.GetValue(settings).ToString(),
                });
            }

            return result;
        }
    }
}
