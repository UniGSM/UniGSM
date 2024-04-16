using GsmCore.Backup;
using GsmCore.Model;
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

        switch (task.Type)
        {
            case CronTaskType.Backup:
                var backupRepository = scope.ServiceProvider.GetRequiredService<IBackupRepository>();
                await backupRepository.Backup(task.CronChain.Server);
                break;
            case CronTaskType.Restart:
                break;
            case CronTaskType.Stop:
                break;
            case CronTaskType.Start:
                break;
            case CronTaskType.Update:
                break;
            case CronTaskType.Rcon:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}