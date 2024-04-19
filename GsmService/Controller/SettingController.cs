using System.Text.Json;
using GsmCore;
using GsmCore.Model;
using GsmCore.Param;
using GsmCore.Repository;
using GsmCore.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace GsmApi.Controller;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SettingController(GsmDbContext context, ISettingRepository settingRepository) : ControllerBase
{
    [SwaggerResponse(201, "Setting created")]
    [HttpPut(Name = "CreateSetting")]
    public async Task<IActionResult> Create([FromBody] Setting settingsParams)
    {
        await context.Settings.AddAsync(settingsParams);
        await context.SaveChangesAsync();

        return Ok();
    }

    [SwaggerResponse(200, "Setting found and updated")]
    [HttpPost(Name = "UpdateSetting")]
    public async Task<IActionResult> Update([FromBody] Setting settingsParams)
    {
        context.Settings.Attach(settingsParams);
        context.Entry(settingsParams).Property(x => x.Value).IsModified = true;
        await context.SaveChangesAsync();

        return Ok();
    }

    [SwaggerResponse(200, "Setting found and deleted")]
    [SwaggerResponse(404, "Setting not found")]
    [HttpDelete("{key:required}", Name = "DeleteSetting")]
    public async Task<IActionResult> Delete(string key)
    {
        var settings = await settingRepository.GetSettingByKey(key);
        if (settings == null)
        {
            return NotFound();
        }

        context.Settings.Remove(settings);
        await context.SaveChangesAsync();

        return Ok();
    }

    [SwaggerResponse(200, "Paginated settings", typeof(PagedList<Setting>))]
    [HttpGet(Name = "GetSettings")]
    public async Task<IActionResult> Get([FromQuery] SettingParameters settingParameters)
    {
        var settings = await settingRepository.GetSettings(settingParameters);
        var metadata = new
        {
            settings.TotalCount,
            settings.PageSize,
            settings.CurrentPage,
            settings.TotalPages,
            settings.HasNext,
            settings.HasPrevious
        };
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));

        return Ok(settings);
    }

    [SwaggerResponse(200, "Setting found", typeof(Setting))]
    [SwaggerResponse(404, "Setting not found")]
    [HttpGet("{key:required}", Name = "GetSetting")]
    public async Task<IActionResult> GetOne(string key)
    {
        var settings = await settingRepository.GetSettingByKey(key);
        if (settings == null)
        {
            return NotFound();
        }

        return Ok(settings);
    }
}