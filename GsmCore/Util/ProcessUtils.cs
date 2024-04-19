using System.Diagnostics;

namespace GsmCore.Util;

public static class ProcessUtils
{
    public static async Task<(int[] processIds, double[] cpuUsageTotals)> GetCpuUsageForProcesses(
        int[] processIds)
    {
        var startTime = DateTime.UtcNow;
        var processes = new List<Process>();
        var cpuUsages = new List<TimeSpan>();
        var cpuUsagesEnd = new List<TimeSpan>();
        var cpuUsedMs = new double[processIds.Length];
        var cpuUsageTotals = new double[processIds.Length];

        foreach (var processId in processIds)
        {
            var process = Process.GetProcessById(processId);
            processes.Add(process);
            cpuUsages.Add(process.TotalProcessorTime);
        }

        await Task.Delay(500);

        var endTime = DateTime.UtcNow;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;

        foreach (var process in processes)
        {
            var i = processes.IndexOf(process);
            cpuUsagesEnd.Add(process.TotalProcessorTime);
            cpuUsedMs[i] = (cpuUsagesEnd[i] - cpuUsages[i]).TotalMilliseconds;
            cpuUsageTotals[i] = cpuUsedMs[i] / (Environment.ProcessorCount * totalMsPassed);
        }

        return (processIds, cpuUsageTotals);
    }
}