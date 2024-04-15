using System.IO.Compression;
using GsmCore.Model;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace GsmCore.Backup;

public class SftpRepository : IBackupRepository
{
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public SftpRepository(ILogger logger, CancellationTokenSource cancellationTokenSource)
    {
        _logger = logger;
        _cancellationTokenSource = cancellationTokenSource;
    }

    public async Task Backup(Server server)
    {
        _logger.LogInformation("Backing up server {} to sftp storage", server.Id);

        var sftpClient = await CreateSftpSession();
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupFile = $"server{server.Id}-{timeStamp}.zip";
        using var archive = ZipFile.Open(backupFile, ZipArchiveMode.Create);
        var serverFiles = Directory.GetFiles(server.ServerPath);
        foreach (var file in serverFiles)
        {
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
        }

        await using var fileStream = File.OpenRead(backupFile);
        sftpClient.UploadFile(fileStream, backupFile, true, obj => File.Delete(backupFile));

        sftpClient.Disconnect();
        sftpClient.Dispose();

        _logger.LogInformation("Backup complete");
    }

    public async Task Restore(Server server)
    {
        _logger.LogInformation("Restoring server {} from sftp storage", server.Id);

        var sftpClient = await CreateSftpSession();
        var backupFile = sftpClient.ListDirectory("/").OrderByDescending(f => f.LastWriteTime).First().Name;
        await using var fileStream = File.OpenWrite(backupFile);
        sftpClient.DownloadFile(backupFile, fileStream);

        using var archive = ZipFile.OpenRead(backupFile);
        foreach (var entry in archive.Entries)
        {
            var path = Path.Join(server.ServerPath, entry.FullName);
            entry.ExtractToFile(path, true);
        }

        sftpClient.Disconnect();
        sftpClient.Dispose();

        _logger.LogInformation("Restore complete");
    }

    private async Task<SftpClient> CreateSftpSession(string host, string username, string password,
        string privateKeyPath = "")
    {
        SftpClient client;
        if (string.IsNullOrEmpty(privateKeyPath))
        {
            client = new SftpClient(host, username, password);
        }
        else
        {
            var privateKeySource = new PrivateKeyFile(privateKeyPath);
            client = new SftpClient(host, username, privateKeySource);
        }

        await client.ConnectAsync(_cancellationTokenSource.Token);

        return client;
    }
}