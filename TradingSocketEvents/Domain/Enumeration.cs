using System.ComponentModel;

namespace TradingSocketEvents.Domain;

public enum ExchangeType
{
    [Description("Deribit")]
    Deribit,
    [Description("Okx")]
    Okx
}

public enum InstrumentType
{
    [Description("Spot")]
    Spot,
    [Description("Futures")]
    Futures,
    [Description("Option")]
    Option
}

public enum OptionType
{
    [Description("Call")]
    Call,
    [Description("Put")]
    Put
}

public enum ContractType
{
    [Description("Linear")]
    Linear,
    [Description("Inverse")]
    Inverse
}