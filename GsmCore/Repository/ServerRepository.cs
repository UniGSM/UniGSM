﻿using GsmCore.Model;
using GsmCore.Util;
using Microsoft.EntityFrameworkCore;

namespace GsmCore.Repository;

public class ServerRepository : IServerRepository
{
    private GsmDbContext context;

    public ServerRepository(GsmDbContext context)
    {
        this.context = context;
    }

    public IEnumerable<Server> GetServers()
    {
        return context.Servers.ToList();
    }

    public async Task<PagedList<Server>> GetServers(QueryStringParameters parameters)
    {
        return await PagedList<Server>.ToPagedList(context.Servers.OrderBy(s => s.Id), parameters.PageNumber,
            parameters.PageSize);
    }

    public ValueTask<Server?> GetServerById(int serverId)
    {
        return context.Servers.FindAsync(serverId);
    }

    public async Task InsertServer(Server server)
    {
        await context.Servers.AddAsync(server);
    }

    public async Task DeleteServer(int serverId)
    {
        var server = await context.Servers.FindAsync(serverId);
        context.Servers.Remove(server);
    }

    public void UpdateServer(Server server)
    {
        context.Entry(server).State = EntityState.Modified;
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