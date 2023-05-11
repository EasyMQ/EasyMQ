// See https://aka.ms/new-console-template for more information

using EasyMQ.Console;
using EasyMQ.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

await Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder => configurationBuilder.AddJsonFile("appsettings.json", false, true))
    .ConfigureServices((context, services) =>
    {
        services.AddEasyMqConsumer(factory =>
            {
                factory.Uri = new Uri("amqp://localhost:5672/");
                factory.DispatchConsumersAsync = true;
                factory.TopologyRecoveryEnabled = true;
                factory.AutomaticRecoveryEnabled = true;
                return factory;
            }, context.Configuration, "RabbitConsumerConfigurations")
            .AddEventConsumer<EasyMqEvent, EasyMqEventHandler>();
    })
    .RunConsoleAsync();