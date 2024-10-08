using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using TradingSocket.Deribit;
using DeribitEventHandlers = TradingSocket.Deribit.EventHandlers;
using OkxEventHandlers = TradingSocket.Okx.EventHandlers;
using TradingSocket.Entities;
using TradingSocket.Helpers;
using TradingSocket.Okx;
using TradingSocketEvents;
using TradingSocketEvents.Domain;

namespace TradingSocket;

public class TradingSocketServiceConfiguration
{
    private readonly IConfiguration _configuration;

    public TradingSocketServiceConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ILogger>(provider => new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger());
        serviceCollection.AddSingleton<EventsAggregator>();
        serviceCollection.AddSingleton<TradingSocketEventDispatcher>();
        serviceCollection.AddSingleton<MessageRegistry>();
        serviceCollection.AddSingleton<MessageIndexer>();
        
        serviceCollection.AddHttpClient();
        
        var connectorSettings = _configuration.GetSection("ConnectorSettings").Get<List<ConnectionConfig>>();
        if (connectorSettings == null)
        {
            throw new Exception("Application misconfigure, add ConnectorSettings");
        }
        
        var clients = new Dictionary<ExchangeType, ITradingSocketClient>();
        var messageIndexer = serviceCollection.BuildServiceProvider().GetRequiredService<MessageIndexer>();
        var messageRegistry = serviceCollection.BuildServiceProvider().GetRequiredService<MessageRegistry>();
        var httpClientFactory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();

        foreach (var setting in connectorSettings)
        {
            switch (setting.Exchange.ToLower())
            {
                case "deribit":
                    serviceCollection.AddTransient<IDomainEventHandler<DeribitEventHandlers.MessageArrivedEvent>, DeribitEventHandlers.MessageArrivedEventHandler>();
                    clients.Add(ExchangeType.Deribit, new DeribitSocketClient(setting, serviceCollection.BuildServiceProvider().GetRequiredService<TradingSocketEventDispatcher>(), messageRegistry, messageIndexer, httpClient));
                    break;
                case "okx":
                    serviceCollection.AddTransient<IDomainEventHandler<OkxEventHandlers.MessageArrivedEvent>, OkxEventHandlers.MessageArrivedEventHandler>();
                    clients.Add(ExchangeType.Okx, new OkxSocketClient(setting, serviceCollection.BuildServiceProvider().GetRequiredService<TradingSocketEventDispatcher>(), messageRegistry, messageIndexer, httpClient));
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported exchange: {setting.Exchange}");
            }
        }
        serviceCollection.AddSingleton(new TradingSocketService(clients, serviceCollection.BuildServiceProvider().GetRequiredService<EventsAggregator>()));
        
        return serviceCollection.BuildServiceProvider();
    }
}