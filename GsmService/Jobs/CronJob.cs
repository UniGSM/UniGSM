using Quartz;
using Quartz.Impl;

namespace GsmApi.Jobs;

public class CronJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public CronJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GsmDbContext>();

        var dataMap = context.MergedJobDataMap;
        var chainId = dataMap.GetIntValue("chainId");

        // Get chain from database
        var cronChain = await dbContext.CronChains.FindAsync(chainId);
        if (cronChain == null) return;

        foreach (var task in cronChain.CronTasks.OrderBy(t => t.OffsetMs))
        {
            var job = JobBuilder.Create<TaskJob>()
                .Build();

            var trigger = TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.AddMilliseconds(task.OffsetMs))
                .Build();

            var scheduler = new StdSchedulerFactory().GetScheduler().Result;
            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }
    }
}