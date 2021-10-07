using System;
using System.Threading.Tasks;
using Animeschedule;
using Cida.Client.Avalonia.Api;
using Grpc.Core;
using Module.AnimeSchedule.Avalonia.ViewModels;

namespace Module.AnimeSchedule.Avalonia
{
    public class Module : IModule
    {
        private AnimeScheduleService.AnimeScheduleServiceClient client;

        public string Name => "Anime Schedule";

        public ModuleViewModel ViewModel { get; private set; }

        public Task LoadAsync(Channel channel, ISettingsFactory settingsFactory)
        {
            this.client = new AnimeScheduleService.AnimeScheduleServiceClient(channel);
            this.ViewModel = new AnimeScheduleViewModel(client);
            return Task.CompletedTask;
        }
    }
}
