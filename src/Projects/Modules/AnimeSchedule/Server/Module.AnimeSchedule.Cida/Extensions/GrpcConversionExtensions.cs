using Module.AnimeSchedule.Cida.Models;
using AnimeS = Animeschedule;

namespace Module.AnimeSchedule.Cida.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static AnimeInfoType FromGrpc(this AnimeS.CreateAnimeRequest.Types.AnimeInfoType data)
        {
            return data switch
            {
                AnimeS.CreateAnimeRequest.Types.AnimeInfoType.Crunchyroll => AnimeInfoType.Crunchyroll,
                AnimeS.CreateAnimeRequest.Types.AnimeInfoType.Nibl => AnimeInfoType.Nibl,
                _ => AnimeInfoType.Crunchyroll,
            };
        }
    }
}