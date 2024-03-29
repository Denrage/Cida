﻿using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Module.IrcAnime.Avalonia.ViewModels;
using ReactiveUI;

namespace Module.IrcAnime.Avalonia.Views
{
    public class DownloadsView : ReactiveUserControl<DownloadsViewModel>
    {
        public DownloadsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
