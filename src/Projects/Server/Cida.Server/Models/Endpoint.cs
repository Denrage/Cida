using Infrastructure;

namespace Cida.Server.Models
{
    public class Endpoint
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }
        
        public static implicit operator SynchronizeRequest.Types.Endpoint(Endpoint endpoint)
        {
            return new SynchronizeRequest.Types.Endpoint()
            {
                Host = endpoint.Host,
                Port = endpoint.Port,
            };
        }

        public static implicit operator Endpoint(SynchronizeRequest.Types.Endpoint endpoint)
        {
            return new Endpoint()
            {
                Host = endpoint.Host,
                Port = endpoint.Port,
            };
        }
    }
}