using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Module.Crunchyroll.Libs.Models;
using Module.Crunchyroll.Libs.Models.Session;
using RestSharp;

namespace Module.Crunchyroll.Cida.Services.Sessions
{
    internal abstract class SessionServer
    {
        protected abstract RestClient RestClient { get; }

        protected abstract RestRequest GenerateRestRequest();

        public async Task<Result<Session>> GetSession(CancellationToken cancellationToken)
        {
            var response = await this.RestClient.ExecuteTaskAsync(this.GenerateRestRequest(), cancellationToken);
            return response.IsSuccessful ? JsonSerializer.Deserialize<Result<Session>>(response.Content) : null;
        }
    }
}