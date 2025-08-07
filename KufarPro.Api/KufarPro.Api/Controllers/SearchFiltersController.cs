using KufarPro.Api.Auth;
using KufarPro.Api.Services.Interfaces;
using KufarPro.Shared.Models.Search;
using Microsoft.AspNetCore.Mvc;

namespace KufarPro.Api.Controllers
{
    [ApiController]
    [Route("api/search-filters")]
    public class SearchFiltersController : ControllerBase
    {
        private readonly IDbSubscriptionService _subscriptionService;
        private readonly IDbUpdaterService _updaterService;

        /// <summary>
        /// Controller for managing search filters.
        /// </summary>
        /// <param name="subscriptionService">Service for telegram app to manage subscription filter.</param>
        /// <param name="updaterService">Service for KufarPro.Scanner to update filter in database when new ad published.</param>
        public SearchFiltersController(IDbSubscriptionService subscriptionService, IDbUpdaterService updaterService)
        {
            _subscriptionService = subscriptionService;
            _updaterService = updaterService;
        }

        [HttpGet]
        [ApiKeyAuthorize]
        public async Task<IActionResult> GetAll()
        {
            var filters = await _updaterService.GetAll();

            return Ok(filters);
        }

        [HttpGet("my")]
        [TelegramAuthorize]
        public async Task<IActionResult> GetMyFilters()
        {
            var chatId = GetChatIdFromContext();
            var filters = await _subscriptionService.GetAll(chatId);

            return Ok(filters);
        }

        [HttpPost("subscribe")]
        [TelegramAuthorize]
        public async Task<IActionResult> AddOrUpdateSubscription([FromQuery] string urlQuery)
        {
            var chatId = GetChatIdFromContext();
            var result = await _subscriptionService.AddOrUpdate(chatId, urlQuery);

            return result is not null ? Ok(result) : Conflict("Already subscribed.");
        }

        [HttpDelete("unsubscribe")]
        [TelegramAuthorize]
        public async Task<IActionResult> RemoveSubscription([FromQuery] string urlQuery)
        {
            var chatId = GetChatIdFromContext();
            bool result = await _subscriptionService.Delete(chatId, urlQuery);

            return result ? NoContent() : NotFound("Subscription not found.");
        }

        [HttpPatch("update")]
        [ApiKeyAuthorize]
        public async Task<IActionResult> UpdateFilter([FromBody] SearchFilter filter)
        {
            await _updaterService.Update(filter);
            return NoContent();
        }

        private long GetChatIdFromContext()
        {
            return HttpContext.Items["ChatId"] is long id ? id : throw new UnauthorizedAccessException("Missing ChatId.");
        }
    }
}