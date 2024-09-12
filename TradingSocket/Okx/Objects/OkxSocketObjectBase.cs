using TradingSocket.Entities;
using TradingSocketEvents.Domain;

namespace TradingSocket.Okx.Objects;

public class OkxSocketObjectBase : BaseSocketObjectInterface
{
    public ExchangeType Exchange { get; } = ExchangeType.Deribit;
}