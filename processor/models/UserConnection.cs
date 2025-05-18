namespace processor.models;

using System.Net;

public class UserConnection
{
    public long UserId { get; set; }
    public required IPAddress IpAddress { get; set; } // Npgsql отображает INET на IPAddress
    public DateTimeOffset LastSeenAt { get; set; }
}