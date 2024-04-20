using GsmCore.Models;
using GsmCore.Utils;

namespace GsmCore.ApiClient;

public partial class ApiClient
{
    public async Task<HttpResponseMessage> CreateSetting(Setting settingParams)
    {
        return await Put("setting", settingParams);
    }

    public async Task<HttpResponseMessage> UpdateSetting(Setting settingParams)
    {
        return await Post("setting", settingParams);
    }

    public async Task<HttpResponseMessage> DeleteSetting(string key)
    {
        return await Delete($"setting/{key}");
    }

    public async Task<PagedList<Setting>?> GetSettings(int pageNumber = 1)
    {
        return await GetPaginated<Setting>("setting", pageNumber);
    }

    public async Task<Setting?> GetSetting(string key)
    {
        return await Get<Setting>($"setting/{key}");
    }
}