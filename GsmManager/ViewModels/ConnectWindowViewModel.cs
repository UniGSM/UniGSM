using System;
using System.Threading.Tasks;
using GsmCore.ApiClient;
using GsmManager.Events;
using GsmManager.Views;
using ReactiveUI;

namespace GsmManager.ViewModels;

public class ConnectWindowViewModel : ReactiveObject
{
    public string Address { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    
    private bool _isConnecting = false;
    public bool IsConnecting
    {
        get => _isConnecting;
        set => this.RaiseAndSetIfChanged(ref _isConnecting, value);
    }
    
    public EventHandler RequestClose;
    
    private bool _connectionError = false;
    public bool ConnectionError
    {
        get => _connectionError;
        set => this.RaiseAndSetIfChanged(ref _connectionError, value);
    }
    
    
    public async Task Connect()
    {
        IsConnecting = true;
        ConnectionError = false;
        Console.WriteLine($"Connecting to {Address} with {Username}:{Password}");
        var apiClient = new ApiClient(Address, Username, Password);
        try
        {
            await apiClient.GetSettings();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ConnectionError = true;
        }

        var mainWindowViewModel = new MainWindowViewModel
        {
            ApiClient = apiClient
        };

        var window = new MainWindow()
        {
            DataContext = mainWindowViewModel
        };

        try
        {
            RequestClose?.Invoke(this, new RequestCloseReplaceMainWindowEventArgs() { NewMainWindow = window });
            window.Show();
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        IsConnecting = false;
    }
}