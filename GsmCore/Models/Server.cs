namespace GsmCore.Models;

public class Server : BaseModel
{
    public string Name { get; set; }
    public string BindIp { get; set; }
    public uint GamePort { get; set; }
    public uint QueryPort { get; set; }
    public string Map { get; set; }
    public uint Slots { get; set; }
}