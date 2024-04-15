using System.Net;
using GsmCore.Models;
using GsmCore.Repositories;

namespace GsmCore.Service;

public class ServerService(GsmDbContext dbContext)
{
    public Server CreateServer(string name, IPAddress bindIp, uint gamePort, uint queryPort, uint slots = 32)
    {
        var server = new Server
        {
            Name = name,
            BindIp = bindIp.ToString(),
            GamePort = gamePort,
            QueryPort = queryPort,
            Slots = slots
        };

        using var serverRepository = new ServerRepository(dbContext);
        serverRepository.InsertServer(server);
        serverRepository.Save();
        return server;
    }

    public void StartServer(Server server)
    {
        if (server.AutoUpdate) UpdateServer(server);
    }

    public void StopServer(Server server)
    {
    }

    public void RestartServer(Server server)
    {
    }

    private void UpdateServer(Server server)
    {
        
    }
}