using KufarPro.Shared.Logging;
using KufarPro.Shared.Models.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace KufarPro.Bot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<BotSettings>(context.Configuration.GetSection("BotSettings"));
                    services.Configure<DatabaseSettings>(context.Configuration.GetSection("DatabaseSettings"));
                    services.Configure<LogSettings>(context.Configuration.GetSection("LogSettings"));

                    services.AddLogging(configure => configure.AddSimpleConsole());
                    services.AddSingleton<HttpClient>();
                    services.AddSingleton<ITelegramBotClient>(sp =>
                    {
                        var botOpts = sp.GetRequiredService<IOptions<BotSettings>>().Value;
                        var token = Environment.GetEnvironmentVariable(botOpts.BotTokenEnvVariableName);
                        if (string.IsNullOrEmpty(token))
                        {
                            throw new InvalidOperationException($"Env variable '{botOpts.BotTokenEnvVariableName}' is not set.");
                        }

                        return new TelegramBotClient(token);
                    });

                    services.AddHostedService<BotService>();

                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddSimpleConsole(options =>
                        {
                            options.TimestampFormat = "[dd/MM/yyyy HH:mm:ss] ";
                        });

                        var logSettings = context.Configuration.GetSection("LogSettings").Get<LogSettings>();
                        logging.AddProvider(new FileLoggerProvider(Path.Combine(AppContext.BaseDirectory, logSettings.LogFileName), logSettings.MaxLogFileSize));
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}
