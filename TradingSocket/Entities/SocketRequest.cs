using System.ComponentModel;

namespace TradingSocket.Entities;

public enum SocketRequest
{
    [Description("Get instruments")]
    GetInstrument,
    [Description("Subscribe")]
    Subscribe,
    [Description("Authenticate")]
    Authenticate
}