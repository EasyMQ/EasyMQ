namespace EasyMQ.EventHost.Abstractions;

public interface IRabbitMqProvider
{
    void InitializeConnection();
    void ShutDown();
}