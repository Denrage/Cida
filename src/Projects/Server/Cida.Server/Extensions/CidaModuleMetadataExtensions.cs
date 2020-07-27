using Cida.Server.Module;

namespace Cida.Server.Extensions
{
    public static class CidaModuleMetadataExtensions
    {
        public static string IdToString(this CidaModuleMetadata metadata)
            => metadata.Id.ToString("N");
    }
}