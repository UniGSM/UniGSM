using GsmCore.Models;
using GsmCore.Utils;

namespace GsmApi.Repositories;

public interface ISettingRepository : IDisposable
{
    Task<List<Setting>> GetSettings();
    Task<PagedList<Setting>> GetSettings(QueryStringParameters parameters);
    ValueTask<Setting?> GetSettingByKey(string key);
    internal Task Save();
}