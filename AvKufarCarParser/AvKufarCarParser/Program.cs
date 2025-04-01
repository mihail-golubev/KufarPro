using AvKufarCarParser.Kufar;
using AvKufarCarParser.Logging;
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
            var logFilePath = Path.Combine(AppContext.BaseDirectory, "AutoKufarParser.log");

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
                    logging.AddProvider(new FileLoggerProvider(logFilePath));
                })
                .Build();

            await host.RunAsync();
        }
    }
}
