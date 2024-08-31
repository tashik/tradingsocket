using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TradingSocketClient.Deribit.Entities;

namespace TradingSocketClient.Deribit.Factories;

public static class SubscriptionUpdateFactory
{
    public static ISubscriptionUpdate? CreateSubscriptionUpdate(string jsonString)
    {
        var jsonObject = JObject.Parse(jsonString);
        var channel = jsonObject["params"]?["channel"]?.ToString();

        if (channel == "user.access_log")
        {
            return jsonObject.ToObject<SubscriptionUpdate<AccessLogData>>();
        }
        
        return null;
    }
}

