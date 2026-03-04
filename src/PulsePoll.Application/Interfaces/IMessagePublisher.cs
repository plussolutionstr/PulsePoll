namespace PulsePoll.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string queueName, CancellationToken ct = default) where T : class;
}
