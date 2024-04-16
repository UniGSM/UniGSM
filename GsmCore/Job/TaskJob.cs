using GsmCore.Backup;
using GsmCore.Model;
using GsmCore.Service;
using GsmCore.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace GsmCore.Job;

public class TaskJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public TaskJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GsmDbContext>();

        var dataMap = context.MergedJobDataMap;
        var taskId = dataMap.GetIntValue("taskId");

        var task = await dbContext.CronTasks.Include(t => t.CronChain).ThenInclude(t => t.Server)
            .FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null) return;

        var serverService = scope.ServiceProvider.GetRequiredService<ServerService>();

        switch (task.Type)
        {
            case CronTaskType.Backup:
                var backupRepository = scope.ServiceProvider.GetRequiredService<IBackupRepository>();
                await backupRepository.Backup(task.CronChain.Server);
                break;
            case CronTaskType.Restart:
                await serverService.RestartServer(task.CronChain.Server);
                break;
            case CronTaskType.Stop:
                serverService.StopServer(task.CronChain.Server);
                break;
            case CronTaskType.Start:
                await serverService.StartServer(task.CronChain.Server);
                break;
            case CronTaskType.Update:
                await serverService.UpdateServer(task.CronChain.Server);
                break;
            case CronTaskType.Rcon:
                var rconClient = scope.ServiceProvider.GetRequiredService<RconClient>();
                await rconClient.SendCommand(task.CronChain.Server, task.Payload);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}