using GsmApi.Cron;

namespace GsmApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder RunCronJobScheduling(this IApplicationBuilder builder)
    {
        CronJobSchedulingStarter.StartSchedulingAsync(builder).GetAwaiter().GetResult();

        return builder;
    }
}