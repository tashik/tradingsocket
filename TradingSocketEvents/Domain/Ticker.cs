namespace TradingSocketEvents.Domain;

public class Ticker
{
    public ExchangeType Exchange { get; set; }
    public string Code { get; set; } = "";
    public string Underlying { get; set; } = "";
    public string BaseCurrency { get; set; } = "";
    public string QuoteCurrency { get; set; } = "";
    public string SettleCurrency { get; set; } = "";
    public decimal Multiplier { get; set; } = 1m;
    public int MaxLeverage { get; set; } = 1;
    public OptionType? OptionType { get; set; }
    public DateTime? ExpDate { get; set; }
    public decimal TickSize { get; set; }
    public decimal LotSize { get; set; }
    public decimal MinSize { get; set; }
    public ContractType ContractType { get; set; }
    public decimal Strike { get; set; }
    public InstrumentType Type { get; set; }
}