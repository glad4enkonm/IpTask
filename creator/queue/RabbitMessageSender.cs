using System.Diagnostics;
using System.Text;
using System.Text.Json;
using creator.interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using creator.settings;

namespace creator.queue;

public class RabbitMessageSender : IMessageBatchSender
{
    private readonly IChannel _channel;
    private readonly IMessageGenerator _generator;
    private readonly IMetricsTracker _metricsTracker;

    private readonly RabbitMq _settings;

    public RabbitMessageSender(        
        IMessageGenerator generator,
        IMetricsTracker metricsTracker,
        IOptions<RabbitMq> settings)
    {        
        _generator = generator;
        _metricsTracker = metricsTracker;
        _settings = settings.Value;
        _channel = InitializeQueue();
    }

    public async Task SendBatch(uint batchSize)
    {        
        uint messagesRequestedToSend = 0; // сообщения для которых создавали задачу отправки
        uint messagesSent = 0; // те, для которхы задача отправки завершилась без ошибок

        var stopwatch = Stopwatch.StartNew();
        var publishTasks = new List<ValueTask>();

        try
        {
            for (int i = 0; i < batchSize; i++)
            {
                var message = _generator.Generate();
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                ValueTask publishTask = _channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: _settings.QueueName,
                    mandatory: true,
                    basicProperties: new BasicProperties { Persistent = true },
                    body: body
                );
                publishTasks.Add(publishTask);
                messagesRequestedToSend++;
            }
            await AwaitAndLogOnError(publishTasks);
            messagesSent = messagesRequestedToSend;
        }
        finally
        {
            stopwatch.Stop();
            _metricsTracker.TrackBatch(messagesSent, stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Ожидаем каждое из отправленных сообщений и показываем ошибки если есть
    /// согласно https://www.rabbitmq.com/tutorials/tutorial-seven-dotnet#strategy-2-publishing-messages-in-batches
    /// </summary>
    /// <param name="publishTasks">Список с задачами на отправку сообщений</param>
    /// <returns></returns>
    private async Task AwaitAndLogOnError(List<ValueTask> publishTasks)
    {
        foreach (ValueTask pt in publishTasks)
            try
            {
                await pt;
            }
            catch (Exception ex)
            {
                _metricsTracker.TrackError(ex);
            }
    }

    /// <summary>
    /// Создаёт и инициализурет очередь
    /// </summary>
    private IChannel InitializeQueue()
    {
        var factory = new ConnectionFactory { HostName = _settings.HostName };

        var connection = factory.CreateConnectionAsync().Result;
        var channel = connection.CreateChannelAsync().Result;

        channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true, // TODO: вынести остальные параметры в настройки
            exclusive: false,
            autoDelete: false,
            arguments: null
        ).Wait();
        return channel;
    }
}