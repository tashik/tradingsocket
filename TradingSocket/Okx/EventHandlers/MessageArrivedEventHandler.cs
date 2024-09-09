using Serilog;
using TradingSocket.Deribit.Entities;
using TradingSocket.Deribit.EventHandlers;
using TradingSocket.Deribit.Factories;
using TradingSocket.Entities;

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
                break;
        }

        return Task.CompletedTask;
    }
}