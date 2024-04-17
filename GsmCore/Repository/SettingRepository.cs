using GsmApi.Util;
using GsmCore.Model;
using GsmCore.Util;
using Microsoft.EntityFrameworkCore;

namespace GsmCore.Repository;

public class SettingRepository : ISettingRepository
{
    private GsmDbContext context;

    public SettingRepository(GsmDbContext context)
    {
        this.context = context;
    }

    public Task<List<Setting>> GetSettings()
    {
        return context.Settings.ToListAsync();
    }

    public async Task<PagedList<Setting>> GetSettings(QueryStringParameters parameters)
    {
        return await PagedList<Setting>.ToPagedList(context.Settings.OrderBy(s => s.Key), parameters.PageNumber,
            parameters.PageSize);
    }

    public ValueTask<Setting?> GetSettingByKey(string key)
    {
        return context.Settings.FindAsync(key);
    }

    public async Task Save()
    {
        await context.SaveChangesAsync();
    }

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                context.Dispose();
            }
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}