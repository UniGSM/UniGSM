using System.Net.Sockets;

namespace GsmCore.Config;

public class GamePort
{
    public ushort Port { get; set; }
    public ProtocolType Protocol { get; set; } = ProtocolType.Tcp;
}