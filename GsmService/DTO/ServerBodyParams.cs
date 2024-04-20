namespace GsmApi.DTO;

public class ServerBodyParams
{
    public string TemplateName { get; set; }
    public string Name { get; set; }
    public string BindIp { get; set; }
    public uint GamePort { get; set; }
    public uint QueryPort { get; set; }
    public uint Slots { get; set; } = 32;
}