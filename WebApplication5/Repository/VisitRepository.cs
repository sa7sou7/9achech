using Microsoft.EntityFrameworkCore;
using WebApplication5.Data;
using WebApplication5.Models;

namespace WebApplication5.Repository
{
    public class VisitRepository : IVisitRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VisitRepository> _logger;

        public VisitRepository(AppDbContext context, ILogger<VisitRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Visit> CreateVisitAsync(Visit visit)
        {
            _logger.LogInformation($"Creating visit for commercial {visit.CommercialId}");
            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();
            return visit;
        }

        public async Task<Visit?> GetVisitByIdAsync(int id)
        {
            return await _context.Visits
                .Include(v => v.Commercial)
                .Include(v => v.Client)
                .Include(v => v.Objectives)
                    .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Article)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Visit>> GetVisitsByCommercialAsync(string commercialId)
        {
            return await _context.Visits
                .Include(v => v.Commercial)
                .Include(v => v.Client)
                .Include(v => v.Objectives)
                    .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Article)
                .Where(v => v.CommercialId == commercialId)
                .ToListAsync();
        }

        public async Task<bool> UpdateVisitAsync(Visit visit)
        {
            var existing = await _context.Visits
                .Include(v => v.Objectives)
                    .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(v => v.Id == visit.Id);
            if (existing == null) return false;

            existing.ScheduledDate = visit.ScheduledDate;
            existing.CompletedDate = visit.CompletedDate;
            existing.Report = visit.Report;
            existing.IsValidated = visit.IsValidated;

            existing.Objectives.Clear();
            existing.Objectives.AddRange(visit.Objectives);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVisitAsync(int id)
        {
            var visit = await _context.Visits.FindAsync(id);
            if (visit == null) return false;

            _context.Visits.Remove(visit);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}