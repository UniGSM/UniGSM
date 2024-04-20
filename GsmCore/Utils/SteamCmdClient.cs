using System.Diagnostics;
using System.IO.Compression;
using GsmCore.Extensions;
using Microsoft.Extensions.Logging;

namespace GsmCore.Utils;

public class SteamCmdClient(ILogger<SteamCmdClient> logger)
{
    public async Task UpdateGame(string installDir, uint appId, string login = "anonymous")
    {
        if (!await IsSteamCmdInstalled()) await InstallSteamCmd();

        logger.LogInformation("Updating game {} in {}", appId, installDir);
        var process = new Process();
        process.StartInfo.FileName = "steamcmd";
        process.StartInfo.Arguments =
            $"+login {login} +force_install_dir {installDir} +app_update {appId} validate +quit";
        process.StartInfo.UseShellExecute = false;
        process.Start();
        await process.WaitForExitAsync();
        logger.LogInformation("Update complete");
    }

    public string GetSteamCmdPath()
    {
        return Path.Combine(PathUtil.GetAppDataPath(), "steamcmd");
    }

    public async Task EnsureSteamCmdIsInstalled()
    {
        if (!await IsSteamCmdInstalled()) await InstallSteamCmd();
    }

    private async Task<bool> IsSteamCmdInstalled()
    {
        var process = new Process();
        process.StartInfo.FileName = "steamcmd";
        process.StartInfo.Arguments = "+quit";
        process.StartInfo.UseShellExecute = false;
        process.Start();
        await process.WaitForExitAsync();
        return process.ExitCode == 0;
    }

    private async Task InstallSteamCmd()
    {
        const string downloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";

        logger.LogInformation("Installing steamcmd");

        var tempPath = Path.GetTempPath();
        var tempFile = Path.Combine(tempPath, "steamcmd.zip");
        using var client = new HttpClient();
        await client.DownloadFileTaskAsync(new Uri(downloadUrl), tempFile);

        var steamCmdPath = Path.Combine(PathUtil.GetAppDataPath(), "steamcmd");
        ZipFile.ExtractToDirectory(tempFile, steamCmdPath);
        File.Delete(tempFile);

        logger.LogInformation("Installation of steamcmd complete");
    }
}