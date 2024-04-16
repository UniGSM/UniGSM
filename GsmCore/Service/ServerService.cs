using System.Diagnostics;
using System.Net;
using GsmCore.Model;
using GsmCore.Repository;
using GsmCore.Util;
using Microsoft.Extensions.Logging;

namespace GsmCore.Service;

public class ServerService(GsmDbContext dbContext, SteamCmdClient steamCmdClient, ILogger logger)
{
    private static Dictionary<int, Process> _processes = new();

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
        logger.LogInformation("Starting server {}", server.Id);
        if (server.AutoUpdate) await UpdateServer(server);

        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var serverPath = Path.Combine(Environment.GetFolderPath(folder), "servers", server.Id.ToString());

        var process = new Process();
        process.StartInfo.FileName = Path.Combine(serverPath, server.Executable);
        process.StartInfo.Arguments = $"-port={server.GamePort} -config=server.cfg";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        _processes[server.Id] = process;
    }

    public void StopServer(Server server)
    {
        logger.LogInformation("Stopping server {}", server.Id);
        if (!_processes.TryGetValue(server.Id, out var value)) return;
        value.Kill();
        _processes.Remove(server.Id);
    }

    public async Task RestartServer(Server server)
    {
        logger.LogInformation("Restarting server {}", server.Id);
        StopServer(server);
        await StartServer(server);
    }

    private async Task UpdateServer(Server server)
    {
        logger.LogInformation("Updating server {}", server.Id);
        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var serverPath = Path.Combine(Environment.GetFolderPath(folder), "servers", server.Id.ToString());

        await steamCmdClient.UpdateGame(serverPath, server.AppId);
    }
}