using GsmApi.Cron;
using GsmApi.Hubs;
using GsmApi.Service;
using GsmCore.Util;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GsmApi.Job;

public class CpuJob(IServiceProvider serviceProvider) : CronJobBase<CpuJob>
{
    public override string Description => "Sends CPU usage to clients.";
    public override string Group => CronGroupDefaults.User;
    public override string CronExpression => CronExpressionDefaults.EveryMinuteAtSecond0;

    protected override async Task InvokeAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<GsmDbContext>();

        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<GsmHub>>();

        // Get all servers from database
        var servers = await dbContext.Servers.ToListAsync(cancellationToken);
        var processIds = servers.Select(server => serverService.GetProcessForServer(server.Id)).ToArray();
        var (_, cpuUsages) = await ProcessUtils.GetCpuUsageForProcesses(processIds);

        await hubContext.Clients.Group("status-updates").SendAsync("Cpu", cpuUsages, cancellationToken);
    }
}