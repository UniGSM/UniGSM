using GsmApi.Util;
using GsmCore.Model;
using GsmCore.Util;

namespace GsmCore.Repository;

public interface ISettingRepository : IDisposable
{
    Task<List<Setting>> GetSettings();
    Task<PagedList<Setting>> GetSettings(QueryStringParameters parameters);
    ValueTask<Setting?> GetSettingByKey(string key);
    internal Task Save();
}