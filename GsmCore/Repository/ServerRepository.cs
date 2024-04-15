using GsmCore.Model;
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

    public Server GetServerById(int serverId)
    {
        return context.Servers.Find(serverId);
    }

    public void InsertServer(Server server)
    {
        context.Servers.Add(server);
    }

    public void DeleteServer(int serverId)
    {
        var server = context.Servers.Find(serverId);
        context.Servers.Remove(server);
    }

    public void UpdateServer(Server server)
    {
        context.Entry(server).State = EntityState.Modified;
    }
    
    public void Save()
    {
        context.SaveChanges();
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