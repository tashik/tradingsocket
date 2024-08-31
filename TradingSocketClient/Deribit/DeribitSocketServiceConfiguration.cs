using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace TradingSocketClient.Deribit;

public class DeribitSocketServiceConfiguration
{
    private static IServiceProvider _serviceProvider;

    public static void ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.AddSingleton<ILogger>(provider => new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger());
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }
    
    public static IServiceProvider ServiceProvider => _serviceProvider;
}