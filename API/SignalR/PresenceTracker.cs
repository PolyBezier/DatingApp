using API.Extensions;

namespace API.SignalR;

public class PresenceTracker
{
    private readonly Dictionary<string, List<string>> _onlineUsers = [];

    public Task UserConnected(string username, string connectionId)
    {
        lock (_onlineUsers)
        {
            if (_onlineUsers.TryGetValue(username, out var userConnections))
                userConnections.Add(connectionId);
            else
                _onlineUsers.Add(username, [connectionId]);
        }

        return Task.CompletedTask;
    }

    public Task UserDisconnected(string username, string connectionId)
    {
        lock (_onlineUsers)
        {
            if (!_onlineUsers.TryGetValue(username, out var userConnections))
                return Task.CompletedTask;

            userConnections.Remove(connectionId);

            if (userConnections.None())
                _onlineUsers.Remove(username);
        }

        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsers()
    {
        lock (_onlineUsers)
        {
            return Task.FromResult(_onlineUsers.Keys
                .OrderBy(x => x)
                .ToArray());
        }
    }
}
