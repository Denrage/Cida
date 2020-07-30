using System;
using System.IO;
using System.Threading.Tasks;
using IrcClient.Clients;

namespace IrcClient.Downloaders
{
    public class DccDownloader : IDisposable
    {
        private readonly DccClient client;
        private readonly object streamLock;

        private Stream stream;
        private ulong downloadedBytes;

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
            streamLock = new object();

            this.client.DataReceived += DccClient_DataReceived;
        }

        public event Action DownloadFinished;

        public event Action DownloadCanceled;

        public event Action<ulong, ulong> ProgressChanged;

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

        public void StartDownload()
            => StartDownload(Path.Combine(TempFolder, Filename));

        public void StartDownload(string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException());
            downloadedBytes = 0;

            lock (streamLock)
            {
                stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            }

            client.Connect();
        }

        public void CancelDownload() => OnDownloadCanceled();

        public void Dispose()
        {
            CleanupStream();
            client.Dispose();
            stream.Dispose();
        }

        protected virtual void OnDownloadCanceled()
        {
            DownloadCanceled?.Invoke();
            OnDownloadFinished();
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

        private void DccClient_DataReceived(byte[] buffer, int index, int count)
        {
            lock (streamLock)
            {
                stream.Write(buffer, index, count);
            }

            downloadedBytes += (ulong)count;
            Task.Run(() => ProgressChanged?.Invoke(downloadedBytes, Filesize));

            // TODO Find a better way to check for finished downloads (which works on clients not sending filesize)
            if ((downloadedBytes >= Filesize || !client.IsConnected) && !client.IsDataAvailable)
            {
                Task.Run(() => OnDownloadFinished());
            }
        }

        private void OnDownloadFinished()
        {
            client.Disconnect();
            CleanupStream();
            DownloadFinished?.Invoke();
        }

        private void CleanupStream()
        {
            lock (streamLock)
            {
                stream.Close();
            }
        }
    }
}