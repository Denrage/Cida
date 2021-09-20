using Module.AnimeSchedule.Avalonia.Models;
using AnimeS = Animeschedule;

namespace Module.AnimeSchedule.Avalonia.Extensions
{
    public static class GrpcConversionExtensions
    {
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