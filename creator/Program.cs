using creator.util;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

Console.WriteLine("Введите количество пакетов в сек.");
var rate = int.Parse(Console.ReadLine()!);

var stopwatch = new Stopwatch();


async void SendMessageBatch(object? sender, System.Timers.ElapsedEventArgs e)
{
    int messagesSent = 0;
    stopwatch.Restart();

    try
    {
        for (int i = 0; i < rate; i++)
        {
            var message = Generator.GetRandomMessage();

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await channel.BasicPublishAsync(exchange: "",
                routingKey: "demo_queue",
                mandatory: true,
                basicProperties: new BasicProperties { Persistent = true },
                body: body);

            messagesSent += 1;
        }
    }
    finally
    {
        stopwatch.Stop();
        var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

        Console.WriteLine($"Отправлено {messagesSent} сообщений за {elapsedMs:F2} ms");
        if (elapsedMs > 1000)
            Console.WriteLine($"🚨 отправка пакета сообщений заняла ({elapsedMs:F2} ms)");
    }
}

var timer = new System.Timers.Timer(1000);

Console.WriteLine($"Начинаем отправку пакетов на скорости {rate} собщ./сек.");
timer.Elapsed += SendMessageBatch;
timer.Start();

// Ожидаем завершения
Console.Read();
timer.Stop();
