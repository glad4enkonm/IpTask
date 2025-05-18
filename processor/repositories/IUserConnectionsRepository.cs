using System.Net;
using processor.models;

namespace processor.repositories;

public interface IUserConnectionsRepository
{
    /// <summary>
    /// Inserts a new user connection or updates the last_seen_at if the new time is later.
    /// </summary>
    Task<bool> UpsertUserConnectionAsync(long userId, IPAddress ipAddress, DateTimeOffset lastSeenAt);

    /// <summary>
    /// Gets a specific user connection.
    /// </summary>
    Task<UserConnection?> GetUserConnectionAsync(long userId, IPAddress ipAddress);

    /// <summary>
    /// Gets all IP addresses associated with a user, ordered by last seen descending.
    /// </summary>
    Task<IEnumerable<UserConnection>> GetConnectionsByUserIdAsync(long userId, int limit = 50);

    /// <summary>
    /// Gets the N most recent IP addresses for a user.
    /// </summary>
    Task<IEnumerable<UserConnection>> GetLastNConnectionsByUserIdAsync(long userId, int N);
}