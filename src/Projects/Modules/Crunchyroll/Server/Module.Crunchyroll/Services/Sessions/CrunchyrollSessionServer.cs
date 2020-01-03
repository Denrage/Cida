using System;
using RestSharp;

namespace Module.Crunchyroll.Services.Sessions
{
    internal class CrunchyrollSessionServer : SessionServer
    {
        private const string CrunchyrollBaseUrl = "http://api-manga.crunchyroll.com";
        private const string CrunchyrollApiCommand = "cr_start_session";

        private readonly string deviceType;
        private readonly string accessToken;

        public static (string DeviceType, string AccessToken)[] Devices =
        {
            ("com.crunchyroll.manga.android", "FLpcfZH4CbW4muO"),
            ("com.crunchyroll.iphone", "QWjz212GspMHH9h"),
            ("com.crunchyroll.windows.desktop", "LNDJgOit5yaRIWN")
        };

        public CrunchyrollSessionServer(string deviceType, string accessToken)
        {
            this.RestClient = new RestClient(CrunchyrollBaseUrl);
            this.deviceType = deviceType;
            this.accessToken = accessToken;
        }

        protected override RestClient RestClient { get; }

        protected override RestRequest GenerateRestRequest()
        {
            var request = new RestRequest(CrunchyrollApiCommand, Method.GET);
            request.AddParameter("api_ver", "1.0");
            request.AddParameter("device_type", this.deviceType);
            request.AddParameter("access_token", this.accessToken);
            request.AddParameter("device_id", this.GenerateDeviceId());
            return request;
        }

        private string GenerateDeviceId()
        {
            var id = string.Empty;
            var random = new Random();
            const string possibleCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            for (int i = 0; i < 32; i++)
            {
                id += possibleCharacters[random.Next(0, possibleCharacters.Length)];
            }

            return id;
        }
    }
}