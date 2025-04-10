using Microsoft.AspNetCore.Mvc;
using WebApplication5.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;
        private readonly ILogger<SyncController> _logger;

        public SyncController(ISyncService syncService, ILogger<SyncController> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        [HttpPost("syncClients")]
        public async Task<IActionResult> SyncClients()
        {
            try
            {
                var result = await _syncService.SynchronizeClientsAsync();
                _logger.LogInformation(result);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize clients.");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("syncCommercials")]
        public async Task<IActionResult> SyncCommercials()
        {
            try
            {
                var result = await _syncService.SynchronizeCommercialsAsync();
                _logger.LogInformation(result);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize commercials.");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("syncSales")]
        public async Task<IActionResult> SyncSales()
        {
            try
            {
                var result = await _syncService.SynchronizeSalesAsync();
                _logger.LogInformation(result);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize sales.");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
