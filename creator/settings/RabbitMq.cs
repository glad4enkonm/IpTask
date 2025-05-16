namespace creator.settings;

public class RabbitMq
{
    public required string HostName { get; set; }

    public required string QueueName { get; set; }

    public int Port { get; set; } = 5672;

    public required string UserName { get; set; }

    public required string Password { get; set; }
}