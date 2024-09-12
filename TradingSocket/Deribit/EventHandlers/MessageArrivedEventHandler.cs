using Serilog;
using TradingSocket.Deribit.Factories;
using TradingSocket.Deribit.Objects;
using TradingSocket.Entities;

namespace TradingSocket.Deribit.EventHandlers;

public class MessageArrivedEventHandler : IDomainEventHandler<MessageArrivedEvent>
{
    private string? _accessToken;
    
    public Task HandleAsync(MessageArrivedEvent domainEvent)
    {
        var message = domainEvent.Req;
        var requestType = message.RequestType;
        switch (requestType)
        {
            case SocketRequest.Authenticate:
                if (message.DataObject?["access_token"] != null)
                {
                    _accessToken = message.DataObject?["access_token"]?.ToString();

                    Log.Information("Authentication successful. Access Token: " + _accessToken);
                }

                break;
            case SocketRequest.Subscribe:
                // Use the factory to create the appropriate subscription update object
                var subscriptionUpdate = SubscriptionUpdateFactory.CreateSubscriptionUpdate(message);
                var data = subscriptionUpdate?.Data;
                if (data is AccessLogData accessLogData)
                {
                    Console.WriteLine($"Timestamp: {accessLogData.Timestamp}");
                    Console.WriteLine($"Log: {accessLogData.Log}");
                    Console.WriteLine($"IP: {accessLogData.Ip}");
                    Console.WriteLine($"ID: {accessLogData.Id}");
                    Console.WriteLine($"Country: {accessLogData.Country}");
                    Console.WriteLine($"City: {accessLogData.City}");
                }
                // Handle other data types similarly
                break;
            case SocketRequest.InstrumentList:
                break;
        }

        return Task.CompletedTask;
    }
}