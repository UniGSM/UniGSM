using System.ComponentModel;

namespace GsmCore.Model;

public class CronChain
{
    public string Name { get; set; }
    public string CronExpression { get; set; }
    [DefaultValue(true)] public bool IsEnabled { get; set; }

    IEnumerable<CronTask> CronTasks { get; set; }
}