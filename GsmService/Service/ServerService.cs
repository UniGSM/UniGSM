using System.Diagnostics;
using System.Net;
using GsmApi.Repository;
using GsmCore.Model;
using GsmCore.Util;

namespace GsmApi.Service;

public class ServerService(GsmDbContext dbContext, SteamCmdClient steamCmdClient, ILogger<ServerService> logger) : IServerService
{
    private static Dictionary<int, Process> _processes = new();

    public async Task<Server> CreateServer(string name, IPAddress bindIp, uint gamePort, uint queryPort,
        uint slots = 32)
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
        await serverRepository.InsertServer(server);
        await serverRepository.Save();
        return server;
    }

    public async Task StartServer(Server server)
    {
        logger.LogInformation("Starting server {}", server.Id);
        if (server.AutoUpdate) await UpdateServer(server);

        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var serverPath = Path.Combine(Environment.GetFolderPath(folder), "dayzgsm", "servers", server.Id.ToString());

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

    public async Task UpdateServer(Server server)
    {
        logger.LogInformation("Updating server {}", server.Id);
        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var serverPath = Path.Combine(Environment.GetFolderPath(folder), "dayzgsm", "servers", server.Id.ToString());

        await steamCmdClient.UpdateGame(serverPath, server.AppId);
    }

    public async Task DeleteServer(Server server)
    {
        logger.LogInformation("Deleting server {}", server.Id);
        StopServer(server);

        using var serverRepository = new ServerRepository(dbContext);
        await serverRepository.DeleteServer(server.Id);
        await serverRepository.Save();
    }

    public async Task UpdateNetworking(Server server, IPAddress bindIp, uint gamePort, uint queryPort, uint rconPort,
        bool restart = false)
    {
        logger.LogInformation("Changing server networking to {}:{}:{}:{} for server {}", bindIp, gamePort, queryPort,
            rconPort,
            server.Id);
        server.BindIp = bindIp.ToString();
        server.GamePort = gamePort;
        server.QueryPort = queryPort;
        server.RconPort = rconPort;
        using var serverRepository = new ServerRepository(dbContext);
        serverRepository.UpdateServer(server);
        await serverRepository.Save();
        if (restart) await RestartServer(server);
    }

    public async Task UpdateFlags(Server server, bool autoStart, bool autoRestart, bool autoUpdate, bool doLogs,
        bool adminLog, bool netLog)
    {
        server.AutoStart = autoStart;
        server.AutoRestart = autoRestart;
        server.AutoUpdate = autoUpdate;
        server.DoLogs = doLogs;
        server.AdminLog = adminLog;
        server.NetLog = netLog;
        using var serverRepository = new ServerRepository(dbContext);
        serverRepository.UpdateServer(server);
        await serverRepository.Save();
    }

    public async Task UpdateGameData(Server server, string name, uint slots, string map, string rconPassword,
        string additionalStartParams)
    {
        server.Name = name;
        server.Slots = slots;
        server.Map = map;
        server.RconPassword = rconPassword;
        server.AdditionalStartParams = additionalStartParams;

        using var serverRepository = new ServerRepository(dbContext);
        serverRepository.UpdateServer(server);
        await serverRepository.Save();
    }
}