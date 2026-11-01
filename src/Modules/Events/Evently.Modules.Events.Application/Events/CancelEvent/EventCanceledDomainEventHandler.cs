using Evently.Common.Application.Caching;
using Evently.Common.Application.EventBus;
using Evently.Common.Application.Messaging;
using Evently.Modules.Events.Domain.Events;
using Evently.Modules.Events.Application.Events.SearchEvents;
using Evently.Modules.Events.IntegrationEvents;

namespace Evently.Modules.Events.Application.Events.CancelEvent;

internal sealed class EventCanceledDomainEventHandler(IEventBus eventBus, ICacheService cacheService)
    : DomainEventHandler<EventCanceledDomainEvent>
{
    public override async Task Handle(
        EventCanceledDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await SearchEventsCache.InvalidateAsync(cacheService, cancellationToken);

        await eventBus.PublishAsync(
            new EventCanceledIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.EventId),
            cancellationToken);
    }
}
