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
            switch (_retryPolicy.Type)
            {
                case RetryType.Immediate:
                    {
                        var connection = _connectionProvider.AcquireProducerConnection();
                        using (var channel = connection.CreateModel())
                        {
                            var newBasicProps = channel.CreateBasicProperties();
                            newBasicProps.Headers = new Dictionary<string, object>();
                            if (args.BasicProperties.Headers is not null && args.BasicProperties.Headers.ContainsKey("x-retries"))
                            {
                                args.BasicProperties.Headers.TryGetValue("x-retries", out var retries);
                                newBasicProps.Headers.Add("x-retries", (int)retries + 1);
                            }
                            else
                            {
                                newBasicProps.Headers.Add("x-retries", 1);
                                newBasicProps.Headers.Add("x-delay", 1);
                            }
                            channel.BasicPublish($"{config.ExchangeName}.immediate.retry", config.QueueName, false, newBasicProps, args.Body);
                        }
                    }
                    break;
                case RetryType.Delayed:
                    {
                        var connection = _connectionProvider.AcquireProducerConnection();
                        using (var channel = connection.CreateModel())
                        {
                            var newBasicProps = channel.CreateBasicProperties();
                            newBasicProps.Headers = new Dictionary<string, object>();
                            if (args.BasicProperties.Headers is not null && args.BasicProperties.Headers.ContainsKey("x-retries"))
                            {
                                args.BasicProperties.Headers.TryGetValue("x-retries", out var retries);
                                newBasicProps.Headers.Add("x-retries", (int)retries + 1);
                                newBasicProps.Headers.Add("x-delay", Math.Pow(2, (int)retries + 1));
                            }
                            else
                            {
                                newBasicProps.Headers.Add("x-retries", 1);
                                newBasicProps.Headers.Add("x-delay", Math.Pow(2, 0));
                            }
                            channel.BasicPublish($"{config.ExchangeName}.delayed.retry", config.QueueName, false, newBasicProps, args.Body);
                        }
                    }
                    break;
            }
        }
    }
}
