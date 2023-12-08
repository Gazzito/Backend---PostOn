using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
[Authorize]
public class FriendsHub : Hub
{

    private static Dictionary<string, string> userConnectionMapping = new Dictionary<string, string>();
    public async Task FriendOnline(string userId)
    {
       // Check if the user is in the mapping
        if (userConnectionMapping.TryGetValue(userId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("FriendOnline", userId);
        }
    }
public override async Task OnConnectedAsync()
{
    var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var connectionId = Context.ConnectionId;
    Console.WriteLine($"Client connected: {Context.ConnectionId}");
           Console.WriteLine($"Client connected UID: {userId}");
    // Add user to connection mapping
    if (!string.IsNullOrEmpty(userId))
    {
        userConnectionMapping[userId] = connectionId;
         Console.WriteLine($"Client connected: {Context.ConnectionId}");
           Console.WriteLine($"Client connected UID: {userId}");
    }

    await base.OnConnectedAsync();
}


    public override async Task OnDisconnectedAsync(Exception exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
         
        await base.OnDisconnectedAsync(exception);
    }
}

