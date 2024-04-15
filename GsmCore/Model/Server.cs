using System.ComponentModel;

namespace GsmCore.Models;

public class Server : BaseModel
{
    public string Name { get; set; }
    [DefaultValue("0.0.0.0")] public string BindIp { get; set; }
    public uint GamePort { get; set; }
    public uint QueryPort { get; set; }

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
    public string ServerPath { get; set; }
}