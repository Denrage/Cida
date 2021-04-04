using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using IrcClient.Connections;

namespace IrcClient.Clients
{
    public class IrcClient : IDisposable
    {
        private readonly Action<IrcConnection> initializer;
        private readonly ConcurrentBag<IrcConnection> connections;

        private bool isDisposed = false;

        public IrcClient(Action<IrcConnection> initializer)
        {
            this.initializer = initializer;
            this.connections = new ConcurrentBag<IrcConnection>();
        }

        public IrcConnection GetConnection(string host, int port)
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException($"{nameof(IrcClient)} already disposed");
            }

            var connection = new IrcConnection(host, port);
            this.connections.Add(connection);
            this.initializer.Invoke(connection);
            return connection;
        }

        ~IrcClient()
        {
            Dispose();
        }

        public void Dispose()
        {
            this.isDisposed = true;
            foreach (var connection in this.connections)
            {
                connection.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
