using System.Net;
using System.Text.Json;
using GsmApi.DTO;
using GsmCore.Model;
using GsmCore.Param;
using GsmCore.Repository;
using GsmCore.Service;
using GsmCore.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GsmApi.Controller;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ServerController(
    ILogger logger,
    IServerRepository serverRepository,
    IServerService serverService) : ControllerBase
{
    [SwaggerResponse(StatusCodes.Status201Created, "Server created", typeof(Server))]
    [HttpPut(Name = "CreateServer")]
    public async Task<IActionResult> Create([FromBody] ServerBodyParams serverParams)
    {
        var server = await serverService.CreateServer(serverParams.Name, IPAddress.Parse(serverParams.BindIp),
            serverParams.GamePort,
            serverParams.QueryPort, serverParams.Slots);

        return Created("", server);
    }

    [SwaggerResponse(200, "Server deleted")]
    [HttpDelete("{serverId:int}", Name = "DeleteServer")]
    public async Task<IActionResult> Delete(int serverId)
    {
        var server = await serverRepository.GetServerById(serverId);
        if (server == null)
        {
            logger.LogWarning("Server with id {} not found", serverId);
            return NotFound();
        }

        await serverService.DeleteServer(server);
        return NoContent();
    }

    [SwaggerResponse(200, "Server started")]
    [HttpPost("{serverId:int}/start", Name = "StartServer")]
    public async Task<IActionResult> Start(int serverId)
    {
        var server = await serverRepository.GetServerById(serverId);
        if (server == null)
        {
            logger.LogWarning("Server with id {} not found", serverId);
            return NotFound();
        }

        await serverService.StartServer(server);
        return NoContent();
    }

    [SwaggerResponse(200, "Server stopped")]
    [HttpPost("{serverId:int}/stop", Name = "StopServer")]
    public async Task<IActionResult> Stop(int serverId)
    {
        var server = await serverRepository.GetServerById(serverId);
        if (server == null)
        {
            logger.LogWarning("Server with id {} not found", serverId);
            return NotFound();
        }

        serverService.StopServer(server);
        return NoContent();
    }

    [SwaggerResponse(200, "Server restarted")]
    [HttpPost("{serverId:int}/restart", Name = "RestartServer")]
    public async Task<IActionResult> Restart(int serverId)
    {
        var server = await serverRepository.GetServerById(serverId);
        if (server == null)
        {
            logger.LogWarning("Server with id {} not found", serverId);
            return NotFound();
        }

        await serverService.RestartServer(server);
        return NoContent();
    }

    [SwaggerResponse(200, "Server updated")]
    [HttpPost("{serverId:int}/update", Name = "UpdateServer")]
    public async Task<IActionResult> Update(int serverId)
    {
        var server = await serverRepository.GetServerById(serverId);
        if (server == null)
        {
            logger.LogWarning("Server with id {} not found", serverId);
            return NotFound();
        }

        await serverService.UpdateServer(server);
        return NoContent();
    }

    [SwaggerResponse(200, "Single server", typeof(Server))]
    [HttpGet("{serverId:int}", Name = "GetServer")]
    public async Task<IActionResult> GetOne(int serverId)
    {
        var server = await serverRepository.GetServerById(serverId);

        if (server != null) return Ok(server);
        logger.LogWarning("Server with id {} not found", serverId);
        return NotFound();
    }

    [SwaggerResponse(200, "Paginated servers", typeof(PagedList<Server>))]
    [HttpGet(Name = "GetServers")]
    public async Task<IActionResult> Get([FromQuery] ServerParameters serverParams)
    {
        var servers = await serverRepository.GetServers(serverParams);
        var metadata = new
        {
            servers.TotalCount,
            servers.PageSize,
            servers.CurrentPage,
            servers.TotalPages,
            servers.HasNext,
            servers.HasPrevious
        };
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));
        logger.LogInformation("Returned {} servers from database", servers.TotalCount);

        return Ok(servers);
    }

    [SwaggerResponse(200, "Updated server networking")]
    [SwaggerResponse(404, "Server not found")]
    [HttpPut("{serverId:int:required}/networking", Name = "UpdateNetworking")]
    public async Task<IActionResult> UpdateNetworking([FromBody] ServerNetworkingParams networkingParams, int serverId)
    {
        var server = await serverRepository.GetServerById(serverId);
        if (server == null)
        {
            logger.LogWarning("Server with id {} not found", serverId);
            return NotFound();
        }

        await serverService.UpdateNetworking(server, IPAddress.Parse(networkingParams.BindIp),
            networkingParams.GamePort, networkingParams.QueryPort, networkingParams.RconPort, networkingParams.Restart);
        return NoContent();
    }

    [SwaggerResponse(200, "Updated server flags")]
    [SwaggerResponse(404, "Server not found")]
    [HttpPut("{serverId:int:required}/flags", Name = "UpdateFlags")]
    public async Task<IActionResult> UpdateFlags([FromBody] ServerFlagParams flagParams, int serverId)
    {
        var server = await serverRepository.GetServerById(serverId);
        if (server == null)
        {
            logger.LogWarning("Server with id {} not found", serverId);
            return NotFound();
        }

        await serverService.UpdateFlags(server, flagParams.AutoStart, flagParams.AutoRestart, flagParams.AutoUpdate,
            flagParams.DoLogs, flagParams.AdminLog, flagParams.NetLog);
        return NoContent();
    }

    [SwaggerResponse(200, "Updated server game data")]
    [SwaggerResponse(404, "Server not found")]
    [HttpPut("{serverId:int:required}/gamedata", Name = "UpdateGameData")]
    public async Task<IActionResult> UpdateGameData([FromBody] ServerGameDataParams gameDataParams, int serverId)
    {
        var server = await serverRepository.GetServerById(serverId);
        if (server == null)
        {
            logger.LogWarning("Server with id {} not found", serverId);
            return NotFound();
        }

        await serverService.UpdateGameData(server, gameDataParams.Name, gameDataParams.Slots, gameDataParams.Map,
            gameDataParams.RconPassword, gameDataParams.AdditionalStartParams);
        return NoContent();
    }
}