using Microsoft.AspNetCore.SignalR;

namespace FraudDetection.API.Hubs;

// This is the WebSocket channel the React app will connect to
public class FraudHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}