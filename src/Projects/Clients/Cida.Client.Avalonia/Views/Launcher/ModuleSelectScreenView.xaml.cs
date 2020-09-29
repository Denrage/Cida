using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Cida.Client.Avalonia.Api;
using System.Threading.Tasks;

namespace Cida.Client.Avalonia.Views.Launcher
{
    public class ModuleSelectScreenView : UserControl
    {

        public ModuleSelectScreenView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
