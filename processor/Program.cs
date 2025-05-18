using contract;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using processor.config;

/*
Формат переменных среды для переопределения значений конфигурации

export ShardingConfig__Shards__0__Host="db0.mycompany.local"
export ShardingConfig__Shards__0__Database="prod_user_db_0"
export ShardingConfig__Shards__0__Username="prod_user"
export ShardingConfig__Shards__0__Password="12345"
*/

// Строим конфигурацию
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json") // здесь можно было добавить метаданные и сделать авто проверку конфигурации
    .AddEnvironmentVariables() // переопределяем значения из среды
    .Build();

// Поддержка DI
var services = new ServiceCollection();

// Регистрируем конфигурации
services.Configure<Sharding>(config.GetSection("Sharding"));


// services.AddSingleton<IMessageBatchSender, RabbitMessageSender>();


var serviceProvider = services.BuildServiceProvider();

// var sender = serviceProvider.GetRequiredService<IRateRegulatedSender>();
// sender.Start();

// Ожидаем завершения
// Console.Read();
// sender.Stop();


/*
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
*/