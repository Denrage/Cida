using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cida.Api;
using Google.Protobuf.Collections;
using Grpc.Core;
using Horriblesubs;
using Module.Crunchyroll.Cida.Extensions;
using Module.HorribleSubs.Cida.Services;

namespace Module.HorribleSubs.Cida
{
    public class Module : IModule
    {
        // Hack: Remove this asap
        private const string DatabasePassword = "HorribleSubs";
        private const string Id = "109F8F54-DE94-479A-840A-7B4EF0F284D2";
        private string connectionString;
        private SearchService searchService;
        private DownloadService downloadService;

        public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } = Array.Empty<ServerServiceDefinition>();

        public async Task Load(IDatabaseConnector databaseConnector, IFtpClient ftpClient)
        {
            this.connectionString =
                await databaseConnector.GetDatabaseConnectionStringAsync(Guid.Parse(Id), DatabasePassword);

            this.searchService = new SearchService();
            this.downloadService = new DownloadService("irc.rizon.net", 6667, this.connectionString, ftpClient);

            this.GrpcServices = new[] {HorribleSubsService.BindService(new HorribleSubsImplementation(this.searchService, this.downloadService)),};
            Console.WriteLine("Loaded HorribleSubs");
        }

        public class HorribleSubsImplementation : HorribleSubsService.HorribleSubsServiceBase
        {
            private readonly SearchService searchService;
            private readonly DownloadService downloadService;

            public HorribleSubsImplementation(SearchService searchService, DownloadService downloadService)
            {
                this.searchService = searchService;
                this.downloadService = downloadService;
            }

            public override async Task<SearchResponse> Search(SearchRequest request, ServerCallContext context)
            {
                return new SearchResponse()
                {
                    SearchResults = { (await this.searchService.SearchAsync(request.SearchTerm)).Select(x => x.ToGrpc()).ToArray() }
                };
            }

            public override async Task<DownloadResponse> Download(DownloadRequest request, ServerCallContext context)
            {
                await this.downloadService.CreateDownloader(request.DownloadRequest_.FromGrpc());
                return new DownloadResponse();
            }

            public override async Task<DownloadStatusResponse> DownloadStatus(DownloadStatusRequest request, ServerCallContext context)
            {
                var currentDownloads = this.downloadService.CurrentDownloadStatus.Select(x => new DownloadStatusResponse.Types.DownloadStatus()
                {
                    Downloaded = false,
                    DownloadedBytes = x.Value.DownloadedBytes,
                    Filename = x.Key,
                    Filesize = x.Value.Size,
                }).ToList();

                currentDownloads.AddRange(await this.context.Downloads.Where(x => x.DownloadStatus == Models.Database.DownloadStatus.Available).Select(x => new DownloadStatusResponse.Types.DownloadStatus()
                {
                    Downloaded = true,
                    DownloadedBytes = 0,
                    Filename = x.Name,
                    Filesize = x.Size,
                }).ToArrayAsync());

                return await Task.FromResult(new DownloadStatusResponse()
                {
                    Status = { currentDownloads }
                });

            }
        }
    }
}