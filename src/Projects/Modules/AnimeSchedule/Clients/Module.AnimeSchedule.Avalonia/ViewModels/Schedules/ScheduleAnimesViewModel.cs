using Animeschedule;
using Avalonia.Collections;
using Cida.Client.Avalonia.Api;
using Module.AnimeSchedule.Avalonia.Models;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Schedules;

public class AnimeComparer : IEqualityComparer<AnimeInfo>
{
    public bool Equals(AnimeInfo x, AnimeInfo y)
        => x.Id.Equals(y.Id);

    public int GetHashCode([DisallowNull] AnimeInfo obj)
        => obj.Id.GetHashCode();
}

public class ScheduleAnimesViewModel : AssignmentViewModel<Schedule, AnimeInfo, AnimeComparer>
{
   private readonly AnimeScheduleService.AnimeScheduleServiceClient client;

   public ScheduleAnimesViewModel(AnimeScheduleService.AnimeScheduleServiceClient client, Schedule schedule)
        : base(schedule)
    {
        this.client = client;
    }

    protected override void Add(AnimeInfo item) => this.Item.Animes.Add(item);

    protected override async Task<bool> AssignInternal(AnimeInfo item)
    {
        var assignResult = await client.AssignAnimeInfoToScheduleAsync(new AssignAnimeInfoToScheduleRequest()
        {
            AnimeId = item.Id,
            ScheduleId = this.Item.ScheduleId,
        });

        return assignResult.AssignResult == AssignAnimeInfoToScheduleResponse.Types.Result.Success;
    }

    protected override async Task<IEnumerable<AnimeInfo>> GetAssignableItems()
    {
        var animes = await this.client.GetAnimesAsync(new GetAnimesRequest());

        return animes.Animes.Select(x => new AnimeInfo()
        {
            Identifier = x.Identifier,
            Id = x.Id,
        });
    }

    protected override IEnumerable<AnimeInfo> GetAssignedItems()
        => this.Item.Animes;

    protected override void Remove(AnimeInfo item)
        => this.Item.Animes.Remove(item);

    protected override async Task<bool> UnassignInternal(AnimeInfo item)
    {
        var unassignResult = await client.UnassignAnimeInfoToScheduleAsync(new UnassignAnimeInfoToScheduleRequest()
        {
            AnimeId = item.Id,
            ScheduleId = this.Item.ScheduleId,
        });

        return unassignResult.AssignResult == UnassignAnimeInfoToScheduleResponse.Types.Result.Success;
    }
}
