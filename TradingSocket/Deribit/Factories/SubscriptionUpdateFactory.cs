using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TradingSocket.Deribit.Entities;
using TradingSocket.Entities;

namespace TradingSocket.Deribit.Factories;

public static class SubscriptionUpdateFactory
{
    public static ISubscriptionUpdate? CreateSubscriptionUpdate(SocketResponse socketResponse)
    {
        if (socketResponse.DataObject is null)
        {
            return null;
        }
        var channel = socketResponse.DataObject["params"]?["channel"]?.ToString();

        if (channel == "user.access_log")
        {
            return socketResponse.DataObject.ToObject<SubscriptionUpdate<AccessLogData>>();
        }
        
        return null;
    }
}

