using System.Net;
using GsmCore.Model;

namespace GsmCore.Service;

public interface IServerService
{
    public Server CreateServer(string name, IPAddress bindIp, uint gamePort, uint queryPort, uint slots = 32);

    public Task StartServer(Server server);

    public void StopServer(Server server);

    public Task RestartServer(Server server);

    public Task UpdateServer(Server server);
}