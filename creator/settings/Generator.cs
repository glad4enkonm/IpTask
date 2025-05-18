namespace creator.settings;

public class Generation
{
    public required UserIdSettings UserId { get; set; }
    public required IpSettings Ip { get; set; }
    public long Rate { get; set; }
    public uint BatchSize { get; set; }
}

public class UserIdSettings
{
    public long Min { get; set; }
    public long Max { get; set; }
}

public class IpSettings
{
    public double IPv4Probability { get; set; }
}
