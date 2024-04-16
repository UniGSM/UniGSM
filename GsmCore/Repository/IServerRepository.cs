using GsmApi.Util;
using GsmCore.Model;
using GsmCore.Util;

namespace GsmCore.Repository;

public interface IServerRepository : IDisposable
{
    IEnumerable<Server> GetServers();
    Task<PagedList<Server>> GetServers(QueryStringParameters parameters);
    Server GetServerById(int serverId);
    void InsertServer(Server server);
    void DeleteServer(int serverId);
    void UpdateServer(Server server);
    void Save();
}