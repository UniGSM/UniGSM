using System.ComponentModel;

namespace GsmCore.Models;

public class CronChain : BaseModel
{
    public int ServerId { get; set; }
    public Server Server { get; set; } = null!;
    public string Name { get; set; }
    public string CronExpression { get; set; }
    [DefaultValue(true)] public bool IsEnabled { get; set; }

    public ICollection<CronTask> CronTasks { get; } = new List<CronTask>();
}