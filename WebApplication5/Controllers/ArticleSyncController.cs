using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication5.Services;
using System.Threading.Tasks;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleSyncController : ControllerBase
    {
        private readonly IArticleSyncService _syncService;
        private readonly ILogger<ArticleSyncController> _logger;

        public ArticleSyncController(IArticleSyncService syncService, ILogger<ArticleSyncController> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        [HttpPost("syncArticles")]
        public async Task<IActionResult> SyncArticles()
        {
            try
            {
                var result = await _syncService.SynchronizeArticlesAsync();
                _logger.LogInformation(result ? "Synchronization successful" : "Synchronization failed");
                return Ok(new { message = result ? "Synchronization successful" : "Synchronization failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize articles.");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
