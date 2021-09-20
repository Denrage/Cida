using Animeschedule;
using Avalonia.Collections;
using Cida.Client.Avalonia.Api;
using Module.AnimeSchedule.Avalonia.Extensions;
using Module.AnimeSchedule.Avalonia.Models;

namespace Module.AnimeSchedule.Avalonia.ViewModels;

public class AnimeViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;

    public AvaloniaList<AnimeInfo> Animes { get; } = new();

    public AnimeInfo SelectedAnime { get; set; }

    public AnimeViewModel(Animeschedule.AnimeScheduleService.AnimeScheduleServiceClient client)
    {
        this.client = client;
    }

    public async Task LoadAsync()
    {
        this.Animes.Clear();

        var result = await this.client.GetAnimesAsync(new GetAnimesRequest());
        this.Animes.AddRange(result.Animes.Select(x => new AnimeInfo()
        {
            Id = x.Id,
            Identifier = x.Identifier,
            Type = x.Type.FromGrpc(),
        }));
    }
}
