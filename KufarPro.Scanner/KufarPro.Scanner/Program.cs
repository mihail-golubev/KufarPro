using KufarPro.Scanner.HttpClients;
using KufarPro.Scanner.HttpClients.Interfaces;
using KufarPro.Scanner.Processors;
using KufarPro.Scanner.Services;
using KufarPro.Shared;
using KufarPro.Shared.Messaging;
using KufarPro.Shared.Messaging.Interfaces;
using KufarPro.Shared.Models.Settings;
using System.Net.Http.Headers;

namespace KufarPro.Scanner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var httpClientsSettings = new HttpClientsSettings();
            builder.Configuration.GetSection(Constants.HttpClientsSettingsSectionName).Bind(httpClientsSettings);
            builder.Services.Configure<MessageQueueSettings>(builder.Configuration.GetSection(Constants.MessageQueueSettingsSectionName));

            builder.Services.AddHttpClient(Constants.SearchFiltersApiClientName, client =>
            {
                client.BaseAddress = new Uri(httpClientsSettings.SearchFiltersApiBaseUrl);
                client.DefaultRequestHeaders.Add("X-API-Key", Environment.GetEnvironmentVariable(httpClientsSettings.SearchFiltersApiKeyEnvVariableName));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            builder.Services.AddHttpClient(Constants.KufarApiClientName, client =>
            {
                client.BaseAddress = new Uri(httpClientsSettings.KufarApiBaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            builder.Services.AddSingleton<IUpdateSearchFilterApiClient, SearchFiltersApiClient>();
            builder.Services.AddSingleton<IGetSearchFiltersApiClient, SearchFiltersApiClient>();
            builder.Services.AddSingleton<IKufarApiClient, KufarClient>();
            builder.Services.AddSingleton<IMessageQueueService, MessageQueueService>();
            builder.Services.AddSingleton<KufarProcessor>();
            builder.Services.AddHostedService<ScannerService>();

            var host = builder.Build();
            host.Run();
        }
    }
}