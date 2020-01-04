using System;
using Module.Crunchyroll.Libs.Models.Details;
using Module.Crunchyroll.Libs.Models.Episode;
using CR = Crunchyroll;

namespace Module.Crunchyroll.Cida.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static CR.SearchResponse.Types.SearchItem ToGrpc(this Details details) =>
            new CR.SearchResponse.Types.SearchItem()
            {
                Name = details.Name,
                Id = details.SeriesId,
                PortraitImage = details.PortraitImage?.ToGrpc(),
                LandscapeImage = details.LandscapeImage?.ToGrpc(),
                Url = details.Url.OriginalString,
                Description = details.Description,
            };

        public static CR.SearchResponse.Types.SearchItem.Types.Image ToGrpc(this Image image)
            => new CR.SearchResponse.Types.SearchItem.Types.Image()
            {
                Full = image.FullUrl.OriginalString,
                FullWide = image.FwideUrl.OriginalString,
                FullWideWithStar = image.FwidestarUrl.OriginalString,
                Height = image.Height,
                Large = image.LargeUrl.OriginalString,
                Medium = image.MediumUrl.OriginalString,
                Small = image.SmallUrl.OriginalString,
                Thumbnail = image.ThumbUrl.OriginalString,
                Wide = image.WideUrl.OriginalString,
                WideWithStar = image.WidestarUrl.OriginalString,
                Width = image.Width,
            };

        public static CR.EpisodeResponse.Types.EpisodeItem ToGrpc(this Episode episode) =>
            new CR.EpisodeResponse.Types.EpisodeItem()
            {
                Name = episode.Name,
                Id = episode.MediaId,
                Description = episode.Description,
            };
    }
}