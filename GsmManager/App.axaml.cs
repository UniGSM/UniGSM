using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GsmManager.Events;
using GsmManager.ViewModels;
using GsmManager.Views;

namespace GsmManager;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var connectWindowViewModel = new ConnectWindowViewModel();
            desktop.MainWindow = new ConnectWindow()
            {
                DataContext = connectWindowViewModel,
            };
            connectWindowViewModel.RequestClose += (sender, args) =>
            { 
                desktop.MainWindow.Hide();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}