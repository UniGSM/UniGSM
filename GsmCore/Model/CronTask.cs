using System.ComponentModel;

namespace GsmCore.Model;

public class CronTask : BaseModel
{
    public CronChain Chain { get; set; }
    public CronTaskType Type { get; set; }
    public string Payload { get; set; }
    public int OffsetMs { get; set; }
    [DefaultValue(true)] public bool IsEnabled { get; set; }
}