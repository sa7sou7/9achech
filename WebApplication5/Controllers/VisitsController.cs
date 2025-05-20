using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Dto;
using WebApplication5.Dtos;
using WebApplication5.Models;
using WebApplication5.Repositories;
using WebApplication5.Repository;
using WebApplication5.Services;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitsController : ControllerBase
    {
        private readonly IVisitRepository _visitRepository;
        private readonly IChecklistRapportRepository _checklistRapportRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRecoveryRepository _recoveryRepository;
        private readonly ICompetitorProductRepository _competitorProductRepository;
        private readonly NotificationService _notificationService;
        private readonly UserMappingService _userMappingService;

        public VisitsController(
            IVisitRepository visitRepository,
            IChecklistRapportRepository checklistRapportRepository,
            IOrderRepository orderRepository,
            IRecoveryRepository recoveryRepository,
            ICompetitorProductRepository competitorProductRepository,
            NotificationService notificationService,
            UserMappingService userMappingService)
        {
            _visitRepository = visitRepository;
            _checklistRapportRepository = checklistRapportRepository;
            _orderRepository = orderRepository;
            _recoveryRepository = recoveryRepository;
            _competitorProductRepository = competitorProductRepository;
            _notificationService = notificationService;
            _userMappingService = userMappingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VisitDto>>> GetVisits([FromQuery] string? commercialCref)
        {
            IEnumerable<VisitDto> visitDtos;
            if (!string.IsNullOrEmpty(commercialCref))
            {
                visitDtos = await _visitRepository.GetAllDtosByCommercialCrefAsync(commercialCref);
            }
            else
            {
                visitDtos = await _visitRepository.GetAllDtosAsync();
            }
            return Ok(visitDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VisitDto>> GetVisit(int id)
        {
            var visitDto = await _visitRepository.GetDtoByIdAsync(id);
            if (visitDto == null)
            {
                return NotFound();
            }
            return Ok(visitDto);
        }

        [HttpPost]
        public async Task<ActionResult<VisitDto>> PostVisit([FromBody] VisitCreateWithChecklistDto visitCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(visitCreateDto.CommercialCref) || visitCreateDto.CommercialCref == "N/A")
            {
                return BadRequest("CommercialCref cannot be null or 'N/A'.");
            }

            if (!await _visitRepository.CommercialExistsAsync(visitCreateDto.CommercialCref))
            {
                return BadRequest("Invalid Commercial Cref.");
            }
            if (!await _visitRepository.TiersExistsAsync(visitCreateDto.TiersId))
            {
                return BadRequest("Invalid Tiers ID.");
            }

            if (visitCreateDto.Checklists == null || !visitCreateDto.Checklists.Any())
            {
                return BadRequest("At least one checklist is required.");
            }

            foreach (var checklist in visitCreateDto.Checklists)
            {
                if (!Enum.IsDefined(typeof(ChecklistLibelle), checklist.Libelle))
                {
                    return BadRequest($"Invalid Checklist Libelle value: {checklist.Libelle}.");
                }
                if (checklist.Libelle == ChecklistLibelle.Recouvrement &&
                    (!checklist.ExpectedRecoveryAmount.HasValue || checklist.ExpectedRecoveryAmount <= 0))
                {
                    return BadRequest("ExpectedRecoveryAmount is required and must be greater than 0 for Recouvrement.");
                }
            }

            var visit = new Visit
            {
                Date = visitCreateDto.Date,
                TiersId = visitCreateDto.TiersId,
                Note = visitCreateDto.Note,
                CommercialCref = visitCreateDto.CommercialCref,
                Status = VisitStatus.Incompleted
            };

            await _visitRepository.AddAsync(visit);

            foreach (var checklistDto in visitCreateDto.Checklists)
            {
                var checklistRapport = new ChecklistRapport
                {
                    VisitId = visit.Id,
                    Libelle = checklistDto.Libelle,
                    Commentaire = checklistDto.Commentaire ?? string.Empty,
                    IsCompleted = false,
                    ExpectedRecoveryAmount = checklistDto.Libelle == ChecklistLibelle.Recouvrement
                        ? checklistDto.ExpectedRecoveryAmount : null,
                    RemainingRecoveryAmount = checklistDto.Libelle == ChecklistLibelle.Recouvrement
                        ? checklistDto.ExpectedRecoveryAmount : null
                };
                await _checklistRapportRepository.AddAsync(checklistRapport);
            }

            try
            {
                var clientName = await _visitRepository.GetClientNameByTiersIdAsync(visit.TiersId);
                var message = $"A new visit has been scheduled for you on {visit.Date:yyyy-MM-dd HH:mm} with client {clientName}.";
                var userId = await _userMappingService.GetUserIdFromCrefAsync(visit.CommercialCref);
                await _notificationService.SendNotificationAsync(userId, message, visit.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send notification for visit {visit.Id}: {ex.Message}");
            }

            var visitDto = await _visitRepository.GetDtoByIdAsync(visit.Id);
            return CreatedAtAction(nameof(GetVisit), new { id = visit.Id }, visitDto);
        }

        [HttpGet("{id}/Checklist")]
        public async Task<ActionResult<IEnumerable<ChecklistRapportDto>>> GetChecklistRapport(int id)
        {
            if (!await _visitRepository.VisitExistsAsync(id))
            {
                return NotFound("Visit not found.");
            }
            var checklistDtos = await _checklistRapportRepository.GetDtosByVisitAsync(id);
            return Ok(checklistDtos);
        }

        [HttpPost("{id}/Checklist")]
        public async Task<ActionResult<ChecklistRapportDto>> PostChecklistRapport(int id, [FromBody] ChecklistRapportCreateDto checklistCreateDto)
        {
            if (!await _visitRepository.VisitExistsAsync(id))
            {
                return NotFound("Visit not found.");
            }
            if (!Enum.IsDefined(typeof(ChecklistLibelle), checklistCreateDto.Libelle))
            {
                return BadRequest("Invalid Libelle value.");
            }
            if (checklistCreateDto.Libelle == ChecklistLibelle.Recouvrement && (!checklistCreateDto.ExpectedRecoveryAmount.HasValue || checklistCreateDto.ExpectedRecoveryAmount <= 0))
            {
                return BadRequest("ExpectedRecoveryAmount is required and must be greater than 0 for Recouvrement.");
            }

            var checklistRapport = new ChecklistRapport
            {
                VisitId = id,
                Libelle = checklistCreateDto.Libelle,
                Commentaire = checklistCreateDto.Commentaire,
                IsCompleted = false,
                ExpectedRecoveryAmount = checklistCreateDto.Libelle == ChecklistLibelle.Recouvrement ? checklistCreateDto.ExpectedRecoveryAmount : null,
                RemainingRecoveryAmount = checklistCreateDto.Libelle == ChecklistLibelle.Recouvrement ? checklistCreateDto.ExpectedRecoveryAmount : null
            };

            await _checklistRapportRepository.AddAsync(checklistRapport);

            var checklistDto = await _checklistRapportRepository.GetDtoByIdAsync(checklistRapport.Id);
            return CreatedAtAction(nameof(GetChecklistRapport), new { id = checklistRapport.VisitId }, checklistDto);
        }

        [HttpPut("Checklist/{checklistId}/Complete")]
        public async Task<IActionResult> UpdateChecklistCompletion(int checklistId, [FromBody] bool isCompleted)
        {
            if (!await _checklistRapportRepository.ChecklistRapportExistsAsync(checklistId))
            {
                return NotFound("Checklist item not found.");
            }

            await _checklistRapportRepository.UpdateChecklistCompletionAsync(checklistId, isCompleted);

            var checklist = await _checklistRapportRepository.GetByIdAsync(checklistId);
            if (await _visitRepository.AreAllChecklistItemsCompletedAsync(checklist.VisitId))
            {
                await _visitRepository.UpdateVisitStatusAsync(checklist.VisitId, VisitStatus.Completed);
            }
            else if (!isCompleted)
            {
                await _visitRepository.UpdateVisitStatusAsync(checklist.VisitId, VisitStatus.Incompleted);
            }

            return NoContent();
        }

        [HttpPut("Checklist/{checklistId}/UpdateRecovery")]
        public async Task<IActionResult> UpdateRecoveryAmount(int checklistId, [FromBody] decimal collectedAmount)
        {
            if (collectedAmount < 0)
            {
                return BadRequest("Collected amount cannot be negative.");
            }

            var checklist = await _checklistRapportRepository.GetByIdAsync(checklistId);
            if (checklist == null || checklist.Libelle != ChecklistLibelle.Recouvrement)
            {
                return NotFound("Recouvrement checklist item not found.");
            }

            if (!checklist.ExpectedRecoveryAmount.HasValue)
            {
                return BadRequest("Expected recovery amount is not set.");
            }

            checklist.RemainingRecoveryAmount ??= checklist.ExpectedRecoveryAmount.Value;
            checklist.RemainingRecoveryAmount = checklist.RemainingRecoveryAmount.Value - collectedAmount;

            if (checklist.RemainingRecoveryAmount < 0)
            {
                return BadRequest("Collected amount exceeds expected recovery amount.");
            }

            checklist.IsCompleted = checklist.RemainingRecoveryAmount <= 0;
            await _checklistRapportRepository.UpdateAsync(checklist);

            var recovery = await _recoveryRepository.GetByVisitIdAsync(checklist.VisitId) ?? new Recovery
            {
                VisitId = checklist.VisitId,
                CollectionDate = DateTime.UtcNow
            };

            recovery.AmountCollected += collectedAmount;
            recovery.Notes = $"Collected {collectedAmount} on {DateTime.UtcNow}. Total collected: {recovery.AmountCollected}. Remaining: {checklist.RemainingRecoveryAmount}";

            if (recovery.Id == 0)
            {
                await _recoveryRepository.CreateAsync(recovery);
            }
            else
            {
                await _recoveryRepository.UpdateAsync(recovery);
            }

            var visitId = checklist.VisitId;
            if (await _visitRepository.AreAllChecklistItemsCompletedAsync(visitId))
            {
                await _visitRepository.UpdateVisitStatusAsync(visitId, VisitStatus.Completed);
            }
            else
            {
                await _visitRepository.UpdateVisitStatusAsync(visitId, VisitStatus.Incompleted);
            }

            return NoContent();
        }

        [HttpGet("Recovery/Report")]
        public async Task<ActionResult<IEnumerable<RecoveryReportDto>>> GetRecoveryReport([FromQuery] string? commercialCref)
        {
            if (string.IsNullOrEmpty(commercialCref))
            {
                return BadRequest("CommercialCref is required.");
            }

            var checklists = await _checklistRapportRepository.GetByCommercialAsync(commercialCref);
            var recoveries = await _recoveryRepository.GetByCommercialAsync(commercialCref);

            var report = checklists
                .Where(c => c.Libelle == ChecklistLibelle.Recouvrement && c.ExpectedRecoveryAmount.HasValue)
                .GroupJoin(recoveries,
                    checklist => checklist.VisitId,
                    recovery => recovery.VisitId,
                    (checklist, recoveryGroup) =>
                    {
                        var totalCollected = recoveryGroup.Sum(r => r.AmountCollected);
                        var remaining = checklist.RemainingRecoveryAmount ?? (checklist.ExpectedRecoveryAmount.Value - totalCollected);
                        return new RecoveryReportDto
                        {
                            VisitId = checklist.VisitId,
                            ChecklistId = checklist.Id,
                            ClientNom = checklist.Visit.Client?.Nom ?? "Unknown",
                            CommercialCref = checklist.Visit.CommercialCref ?? commercialCref,
                            ExpectedRecoveryAmount = checklist.ExpectedRecoveryAmount.Value,
                            RemainingRecoveryAmount = remaining < 0 ? 0 : remaining,
                            CollectedAmount = totalCollected,
                            VisitStatus = checklist.Visit.Status,
                            LastCollectionDate = recoveryGroup.Any() ? recoveryGroup.Max(r => (DateTime?)r.CollectionDate) : null
                        };
                    })
                .ToList();

            return Ok(report);
        }

        [HttpPut("{id}/Cancel")]
        public async Task<IActionResult> CancelVisit(int id)
        {
            if (!await _visitRepository.VisitExistsAsync(id))
            {
                return NotFound("Visit not found.");
            }

            await _visitRepository.UpdateVisitStatusAsync(id, VisitStatus.Annuler);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVisit(int id)
        {
            if (!await _visitRepository.VisitExistsAsync(id))
            {
                return NotFound("Visit not found.");
            }

            await _visitRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/Order")]
        public async Task<ActionResult<OrderDto>> CreateOrder(int id, [FromBody] OrderCreateDto orderCreateDto)
        {
            if (!await _visitRepository.VisitExistsAsync(id))
            {
                return NotFound("Visit not found.");
            }

            var checklists = await _checklistRapportRepository.GetDtosByVisitAsync(id);
            if (!checklists.Any(c => c.Libelle == ChecklistLibelle.PasserCommande))
            {
                return BadRequest("This visit does not have a PasserCommande checklist.");
            }

            var order = new Order
            {
                VisitId = id,
                OrderRef = orderCreateDto.OrderRef,
                TotalAmount = orderCreateDto.OrderLines.Sum(ol => ol.Quantity * ol.UnitPrice),
                OrderDate = DateTime.UtcNow,
                OrderLines = orderCreateDto.OrderLines.Select(ol => new OrderLine
                {
                    ArticleId = ol.ArticleId,
                    Quantity = ol.Quantity,
                    UnitPrice = ol.UnitPrice
                }).ToList()
            };

            var createdOrder = await _orderRepository.CreateAsync(order);
            var orderDto = new OrderDto
            {
                Id = createdOrder.Id,
                VisitId = createdOrder.VisitId,
                OrderRef = createdOrder.OrderRef,
                TotalAmount = createdOrder.TotalAmount,
                OrderDate = createdOrder.OrderDate,
                OrderLines = createdOrder.OrderLines.Select(ol => new OrderLineDto
                {
                    Id = ol.Id,
                    ArticleId = ol.ArticleId,
                    Quantity = ol.Quantity,
                    UnitPrice = ol.UnitPrice
                }).ToList()
            };

            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, orderDto);
        }

        [HttpGet("Order/{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var orderDto = new OrderDto
            {
                Id = order.Id,
                VisitId = order.VisitId,
                OrderRef = order.OrderRef,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                OrderLines = order.OrderLines.Select(ol => new OrderLineDto
                {
                    Id = ol.Id,
                    ArticleId = ol.ArticleId,
                    Quantity = ol.Quantity,
                    UnitPrice = ol.UnitPrice
                }).ToList()
            };

            return Ok(orderDto);
        }

        [HttpPost("{id}/Recovery")]
        public async Task<ActionResult<RecoveryDto>> CreateRecovery(int id, [FromBody] RecoveryDto recoveryCreateDto)
        {
            if (!await _visitRepository.VisitExistsAsync(id))
            {
                return NotFound("Visit not found.");
            }

            var checklists = await _checklistRapportRepository.GetDtosByVisitAsync(id);
            var recouvrementChecklist = checklists.FirstOrDefault(c => c.Libelle == ChecklistLibelle.Recouvrement);
            if (recouvrementChecklist == null)
            {
                return BadRequest("This visit does not have a Recouvrement checklist.");
            }

            if (recoveryCreateDto.AmountCollected < 0)
            {
                return BadRequest("Collected amount cannot be negative.");
            }

            var checklist = await _checklistRapportRepository.GetByIdAsync(recouvrementChecklist.Id);
            if (!checklist.ExpectedRecoveryAmount.HasValue)
            {
                return BadRequest("Expected recovery amount is not set.");
            }

            checklist.RemainingRecoveryAmount ??= checklist.ExpectedRecoveryAmount.Value;
            checklist.RemainingRecoveryAmount -= recoveryCreateDto.AmountCollected;

            if (checklist.RemainingRecoveryAmount < 0)
            {
                return BadRequest("Collected amount exceeds expected recovery amount.");
            }

            checklist.IsCompleted = checklist.RemainingRecoveryAmount <= 0;
            await _checklistRapportRepository.UpdateAsync(checklist);

            var existingRecovery = await _recoveryRepository.GetByVisitIdAsync(id);
            Recovery recovery;
            if (existingRecovery != null)
            {
                existingRecovery.AmountCollected += recoveryCreateDto.AmountCollected;
                existingRecovery.CollectionDate = DateTime.UtcNow;
                existingRecovery.Notes = recoveryCreateDto.Notes ??
                    $"Updated collection of {recoveryCreateDto.AmountCollected} on {DateTime.UtcNow}. Total collected: {existingRecovery.AmountCollected}. Remaining: {checklist.RemainingRecoveryAmount}";
                recovery = existingRecovery;
                await _recoveryRepository.UpdateAsync(recovery);
            }
            else
            {
                recovery = new Recovery
                {
                    VisitId = id,
                    AmountCollected = recoveryCreateDto.AmountCollected,
                    CollectionDate = DateTime.UtcNow,
                    Notes = recoveryCreateDto.Notes ??
                        $"Collected {recoveryCreateDto.AmountCollected} on {DateTime.UtcNow}. Total collected: {recoveryCreateDto.AmountCollected}. Remaining: {checklist.RemainingRecoveryAmount}"
                };
                recovery = await _recoveryRepository.CreateAsync(recovery);
            }

            if (await _visitRepository.AreAllChecklistItemsCompletedAsync(id))
            {
                await _visitRepository.UpdateVisitStatusAsync(id, VisitStatus.Completed);
            }
            else
            {
                await _visitRepository.UpdateVisitStatusAsync(id, VisitStatus.Incompleted);
            }

            var recoveryDto = new RecoveryDto
            {
                Id = recovery.Id,
                VisitId = recovery.VisitId,
                AmountCollected = recovery.AmountCollected,
                CollectionDate = recovery.CollectionDate,
                Notes = recovery.Notes
            };

            return CreatedAtAction(nameof(GetRecovery), new { id = recovery.Id }, recoveryDto);
        }

        [HttpGet("Recovery/{id}")]
        public async Task<ActionResult<RecoveryDto>> GetRecovery(int id)
        {
            var recovery = await _recoveryRepository.GetByIdAsync(id);
            if (recovery == null)
            {
                return NotFound();
            }

            var recoveryDto = new RecoveryDto
            {
                Id = recovery.Id,
                VisitId = recovery.VisitId,
                AmountCollected = recovery.AmountCollected,
                CollectionDate = recovery.CollectionDate,
                Notes = recovery.Notes
            };

            return Ok(recoveryDto);
        }

        [HttpGet("{visitId}/RecoveryByVisit")]
        public async Task<ActionResult<RecoveryDto>> GetRecoveryByVisit(int visitId)
        {
            var recovery = await _recoveryRepository.GetByVisitIdAsync(visitId);
            if (recovery == null)
            {
                return Ok(null);
            }
            var recoveryDto = new RecoveryDto
            {
                Id = recovery.Id,
                VisitId = recovery.VisitId,
                AmountCollected = recovery.AmountCollected,
                CollectionDate = recovery.CollectionDate,
                Notes = recovery.Notes
            };
            return Ok(recoveryDto);
        }

        [HttpPost("{id}/CompetitorProduct")]
        public async Task<ActionResult<CompetitorProductDto>> CreateCompetitorProduct(int id, [FromBody] CompetitorProductDto competitorProductCreateDto)
        {
            if (!await _visitRepository.VisitExistsAsync(id))
            {
                return NotFound("Visit not found.");
            }

            var checklists = await _checklistRapportRepository.GetDtosByVisitAsync(id);
            if (!checklists.Any(c => c.Libelle == ChecklistLibelle.ProduitConcurant))
            {
                return BadRequest("This visit does not have a ProduitConcurant checklist.");
            }

            var competitorProduct = new CompetitorProduct
            {
                VisitId = id,
                ProductName = competitorProductCreateDto.ProductName,
                Price = competitorProductCreateDto.Price,
                ImageUrl = competitorProductCreateDto.ImageUrl,
                Notes = competitorProductCreateDto.Notes
            };

            var createdCompetitorProduct = await _competitorProductRepository.CreateAsync(competitorProduct);
            var competitorProductDto = new CompetitorProductDto
            {
                Id = createdCompetitorProduct.Id,
                VisitId = createdCompetitorProduct.VisitId,
                ProductName = createdCompetitorProduct.ProductName,
                Price = createdCompetitorProduct.Price,
                ImageUrl = createdCompetitorProduct.ImageUrl,
                Notes = createdCompetitorProduct.Notes
            };

            return CreatedAtAction(nameof(GetCompetitorProduct), new { id = createdCompetitorProduct.Id }, competitorProductDto);
        }

        [HttpGet("{visitId}/CompetitorProducts")]
        public async Task<ActionResult<IEnumerable<CompetitorProductDto>>> GetCompetitorProductsByVisit(int visitId)
        {
            if (!await _visitRepository.VisitExistsAsync(visitId))
            {
                return NotFound("Visit not found.");
            }

            var products = await _competitorProductRepository.GetByVisitIdAsync(visitId);
            var productDtos = products.Select(p => new CompetitorProductDto
            {
                Id = p.Id,
                VisitId = p.VisitId,
                ProductName = p.ProductName,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Notes = p.Notes
            }).ToList();

            return Ok(productDtos);
        }

        [HttpGet("CompetitorProduct/{id}")]
        public async Task<ActionResult<CompetitorProductDto>> GetCompetitorProduct(int id)
        {
            var competitorProduct = await _competitorProductRepository.GetByIdAsync(id);
            if (competitorProduct == null)
            {
                return NotFound();
            }

            var competitorProductDto = new CompetitorProductDto
            {
                Id = competitorProduct.Id,
                VisitId = competitorProduct.VisitId,
                ProductName = competitorProduct.ProductName,
                Price = competitorProduct.Price,
                ImageUrl = competitorProduct.ImageUrl,
                Notes = competitorProduct.Notes
            };

            return Ok(competitorProductDto);
        }

        [HttpGet("Completed")]
        public async Task<ActionResult<IEnumerable<VisitDto>>> GetCompletedVisits([FromQuery] string commercialCref)
        {
            if (string.IsNullOrEmpty(commercialCref))
            {
                return BadRequest("CommercialCref is required.");
            }

            if (!await _visitRepository.CommercialExistsAsync(commercialCref))
            {
                return BadRequest("Invalid Commercial Cref.");
            }

            var completedVisits = await _visitRepository.GetAllDtosByCommercialCrefAsync(commercialCref);
            var filteredVisits = completedVisits
                .Where(v => v.Status == VisitStatus.Completed)
                .Select(v => new VisitDto
                {
                    Id = v.Id,
                    Date = v.Date,
                    TiersId = v.TiersId,
                    ClientNom = v.ClientNom,
                    Note = v.Note,
                    CommercialCref = v.CommercialCref,
                    CommercialNom = v.CommercialNom,
                    Status = v.Status,
                    Title = v.Title,
                    Start = v.Start,
                    Checklists = v.Checklists,
                    Recoveries = v.Recoveries,
                    CompetingProducts = v.CompetingProducts ?? new List<CompetitorProductDto>(),
                    Orders = v.Orders ?? new List<OrderDto>()
                })
                .ToList();

            return Ok(filteredVisits);
        }
    }
}