using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AvKufarCarParser
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<BotService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
