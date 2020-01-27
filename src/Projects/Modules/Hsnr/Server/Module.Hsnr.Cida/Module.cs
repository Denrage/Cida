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
using CalendarType = Hsnr.CalendarType;
using SemesterType = Module.Hsnr.Timetable.Data.SemesterType;

namespace Module.Hsnr
{
    public class Module : IModule
    {
        public async Task Load(IDatabaseConnector connector)
        {
            Console.WriteLine("Loaded");

            var timetableService =
                new TimetableService(new WeekDayParser(new TimetableTimeParser(), new SubjectParser()));

            this.GrpcServices = new[]
            {
                HsnrService.BindService(new HsnrServiceImplementation()),
                HsnrTimetableService.BindService(new HsnrTimetableServiceImplementation(timetableService)),
            };

            await Task.CompletedTask;
        }

        public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } 

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

        public HsnrTimetableServiceImplementation(TimetableService timetableService)
        {
            this.timetableService = timetableService;
        }

        public override async Task<TimetableResponse> Timetable(TimetableRequest request, ServerCallContext context)
        {
            var result = (await this.timetableService.GetTimetableAsync(new FormData()
            {
                Calendar = request.Calendar.ToModel(),
                Semester = request.Semester.ToModel(),
                BranchOfStudy = request.BranchOfStudy,
                Lecturer = request.Lecturer,
                Room = request.Room,
            })).ToGrpc();
            
            return new TimetableResponse()
            {
                Result = result, 
            };
        }
    }
}
