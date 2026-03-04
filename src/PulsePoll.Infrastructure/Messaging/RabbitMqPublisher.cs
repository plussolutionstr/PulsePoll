using MassTransit;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Infrastructure.Messaging;

public class RabbitMqPublisher(ISendEndpointProvider sendEndpointProvider) : IMessagePublisher
{
    public async Task PublishAsync<T>(T message, string queueName, CancellationToken ct = default) where T : class
    {
        var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));
        await endpoint.Send(message, ct);
    }
}
