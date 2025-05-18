using Dapper;
using System.Net;
using processor.models;
using processor.sharding;

namespace processor.repositories;

public class UserConnectionsRepository(IShardingConnectionFactory connectionFactory) : IUserConnectionsRepository
{
    private readonly IShardingConnectionFactory _connectionFactory = connectionFactory;

    public async Task<bool> UpsertUserConnectionAsync(long userId, IPAddress ipAddress, DateTimeOffset lastSeenAt)
    {
        const string sql = @"
            INSERT INTO user_connections_user_shard (user_id, ip_address, last_seen_at)
            VALUES (@UserId, @IpAddress, @LastSeenAt)
            ON CONFLICT (user_id, ip_address) DO UPDATE
            SET last_seen_at = EXCLUDED.last_seen_at
            WHERE EXCLUDED.last_seen_at > user_connections_user_shard.last_seen_at;
        ";
        using var connection = _connectionFactory.GetUserShardConnection(userId);
        var result = await connection.ExecuteAsync(sql, new { UserId = userId, IpAddress = ipAddress, LastSeenAt = lastSeenAt });
        return result > 0; // Returns number of rows affected
    }

    public async Task<UserConnection?> GetUserConnectionAsync(long userId, IPAddress ipAddress)
    {
        const string sql = @"
            SELECT user_id AS UserId, ip_address AS IpAddress, last_seen_at AS LastSeenAt
            FROM user_connections_user_shard
            WHERE user_id = @UserId AND ip_address = @IpAddress;
        ";
        using var connection = _connectionFactory.GetUserShardConnection(userId);
        return await connection.QuerySingleOrDefaultAsync<UserConnection>(sql, new { UserId = userId, IpAddress = ipAddress });
    }

    public async Task<IEnumerable<UserConnection>> GetConnectionsByUserIdAsync(long userId, int limit = 50)
    {
        // Uses: idx_user_connections_user_shard_user_id_last_seen_at
        const string sql = @"
            SELECT user_id AS UserId, ip_address AS IpAddress, last_seen_at AS LastSeenAt
            FROM user_connections_user_shard
            WHERE user_id = @UserId
            ORDER BY last_seen_at DESC
            LIMIT @Limit;
        ";
        using var connection = _connectionFactory.GetUserShardConnection(userId);
        return await connection.QueryAsync<UserConnection>(sql, new { UserId = userId, Limit = limit });
    }

    public async Task<IEnumerable<UserConnection>> GetLastNConnectionsByUserIdAsync(long userId, int n)
    {
        return await GetConnectionsByUserIdAsync(userId, n);
    }
}