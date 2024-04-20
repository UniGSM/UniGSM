using Docker.DotNet;
using Docker.DotNet.Models;
using GsmCore.Config;
using GsmCore.Util;

namespace GsmApi.Docker;

public class GameContainer(IDockerClient client, string containerId)
{
    public string ContainerId => containerId;

    public async Task<bool> Start(CancellationToken cT = default)
    {
        return await client.Containers.StartContainerAsync(containerId, new ContainerStartParameters(), cT);
    }

    public async Task<bool> Stop(CancellationToken cT = default)
    {
        return await client.Containers.StopContainerAsync(containerId, new ContainerStopParameters(), cT);
    }

    public async Task Kill(CancellationToken cT = default)
    {
        await client.Containers.KillContainerAsync(containerId, new ContainerKillParameters(), cT);
    }

    public async Task Restart(CancellationToken cT = default)
    {
        await client.Containers.RestartContainerAsync(containerId, new ContainerRestartParameters(), cT);
    }

    public async Task Remove(CancellationToken cT = default)
    {
        await client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters(), cT);
    }

    public async Task MonitorStats(EventHandler<ContainerStatsResponse> eventHandler,
        CancellationToken cT = default)
    {
        var progress = new Progress<ContainerStatsResponse>();
        progress.ProgressChanged += eventHandler;
        await client.Containers.GetContainerStatsAsync(containerId, new ContainerStatsParameters(),
            progress, cT);
    }

    public async Task<ContainerInspectResponse> GetInfo(CancellationToken cT = default)
    {
        return await client.Containers.InspectContainerAsync(containerId, cT);
    }

    public async Task MonitorEvents(EventHandler<Message> eventHandler, CancellationToken cT = default)
    {
        var progress = new Progress<Message>();
        progress.ProgressChanged += eventHandler;
        await client.System.MonitorEventsAsync(new ContainerEventsParameters(), progress, cT);
    }

    public static async Task<GameContainer> Create(IDockerClient client, GameTemplate gameTemplate,
        SteamCmdClient steamClient, Guid containerGuid)
    {
        var exposedPorts = new Dictionary<string, EmptyStruct>();
        foreach (var port in gameTemplate.Ports)
        {
            exposedPorts[port.Port.ToString()] = new EmptyStruct();
        }

        var createParams = new CreateContainerParameters()
        {
            Image = gameTemplate.ContainerImage,
            Tty = true,
            ExposedPorts = exposedPorts,
            Volumes = new Dictionary<string, EmptyStruct>()
            {
                { $"{Path.Join(PathUtil.GetAppDataPath(), "containers", containerGuid.ToString())}:/game", default }
            }
        };

        if (gameTemplate.IsSteamGame)
        {
            await steamClient.EnsureSteamCmdIsInstalled();
            createParams.Volumes.Add($"{steamClient.GetSteamCmdPath()}:/steamcmd", default);
        }

        var container = await client.Containers.CreateContainerAsync(createParams);
        return new GameContainer(client, container.ID);
    }
}