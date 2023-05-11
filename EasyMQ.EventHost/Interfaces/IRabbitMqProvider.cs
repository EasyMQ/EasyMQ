namespace EasyMQ.Consumer.Interfaces;

public interface IRabbitMqProvider
{
    void InitializeConnection();
    void ShutDown();
}