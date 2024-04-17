using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace GsmCore.Util;

public class ResticUtil
{
    private readonly ILogger _logger;

    public ResticUtil(ILogger logger)
    {
        _logger = logger;
    }

    public bool Backup(string repository, string password, string source)
    {
        return RunResticCommand(repository, password, $"backup {source}");
    }

    public bool Restore(string repository, string password, string target)
    {
        return RunResticCommand(repository, password, $"restore latest --target {target}");
    }

    public bool IsResticInstalled()
    {
        var process = Process.Start("restic version");
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    private bool RunResticCommand(string repository, string password, string command)
    {
        using var process = new Process();
        process.StartInfo.FileName = "restic";
        process.StartInfo.Arguments = $"-r {repository} {command}";
        process.StartInfo.EnvironmentVariables["RESTIC_PASSWORD"] = password;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.WaitForExit();
        if (process.ExitCode == 0) return true;
        _logger.LogError("Command failed");
        return false;
    }
}