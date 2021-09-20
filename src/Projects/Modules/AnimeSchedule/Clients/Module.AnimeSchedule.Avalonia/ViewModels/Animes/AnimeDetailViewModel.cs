namespace Module.AnimeSchedule.Avalonia.ViewModels.Animes;

public class AnimeDetailViewModel : ViewModelBase
{
    public AnimeEditViewModel AnimeEdit {  get; set; }
    public AnimeTestResultViewModel AnimeTestResult {  get; set; }

    public AnimeDetailViewModel(Animeschedule.AnimeScheduleService.AnimeScheduleServiceClient client, AnimeInfo animeInfo)
    {
        this.AnimeEdit = new AnimeEditViewModel(animeInfo);
        this.AnimeTestResult = new AnimeTestResultViewModel();
    }

    public async Task Save()
    {

    }

    public async Task Test()
    {
        this.AnimeTestResult.Test();
    }
}
