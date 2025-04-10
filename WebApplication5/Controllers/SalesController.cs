using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication5.Data;
using WebApplication5.Dto;
using WebApplication5.Models;
using WebApplication5.Repository;
using WebApplication5.Services;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ISyncService _syncService;
        private readonly ILogger<SalesController> _logger;
        private readonly AppDbContext _context;

        public SalesController(
            ISaleRepository saleRepository,
            ISyncService syncService,
            ILogger<SalesController> logger,
            AppDbContext context)
        {
            _saleRepository = saleRepository;
            _syncService = syncService;
            _logger = logger;
            _context = context;
        }

        // GET: api/Sales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetSales()
        {
            try
            {
                _logger.LogInformation("Fetching all sales");
                var sales = await _saleRepository.GetAllAsync();
                var result = sales.Select(s => new SaleDto
                {
                    DocRef = s.DocRef,
                    DocTiers = s.Client?.Matricule ?? string.Empty,
                    DocRepresentant = s.DocRepresentant,
                    DocNetaPayer = s.DocNetAPayer.ToString("F4"),
                    DocDate = s.DocDate.ToString("yyyy-MM-dd")
                }).ToList();

                _logger.LogInformation($"Retrieved {result.Count} sales");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales");
                return StatusCode(500, new { error = "Failed to retrieve sales: " + ex.Message });
            }
        }

        // GET: api/Sales/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SaleDto>> GetSale(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching sale with ID: {id}");
                var sale = await _saleRepository.GetByIdAsync(id);
                if (sale == null)
                {
                    _logger.LogWarning($"Sale with ID {id} not found");
                    return NotFound(new { error = $"Sale with ID {id} not found" });
                }

                var result = new SaleDto
                {
                    DocRef = sale.DocRef,
                    DocTiers = sale.Client?.Matricule ?? string.Empty,
                    DocRepresentant = sale.DocRepresentant,
                    DocNetaPayer = sale.DocNetAPayer.ToString("F4"),
                    DocDate = sale.DocDate.ToString("yyyy-MM-dd")
                };

                _logger.LogInformation($"Retrieved sale with ID: {id}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving sale with ID: {id}");
                return StatusCode(500, new { error = "Failed to retrieve sale: " + ex.Message });
            }
        }

        // POST: api/Sales
        [HttpPost]
        public async Task<ActionResult<SaleDto>> CreateSale([FromBody] SaleDto saleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid sale data provided");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation($"Creating sale with DocRef: {saleDto.DocRef}");
                var client = await _context.Tiers.FirstOrDefaultAsync(t => t.Matricule == saleDto.DocTiers);
                if (client == null)
                {
                    _logger.LogWarning($"Client with Matricule {saleDto.DocTiers} not found");
                    return BadRequest(new { error = $"Client with Matricule {saleDto.DocTiers} not found" });
                }

                var sale = new Sale
                {
                    DocRef = saleDto.DocRef ?? string.Empty,
                    TiersId = client.Id,
                    DocRepresentant = saleDto.DocRepresentant ?? "N/A",
                    DocNetAPayer = decimal.TryParse(saleDto.DocNetaPayer, out var net) ? net : 0,
                    DocDate = DateTime.TryParse(saleDto.DocDate, out var date) ? date : DateTime.Now
                };

                var createdSale = await _saleRepository.AddAsync(sale);

                // Update DTO with created ID (though not strictly necessary since DTO doesn't have Id)
                _logger.LogInformation($"Created sale with ID: {createdSale.Id}");
                return CreatedAtAction(nameof(GetSale), new { id = createdSale.Id }, saleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating sale with DocRef: {saleDto.DocRef}");
                return StatusCode(500, new { error = "Failed to create sale: " + ex.Message });
            }
        }

        // PUT: api/Sales/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSale(int id, [FromBody] SaleDto saleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid sale data provided for ID: {id}");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation($"Updating sale with ID: {id}");
                var existingSale = await _saleRepository.GetByIdAsync(id);
                if (existingSale == null)
                {
                    _logger.LogWarning($"Sale with ID {id} not found for update");
                    return NotFound(new { error = $"Sale with ID {id} not found" });
                }

                var client = await _context.Tiers.FirstOrDefaultAsync(t => t.Matricule == saleDto.DocTiers);
                if (client == null)
                {
                    _logger.LogWarning($"Client with Matricule {saleDto.DocTiers} not found for sale update");
                    return BadRequest(new { error = $"Client with Matricule {saleDto.DocTiers} not found" });
                }

                existingSale.DocRef = saleDto.DocRef ?? existingSale.DocRef;
                existingSale.TiersId = client.Id;
                existingSale.DocRepresentant = saleDto.DocRepresentant ?? existingSale.DocRepresentant;
                existingSale.DocNetAPayer = decimal.TryParse(saleDto.DocNetaPayer, out var net) ? net : existingSale.DocNetAPayer;
                existingSale.DocDate = DateTime.TryParse(saleDto.DocDate, out var date) ? date : existingSale.DocDate;

                var updated = await _saleRepository.UpdateAsync(existingSale);
                if (!updated)
                {
                    _logger.LogWarning($"Failed to update sale with ID: {id}");
                    return StatusCode(500, new { error = "Failed to update sale" });
                }

                _logger.LogInformation($"Updated sale with ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating sale with ID: {id}");
                return StatusCode(500, new { error = "Failed to update sale: " + ex.Message });
            }
        }

        // DELETE: api/Sales/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting sale with ID: {id}");
                var deleted = await _saleRepository.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning($"Sale with ID {id} not found for deletion");
                    return NotFound(new { error = $"Sale with ID {id} not found" });
                }

                _logger.LogInformation($"Deleted sale with ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting sale with ID: {id}");
                return StatusCode(500, new { error = "Failed to delete sale: " + ex.Message });
            }
        }

        // POST: api/Sales/sync
        [HttpPost("sync")]
        public async Task<IActionResult> SyncSales()
        {
            try
            {
                _logger.LogInformation("Starting sales synchronization");
                var result = await _syncService.SynchronizeSalesAsync();
                _logger.LogInformation(result);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize sales");
                return StatusCode(500, new { error = "Failed to synchronize sales: " + ex.Message });
            }
        }
    }
}