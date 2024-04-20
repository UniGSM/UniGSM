using GsmCore.Models;
using GsmCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace GsmApi.Repositories;

public class SettingRepository(GsmDbContext context) : ISettingRepository
{
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