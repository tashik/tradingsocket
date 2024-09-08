namespace TradingSocket;

public class ConnectionConfig
{
    public string Exchange { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";

    public string WsUrl { get; set; } = "";
}