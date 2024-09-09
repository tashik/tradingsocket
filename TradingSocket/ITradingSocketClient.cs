using TradingSocket.Entities;
using TradingSocketEvents.Domain;

namespace TradingSocket;

public interface ITradingSocketClient
{
    public Task ConnectAsync(CancellationToken cancellationToken);

    public Task SubscribeToPrivateChannelAsync(string channel, CancellationToken cancellationToken);

    public Task SubscribeToTickerAsync(string instrumentName, CancellationToken cancellationToken);
    public Task CloseAsync(CancellationToken cancellationToken);

    public Task GetInstrumentsByType(string primaryCurrency, InstrumentType? instrumentType,
        CancellationToken cancellationToken);
}