// See https://aka.ms/new-console-template for more information

using EasyMQ.Console;
using EasyMQ.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder => configurationBuilder.AddJsonFile("appsettings.json", false, true))
    .ConfigureServices((context, services) =>
    {
        services.AddEasyMq(factory =>
            {
                factory.Uri = new Uri("amqp://localhost:5672/");
                factory.DispatchConsumersAsync = true;
                factory.TopologyRecoveryEnabled = true;
                factory.AutomaticRecoveryEnabled = true;
                return factory;
            }, context.Configuration, "RabbitConsumerConfigurations", "RabbitProducerConfigurations")
            .AddEventConsumer<EasyMqEvent, EasyMqEventHandler>()
            .AddEventProducer<EasyMqEvent>();
        services.AddHostedService<EasyMqTimedProducerService>();
    })
    .RunConsoleAsync();
    