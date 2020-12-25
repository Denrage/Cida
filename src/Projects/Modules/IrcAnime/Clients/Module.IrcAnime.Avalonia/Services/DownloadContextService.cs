using Cida.Client.Avalonia.Api;
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
        private ConcurrentDictionary<string, DownloadContext> context = new ConcurrentDictionary<string, DownloadContext>();

        public DownloadContextService(PackService packService, DownloadStatusService downloadStatusService, IModuleSettingsService moduleSettingsService)
        {
            this.packService = packService;
            this.downloadStatusService = downloadStatusService;
            this.moduleSettingsService = moduleSettingsService;
            this.downloadStatusService.OnStatusUpdate += async () => await this.DownloadStatusService_OnStatusUpdate();
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

        public DownloadContext GetContext(PackMetadata packMetadata)
        {
            if (!this.context.TryGetValue(packMetadata.Name, out var result))
            {
                result = new DownloadContext(this.packService.Get(packMetadata), this.moduleSettingsService);
                this.context.TryAdd(packMetadata.Name, result);
            }

            // This only works bc Pack is not be copied and one instance is shared, maybe not desireable
            this.packService.Update(packMetadata);

            return result;
        }
    }
}
