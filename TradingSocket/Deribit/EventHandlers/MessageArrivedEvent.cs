using TradingSocket.Entities;

namespace TradingSocket.Deribit.EventHandlers;

public class MessageArrivedEvent: IDomainEvent
{
    public SocketResponse? Req { get; }
    public DateTime ReceivedAt { get; }

    public MessageArrivedEvent(SocketResponse data)
    {
        Req = data;
        ReceivedAt = DateTime.Now;
    }    
}