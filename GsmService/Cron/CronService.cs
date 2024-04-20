using GsmApi.Jobs;
using Quartz;
using Quartz.Impl;

namespace GsmApi.Cron;

public class CronService
{
    private void Update()
    {
        var job = JobBuilder.Create<CronJob>()
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithCronSchedule("0 0 0/1 1/1 * ? *")
            .Build();

        var scheduler = new StdSchedulerFactory().GetScheduler().Result;
        scheduler.ScheduleJob(job, trigger);
        scheduler.Start();
    }
}