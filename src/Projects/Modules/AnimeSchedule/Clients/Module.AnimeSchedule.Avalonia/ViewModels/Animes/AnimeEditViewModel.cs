namespace Module.AnimeSchedule.Avalonia.ViewModels.Animes;

public class AnimeEditViewModel : ViewModelBase
{
    private readonly AnimeInfo animeInfo;

    public AnimeInfo Anime => this.animeInfo;

    public AnimeType[] PossibleTypes => Enum.GetValues(typeof(AnimeType)).Cast<AnimeType>().ToArray();

    public AnimeEditViewModel(AnimeInfo animeInfo)
    {
        this.animeInfo = animeInfo;
    }
}
