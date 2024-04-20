using Quartz;

namespace GsmApi.Jobs;

public interface ICronJob : IJob
{
    string Name { get; }
    string Group { get; }
    string CronExpression { get; }
    string Description { get; }
}