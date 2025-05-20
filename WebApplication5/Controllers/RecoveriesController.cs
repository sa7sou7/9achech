using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication5.Dtos;
using WebApplication5.Models;
using WebApplication5.Repositories;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecoveriesController : ControllerBase
    {
        private readonly IRecoveryRepository _recoveryRepository;
        private readonly IVisitRepository _visitRepository;
        private readonly IChecklistRapportRepository _checklistRapportRepository;

        public RecoveriesController(
            IRecoveryRepository recoveryRepository,
            IVisitRepository visitRepository,
            IChecklistRapportRepository checklistRapportRepository)
        {
            _recoveryRepository = recoveryRepository;
            _visitRepository = visitRepository;
            _checklistRapportRepository = checklistRapportRepository;
        }

        [HttpPost]
        public async Task<ActionResult<RecoveryDto>> CreateRecovery([FromBody] RecoveryDto recoveryCreateDto)
        {
            if (!await _visitRepository.VisitExistsAsync(recoveryCreateDto.VisitId))
            {
                return NotFound("Visit not found.");
            }

            var checklists = await _checklistRapportRepository.GetDtosByVisitAsync(recoveryCreateDto.VisitId);
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

            var recovery = new Recovery
            {
                VisitId = recoveryCreateDto.VisitId,
                AmountCollected = recoveryCreateDto.AmountCollected,
                CollectionDate = DateTime.UtcNow,
                Notes = recoveryCreateDto.Notes ?? $"Collected {recoveryCreateDto.AmountCollected} on {DateTime.UtcNow}. Total collected: {recoveryCreateDto.AmountCollected}. Remaining: {checklist.RemainingRecoveryAmount}"
            };

            var createdRecovery = await _recoveryRepository.CreateAsync(recovery);

            if (await _visitRepository.AreAllChecklistItemsCompletedAsync(recoveryCreateDto.VisitId))
            {
                await _visitRepository.UpdateVisitStatusAsync(recoveryCreateDto.VisitId, VisitStatus.Completed);
            }
            else
            {
                await _visitRepository.UpdateVisitStatusAsync(recoveryCreateDto.VisitId, VisitStatus.Incompleted);
            }

            var recoveryDto = new RecoveryDto
            {
                Id = createdRecovery.Id,
                VisitId = createdRecovery.VisitId,
                AmountCollected = createdRecovery.AmountCollected,
                CollectionDate = createdRecovery.CollectionDate,
                Notes = createdRecovery.Notes
            };

            return CreatedAtAction(nameof(CreateRecovery), new { id = createdRecovery.Id }, recoveryDto);
        }

        [HttpGet("{id}")]
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
    }
}