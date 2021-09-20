using Cida.Client.Avalonia.Api;
using Module.AnimeSchedule.Avalonia.ViewModels.Animes;
using Module.AnimeSchedule.Avalonia.ViewModels.Schedules;
using Module.AnimeSchedule.Avalonia.ViewModels.Webhooks;

namespace Module.AnimeSchedule.Avalonia.ViewModels;

public class AnimeScheduleViewModel : ModuleViewModel
{
    public ScheduleViewModel Schedules { get; set; }

    public AnimeViewModel Animes { get; set; }

    public WebhookViewModel Webhooks { get; set; }

    public override string Name => "Anime Schedule";

    public AnimeScheduleViewModel(Animeschedule.AnimeScheduleService.AnimeScheduleServiceClient client)
    {
        Schedules = new ScheduleViewModel(client);
        Animes = new AnimeViewModel(client);
        Webhooks = new WebhookViewModel();
    }

    public override async Task LoadAsync()
    {
        await Schedules.LoadAsync();
        await Animes.LoadAsync();
    }
}
