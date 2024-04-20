using GsmCore.Models;
using GsmCore.Utils;

namespace GsmApi.Repositories;

public interface IServerRepository : IDisposable
{
    IEnumerable<Server> GetServers();
    Task<PagedList<Server>> GetServers(QueryStringParameters parameters);
    ValueTask<Server?> GetServerById(string serverId);
    internal Task InsertServer(Server server);
    internal Task DeleteServer(string serverId);
    internal void UpdateServer(Server server);
    internal Task Save();
}