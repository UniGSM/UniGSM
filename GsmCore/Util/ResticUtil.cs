using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace GsmCore.Utils;

public class ResticUtil
{
    private readonly ILogger _logger;

    public ResticUtil(ILogger logger)
    {
        _logger = logger;
    }

    public bool Backup(string repository, string password, string source)
    {
        var process = new Process();
        process.StartInfo.FileName = "restic";
        process.StartInfo.Arguments = $"-r {repository} backup {source}";
        process.StartInfo.EnvironmentVariables["RESTIC_PASSWORD"] = password;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.WaitForExit();
        if (process.ExitCode == 0) return true;
        _logger.LogError("Backup failed");
        return false;
    }

    public bool Restore(string repository, string password, string target)
    {
        var process = new Process();
        process.StartInfo.FileName = "restic";
        process.StartInfo.Arguments = $"-r {repository} restore latest --target {target}";
        process.StartInfo.EnvironmentVariables["RESTIC_PASSWORD"] = password;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.WaitForExit();
        if (process.ExitCode == 0) return true;
        _logger.LogError("Restore failed");
        return false;
    }

    public bool IsResticInstalled()
    {
        var process = Process.Start("restic version");
        process.WaitForExit();
        return process.ExitCode == 0;
    }
}