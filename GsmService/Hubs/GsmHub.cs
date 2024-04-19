using Microsoft.AspNetCore.SignalR;

namespace GsmApi.Hubs;

public class GsmHub : Hub
{
    public async Task SubscribeStatusUpdates()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"status-updates");
    }

    public async Task UnsubscribeStatusUpdates()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"status-updates");
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}