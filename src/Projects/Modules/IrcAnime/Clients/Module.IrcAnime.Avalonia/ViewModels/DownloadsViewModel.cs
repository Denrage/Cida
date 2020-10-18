﻿using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Ircanime;
using Module.IrcAnime.Avalonia.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.ViewModels
{
    public class DownloadsViewModel : ViewModelBase
    {
        private readonly IrcAnimeService.IrcAnimeServiceClient client;
        private readonly DownloadContextService downloadContextService;

        public AvaloniaList<DownloadContext> AvailableDownloads { get; } = new AvaloniaList<DownloadContext>();

        public DownloadsViewModel(IrcAnimeService.IrcAnimeServiceClient client, DownloadContextService downloadContextService)
        {
            this.client = client;
            this.downloadContextService = downloadContextService;
        }

        public async Task RefreshAvailableDownloads()
        {
            var files = (await this.client.DownloadedFilesAsync(new DownloadedFilesRequest())).Files;
            var contexts = new List<DownloadContext>();
            foreach (var item in files)
            {
                var context = await this.downloadContextService.GetContextAsync(new Models.PackMetadata()
                {
                    Bot = null,
                    Name = item.Filename,
                    Number = 0,
                    Size = (long)item.Filesize,
                });

                contexts.Add(context);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.AvailableDownloads.Clear();
                this.AvailableDownloads.AddRange(contexts);
            });

            //TODO: Reminder for future use
            //await this.downloadService.Download(this.AvailableDownloads[0].Name, default);
        }
    }
}