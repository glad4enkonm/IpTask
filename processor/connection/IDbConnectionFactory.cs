using Npgsql;
namespace processor.connection;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}