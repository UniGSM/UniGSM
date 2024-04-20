using System.IO.Compression;
using GsmCore.Model;
using Microsoft.Extensions.Logging;

namespace GsmCore.Backup;

public class LocalRepository : IBackupRepository
{
    private readonly ILogger _logger;
    private readonly string _backupPath;
    private readonly string _serverPath;

    public LocalRepository(ILogger logger)
    {
        _logger = logger;
        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var path = Environment.GetFolderPath(folder);
        _backupPath = Path.Join(path, "dayzgsm", "backups");
        _serverPath = Path.Join(path, "dayzgsm", "servers");
        Directory.CreateDirectory(_backupPath);
    }

    public async Task Backup(Server server)
    {
        _logger.LogInformation("Backing up server {} to local storage", server.GuId);
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupFile = Path.Join(_backupPath, $"server{server.GuId}-{timeStamp}.zip");
        using var archive = ZipFile.Open(backupFile, ZipArchiveMode.Create);
        var serverFiles = Directory.GetFiles(_serverPath);
        foreach (var file in serverFiles)
        {
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
        }

        _logger.LogInformation("Backup complete");
        await Task.CompletedTask;
    }

    public async Task Restore(Server server)
    {
        _logger.LogInformation("Restoring server {} from local storage", server.GuId);
        var backupFile = Directory.GetFiles(_backupPath).OrderByDescending(f => f).First();
        using var archive = ZipFile.OpenRead(backupFile);
        foreach (var entry in archive.Entries)
        {
            var path = Path.Join(_serverPath, entry.FullName);
            entry.ExtractToFile(path, true);
        }

        _logger.LogInformation("Restore complete");
    }
}