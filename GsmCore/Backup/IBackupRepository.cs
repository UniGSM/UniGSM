using GsmCore.Model;

namespace GsmCore.Backup;

public interface IBackupRepository
{
    Task Backup(Server server);
    Task Restore(Server server);
}