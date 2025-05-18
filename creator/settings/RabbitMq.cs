namespace creator.settings;

public class RabbitMq
{
    public required string HostName { get; set; }

    public required string QueueName { get; set; }

    public required int Port { get; set; }

    public required string UserName { get; set; }

    public required string Password { get; set; }
}