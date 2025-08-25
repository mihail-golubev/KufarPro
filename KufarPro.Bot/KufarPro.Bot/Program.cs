using KufarPro.Bot.Messaging;
using KufarPro.Shared;
using KufarPro.Shared.Logging;
using KufarPro.Shared.Messaging.Interfaces;
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
            var builder = Host.CreateApplicationBuilder(args);

            builder.Configuration
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            builder.Services.Configure<BotSettings>(builder.Configuration.GetSection(Constants.BotSettingsSectionName));
            builder.Services.Configure<MessageQueueSettings>(builder.Configuration.GetSection(Constants.MessageQueueSettingsSectionName));

            builder.Services.AddLogging(configure => configure.AddSimpleConsole());
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<ITelegramBotClient>(sp =>
            {
                var botOpts = sp.GetRequiredService<IOptions<BotSettings>>().Value;
                var token = Environment.GetEnvironmentVariable(botOpts.BotTokenEnvVariableName);
                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException($"Env variable '{botOpts.BotTokenEnvVariableName}' is not set.");
                }

                return new TelegramBotClient(token);
            });

            builder.Services.AddHostedService<BotService>();
            builder.Services.AddSingleton<IMessageQueueService, BotMessageQueueService>();

            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "[dd/MM/yyyy HH:mm:ss] ";
                });

                var logSettings = builder.Configuration.GetSection(Constants.LogSettingsSectionName).Get<LogSettings>();
                logging.AddProvider(new FileLoggerProvider(Path.Combine(AppContext.BaseDirectory, logSettings.LogFileName), logSettings.MaxLogFileSize));
            });

            var app = builder.Build();

            await app.RunAsync();
        }
    }
}
