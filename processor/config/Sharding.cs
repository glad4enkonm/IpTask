namespace processor.config;
public class Sharding
{    
    public string BitMask { get; set; } = string.Empty;
    public int MaskShift { get; set; }
    public string IpRegex { get; set; } = string.Empty;
    public required List<ShardInfo> Shards { get; set; }
    
}

public class ShardInfo
{
    public required string Host { get; set; }
    public required string Database { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
   
        
    public string GetConnectionString()
    {
        return $"Host={Host};Database={Database};Username={Username};Password={Password}";
    }
}