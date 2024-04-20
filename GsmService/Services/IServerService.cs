using System.Net;
using GsmCore.Models;

namespace GsmApi.Services;

public interface IServerService
{
    public Task<Server> CreateServer(string templateName, string name, IPAddress bindIp, uint gamePort,
        uint queryPort,
        uint slots = 32);

    public Task<bool> StartServer(Server server);

    public Task<bool> StopServer(Server server);

    public Task KillServer(Server server);

    public Task RestartServer(Server server);

    public Task UpdateServer(Server server);

    public Task DeleteServer(Server server);

    public Task UpdateNetworking(Server server, IPAddress bindIp, uint gamePort, uint queryPort, uint rconPort,
        bool restart = false);

    public Task UpdateFlags(Server server, bool autoStart, bool autoRestart, bool autoUpdate, bool doLogs,
        bool adminLog, bool netLog);

    public Task UpdateGameData(Server server, string name, uint slots, string map, string rconPassword,
        string additionalStartParams);
}