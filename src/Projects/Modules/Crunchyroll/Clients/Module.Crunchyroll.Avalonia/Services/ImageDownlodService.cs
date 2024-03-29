using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Module.Crunchyroll.Avalonia.Services
{
    public class ImageDownlodService : IImageDownloadService, IDisposable
    {
        Dictionary<string, Stream> cache = new Dictionary<string, Stream>();

        public void DownloadImageAsync(string url, Action<IBitmap> continueWith)
        {
            Task.Run(async () =>
            {
                if (!this.cache.TryGetValue(url, out var stream))
                {
                    stream = await this.DownloadImageAsync(url);
                    this.cache.Add(url, stream);
                }
                var memoryStream = new MemoryStream();
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(memoryStream);
                stream.Seek(0, SeekOrigin.Begin);
                memoryStream.Seek(0, SeekOrigin.Begin);
                
                continueWith(new Bitmap(memoryStream));
            });
        }

        private async Task<Stream> DownloadImageAsync(string url)
        {
            var httpClient = new HttpClient();
            var memoryStream = new MemoryStream();
            using var response = await httpClient.GetStreamAsync(url);

            await response.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        public void Dispose()
        {
            foreach (var stream in this.cache.Values)
            {
                stream.Close();
            }
        }
    }


    public interface IImageDownloadService
    {
        void DownloadImageAsync(string url, Action<IBitmap> continueWith);
    }
}