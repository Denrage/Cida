using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Api
{
    public interface IModuleSubLoggerFactory
    {
        NLog.ILogger Create(string name);
    }
}
