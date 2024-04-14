using GsmCore.Models;

namespace GsmCore.Backup;

public interface IBackupRepository
{
    void Backup(Server server);
    void Restore(Server server);
}