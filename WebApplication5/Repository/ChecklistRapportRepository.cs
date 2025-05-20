using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Data;
using WebApplication5.Dtos;
using WebApplication5.Models;

namespace WebApplication5.Repositories
{
    public class ChecklistRapportRepository : IChecklistRapportRepository
    {
        private readonly AppDbContext _context;

        public ChecklistRapportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ChecklistRapport checklistRapport)
        {
            await _context.ChecklistRapports.AddAsync(checklistRapport);
            await _context.SaveChangesAsync();
        }

        public async Task<ChecklistRapportDto> GetDtoByIdAsync(int id)
        {
            return await _context.ChecklistRapports
                .Where(c => c.Id == id)
                .Select(c => new ChecklistRapportDto
                {
                    Id = c.Id,
                    VisitId = c.VisitId,
                    Libelle = c.Libelle,
                    Commentaire = c.Commentaire,
                    IsCompleted = c.IsCompleted,
                    ExpectedRecoveryAmount = c.ExpectedRecoveryAmount,
                    RemainingRecoveryAmount = c.RemainingRecoveryAmount
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ChecklistRapportDto>> GetDtosByVisitAsync(int visitId)
        {
            return await _context.ChecklistRapports
                .Where(c => c.VisitId == visitId)
                .Select(c => new ChecklistRapportDto
                {
                    Id = c.Id,
                    VisitId = c.VisitId,
                    Libelle = c.Libelle,
                    Commentaire = c.Commentaire,
                    IsCompleted = c.IsCompleted,
                    ExpectedRecoveryAmount = c.ExpectedRecoveryAmount,
                    RemainingRecoveryAmount = c.RemainingRecoveryAmount
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ChecklistRapportDto>> GetDtosByCommercialAsync(string commercialCref)
        {
            return await _context.ChecklistRapports
                .Include(c => c.Visit)
                .Where(c => c.Visit.CommercialCref == commercialCref)
                .Select(c => new ChecklistRapportDto
                {
                    Id = c.Id,
                    VisitId = c.VisitId,
                    Libelle = c.Libelle,
                    Commentaire = c.Commentaire,
                    IsCompleted = c.IsCompleted,
                    ExpectedRecoveryAmount = c.ExpectedRecoveryAmount,
                    RemainingRecoveryAmount = c.RemainingRecoveryAmount
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ChecklistRapport>> GetByCommercialAsync(string commercialCref)
        {
            return await _context.ChecklistRapports
                .Include(c => c.Visit)
                    .ThenInclude(v => v.Client)
                .Include(c => c.Visit)
                    .ThenInclude(v => v.Commercial)
                .Where(c => c.Visit.CommercialCref == commercialCref)
                .ToListAsync();
        }

        public async Task<bool> ChecklistRapportExistsAsync(int id)
        {
            return await _context.ChecklistRapports.AnyAsync(c => c.Id == id);
        }

        public async Task<ChecklistRapport> GetByIdAsync(int id)
        {
            return await _context.ChecklistRapports.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(ChecklistRapport checklistRapport)
        {
            _context.Entry(checklistRapport).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateChecklistCompletionAsync(int checklistId, bool isCompleted)
        {
            var checklist = await _context.ChecklistRapports.FindAsync(checklistId);
            if (checklist != null)
            {
                checklist.IsCompleted = isCompleted;
                await _context.SaveChangesAsync();
            }
        }
    }
}