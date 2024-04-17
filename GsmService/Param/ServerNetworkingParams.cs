namespace GsmApi.Param;

public class ServerNetworkingParams
{
    public string BindIp { get; set; }
    public uint GamePort { get; set; }
    public uint QueryPort { get; set; }
    public uint RconPort { get; set; }
    public bool Restart { get; set; } = false;
}