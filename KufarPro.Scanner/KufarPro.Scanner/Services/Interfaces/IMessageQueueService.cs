namespace KufarPro.Scanner.Services.Interfaces
{
    public interface IMessageQueueService
    {
        Task InitializeAsync();
        Task PublishAsync<T>(string queueName, T message);
        Task ConsumeAsync<T>(string queueName, Func<T, Task> handler);
    }
}
