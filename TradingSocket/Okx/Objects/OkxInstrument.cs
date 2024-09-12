using Newtonsoft.Json;

namespace TradingSocket.Okx.Objects;

public class OkxInstrumentsResponse
{
    [JsonProperty("data")]
    public List<OkxInstrument>? Data { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; } = "";

    [JsonProperty("msg")]
    public string Msg { get; set; } = "";
}

public class OkxInstrument : OkxSocketObjectBase
{
    [JsonProperty("instId")]
    public string InstId { get; set; }
    [JsonProperty("instType")]
    public string InstType { get; set; } = "";
    [JsonProperty("baseCcy")]
    public string BaseCurrency { get; set; } = "";
    [JsonProperty("quoteCcy")]
    public string QuoteCurrency { get; set; } = "";
    [JsonProperty("uly")]
    public string Underlying { get; set; } = "";
    [JsonProperty("tickSz")]
    public string TickSize { get; set; } = "1";
    [JsonProperty("lotSz")]
    public string LotSize { get; set; } = "0";
    [JsonProperty("settleCcy")]
    public string SettleCurrency { get; set; } = "";
    [JsonProperty("ctMult")]
    public decimal Multiplier { get; set; } = 1m;
    [JsonProperty("lever")]
    public int MaxLeverage { get; set; } = 1;
    [JsonProperty("optType")]
    public string OptionType { get; set; } = "";
    [JsonProperty("expTime")]
    public string ExpDate { get; set; } = "";
    [JsonProperty("minSz")]
    public decimal MinSize { get; set; }

    [JsonProperty("ctType")]
    public string ContractType { get; set; } = "";
    [JsonProperty("stk")]
    public decimal Strike { get; set; }
}