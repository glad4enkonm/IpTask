using Dapper;
using System.Net;
using processor.models;
using processor.util;
using processor.sharding;

namespace processor.repositories;

public class IpToUserRegistryRepository(IShardingConnectionFactory connectionFactory) : IIpToUserRegistryRepository
{
    private readonly IShardingConnectionFactory _connectionFactory = connectionFactory;

    public async Task<bool> UpsertIpUserEntryAsync(IPAddress ipAddress, long userId, DateTimeOffset lastSeenAt)
    {
        string ipAddressSearchText = IpAddressFormatter.ToSearchableString(ipAddress);

        const string sql = @"
            INSERT INTO ip_to_user_registry_ip_shard (ip_address, user_id, last_seen_at, ip_address_search_text)
            VALUES (@IpAddress, @UserId, @LastSeenAt, @IpAddressSearchText)
            ON CONFLICT (ip_address, user_id) DO UPDATE
            SET last_seen_at = EXCLUDED.last_seen_at
            -- ip_address_search_text only needs to be set on insert or if ip_address could change on conflict (which it can't here)
            -- If last_seen_at is not updated, no need to update ip_address_search_text either
            WHERE EXCLUDED.last_seen_at > ip_to_user_registry_ip_shard.last_seen_at;";
            // Alternative for ON CONFLICT if you want to ensure ip_address_search_text is always correct if
            // for some reason it could be null or wrong on an existing row (though primary key prevents ip_address changing):
            // SET last_seen_at = EXCLUDED.last_seen_at,
            //     ip_address_search_text = EXCLUDED.ip_address_search_text 
            // WHERE EXCLUDED.last_seen_at > ip_to_user_registry_ip_shard.last_seen_at
            //    OR ip_to_user_registry_ip_shard.ip_address_search_text IS DISTINCT FROM EXCLUDED.ip_address_search_text;

        using var connection = _connectionFactory.GetIpShardConnection(ipAddress);
        var result = await connection.ExecuteAsync(sql, new { IpAddress = ipAddress, UserId = userId, LastSeenAt = lastSeenAt, IpAddressSearchText = ipAddressSearchText });
        return result > 0;
    }

    public async Task<IpUserRegistryEntry?> GetIpUserEntryAsync(IPAddress ipAddress, long userId)
    {
        const string sql = @"
            SELECT ip_address AS IpAddress, user_id AS UserId, last_seen_at AS LastSeenAt, ip_address_search_text AS IpAddressSearchText
            FROM ip_to_user_registry_ip_shard
            WHERE ip_address = @IpAddress AND user_id = @UserId;
        ";
        using var connection = _connectionFactory.GetIpShardConnection(ipAddress);
        return await connection.QuerySingleOrDefaultAsync<IpUserRegistryEntry>(sql, new { IpAddress = ipAddress, UserId = userId });
    }

    public async Task<IEnumerable<IpUserRegistryEntry>> GetUsersByExactIpAsync(IPAddress ipAddress, int limit = 50)
    {
        // Uses: idx_ip_to_user_registry_ip_shard_ip_last_seen_user or idx_ip_to_user_registry_ip_shard_ip_address_gist
        const string sql = @"
            SELECT ip_address AS IpAddress, user_id AS UserId, last_seen_at AS LastSeenAt, ip_address_search_text AS IpAddressSearchText
            FROM ip_to_user_registry_ip_shard
            WHERE ip_address = @IpAddress
            ORDER BY last_seen_at DESC, user_id
            LIMIT @Limit;
        ";
        using var connection = _connectionFactory.GetIpShardConnection(ipAddress);
        return await connection.QueryAsync<IpUserRegistryEntry>(sql, new { IpAddress = ipAddress, Limit = limit });
    }

    public async Task<IEnumerable<IpUserRegistryEntry>> GetUsersByIpPrefixAsync(string ipNetworkCidr, int limit = 100)
    {
        // Uses: idx_ip_to_user_registry_ip_shard_ip_address_gist
        // The ipNetworkCidr parameter should be like '192.168.1.0/24' or '2001:db8::/32'
        // PostgreSQL's INET type can be directly cast from text for CIDR.
        const string sql = @"
            SELECT ip_address AS IpAddress, user_id AS UserId, last_seen_at AS LastSeenAt, ip_address_search_text AS IpAddressSearchText
            FROM ip_to_user_registry_ip_shard
            WHERE ip_address <<= @IpNetworkCidr::inet  -- 'is contained within or equal to'
            ORDER BY ip_address, last_seen_at DESC, user_id -- Or desired order
            LIMIT @Limit;
        ";
        using var connection = _connectionFactory.GetAllIpShardConnections()[0]; // TODO: remove!
        return await connection.QueryAsync<IpUserRegistryEntry>(sql, new { IpNetworkCidr = ipNetworkCidr, Limit = limit });
    }
    
    public async Task<IEnumerable<IpUserRegistryEntry>> GetUsersByIpSearchTextPrefixAsync(string searchTextPrefix, int limit = 100)
    {
        // Uses: idx_ip_to_user_registry_ip_shard_search_text
        // searchTextPrefix should be the start of a normalized IP string
        // e.g., "192.168" or "2001:0db8:0000"
        const string sql = @"
            SELECT ip_address AS IpAddress, user_id AS UserId, last_seen_at AS LastSeenAt, ip_address_search_text AS IpAddressSearchText
            FROM ip_to_user_registry_ip_shard
            WHERE ip_address_search_text LIKE @SearchTextPrefixPattern
            ORDER BY ip_address_search_text, last_seen_at DESC, user_id -- text sort might be different from INET sort for truncated prefixes
            LIMIT @Limit;
        ";
        using var connection = _connectionFactory.GetAllIpShardConnections()[0]; // TODO: remove!
        // Ensure the pattern for LIKE is correctly formed
        return await connection.QueryAsync<IpUserRegistryEntry>(sql, new { SearchTextPrefixPattern = searchTextPrefix + "%", Limit = limit });
    }
}