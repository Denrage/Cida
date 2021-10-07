using Module.AnimeSchedule.Avalonia.Models;
using AnimeS = Animeschedule;

namespace Module.AnimeSchedule.Avalonia.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static AnimeS.AnimeInfoType ToGrpc(this AnimeType data)
        {
            return data switch
            {
                AnimeType.Crunchyroll => AnimeS.AnimeInfoType.Crunchyroll,
                AnimeType.Nibl => AnimeS.AnimeInfoType.Nibl,
                _ => AnimeS.AnimeInfoType.Crunchyroll,
            };
        }

        public static AnimeType FromGrpc(this AnimeS.AnimeInfoType data)
        {
            return data switch
            {
                AnimeS.AnimeInfoType.Crunchyroll => AnimeType.Crunchyroll,
                AnimeS.AnimeInfoType.Nibl => AnimeType.Nibl,
                _ => AnimeType.Crunchyroll,
            };
        }
    }
}