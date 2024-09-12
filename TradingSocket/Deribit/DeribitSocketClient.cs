using Newtonsoft.Json.Linq;
using Serilog;
using TradingSocket.Entities;
using TradingSocket.Helpers;
using TradingSocketEvents.Domain;
using DeribitEventHandlers = TradingSocket.Deribit.EventHandlers;

namespace TradingSocket.Deribit;

public class DeribitSocketClient : TradingSocketClientAbstract
{
    
    private string? _accessToken;

    public DeribitSocketClient(ConnectionConfig connectionConfig, TradingSocketEventDispatcher eventDispatcher, MessageRegistry msgRegistry, MessageIndexer indexer, HttpClient httpClient) 
        : base(connectionConfig, eventDispatcher, msgRegistry, indexer, httpClient)
    {
    }

    protected override string GetClientName()
    {
        return "Deribit";
    }

    protected override bool IsAccessTokenRequired()
    {
        return true;
    }

    public override async Task SubscribeToPrivateChannelAsync(string channel, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            throw new InvalidOperationException("Client is not authenticated.");
        }
        var msgId = MsgIndexer.GetIndex();
        var subscribeMessage = new
        {
            jsonrpc = "2.0",
            method = "private/subscribe",
            @params = new
            {
                channels = new[] { channel }
            },
            id = msgId
        };
        MsgRegistry.RegisterRequest(msgId, SocketRequest.Subscribe);
        var message = Newtonsoft.Json.JsonConvert.SerializeObject(subscribeMessage);
        await SendAsync(message, cancellationToken);
    }

    public override async Task SubscribeToTickerAsync(string instrumentName, CancellationToken cancellationToken)
    {
        var msgId = MsgIndexer.GetIndex();
        var subscribeMessage = new
        {
            jsonrpc = "2.0",
            method = "public/subscribe",
            @params = new
            {
                channels = new[] { $"ticker.{instrumentName}.raw" }
            },
            id = msgId
        };
        MsgRegistry.RegisterRequest(msgId, SocketRequest.Subscribe);
        var message = Newtonsoft.Json.JsonConvert.SerializeObject(subscribeMessage);
        await SendAsync(message, cancellationToken);
    }
    

    protected override async Task AuthenticateAsync(CancellationToken cancellationToken)
    {
        var msgId = MsgIndexer.GetIndex();
        var authRequest = new
        {
            jsonrpc = "2.0",
            method = "public/auth",
            @params = new
            {
                grant_type = "client_credentials",
                client_id = ClientId,
                client_secret = ClientSecret
            },
            id = msgId
        };
        MsgRegistry.RegisterRequest(msgId, SocketRequest.Authenticate);
        var message = Newtonsoft.Json.JsonConvert.SerializeObject(authRequest);
        await SendAsync(message, cancellationToken);
    }

    protected override async Task ProcessResponseAsync(string message)
    {
        var jsonResponse = JObject.Parse(message);
        var resultWrapper = jsonResponse["result"];
        var msgId = jsonResponse["id"];
        if (msgId != null && resultWrapper != null)
        {
            await OnNewMessage(resultWrapper, message, msgId.ToObject<int>());
        }
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
       
        await EventDispatcher.DispatchAsync(new DeribitEventHandlers.MessageArrivedEvent(jReq));
    }

    public override async Task GetInstrumentsByType(string primaryCurrency, InstrumentType? instrumentType, CancellationToken cancellationToken)
    {
        var type = "";
        if (instrumentType != null)
        {
            switch (instrumentType)
            {
                case InstrumentType.Futures:
                    type = "future";
                    break;
                case InstrumentType.Option:
                    type = "option";
                    break;
                case InstrumentType.Spot:
                    type = "spot";
                    break;
                default:
                    throw new Exception("Unsupported instrument type");
            }
        }

        var reqParams = $"currency={primaryCurrency}&expired=false";
        if (type != "")
        {
            reqParams += $"&kind={type}";
        }
        var endpoint = $"{RestApiUrl}/public/get_instruments?{reqParams}";
        var content = await SendGetRequestAsync(endpoint, cancellationToken);
        if (content != null)
        {
            var jDoc = JObject.Parse(content);
            var jMessage = new SocketResponse()
            {
                Args = reqParams,
                DataObject = jDoc,
                RequestType = SocketRequest.InstrumentList
            };
            await EventDispatcher.DispatchAsync(new DeribitEventHandlers.MessageArrivedEvent(jMessage)); 
        }
    }
}