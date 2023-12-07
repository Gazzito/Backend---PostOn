using Microsoft.AspNetCore.SignalR;

public class FriendsHub : Hub
{
    public async Task FriendLoggedIn(string userId)
    {
        await Clients.Others.SendAsync("FriendLoggedIn", userId);
    }
}