using Cida.Api.Database;

namespace Cida.Server.Infrastructure.Database.ProviderLoader
{
    public interface IDatabaseProvidersProvider
    {
        IReadOnlyList<IDatabaseProvider> Providers { get; }

        IDatabaseProvider? SelectedProvider { get; }
    }
}