using GsmCore.Model;
using Microsoft.Extensions.Logging;

namespace GsmCore.Util;

public class RconClient(ILogger logger)
{
    public async Task SendCommand(Server server, string command)
    {
        logger.LogInformation("Sending RCON command {} to server {}", command, server.Id);

        var client = new BytexDigital.BattlEye.Rcon.RconClient("127.0.0.1", (int)server.RconPort, server.RconPassword);
        client.Connect();
        await client.WaitUntilConnectedAsync();

        client.Send(command);

        client.Disconnect();
    }
}