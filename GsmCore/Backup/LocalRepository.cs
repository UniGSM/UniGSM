using System.IO.Compression;
using GsmCore.Model;
using Microsoft.Extensions.Logging;

namespace GsmCore.Backup;

public class LocalRepository : IBackupRepository
{
    private readonly ILogger _logger;
    private readonly string _backupPath;

    public LocalRepository(ILogger logger)
    {
        _logger = logger;
        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var path = Environment.GetFolderPath(folder);
        _backupPath = Path.Join(path, "backups");
    }

    public async Task Backup(Server server)
    {
        _logger.LogInformation("Backing up server {} to local storage", server.Id);
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupFile = Path.Join(_backupPath, $"server{server.Id}-{timeStamp}.zip");
        using var archive = ZipFile.Open(backupFile, ZipArchiveMode.Create);
        var serverFiles = Directory.GetFiles(server.ServerPath);
        foreach (var file in serverFiles)
        {
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
        }

        _logger.LogInformation("Backup complete");
    }

    public async Task Restore(Server server)
    {
        _logger.LogInformation("Restoring server {} from local storage", server.Id);
        var backupFile = Directory.GetFiles(_backupPath).OrderByDescending(f => f).First();
        using var archive = ZipFile.OpenRead(backupFile);
        foreach (var entry in archive.Entries)
        {
            var path = Path.Join(server.ServerPath, entry.FullName);
            entry.ExtractToFile(path, true);
        }

        _logger.LogInformation("Restore complete");
    }
}