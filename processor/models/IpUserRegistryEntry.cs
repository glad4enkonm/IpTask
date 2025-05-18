namespace processor.models;

using System.Net;

public class IpUserRegistryEntry
{
    public required IPAddress IpAddress { get; set; }
    public long UserId { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public required string IpAddressSearchText { get; set; }
}