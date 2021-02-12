using Module.AnimeSchedule.Cida.Interfaces;

namespace Module.AnimeSchedule.Cida.Models.Schedule
{
    public abstract class AnimeInfoContext<TSourceService> : AnimeInfoContext
       where TSourceService : ISourceService
    {
        protected AnimeInfoContext(ISourceService sourceService)
            : base(sourceService)
        {
        }

        protected new TSourceService SourceService => (TSourceService)base.SourceService;
    }
}
