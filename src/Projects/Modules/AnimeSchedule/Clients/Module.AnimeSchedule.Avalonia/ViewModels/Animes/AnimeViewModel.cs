using Animeschedule;
using Avalonia.Collections;
using Cida.Client.Avalonia.Api;
using Module.AnimeSchedule.Avalonia.Extensions;
using Module.AnimeSchedule.Avalonia.Models;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Animes;

public class AnimeViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;

    public AvaloniaList<AnimeInfo> Animes { get; } = new();

    public AnimeInfo SelectedAnime { get; set; }

    public ViewModelBase SubViewModel { get; set; }

    public AnimeViewModel(AnimeScheduleService.AnimeScheduleServiceClient client)
    {
        this.client = client;
        this.SubViewModel = new AnimeDetailViewModel(client, new AnimeInfo());
    }

    public async Task LoadAsync()
    {
        Animes.Clear();

        var result = await client.GetAnimesAsync(new GetAnimesRequest());
        Animes.AddRange(result.Animes.Select(x => new AnimeInfo()
        {
            Id = x.Id,
            Identifier = x.Identifier,
            Type = x.Type.FromGrpc(),
        }));
    }
}
