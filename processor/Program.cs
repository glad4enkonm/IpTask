using contract;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

Console.WriteLine("Готов к приёму сообщений...");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = JsonSerializer.Deserialize<IpMessage>(Encoding.UTF8.GetString(body));
    
    Console.WriteLine($"Received message:");
    Console.WriteLine($"ID: {message?.UserId}");
    Console.WriteLine($"Name: {message?.Ip}");
    Console.WriteLine($"Timestamp (now): {DateTime.Now}");
    Console.WriteLine("----------------------------------");
};

await channel.BasicConsumeAsync(queue: "demo_queue",
                     autoAck: true,
                     consumer: consumer);

// Ожидаем чтобы потребитель продолжал получать сообщения
Console.Read();