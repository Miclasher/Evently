using Evently.Common.Application.EventBus;
using MassTransit;

namespace Evently.Common.Infrastructure.EventBus;

internal sealed class EventBus(IBus bus) : IEventBus
{
    public async Task PublishAsync<T>(T intergrationEvent, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent
    {
        await bus.Publish(intergrationEvent, cancellationToken);
    }
}
