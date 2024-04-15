using GsmCore.Models;

namespace GsmCore.Repositories;

public interface IServerRepository : IDisposable
{
    IEnumerable<Server> GetServers();
    Server GetServerById(int serverId);
    void InsertServer(Server server);
    void DeleteServer(int serverId);
    void UpdateServer(Server server);
    void Save();
}