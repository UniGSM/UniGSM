using GsmCore.Model;
using GsmCore.Util;

namespace GsmApi.Repository;

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