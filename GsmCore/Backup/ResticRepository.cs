using GsmCore.Models;
using GsmCore.Utils;
using Microsoft.Extensions.Logging;

namespace GsmCore.Backup;

public class ResticRepository : IBackupRepository
{
    private readonly ILogger _logger;
    private readonly ResticUtil _resticUtil;

    public ResticRepository(ILogger logger, ResticUtil resticUtil)
    {
        _logger = logger;
        _resticUtil = resticUtil;
    }

    public async Task Backup(Server server)
    {
        _logger.LogInformation("Backing up server {} to restic storage", server.Id);

        if (!_resticUtil.IsResticInstalled())
        {
            _logger.LogError("Restic is not installed");
            return;
        }

        var repository = "restic-repo";
        var password = "password";
        var source = server.ServerPath;
        if (!_resticUtil.Backup(repository, password, source))
        {
            _logger.LogError("Backup failed");
            return;
        }

        _logger.LogInformation("Backup complete");
    }

    public async Task Restore(Server server)
    {
        _logger.LogInformation("Restoring server {} from restic storage", server.Id);
        var repository = "restic-repo";
        var password = "password";
        var source = server.ServerPath;
        if (!_resticUtil.Restore(repository, password, source))
        {
            _logger.LogError("Restore failed");
            return;
        }

        _logger.LogInformation("Restore complete");
    }
}