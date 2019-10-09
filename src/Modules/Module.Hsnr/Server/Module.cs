using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cida.Api;
using Grpc.Core;
using Hsnr;
using Module.Hsnr.Extensions;
using Module.Hsnr.Timetable;
using Module.Hsnr.Timetable.Data;
using Module.Hsnr.Timetable.Parser;
using SemesterType = Module.Hsnr.Timetable.Data.SemesterType;

namespace Module.Hsnr
{
    public class Module : IModule
    {
        public void Load()
        {
        }

        public IEnumerable<ServerServiceDefinition> GrpcServices { get; } = new[]
        {
            HsnrService.BindService(new HsnrServiceImplementation()),
            HsnrTimetableService.BindService(new HsnrTimetableServiceImplementation()),
        };

    }

    public class HsnrServiceImplementation : HsnrService.HsnrServiceBase
    {
        public override Task<VersionResponse> Version(VersionRequest request, ServerCallContext context)
        {
            return Task.FromResult(new VersionResponse() {Version = 1});
        }
    }

    public class HsnrTimetableServiceImplementation : HsnrTimetableService.HsnrTimetableServiceBase
    {
        private readonly Timetable.TimetableService timetableService;

        public HsnrTimetableServiceImplementation()
        {
            this.timetableService = new TimetableService(new WeekDayParser(new TimetableTimeParser(), new SubjectParser()));
        }

        public override async Task<TimetableResponse> Timetable(TimetableRequest request, ServerCallContext context)
        {
            var result = (await this.timetableService.GetTimetableAsync(new FormData()
            {
                Calendar = CalendarType.BranchOfStudy,
                Semester = SemesterType.WinterSemester,
                BranchOfStudy = "KBI4",
            })).ToGrpc();
            
            return new TimetableResponse()
            {
                Result = result, 
            };
        }
    }
}
