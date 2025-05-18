using System.Net;
using processor.models;

namespace processor.repositories;
public interface IIpToUserRegistryRepository
{
    /// <summary>
    /// Inserts a new IP-user mapping or updates last_seen_at if the new time is later.
    /// The ipAddressSearchText is generated from ipAddress.
    /// </summary>
    Task<bool> UpsertIpUserEntryAsync(IPAddress ipAddress, long userId, DateTimeOffset lastSeenAt);

    /// <summary>
    /// Gets a specific IP-user mapping.
    /// </summary>
    Task<IpUserRegistryEntry?> GetIpUserEntryAsync(IPAddress ipAddress, long userId);

    /// <summary>
    /// Finds users associated with an exact IP address, ordered by last_seen_at descending.
    /// </summary>
    Task<IEnumerable<IpUserRegistryEntry>> GetUsersByExactIpAsync(IPAddress ipAddress, int limit = 50);

    /// <summary>
    /// Finds users whose IP addresses fall within the given CIDR prefix (e.g., "192.168.1.0/24").
    /// Ordered by IP, then last_seen_at descending.
    /// </summary>
    Task<IEnumerable<IpUserRegistryEntry>> GetUsersByIpPrefixAsync(string ipNetworkCidr, int limit = 100);

    /// <summary>
    /// Finds users by a text prefix of the normalized IP address string.
    /// Ordered by IP, then last_seen_at descending.
    /// </summary>
    Task<IEnumerable<IpUserRegistryEntry>> GetUsersByIpSearchTextPrefixAsync(string searchTextPrefix, int limit = 100);
}