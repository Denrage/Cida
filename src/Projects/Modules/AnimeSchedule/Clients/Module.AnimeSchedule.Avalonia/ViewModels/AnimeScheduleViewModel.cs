using Cida.Client.Avalonia.Api;

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
        this.Animes =  new AnimeViewModel(client);
        this.Webhooks = new WebhookViewModel();
    }

    public override async Task LoadAsync()
    {
        await this.Schedules.LoadAsync();
        await this.Animes.LoadAsync();
    }
}
