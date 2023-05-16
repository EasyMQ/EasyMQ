// See https://aka.ms/new-console-template for more information

using EasyMQ.Console;
using EasyMQ.Consumers;
using EasyMQ.Extensions.DependencyInjection;
using EasyMQ.Publisher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder => configurationBuilder
        .AddJsonFile("appsettings.json", false, true)
        .AddUserSecrets<Program>())
    .ConfigureServices((context, services) =>
    {
        services.AddEasyMq(context.Configuration, builder =>
            {
                builder.WithConnectionFactory(factory =>
                {
                    factory.Uri =
                        new Uri(
                            $"amqp://{context.Configuration["rmq_username"]}:{context.Configuration["rmq_password"]}@localhost:5672/");
                    factory.DispatchConsumersAsync = true;
                    factory.TopologyRecoveryEnabled = true;
                    factory.AutomaticRecoveryEnabled = true;
                    return factory;
                });
                builder.WithConsumerSection("RabbitConsumerConfigurations");
                builder.WithProducerSection("RabbitProducerConfigurations");
            })
            .AddEventConsumer<EasyMqTopicEvent, EasyMqTopicEventHandler>()
            .AddEventProducer<EasyMqTopicEvent>()
            .AddEventConsumer<EasyMqHeaderEvent, EasyMqHeaderEventHandler>()
            .AddEventProducer<EasyMqHeaderEvent>();
        
        services.AddHostedService<EasyMqTimedProducerService>();
    })
    .RunConsoleAsync();
    