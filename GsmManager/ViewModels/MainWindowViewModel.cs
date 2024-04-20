using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DynamicData;
using GsmCore.ApiClient;

namespace GsmManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
    public ApiClient ApiClient { get; set; }

    
    public ServerViewModel ServerViewModel { get; set; } = new();
   

    public MainWindowViewModel()
    {
        
    }

#pragma warning restore CA1822 // Mark members as static
}