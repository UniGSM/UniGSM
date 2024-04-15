using System.Net;
using GsmCore.Model;
using GsmCore.Repository;
using GsmCore.Util;

namespace GsmCore.Service;

public class ServerService(GsmDbContext dbContext, SteamCmdClient steamCmdClient)
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

    public async Task StartServer(Server server)
    {
        if (server.AutoUpdate) await UpdateServer(server);
    }

    public void StopServer(Server server)
    {
    }

    public void RestartServer(Server server)
    {
    }

    private async Task UpdateServer(Server server)
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonPrograms;
        var serverPath = Path.Combine(Environment.GetFolderPath(folder), "servers", server.Id.ToString());

        await steamCmdClient.UpdateGame(serverPath, server.AppId);
    }
}