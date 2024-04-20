using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace GsmCore.Model;

[PrimaryKey(nameof(GuId))]
public class Server
{
    public string GuId { get; set; } = null!;
    public string ContainerId { get; set; } = null!;
    public string Name { get; set; }
    [DefaultValue("0.0.0.0")] public string BindIp { get; set; }
    public uint GamePort { get; set; }
    public uint QueryPort { get; set; }
    public uint RconPort { get; set; }

    [DefaultValue("dayzOffline.chernarusplus")]
    public string Map { get; set; }

    [DefaultValue(32)] public uint Slots { get; set; }
    [DefaultValue(true)] public bool AutoStart { get; set; }
    [DefaultValue(true)] public bool AutoRestart { get; set; }
    [DefaultValue(true)] public bool AutoUpdate { get; set; }
    [DefaultValue(true)] public bool DoLogs { get; set; }
    [DefaultValue(true)] public bool AdminLog { get; set; }
    [DefaultValue(true)] public bool NetLog { get; set; }
    public string AdditionalStartParams { get; set; }
    [DefaultValue(223350)] public uint AppId { get; set; }
    [DefaultValue("dayzServer_x64.exe")] public string Executable { get; set; }
    public string RconPassword { get; set; }

    public ICollection<CronChain> ChronChains { get; } = new List<CronChain>();
}