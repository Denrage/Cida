﻿using Ircanime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.Services
{
    //TODO: Interface this
    public class DownloadStatusService
    {
        private const int WaitTime = 3000;
        private readonly IrcAnimeService.IrcAnimeServiceClient client;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly Thread statusThread;
        private readonly CancellationTokenSource statusThreadCancellationToken;
        private Dictionary<string, DownloadStatusResponse.Types.DownloadStatus> status;

        public event Action OnStatusUpdate;

        public DownloadStatusService(IrcAnimeService.IrcAnimeServiceClient client)
        {
            this.client = client;
            this.statusThreadCancellationToken = new CancellationTokenSource();
            this.statusThread = new Thread(new ThreadStart(UpdateStatus));
            this.statusThread.IsBackground = true;
            this.statusThread.Start();
        }

        public async Task<Dictionary<string, DownloadStatusResponse.Types.DownloadStatus>> GetStatus()
        {
            await this.semaphore.WaitAsync();
            try
            {
                return this.status.ToDictionary(x => x.Key, y => y.Value);
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        private void UpdateStatus()
        {
            while (true)
            {
                try
                {
                    // TODO: ADD CANCEL
                    this.statusThreadCancellationToken.Token.ThrowIfCancellationRequested();
                    var result = this.client.DownloadStatus(new DownloadStatusRequest(), cancellationToken: default);
                    this.semaphore.Wait(this.statusThreadCancellationToken.Token);
                    try
                    {
                        this.status = result.Status.ToDictionary(x => x.Filename, y => y);
                        Task.Run(() => this.OnStatusUpdate?.Invoke());
                    }
                    finally
                    {
                        this.semaphore.Release();
                    }

                    Thread.Sleep(WaitTime);
                }
                // Ignore Exceptions so loop won't get interrupted
                catch(Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        // Throw cancelled exception, so thread gets cancelled
                        throw;
                    }
                }
            }
        }

        ~DownloadStatusService()
        {
            this.statusThreadCancellationToken.Cancel();
        }
    }
}
