namespace Evently.Common.Application.EventBus;

public interface IEventBus
{
    Task PublishAsync<T>(T intergrationEvent, CancellationToken cancellationToken = default) 
        where T : class, IIntegrationEvent;
}
