using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Actions
{
    public class DiscordNotificationActionService : IActionService
    {
        private readonly ILogger logger;

        public DiscordNotificationActionService(ILogger logger)
        {
            this.logger = logger;
        }
        public Task Execute(IAnimeInfo animeInfo, CancellationToken cancellationToken)
        {
            this.logger.Info(animeInfo.Name);
            return Task.CompletedTask;
        }
    }
}
