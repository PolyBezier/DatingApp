using API.Extensions;

namespace API.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> _onlineUsers = [];

    public Task<bool> UserConnected(string username, string connectionId)
    {
        bool isOnline = false;

        lock (_onlineUsers)
        {
            if (_onlineUsers.TryGetValue(username, out var userConnections))
                userConnections.Add(connectionId);
            else
            {
                _onlineUsers.Add(username, [connectionId]);
                isOnline = true;
            }
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
        bool isOffline = false;

        lock (_onlineUsers)
        {
            if (!_onlineUsers.TryGetValue(username, out var userConnections))
                return Task.FromResult(false);

            userConnections.Remove(connectionId);

            if (userConnections.None())
            {
                _onlineUsers.Remove(username);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
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

    public static Task<List<string>> GetConnectionsForUser(string username)
    {
        List<string> connectionIds;

        lock (_onlineUsers)
        {
            connectionIds = _onlineUsers.GetValueOrDefault(username) ?? [];
        }

        return Task.FromResult(connectionIds);
    }
}
