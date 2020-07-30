using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Module.Crunchyroll.Avalonia.ViewModels;
using ReactiveUI;

namespace Module.Crunchyroll.Avalonia.Views
{
    public class CrunchyrollView : ReactiveUserControl<CrunchyrollViewModel>
    {
        public CrunchyrollView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
