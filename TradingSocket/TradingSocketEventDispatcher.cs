using Microsoft.Extensions.DependencyInjection;

namespace TradingSocket;

public interface IDomainEvent
{
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent);
}

public interface IEventDispatcher
{
    Task DispatchAsync<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent;
}

public class TradingSocketEventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public TradingSocketEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>().ToList();

        if (handlers == null || !handlers.Any())
        {
            throw new Exception($"No handler registered for event type {typeof(TDomainEvent).Name}");
        }

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(domainEvent);
        }
    }
}