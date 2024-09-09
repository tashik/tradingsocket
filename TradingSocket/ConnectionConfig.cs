namespace TradingSocket;

public class ConnectionConfig
{
    public string Exchange { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";

    public string WsUrl { get; set; } = "";
    public string RestApiUrl { get; set; } = "";
}