using System.ComponentModel;

namespace TradingSocket.Entities;

public enum ExchangeType
{
    [Description("Deribit")]
    Deribit,
    [Description("Okx")]
    Okx
}