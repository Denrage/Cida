using System;
using System.IO;
using System.Threading.Tasks;
using IrcClient.Clients;

namespace IrcClient.Downloaders
{
    public class DccDownloader : IDisposable
    {
        private readonly DccClient client;

        public DccDownloader(DccClient client, string filename, string tempFolder, ulong filesize)
            : this(client, filename, tempFolder)
        {
            Filesize = filesize;
        }

        public DccDownloader(DccClient client, string filename, string tempFolder)
        {
            this.client = client;
            Filename = filename;
            TempFolder = tempFolder;
        }

        public string Filename { get; }

        public ulong Filesize { get; }

        public string TempFolder { get; }

        public static bool TryCreateFromSendMessage(string message, string tempFolder, out DccDownloader downloader)
        {
            try
            {
                string processedMessage = message.Trim();
                string filename = ParseFilename(ref processedMessage);
                string host = ParseHost(ref processedMessage);
                int port = ParsePort(ref processedMessage);

                DccClient client = new DccClient(host, port, 64 * 1024);

                if (ulong.TryParse(processedMessage, out ulong filesize))
                {
                    downloader = new DccDownloader(client, filename, tempFolder, filesize);
                }
                else
                {
                    downloader = new DccDownloader(client, filename, tempFolder);
                }

                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                downloader = null;
                return false;
            }
        }

        public async Task StartDownload()
            => await StartDownload(Path.Combine(TempFolder, Filename));

        public async Task StartDownload(string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException());

            ulong downloadedBytes = 0;
            using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

            this.client.Connect();

            await Task.Run(() =>
            {
                // TODO Find a better way to check for finished downloads (which works on clients not sending filesize)
                while ((downloadedBytes >= Filesize || !client.IsConnected) && !client.IsDataAvailable)
                {
                    (byte[] buffer, int count) = this.client.GetBuffer();
                    stream.Write(buffer, 0, count);
                    downloadedBytes += (ulong)count;
                }
            });

            stream.Close();
            this.client.Disconnect();
            
        }

        public void Dispose()
        {
            client.Dispose();
        }

        private static string ParseFilename(ref string processedMessage)
        {
            const string quote = "\"";
            const string space = " ";

            string filename;
            if (processedMessage.Contains(quote))
            {
                int indexOfQuote = processedMessage.IndexOf(quote, StringComparison.Ordinal);
                processedMessage = processedMessage.Substring(indexOfQuote + quote.Length);
                indexOfQuote = processedMessage.IndexOf(quote, StringComparison.Ordinal);
                filename = processedMessage.Remove(indexOfQuote);
                processedMessage = processedMessage.Substring(indexOfQuote + quote.Length + space.Length);
            }
            else
            {
                int indexOfSpace = processedMessage.IndexOf(space, StringComparison.Ordinal);
                filename = processedMessage.Remove(indexOfSpace);
                processedMessage = processedMessage.Substring(indexOfSpace + space.Length);
            }

            return filename;
        }

        private static string ParseHost(ref string processedMessage)
        {
            const string space = " ";
            int indexOfSpace = processedMessage.IndexOf(space, StringComparison.Ordinal);
            long bitwiseIp = long.Parse(processedMessage.Remove(indexOfSpace));
            processedMessage = processedMessage.Substring(indexOfSpace + space.Length);
            return DccClient.GetHostFromBitwiseIp(bitwiseIp);
        }

        private static int ParsePort(ref string processedMessage)
        {
            const string space = " ";
            int port;
            if (processedMessage.Contains(space))
            {
                int indexOfSpace = processedMessage.IndexOf(space, StringComparison.Ordinal);
                port = int.Parse(processedMessage.Remove(indexOfSpace));
                processedMessage = processedMessage.Substring(indexOfSpace + space.Length);
            }
            else
            {
                port = int.Parse(processedMessage);
            }

            return port;
        }
    }
}