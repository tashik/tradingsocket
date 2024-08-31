using Newtonsoft.Json;

namespace TradingSocketClient.Deribit.Entities;


public class AccessLogData : ISubscriptionData
{
    public long Timestamp { get; set; }
    public string? Log { get; set; }
    public string? Ip { get; set; }
    public int Id { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}