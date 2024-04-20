using System.IO.Compression;
using GsmCore.Models;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace GsmCore.Backup;

public class SftpRepository : IBackupRepository
{
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Dictionary<string, object> _data;
    private readonly string _serverPath;

    public SftpRepository(ILogger logger, CancellationTokenSource cancellationTokenSource,
        Dictionary<string, object> Data)
    {
        _logger = logger;
        _cancellationTokenSource = cancellationTokenSource;
        _data = Data;
        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var path = Environment.GetFolderPath(folder);
        _serverPath = Path.Join(path, "dayzgsm", "servers");
    }

    public async Task Backup(Server server)
    {
        _logger.LogInformation("Backing up server {} to sftp storage", server.GuId);

        var sftpClient = await CreateSftpSession((string)_data["host"], GetAuthenticationMethod());
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupFile = $"server{server.GuId}-{timeStamp}.zip";
        using var archive = ZipFile.Open(backupFile, ZipArchiveMode.Create);
        var serverFiles = Directory.GetFiles(_serverPath);
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
        _logger.LogInformation("Restoring server {} from sftp storage", server.GuId);

        var sftpClient = await CreateSftpSession((string)_data["host"], GetAuthenticationMethod());
        var backupFile = sftpClient.ListDirectory("/").OrderByDescending(f => f.LastWriteTime).First().Name;
        await using var fileStream = File.OpenWrite(backupFile);
        sftpClient.DownloadFile(backupFile, fileStream);

        using var archive = ZipFile.OpenRead(backupFile);
        foreach (var entry in archive.Entries)
        {
            var path = Path.Join(_serverPath, entry.FullName);
            entry.ExtractToFile(path, true);
        }

        sftpClient.Disconnect();
        sftpClient.Dispose();

        _logger.LogInformation("Restore complete");
    }

    private async Task<SftpClient> CreateSftpSession(string host, AuthenticationMethod authenticationMethod)
    {
        SftpClient client;
        var connectionInfo = new ConnectionInfo(host, authenticationMethod.Username, authenticationMethod);
        client = new SftpClient(connectionInfo);

        await client.ConnectAsync(_cancellationTokenSource.Token);

        return client;
    }

    private AuthenticationMethod GetAuthenticationMethod()
    {
        var privateKeyPath = (string)_data["privateKeyPath"];
        if (string.IsNullOrEmpty(privateKeyPath))
        {
            return new PasswordAuthenticationMethod((string)_data["username"],
                (string)_data["password"]);
        }

        return new PrivateKeyAuthenticationMethod((string)_data["username"],
            new PrivateKeyFile(privateKeyPath));
    }
}