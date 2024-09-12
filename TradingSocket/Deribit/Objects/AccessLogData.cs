namespace TradingSocket.Deribit.Objects;


public class AccessLogData : DeribitSocketObjectBase, ISubscriptionData
{
    public long Timestamp { get; set; }
    public string? Log { get; set; }
    public string? Ip { get; set; }
    public int Id { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}