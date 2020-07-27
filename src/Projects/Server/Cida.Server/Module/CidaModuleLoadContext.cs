using System.Runtime.Loader;

namespace Cida.Server.Module
{
    public class CidaModuleLoadContext : AssemblyLoadContext
    {
        public CidaModuleLoadContext()
            : base(true)
        {
        }
    }
}