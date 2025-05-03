using AvKufarCarParser.DataAccess;
using AvKufarCarParser.Helpers;
using AvKufarCarParser.Kufar;
using AvKufarCarParser.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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
                    services.AddSingleton<IMongoClient>(sp =>
                    {
                        var settings = MongoClientSettings.FromConnectionString(AppHelper.DbConnectionString);
                        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                        return new MongoClient(settings);
                    });

                    services.AddSingleton<ITelegramBotClient>(sp =>
                        new TelegramBotClient(Environment.GetEnvironmentVariable("AV_KUFAR_CAR_PARSER_BOT_TOKEN")));

                    services.AddSingleton<KufarProcessor>();
                    services.AddSingleton<LiteDbService>();
                    services.AddHostedService<BotService>();

                    services.AddSingleton<IDbSubscriptionService>(provider => provider.GetRequiredService<LiteDbService>());
                    services.AddSingleton<IDbUpdaterService>(provider => provider.GetRequiredService<LiteDbService>());
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
