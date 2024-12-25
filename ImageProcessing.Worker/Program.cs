using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ImageProcessing.Worker;
using ImageProcessing.Core.Interfaces;
using ImageProcessing.Core.Utils;
using ImageProcessing.Common;
using System.Threading.Tasks;


public class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        // Load settings
        var settings = new WorkerSettings
        {
            Port = 12346,
            CoordinatorIp = "127.0.0.1",
            CoordinatorPort = 12345
        };
        services.AddSingleton(settings);
        services.AddLogging(configure => configure.AddConsole())
          .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

        services.AddSingleton<UdpHelper>(provider => new UdpHelper(provider.GetService<ILogger<UdpHelper>>(), settings.Port));
        services.AddSingleton<IImageProcessor, ImageUtilities>();
        services.AddSingleton<Worker>();

        var provider = services.BuildServiceProvider();
        var worker = provider.GetService<Worker>();
        if (worker != null)
            await worker.StartListening();
    }
}