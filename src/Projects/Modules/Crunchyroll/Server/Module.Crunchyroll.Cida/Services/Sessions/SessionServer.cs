using System.Text.Json;
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

        public async Task<Result<Session>> GetSession()
        {
            var response = await this.RestClient.ExecuteTaskAsync(this.GenerateRestRequest());
            return response.IsSuccessful ? JsonSerializer.Deserialize<Result<Session>>(response.Content) : null;
        }
    }
}