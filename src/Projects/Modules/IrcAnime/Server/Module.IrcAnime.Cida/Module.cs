using Cida.Api;
using Cida.Api.Models.Filesystem;
using Google.Protobuf;
using Grpc.Core;
using Ircanime;
using Microsoft.EntityFrameworkCore;
using Module.Crunchyroll.Cida.Extensions;
using Module.IrcAnime.Cida.Services;
using NLog;

namespace Module.IrcAnime.Cida
{
    public class Module : API.IModule
    {
        // Hack: Remove this asap
        private const string DatabasePassword = "IrcAnime";

        private const string Id = "109F8F54-DE94-479A-840A-7B4EF0F284D2";
        private string connectionString;
        private IDatabaseProvider databaseProvider;
        private SearchService searchService;
        private DownloadService downloadService;
        private API.Models.Filesystem.Directory moduleDirectory;
        private API.Models.Filesystem.Directory downloadDirectory;

        public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } = Array.Empty<ServerServiceDefinition>();

        public async Task Load(API.IDatabaseConnector databaseConnector, API.IFtpClient ftpClient, API.Models.Filesystem.Directory moduleDirectory, API.IModuleLogger moduleLogger)
        {
            this.connectionString =
                await databaseConnector.GetDatabaseConnectionStringAsync(Guid.Parse(Id), DatabasePassword);

            this.databaseProvider = databaseConnector.GetDatabaseProvider();

            this.moduleDirectory = moduleDirectory;
            this.downloadDirectory = new API.Models.Filesystem.Directory("Files", moduleDirectory);

            using (var context = this.GetContext())
            {
                await context.Database.EnsureCreatedAsync();
            }
            databaseConnector.OnConnectionChanged += async () =>
            {
                using (var context = this.GetContext())
                {
                    await context.Database.EnsureCreatedAsync();
                }
            };

            this.searchService = new SearchService(moduleLogger.CreateSubLogger("Search-Service"));
            this.downloadService = new DownloadService("irc.rizon.net", 6667, this.GetContext, ftpClient, downloadDirectory, moduleLogger);

            this.GrpcServices = new[] { IrcAnimeService.BindService(new IrcAnimeImplementation(this.searchService, this.downloadService, this.GetContext, ftpClient, downloadDirectory, moduleLogger.CreateSubLogger("GRPC-Implementation"))), };

            moduleLogger.Log(NLog.LogLevel.Info, "IrcAnime loaded successfully");
        }

        private IrcAnimeDbContext GetContext() => new IrcAnimeDbContext(this.connectionString, this.databaseProvider);

        public class IrcAnimeImplementation : IrcAnimeService.IrcAnimeServiceBase
        {
            // One megabyte
            private const int ChunkSize = 1024 * 1024;

            private readonly SearchService searchService;
            private readonly DownloadService downloadService;
            private readonly Func<IrcAnimeDbContext> getContext;
            private readonly API.IFtpClient ftpClient;
            private readonly API.Models.Filesystem.Directory downloadDirectory;
            private readonly ILogger logger;

            public IrcAnimeImplementation(SearchService searchService, DownloadService downloadService, Func<IrcAnimeDbContext> getContext, API.IFtpClient ftpClient, API.Models.Filesystem.Directory downloadDirectory, ILogger logger)
            {
                this.searchService = searchService;
                this.downloadService = downloadService;
                this.getContext = getContext;
                this.ftpClient = ftpClient;
                this.downloadDirectory = downloadDirectory;
                this.logger = logger;
            }

            public override async Task<SearchResponse> Search(SearchRequest request, ServerCallContext context)
            {
                this.logger.Info($"Initiate search-request from {context.Peer}");
                return new SearchResponse()
                {
                    SearchResults = { (await this.searchService.SearchAsync(request.SearchTerm, context.CancellationToken)).Select(x => x.ToGrpc()).ToArray() }
                };
            }

            public override async Task<DownloadedFilesResponse> DownloadedFiles(DownloadedFilesRequest request, ServerCallContext context)
            {
                using (var databaseContext = this.getContext())
                {
                    var downloads = await databaseContext.Downloads.ToArrayAsync(context.CancellationToken);
                    return new DownloadedFilesResponse(new DownloadedFilesResponse()
                    {
                        Files = { downloads.Select(x => new DownloadedFilesResponse.Types.File() { Filename = x.Name, Filesize = x.Size }).ToArray() },
                    });
                }
            }

            public override async Task<DownloadResponse> Download(DownloadRequest request, ServerCallContext context)
            {
                context.CancellationToken.Register(() => this.logger.Info($"'{string.Join(",", request.DownloadRequest_.Select(x => x.FileName))}' cancelled!"));
                this.logger.Info($"Incoming downloadrequest from {context.Peer}");
                await this.downloadService.InitiateDownload(request.DownloadRequest_.Select(x => x.FromGrpc()), context.CancellationToken);
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

                using (var databaseContext = this.getContext())
                {
                    currentDownloads.AddRange((await databaseContext.Downloads.Where(x => x.DownloadStatus == Models.Database.DownloadStatus.Available).ToArrayAsync(context.CancellationToken)).Select(x => new DownloadStatusResponse.Types.DownloadStatus()
                    {
                        Downloaded = true,
                        DownloadedBytes = 0,
                        Filename = x.Name,
                        Filesize = x.Size,
                    }));
                }

                return await Task.FromResult(new DownloadStatusResponse()
                {
                    Status = { currentDownloads }
                });
            }

            public override async Task<FileTransferInformationResponse> FileTransferInformation(FileTransferInformationRequest request, ServerCallContext context)
            {
                using var databaseContext = this.getContext();
                var downloadItem = await databaseContext.Downloads.FindAsync(new object[] { request.FileName }, cancellationToken: context.CancellationToken);
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
                this.logger.Info($"Incoming download to client request from {context.Peer}");
                using var databaseContext = this.getContext();
                var databaseFile = await databaseContext.Downloads.FindAsync(new object[] { request.FileName }, cancellationToken: context.CancellationToken);
                if (databaseFile != null)
                {
                    using var file = new API.Models.Filesystem.File(System.IO.Path.GetFileName(databaseFile.FtpPath), this.downloadDirectory, null);
                    using var downloadedFile = await this.ftpClient.DownloadFileAsync(file, context.CancellationToken);

                    if (downloadedFile.Equals(API.Models.Filesystem.File.EmptyFile))
                    {
                        this.logger.Warn($"Error occured on downloading '{request.FileName}' from ftp server!");
                        return;
                    }

                    using var fileStream = await downloadedFile.GetStreamAsync(context.CancellationToken);
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
