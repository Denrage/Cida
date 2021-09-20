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
                AnimeInfoType.Nibl => AnimeS.AnimeInfoType.Nibl,
                _ => AnimeS.AnimeInfoType.Crunchyroll,
            };
        }

        public static AnimeS.GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState ToGrpc(this ScheduleState data)
        {
            return data switch
            {
                ScheduleState.Running => AnimeS.GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState.Running,
                ScheduleState.Waiting => AnimeS.GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState.Waiting,
                ScheduleState.Stopped => AnimeS.GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState.Stopped,
                _ => AnimeS.GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState.Stopped,
            };
        }
    }
}