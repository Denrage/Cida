using Module.IrcAnime.Cida.Models;
using IrcA = Ircanime;

namespace Module.Crunchyroll.Cida.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static IrcA.SearchResponse.Types.SearchResult ToGrpc(this SearchResult data) =>
            new IrcA.SearchResponse.Types.SearchResult()
            {
                BotName = data.BotName,
                FileName = data.FileName,
                FileSize = data.FileSize,
                PackageNumber = data.PackageNumber,
            };

        public static DownloadRequest FromGrpc(this IrcA.DownloadRequest.Types.Request data) =>
            new DownloadRequest()
            {
                BotName = data.BotName,
                FileName = data.FileName,
                PackageNumber = data.PackageNumber,
            };
    }
}