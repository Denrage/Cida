using Ircanime;
using SharpDX.Direct2D1.Effects;
using SharpDX.DirectWrite;
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
        private Dictionary<string, DownloadStatusResponse.Types.DownloadStatus> status;

        public event Action OnStatusUpdate;


        public DownloadStatusService(IrcAnimeService.IrcAnimeServiceClient client)
        {
            this.client = client;
            this.statusThread = new Thread(new ThreadStart(UpdateStatus));
            this.statusThread.IsBackground = true;
            this.statusThread.Start();
        }

        public async Task<DownloadStatusResponse.Types.DownloadStatus> GetStatus(string filename)
        {
            await this.semaphore.WaitAsync();
            try
            {
                if (this.status.TryGetValue(filename, out var value))
                {
                    return value;
                }

                return null;
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
                    var result = this.client.DownloadStatus(new DownloadStatusRequest());
                    this.semaphore.Wait();
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
                catch
                {
                }
            }
        }

        ~DownloadStatusService()
        {
            this.statusThread.Abort();
        }
    }
}
