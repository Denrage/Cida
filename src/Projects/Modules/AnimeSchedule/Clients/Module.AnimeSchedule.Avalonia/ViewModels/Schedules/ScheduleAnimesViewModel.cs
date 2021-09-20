using Animeschedule;
using Avalonia.Collections;
using Cida.Client.Avalonia.Api;
using Module.AnimeSchedule.Avalonia.Models;
using ReactiveUI;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Schedules;

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
            if (schedule != value)
            {
                schedule = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(ScheduleAnimes));
                this.RaisePropertyChanged(nameof(NotInScheduleAnime));
            }
        }
    }

    public AvaloniaList<AnimeInfo> ScheduleAnimes => new AvaloniaList<AnimeInfo>(Schedule?.Animes ?? Enumerable.Empty<AnimeInfo>());

    public AvaloniaList<AnimeInfo> NotInScheduleAnime => new AvaloniaList<AnimeInfo>(allAnimes.Where(x => !ScheduleAnimes.Select(y => y.Id).Contains(x.Id)));

    public ScheduleAnimesViewModel(AnimeScheduleService.AnimeScheduleServiceClient client, Schedule schedule)
    {
        this.client = client;
        this.schedule = schedule;
        Task.Run(async () =>
        {
            var animes = await this.client.GetAnimesAsync(new GetAnimesRequest());

            allAnimes.AddRange(animes.Animes.Select(x => new AnimeInfo()
            {
                Identifier = x.Identifier,
                Id = x.Id,
            }));
            this.RaisePropertyChanged(nameof(NotInScheduleAnime));
        });
    }

    public async Task AssignAnime(AnimeInfo animeInfo)
    {
        var assignResult = await client.AssignAnimeInfoToScheduleAsync(new AssignAnimeInfoToScheduleRequest()
        {
            AnimeId = animeInfo.Id,
            ScheduleId = Schedule.ScheduleId,
        });

        if (assignResult.AssignResult == AssignAnimeInfoToScheduleResponse.Types.Result.Success)
        {
            Schedule.Animes.Add(animeInfo);
            this.RaisePropertyChanged(nameof(ScheduleAnimes));
            this.RaisePropertyChanged(nameof(NotInScheduleAnime));
        }
    }

    public async Task UnassignAnime(AnimeInfo animeInfo)
    {
        var unassignResult = await client.UnassignAnimeInfoToScheduleAsync(new UnassignAnimeInfoToScheduleRequest()
        {
            AnimeId = animeInfo.Id,
            ScheduleId = Schedule.ScheduleId,
        });

        if (unassignResult.AssignResult == UnassignAnimeInfoToScheduleResponse.Types.Result.Success)
        {
            Schedule.Animes.Remove(animeInfo);
            this.RaisePropertyChanged(nameof(ScheduleAnimes));
            this.RaisePropertyChanged(nameof(NotInScheduleAnime));
        }
    }
}
