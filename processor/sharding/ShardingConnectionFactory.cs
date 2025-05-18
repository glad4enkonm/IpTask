namespace processor.sharding;

using Microsoft.Extensions.Options;
using Npgsql;
using processor.config;
using System.Net;

public class ShardingConnectionFactory : IShardingConnectionFactory
{
    private readonly Sharding _config;
    private readonly List<ShardInfo> _shards;    
    private readonly int _numShards;

    public ShardingConnectionFactory(IOptions<Sharding> shardingConfig)
    {
        _config = shardingConfig.Value;
        _shards = _config.Shards;
        _numShards = _shards.Count;        

        if (_numShards == 0)        
            throw new InvalidOperationException("Проверьте конфигурацию части (shards) базы данных не найдены.");
    }

    private NpgsqlConnection CreateAndOpenConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or whitespace.", nameof(connectionString));
        }
        var connection = new NpgsqlConnection(connectionString);
        //await connection.OpenAsync();
        return connection;
    }

    public NpgsqlConnection GetUserShardConnection(long userId)
    {        
        
        var shardIndex = (int)(Math.Abs(userId) % _numShards); 
        var shardInfo = _shards[shardIndex];
        
        return CreateAndOpenConnection(shardInfo.GetConnectionString());
    }

    public NpgsqlConnection GetIpShardConnection(IPAddress ipAddress)
    {
        if (ipAddress == null)
            throw new ArgumentNullException(nameof(ipAddress));

        // Sharding based on hash of the IP address
        // Ensure positive hashcode for modulo
        var hashCode = ipAddress.GetHashCode();
        var shardIndex = Math.Abs(hashCode) % _numShards;
        var shardInfo = _shards[shardIndex]; // TODO:

        return CreateAndOpenConnection(shardInfo.GetConnectionString());
    }

    public List<NpgsqlConnection> GetAllIpShardConnections()
    {        

        var connections = new List<NpgsqlConnection>(_numShards);
        try
        {
            foreach (var shardInfo in _shards)
            {
                //connections.Add(await CreateAndOpenConnectionAsync(shardInfo.ConnectionString));
            }
            return connections;
        }
        catch
        {
            // If any connection fails to open, dispose already opened ones before rethrowing
            foreach (var conn in connections)
            {
                //await conn.DisposeAsync();
            }
            throw;
        }
    }
}