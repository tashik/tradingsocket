using Newtonsoft.Json;
using TradingSocketClient.Deribit.Entities;

namespace TradingSocketClient.Deribit.Factories;

public interface ISubscriptionUpdate
{
    string Channel { get; set; }
    ISubscriptionData? Data { get; }
}

public class SubscriptionUpdate<T> : ISubscriptionUpdate where T : ISubscriptionData
{
    public UpdateParams<T> Params { get; set; }
    public string Channel { get; set; }

    public ISubscriptionData? Data => Params.Data;
}

public class UpdateParams<T> where T : ISubscriptionData
{
    public T? Data { get; set; }
}
