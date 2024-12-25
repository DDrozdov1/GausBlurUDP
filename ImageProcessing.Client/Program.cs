using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ImageProcessing.Client;
using ImageProcessing.Common;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        // Load settings
        var coordinatorIp = "127.0.0.1";
        var coordinatorPort = 12345;
        services.AddLogging(configure => configure.AddConsole())
          .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);
        services.AddSingleton<UdpHelper>(provider => new UdpHelper(provider.GetService<ILogger<UdpHelper>>(), 0));
        services.AddSingleton<ImageSender>(provider => new ImageSender(provider.GetService<UdpHelper>(), provider.GetService<ILogger<ImageSender>>(), coordinatorIp, coordinatorPort));

        var provider = services.BuildServiceProvider();
        var imageSender = provider.GetService<ImageSender>();
        if (imageSender != null)
        {
            await imageSender.SendImage("art.png", 1);

        }
    }
}