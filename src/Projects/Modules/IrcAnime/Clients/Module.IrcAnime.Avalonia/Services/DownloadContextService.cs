using Cida.Client.Avalonia.Api;
using Ircanime;
using Module.IrcAnime.Avalonia.Models;
using Module.IrcAnime.Avalonia.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.Services
{
    //TODO: Interface this
    public class DownloadContextService
    {
        private readonly PackService packService;
        private readonly DownloadStatusService downloadStatusService;
        private readonly IModuleSettingsService moduleSettingsService;
        private readonly DownloadService downloadService;
        private readonly IrcAnimeService.IrcAnimeServiceClient client;
        private ConcurrentDictionary<string, DownloadContext> context = new ConcurrentDictionary<string, DownloadContext>();

        public DownloadContextService(PackService packService, DownloadStatusService downloadStatusService, IModuleSettingsService moduleSettingsService, DownloadService downloadService, IrcAnimeService.IrcAnimeServiceClient client)
        {
            this.packService = packService;
            this.downloadStatusService = downloadStatusService;
            this.moduleSettingsService = moduleSettingsService;
            this.downloadService = downloadService;
            this.client = client;
            this.downloadStatusService.OnStatusUpdate += async () => await this.DownloadStatusService_OnStatusUpdate();
            this.downloadService.OnBytesDownloaded += this.DownloadService_OnBytesDownloaded;
            this.downloadService.OnDownloadFinished += this.DownloadService_OnDownloadFinished;
        }

        private void DownloadService_OnDownloadFinished(DownloadContext context)
        {
            context.SetDownloadedLocally();
        }

        private async Task DownloadStatusService_OnStatusUpdate()
        {
            var status = await this.downloadStatusService.GetStatus();
            foreach (var item in context.Values)
            {
                if (status.TryGetValue(item.Pack.Name, out var result))
                {
                    item.DownloadedBytes = !result.Downloaded ? (long)result.DownloadedBytes : (long)result.Filesize;
                    if (result.Downloaded)
                    {
                        item.SetDownloadedToCida();
                    }
                    else
                    {
                        if (!item.Status.HasFlag(PackageStatus.Downloading))
                        {
                            item.SetDownloading();
                        }
                    }
                }
            }
        }

        private void DownloadService_OnBytesDownloaded(DownloadContext context, long receivedBytes)
        {
            context.DownloadedBytes = receivedBytes;
            if (!context.Status.HasFlag(PackageStatus.Downloading))
            {
                context.SetDownloading();
            }
        }

        public DownloadContext GetContext(PackMetadata packMetadata)
        {
            if (!this.context.TryGetValue(packMetadata.Name, out var result))
            {
                result = new DownloadContext(this.packService.Get(packMetadata), this.moduleSettingsService);
                result.OnDownload += async item => await this.Context_OnDownload(item);
                result.OnDownloadLocally += async item => await this.Context_OnDownloadLocally(item);
                this.context.TryAdd(packMetadata.Name, result);
            }

            // This only works bc Pack is not be copied and one instance is
            // shared, maybe not desireable
            this.packService.Update(packMetadata);

            return result;
        }

        private async Task Context_OnDownload(DownloadContext item)
        {
            // TODO: ADD CANCEL
            await this.client.DownloadAsync(new DownloadRequest()
            {
                DownloadRequest_ = { new DownloadRequest.Types.Request()
                {
                    BotName = item.Pack.Packs.First().Key,
                    FileName = item.Pack.Name,
                    PackageNumber = (long)item.Pack.Packs.First().Value,
                } }
            }, cancellationToken: default);
        }

        private async Task Context_OnDownloadLocally(DownloadContext item)
        {
            // TODO: Add cancel
            await this.downloadService.Download(item, default);
            await item.UpdateLocallyAvailable();
        }
    }
}
