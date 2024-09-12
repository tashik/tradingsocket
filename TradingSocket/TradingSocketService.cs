using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using TradingSocket.Deribit;
using TradingSocket.Deribit.Factories;
using TradingSocket.Entities;
using TradingSocketEvents;
using TradingSocketEvents.Domain;

namespace TradingSocket;

public class TradingSocketService : ITradingSocketService
{
    private readonly Dictionary<ExchangeType, ITradingSocketClient> _tradingSockets;
    private readonly EventsAggregator _eventsAggregator;

    public TradingSocketService(Dictionary<ExchangeType, ITradingSocketClient> tradingSockets, EventsAggregator aggregator)
    {
        _eventsAggregator = aggregator;
        _tradingSockets = tradingSockets;
    }

    public async Task StartAsync(ExchangeType exchange, CancellationToken cancellationToken)
    {
        await _tradingSockets[exchange].ConnectAsync(cancellationToken);
    }

    public async Task StopAsync(ExchangeType exchange, CancellationToken cancellationToken)
    {
        await _tradingSockets[exchange].CloseAsync(cancellationToken);
    }
    
    public EventsAggregator GetEventsAggregator()
    {
        return _eventsAggregator;
    }
}