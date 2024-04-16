using GsmApi.Util;
using GsmCore.Model;
using GsmCore.Util;

namespace GsmCore.Repository;

public interface IServerRepository : IDisposable
{
    IEnumerable<Server> GetServers();
    Task<PagedList<Server>> GetServers(QueryStringParameters parameters);
    ValueTask<Server?> GetServerById(int serverId);
    internal Task InsertServer(Server server);
    internal Task DeleteServer(int serverId);
    internal void UpdateServer(Server server);
    internal Task Save();
}