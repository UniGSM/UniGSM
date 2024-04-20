using System.Net;
using System.Text.Json;
using Docker.DotNet;
using GsmApi.Docker;
using GsmApi.Hubs;
using GsmApi.Repository;
using GsmCore.Config;
using GsmCore.Model;
using GsmCore.Util;
using Microsoft.AspNetCore.SignalR;

namespace GsmApi.Service;

public class ServerService(
    GsmDbContext dbContext,
    SteamCmdClient steamCmdClient,
    IDockerClient dockerClient,
    ILogger<ServerService> logger,
    IHubContext<GsmHub> hubContext) : IServerService
{
    public async Task<Server> CreateServer(string templateName, string name, IPAddress bindIp, uint gamePort,
        uint queryPort,
        uint slots = 32)
    {
        var guid = Guid.NewGuid();

        var (templateExists, template) = await TryGetTemplate(templateName);
        if (!templateExists || template == null) throw new ArgumentException("Template not found");

        var container = await GameContainer.Create(dockerClient, template, steamCmdClient, guid);
        var server = new Server
        {
            GuId = guid.ToString(),
            Name = name,
            BindIp = bindIp.ToString(),
            GamePort = gamePort,
            QueryPort = queryPort,
            Slots = slots,
            ContainerId = container.ContainerId,
        };
        using var serverRepository = new ServerRepository(dbContext);
        await serverRepository.InsertServer(server);
        await serverRepository.Save();
        return server;
    }

    public async Task<bool> StartServer(Server server)
    {
        var container = new GameContainer(dockerClient, server.ContainerId);
        return await container.Start();
    }

    public async Task<bool> StopServer(Server server)
    {
        var container = new GameContainer(dockerClient, server.ContainerId);
        return await container.Stop();
    }

    public async Task RestartServer(Server server)
    {
        var container = new GameContainer(dockerClient, server.ContainerId);
        await container.Restart();
    }

    public async Task KillServer(Server server)
    {
        var container = new GameContainer(dockerClient, server.ContainerId);
        await container.Kill();
    }

    public async Task UpdateServer(Server server)
    {
        await NotifyStatusUpdate(server, ServerEventType.Updating);
        logger.LogInformation("Updating server {}", server.GuId);
        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var serverPath = Path.Combine(Environment.GetFolderPath(folder), "dayzgsm", "servers", server.GuId);

        await steamCmdClient.UpdateGame(serverPath, server.AppId);
        await NotifyStatusUpdate(server, ServerEventType.Updated);
    }

    public async Task DeleteServer(Server server)
    {
        logger.LogInformation("Deleting server {}", server.GuId);
        await StopServer(server);

        using var serverRepository = new ServerRepository(dbContext);
        await serverRepository.DeleteServer(server.GuId);
        await serverRepository.Save();
    }

    public async Task UpdateNetworking(Server server, IPAddress bindIp, uint gamePort, uint queryPort, uint rconPort,
        bool restart = false)
    {
        logger.LogInformation("Changing server networking to {}:{}:{}:{} for server {}", bindIp, gamePort, queryPort,
            rconPort,
            server.GuId);
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

    private async Task NotifyStatusUpdate(Server server, ServerEventType eventType)
    {
        await hubContext.Clients.Group("status-updates").SendAsync("StatusUpdate", server.GuId, eventType);
    }

    private async Task<(bool, GameTemplate?)> TryGetTemplate(string templateName)
    {
        var templateDirPath = Path.Combine(PathUtil.GetAppDataPath(), "templates");
        Directory.CreateDirectory(templateDirPath);
        var templatePath = Path.Combine(PathUtil.GetAppDataPath(), "templates", templateName);

        if (!File.Exists(templatePath))
        {
            logger.LogWarning("Template {} not found", templateName);
            return (false, null);
        }

        try
        {
            return (true, JsonSerializer.Deserialize<GameTemplate>(await File.ReadAllTextAsync(templatePath)));
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Failed to parse template {}", templateName);
            return (false, null);
        }
    }
}