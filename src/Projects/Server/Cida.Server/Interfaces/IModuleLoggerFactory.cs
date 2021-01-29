using System;
using System.Collections.Generic;
using System.Text;
using Cida.Api;

namespace Cida.Server.Interfaces
{
    public interface IModuleLoggerFactory
    {
        IModuleLogger Create(string name);
    }
}
