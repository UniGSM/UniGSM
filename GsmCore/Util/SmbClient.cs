// Based on https://github.com/MattMofDoom/Lurgle.Transfer/blob/master/Lurgle.Transfer/Classes/SmbClient.cs by MattMofDoom

using Microsoft.Extensions.Logging;
using SMBLibrary;
using SMBLibrary.Client;
using FileAttributes = SMBLibrary.FileAttributes;

namespace GsmCore.Util;

public class SmbClient
{
    private readonly ILogger _logger;
    private readonly string _server;
    private readonly string _username;
    private readonly string _password;
    private readonly uint _port;
    private readonly CancellationToken _cancellationToken;

    private SMB2Client _client;

    public SmbClient(ILogger logger, string server, string username, string password, uint port,
        CancellationToken cancellationToken)
    {
        _logger = logger;
        _server = server;
        _username = username;
        _password = password;
        _port = port;
        _cancellationToken = cancellationToken;
    }

    public async Task<bool> Connect()
    {
        _client = new SMB2Client();

        var transportType = SMBTransportType.DirectTCPTransport;
        if (_port == 135)
        {
            transportType = SMBTransportType.NetBiosOverTCP;
        }

        var (success, errorMessage) = await _client.ConnectAsync(_server, transportType, _cancellationToken);

        if (!success)
        {
            _logger.LogError("Error connecting to SMB Share: {}", errorMessage);
            return false;
        }

        CheckStatus(await _client.LoginAsync(String.Empty, _username, _password, _cancellationToken));

        return true;
    }

    public async Task Disconnect()
    {
        CheckStatus(await _client.LogoffAsync(_cancellationToken));
        _client.Disconnect();
    }

    public async Task<IEnumerable<(string, DateTime, DateTime, long)>> ListFiles(string filePath, bool listFolders)
    {
        var listFiles = new List<(string, DateTime, DateTime, long)>();
        var (status, fileStore) = await _client.TreeConnectAsync(GetShare(filePath), _cancellationToken);
        CheckStatus(status);

        (status, var folder, _) = await fileStore.CreateFile(GetFolder(filePath), AccessMask.GENERIC_READ,
            FileAttributes.Directory, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE,
            null, _cancellationToken);
        CheckStatus(status);

        (status, var fileList) = await fileStore.QueryDirectory(folder, "*",
            FileInformationClass.FileDirectoryInformation, _cancellationToken);
        CheckStatus(status);

        CheckStatus(await fileStore.CloseFileAsync(folder, _cancellationToken));

        listFiles.AddRange(from FileDirectoryInformation fileInfo in fileList
            where listFolders && !fileInfo.FileName.StartsWith('.') ||
                  fileInfo.FileAttributes != FileAttributes.Directory
            select (fileInfo.FileName, fileInfo.LastAccessTime, fileInfo.ChangeTime,
                fileInfo.EndOfFile));

        return listFiles;
    }

    public async Task<(string, DateTime, DateTime, long)> GetFile(string fileName, string remotePath,
        Stream transferFile)
    {
        var (status, fileStore) = await _client.TreeConnectAsync(GetShare(remotePath), _cancellationToken);
        CheckStatus(status);

        var remoteFile = Path.Combine(GetFolder(remotePath), fileName);
        (status, var fileHandle, _) = await fileStore.CreateFile(remoteFile,
            AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, FileAttributes.Normal, ShareAccess.Read,
            CreateDisposition.FILE_OPEN,
            CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null, _cancellationToken);
        CheckStatus(status);


        (status, var fileAttributes) = await fileStore.GetFileInformationAsync(fileHandle,
            FileInformationClass.FileStandardInformation,
            _cancellationToken);

        CheckStatus(status);

        var size = ((FileStandardInformation)fileAttributes).EndOfFile;

        (status, fileAttributes) = await fileStore.GetFileInformationAsync(fileHandle,
            FileInformationClass.FileBasicInformation, _cancellationToken);

        CheckStatus(status);

        var accessTime = ((FileBasicInformation)fileAttributes).LastAccessTime.Time;
        var modifyTime = ((FileBasicInformation)fileAttributes).ChangeTime.Time;
        DateTime accessDate;
        DateTime modifiedDate;
        if (accessTime != null && modifyTime != null)
        {
            accessDate = (DateTime)accessTime;
            modifiedDate = (DateTime)modifyTime;
        }
        else
        {
            throw new Exception("Unable to read remote file attributes after transfer");
        }

        byte[] data;
        long bytesRead = 0;

        do
        {
            (status, data) = await fileStore.ReadFileAsync(fileHandle, bytesRead,
                (int)_client.MaxReadSize, _cancellationToken);

            if (status == NTStatus.STATUS_END_OF_FILE || data.Length == 0) continue;
            bytesRead += data.Length;
            await transferFile.WriteAsync(data, 0, data.Length);
        } while (status != NTStatus.STATUS_END_OF_FILE && data.Length != 0);

        CheckStatus(await fileStore.CloseFileAsync(fileHandle, _cancellationToken));
        CheckStatus(await fileStore.DisconnectAsync());

        return (fileName, accessDate, modifiedDate, size);
    }

    public async Task<(string, DateTime, DateTime, long)> SendFile(string fileName, Stream transferFile,
        string remotePath, bool overWrite)
    {
        var (status, fileStore) = await _client.TreeConnectAsync(GetShare(remotePath), _cancellationToken);
        CheckStatus(status);

        var remoteFile = Path.Combine(GetFolder(remotePath), fileName);
        var remoteList = await ListFiles(remotePath, false);

        foreach (var file in remoteList.Where(file =>
                     file.Item1.Equals(fileName, StringComparison.OrdinalIgnoreCase) && !overWrite))
            return (fileName, file.Item2, file.Item3, file.Item4);


        (status, var fileHandle, _) = await fileStore.CreateFile(remoteFile,
            AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE, FileAttributes.Normal, ShareAccess.None,
            overWrite ? CreateDisposition.FILE_OVERWRITE_IF : CreateDisposition.FILE_CREATE,
            CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null, _cancellationToken);
        CheckStatus(status);

        long writeOffset = 0;
        var maxWrite = (int)_client.MaxWriteSize;

        do
        {
            var data = new byte[maxWrite];
            var bytesRead = transferFile.Read(data, 0, data.Length);
            if (bytesRead < maxWrite)
                Array.Resize(ref data, bytesRead);

            (status, _) = await fileStore.WriteFileAsync(fileHandle, writeOffset, data, _cancellationToken);
            CheckStatus(status);
            writeOffset += bytesRead;
        } while (transferFile.Position < transferFile.Length);

        CheckStatus(await fileStore.CloseFileAsync(fileHandle, _cancellationToken));

        (status, fileHandle, _) = await fileStore.CreateFile(remoteFile,
            AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, FileAttributes.Normal, ShareAccess.Read,
            CreateDisposition.FILE_OPEN,
            CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null, _cancellationToken);
        CheckStatus(status);

        (status, var fileAttributes) = await fileStore.GetFileInformationAsync(fileHandle,
            FileInformationClass.FileBasicInformation, _cancellationToken);
        CheckStatus(status);

        CheckStatus(await fileStore.CloseFileAsync(fileHandle, _cancellationToken));

        CheckStatus(await fileStore.DisconnectAsync());

        var accessTime = ((FileBasicInformation)fileAttributes).LastAccessTime.Time;
        var modifyTime = ((FileBasicInformation)fileAttributes).ChangeTime.Time;
        DateTime accessDate;
        DateTime modifiedDate;
        if (accessTime != null && modifyTime != null)
        {
            accessDate = (DateTime)accessTime;
            modifiedDate = (DateTime)modifyTime;
        }
        else
        {
            throw new Exception("Unable to read remote file attributes after transfer");
        }

        return (fileName, accessDate, modifiedDate, transferFile.Length);
    }

    private void CheckStatus(NTStatus status)
    {
        if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_NO_MORE_FILES &&
            status != NTStatus.STATUS_END_OF_FILE)
        {
            _logger.LogError("SMB error: {}", status);
        }
    }


    private string GetUserName(string userName)
    {
        return !string.IsNullOrEmpty(GetDomain(userName))
            ? userName.Contains("@")
                ? GetUserNameFromUpn(userName)
                : userName[..(userName.IndexOf("\\", StringComparison.Ordinal) + 1)]
            : userName;
    }

    private string GetDomain(string userName)
    {
        var pos = userName.IndexOf('\\', StringComparison.Ordinal);
        if (!pos.Equals(-1))
            return userName.Substring(0, pos);
        pos = userName.IndexOf("@", StringComparison.OrdinalIgnoreCase);
        return !pos.Equals(-1) ? userName[..(pos + 1)] : string.Empty;
    }

    private string GetUserNameFromUpn(string userName)
    {
        var pos = userName.IndexOf("@", StringComparison.OrdinalIgnoreCase);
        return !pos.Equals(-1) ? userName[..pos] : userName;
    }

    private string GetShare(string path)
    {
        var pos = -1;
        if (path.Contains('\\'))
            pos = path.IndexOf('\\', StringComparison.OrdinalIgnoreCase);
        else if (path.Contains('/'))
            pos = path.IndexOf('/', StringComparison.OrdinalIgnoreCase);
        return pos > 1 ? path[..pos] : path;
    }

    private string GetFolder(string path)
    {
        var pos = -1;
        if (path.Contains('\\'))
            pos = path.IndexOf('\\', StringComparison.OrdinalIgnoreCase);
        else if (path.Contains('/'))
            pos = path.IndexOf('/', StringComparison.OrdinalIgnoreCase);
        return pos > 1 ? path[..(pos + 1)] : "";
    }
}