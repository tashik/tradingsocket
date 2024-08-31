using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TradingSocketClient.Deribit;
using Microsoft.Extensions.Options;

class Program
{
    static async Task Main(string[] args)
    {
        DeribitSocketServiceConfiguration.ConfigureServices();
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var serviceProvider = new ServiceCollection();
    }
}