using TradingSocket.Entities;

namespace TradingSocket;

public interface ITradingSocketClient
{
    public Task ConnectAsync(CancellationToken cancellationToken);

    public Task SubscribeToPrivateChannelAsync(string channel, CancellationToken cancellationToken);

    public Task SubscribeToTickerAsync(string instrumentName, CancellationToken cancellationToken);
    public Task CloseAsync(CancellationToken cancellationToken);
}