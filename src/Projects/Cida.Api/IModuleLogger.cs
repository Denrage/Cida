using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Api
{
    public interface IModuleLogger : NLog.ILogger
    {
        NLog.ILogger CreateSubLogger(string name);
    }
}
