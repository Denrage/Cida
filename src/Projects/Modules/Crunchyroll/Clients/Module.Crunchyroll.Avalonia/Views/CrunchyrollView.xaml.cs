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
        private TextBox searchTerm => this.FindControl<TextBox>("Search");
        private ListBox searchResults => this.FindControl<ListBox>("SearchResults");

        public CrunchyrollView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposableRegistration =>
            {

                this.OneWayBind(this.ViewModel,
                        viewModel => viewModel.SearchResults,
                        view => view.searchResults.Items)
                    .DisposeWith(disposableRegistration);
                
                this.OneWayBind(this.ViewModel,
                    viewModel => viewModel.ShowResults,
                    view => view.searchResults.IsVisible);

                this.Bind(ViewModel,
                        viewModel => viewModel.SearchTerm,
                        view => view.searchTerm.Text)
                    .DisposeWith(disposableRegistration);

                this.Bind(this.ViewModel,
                    viewModel => viewModel.IsSearchFocused,
                    view => view.searchTerm.IsFocused);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
