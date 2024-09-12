using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using TradingSocket.Deribit;
using TradingSocket;
using TradingSocket.Entities;
using TradingSocketEvents.Domain;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var tradingSocketConfigurator = new TradingSocketServiceConfiguration(configuration);
        var tradingServiceProvider = tradingSocketConfigurator.ConfigureServices();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.Configure<ConnectionConfig[]>(configuration.GetSection("ConnectorSettings"));
        serviceCollection.AddSingleton<ITradingSocketClient, DeribitSocketClient>();
        serviceCollection.AddSingleton(sp => tradingServiceProvider.GetRequiredService<TradingSocketService>());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        CancellationTokenSource cts = new CancellationTokenSource();
        
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        var clientService = serviceProvider.GetRequiredService<TradingSocketService>();
        var eventsAggregator = clientService.GetEventsAggregator();
        
        eventsAggregator.SubscribeToSecurityInfo( contract =>
        {
            Log.Information("Security contract arrived");
            return Task.CompletedTask;
        });
        await clientService.StartAsync(ExchangeType.Deribit, cts.Token);
        
        
        Console.WriteLine("Press any key to stop...");
        Console.ReadKey();

        // Stop the service
        await clientService.StopAsync(ExchangeType.Deribit, cts.Token);
        await Log.CloseAndFlushAsync();
    }
}
