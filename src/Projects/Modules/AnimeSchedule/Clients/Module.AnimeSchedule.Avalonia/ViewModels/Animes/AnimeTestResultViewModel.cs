using Animeschedule;
using Avalonia.Collections;
using Module.AnimeSchedule.Avalonia.Extensions;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Animes;

public class AnimeTestResultViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private readonly AnimeInfo animeInfo;

    public AvaloniaList<Animeschedule.TestAnimeResponse.Types.AnimeItem> AnimeTestResults { get; } = new();

    public AnimeTestResultViewModel(Animeschedule.AnimeScheduleService.AnimeScheduleServiceClient client, AnimeInfo animeInfo)
    {
        this.client = client;
        this.animeInfo = animeInfo;
    }

    public async Task Test()
    {
        var result = await this.client.TestAnimeAsync(new TestAnimeRequest()
        {
            Filter = this.animeInfo.Filter,
            Id = this.animeInfo.Id,
            Identifier = this.animeInfo.Identifier,
            Type = this.animeInfo.Type.ToGrpc(),
        });

        this.AnimeTestResults.Clear();
        this.AnimeTestResults.AddRange(result.Animes);
    }
}
