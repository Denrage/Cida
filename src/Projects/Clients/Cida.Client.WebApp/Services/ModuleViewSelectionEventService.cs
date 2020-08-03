using Cida.Client.WebApp.Api.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cida.Client.WebApp.Services
{
    public class ModuleViewSelectionEventService
    {
        public event EventHandler<ModuleViewBase> ModuleViewChanged;

        public async Task ChangeModuleViewAsync(object sender, ModuleViewBase moduleView)
        {
            await Task.Run(() =>
            {
                this.SelectedModuleView = moduleView;
                ModuleViewChanged?.Invoke(sender, this.SelectedModuleView);
            });
        }

        public ModuleViewBase SelectedModuleView { get; private set; }
    }
}
