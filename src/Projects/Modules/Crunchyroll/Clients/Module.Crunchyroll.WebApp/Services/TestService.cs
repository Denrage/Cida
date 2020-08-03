using Grpc.Core;
using Grpc.Net.Client;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CrunchyrollApi = Crunchyroll;
using CrunchyrollLibs = Module.Crunchyroll.Libs.Models;

namespace Module.Crunchyroll.WebApp.Services
{
    public class TestService
    {
        private CrunchyrollApi.CrunchyrollService.CrunchyrollServiceClient client;

        public TestService(GrpcChannel channel)
        {
            //var channel = new Channel("127.0.0.1", 31564, ChannelCredentials.Insecure);
            this.client = new CrunchyrollApi.CrunchyrollService.CrunchyrollServiceClient(channel);
        }

        public async Task<IEnumerable<CrunchyrollLibs.Search.SearchItem>> Search(string searchTerm)
        {
            var searchResponse = await this.client.SearchAsync(new CrunchyrollApi.SearchRequest()
            {
                SearchTerm = searchTerm,
            });

            return searchResponse.Items.Select(x => new CrunchyrollLibs.Search.SearchItem()
            {
                Id = x.Id,
                Link = x.Url,
                Name = x.Name,
                Img = new Uri(x.LandscapeImage.Full),
            });
        }

        public async Task<IEnumerable<CrunchyrollLibs.Collection.Collection>> GetCollections(string seriesId)
        {
            var collection = await this.client.GetCollectionsAsync(new CrunchyrollApi.CollectionsRequest()
            {
                Id = seriesId,
            });

            return collection.Collections.Select(x => new CrunchyrollLibs.Collection.Collection()
            {
                Complete = x.Complete,
                Description = x.Description,
                AvailabilityNotes = x.AvailabilityNotes,
                Created = x.Created,
                Season = x.Season,
                SeriesId = seriesId,
                Name = x.Name,
                CollectionId = x.Id,
            });
        }

        public async Task<IEnumerable<CrunchyrollLibs.Episode.Episode>> GetEpisodes(string collectionId)
        {
            var episodes = await this.client.GetEpisodesAsync(new CrunchyrollApi.EpisodeRequest()
            {
                Id = collectionId,
            });

            return episodes.Episodes.Select(x => new CrunchyrollLibs.Episode.Episode() 
            { 
                CollectionId = collectionId,
                Name = x.Name,
                Url = x.Url == string.Empty ? null : new Uri(x.Url),
                Description = x.Description,
                AvailabilityNotes = x.AvailabilityNotes,
                Available = x.Available,
                EpisodeNumber = x.EpisodeNumber,
                FreeAvailable = x.FreeAvailable,
                PremiumAvailable = x.PremiumAvailable,
                SeriesId = collectionId,
                //TODO: This is not media ID, maybe move the loading to CollectionDetailView (where the EpisodeDetailView is created)
                MediaId = x.Id,
            });
        }

        public async Task<string> GetStreamUrl(string episodeId)
        {
            var stream = await this.client.GetEpisodeStreamAsync(new CrunchyrollApi.EpisodeStreamRequest()
            {
                Id = episodeId,
            });
            return stream.StreamUrl;
        }
    }
}
