using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using static API.Helpers.Constants.SignalRMessages;

namespace API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker _tracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var username = Context.User!.GetUsername();

        await _tracker.UserConnected(username, Context.ConnectionId);
        await Clients.Others.SendAsync(UserIsOnline, username);

        await SendOnlineUsers();

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User!.GetUsername();

        await _tracker.UserDisconnected(username, Context.ConnectionId);
        await Clients.Others.SendAsync(UserIsOffline, Context.User?.GetUsername());

        await SendOnlineUsers();

        await base.OnDisconnectedAsync(exception);
    }

    private async Task SendOnlineUsers()
    {
        var currentUsers = await _tracker.GetOnlineUsers();
        await Clients.All.SendAsync(GetOnlineUsers, currentUsers);
    }
}
