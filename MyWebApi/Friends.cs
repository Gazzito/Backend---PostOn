using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
[Authorize]
public class FriendsHub : Hub
{

    private static Dictionary<string, string> userConnectionMapping = new Dictionary<string, string>();

    public async Task FriendBackOnline(string userId, ApplicationDbContext dbContext)
    {
        Console.WriteLine("uSER BackOnline UID: " + userId);
        var friendships = await dbContext.Friendships
   .Where(f => (f.CreatedBy == int.Parse(userId) && f.FriendId != int.Parse(userId)) || (f.FriendId == int.Parse(userId) && f.CreatedBy != int.Parse(userId)))
   .ToListAsync();


        foreach (var friendship in friendships)
        {

            // Check if the user is in the mapping

            int friendUserId;

            if (friendship.CreatedBy == int.Parse(userId))
            {
                // The friend is the one who received the friend request
                friendUserId = friendship.FriendId;
                if (userConnectionMapping.TryGetValue(friendUserId.ToString(), out var connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("FriendBackOnline", userId);
                }
                Console.WriteLine(friendUserId + "CreatedBy");
            }
            else
            {
                // The friend is the one who initiated the friend request
                friendUserId = friendship.CreatedBy;
                if (userConnectionMapping.TryGetValue(friendUserId.ToString(), out var connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("FriendBackOnline", userId);
                }
                Console.WriteLine(friendUserId + "FriendId");
            }
        }
    }


    public async Task FriendOnline(string userId)
    {
        // Check if the user is in the mapping
        if (userConnectionMapping.TryGetValue(userId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("FriendOnline", userId);
        }
    }

    public async Task FriendOffline(string userId, ApplicationDbContext dbContext)
    {
        Console.WriteLine("uSER OFFLINE? UID: " + userId);
        var friendships = await dbContext.Friendships
   .Where(f => (f.CreatedBy == int.Parse(userId) && f.FriendId != int.Parse(userId)) || (f.FriendId == int.Parse(userId) && f.CreatedBy != int.Parse(userId)))
   .ToListAsync();


        foreach (var friendship in friendships)
        {

            // Check if the user is in the mapping

            int friendUserId;

            if (friendship.CreatedBy == int.Parse(userId))
            {
                // The friend is the one who received the friend request
                friendUserId = friendship.FriendId;
                if (userConnectionMapping.TryGetValue(friendUserId.ToString(), out var connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("FriendOffline", userId);
                }
                Console.WriteLine(friendUserId + "CreatedBy");
            }
            else
            {
                // The friend is the one who initiated the friend request
                friendUserId = friendship.CreatedBy;
                if (userConnectionMapping.TryGetValue(friendUserId.ToString(), out var connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("FriendOffline", userId);
                }
                Console.WriteLine(friendUserId + "FriendId");
            }
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


    [Authorize]
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");

        await base.OnDisconnectedAsync(exception);
    }
}

