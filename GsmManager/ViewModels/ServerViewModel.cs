using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GsmCore.Model;

namespace GsmManager.ViewModels;

public class ServerViewModel : ViewModelBase
{
    public ObservableCollection<Server> Servers { get; }
    public Server SelectedServer { get; set; }

    public async Task DoubleTap()
    {
        Console.WriteLine(SelectedServer.Name);
    }

    public ServerViewModel()
    {
        Servers = new ObservableCollection<Server>()
        {
            new()
            {
                Name = "OnlyZ Chiemsee", Slots = 90, BindIp = "127.0.0.1"
            },
            new()
            {
                Name = "OnlyZ Banov", Slots = 90, BindIp = "127.0.0.1"
            },
            new()
            {
                Name = "OnlyZ Altis", Slots = 90, BindIp = "127.0.0.1"
            },
            new()
            {
                Name = "OnlyZ Tanoa", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Malden", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Stratis", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Livonia", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Chernarus", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Winter", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Taviana", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Panthera", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Napf", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Namalsk", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Esseker", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Lingor", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Utes", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Sahrani", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Zargabad", Slots = 90, BindIp = "localhost"
            },
            new()
            {
                Name = "OnlyZ Takistan", Slots = 90, BindIp = "localhost"
            },
        };
    }
}