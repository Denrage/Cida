using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;

namespace Cida.Client.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase content;

        public ViewModelBase Content
        {
            get => content;
            set => this.RaiseAndSetIfChanged(ref content, value);
        }
    }
}
