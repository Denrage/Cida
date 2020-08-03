using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cida.Api;
using Cida.Api.Models.Filesystem;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using Horriblesubs;
using Microsoft.EntityFrameworkCore;
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
        private HorribleSubsDbContext context;
        private Directory moduleDirectory;
        private Directory downloadDirectory;

        public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } = Array.Empty<ServerServiceDefinition>();

        public async Task Load(IDatabaseConnector databaseConnector, IFtpClient ftpClient, Directory moduleDirectory)
        {
            this.connectionString =
                await databaseConnector.GetDatabaseConnectionStringAsync(Guid.Parse(Id), DatabasePassword);

            this.moduleDirectory = moduleDirectory;
            this.downloadDirectory = new Directory("Files", moduleDirectory);

            this.context = new HorribleSubsDbContext(connectionString);
            await this.context.Database.EnsureCreatedAsync();

            this.searchService = new SearchService();
            this.downloadService = new DownloadService("irc.rizon.net", 6667, this.context, ftpClient, downloadDirectory);

            this.GrpcServices = new[] {HorribleSubsService.BindService(new HorribleSubsImplementation(this.searchService, this.downloadService, this.context, ftpClient, downloadDirectory)),};
            Console.WriteLine("Loaded HorribleSubs");
        }

        public class HorribleSubsImplementation : HorribleSubsService.HorribleSubsServiceBase
        {
            // One megabyte
            private const int ChunkSize = 1024 * 1024;
            private readonly SearchService searchService;
            private readonly DownloadService downloadService;
            private readonly HorribleSubsDbContext context;
            private readonly IFtpClient ftpClient;
            private readonly Directory downloadDirectory;

            public HorribleSubsImplementation(SearchService searchService, DownloadService downloadService, HorribleSubsDbContext context, IFtpClient ftpClient, Directory downloadDirectory)
            {
                this.searchService = searchService;
                this.downloadService = downloadService;
                this.context = context;
                this.ftpClient = ftpClient;
                this.downloadDirectory = downloadDirectory;
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

                currentDownloads.AddRange((await this.context.Downloads.Where(x => x.DownloadStatus == Models.Database.DownloadStatus.Available).ToArrayAsync()).Select(x => new DownloadStatusResponse.Types.DownloadStatus()
                {
                    Downloaded = true,
                    DownloadedBytes = 0,
                    Filename = x.Name,
                    Filesize = x.Size,
                }));

                return await Task.FromResult(new DownloadStatusResponse()
                {
                    Status = { currentDownloads }
                });

            }

            public override async Task<FileTransferInformationResponse> FileTransferInformation(FileTransferInformationRequest request, ServerCallContext context)
            {
                var downloadItem = await this.context.Downloads.FindAsync(request.FileName);
                ulong fileSize = 0;
                var sha = string.Empty;

                if (downloadItem != null)
                {
                    fileSize = downloadItem.Size;
                    sha = downloadItem.Sha256;
                }

                return new FileTransferInformationResponse()
                {
                    ChunkSize = ChunkSize,
                    Size = fileSize,
                    Sha256 = sha,
                };
            }

            public override async Task File(FileRequest request, IServerStreamWriter<FileResponse> responseStream, ServerCallContext context)
            {
                var databaseFile = await this.context.Downloads.FindAsync(request.FileName);
                if(databaseFile != null)
                {
                    using var file = new File(System.IO.Path.GetFileName(databaseFile.FtpPath), this.downloadDirectory, null);
                    using var downloadedFile = await this.ftpClient.DownloadFileAsync(file);
                    using var fileStream = await downloadedFile.GetStreamAsync();
                    fileStream.Seek((long)request.Position, System.IO.SeekOrigin.Begin);
                    
                    while (fileStream.Position != fileStream.Length)
                    {
                        var buffer = new byte[ChunkSize];
                        var oldPosition = fileStream.Position;
                        await fileStream.ReadAsync(buffer, 0, ChunkSize);

                        await responseStream.WriteAsync(new FileResponse()
                        {
                            Chunk = ByteString.CopyFrom(buffer),
                            Length = (ulong)Math.Min(ChunkSize, fileStream.Length - oldPosition),
                            Position = (ulong)fileStream.Position,
                        });
                    }
                }
            }
        }
    }
}