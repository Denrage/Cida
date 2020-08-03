using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cida.Client.WebApp.Api.BaseClasses
{
    public abstract class ModuleViewBase : ComponentBase
    {
        public abstract string ModuleName { get; }
    }
}
