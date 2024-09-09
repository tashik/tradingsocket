using Newtonsoft.Json.Linq;
using Serilog;
using TradingSocket.Entities;
using TradingSocket.Helpers;
using TradingSocketEvents.Domain;
using OkxEventHandlers = TradingSocket.Deribit.EventHandlers;

namespace TradingSocket.Okx;

public class OkxSocketClient : TradingSocketClientAbstract
{
    public OkxSocketClient(ConnectionConfig connectionConfig, TradingSocketEventDispatcher eventDispatcher, MessageRegistry msgRegistry, MessageIndexer indexer, HttpClient httpClient)
    : base(connectionConfig, eventDispatcher, msgRegistry, indexer, httpClient) {}


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
    
    public override async Task GetInstrumentsByType(string primaryCurrency, InstrumentType? instrumentType, CancellationToken cancellationToken)
    {
        var type = "";
        if (instrumentType != null)
        {
            switch (instrumentType)
            {
                case InstrumentType.Futures:
                    type = "FUTURE";
                    break;
                case InstrumentType.Option:
                    type = "OPTION";
                    break;
                case InstrumentType.Spot:
                    type = "SPOT";
                    break;
                default:
                    throw new Exception("Unsupported instrument type");
            }
        }

        var reqParams = $"uly={primaryCurrency}";
        if (type != "")
        {
            reqParams += $"&instType={type}";
        }
        var endpoint = $"{RestApiUrl}/market/instruments?{reqParams}";
        var content = await SendGetRequestAsync(endpoint, cancellationToken);
        if (content != null)
        {
            var jDoc = JObject.Parse(content);
            var jMessage = new SocketResponse()
            {
                Args = reqParams,
                DataObject = jDoc["data"],
                RequestType = SocketRequest.InstrumentList
            };
            await EventDispatcher.DispatchAsync(new OkxEventHandlers.MessageArrivedEvent(jMessage)); 
        }
    }
}