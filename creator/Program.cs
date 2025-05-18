using creator.util;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using creator.settings;
using creator.interfaces;
using creator.queue;
using creator;

// Строим конфигурацию
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json") // здесь можно было добавить метаданные и сделать авто проверку конфигурации
    .AddEnvironmentVariables() // переопределяем значения из среды
    .Build();

// Поддержка DI
var services = new ServiceCollection();

// Регистрируем конфигурации
services.Configure<RabbitMq>(config.GetSection("RabbitMq"));
services.Configure<Generation>(config.GetSection("Generation"));

services.AddSingleton<IMessageBatchSender, RabbitMessageSender>();
services.AddSingleton<IMessageGenerator, RandomMessageGenerator>();
services.AddSingleton<IMetricsTracker, ConsoleMetricsTracker>();
services.AddSingleton<IRateRegulatedSender, RateRegulatedSender>();

var serviceProvider = services.BuildServiceProvider();

var sender = serviceProvider.GetRequiredService<IRateRegulatedSender>();
sender.Start();

// Ожидаем завершения
Console.Read();
sender.Stop();