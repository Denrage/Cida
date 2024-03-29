﻿using Animeschedule;
using Module.AnimeSchedule.Avalonia.Extensions;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Animes;

public class AnimeDetailViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private readonly AnimeInfo animeInfo;
    private readonly Action<AnimeInfo> onChange;

    public AnimeEditViewModel AnimeEdit { get; set; }
    public AnimeTestResultViewModel AnimeTestResult { get; set; }

    public AnimeDetailViewModel(Animeschedule.AnimeScheduleService.AnimeScheduleServiceClient client, AnimeInfo animeInfo, Action<AnimeInfo> onChange)
    {
        this.AnimeEdit = new AnimeEditViewModel(animeInfo);
        this.AnimeTestResult = new AnimeTestResultViewModel(client, animeInfo);
        this.client = client;
        this.animeInfo = animeInfo;
        this.onChange = onChange;
    }

    public async Task Save()
    {
        var existingAnime = (await this.client.GetAnimesAsync(new GetAnimesRequest())).Animes.FirstOrDefault(x => x.Id == this.animeInfo.Id);

        await this.client.CreateAnimeAsync(new CreateAnimeRequest()
        {
            Filter = this.animeInfo.Filter,
            Folder = this.animeInfo.AnimeFolder,
            Id = this.animeInfo.Id,
            Identifier = this.animeInfo.Identifier,
            Type = this.animeInfo.Type.ToGrpc(),
            Override = existingAnime != null,
        });

        this.onChange(this.animeInfo);
    }

    public async Task Test()
    {
        await this.AnimeTestResult.Test();
    }
}
