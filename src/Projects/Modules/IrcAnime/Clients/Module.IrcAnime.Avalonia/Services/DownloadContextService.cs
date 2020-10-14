using Module.IrcAnime.Avalonia.Models;
using Module.IrcAnime.Avalonia.ViewModels;
using System;
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
        private Dictionary<string, DownloadContext> context = new Dictionary<string, DownloadContext>();

        public DownloadContextService(PackService packService, DownloadStatusService downloadStatusService)
        {
            this.packService = packService;
            this.downloadStatusService = downloadStatusService;

            this.downloadStatusService.OnStatusUpdate += async () => await this.DownloadStatusService_OnStatusUpdate();
        }

        private async Task DownloadStatusService_OnStatusUpdate()
        {
            foreach (var item in context.Values.Where(x => !x.Downloaded))
            {
                var status = await this.downloadStatusService.GetStatus(item.Pack.Identifier);
                if (status != null)
                {
                    item.DownloadedBytes = !status.Downloaded ? (long)status.DownloadedBytes : (long)status.Filesize;
                }
            }
        }

        public async Task<DownloadContext> GetContextAsync(PackMetadata packMetadata)
        {
            if (!this.context.TryGetValue(packMetadata.Name, out var result))
            {
                result = new DownloadContext(await this.packService.GetAsync(packMetadata));
                this.context.Add(packMetadata.Name, result);
            }
            else
            {
                // This only works bc the pack instance is never being copied and everyone could change it for everyone else. Probably not desireable
                await this.packService.UpdatePack(packMetadata);
            }

            return result;
        }
    }
}
