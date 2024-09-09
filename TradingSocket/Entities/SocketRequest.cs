using System.ComponentModel;

namespace TradingSocket.Entities;

public enum SocketRequest
{
    [Description("Subscribe")]
    Subscribe,
    [Description("Authenticate")]
    Authenticate,
    [Description("Instrument List")]
    InstrumentList,
}