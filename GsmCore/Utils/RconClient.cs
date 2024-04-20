using GsmCore.Models;
using Microsoft.Extensions.Logging;

namespace GsmCore.Utils;

public class RconClient(ILogger<RconClient> logger)
{
    public async Task SendCommand(Server server, string command)
    {
        logger.LogInformation("Sending RCON command {} to server {}", command, server.GuId);

        var client = new BytexDigital.BattlEye.Rcon.RconClient("127.0.0.1", (int)server.RconPort, server.RconPassword);
        client.Connect();
        await client.WaitUntilConnectedAsync();

        client.Send(command);

        client.Disconnect();
    }
}