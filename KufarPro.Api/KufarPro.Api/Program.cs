using KufarPro.Api.Services;
using KufarPro.Api.Services.Interfaces;
using KufarPro.Shared;
using KufarPro.Shared.Models.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace KufarPro.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(Constants.MongoDbSettingsSectionName));

            builder.Services.AddSingleton<IMongoDatabase>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                var client = new MongoClient(settings.ConnectionString);
                return client.GetDatabase(settings.DatabaseName);
            });

            builder.Services.AddSingleton<IDbUpdaterService, SearchFilterService>();
            builder.Services.AddSingleton<IDbSubscriptionService, SearchFilterService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapControllers();

            app.Run();
        }
    }
}
