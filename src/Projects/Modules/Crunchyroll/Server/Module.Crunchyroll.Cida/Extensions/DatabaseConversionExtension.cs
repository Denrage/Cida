using Module.Crunchyroll.Libs.Models.Database;
using Module.Crunchyroll.Libs.Models.Details;
using Image = Module.Crunchyroll.Libs.Models.Database.Image;

namespace Module.Crunchyroll.Cida.Extensions
{
    public static class DatabaseConversionExtension
    {
        public static Anime ToDatabaseModel(this Details data)
            => new Anime()
            {
                Id = data.SeriesId,
                Name = data.Name,
                Url = data.Url.OriginalString,
                Description = data.Description,
                Portrait = data.PortraitImage?.ToDatabaseModel(),
                Landscape = data.LandscapeImage?.ToDatabaseModel(),
            };

        public static Episode ToDatabaseModel(this Libs.Models.Episode.Episode episode)
            => new Episode()
            {
                CollectionId = episode.CollectionId,
                Available = episode.Available,
                Description = episode.Description,
                Id = episode.MediaId,
                Name = episode.Name,
                Duration = episode.
                Url = episode.Url.OriginalString,
                AvailabilityNotes = episode.AvailabilityNotes,
                EpisodeNumber = episode.EpisodeNumber,
                FreeAvailable = episode.FreeAvailable,
                PremiumAvailable = episode.PremiumAvailable,
                Image = episode.ScreenshotImage?.ToDatabaseModel(),
            };

        public static Collection ToDatabaseModel(this Libs.Models.Collection.Collection collection)
            => new Collection()
            {
                Complete = collection.Complete,
                Created = collection.Created,
                Description = collection.Description,
                Id = collection.CollectionId,
                Name = collection.Name,
                Season = collection.Season,
                AvailabilityNotes = collection.AvailabilityNotes,
                AnimeId = collection.SeriesId,
                Landscape = collection.LandscapeImage?.ToDatabaseModel(),
                Portrait = collection.PortraitImage?.ToDatabaseModel(),
            };

        public static Image ToDatabaseModel(this Libs.Models.Image image)
            => new Image()
            {
                Full = image.FullUrl.OriginalString,
                Height = image.Height,
                Large = image.LargeUrl.OriginalString,
                Medium = image.MediumUrl.OriginalString,
                Small = image.SmallUrl.OriginalString,
                Thumbnail = image.ThumbUrl.OriginalString,
                Wide = image.WideUrl.OriginalString,
                Width = image.Width,
                FullWide = image.FwideUrl.OriginalString,
                WideWithStar = image.WidestarUrl.OriginalString,
                FullWideWithStar = image.FwidestarUrl.OriginalString,
            };
    }
}