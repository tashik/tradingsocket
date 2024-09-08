using System.Collections.Concurrent;
using System.Threading.Channels;
using TradingSocketEvents.Domain;

namespace TradingSocketEvents;

public delegate Task ContractInfoHandler(Ticker contract);
public class EventsAggregator
{
    // Channels
    private readonly Channel<Ticker> _contractsChannel = Channel.CreateUnbounded<Ticker>();
    
    // Subscriber pools
    private readonly ConcurrentBag<ContractInfoHandler> _contractInfoHandlers = new();
    
    public async Task RaiseContractInfoEvent(Ticker contract)
    {
        await _contractsChannel.Writer.WriteAsync(contract);
    }
    
    public void SubscribeToSecurityInfo(ContractInfoHandler handler)
    {
        _contractInfoHandlers.Add(handler);
        _ = ProcessContractInfo();
    }
    
    
    private async Task ProcessContractInfo()
    {
        await foreach (var contract in _contractsChannel.Reader.ReadAllAsync())
        {
            var handlers = _contractInfoHandlers.ToArray();
            var tasks = handlers.Select(handler => handler(contract)).ToList();
            await Task.WhenAll(tasks);
        }
    }
}
