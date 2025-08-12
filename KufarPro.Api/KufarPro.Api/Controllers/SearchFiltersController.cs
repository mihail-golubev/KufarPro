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
        private readonly ILogger<SearchFiltersController> _logger;

        /// <summary>
        /// Controller for managing search filters.
        /// </summary>
        /// <param name="subscriptionService">Service for telegram app to manage subscription filter.</param>
        /// <param name="updaterService">Service for KufarPro.Scanner to update filter in database when new ad published.</param>
        public SearchFiltersController(IDbSubscriptionService subscriptionService, IDbUpdaterService updaterService, ILogger<SearchFiltersController> logger)
        {
            _subscriptionService = subscriptionService;
            _updaterService = updaterService;
            _logger = logger;
        }

        [HttpGet]
        [ApiKeyAuthorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var filters = await _updaterService.GetAll();
                return Ok(filters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all search filters.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("my")]
        [TelegramAuthorize]
        public async Task<IActionResult> GetMyFilters()
        {
            try
            {
                var chatId = GetChatIdFromContext();
                var filters = await _subscriptionService.GetAll(chatId);
                return Ok(filters);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to GetMyFilters.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user filters.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("subscribe")]
        [TelegramAuthorize]
        public async Task<IActionResult> AddOrUpdateSubscription([FromQuery] string urlQuery)
        {
            try
            {
                var chatId = GetChatIdFromContext();
                var result = await _subscriptionService.AddOrUpdate(chatId, urlQuery);

                return result is not null ? Ok(result) : Conflict("Already subscribed.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to AddOrUpdateSubscription.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add or update subscription.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("unsubscribe")]
        [TelegramAuthorize]
        public async Task<IActionResult> RemoveSubscription([FromQuery] string urlQuery)
        {
            try
            {
                var chatId = GetChatIdFromContext();
                bool result = await _subscriptionService.Delete(chatId, urlQuery);

                return result ? NoContent() : NotFound("Subscription not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to RemoveSubscription.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove subscription.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPatch("update")]
        [ApiKeyAuthorize]
        public async Task<IActionResult> UpdateFilter([FromBody] SearchFilter filter)
        {
            try
            {
                await _updaterService.Update(filter);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update filter.");
                return StatusCode(500, "Internal server error.");
            }
        }

        private long GetChatIdFromContext()
        {
            return HttpContext.Items["ChatId"] is long id ? id : throw new UnauthorizedAccessException("Missing ChatId.");
        }
    }
}