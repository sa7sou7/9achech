using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication5.Data;
using WebApplication5.Dto;
using WebApplication5.Models;
using WebApplication5.Repository;
using Microsoft.Extensions.Logging; // Add this

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IArticleRepository _articleRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<OrdersController> _logger; // Add logger

        public OrdersController(
            IOrderRepository orderRepository,
            IArticleRepository articleRepository,
            AppDbContext context,
            ILogger<OrdersController> logger) // Inject logger
        {
            _orderRepository = orderRepository;
            _articleRepository = articleRepository;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
        {
            if (orderDto.OrderLines == null || !orderDto.OrderLines.Any())
            {
                _logger.LogWarning("CreateOrder: No order lines provided.");
                return BadRequest("At least one order line is required.");
            }

            var order = new Order
            {
                VisitId = orderDto.VisitId,
                OrderRef = orderDto.OrderRef,
                OrderDate = orderDto.OrderDate,
                OrderLines = new List<OrderLine>()
            };

            // Validate stock and prepare order lines
            decimal totalAmount = 0;
            var stockIssues = new List<string>();
            var articlesToUpdate = new List<Article>();

            foreach (var lineDto in orderDto.OrderLines)
            {
                if (lineDto.Quantity <= 0)
                {
                    stockIssues.Add($"Quantity must be positive for Article ID {lineDto.ArticleId}");
                    continue;
                }

                var article = await _articleRepository.GetByIdAsync(lineDto.ArticleId);
                if (article == null)
                {
                    stockIssues.Add($"Article ID {lineDto.ArticleId} not found");
                    continue;
                }

                if (lineDto.Quantity > article.StockQuantity)
                {
                    stockIssues.Add($"Insufficient stock for Article ID {lineDto.ArticleId}: requested {lineDto.Quantity}, available {article.StockQuantity}");
                    continue;
                }

                var orderLine = new OrderLine
                {
                    ArticleId = lineDto.ArticleId,
                    Quantity = lineDto.Quantity,
                    UnitPrice = article.PrixVente
                };
                order.OrderLines.Add(orderLine);
                totalAmount += lineDto.Quantity * orderLine.UnitPrice;

                // Prepare stock deduction
                article.StockQuantity -= lineDto.Quantity;
                articlesToUpdate.Add(article);
            }

            if (stockIssues.Any())
            {
                _logger.LogWarning("CreateOrder: Stock issues detected: {Errors}", string.Join(", ", stockIssues));
                return BadRequest(new { Errors = stockIssues });
            }

            order.TotalAmount = totalAmount;

            // Save order and update stock in a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating order with VisitId: {VisitId}, OrderRef: {OrderRef}", order.VisitId, order.OrderRef);
                await _orderRepository.CreateAsync(order);
                foreach (var article in articlesToUpdate)
                {
                    _context.Articles.Update(article);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create order with VisitId: {VisitId}. Error: {Message}", order.VisitId, ex.Message);
                return StatusCode(500, $"An error occurred while processing the order: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            return Ok(order);
        }
    }
}