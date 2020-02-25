using Module.Crunchyroll.Libs.Models.Database;
using CR = Crunchyroll;
using Image = Module.Crunchyroll.Libs.Models.Database.Image;

namespace Module.Crunchyroll.Cida.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static CR.SearchResponse.Types.SearchItem ToGrpc(this Anime data) =>
            new CR.SearchResponse.Types.SearchItem()
            {
                Name = data.Name,
                Id = data.Id,
                PortraitImage = data.Portrait?.ToGrpc(),
                LandscapeImage = data.Landscape?.ToGrpc(),
                Url = data.Url,
                Description = data.Description,
            };

        public static CR.Image ToGrpc(this Image image)
            => new CR.Image()
            {
                Full = image.Full,
                FullWide = image.FullWide,
                FullWideWithStar = image.FullWideWithStar,
                Height = image.Height,
                Large = image.Large,
                Medium = image.Medium,
                Small = image.Small,
                Thumbnail = image.Thumbnail,
                Wide = image.Wide,
                WideWithStar = image.WideWithStar,
                Width = image.Width,
            };

        public static CR.EpisodeResponse.Types.EpisodeItem ToGrpc(this Episode episode) =>
            new CR.EpisodeResponse.Types.EpisodeItem()
            {
                Name = episode.Name,
                Id = episode.Id,
                EpisodeNumber = episode.EpisodeNumber,
                Description = episode.Description,
            };

        public static CR.CollectionsResponse.Types.CollectionItem ToGrpc(this Collection collection) =>
            new CR.CollectionsResponse.Types.CollectionItem()
            {
                Name = collection.Name,
                Id = collection.Id,
                Description = collection.Description,
                AvailabilityNotes = collection.AvailabilityNotes,
                Complete = collection.Complete,
                Created = collection.Created,
                Landscape = collection.Landscape?.ToGrpc(),
                Portrait = collection.Portrait?.ToGrpc(),
                Season = collection.Season,
            };
    }
}