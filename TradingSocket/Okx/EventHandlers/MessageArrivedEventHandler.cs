using System.Xml;
using Newtonsoft.Json;
using Serilog;
using TradingSocket.Deribit.EventHandlers;
using TradingSocket.Deribit.Factories;
using TradingSocket.Entities;
using TradingSocket.Okx.Objects;
using TradingSocketEvents.Domain;

namespace TradingSocket.Okx.EventHandlers;

public class MessageArrivedEventHandler : IDomainEventHandler<MessageArrivedEvent>
{
    private string? _accessToken;
    
    public Task HandleAsync(MessageArrivedEvent domainEvent)
    {
        var message = domainEvent.Req;
        var requestType = message!.RequestType;
        switch (requestType)
        {
            case SocketRequest.Authenticate:
                break;
            case SocketRequest.Subscribe:
                break;
            case SocketRequest.InstrumentList:
                if (message.DataObject == null)
                {
                    break;
                }

                var instrumentsResponse = JsonConvert.DeserializeObject<OkxInstrumentsResponse>(message.DataObject.ToString());
                if (instrumentsResponse is { Data: not null })
                {
                    foreach (var row in instrumentsResponse.Data)
                    {
                    }
                }

                break;
        }

        return Task.CompletedTask;
    }
}