using System.ComponentModel;

namespace GsmCore.Model;

public class CronTask : BaseModel
{
    public int CronChainId { get; set; }
    public CronChain CronChain { get; set; } = null!;
    public CronTaskType Type { get; set; }
    public string Payload { get; set; }
    public int OffsetMs { get; set; }
    [DefaultValue(true)] public bool IsEnabled { get; set; }
}