using System;
using System.Runtime.CompilerServices;
using Module.Crunchyroll.Libs;
using Module.Crunchyroll.Libs.Models.Details;
using Module.Crunchyroll.Libs.Models.Search;
using CR = Crunchyroll;
using Data = Module.Crunchyroll.Libs.Models.Details.Data;

namespace Module.Crunchyroll.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static CR.SearchResponse.Types.SearchItem ToGrpc(this Data data) =>
            new CR.SearchResponse.Types.SearchItem()
            {
                Name = data.Name,
                Id = data.SeriesId,
                PortraitImage = data.PortraitImage?.ToGrpc(),
                LandscapeImage = data.LandscapeImage?.ToGrpc(),
                Url = data.Url.OriginalString,
                Description = data.Description,
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
    }
}