using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication5.Dto;
using WebApplication5.Models;
using WebApplication5.Repository;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VisitsController : ControllerBase
    {
        private readonly IVisitRepository _visitRepository;
        private readonly IArticleRepository _articleRepository;
        private readonly ILogger<VisitsController> _logger;

        public VisitsController(IVisitRepository visitRepository, IArticleRepository articleRepository, ILogger<VisitsController> logger)
        {
            _visitRepository = visitRepository;
            _articleRepository = articleRepository;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> CreateVisit([FromBody] VisitDto visitDto)
        {
            var visit = new Visit
            {
                CommercialId = visitDto.CommercialId,
                TiersId = visitDto.TiersId,
                ScheduledDate = DateTime.Parse(visitDto.ScheduledDate),
                Objectives = visitDto.Objectives.Select(o => new VisitChecklist
                {
                    ObjectiveType = Enum.Parse<VisitObjectiveType>(o.ObjectiveType),
                    IsSelected = o.IsSelected,
                    Encours = o.Encours,
                    ReglementPret = o.ReglementPret,
                    Recupere = o.Recupere,
                    OrderItems = o.OrderItems?.Select(oi => new VisitOrderItem
                    {
                        ArticleId = oi.ArticleId,
                        UnitPriceHT = oi.UnitPriceHT,
                        Discount = oi.Discount,
                        Quantity = oi.Quantity,
                        AvailableQuantity = oi.AvailableQuantity
                    }).ToList() ?? new List<VisitOrderItem>()
                }).ToList()
            };
            var createdVisit = await _visitRepository.CreateVisitAsync(visit);
            return CreatedAtAction(nameof(GetVisit), new { id = createdVisit.Id }, visitDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVisit(int id)
        {
            var visit = await _visitRepository.GetVisitByIdAsync(id);
            if (visit == null) return NotFound();
            var visitDto = new VisitDto
            {
                CommercialId = visit.CommercialId,
                TiersId = visit.TiersId,
                ScheduledDate = visit.ScheduledDate.ToString("yyyy-MM-dd"),
                CompletedDate = visit.CompletedDate?.ToString("yyyy-MM-dd"),
                Report = visit.Report,
                IsValidated = visit.IsValidated,
                Objectives = visit.Objectives.Select(o => new VisitChecklistDto
                {
                    ObjectiveType = o.ObjectiveType.ToString(),
                    IsSelected = o.IsSelected,
                    Encours = o.Encours,
                    ReglementPret = o.ReglementPret,
                    Recupere = o.Recupere,
                    OrderItems = o.OrderItems?.Select(oi => new VisitOrderItemDto
                    {
                        ArticleId = oi.ArticleId,
                        ArticleCode = oi.Article?.Code ?? string.Empty,
                        ArticleDesignation = oi.Article?.Designation ?? string.Empty,
                        UnitPriceHT = oi.UnitPriceHT,
                        Discount = oi.Discount,
                        Quantity = oi.Quantity,
                        AvailableQuantity = oi.AvailableQuantity
                    }).ToList() ?? new List<VisitOrderItemDto>()
                }).ToList()
            };
            return Ok(visitDto);
        }

        [HttpGet("commercial/{commercialId}")]
        public async Task<IActionResult> GetVisitsByCommercial(string commercialId)
        {
            var visits = await _visitRepository.GetVisitsByCommercialAsync(commercialId);
            var visitDtos = visits.Select(v => new VisitDto
            {
                CommercialId = v.CommercialId,
                TiersId = v.TiersId,
                ScheduledDate = v.ScheduledDate.ToString("yyyy-MM-dd"),
                CompletedDate = v.CompletedDate?.ToString("yyyy-MM-dd"),
                Report = v.Report,
                IsValidated = v.IsValidated,
                Objectives = v.Objectives.Select(o => new VisitChecklistDto
                {
                    ObjectiveType = o.ObjectiveType.ToString(),
                    IsSelected = o.IsSelected,
                    Encours = o.Encours,
                    ReglementPret = o.ReglementPret,
                    Recupere = o.Recupere,
                    OrderItems = o.OrderItems?.Select(oi => new VisitOrderItemDto
                    {
                        ArticleId = oi.ArticleId,
                        ArticleCode = oi.Article?.Code ?? string.Empty,
                        ArticleDesignation = oi.Article?.Designation ?? string.Empty,
                        UnitPriceHT = oi.UnitPriceHT,
                        Discount = oi.Discount,
                        Quantity = oi.Quantity,
                        AvailableQuantity = oi.AvailableQuantity
                    }).ToList() ?? new List<VisitOrderItemDto>()
                }).ToList()
            });
            return Ok(visitDtos);
        }

        [HttpPut("{id}/validate")]
        [Authorize(Policy = "CommercialOnly")]
        public async Task<IActionResult> ValidateVisit(int id, [FromBody] VisitReportDto reportDto)
        {
            var visit = await _visitRepository.GetVisitByIdAsync(id);
            if (visit == null) return NotFound();
            visit.CompletedDate = DateTime.Now;
            visit.Report = reportDto.Report;
            visit.IsValidated = true;
            var updated = await _visitRepository.UpdateVisitAsync(visit);
            return updated ? NoContent() : StatusCode(500);
        }

        [HttpPut("{id}/checklist")]
        [Authorize(Policy = "CommercialOnly")]
        public async Task<IActionResult> UpdateChecklist(int id, [FromBody] List<VisitChecklistDto> checklistDtos)
        {
            var visit = await _visitRepository.GetVisitByIdAsync(id);
            if (visit == null) return NotFound();

            visit.Objectives = checklistDtos.Select(dto => new VisitChecklist
            {
                VisitId = id,
                ObjectiveType = Enum.Parse<VisitObjectiveType>(dto.ObjectiveType),
                IsSelected = dto.IsSelected,
                Encours = dto.Encours,
                ReglementPret = dto.ReglementPret,
                Recupere = dto.Recupere,
                OrderItems = dto.OrderItems?.Select(oi => new VisitOrderItem
                {
                    ArticleId = oi.ArticleId,
                    UnitPriceHT = oi.UnitPriceHT,
                    Discount = oi.Discount,
                    Quantity = oi.Quantity,
                    AvailableQuantity = oi.AvailableQuantity
                }).ToList() ?? new List<VisitOrderItem>()
            }).ToList();

            var updated = await _visitRepository.UpdateVisitAsync(visit);
            return updated ? NoContent() : StatusCode(500);
        }

        [HttpPost("{id}/order-items")]
        [Authorize(Policy = "CommercialOnly")]
        public async Task<IActionResult> AddOrderItem(int id, [FromBody] VisitOrderItemDto orderItemDto)
        {
            var visit = await _visitRepository.GetVisitByIdAsync(id);
            if (visit == null) return NotFound();

            var passerCommandeObjective = visit.Objectives
                .FirstOrDefault(o => o.ObjectiveType == VisitObjectiveType.PasserCommande && o.IsSelected);
            if (passerCommandeObjective == null) return BadRequest("PasserCommande objective is not selected for this visit.");

            var article = await _articleRepository.GetArticleByIdAsync(orderItemDto.ArticleId);
            if (article == null) return BadRequest("Article not found.");

            if (orderItemDto.Quantity > article.StockQuantity)
                return BadRequest("Requested quantity exceeds available stock.");

            var orderItem = new VisitOrderItem
            {
                VisitChecklistId = passerCommandeObjective.Id,
                ArticleId = orderItemDto.ArticleId,
                UnitPriceHT = orderItemDto.UnitPriceHT,
                Discount = orderItemDto.Discount,
                Quantity = orderItemDto.Quantity,
                AvailableQuantity = article.StockQuantity
            };

            passerCommandeObjective.OrderItems.Add(orderItem);
            var updated = await _visitRepository.UpdateVisitAsync(visit);
            return updated ? NoContent() : StatusCode(500);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeleteVisit(int id)
        {
            var deleted = await _visitRepository.DeleteVisitAsync(id);
            return deleted ? NoContent() : NotFound();
        }

    }
}