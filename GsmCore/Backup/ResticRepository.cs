using GsmCore.Model;
using GsmCore.Util;
using Microsoft.Extensions.Logging;

namespace GsmCore.Backup;

public class ResticRepository : IBackupRepository
{
    private readonly ILogger _logger;
    private readonly ResticUtil _resticUtil;
    private readonly string _serverPath;

    public ResticRepository(ILogger logger, ResticUtil resticUtil)
    {
        _logger = logger;
        _resticUtil = resticUtil;
        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var path = Environment.GetFolderPath(folder);
        _serverPath = Path.Join(path, "dayzgsm", "servers");
    }

    public async Task Backup(Server server)
    {
        _logger.LogInformation("Backing up server {} to restic storage", server.GuId);

        if (!_resticUtil.IsResticInstalled())
        {
            _logger.LogError("Restic is not installed");
            return;
        }

        var repository = "restic-repo";
        var password = "password";
        if (!_resticUtil.Backup(repository, password, _serverPath))
        {
            _logger.LogError("Backup failed");
            return;
        }

        _logger.LogInformation("Backup complete");
    }

    public async Task Restore(Server server)
    {
        _logger.LogInformation("Restoring server {} from restic storage", server.GuId);
        var repository = "restic-repo";
        var password = "password";

        if (!_resticUtil.Restore(repository, password, _serverPath))
        {
            _logger.LogError("Restore failed");
            return;
        }

        _logger.LogInformation("Restore complete");
    }
}