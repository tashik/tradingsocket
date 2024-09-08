using Serilog;
using TradingSocket.Helpers;

namespace TradingSocket.Okx;

public class OkxSocketClient : TradingSocketClientAbstract
{
    public OkxSocketClient(ConnectionConfig connectionConfig, TradingSocketEventDispatcher eventDispatcher, MessageRegistry messageRegistry, MessageIndexer indexer)
    : base(connectionConfig, eventDispatcher, messageRegistry, indexer) {}


    protected override string GetClientName()
    {
        return "Okx";
    }

    protected override bool IsAccessTokenRequired()
    {
        return false;
    }

    public override async Task SubscribeToPrivateChannelAsync(string channel, CancellationToken cancellationToken)
    {
        var subscribeMessage = new
        {
            op = "subscribe",
            args = new[] { channel }
        };
        var message = Newtonsoft.Json.JsonConvert.SerializeObject(subscribeMessage);
        await SendAsync(message, cancellationToken);
    }

    public override async Task SubscribeToTickerAsync(string instrumentName, CancellationToken cancellationToken)
    {
        var subscribeMessage = new
        {
            op = "subscribe",
            args = new[] { $"ticker:{instrumentName}" }
        };
        var message = Newtonsoft.Json.JsonConvert.SerializeObject(subscribeMessage);
        await SendAsync(message, cancellationToken);
    }

    protected override Task ProcessResponseAsync(string message)
    {
        throw new NotImplementedException();
    }

    protected override Task AuthenticateAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}