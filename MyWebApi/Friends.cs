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

        var userDetails = await dbContext.Users
             .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));



        if (userDetails != null)
        {
            userDetails.LastSeeOn = DateTime.UtcNow; // Update the lastSeen time
            userDetails.IsOnline = true; // Set isOnline to false
            await dbContext.SaveChangesAsync(); // Save the changes to the database
        }

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


    public async Task FriendOnline(string userId, ApplicationDbContext dbContext)
    {

        var userDetails = await dbContext.Users
       .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));


        if (userDetails != null)
        {
            userDetails.LastSeeOn = DateTime.UtcNow; // Update the lastSeen time
            userDetails.IsOnline = true; // Set isOnline to false
            await dbContext.SaveChangesAsync(); // Save the changes to the database
        }

        // Check if the user is in the mapping
        if (userConnectionMapping.TryGetValue(userId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("FriendOnline", userId);
        }
    }

    public async Task FriendOffline(string userId, ApplicationDbContext dbContext)
    {

        var userDetails = await dbContext.Users
       .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));

    
        if (userDetails != null)
        {
          userDetails.LastSeeOn = DateTime.UtcNow; // Update the lastSeen time
            userDetails.IsOnline = false; // Set isOnline to false
            await dbContext.SaveChangesAsync(); // Save the changes to the database
        }
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
    var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    Console.WriteLine($"Client disconnected: {Context.ConnectionId}");

    if (!string.IsNullOrEmpty(userId))
    {
        // Assuming you have an instance of your ApplicationDbContext
        using (var dbContext = new ApplicationDbContext())
        {
            await FriendOffline(userId, dbContext);
        }

        // Remove user from connection mapping
        userConnectionMapping.Remove(userId);
    }

    await base.OnDisconnectedAsync(exception);
}
}

