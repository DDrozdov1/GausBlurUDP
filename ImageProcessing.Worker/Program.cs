using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ImageProcessing.Worker;
using ImageProcessing.Core.Interfaces;
using ImageProcessing.Core.Utils;
using ImageProcessing.Common;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.Arm;
using Microsoft.Extensions.Options;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        // Настройки из appsettings.json
        services.Configure<WorkerSettings>(options =>
        {
            options.Port = 12346;
            options.CoordinatorIp = "127.0.0.1";
            options.CoordinatorPort = 12345;
        });

        services.AddLogging(configure => configure.AddConsole())
          .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

        services.AddSingleton<UdpHelper>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<WorkerSettings>>().Value;
            return new UdpHelper(provider.GetService<ILogger<UdpHelper>>(), settings.Port);
        });

        services.AddSingleton<IImageProcessor, ImageUtilities>();
        services.AddSingleton<Worker>();

        var provider = services.BuildServiceProvider();
        var worker = provider.GetService<Worker>();

        if (worker != null)
        {
            await worker.StartListening();
        }
    }
}