using Newtonsoft.Json.Linq;
using Serilog;
using TradingSocket.Entities;
using TradingSocket.Helpers;
using OkxEventHandlers = TradingSocket.Deribit.EventHandlers;

namespace TradingSocket.Okx;

public class OkxSocketClient : TradingSocketClientAbstract
{
    public OkxSocketClient(ConnectionConfig connectionConfig, TradingSocketEventDispatcher eventDispatcher, MessageRegistry msgRegistry, MessageIndexer indexer)
    : base(connectionConfig, eventDispatcher, msgRegistry, indexer) {}


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
    
    protected override async Task OnNewMessage(JToken? data, string args, int id)
    {
        MsgRegistry.TryGetRequestType(id, out var requestType);
        var jReq = new SocketResponse()
        {
            DataObject = data,
            Args = args,
            RequestType = requestType
        };
       
        await EventDispatcher.DispatchAsync(new OkxEventHandlers.MessageArrivedEvent(jReq));
    }
}