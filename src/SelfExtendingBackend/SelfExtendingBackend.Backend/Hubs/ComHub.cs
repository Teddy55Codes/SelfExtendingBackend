using Microsoft.AspNetCore.SignalR;

namespace SelfExtendingBackend.Backend.Hubs;

public class ComHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync(message);
    }
}