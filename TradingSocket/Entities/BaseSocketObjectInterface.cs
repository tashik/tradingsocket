using TradingSocketEvents.Domain;

namespace TradingSocket.Entities;

interface BaseSocketObjectInterface
{
    public ExchangeType Exchange { get; }
}