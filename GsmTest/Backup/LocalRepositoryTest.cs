using GsmCore.Backup;
using GsmCore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GsmTest.Backup;

public class Tests
{
    private ILogger _logger;

    [SetUp]
    public void Setup()
    {
        _logger = new NullLogger<ILogger>();

        // setup test for LocalRepository.cs
    }

    [Test]
    public async Task TestBackup()
    {
        var localRepository = new LocalRepository(_logger);
        var server = new Server
        {
            Name = "TestServer",
            GuId = Guid.NewGuid().ToString(),
        };

        const Environment.SpecialFolder folder = Environment.SpecialFolder.CommonApplicationData;
        var path = Environment.GetFolderPath(folder);
        var serverPath = Path.Combine(path, "dayzgsm", "servers", server.GuId);
        Console.WriteLine(serverPath);
        Directory.CreateDirectory(serverPath);
        await File.WriteAllTextAsync(Path.Combine(serverPath, "test.txt"), "test");
        await localRepository.Backup(server);
        File.Delete(Path.Combine(serverPath, "test.txt"));

        // find all files beginning with "server{server.Id}" in the backup path
        var backupFiles = Directory.GetFiles(Path.Combine(path, "backups"), $"server{server.GuId}-*.zip");
        Assert.AreNotEqual(0, backupFiles.Length);

        // cleanup
        backupFiles.ToList().ForEach(File.Delete);
        Assert.Pass();
    }
}