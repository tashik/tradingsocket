using Newtonsoft.Json.Linq;

namespace TradingSocket.Entities;

public class SocketResponse
{
    public string Args { get; set; } = "";
    
    public JToken? DataObject { get; set; }
    
    public SocketRequest RequestType { get; set; }
}