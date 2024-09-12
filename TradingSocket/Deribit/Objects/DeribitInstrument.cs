using Newtonsoft.Json;
using TradingSocket.Entities;
using TradingSocketEvents.Domain;

namespace TradingSocket.Deribit.Objects;

public class DeribitInstrument: DeribitSocketObjectBase
{
    [JsonProperty("tick_size")]
    public decimal TickSize { get; set; }
    [JsonProperty("settlement_currency")]
    public string SettlementCurrency { get; set; } = "";
    [JsonProperty("quote_currency")]
    public string QuoteCurrency { get; set; } = "";
    [JsonProperty("base_currency")]
    public string BaseCurrency { get; set; } = "";
    [JsonProperty("min_trade_amount")]
    public decimal MinSize { get; set; }
    [JsonProperty("max_leverage")]
    public int MaxLeverage { get; set; }
    [JsonProperty("kind")]
    public string InstType { get; set; } = "";
    [JsonProperty("instrument_name")]
    public string Code { get; set; } = "";
    [JsonProperty("instrument_type")]
    public string ContractType { get; set; } = "";
    [JsonProperty("expiration_timestamp")]
    public long ExpirationTimestamp { get; set; }
    [JsonProperty("contract_size")]
    public decimal LotSize { get; set; }
    [JsonProperty("strike")]
    public decimal Strike { get; set; }
    [JsonProperty("option_type")]
    public string OptionType { get; set; } = "";
}