namespace API.Entities;

public class Connection
{
    public Connection()
    {
        ConnectionId = null!;
        Username = null!;
    }

    public Connection(string connectionId, string username)
    {
        ConnectionId = connectionId;
        Username = username;
    }

    public string ConnectionId { get; set; }

    public string Username { get; set; }
}
