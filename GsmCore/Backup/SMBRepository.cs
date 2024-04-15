using System.Net;
using GsmCore.Model;
using Microsoft.Extensions.Logging;
using SMBLibrary;
using SMBLibrary.Client;

namespace GsmCore.Backup;

public class SMBRepository : IBackupRepository
{
    private readonly ILogger _logger;
    private readonly string _smbPath;
    private NetworkCredential _credentials;

    public SMBRepository(ILogger logger, string userName, string password)
    {
        _logger = logger;
        _smbPath = "\\\\smb\\backups";
        _credentials = new NetworkCredential(userName, password);
    }

    public async Task Backup(Server server)
    {
        _logger.LogInformation("Backing up server {} to smb storage", server.Id);

        _logger.LogInformation("Backup complete");
    }

    public async Task Restore(Server server)
    {
        _logger.LogInformation("Restoring server {} from smb storage", server.Id);
        _logger.LogInformation("Restore complete");
    }

    private async Task FileUpload(string UploadURL)
    {
        SMB2Client client = new SMB2Client();

        (var isConnected, var errorMessage) = await client.ConnectAsync(IPAddress.Loopback,
            SMBTransportType.DirectTCPTransport, new CancellationToken());
        if (!isConnected)
        {
            _logger.LogError(errorMessage);
            return;
        }

        var status = await client.LoginAsync(String.Empty, "username", "password", new CancellationToken());
        if (status == NTStatus.STATUS_SUCCESS)
        {
            (status, IEnumerable<string> shares) = await client.ListShares(new CancellationToken());
            await client.LogoffAsync(new CancellationToken());
        }

        client.Disconnect();
    }
}