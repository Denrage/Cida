using RestSharp;

namespace Module.Crunchyroll.Cida.Services.Sessions
{
    internal class CrUnblockerSessionServer : SessionServer
    {
        public const string Version1BaseUrl = "https://api1.cr-unblocker.com";
        public const string Version1ApiCommand = "getsession.php";
        public const string Version2BaseUrl = "https://api2.cr-unblocker.com";
        public const string Version2ApiCommand = "start_session";
        private readonly string command;

        protected override RestClient RestClient { get; }

        public CrUnblockerSessionServer(string baseUrl, string command)
        {
            this.RestClient = new RestClient(baseUrl);
            this.command = command;
        }

        protected override RestRequest GenerateRestRequest()
        {
            var request = new RestRequest(this.command, Method.GET);
            request.AddParameter("version", "1.1");
            // Maybe user_id
            return request;
        }
    }
}