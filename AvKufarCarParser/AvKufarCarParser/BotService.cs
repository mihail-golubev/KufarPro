using AvKufarCarParser.Kufar;
using AvKufarCarParser.Models;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AvKufarCarParser
{
    public class BotService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly ITelegramBotClient _botClient;
        private readonly KufarProcessor _kufarProcessor;
        private static readonly HashSet<long> SubscribedUsers = new HashSet<long>();

        public BotService()
        {
            _httpClient = new HttpClient();
            _botClient = new TelegramBotClient(Util.BotToken);
            _kufarProcessor = new KufarProcessor(_httpClient);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: stoppingToken);
                Console.WriteLine("Bot is running...");

                //SubscribedUsers.Add(Util.UserId);
                //SubscribedUsers.Add(769603864);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var newAds = await _kufarProcessor.GetNewAds();
                    await Task.WhenAll(newAds.Select(ad => NotifyUsers(ad)));

                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Some error occured: {ex.Message}");
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message && update.Message!.Text != null)
            {
                long chatId = update.Message.Chat.Id;
                string messageText = update.Message.Text.ToLower();

                if (messageText == "/start")
                {
                    await bot.SendMessage(chatId, "Welcome! Use /subscribe to receive new ads.", cancellationToken: token);
                }
                else if (messageText == "/subscribe")
                {
                    SubscribedUsers.Add(chatId);
                    await bot.SendMessage(chatId, "You have subscribed to new ads notifications!", cancellationToken: token);
                }
                else if (messageText == "/unsubscribe")
                {
                    SubscribedUsers.Remove(chatId);
                    await bot.SendMessage(chatId, "You have unsubscribed from notifications.", cancellationToken: token);
                }
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Bot error: {exception.Message}");
            return Task.CompletedTask;
        }

        private async Task NotifyUsers(Ad ad)
        {
            string message = $"Вышло новое объявление!" +
                $"\n\nНазвание автомобиля: {ad.CarParams.Brand} {ad.CarParams.Model}" +
                $"\nЦена: ${ad.Price}" +
                $"\nОбласть: {ad.CarParams.Region}" +
                $"\nГород: {ad.CarParams.City}" +
                $"\nСсылка на объявление: {ad.Link}" +
                $"\n\nХарактеристики автомобиля:" +
                $"\nДвигатель: {ad.CarParams.FuelType} {ad.CarParams.EngineCapacity}" +
                $"\nПробег: {ad.CarParams.Mileage} км" +
                $"\nГод выпуска: {ad.CarParams.Year}" +
                $"\nКоробка передач: {ad.CarParams.GearboxType}" +
                $"\nТип кузова: {ad.CarParams.BodyType}" +
                $"\nПривод: {ad.CarParams.DriveType}";

            foreach (var userId in SubscribedUsers)
            {
                if (ad.Images.Count > 0)
                {
                    var media = ad.Images.Select((img, index) =>
                        new InputMediaPhoto(img.Link) { Caption = index == 0 ? message : null }).ToArray();

                    await _botClient.SendMediaGroup(userId, media);
                }
                else
                {
                    await _botClient.SendMessage(userId, message);
                }
            }

            Console.WriteLine("Users have been notified.");
        }
    }
}
