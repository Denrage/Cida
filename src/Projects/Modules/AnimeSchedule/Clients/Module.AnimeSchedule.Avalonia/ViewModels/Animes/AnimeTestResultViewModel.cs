using Animeschedule;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Animes;

public class AnimeTestResultViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;

    public AnimeTestResultViewModel(Animeschedule.AnimeScheduleService.AnimeScheduleServiceClient client)
    {
        this.client = client;
    }

    public async Task Test()
    {
    }
}
