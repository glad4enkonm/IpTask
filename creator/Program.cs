using contract;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// Объявляем очередь
await channel.QueueDeclareAsync(queue: "demo_queue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

// Объявляем сообщение
var message = new IpMessage
{
    UserId = 1,
    Ip = "0.0.0.0"
};

var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

// Публикуем сообщение
await channel.BasicPublishAsync(exchange: "",
    routingKey: "demo_queue",
    mandatory: true,
    basicProperties: new BasicProperties { Persistent = true },
    body: body);

Console.WriteLine($"Сообщение опубликовано: {message}");
