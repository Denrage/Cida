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
        this.Schedules = new ScheduleViewModel(client);
        this.Animes = new AnimeViewModel(client);
        this.Webhooks = new WebhookViewModel(client);
    }

    public override async Task LoadAsync()
    {
        await this.Schedules.LoadAsync();
        await this.Animes.LoadAsync();
        await this.Webhooks.LoadAsync();
    }
}
