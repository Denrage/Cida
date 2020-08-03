using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Module.HorribleSubs.Avalonia.ViewModels;
using ReactiveUI;

namespace Module.HorribleSubs.Avalonia.Views
{
    public class HorribleSubsView : ReactiveUserControl<HorribleSubsViewModel>
    {
        public HorribleSubsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
