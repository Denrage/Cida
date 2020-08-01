using Module.HorribleSubs.Cida.Models;
using HS = Horriblesubs;

namespace Module.Crunchyroll.Cida.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static HS.SearchResponse.Types.SearchResult ToGrpc(this SearchResult data) =>
            new HS.SearchResponse.Types.SearchResult()
            {
                BotName = data.BotName,
                FileName = data.FileName,
                FileSize = data.FileSize,
                PackageNumber = data.PackageNumber,
            };

        public static DownloadRequest FromGrpc(this HS.DownloadRequest.Types.Request data) =>
            new DownloadRequest()
            {
                BotName = data.BotName,
                FileName = data.FileName,
                PackageNumber = data.PackageNumber,
            };
    }
}