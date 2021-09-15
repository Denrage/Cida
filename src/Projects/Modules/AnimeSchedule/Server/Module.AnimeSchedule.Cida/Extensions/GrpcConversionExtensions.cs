using Module.AnimeSchedule.Cida.Models;
using AnimeS = Animeschedule;

namespace Module.AnimeSchedule.Cida.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static AnimeInfoType FromGrpc(this AnimeS.AnimeInfoType data)
        {
            return data switch
            {
                AnimeS.AnimeInfoType.Crunchyroll => AnimeInfoType.Crunchyroll,
                AnimeS.AnimeInfoType.Nibl => AnimeInfoType.Nibl,
                _ => AnimeInfoType.Crunchyroll,
            };
        }

        public static AnimeS.AnimeInfoType ToGrpc(this AnimeInfoType data)
        {
            return data switch
            {
                AnimeInfoType.Crunchyroll => AnimeS.AnimeInfoType.Crunchyroll,
                AnimeInfoType.Nibl => AnimeS.AnimeInfoType.Crunchyroll,
                _ => AnimeS.AnimeInfoType.Crunchyroll,
            };
        }
    }
}