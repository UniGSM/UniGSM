using System.Net;
using System.Text.Json;
using GsmApi.DTO;
using GsmApi.Param;
using GsmApi.Util;
using GsmCore;
using GsmCore.Model;
using GsmCore.Repository;
using GsmCore.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GsmApi.Controller;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ServerController(
    ILogger logger,
    GsmDbContext context,
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
}