using System.Text.Json;
using GsmApi.Param;
using GsmCore;
using GsmCore.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GsmApi.Controller;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ServerController(ILogger logger, GsmDbContext context, ServerRepository serverRepository) : ControllerBase
{
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