using AvKufarCarParser.Kufar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace AvKufarCarParser
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(configure => configure.AddSimpleConsole());
                    services.AddSingleton<HttpClient>();
                    services.AddSingleton<ITelegramBotClient>(sp =>
                        new TelegramBotClient(Util.MikhailBotToken));
                    services.AddSingleton<KufarProcessor>();
                    services.AddHostedService<BotService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSimpleConsole(options =>
                    {
                        options.TimestampFormat = "[dd/MM/yyyy HH:mm:ss] ";
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}
