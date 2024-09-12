using TradingSocket.Entities;
using TradingSocketEvents.Domain;

namespace TradingSocket.Deribit.Objects;

public class DeribitSocketObjectBase : BaseSocketObjectInterface
{
    public ExchangeType Exchange { get; } = ExchangeType.Deribit;
}