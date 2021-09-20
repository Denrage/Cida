using Animeschedule;
using Avalonia.Collections;
using Cida.Client.Avalonia.Api;
using Module.AnimeSchedule.Avalonia.Models;
using ReactiveUI;

namespace Module.AnimeSchedule.Avalonia.ViewModels;

public class ScheduleAnimesViewModel : ViewModelBase
{
    private readonly List<AnimeInfo> allAnimes = new List<AnimeInfo>();
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private Schedule schedule;

    public Schedule Schedule
    {
        get => schedule;
        set
        {
            if (this.schedule != value)
            {
                this.schedule = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.ScheduleAnimes));
                this.RaisePropertyChanged(nameof(this.NotInScheduleAnime));
            }
        }
    }

    public AvaloniaList<AnimeInfo> ScheduleAnimes => new AvaloniaList<AnimeInfo>(this.Schedule?.Animes ?? Enumerable.Empty<AnimeInfo>());

    public AvaloniaList<AnimeInfo> NotInScheduleAnime => new AvaloniaList<AnimeInfo>(this.allAnimes.Where(x => !this.ScheduleAnimes.Select(y => y.Id).Contains(x.Id)));

    public ScheduleAnimesViewModel(AnimeScheduleService.AnimeScheduleServiceClient client, Schedule schedule)
    {
        this.client = client;
        this.schedule = schedule;
        Task.Run(async () =>
        {
            var animes = await this.client.GetAnimesAsync(new GetAnimesRequest());

            this.allAnimes.AddRange(animes.Animes.Select(x => new AnimeInfo()
            {
                Identifier = x.Identifier,
                Id = x.Id,
            }));
            this.RaisePropertyChanged(nameof(this.NotInScheduleAnime));
        });
    }

    public async Task AssignAnime(AnimeInfo animeInfo)
    {
        var assignResult = await this.client.AssignAnimeInfoToScheduleAsync(new AssignAnimeInfoToScheduleRequest()
        {
            AnimeId = animeInfo.Id,
            ScheduleId = this.Schedule.ScheduleId,
        });

        if (assignResult.AssignResult == AssignAnimeInfoToScheduleResponse.Types.Result.Success)
        {
            this.Schedule.Animes.Add(animeInfo);
            this.RaisePropertyChanged(nameof(this.ScheduleAnimes));
            this.RaisePropertyChanged(nameof(this.NotInScheduleAnime));
        }
    }

    public async Task UnassignAnime(AnimeInfo animeInfo)
    {
        var unassignResult = await this.client.UnassignAnimeInfoToScheduleAsync(new UnassignAnimeInfoToScheduleRequest()
        {
            AnimeId = animeInfo.Id,
            ScheduleId = this.Schedule.ScheduleId,
        });

        if (unassignResult.AssignResult == UnassignAnimeInfoToScheduleResponse.Types.Result.Success)
        {
            this.Schedule.Animes.Remove(animeInfo);
            this.RaisePropertyChanged(nameof(this.ScheduleAnimes));
            this.RaisePropertyChanged(nameof(this.NotInScheduleAnime));
        }
    }
}
