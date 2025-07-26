using KufarPro.DataAccess;
using KufarPro.Helpers;
using KufarPro.Models.Database;
using KufarPro.Models.Kufar.HelperModels;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace KufarPro.Handlers
{
    public class FilterMessageHandler
    {
        private readonly Dictionary<long, UserFilterState> _userStates;
        private readonly IDbSubscriptionService _dbService;
        private readonly List<SearchFilter> _searchFilters;
        private readonly ITelegramBotClient _bot;
        private readonly ILogger _logger;

        public FilterMessageHandler(
            Dictionary<long, UserFilterState> userStates,
            IDbSubscriptionService dbService,
            List<SearchFilter> searchFilters,
            ITelegramBotClient bot,
            ILogger logger)
        {
            _userStates = userStates;
            _dbService = dbService;
            _searchFilters = searchFilters;
            _bot = bot;
            _logger = logger;
        }

        public async Task HandleMessageAsync(Message message, CancellationToken token)
        {
            long chatId = message.Chat.Id;
            string messageText = message.Text.ToLower();

            await HandlePriceInput(messageText, chatId, token);

            await HandleSubscribeUnsubscribeAction(messageText, chatId, token);
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken token)
        {
            long chatId = callbackQuery.Message.Chat.Id;
            var data = callbackQuery.Data;

            if (data.StartsWith("category:"))
            {
                var category = data.Split(':')[1];

                if (!_userStates.ContainsKey(chatId))
                {
                    _userStates[chatId] = new UserFilterState();
                }

                _userStates[chatId].Category = category;

                _userStates[chatId].AdType = category switch
                {
                    "2010" => AdType.Auto,
                    "4050" => AdType.Bicycle,
                    "1020" => AdType.RealEstate,
                    _ => AdType.Unknown
                };

                await SendRegionSelectionMenu(chatId, token);
                await _bot.AnswerCallbackQuery(callbackQuery.Id, "Категория выбрана", cancellationToken: token);
            }
            else if (data.StartsWith("region:"))
            {
                var region = data.Split(':')[1];

                if (!_userStates.ContainsKey(chatId))
                {
                    _userStates[chatId] = new UserFilterState();
                }

                _userStates[chatId].Region = region;

                await _bot.SendMessage(chatId, "Введите диапазон цены в формате: от-до (например: 100-1500), или отправьте 0, если не важно. Цена указывается в долларах $.", cancellationToken: token);
                await _bot.AnswerCallbackQuery(callbackQuery.Id, "Область выбрана, теперь введите цену", cancellationToken: token);
            }
        }

        private async Task SendCategorySelectionMenu(long chatId, CancellationToken token)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🚗 Авто", "category:2010"),
                    //InlineKeyboardButton.WithCallbackData("🚲 Велосипеды", "category:4050"),
                    //InlineKeyboardButton.WithCallbackData("🏠 Недвижимость", "category:1020")
                }
            });

            await _bot.SendMessage(chatId, "Выберите категорию:", replyMarkup: keyboard, cancellationToken: token);
        }

        private async Task SendRegionSelectionMenu(long chatId, CancellationToken token)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Минск", "region:7"),
                    InlineKeyboardButton.WithCallbackData("Минская", "region:5"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Брестская", "region:1"),
                    InlineKeyboardButton.WithCallbackData("Гомельская", "region:2")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Гродненская", "region:3"),
                    InlineKeyboardButton.WithCallbackData("Могилевская", "region:4"),
                    InlineKeyboardButton.WithCallbackData("Витебская", "region:6")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Все области", "region:0"),
                },
            });

            await _bot.SendMessage(chatId, "Выберите область:", replyMarkup: keyboard, cancellationToken: token);
        }

        private async Task HandleSubscribeUnsubscribeAction(string messageText, long chatId, CancellationToken token)
        {
            if (messageText.StartsWith("/subscribe"))
            {
                await SendCategorySelectionMenu(chatId, token);
            }
            else if (messageText.StartsWith("/unsubscribe"))
            {
                var parameters = AppHelper.ParseFilterParameters(messageText);
                var result = await _dbService.RemoveSubscriptionAsync(chatId, parameters);

                if (result != null)
                {
                    await _bot.SendMessage(chatId, "Вы отписались от уведомлений о новых объявлениях!", cancellationToken: token);
                    _searchFilters.Remove(result);
                    _logger.LogInformation($"User {chatId} has unsubscribed.");
                }
                else
                {
                    await _bot.SendMessage(chatId, "Что-то пошло не так. Возможно, вы не подписаны на уведомления по этому фильтру.", cancellationToken: token);
                }
            }
            //else
            //{
            //    await _bot.SendMessage(chatId, "Я не понимаю эту команду. Список доступных команд:" +
            //        "\n1) /subscribe - подписаться на уведомления о новых объявлениях" +
            //        "\n2) /unsubscribe - отписаться от уведомлений о новых объявлениях", cancellationToken: token);
            //}
        }

        private async Task HandlePriceInput(string messageText, long chatId, CancellationToken token)
        {
            if (!_userStates.TryGetValue(chatId, out var state))
            {
                return;
            }

            if (string.IsNullOrEmpty(state.Category) || string.IsNullOrEmpty(state.Region))
            {
                return;
            }

            if (state.PriceRange.From != null || state.PriceRange.To != null)
            {
                return;
            }

            var priceRange = AppHelper.ParsePriceRange(messageText);
            state.PriceRange.From = priceRange.From;
            state.PriceRange.To = priceRange.To;

            await FinalizeSubscription(chatId, state, token);
        }

        private async Task FinalizeSubscription(long chatId, UserFilterState state, CancellationToken token)
        {
            string query = new FilterBuilder()
                .SetCategory(state.Category)
                .SetRegion(state.Region)
                .SetPriceRange(state.PriceRange)
                .Build();

            var result = await _dbService.AddOrUpdateSubscriptionAsync(chatId, query);

            if (result != null)
            {
                await _bot.SendMessage(chatId, "Вы успешно подписаны на уведомления с выбранными параметрами!", cancellationToken: token);
                _searchFilters.AddOrUpdate(result);
                _logger.LogInformation($"User {chatId} has subscribed with filter: {query}");

                _userStates.Remove(chatId);
            }
            else
            {
                await _bot.SendMessage(chatId, "Что-то пошло не так при подписке. Возможно, вы уже подписаны.", cancellationToken: token);
            }
        }
    }
}
