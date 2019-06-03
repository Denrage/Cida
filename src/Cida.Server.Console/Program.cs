using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Cida.Server.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Cida.Server.CidaServer(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:", string.Empty));
        }
    }
}
