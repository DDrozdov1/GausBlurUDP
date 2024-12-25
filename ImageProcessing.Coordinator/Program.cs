using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ImageProcessing.Coordinator;
using ImageProcessing.Common;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        // Load settings
        var settings = new CoordinatorSettings
        {
            Port = 12345,
            WorkerIp = "127.0.0.1",
            WorkerPort = 12346
        };
        services.AddSingleton(settings);
        services.AddLogging(configure => configure.AddConsole())
          .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);


        services.AddSingleton<UdpHelper>(provider => new UdpHelper(provider.GetService<ILogger<UdpHelper>>(), settings.Port));
        services.AddSingleton<Coordinator>();



        var provider = services.BuildServiceProvider();
        var coordinator = provider.GetService<Coordinator>();
        if (coordinator != null)
            await coordinator.StartListening();


    }
}