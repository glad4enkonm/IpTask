namespace processor.sharding;

using Npgsql;
using System.Net;

public interface IShardingConnectionFactory
{    
    NpgsqlConnection GetUserShardConnection(long userId);
 
    NpgsqlConnection GetIpShardConnection(IPAddress ipAddress);
    
    List<NpgsqlConnection> GetAllIpShardConnections();
}