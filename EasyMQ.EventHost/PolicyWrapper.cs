using EasyMQ.Abstractions.Consumer;
using EasyMQ.EventHost.Abstractions;
using Polly;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMQ.Consumer
{
    internal class PolicyWrapper
    {
        private readonly RetryPolicy _retryPolicy;
        private readonly IConnectionProvider _connectionProvider;

        public PolicyWrapper(RetryPolicy retryPolicy,
            IConnectionProvider connectionProvider)
        {
            _retryPolicy = retryPolicy;
            _connectionProvider = connectionProvider;
        }

        public void ApplyPolicy(ConsumerConfiguration config, RabbitMQ.Client.Events.BasicDeliverEventArgs args)
        {
            switch (_retryPolicy.RetryType)
            {
                case RetryType.Immediate:

                    var connection = _connectionProvider.AcquireProducerConnection();
                    using(var channel = connection.CreateModel())
                    {
                        if (args.BasicProperties.Headers is null)
                        {
                            args.BasicProperties.Headers = new Dictionary<string, object>();
                        }
                        if (args.BasicProperties.Headers.ContainsKey("x-retries"))
                        {
                            args.BasicProperties.Headers.TryGetValue("x-retries", out var retries);
                            args.BasicProperties.Headers.Remove("x-retries");
                            args.BasicProperties.Headers.Add("x-retries", (int)retries + 1);
                        }
                        else
                        {
                            args.BasicProperties.Headers.Add("x-retries", 1);
                            args.BasicProperties.Headers.Add("x-delay", 1000);
                        }
                        channel.BasicPublish($"{config.ExchangeName}.immediate.retry", config.QueueName, false, args.BasicProperties, args.Body);
                    }
                    break;
            }
        }
    }
}
